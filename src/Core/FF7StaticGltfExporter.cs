using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace KimeraCS
{

    using static FF7FieldSkeleton;
    using static FF7FieldRSDResource;
    using static FF7BattleSkeleton;
    using static FF7PModel;
    using static FF7TEXTexture;
    using static FF7TMDModel;

    using static Utils;

    //
    // Static models (no skeleton, no animations) -> glTF for FFNx: single RSD resources (e.g. Fort
    // Condor), single P / 3DS models, TMD models (e.g. snowboard mini-game) and battle scenes.
    //
    //   - one joint "static", every vertex fully weighted to it (FFNx skins every gltf model)
    //   - vertices stored in the game's own space; only the "root" node carries the 180 degree turn
    //     about Z for viewers (FFNx draws a model without animations exactly as stored)
    //   - no animations; materials unlit (static FF7 geometry has its lighting baked in)
    //
    public static class FF7StaticGltfExporter
    {
        public class Options
        {
            public string OutputFolder = "";
            public string FileName = "";
            public string TexturePrefix = "";
            public bool WriteDDS = true;
            public bool BakeVertexColors = true;
        }

        public class Result : GltfRigExporter.Result { }

        private static double[] PartXfXYZ(PModel m)
        {
            // glTranslatef(reposition); rotate X, Y, Z (BuildRotationMatrixWithQuaternionsXYZ); glScalef(resize)
            double[] rot = new double[16];
            BuildRotationMatrixWithQuaternionsXYZ(m.rotateAlpha, m.rotateBeta, m.rotateGamma, ref rot);
            return GltfMath.Mul(GltfMath.Mul(GltfMath.Translation(m.repositionX, m.repositionY, m.repositionZ), rot),
                                GltfMath.Scaling(m.resizeX, m.resizeY, m.resizeZ));
        }

        private static GltfRigExporter.Rig NewRig()
        {
            GltfRigExporter.Rig rig = new GltfRigExporter.Rig { Static = true };
            rig.Joints.Add(new GltfRigExporter.Joint { Name = "static", Parent = -1 });
            return rig;
        }

        private static Result Write(GltfRigExporter.Rig rig, Options opt, Result res, string description)
        {
            try
            {
                res.Report.Insert(0, "Model: " + description + " (static: no skeleton, no animations)");
                if (rig.Parts.Count == 0) res.Errors.Add("The model has no geometry.");
                if (res.Errors.Count > 0) return res;

                GltfRigExporter.Write(rig, new GltfRigExporter.Options
                {
                    OutputFolder = opt.OutputFolder,
                    FileName = opt.FileName,
                    Fps = 30,
                    TexturePrefix = opt.TexturePrefix,
                    WriteDDS = opt.WriteDDS,
                    BakeVertexColors = opt.BakeVertexColors,
                    Unlit = true,
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

        private static string DefaultName(Options opt, string name)
        {
            if (string.IsNullOrWhiteSpace(opt.FileName)) opt.FileName = name.ToUpperInvariant();
            return opt.FileName;
        }

        // ------------------------------------------------------------------------------------------
        // Sources
        // ------------------------------------------------------------------------------------------

        // A field skeleton used as a static model: an RSD resource (Kimera loads it as a one-bone
        // skeleton) - every part is placed like the viewer shows it with no animation (bone rotation 0).
        public static Result ExportRSD(FieldSkeleton skel, Options opt)
        {
            Result res = new Result();
            GltfRigExporter.Rig rig = NewRig();
            DefaultName(opt, FF7FieldGltfExporter.FirstPieceName(skel));     // the RSD's .p name

            foreach (FieldBone bone in skel.bones)
                for (int ri = 0; ri < bone.nResources; ri++)
                {
                    FieldRSDResource rsd = bone.fRSDResources[ri];
                    PModel m = rsd.Model;
                    if (m.Polys == null || m.Groups == null)
                    {
                        res.Warnings.Add("Part " + rsd.res_file + " has no geometry (missing .P?); skipped.");
                        continue;
                    }
                    List<TEX> textures = rsd.textures;
                    rig.Parts.Add(new GltfRigExporter.Part
                    {
                        MeshName = Path.GetFileNameWithoutExtension(m.fileName ?? rsd.res_file).ToUpperInvariant(),
                        Joint = 0,
                        Model = m,
                        BoneXf = GltfMath.Scaling(bone.resizeX, bone.resizeY, bone.resizeZ),
                        PartXf = GltfMath.Mul(GltfMath.Mul(GltfMath.Translation(m.repositionX, m.repositionY, m.repositionZ),
                                                           GltfMath.RotationFromQuat(m.rotationQuaternion)),
                                              GltfMath.Scaling(m.resizeX, m.resizeY, m.resizeZ)),
                        Texture = texID => textures != null && texID >= 0 && texID < textures.Count ? textures[texID] : (TEX?)null,
                    });
                }

            return Write(rig, opt, res, (skel.fileName ?? "") + " (RSD resource)");
        }

        // A single P (or 3DS) model as Kimera shows it. Single P files carry no textures (those come
        // from an RSD), so untextured groups get the vertex colour bake.
        public static Result ExportPModel(PModel m, Options opt)
        {
            Result res = new Result();
            GltfRigExporter.Rig rig = NewRig();
            string name = Path.GetFileNameWithoutExtension(m.fileName ?? "model");
            DefaultName(opt, name);

            if (m.Polys != null && m.Groups != null)
                rig.Parts.Add(new GltfRigExporter.Part
                {
                    MeshName = name.ToUpperInvariant(),
                    Joint = 0,
                    Model = m,
                    BoneXf = GltfMath.Identity(),
                    PartXf = PartXfXYZ(m),
                    Texture = null,
                });

            if (m.Groups != null && m.Groups.Any(g => g.texFlag == 1))
                res.Warnings.Add("A single .P file has no texture list (textures come from its .RSD); textured groups are " +
                                 "exported with vertex colours. Load the .RSD instead to keep the textures.");

            return Write(rig, opt, res, (m.fileName ?? "") + " (P model)");
        }

        // Every object of a TMD model (converted with Kimera's TMD to P conversion).
        public static Result ExportTMD(TMDModel tmd, string tmdName, Options opt)
        {
            Result res = new Result();
            GltfRigExporter.Rig rig = NewRig();
            string name = Path.GetFileNameWithoutExtension(tmdName ?? "model");
            DefaultName(opt, name);

            int n = tmd.TMDObjectList?.Length ?? 0;
            for (int i = 0; i < n; i++)
            {
                PModel m = new PModel { fileName = name + "_" + (i + 1).ToString("000", CultureInfo.InvariantCulture) };
                try
                {
                    ConvertTMD2PModel(ref m, tmd, i);
                }
                catch (Exception ex)
                {
                    res.Warnings.Add("TMD object " + i + " can't be converted (" + ex.Message + "); skipped.");
                    continue;
                }
                if (m.Polys == null || m.Groups == null) continue;
                rig.Parts.Add(new GltfRigExporter.Part
                {
                    MeshName = m.fileName.ToUpperInvariant(),
                    Joint = 0,
                    Model = m,
                    BoneXf = GltfMath.Identity(),
                    PartXf = GltfMath.Identity(),
                    Texture = null,
                });
            }
            res.Report.Add("TMD objects: " + n + " (exported untextured, with vertex colours: TMD textures aren't loaded by Kimera)");

            return Write(rig, opt, res, tmdName + " (TMD)");
        }

        // A battle scene (battle skeleton with no bones): every piece as Kimera draws it.
        public static Result ExportBattleLocation(BattleSkeleton skel, Options opt)
        {
            Result res = new Result();
            GltfRigExporter.Rig rig = NewRig();
            string baseName = skel.fileName.Substring(0, 2).ToUpperInvariant();
            DefaultName(opt, FF7BattleGltfExporter.FirstPieceName(skel, false));

            List<TEX> textures = skel.textures;
            Func<int, TEX?> texOf = texID => textures != null && texID >= 0 && texID < textures.Count ? textures[texID] : (TEX?)null;

            for (int bi = 0; bi < skel.bones.Count; bi++)
            {
                BattleBone bone = skel.bones[bi];
                if (bone.Models == null) continue;
                for (int mi = 0; mi < bone.Models.Count; mi++)
                {
                    PModel m = bone.Models[mi];
                    if (m.Polys == null || m.Groups == null) continue;
                    rig.Parts.Add(new GltfRigExporter.Part
                    {
                        MeshName = FF7BattleGltfExporter.BattlePartName(baseName, bi) + (mi > 0 ? "_" + mi : ""),
                        Joint = 0,
                        Model = m,
                        BoneXf = GltfMath.Scaling(bone.resizeX, bone.resizeY, bone.resizeZ),
                        PartXf = PartXfXYZ(m),
                        Texture = texOf,
                    });
                }
            }

            return Write(rig, opt, res, skel.fileName + " (battle scene, " + skel.bones.Count + " pieces)");
        }
    }
}
