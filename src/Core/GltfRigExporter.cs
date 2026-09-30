using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;

namespace KimeraCS
{

    using static FF7PModel;
    using static FF7TEXTexture;

    using static Utils;

    //
    // Writes a rigidly skinned FF7 model ("rig": joints + P model parts + animations) as glTF 2.0
    // for FFNx's smooth-skinned model loader. The format adapters (FF7FieldGltfExporter,
    // FF7BattleGltfExporter) turn Kimera's loaded models into a Rig.
    //
    // Conventions (checked against a gltf that works in FFNx 1.24.0, see PROJECT_NOTES.md):
    //   - skin.joints in depth-first order; FFNx matches animation channels to joints by name
    //   - a "root" node (not a joint) holds the root placement, with a 180 degree turn about Z
    //     (FF7 is Y-down, glTF is Y-up); FFNx ignores it, viewers use it
    //   - meshes are children of "root", never of bones; each part is rigidly bound to its bone
    //   - every joint gets translation AND rotation keys in every animation
    //   - each accessor has its own bufferView (FFNx ignores accessor offsets and strides)
    //
    public static class GltfRigExporter
    {
        public class Joint
        {
            public string Name;
            public int Parent = -1;                                   // index in Rig.Joints, -1 = child of "root"
            public double[] RestT = { 0, 0, 0 };
            public Quaternion RestR = new Quaternion { w = 1 };
        }

        public class Part
        {
            public string MeshName;
            public int Joint;
            public PModel Model;
            public double[] BoneXf;                                   // bone-space transforms applied before the
            public double[] PartXf;                                   // P model's own group transforms
            public Func<int, TEX?> Texture;                           // texture of a group texID (null = none)
        }

        public class Animation
        {
            public string Name;
            public int Frames;
            public float[][] T;                                       // per joint: Frames * 3
            public float[][] R;                                       // per joint: Frames * 4 (x, y, z, w)
            public float[] RootT;                                     // Frames * 3
            public float[] RootR;                                     // Frames * 4
        }

        public class Rig
        {
            public List<Joint> Joints = new List<Joint>();
            public List<Part> Parts = new List<Part>();
            public List<Animation> Animations = new List<Animation>();
            public double[] RootRestT = { 0, 0, 0 };
            public Quaternion RootRestR = GltfMath.FLIP_Z;

            // Static models (no animations): vertices and inverse bind matrices are stored in the
            // game's own space and the root node's flip only applies in viewers. FFNx draws a model
            // without a matching animation exactly as stored, so static models come out right there.
            public bool Static = false;
        }

        public class Options
        {
            public string OutputFolder = "";
            public string FileName = "";              // without extension
            public float Fps = 30;                    // only used for timestamps
            public string TexturePrefix = "";         // image names become <prefix>_0, <prefix>_1 ...
            public bool WriteDDS = true;
            public bool BakeVertexColors = true;      // untextured parts get a baked colour texture
            public bool Unlit = true;                 // KHR_materials_unlit: field colours have the lighting baked in;
                                                      // battle models are lit by the game (viewers only, FFNx ignores it)
            public bool DoubleFrameRate = false;      // e.g. 30 -> 60 fps: an in-between key after every frame, timestamps at 2 x Fps
            public LoopMode Loops = LoopMode.Auto;    // whether the last frame also blends back into the first
        }

        public enum LoopMode { Auto, All, None }

        public class Result
        {
            public bool Success;
            public List<string> Report = new List<string>();
            public List<string> Warnings = new List<string>();
            public List<string> Errors = new List<string>();
            public List<string> FilesWritten = new List<string>();
        }

        private const string GENERATOR = "KimeraCS glTF exporter for FFNx";
        public const int FFNX_MAX_BONES = 128;
        private const int BAKE_CELL = 8;              // pixels per baked triangle cell
        private const int BAKE_MIP_LEVELS = 4;        // 8x8 cells stay >= 1 pixel at the last level

        // FF7 triangles are wound the opposite way to glTF's counter-clockwise front faces
        // (Kimera draws them with glCullFace(GL_FRONT)).
        private const bool FLIP_WINDING = true;

        // ------------------------------------------------------------------------------------------
        // Textures
        // ------------------------------------------------------------------------------------------
        private static byte[] BitmapToBGRA(Bitmap bmp)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            byte[] bytes = new byte[bmp.Width * bmp.Height * 4];
            for (int y = 0; y < bmp.Height; y++)
                Marshal.Copy(data.Scan0 + y * data.Stride, bytes, y * bmp.Width * 4, bmp.Width * 4);
            bmp.UnlockBits(data);
            return bytes;
        }

        private static Bitmap BGRAToBitmap(byte[] bgra, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            for (int y = 0; y < height; y++)
                Marshal.Copy(bgra, y * width * 4, data.Scan0 + y * data.Stride, width * 4);
            bmp.UnlockBits(data);
            return bmp;
        }

        private class ExportContext
        {
            public Options Opt;
            public Result Res;
            public GltfBuilder Gltf = new GltfBuilder();
            public string TexFolder;
            public int Sampler = -1;
            public Dictionary<string, int> MaterialByTex = new Dictionary<string, int>();
            public int PlainColorMaterial = -1;
            public int BakeMaterial = -1;
            public int ImageCounter = 0;

            // baked vertex colour atlas
            public int BakeCols, BakeSize;
            public int BakeNext = 0;
            public byte[] BakePixels;
        }

        private static int GetSampler(ExportContext ctx)
        {
            if (ctx.Sampler < 0)
                ctx.Sampler = ctx.Gltf.AddSampler(new JsonObject { ["magFilter"] = 9729, ["minFilter"] = 9987 });
            return ctx.Sampler;
        }

