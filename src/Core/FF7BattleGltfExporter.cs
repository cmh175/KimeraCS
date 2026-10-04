using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace KimeraCS
{

    using static FF7BattleSkeleton;
    using static FF7BattleAnimation;
    using static FF7BattleAnimationsPack;
    using static FF7PModel;
    using static FF7TEXTexture;

    using static Utils;

    //
    // Battle model (??AA skeleton + ??AM.. parts + ??CK.. weapons + ??AC.. textures) with its
    // animation pack (??DA) and limit break packs (*.A00), or a magic/summon model (.D + .A00),
    // -> glTF for FFNx.
    //
    // Conventions (FFNx reads battle models this way since 2026-10-03; files go in mesh\battle):
    //   - joints "bone_00".."bone_NN" (battle bones have no names), parents like Kimera's drawing code
    //   - joint rest translation = (0, 0, +length of parent bone); rotations use Kimera's quaternion math
    //   - root node = Kimera's root placement turned 180 degrees about Z like the rest of the model:
    //     translation (-startX, -startY, startZ), rotation flipZ * q(root). Battle frames are Y-down (e.g.
    //     Cloud's root at Y -466 bobbing to -450); used as-is the bob came out upside down (hopping).
    //   - all weapon models are bound to one extra joint "weapon", animated by the weapon animation
    //     track (weapon animation i belongs to body animation i), relative to the body root
    //   - animations "ANIM_00".. in pack order; limit breaks "<PACK>_00".. (e.g. LIMCL2_00)
    //   - one key per stored frame; timestamps = frame / fps
    //
    public static class FF7BattleGltfExporter
    {
        public enum RestPoseMode { CurrentFrame, AllZero }
        public enum WeaponsMode { All, Current, None }

        public class Options
        {
            public string OutputFolder = "";
            public string FileName = "";
            public float Fps = 30;
            public string TexturePrefix = "";
            public bool WriteDDS = true;
            public bool BakeVertexColors = true;
            public RestPoseMode RestPose = RestPoseMode.CurrentFrame;
            public BattleFrame? RestFrame = null;          // body frame for the rest pose
            public BattleFrame? RestWeaponFrame = null;    // weapon frame for the rest pose
            public WeaponsMode Weapons = WeaponsMode.All;
            public int CurrentWeapon = 0;                  // for WeaponsMode.Current
            public string AnimationPackFile = "";          // ??DA or .A00; "" = none
            public List<int> AnimationIndexes = null;      // null = all animations with frames
            public List<string> LimitPackFiles = new List<string>();
            public string LimitsSearchFolder = null;       // set when limit packs were looked up there and none
                                                           // were found: the report warns if the model has some
            public bool To60Fps = false;                   // 15 -> 60 fps conversion (battle animations are 15 fps)
            public GltfRigExporter.LoopMode Loops = GltfRigExporter.LoopMode.Auto;
        }

        public class Result : GltfRigExporter.Result { }

        // Part file names, same suffix walk as the BattleSkeleton constructor (??AM, ??AN, .. ??AZ, ??BA ..).
        // The glTF name FFNx looks for: FFNx loads mesh\field\<piece>.gltf for each piece (.p file) it loads,
        // so the export is named after the model's first piece (Cloud: RTAM), not the skeleton (RTAA). Magic
        // pieces are <name>.P00 ...; FFNx drops the extension, which leaves the magic model's own name.
        public static string FirstPieceName(BattleSkeleton skel, bool isMagic)
        {
            if (isMagic) return Path.GetFileNameWithoutExtension(skel.fileName).ToUpperInvariant();
            string baseName = skel.fileName.Substring(0, 2).ToUpperInvariant();
            for (int bi = 0; bi < (skel.bones?.Count ?? 0); bi++)
                if (skel.bones[bi].hasModel != 0) return BattlePartName(baseName, bi);
            return skel.fileName.ToUpperInvariant();
        }

        public static string BattlePartName(string baseName, int boneIndex)
        {
            int s1 = 'A', s2 = 'M';
            for (int i = 0; i < boneIndex; i++)
            {
                s2++;
                if (s2 > 'Z') { s1++; s2 = 'A'; }
            }
            return baseName + (char)s1 + (char)s2;
        }

        private static double[] PartXfXYZ(PModel m)
        {
            // glTranslatef(reposition); glRotated(alpha, X); glRotated(beta, Y); glRotated(gamma, Z); glScalef(resize)
            double[] rot = new double[16];
            BuildRotationMatrixWithQuaternionsXYZ(m.rotateAlpha, m.rotateBeta, m.rotateGamma, ref rot);
            return GltfMath.Mul(GltfMath.Mul(GltfMath.Translation(m.repositionX, m.repositionY, m.repositionZ), rot),
                                GltfMath.Scaling(m.resizeX, m.resizeY, m.resizeZ));
        }

        private static double[] RootMatrix(BattleFrame fr)
        {
            BattleFrameBone b = fr.bones[0];
            return GltfMath.TR(fr.startX, fr.startY, fr.startZ, GltfMath.KimeraQuat(b.alpha, b.beta, b.gamma));
        }

        private static bool FrameUsable(BattleFrame fr, int needBones)
        {
            if (fr.bones == null || fr.bones.Count < needBones) return false;
            for (int i = 0; i < needBones; i++)
                if (float.IsNaN(fr.bones[i].alpha) || float.IsNaN(fr.bones[i].beta) || float.IsNaN(fr.bones[i].gamma)) return false;
            return true;
        }

        // Limit break packs of a battle model (Kimera's limit table in FileTools), found in folder.
        // The limit break packs Kimera's limit table lists for a battle model (none for enemies).
        public static List<string> ExpectedLimitPacks(string skeletonFileName)
        {
            List<string> expected = new List<string>();
            if (FileTools.lstBattleLimitsAnimations == null) FileTools.PrepareLimitsFilterFile();
            string model = Path.GetFileName(skeletonFileName).ToUpperInvariant();
            foreach (FileTools.STLimitsRegister reg in FileTools.lstBattleLimitsAnimations)
                if (reg.lstModelNames != null && reg.lstModelNames.Contains(model))
                    expected.AddRange(reg.lstLimitsAnimations);
            return expected;
        }

        public static List<string> FindLimitPacks(string skeletonFileName, string folder)
        {
            List<string> found = new List<string>();
            if (FileTools.lstBattleLimitsAnimations == null) FileTools.PrepareLimitsFilterFile();

            string model = Path.GetFileName(skeletonFileName).ToUpperInvariant();
            foreach (FileTools.STLimitsRegister reg in FileTools.lstBattleLimitsAnimations)
            {
                if (reg.lstModelNames == null || !reg.lstModelNames.Contains(model)) continue;
                foreach (string lim in reg.lstLimitsAnimations)
                {
                    string p = Path.Combine(folder ?? "", lim);
                    if (File.Exists(p)) found.Add(p);
                }
            }
            return found;
        }

        // Where limit break packs usually are: the magic folder next to the battle folder.
        public static string DefaultLimitsFolder(string modelFolder)
        {
            if (string.IsNullOrEmpty(modelFolder)) return "";
            string parent = Path.GetDirectoryName(modelFolder.TrimEnd('\\', '/'));
            if (parent != null)
            {
                string magic = Path.Combine(parent, "magic");
                if (Directory.Exists(magic)) return magic;
            }
            return modelFolder;
        }

        public static BattleAnimationsPack ReadPack(string file, int nSkeletonBones, int nBodyAnims, int nWeaponAnims, bool isLimit)
        {
            BattleAnimationsPack pack = new BattleAnimationsPack
            {
                SkeletonAnimations = new List<BattleAnimation>(),
                WeaponAnimations = new List<BattleAnimation>(),
                IsLimit = isLimit,
            };
            LoadBattleAnimationsPack(file, nSkeletonBones, nBodyAnims, nWeaponAnims, ref pack);
            return pack;
        }

        // ------------------------------------------------------------------------------------------
        // Export
        // ------------------------------------------------------------------------------------------
        public static Result Export(BattleSkeleton skel, bool isMagic, Options opt)
        {
            Result res = new Result();

            try
            {
                if (skel.IsBattleLocation)
                {
                    res.Errors.Add("Battle scenes (locations) have no skeleton; exporting them isn't supported yet.");
                    return res;
                }
                if (skel.bones == null || skel.bones.Count == 0)
                {
                    res.Errors.Add("The model has no bones.");
                    return res;
                }

                string baseName = isMagic ? Path.GetFileNameWithoutExtension(skel.fileName).ToUpperInvariant()
                                          : skel.fileName.Substring(0, 2).ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(opt.FileName)) opt.FileName = FirstPieceName(skel, isMagic);
                if (opt.Fps <= 0) opt.Fps = 30;
                if (opt.To60Fps) opt.Fps = 15;            // battle animations are 15 fps: four keys per frame

                int nb = skel.bones.Count;
                int boneOffset = nb > 1 ? 1 : 0;           // frame.bones[0] is the root; bone i uses [i + 1]
                int needBones = nb + boneOffset;

                res.Report.Add("Model: " + skel.fileName + (isMagic ? " (magic/summon)" : " (battle)") + ", " + nb + " bones, " +
                               skel.textures.Count + " textures" + (isMagic ? "" : ", " + skel.wpModels.Count(w => w.Polys != null) + " weapons"));

                GltfRigExporter.Rig rig = new GltfRigExporter.Rig();

                // ---------------------------------------------------------------- rest pose
                bool restFromFrame = opt.RestPose == RestPoseMode.CurrentFrame;
                BattleFrame restFrame = default;
                bool haveRestFrame = opt.RestFrame.HasValue && FrameUsable(opt.RestFrame.Value, needBones);
                if (opt.RestFrame.HasValue && !haveRestFrame)
                    res.Warnings.Add("The current frame doesn't fit the skeleton; using the all-zero rest pose.");
                if (haveRestFrame) restFrame = opt.RestFrame.Value;
                if (restFromFrame && !haveRestFrame) restFromFrame = false;

                // ---------------------------------------------------------------- joints
                // parent lookup exactly like ModelDrawing.DrawBattleSkeleton (index stack walk)
                int[] stack = new int[nb + 1];
                int jsp = 0;
                stack[0] = -1;
                for (int bi = 0; bi < nb; bi++)
                {
                    while (skel.bones[bi].parentBone != stack[jsp] && jsp > 0) jsp--;
                    int parent = stack[jsp];

                    BattleFrameBone rb = restFromFrame ? restFrame.bones[bi + boneOffset] : default;
                    rig.Joints.Add(new GltfRigExporter.Joint
                    {
                        Name = "bone_" + bi.ToString("00", CultureInfo.InvariantCulture),
                        Parent = parent,
                        RestT = new double[] { 0, 0, parent >= 0 ? skel.bones[parent].len : 0 },
                        RestR = restFromFrame ? GltfMath.KimeraQuat(rb.alpha, rb.beta, rb.gamma) : new Quaternion { w = 1 },
                    });

                    jsp++;
                    stack[jsp] = bi;
                }

                rig.RootRestR = restFromFrame
                    ? GltfMath.Normalized(GltfMath.QMul(GltfMath.FLIP_Z, GltfMath.KimeraQuat(restFrame.bones[0].alpha, restFrame.bones[0].beta,
                                                                                             restFrame.bones[0].gamma)))
                    : GltfMath.FLIP_Z;
                if (haveRestFrame) rig.RootRestT = new double[] { -restFrame.startX, -restFrame.startY, restFrame.startZ };

                // weapons
                List<int> weaponList = new List<int>();
                if (!isMagic && opt.Weapons != WeaponsMode.None)
                    for (int w = 0; w < skel.wpModels.Count; w++)
                        if (skel.wpModels[w].Polys != null && (opt.Weapons == WeaponsMode.All || w == opt.CurrentWeapon))
                            weaponList.Add(w);

                int weaponJoint = -1;
                if (weaponList.Count > 0)
                {
                    weaponJoint = rig.Joints.Count;
                    double[] restLocal = GltfMath.Identity();
                    if (restFromFrame && opt.RestWeaponFrame.HasValue && FrameUsable(opt.RestWeaponFrame.Value, 1))
                        restLocal = GltfMath.Mul(GltfMath.InvertAffine(RootMatrix(restFrame)), RootMatrix(opt.RestWeaponFrame.Value));
                    rig.Joints.Add(new GltfRigExporter.Joint
                    {
                        Name = "weapon",
                        Parent = -1,
                        RestT = new double[] { restLocal[12], restLocal[13], restLocal[14] },
                        RestR = GltfMath.QuatFromMatrix(restLocal),
                    });
                }

                res.Report.Add("Rest pose: " + (restFromFrame ? "current frame" : "all zero"));

                // ---------------------------------------------------------------- parts
                List<TEX> textures = skel.textures;
                Func<int, TEX?> texOf = texID => textures != null && texID >= 0 && texID < textures.Count ? textures[texID] : (TEX?)null;

                for (int bi = 0; bi < nb; bi++)
                {
                    BattleBone bone = skel.bones[bi];
                    if (bone.Models == null) continue;
                    string partName = isMagic ? baseName + ".P" + bi.ToString("00", CultureInfo.InvariantCulture)
                                              : BattlePartName(baseName, bi);
                    for (int mi = 0; mi < bone.Models.Count; mi++)
                    {
                        PModel m = bone.Models[mi];
                        if (m.Polys == null || m.Groups == null) continue;
                        rig.Parts.Add(new GltfRigExporter.Part
                        {
                            MeshName = partName + (mi > 0 ? "_" + mi : ""),
                            Joint = bi,
                            Model = m,
                            // Same transform chain as ModelDrawing.DrawBattleSkeletonBone.
                            BoneXf = GltfMath.Scaling(bone.resizeX, bone.resizeY, bone.resizeZ),
                            PartXf = PartXfXYZ(m),
                            Texture = texOf,
                        });
                    }
                }

                foreach (int w in weaponList)
                {
                    PModel m = skel.wpModels[w];
                    rig.Parts.Add(new GltfRigExporter.Part
                    {
                        MeshName = baseName + "C" + (char)('K' + w),
                        Joint = weaponJoint,
                        Model = m,
                        BoneXf = GltfMath.Identity(),
                        PartXf = PartXfXYZ(m),      // same as the weapon block of ModelDrawing.DrawBattleSkeleton
                        Texture = texOf,
                    });
                }
                if (weaponList.Count > 0)
                    res.Report.Add("Weapons: " + string.Join(", ", weaponList.Select(w => baseName + "C" + (char)('K' + w))) +
                                   " (all on joint \"weapon\")");

                // ---------------------------------------------------------------- animations
                res.Report.Add(opt.To60Fps
                    ? "Animations (converted 15 -> 60 fps: every stored frame kept, three in-between keys added after each):"
                    : "Animations (" + opt.Fps.ToString(CultureInfo.InvariantCulture) + " fps timestamps, one key per stored frame):");

                void AddPack(BattleAnimationsPack pack, string prefix, List<int> indexes)
                {
                    for (int ai = 0; ai < pack.SkeletonAnimations.Count; ai++)
                    {
                        if (indexes != null && !indexes.Contains(ai)) continue;
                        BattleAnimation ba = pack.SkeletonAnimations[ai];
                        if (ba.frames == null || ba.frames.Count == 0) continue;

                        string name = prefix + "_" + ai.ToString("00", CultureInfo.InvariantCulture);
                        List<BattleFrame> frames = ba.frames;
                        int nf = frames.Count;

                        // frames that don't fit (too few bones / NaN): copy the previous good one
                        List<int> broken = new List<int>();
                        BattleFrame[] fixedFrames = new BattleFrame[nf];
                        int firstGood = -1;
                        for (int f = 0; f < nf; f++) if (FrameUsable(frames[f], needBones)) { firstGood = f; break; }
                        if (firstGood < 0)
                        {
                            res.Errors.Add("Animation " + name + ": no frame fits the skeleton. Skipped.");
                            continue;
                        }
                        int prevGood = firstGood;
                        for (int f = 0; f < nf; f++)
                        {
                            if (FrameUsable(frames[f], needBones)) { fixedFrames[f] = frames[f]; prevGood = f; }
                            else { fixedFrames[f] = frames[prevGood]; broken.Add(f); }
                        }
                        if (broken.Count > 0)
                            res.Warnings.Add("Animation " + name + ": repaired frame(s) " + string.Join(", ", broken) +
                                             " by copying the previous good frame.");

                        List<BattleFrame> wframes = null;
                        if (weaponJoint >= 0 && ai < pack.WeaponAnimations.Count && pack.WeaponAnimations[ai].frames != null &&
                            pack.WeaponAnimations[ai].frames.Count > 0)
                            wframes = pack.WeaponAnimations[ai].frames;

                        // loop or one-shot for the 60 fps conversion (Loops = Auto)
                        string why;
                        bool loop = prefix != "ANIM" ? SixtyFpsLoops.Limit(prefix, ai, out why)
                                  : isMagic ? SixtyFpsLoops.Magic(baseName, ai, out why)
                                  : SixtyFpsLoops.Battle(baseName, ai, out why);

                        GltfRigExporter.Animation ga = new GltfRigExporter.Animation
                        {
                            Name = name,
                            Frames = nf,
                            T = new float[rig.Joints.Count][],
                            R = new float[rig.Joints.Count][],
                            RootT = new float[nf * 3],
                            RootR = new float[nf * 4],
                            Loop = loop,
                            LoopWhy = why,
                        };

                        for (int bi = 0; bi < nb; bi++)
                        {
                            float[] tr = new float[nf * 3];
                            float[] rot = new float[nf * 4];
                            Quaternion prevQ = new Quaternion { w = 1 };
                            for (int f = 0; f < nf; f++)
                            {
                                tr[f * 3 + 2] = (float)rig.Joints[bi].RestT[2];
                                BattleFrameBone b = fixedFrames[f].bones[bi + boneOffset];
                                Quaternion q = GltfMath.KimeraQuat(b.alpha, b.beta, b.gamma);
                                if (f > 0) q = GltfMath.SameHemisphere(q, prevQ);
                                prevQ = q;
                                GltfMath.PutQuat(rot, f, q);
                            }
                            ga.T[bi] = tr;
                            ga.R[bi] = rot;
                        }

                        if (weaponJoint >= 0)
                        {
                            float[] tr = new float[nf * 3];
                            float[] rot = new float[nf * 4];
                            Quaternion prevQ = rig.Joints[weaponJoint].RestR;
                            for (int f = 0; f < nf; f++)
                            {
                                double[] local;
                                if (wframes != null && FrameUsable(wframes[Math.Min(f, wframes.Count - 1)], 1))
                                    local = GltfMath.Mul(GltfMath.InvertAffine(RootMatrix(fixedFrames[f])),
                                                         RootMatrix(wframes[Math.Min(f, wframes.Count - 1)]));
                                else
                                    local = GltfMath.TR(rig.Joints[weaponJoint].RestT[0], rig.Joints[weaponJoint].RestT[1],
                                                        rig.Joints[weaponJoint].RestT[2], rig.Joints[weaponJoint].RestR);
                                tr[f * 3] = (float)local[12]; tr[f * 3 + 1] = (float)local[13]; tr[f * 3 + 2] = (float)local[14];
                                Quaternion q = GltfMath.QuatFromMatrix(local);
                                if (f > 0) q = GltfMath.SameHemisphere(q, prevQ);
                                prevQ = q;
                                GltfMath.PutQuat(rot, f, q);
                            }
                            ga.T[weaponJoint] = tr;
                            ga.R[weaponJoint] = rot;
                        }

                        {
                            Quaternion prevQ = rig.RootRestR;
                            for (int f = 0; f < nf; f++)
                            {
                                BattleFrame fr = fixedFrames[f];
                                ga.RootT[f * 3] = -fr.startX; ga.RootT[f * 3 + 1] = -fr.startY; ga.RootT[f * 3 + 2] = fr.startZ;
                                Quaternion q = GltfMath.Normalized(GltfMath.QMul(GltfMath.FLIP_Z, GltfMath.KimeraQuat(fr.bones[0].alpha, fr.bones[0].beta,
                                                                                                                      fr.bones[0].gamma)));
                                if (f > 0) q = GltfMath.SameHemisphere(q, prevQ);
                                prevQ = q;
                                GltfMath.PutQuat(ga.RootR, f, q);
                            }
                        }

                        rig.Animations.Add(ga);
                        res.Report.Add("  " + name + ": " + nf + " frame(s)" + (wframes != null ? ", weapon track " + wframes.Count : "") +
                                       (broken.Count > 0 ? ", " + broken.Count + " repaired" : ""));
                    }
                }

                if (!string.IsNullOrEmpty(opt.AnimationPackFile))
                {
                    if (!File.Exists(opt.AnimationPackFile))
                        res.Errors.Add("Animation pack not found: " + opt.AnimationPackFile);
                    else
                    {
                        BattleAnimationsPack pack = ReadPack(opt.AnimationPackFile, nb, skel.nsSkeletonAnims,
                                                             isMagic ? 0 : skel.nsWeaponsAnims, false);
                        AddPack(pack, "ANIM", opt.AnimationIndexes);
                    }
                }

                if (!isMagic && opt.LimitPackFiles.Count == 0 && opt.LimitsSearchFolder != null)
                {
                    List<string> expected = ExpectedLimitPacks(skel.fileName);
                    if (expected.Count > 0)
                        res.Warnings.Add("No limit breaks exported: " + skel.fileName.ToUpperInvariant() + " has " +
                                         string.Join(", ", expected.Select(Path.GetFileNameWithoutExtension)) +
                                         ", but none were found in " + opt.LimitsSearchFolder +
                                         ". Point the limit breaks folder at an extracted magic.lgp (model-only mods don't include them).");
                }

                foreach (string lp in opt.LimitPackFiles)
                {
                    if (!File.Exists(lp))
                    {
                        res.Errors.Add("Limit break pack not found: " + lp);
                        continue;
                    }
                    BattleAnimationsPack pack = ReadPack(lp, nb, 8, 8, true);
                    AddPack(pack, Path.GetFileNameWithoutExtension(lp).ToUpperInvariant(), null);
                }

                // ---------------------------------------------------------------- write
                GltfRigExporter.Write(rig, new GltfRigExporter.Options
                {
                    OutputFolder = opt.OutputFolder,
                    FileName = opt.FileName,
                    Fps = opt.Fps,
                    TexturePrefix = opt.TexturePrefix,
                    WriteDDS = opt.WriteDDS,
                    BakeVertexColors = opt.BakeVertexColors,
                    Unlit = false,                  // battle models are lit by the game
                    FrameRateFactor = opt.To60Fps ? 4 : 1,
                    Loops = opt.Loops,
                }, res);

                res.Success = res.Success && res.Errors.Count == 0;
            }
            catch (Exception ex)
            {
                res.Errors.Add("Export failed: " + ex.Message);
                res.Success = false;
            }

            return res;
        }
    }
}
