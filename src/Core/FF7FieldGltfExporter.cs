using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace KimeraCS
{

    using static FF7FieldSkeleton;
    using static FF7FieldAnimation;
    using static FF7FieldRSDResource;
    using static FF7PModel;
    using static FF7TEXTexture;

    using static Utils;

    //
    // Field model (HRC skeleton + RSD/P parts + TEX textures) + field animations (.a) -> glTF for
    // FFNx. Also used for world map, chocobo racing and motorbike game models (same format).
    //
    // Field conventions (checked against a gltf that works in FFNx 1.24.0, see PROJECT_NOTES.md):
    //   - one joint per HRC bone, named like the bone
    //   - joint rest translation = (0, 0, -length of parent bone); rotations use Kimera's own
    //     quaternion math: q = qY(beta) * qX(alpha) * qZ(gamma)
    //   - root node = Kimera's root placement turned 180 degrees about Z like the rest of the model.
    //     Kimera draws it as T(x, -y, z) * q(root) in FF7's Y-down space, so the root node gets
    //     translation (-x, y, z) and rotation flipZ * q(root). (CrossSlash writes (x, y, z) and
    //     q(root) * flipZ, which mirrors sideways root motion and root turns; FFNx ignores the root.)
    //   - one animation key per stored .a frame (no frame conversion); timestamps = frame / fps
    //
    public static class FF7FieldGltfExporter
    {
        public enum RestPoseMode
        {
            CurrentFrame,   // joint rotations from Options.RestFrame (e.g. Kimera's current frame)
            AllZero,        // every joint rotation zero (the CrossSlash / reference convention)
        }

        public class Options
        {
            public string OutputFolder = "";
            public string FileName = "";              // without extension, e.g. "AABA"
            public float Fps = 30;                    // only used for timestamps
            public string TexturePrefix = "";         // image names become <prefix>_0, <prefix>_1 ...
            public bool WriteDDS = true;
            public bool BakeVertexColors = true;      // untextured parts get a baked colour texture
            public RestPoseMode RestPose = RestPoseMode.CurrentFrame;
            public FieldFrame? RestFrame = null;      // pose for CurrentFrame; root placement for both
            public string AnimationFolder = "";
            public List<string> AnimationNames = new List<string>();
            public bool To60Fps = false;              // 30 -> 60 fps conversion (see GltfRigExporter.MultiplyFrameRate)
            public GltfRigExporter.LoopMode Loops = GltfRigExporter.LoopMode.Auto;
        }

        public class Result : GltfRigExporter.Result { }

        // ------------------------------------------------------------------------------------------
        // Skeleton
        // ------------------------------------------------------------------------------------------
        private class Bone
        {
            public string Name;
            public int Parent = -1;           // index in fSkeleton.bones, -1 = attached to "root"
            public double ParentLength;       // length of parent bone (0 for root joints)
        }

        // Parent lookup exactly like Kimera's drawing code (joint stack walk over joint_f / joint_i).
        private static List<Bone> BuildBones(FieldSkeleton skel)
        {
            List<Bone> bones = new List<Bone>();
            string[] nameStack = new string[skel.bones.Count + 1];
            int[] boneStack = new int[skel.bones.Count + 1];
            int jsp = 0;

            nameStack[0] = skel.bones[0].joint_f;
            boneStack[0] = -1;

            for (int bi = 0; bi < skel.bones.Count; bi++)
            {
                while (skel.bones[bi].joint_f != nameStack[jsp] && jsp > 0) jsp--;

                Bone b = new Bone { Name = skel.bones[bi].joint_i, Parent = boneStack[jsp] };
                b.ParentLength = b.Parent >= 0 ? skel.bones[b.Parent].len : 0;
                bones.Add(b);

                jsp++;
                nameStack[jsp] = skel.bones[bi].joint_i;
                boneStack[jsp] = bi;
            }

            return bones;
        }

        // ------------------------------------------------------------------------------------------
        // Animation reading / repair
        // ------------------------------------------------------------------------------------------
        // Kimera's own .a reader, without FixFieldAnimation (that one can drop frames).
        public static FieldAnimation ReadAnimation(string fileName)
        {
            FieldAnimation anim = new FieldAnimation { frames = new List<FieldFrame>() };
            anim.ReadFieldAnimation(fileName);
            return anim;
        }

        // Every compatible animation of a field model: the Ifalna database list in its own order (the
        // model's default idle comes first, e.g. ACFE for Cloud) when the model is in it; otherwise every
        // .a file in the folder with the same bone count (world map, chocobo racing, motorbike models).
        // Only animations whose .a file exists and fits the skeleton are returned.
        public static List<string> CompatibleAnimations(string hrcPath, string animFolder, int nBones, out string source)
        {
            bool Fits(string name)
            {
                string p = Path.Combine(animFolder, name + ".A");
                if (!File.Exists(p)) return false;
                try
                {
                    using (BinaryReader br = new BinaryReader(File.OpenRead(p)))
                    {
                        br.BaseStream.Position = 8;
                        int nb = br.ReadInt32();
                        return nb == nBones || (nb == 0 && nBones == 1);
                    }
                }
                catch { return false; }
            }

            List<string> list = new List<string>();
            string model = Path.GetFileNameWithoutExtension(hrcPath).ToUpperInvariant();
            string db = Path.Combine(AppContext.BaseDirectory, FileTools.CHAR_LGP_FILTER_FILE_NAME);
            int listed = 0;
            if (File.Exists(db))
                foreach (string line in File.ReadAllLines(db))
                {
                    if (!line.StartsWith(model + "Anims", StringComparison.OrdinalIgnoreCase)) continue;
                    foreach (string raw in line.Substring(line.IndexOf('=') + 1).Split(','))
                    {
                        string n = NormalizeAnimationName(raw);
                        if (n == "" || list.Contains(n)) continue;
                        listed++;
                        if (Fits(n)) list.Add(n);
                    }
                }

            if (listed > 0)
            {
                source = "Ifalna database: " + list.Count + " of " + listed + " listed animations found in " + animFolder;
                return list;
            }

            if (Directory.Exists(animFolder))
                foreach (string f in Directory.GetFiles(animFolder, "*.a").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                {
                    string n = NormalizeAnimationName(Path.GetFileName(f));
                    if (!list.Contains(n) && Fits(n)) list.Add(n);
                }
            source = "not in the Ifalna database: all " + list.Count + " .a files in " + animFolder + " with " + nBones + " bones";
            return list;
        }

        // The glTF name FFNx looks for: FFNx loads mesh\field\<p name>.gltf for each .p file it loads, so the
        // export is named after the model's first .p file (Cloud: AAAC), not the skeleton (AAAA.HRC).
        public static string FirstPieceName(FieldSkeleton skel)
        {
            foreach (FieldBone bone in skel.bones ?? new List<FieldBone>())
                for (int ri = 0; ri < bone.nResources; ri++)
                {
                    FieldRSDResource res = bone.fRSDResources[ri];
                    string p = Path.GetFileNameWithoutExtension(res.Model.fileName ?? res.res_file ?? "");
                    if (!string.IsNullOrEmpty(p)) return p.ToUpperInvariant();
                }
            return Path.GetFileNameWithoutExtension(skel.fileName ?? "model").ToUpperInvariant();
        }

        private static bool IsBroken(FieldFrame frame, int nBones)
        {
            if (frame.rotations == null || frame.rotations.Count < nBones) return true;
            FieldFrame tmp = frame;
            return IsBrokenFieldFrame(ref tmp, nBones);
        }

        private static string NormalizeAnimationName(string name)
        {
            name = name.Trim();
            if (name.EndsWith(".a", StringComparison.OrdinalIgnoreCase)) name = name.Substring(0, name.Length - 2);
            return name.ToUpperInvariant();
        }

        // ------------------------------------------------------------------------------------------
        // Export
        // ------------------------------------------------------------------------------------------
        public static Result Export(FieldSkeleton skel, Options opt)
        {
            Result res = new Result();

            try
            {
                if (skel.bones == null || skel.bones.Count == 0)
                {
                    res.Errors.Add("The model has no bones.");
                    return res;
                }
                if (string.IsNullOrWhiteSpace(opt.FileName)) opt.FileName = FirstPieceName(skel);
                if (opt.Fps <= 0) opt.Fps = 30;
                if (opt.To60Fps) opt.Fps = 30;            // field animations are 30 fps: two keys per frame

                res.Report.Add("Model: " + skel.fileName + " (" + skel.name + "), " + skel.bones.Count + " bones");

                GltfRigExporter.Rig rig = new GltfRigExporter.Rig();
                List<Bone> bones = BuildBones(skel);
                int nb = skel.bones.Count;

                // ---------------------------------------------------------------- rest pose
                bool restFromFrame = opt.RestPose == RestPoseMode.CurrentFrame;
                FieldFrame restFrame = default;
                bool haveRestFrame = opt.RestFrame.HasValue;
                if (haveRestFrame)
                {
                    restFrame = opt.RestFrame.Value;
                    if (IsBroken(restFrame, nb))
                    {
                        res.Warnings.Add("The current frame is broken or doesn't fit the skeleton; using the all-zero rest pose.");
                        haveRestFrame = false;
                    }
                }
                if (restFromFrame && !haveRestFrame) restFromFrame = false;

                for (int bi = 0; bi < nb; bi++)
                {
                    rig.Joints.Add(new GltfRigExporter.Joint
                    {
                        Name = bones[bi].Name,
                        Parent = bones[bi].Parent,
                        RestT = new double[] { 0, 0, -bones[bi].ParentLength },
                        RestR = restFromFrame
                            ? GltfMath.KimeraQuat(restFrame.rotations[bi].alpha, restFrame.rotations[bi].beta, restFrame.rotations[bi].gamma)
                            : new Quaternion { w = 1 },
                    });
                }

                rig.RootRestR = restFromFrame
                    ? GltfMath.Normalized(GltfMath.QMul(GltfMath.FLIP_Z, GltfMath.KimeraQuat(restFrame.rootRotationAlpha, restFrame.rootRotationBeta,
                                                                                             restFrame.rootRotationGamma)))
                    : GltfMath.FLIP_Z;
                if (haveRestFrame)
                    rig.RootRestT = new double[] { -restFrame.rootTranslationX, restFrame.rootTranslationY, restFrame.rootTranslationZ };

                res.Report.Add("Rest pose: " + (restFromFrame ? "current frame" : "all zero"));

                // ---------------------------------------------------------------- parts
                for (int bi = 0; bi < nb; bi++)
                {
                    FieldBone bone = skel.bones[bi];
                    for (int ri = 0; ri < bone.nResources; ri++)
                    {
                        FieldRSDResource rsd = bone.fRSDResources[ri];
                        if (rsd.Model.Polys == null || rsd.Model.Groups == null)
                        {
                            res.Warnings.Add("Bone " + bones[bi].Name + ": part " + rsd.res_file + " has no geometry (missing .P?); skipped.");
                            continue;
                        }

                        PModel m = rsd.Model;
                        List<TEX> textures = rsd.textures;
                        rig.Parts.Add(new GltfRigExporter.Part
                        {
                            MeshName = Path.GetFileNameWithoutExtension(m.fileName ?? rsd.res_file).ToUpperInvariant(),
                            Joint = bi,
                            Model = m,
                            // Same transform chain as ModelDrawing.DrawFieldBone / DrawRSDResource.
                            BoneXf = GltfMath.Scaling(bone.resizeX, bone.resizeY, bone.resizeZ),
                            PartXf = GltfMath.Mul(GltfMath.Mul(GltfMath.Translation(m.repositionX, m.repositionY, m.repositionZ),
                                                               GltfMath.RotationFromQuat(m.rotationQuaternion)),
                                                  GltfMath.Scaling(m.resizeX, m.resizeY, m.resizeZ)),
                            Texture = texID => textures != null && texID >= 0 && texID < textures.Count ? textures[texID] : (TEX?)null,
                        });
                    }
                }

                // ---------------------------------------------------------------- animations
                res.Report.Add(opt.To60Fps
                    ? "Animations (converted 30 -> 60 fps: every stored frame kept, an in-between key added after each):"
                    : "Animations (" + opt.Fps.ToString(CultureInfo.InvariantCulture) + " fps timestamps, one key per stored frame):");
                HashSet<string> animKeys = new HashSet<string>();

                foreach (string rawName in opt.AnimationNames)
                {
                    string name = NormalizeAnimationName(rawName);
                    if (name.Length == 0) continue;
                    // FFNx matches on the first 4 characters (the whole name when shorter, e.g. world
                    // map "AAE" or chocobo racing "DB")
                    if (!animKeys.Add(name.Substring(0, Math.Min(4, name.Length))))
                    {
                        res.Warnings.Add("Animation '" + name + "' listed twice; exported once.");
                        continue;
                    }

                    string path = Path.Combine(opt.AnimationFolder, name + ".A");
                    if (!File.Exists(path))
                    {
                        res.Errors.Add("Animation " + name + ": file not found (" + path + ").");
                        continue;
                    }

                    FieldAnimation anim;
                    try { anim = ReadAnimation(path); }
                    catch (Exception ex)
                    {
                        res.Errors.Add("Animation " + name + ": can't be read (" + ex.Message + ").");
                        continue;
                    }

                    if (!(anim.nBones == nb || (anim.nBones == 0 && nb == 1)))
                    {
                        res.Errors.Add("Animation " + name + " has " + anim.nBones + " bones; the model has " + nb + ". Skipped.");
                        continue;
                    }
                    if (anim.frames.Count == 0)
                    {
                        res.Errors.Add("Animation " + name + " has no frames. Skipped.");
                        continue;
                    }
                    if (anim.rotationOrder != null && !(anim.rotationOrder[0] == 1 && anim.rotationOrder[1] == 0 && anim.rotationOrder[2] == 2))
                        res.Warnings.Add("Animation " + name + " has an unusual rotation order (" + string.Join(",", anim.rotationOrder) +
                                         "); exported with Kimera's order like the viewer shows it.");

                    // repair broken frames by copying the previous good frame
                    List<FieldFrame> frames = anim.frames;
                    List<int> broken = new List<int>();
                    int lastGood = -1;
                    for (int f = 0; f < frames.Count; f++)
                    {
                        if (IsBroken(frames[f], nb)) broken.Add(f);
                        else lastGood = f;
                    }
                    if (lastGood < 0)
                    {
                        res.Errors.Add("Animation " + name + ": every frame is broken. Skipped.");
                        continue;
                    }
                    if (broken.Count > 0)
                    {
                        int firstGood = Enumerable.Range(0, frames.Count).First(f => !broken.Contains(f));
                        HashSet<int> brokenSet = new HashSet<int>(broken);
                        List<FieldFrame> fixedFrames = new List<FieldFrame>(frames);
                        int prev = firstGood;       // leading broken frames copy the first good one
                        for (int f = 0; f < frames.Count; f++)
                        {
                            if (brokenSet.Contains(f)) fixedFrames[f] = frames[prev];
                            else prev = f;
                        }
                        frames = fixedFrames;
                        res.Warnings.Add("Animation " + name + ": repaired broken frame(s) " + string.Join(", ", broken) +
                                         " by copying the previous good frame.");
                    }

                    int nf = frames.Count;
                    GltfRigExporter.Animation ga = new GltfRigExporter.Animation
                    {
                        Name = name,
                        Frames = nf,
                        T = new float[nb][],
                        R = new float[nb][],
                        RootT = new float[nf * 3],
                        RootR = new float[nf * 4],
                    };
                    ga.Loop = SixtyFpsLoops.Field(out ga.LoopWhy);

                    for (int bi = 0; bi < nb; bi++)
                    {
                        float[] tr = new float[nf * 3];
                        float[] rot = new float[nf * 4];
                        Quaternion prevQ = new Quaternion { w = 1 };
                        for (int f = 0; f < nf; f++)
                        {
                            tr[f * 3 + 2] = (float)-bones[bi].ParentLength;
                            FieldRotation r = frames[f].rotations[bi];
                            Quaternion q = GltfMath.KimeraQuat(r.alpha, r.beta, r.gamma);
                            if (f > 0) q = GltfMath.SameHemisphere(q, prevQ);
                            prevQ = q;
                            GltfMath.PutQuat(rot, f, q);
                        }
                        ga.T[bi] = tr;
                        ga.R[bi] = rot;
                    }

                    // root node: FFNx ignores it, viewers (Blender, Maya) use it
                    {
                        Quaternion prevQ = rig.RootRestR;
                        for (int f = 0; f < nf; f++)
                        {
                            FieldFrame fr = frames[f];
                            ga.RootT[f * 3] = -fr.rootTranslationX; ga.RootT[f * 3 + 1] = fr.rootTranslationY; ga.RootT[f * 3 + 2] = fr.rootTranslationZ;
                            Quaternion q = GltfMath.Normalized(GltfMath.QMul(GltfMath.FLIP_Z, GltfMath.KimeraQuat(fr.rootRotationAlpha, fr.rootRotationBeta,
                                                                                                                  fr.rootRotationGamma)));
                            if (f > 0) q = GltfMath.SameHemisphere(q, prevQ);
                            prevQ = q;
                            GltfMath.PutQuat(ga.RootR, f, q);
                        }
                    }

                    rig.Animations.Add(ga);
                    res.Report.Add("  " + name + ": " + nf + " frame(s)" + (broken.Count > 0 ? ", " + broken.Count + " repaired" : ""));
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
                    FrameRateFactor = opt.To60Fps ? 2 : 1,
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

        public static string FormatReport(GltfRigExporter.Result r) => GltfRigExporter.FormatReport(r);
    }
}