        private static JsonObject NewMaterial(string name, int textureIndex, bool blend, bool unlit)
        {
            JsonObject pbr = new JsonObject();
            if (textureIndex >= 0) pbr["baseColorTexture"] = new JsonObject { ["index"] = textureIndex };
            pbr["metallicFactor"] = 0;
            pbr["roughnessFactor"] = 0.9;

            JsonObject mat = new JsonObject { ["name"] = name };
            if (blend) mat["alphaMode"] = "BLEND";
            if (unlit) mat["extensions"] = new JsonObject { ["KHR_materials_unlit"] = new JsonObject() };
            mat["pbrMetallicRoughness"] = pbr;
            return mat;
        }

        // Writes the image (PNG + optional DDS) and returns a material that uses it.
        private static int AddImageMaterial(ExportContext ctx, string imageName, byte[] bgra, int width, int height,
                                            bool blend, int ddsMipLevels)
        {
            string pngPath = Path.Combine(ctx.TexFolder, imageName + ".png");
            using (Bitmap bmp = BGRAToBitmap(bgra, width, height)) bmp.Save(pngPath, ImageFormat.Png);
            ctx.Res.FilesWritten.Add(pngPath);

            if (ctx.Opt.WriteDDS)
            {
                string ddsPath = Path.Combine(ctx.TexFolder, imageName + ".dds");
                DDSWriter.Write(ddsPath, bgra, width, height, ddsMipLevels);
                ctx.Res.FilesWritten.Add(ddsPath);
            }

            // FFNx finds the DDS by the uri file name but matches the material by image.name:
            // they must be identical.
            int img = ctx.Gltf.AddImage(new JsonObject
            {
                ["mimeType"] = "image/png",
                ["name"] = imageName,
                ["uri"] = "textures/" + imageName + ".png",
            });
            int tex = ctx.Gltf.AddTexture(new JsonObject { ["sampler"] = GetSampler(ctx), ["source"] = img });

            return ctx.Gltf.AddMaterial(NewMaterial(imageName, tex, blend, ctx.Opt.Unlit));
        }

        private static int GetTextureMaterial(ExportContext ctx, TEX tex)
        {
            string key = (tex.TEXfileName ?? "").ToUpperInvariant();
            if (ctx.MaterialByTex.TryGetValue(key, out int mat)) return mat;

            string imageName = ctx.Opt.TexturePrefix + "_" + ctx.ImageCounter.ToString(CultureInfo.InvariantCulture);
            ctx.ImageCounter++;

            byte[] bgra;
            using (Bitmap bmp = FrmTEXToPNGBatchConversion.PutPixelDataIntoBitmap32ARGB(tex, false))
                bgra = BitmapToBGRA(bmp);

            mat = AddImageMaterial(ctx, imageName, bgra, tex.width, tex.height, true, 0);
            ctx.MaterialByTex[key] = mat;
            ctx.Res.Report.Add("  texture " + key + " -> textures\\" + imageName + ".png" +
                               (ctx.Opt.WriteDDS ? " + .dds" : "") + " (" + tex.width + "x" + tex.height + ")");
            return mat;
        }

        // The group's texture when it can be exported textured, otherwise null.
        private static TEX? UsableTexture(Part part, PGroup group)
        {
            PModel model = part.Model;
            if (group.texFlag != 1 || model.TexCoords == null || model.TexCoords.Length == 0 || part.Texture == null)
                return null;
            TEX? tex = part.Texture(group.texID);
            if (tex == null || tex.Value.pixelData == null || tex.Value.width <= 0) return null;
            return tex;
        }

        private static bool IsFlatShaded(PHundret h)
        {
            // Same decision as ModelDrawing.DrawPModel (V_SHADEMODE).
            if ((h.field_C & 0x20000) == 0) return false;
            if ((h.field_8 & 0x20000) == 0) return true;
            return h.shademode != 2;
        }

        // Fills one BAKE_CELL x BAKE_CELL cell with a triangle's vertex colours and returns the UVs
        // of the triangle's three corners. Pixels outside the triangle get the nearest colour so
        // filtering never bleeds the neighbouring cell in.
        private static float[] BakeTriangle(ExportContext ctx, Color c0, Color c1, Color c2)
        {
            int cell = ctx.BakeNext++;
            int x0 = (cell % ctx.BakeCols) * BAKE_CELL;
            int y0 = (cell / ctx.BakeCols) * BAKE_CELL;

            // corner positions in pixel space (texel centres, inset by one texel)
            double ax = x0 + 1.5, ay = y0 + 1.5;
            double bx = x0 + BAKE_CELL - 1.5, by = y0 + 1.5;
            double cx = x0 + 1.5, cy = y0 + BAKE_CELL - 1.5;
            double area = (bx - ax) * (cy - ay) - (cx - ax) * (by - ay);

            for (int py = y0; py < y0 + BAKE_CELL; py++)
                for (int px = x0; px < x0 + BAKE_CELL; px++)
                {
                    double x = px + 0.5, y = py + 0.5;
                    double w1 = ((x - ax) * (cy - ay) - (cx - ax) * (y - ay)) / area;
                    double w2 = ((bx - ax) * (y - ay) - (x - ax) * (by - ay)) / area;
                    double w0 = 1 - w1 - w2;
                    w0 = Math.Max(0, w0); w1 = Math.Max(0, w1); w2 = Math.Max(0, w2);
                    double s = w0 + w1 + w2;
                    w0 /= s; w1 /= s; w2 /= s;

                    int o = (py * ctx.BakeSize + px) * 4;
                    ctx.BakePixels[o + 0] = (byte)Math.Round(c0.B * w0 + c1.B * w1 + c2.B * w2);
                    ctx.BakePixels[o + 1] = (byte)Math.Round(c0.G * w0 + c1.G * w1 + c2.G * w2);
                    ctx.BakePixels[o + 2] = (byte)Math.Round(c0.R * w0 + c1.R * w1 + c2.R * w2);
                    ctx.BakePixels[o + 3] = 255;
                }

            float size = ctx.BakeSize;
            return new float[] { (float)(ax / size), (float)(ay / size),
                                 (float)(bx / size), (float)(by / size),
                                 (float)(cx / size), (float)(cy / size) };
        }

        // ------------------------------------------------------------------------------------------
        // Mesh building
        // ------------------------------------------------------------------------------------------
        private class PrimitiveData
        {
            public List<float> Pos = new List<float>(), Nrm = new List<float>(), Uv = new List<float>(), Col = new List<float>();
            public List<int> Idx = new List<int>();
            public int Material;
            public int VertexCount => Pos.Count / 3;
        }

        private static void AddVertex(PrimitiveData p, double[] world, double[] normalWorld, Point3D v, Point3D n,
                                      float u, float vv, Color c)
        {
            GltfMath.TransformPoint(world, v.x, v.y, v.z, out double x, out double y, out double z);
            p.Pos.Add((float)x); p.Pos.Add((float)y); p.Pos.Add((float)z);

            double nx = normalWorld[0] * n.x + normalWorld[4] * n.y + normalWorld[8] * n.z;
            double ny = normalWorld[1] * n.x + normalWorld[5] * n.y + normalWorld[9] * n.z;
            double nz = normalWorld[2] * n.x + normalWorld[6] * n.y + normalWorld[10] * n.z;
            double l = Math.Sqrt(nx * nx + ny * ny + nz * nz);
            if (!(l > 1e-12) || double.IsInfinity(l)) { nx = 0; ny = 1; nz = 0; l = 1; }   // also catches NaN
            p.Nrm.Add((float)(nx / l)); p.Nrm.Add((float)(ny / l)); p.Nrm.Add((float)(nz / l));

            p.Uv.Add(u); p.Uv.Add(vv);
            p.Col.Add(c.R / 255f); p.Col.Add(c.G / 255f); p.Col.Add(c.B / 255f); p.Col.Add(1f);
        }

        // Smooth per-vertex normals from the faces, used when a P model has no normals.
        private static Point3D[] ComputeGroupNormals(PModel m, PGroup g)
        {
            Point3D[] acc = new Point3D[g.numVert];
            for (int pi = g.offsetPoly; pi < g.offsetPoly + g.numPoly; pi++)
            {
                ushort[] vi = m.Polys[pi].Verts;
                if (vi[0] >= g.numVert || vi[1] >= g.numVert || vi[2] >= g.numVert) continue;
                Point3D a = m.Verts[g.offsetVert + vi[0]], b = m.Verts[g.offsetVert + vi[1]], c = m.Verts[g.offsetVert + vi[2]];
                float ux = b.x - a.x, uy = b.y - a.y, uz = b.z - a.z;
                float wx = c.x - a.x, wy = c.y - a.y, wz = c.z - a.z;
                Point3D fn = new Point3D(uy * wz - uz * wy, uz * wx - ux * wz, ux * wy - uy * wx);
                foreach (ushort k in vi)
                {
                    acc[k].x += fn.x; acc[k].y += fn.y; acc[k].z += fn.z;
                }
            }
            return acc;
        }

        private static int BuildMesh(ExportContext ctx, Part part, double[] boneRestWorld, int jointIndex)
        {
            PModel m = part.Model;
            string meshName = part.MeshName;

            JsonArray prims = new JsonArray();
            int nTris = 0, nVerts = 0;

            for (int gi = 0; gi < m.Header.numGroups; gi++)
            {
                PGroup g = m.Groups[gi];
                if (g.numPoly <= 0 || g.numVert <= 0) continue;

                double[] grpRot = new double[16];
                BuildRotationMatrixWithQuaternionsXYZ(g.rotGroupAlpha, g.rotGroupBeta, g.rotGroupGamma, ref grpRot);
                double[] grpXf = GltfMath.Mul(GltfMath.Mul(GltfMath.Translation(g.repGroupX, g.repGroupY, g.repGroupZ), grpRot),
                                              GltfMath.Scaling(g.rszGroupX, g.rszGroupY, g.rszGroupZ));

                double[] world = GltfMath.Mul(boneRestWorld, GltfMath.Mul(part.BoneXf, GltfMath.Mul(part.PartXf, grpXf)));
                double[] nrmWorld = GltfMath.NormalMatrix(world);

                // a mirrored part (negative scale) reverses the triangle order; undo that
                double det = world[0] * (world[5] * world[10] - world[9] * world[6]) -
                             world[4] * (world[1] * world[10] - world[9] * world[2]) +
                             world[8] * (world[1] * world[6] - world[5] * world[2]);
                bool flipWinding = FLIP_WINDING ^ (det < 0);

                TEX? tex = UsableTexture(part, g);
                bool textured = tex != null;
                bool bake = !textured && ctx.Opt.BakeVertexColors;
                bool flat = m.Hundrets != null && gi < m.Hundrets.Length && IsFlatShaded(m.Hundrets[gi]);

                bool hasNormals = m.Normals != null && m.Normals.Length > 0 && m.NormalIndex != null;
                Point3D[] fallbackNormals = hasNormals ? null : ComputeGroupNormals(m, g);

                Point3D NormalOf(int vi)
                {
                    if (hasNormals)
                    {
                        int ni = g.offsetVert + vi < m.NormalIndex.Length ? m.NormalIndex[g.offsetVert + vi] : -1;
                        if (ni >= 0 && ni < m.Normals.Length)
                        {
                            // Kimera's computed normals can be NaN / zero on degenerate triangles
                            Point3D kn = m.Normals[ni];
                            double kl = Math.Sqrt(kn.x * kn.x + kn.y * kn.y + kn.z * kn.z);
                            if (kl > 1e-6 && !double.IsInfinity(kl)) return kn;
                        }
                    }
                    if (fallbackNormals == null) fallbackNormals = ComputeGroupNormals(m, g);
                    return fallbackNormals[vi];
                }

                Color ColorOf(int vi)
                {
                    int ci = g.offsetVert + vi;
                    return m.Vcolors != null && ci < m.Vcolors.Length ? m.Vcolors[ci] : Color.White;
                }

                PrimitiveData p = new PrimitiveData();
                int badPolys = 0;

                if (bake)
                {
                    // one set of 3 vertices per triangle, UVs into that triangle's atlas cell
                    for (int pi = g.offsetPoly; pi < g.offsetPoly + g.numPoly; pi++)
                    {
                        ushort[] vi = m.Polys[pi].Verts;
                        if (vi[0] >= g.numVert || vi[1] >= g.numVert || vi[2] >= g.numVert) { badPolys++; continue; }

                        Color c0 = ColorOf(vi[0]), c1 = ColorOf(vi[1]), c2 = ColorOf(vi[2]);
                        if (flat) c0 = c1 = c2;   // GL flat shading uses the last vertex's colour

                        float[] uv = BakeTriangle(ctx, c0, c1, c2);
                        int b = p.VertexCount;
                        for (int k = 0; k < 3; k++)
                            AddVertex(p, world, nrmWorld, m.Verts[g.offsetVert + vi[k]], NormalOf(vi[k]),
                                      uv[k * 2], uv[k * 2 + 1], ColorOf(vi[k]));

                        p.Idx.Add(b);
                        p.Idx.Add(flipWinding ? b + 2 : b + 1);
                        p.Idx.Add(flipWinding ? b + 1 : b + 2);
                    }
                    p.Material = ctx.BakeMaterial;
                }
                else
                {
                    for (int vi = 0; vi < g.numVert; vi++)
                    {
                        float u = 0, v = 0;
                        if (textured)
                        {
                            int ti = g.offsetTex + vi;
                            if (ti < m.TexCoords.Length) { u = m.TexCoords[ti].x; v = m.TexCoords[ti].y; }
                        }
                        AddVertex(p, world, nrmWorld, m.Verts[g.offsetVert + vi], NormalOf(vi), u, v, ColorOf(vi));
                    }

                    for (int pi = g.offsetPoly; pi < g.offsetPoly + g.numPoly; pi++)
                    {
                        ushort[] vi = m.Polys[pi].Verts;
                        if (vi[0] >= g.numVert || vi[1] >= g.numVert || vi[2] >= g.numVert) { badPolys++; continue; }
                        p.Idx.Add(vi[0]);
                        p.Idx.Add(flipWinding ? vi[2] : vi[1]);
                        p.Idx.Add(flipWinding ? vi[1] : vi[2]);
                    }

                    if (textured) p.Material = GetTextureMaterial(ctx, tex.Value);
                    else
                    {
                        if (ctx.PlainColorMaterial < 0)
                            ctx.PlainColorMaterial = ctx.Gltf.AddMaterial(NewMaterial(ctx.Opt.TexturePrefix + "_vertexcolor", -1, false, ctx.Opt.Unlit));
                        p.Material = ctx.PlainColorMaterial;
                    }

                    if (g.texFlag == 1 && !textured)
                        ctx.Res.Warnings.Add(meshName + " group " + gi + " is flagged textured but its texture is missing; " +
                                             "exported as untextured.");
                }

                if (badPolys > 0)
                    ctx.Res.Warnings.Add(meshName + " group " + gi + ": skipped " + badPolys + " polygon(s) with invalid vertex indices.");
                if (p.Idx.Count == 0) continue;

                int n = p.VertexCount;
                byte[] joints = new byte[n * 4];
                float[] weights = new float[n * 4];
                for (int i = 0; i < n; i++)
                {
                    joints[i * 4] = (byte)jointIndex;
                    weights[i * 4] = 1f;
                }

                GltfBuilder gb = ctx.Gltf;
                JsonObject attrs = new JsonObject
                {
                    ["POSITION"] = gb.AddFloatAccessor(p.Pos.ToArray(), "VEC3", GltfBuilder.TARGET_ARRAY_BUFFER, true),
                    ["NORMAL"] = gb.AddFloatAccessor(p.Nrm.ToArray(), "VEC3", GltfBuilder.TARGET_ARRAY_BUFFER, false),
                    ["TEXCOORD_0"] = gb.AddFloatAccessor(p.Uv.ToArray(), "VEC2", GltfBuilder.TARGET_ARRAY_BUFFER, false),
                    ["COLOR_0"] = gb.AddFloatAccessor(p.Col.ToArray(), "VEC4", GltfBuilder.TARGET_ARRAY_BUFFER, false),
                    ["JOINTS_0"] = gb.AddByteAccessor(joints, "VEC4", GltfBuilder.TARGET_ARRAY_BUFFER),
                    ["WEIGHTS_0"] = gb.AddFloatAccessor(weights, "VEC4", GltfBuilder.TARGET_ARRAY_BUFFER, false),
                };

                prims.Add(new JsonObject
                {
                    ["attributes"] = attrs,
                    ["indices"] = gb.AddIndexAccessor(p.Idx.ToArray(), n),
                    ["material"] = p.Material,
                });

                nTris += p.Idx.Count / 3;
                nVerts += n;
            }

            if (prims.Count == 0) return -1;

            ctx.Res.Report.Add("  mesh " + meshName + ": " + prims.Count + " primitive(s), " + nVerts + " vertices, " + nTris + " triangles");
            return ctx.Gltf.AddMesh(new JsonObject { ["name"] = meshName, ["primitives"] = prims });
        }

        private static int CountBakeTriangles(Rig rig)
        {
            int count = 0;
            foreach (Part part in rig.Parts)
            {
                PModel m = part.Model;
                if (m.Polys == null || m.Groups == null) continue;
                for (int gi = 0; gi < m.Header.numGroups; gi++)
                    if (m.Groups[gi].numVert > 0 && UsableTexture(part, m.Groups[gi]) == null)
                        count += Math.Max(0, m.Groups[gi].numPoly);
            }
            return count;
        }

        private static void DepthFirst(List<List<int>> children, int j, List<int> order)
        {
            order.Add(j);
            foreach (int c in children[j]) DepthFirst(children, c, order);
        }

        // ------------------------------------------------------------------------------------------
        // Write
        // ------------------------------------------------------------------------------------------
        public static void Write(Rig rig, Options opt, Result res)
        {
            ExportContext ctx = new ExportContext { Opt = opt, Res = res };

            if (rig.Joints.Count == 0)
            {
                res.Errors.Add("The model has no bones.");
                return;
            }
            if (string.IsNullOrWhiteSpace(opt.TexturePrefix)) opt.TexturePrefix = opt.FileName;
            if (opt.Fps <= 0) opt.Fps = 30;

            Directory.CreateDirectory(opt.OutputFolder);
            ctx.TexFolder = Path.Combine(opt.OutputFolder, "textures");
            Directory.CreateDirectory(ctx.TexFolder);

            // ---------------------------------------------------------------- joint order
            int nj = rig.Joints.Count;
            List<List<int>> children = Enumerable.Range(0, nj).Select(_ => new List<int>()).ToList();
            for (int j = 0; j < nj; j++)
                if (rig.Joints[j].Parent >= 0) children[rig.Joints[j].Parent].Add(j);

            List<int> order = new List<int>();
            for (int j = 0; j < nj; j++) if (rig.Joints[j].Parent < 0) DepthFirst(children, j, order);

            int[] skinIndex = new int[nj];
            for (int k = 0; k < order.Count; k++) skinIndex[order[k]] = k;

            // unique joint names (FFNx matches animation channels to joints by name)
            HashSet<string> used = new HashSet<string> { "root" };
            foreach (int j in order)
            {
                string baseName = string.IsNullOrWhiteSpace(rig.Joints[j].Name) ? "bone" + j : rig.Joints[j].Name;
                string nm = baseName;
                for (int s = 2; used.Contains(nm); s++) nm = baseName + "_" + s;
                if (nm != rig.Joints[j].Name)
                    res.Warnings.Add("Bone " + j + " renamed '" + rig.Joints[j].Name + "' -> '" + nm + "' (names must be unique).");
                rig.Joints[j].Name = nm;
                used.Add(nm);
            }

            if (nj > 255)
            {
                res.Errors.Add("The model has " + nj + " bones; glTF joint indices for FFNx are 8-bit (max 255).");
                return;
            }
            if (nj > FFNX_MAX_BONES)
                res.Warnings.Add("The model has " + nj + " bones; FFNx 1.24.0 supports " + FFNX_MAX_BONES + ".");

            // ---------------------------------------------------------------- rest pose
            double[] rootMatrix = GltfMath.TR(rig.RootRestT[0], rig.RootRestT[1], rig.RootRestT[2], rig.RootRestR);
            double[][] restWorld = new double[nj][];
            foreach (int j in order)
            {
                Joint jt = rig.Joints[j];
                double[] local = GltfMath.TR(jt.RestT[0], jt.RestT[1], jt.RestT[2], jt.RestR);
                double[] parent = jt.Parent >= 0 ? restWorld[jt.Parent] : rootMatrix;
                restWorld[j] = GltfMath.Mul(parent, local);
            }

            // space the vertices are stored in (see Rig.Static)
            double[][] bindWorld = restWorld;
            if (rig.Static)
            {
                double[] invRoot = GltfMath.InvertAffine(rootMatrix);
                bindWorld = restWorld.Select(w => GltfMath.Mul(invRoot, w)).ToArray();
            }

            // ---------------------------------------------------------------- joint nodes
            GltfBuilder gb = ctx.Gltf;
            if (opt.Unlit) gb.ExtensionsUsed.Add("KHR_materials_unlit");

            int[] nodeOfJoint = new int[nj];
            foreach (int j in order)
            {
                Joint jt = rig.Joints[j];
                nodeOfJoint[j] = gb.AddNode(new JsonObject
                {
                    ["name"] = jt.Name,
                    ["rotation"] = GltfMath.JQuat(jt.RestR),
                    ["translation"] = GltfMath.JArr(jt.RestT[0], jt.RestT[1], jt.RestT[2]),
                });
            }
            foreach (int j in order)
            {
                if (children[j].Count == 0) continue;
                JsonArray ch = new JsonArray();
                foreach (int c in children[j]) ch.Add(nodeOfJoint[c]);
                ((JsonObject)gb.Nodes[nodeOfJoint[j]])["children"] = ch;
            }

            // ---------------------------------------------------------------- meshes
            int bakeTris = opt.BakeVertexColors ? CountBakeTriangles(rig) : 0;
            if (bakeTris > 0)
            {
                ctx.BakeCols = (int)Math.Ceiling(Math.Sqrt(bakeTris));
                int size = 8;
                while (size < ctx.BakeCols * BAKE_CELL) size *= 2;
                ctx.BakeSize = size;
                ctx.BakePixels = new byte[size * size * 4];
                for (int i = 3; i < ctx.BakePixels.Length; i += 4) ctx.BakePixels[i] = 255;

                // the material is registered now; its image is written after all meshes are baked
                ctx.BakeMaterial = -2;
            }

            List<int> meshNodes = new List<int>();
            foreach (Part part in rig.Parts)
            {
                int mesh = BuildMesh(ctx, part, bindWorld[part.Joint], skinIndex[part.Joint]);
                if (mesh < 0) continue;
                meshNodes.Add(gb.AddNode(new JsonObject { ["mesh"] = mesh, ["name"] = part.MeshName, ["skin"] = 0 }));
            }

            if (ctx.BakeNext > 0)
            {
                string bakeName = opt.TexturePrefix + "_vc";
                int bakeMat = AddImageMaterial(ctx, bakeName, ctx.BakePixels, ctx.BakeSize, ctx.BakeSize, false, BAKE_MIP_LEVELS);
                // replace the placeholder material index
                foreach (JsonNode mesh in gb.Meshes)
                    foreach (JsonNode prim in (JsonArray)mesh["primitives"])
                        if ((int)prim["material"] == -2) prim["material"] = bakeMat;
                res.Report.Add("  baked vertex colours of " + ctx.BakeNext + " untextured triangles -> textures\\" +
                               bakeName + ".png (" + ctx.BakeSize + "x" + ctx.BakeSize + ")");
            }
            else if (!opt.BakeVertexColors && ctx.PlainColorMaterial >= 0)
            {
                res.Warnings.Add("Untextured parts use vertex colours only (bake off). FFNx 1.24.0 draws them invisible.");
            }

            if (meshNodes.Count == 0) res.Warnings.Add("The model has no geometry.");

            // ---------------------------------------------------------------- root + skin
            JsonArray rootChildren = new JsonArray();
            foreach (int mn in meshNodes) rootChildren.Add(mn);
            foreach (int j in order) if (rig.Joints[j].Parent < 0) rootChildren.Add(nodeOfJoint[j]);

            int rootNode = gb.AddNode(new JsonObject
            {
                ["children"] = rootChildren,
                ["name"] = "root",
                ["rotation"] = GltfMath.JQuat(rig.RootRestR),
                ["translation"] = GltfMath.JArr(rig.RootRestT[0], rig.RootRestT[1], rig.RootRestT[2]),
            });
            gb.SceneNodes.Add(rootNode);

            float[] ibm = new float[order.Count * 16];
            JsonArray skinJoints = new JsonArray();
            for (int k = 0; k < order.Count; k++)
            {
                double[] inv = GltfMath.InvertAffine(bindWorld[order[k]]);
                for (int e = 0; e < 16; e++) ibm[k * 16 + e] = (float)inv[e];
                skinJoints.Add(nodeOfJoint[order[k]]);
            }
            gb.AddSkin(new JsonObject
            {
                ["inverseBindMatrices"] = gb.AddFloatAccessor(ibm, "MAT4", 0, false),
                ["joints"] = skinJoints,
                ["name"] = "root",
            });

            // ---------------------------------------------------------------- animations
            foreach (Animation source in rig.Animations)
            {
                Animation anim = source;
                float fps = opt.Fps;
                if (opt.DoubleFrameRate)
                {
                    fps = opt.Fps * 2;
                    if (source.Frames > 1)
                    {
                        anim = DoubleFrameRate(source, opt.Loops, out string how);
                        res.Report.Add("  x2 fps " + source.Name + ": " + source.Frames + " -> " + anim.Frames + " keys (" + how + ")");
                    }
                }

                int nf = anim.Frames;
                float[] times = new float[nf];
                for (int f = 0; f < nf; f++) times[f] = f / fps;

                JsonArray channels = new JsonArray();
                JsonArray samplers = new JsonArray();
                int input = gb.AddFloatAccessor(times, "SCALAR", 0, true);

                void AddChannel(int node, string pathName, float[] values, string type)
                {
                    int output = gb.AddFloatAccessor(values, type, 0, false);
                    samplers.Add(new JsonObject { ["input"] = input, ["interpolation"] = "LINEAR", ["output"] = output });
                    channels.Add(new JsonObject
                    {
                        ["sampler"] = samplers.Count - 1,
                        ["target"] = new JsonObject { ["node"] = node, ["path"] = pathName },
                    });
                }

                foreach (int j in order)
                {
                    AddChannel(nodeOfJoint[j], "translation", anim.T[j], "VEC3");
                    AddChannel(nodeOfJoint[j], "rotation", anim.R[j], "VEC4");
                }

                // root node: FFNx ignores it, viewers (Blender, Maya) use it
                AddChannel(rootNode, "translation", anim.RootT, "VEC3");
                AddChannel(rootNode, "rotation", anim.RootR, "VEC4");

                gb.AddAnimation(new JsonObject { ["channels"] = channels, ["name"] = anim.Name, ["samplers"] = samplers });
            }

            if (rig.Animations.Count == 0 && !rig.Static)
                res.Warnings.Add("No animations exported. In FFNx the model will be drawn in its rest pose.");

            // ---------------------------------------------------------------- save
            gb.Save(opt.OutputFolder, opt.FileName, GENERATOR);
            res.FilesWritten.Insert(0, Path.Combine(opt.OutputFolder, opt.FileName + ".bin"));
            res.FilesWritten.Insert(0, Path.Combine(opt.OutputFolder, opt.FileName + ".gltf"));

            res.Success = res.Errors.Count == 0;
        }

        // ------------------------------------------------------------------------------------------
        // 30 -> 60 fps conversion
        // ------------------------------------------------------------------------------------------
        private static double KeyAngle(float[] r, int f, int g)
        {
            double d = Math.Abs(r[f * 4] * r[g * 4] + r[f * 4 + 1] * r[g * 4 + 1] + r[f * 4 + 2] * r[g * 4 + 2] + r[f * 4 + 3] * r[g * 4 + 3]);
            return 2 * Math.Acos(Math.Min(1.0, d)) * 180 / Math.PI;
        }

        private static void Slerp(float[] src, int a, int b, double t, float[] dst, int di)
        {
            double ax = src[a * 4], ay = src[a * 4 + 1], az = src[a * 4 + 2], aw = src[a * 4 + 3];
            double bx = src[b * 4], by = src[b * 4 + 1], bz = src[b * 4 + 2], bw = src[b * 4 + 3];
            double d = ax * bx + ay * by + az * bz + aw * bw;
            if (d < 0) { bx = -bx; by = -by; bz = -bz; bw = -bw; d = -d; }
            double wa, wb;
            if (d > 0.9995) { wa = 1 - t; wb = t; }
            else
            {
                double th = Math.Acos(d), sn = Math.Sin(th);
                wa = Math.Sin((1 - t) * th) / sn; wb = Math.Sin(t * th) / sn;
            }
            double x = ax * wa + bx * wb, y = ay * wa + by * wb, z = az * wa + bz * wb, w = aw * wa + bw * wb;
            double l = Math.Sqrt(x * x + y * y + z * z + w * w);
            if (l < 1e-12) l = 1;
            // stay in the hemisphere of the previous key so viewers interpolate the short way
            if (di > 0 && x * dst[di * 4 - 4] + y * dst[di * 4 - 3] + z * dst[di * 4 - 2] + w * dst[di * 4 - 1] < 0) l = -l;
            dst[di * 4] = (float)(x / l); dst[di * 4 + 1] = (float)(y / l); dst[di * 4 + 2] = (float)(z / l); dst[di * 4 + 3] = (float)(w / l);
        }

        private static void Lerp(float[] src, int a, int b, double t, float[] dst, int di)
        {
            for (int c = 0; c < 3; c++) dst[di * 3 + c] = (float)(src[a * 3 + c] * (1 - t) + src[b * 3 + c] * t);
        }

        // A looping animation ends one step before its first frame: the jump from the last frame back to
        // the first is about as big as a normal step. One-shot animations jump much further (or repeat
        // their first frame at the end, which already closes the loop).
        private static bool LooksLikeLoop(Animation a, out double seam, out double maxStep)
        {
            seam = 0; maxStep = 0;
            int nf = a.Frames;
            foreach (float[] r in a.R)
            {
                if (r == null) continue;
                for (int f = 0; f + 1 < nf; f++) maxStep = Math.Max(maxStep, KeyAngle(r, f, f + 1));
                seam = Math.Max(seam, KeyAngle(r, nf - 1, 0));
            }
            return seam > 0.01 && seam <= Math.Max(1.5 * maxStep, 0.5);
        }

        // Every original frame is kept; an in-between key (rotations slerped, translations blended) is
        // added after each frame, and for loops also between the last frame and the first.
        // n frames -> 2n keys (loop) or 2n - 1 keys (one-shot).
        public static Animation DoubleFrameRate(Animation a, LoopMode mode, out string how)
        {
            int nf = a.Frames;
            bool loop;
            if (mode == LoopMode.All) { loop = true; how = "loop"; }
            else if (mode == LoopMode.None) { loop = false; how = "no loop"; }
            else
            {
                loop = LooksLikeLoop(a, out double seam, out double maxStep);
                how = (loop ? "loop" : "no loop") + string.Format(CultureInfo.InvariantCulture,
                      ", last->first {0:0.#} deg, largest step {1:0.#} deg", seam, maxStep);
            }

            int n2 = loop ? nf * 2 : nf * 2 - 1;
            Animation o = new Animation
            {
                Name = a.Name,
                Frames = n2,
                T = new float[a.T.Length][],
                R = new float[a.R.Length][],
                RootT = new float[n2 * 3],
                RootR = new float[n2 * 4],
            };

            void Fill(float[] srcT, float[] srcR, float[] dstT, float[] dstR)
            {
                for (int k = 0; k < n2; k++)
                {
                    int f = k / 2, g = (f + 1) % nf;
                    double t = (k % 2) * 0.5;
                    if (t == 0) g = f;
                    if (dstT != null) Lerp(srcT, f, g, t, dstT, k);
                    if (dstR != null) Slerp(srcR, f, g, t, dstR, k);
                }
            }

            for (int j = 0; j < a.R.Length; j++)
            {
                if (a.R[j] == null) continue;
                o.T[j] = new float[n2 * 3];
                o.R[j] = new float[n2 * 4];
                Fill(a.T[j], a.R[j], o.T[j], o.R[j]);
            }
            Fill(a.RootT, a.RootR, o.RootT, o.RootR);
            return o;
        }

        public static string FormatReport(Result r)
        {
            List<string> lines = new List<string>(r.Report);
            if (r.Warnings.Count > 0) { lines.Add(""); lines.Add("Warnings:"); lines.AddRange(r.Warnings.Select(w => "  " + w)); }
            if (r.Errors.Count > 0) { lines.Add(""); lines.Add("Errors:"); lines.AddRange(r.Errors.Select(e => "  " + e)); }
            lines.Add("");
            lines.Add("Files written:");
            lines.AddRange(r.FilesWritten.Select(f => "  " + f));
            lines.Add("");
            lines.Add(r.Success ? "Export finished." : "Export finished with errors.");
            return string.Join(Environment.NewLine, lines);
        }
    }

    //
    // Math helpers for the glTF exporters (column-major 4x4 like OpenGL / Kimera).
    //
    public static class GltfMath
    {
        // 180 degrees about Z: FF7 (Y down) -> glTF (Y up). Quaternion (x, y, z, w).
        public static readonly Utils.Quaternion FLIP_Z = new Utils.Quaternion { x = 0, y = 0, z = -1, w = 0 };

        public static double[] Identity()
        {
            return new double[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        }

        public static double[] Mul(double[] a, double[] b)
        {
            double[] r = new double[16];
            for (int c = 0; c < 4; c++)
                for (int rr = 0; rr < 4; rr++)
                {
                    double s = 0;
                    for (int k = 0; k < 4; k++) s += a[k * 4 + rr] * b[c * 4 + k];
                    r[c * 4 + rr] = s;
                }
            return r;
        }

        public static double[] Translation(double x, double y, double z)
        {
            double[] m = Identity();
            m[12] = x; m[13] = y; m[14] = z;
            return m;
        }

        public static double[] Scaling(double x, double y, double z)
        {
            double[] m = Identity();
            m[0] = x; m[5] = y; m[10] = z;
            return m;
        }

        public static double[] RotationFromQuat(Utils.Quaternion q)
        {
            double[] m = new double[16];
            Utils.BuildMatrixFromQuaternion(q, ref m);
            return m;
        }

        public static double[] TR(double tx, double ty, double tz, Utils.Quaternion q)
        {
            return Mul(Translation(tx, ty, tz), RotationFromQuat(q));
        }

        public static double[] InvertAffine(double[] m)
        {
            // Inverse of the 3x3 part (general, handles scale) + translation.
            double a = m[0], b = m[4], c = m[8];
            double d = m[1], e = m[5], f = m[9];
            double g = m[2], h = m[6], i = m[10];
            double det = a * (e * i - f * h) - b * (d * i - f * g) + c * (d * h - e * g);
            if (Math.Abs(det) < 1e-12) det = 1e-12;
            double id = 1.0 / det;

            double[] r = Identity();
            r[0] = (e * i - f * h) * id; r[4] = (c * h - b * i) * id; r[8] = (b * f - c * e) * id;
            r[1] = (f * g - d * i) * id; r[5] = (a * i - c * g) * id; r[9] = (c * d - a * f) * id;
            r[2] = (d * h - e * g) * id; r[6] = (b * g - a * h) * id; r[10] = (a * e - b * d) * id;

            double tx = m[12], ty = m[13], tz = m[14];
            r[12] = -(r[0] * tx + r[4] * ty + r[8] * tz);
            r[13] = -(r[1] * tx + r[5] * ty + r[9] * tz);
            r[14] = -(r[2] * tx + r[6] * ty + r[10] * tz);
            return r;
        }

        public static void TransformPoint(double[] m, double x, double y, double z, out double ox, out double oy, out double oz)
        {
            ox = m[0] * x + m[4] * y + m[8] * z + m[12];
            oy = m[1] * x + m[5] * y + m[9] * z + m[13];
            oz = m[2] * x + m[6] * y + m[10] * z + m[14];
        }

        // Normal matrix = inverse transpose of the 3x3 part.
        public static double[] NormalMatrix(double[] m)
        {
            double[] inv = InvertAffine(m);
            double[] r = Identity();
            for (int row = 0; row < 3; row++)
                for (int col = 0; col < 3; col++)
                    r[col * 4 + row] = inv[row * 4 + col];
            return r;
        }

        // Same as Utils.BuildRotationMatrixWithQuaternions: q = qY(beta) * qX(alpha) * qZ(gamma)
        public static Utils.Quaternion KimeraQuat(double alpha, double beta, double gamma)
        {
            Utils.Quaternion qx = new Utils.Quaternion(), qy = new Utils.Quaternion(), qz = new Utils.Quaternion();
            Utils.Quaternion qyx = new Utils.Quaternion(), q = new Utils.Quaternion();
            Utils.Point3D px = new Utils.Point3D(1, 0, 0), py = new Utils.Point3D(0, 1, 0), pz = new Utils.Point3D(0, 0, 1);

            Utils.BuildQuaternionFromAxis(ref px, alpha, ref qx);
            Utils.BuildQuaternionFromAxis(ref py, beta, ref qy);
            Utils.BuildQuaternionFromAxis(ref pz, gamma, ref qz);

            Utils.MultiplyQuaternions(qy, qx, ref qyx);
            Utils.MultiplyQuaternions(qyx, qz, ref q);
            return Normalized(q);
        }

        public static Utils.Quaternion QMul(Utils.Quaternion a, Utils.Quaternion b)
        {
            Utils.Quaternion r = new Utils.Quaternion();
            Utils.MultiplyQuaternions(a, b, ref r);
            return r;
        }

        public static Utils.Quaternion Normalized(Utils.Quaternion q)
        {
            double l = Math.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
            if (l < 1e-12) return new Utils.Quaternion { w = 1 };
            return new Utils.Quaternion { x = q.x / l, y = q.y / l, z = q.z / l, w = q.w / l };
        }

        // Keeps consecutive keys in the same hemisphere so interpolating viewers take the short way.
        public static Utils.Quaternion SameHemisphere(Utils.Quaternion q, Utils.Quaternion prev)
        {
            if (q.x * prev.x + q.y * prev.y + q.z * prev.z + q.w * prev.w < 0)
                return new Utils.Quaternion { x = -q.x, y = -q.y, z = -q.z, w = -q.w };
            return q;
        }

        // Rotation part of a rigid 4x4 matrix as a quaternion.
        public static Utils.Quaternion QuatFromMatrix(double[] m)
        {
            double m00 = m[0], m01 = m[4], m02 = m[8];
            double m10 = m[1], m11 = m[5], m12 = m[9];
            double m20 = m[2], m21 = m[6], m22 = m[10];
            double tr = m00 + m11 + m22;
            Utils.Quaternion q = new Utils.Quaternion();
            if (tr > 0)
            {
                double s = Math.Sqrt(tr + 1.0) * 2;
                q.w = 0.25 * s; q.x = (m21 - m12) / s; q.y = (m02 - m20) / s; q.z = (m10 - m01) / s;
            }
            else if (m00 > m11 && m00 > m22)
            {
                double s = Math.Sqrt(1.0 + m00 - m11 - m22) * 2;
                q.w = (m21 - m12) / s; q.x = 0.25 * s; q.y = (m01 + m10) / s; q.z = (m02 + m20) / s;
            }
            else if (m11 > m22)
            {
                double s = Math.Sqrt(1.0 + m11 - m00 - m22) * 2;
                q.w = (m02 - m20) / s; q.x = (m01 + m10) / s; q.y = 0.25 * s; q.z = (m12 + m21) / s;
            }
            else
            {
                double s = Math.Sqrt(1.0 + m22 - m00 - m11) * 2;
                q.w = (m10 - m01) / s; q.x = (m02 + m20) / s; q.y = (m12 + m21) / s; q.z = 0.25 * s;
            }
            return Normalized(q);
        }

        public static JsonArray JArr(params double[] v)
        {
            JsonArray a = new JsonArray();
            foreach (double d in v) a.Add((float)d);
            return a;
        }

        public static JsonArray JQuat(Utils.Quaternion q) => JArr(q.x, q.y, q.z, q.w);

        public static void PutQuat(float[] dst, int frame, Utils.Quaternion q)
        {
            dst[frame * 4] = (float)q.x; dst[frame * 4 + 1] = (float)q.y; dst[frame * 4 + 2] = (float)q.z; dst[frame * 4 + 3] = (float)q.w;
        }
    }
}
