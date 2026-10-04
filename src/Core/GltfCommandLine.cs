using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace KimeraCS
{

    using static FF7Skeleton;
    using static FF7FieldAnimation;
    using static FF7BattleAnimation;
    using static FF7PModel;
    using static FF7TMDModel;

    //
    // Command-line glTF export (no window), used for automated testing and batch export.
    //
    // Field / world map / mini-game (HRC) models:
    //   KimeraCS.exe --export-gltf-field <model.hrc> --out <folder> --anims ACFE,AAFF,...
    //                [--name AABA] [--fps 30] [--prefix cloud] [--no-dds] [--no-bake]
    //                [--anim-dir <folder>] [--rest zero|frame] [--rest-anim ACFE[:frame]]
    //                [--60fps] [--loops all|auto|none] [--report <file>]
    //   --anims all: every compatible animation (Ifalna database order, so the default idle is first).
    //   --name p: name the file after the model's first .p file (what FFNx looks for).
    //   --rest frame (default) takes the rest pose from --rest-anim (default: the first animation in
    //   --anims, frame 0). The root placement always comes from that frame.
    //   --60fps: field 30 -> 60 fps (2 keys per frame), battle/magic 15 -> 60 fps (4 keys per frame).
    //   --loops auto (default): loop or one-shot per animation as the official 60FPS mod (SixtyFpsLoops);
    //   all: every animation also blends back into its first frame; none: no animation does.
    //
    // Battle (??AA) and magic/summon (.D) models:
    //   KimeraCS.exe --export-gltf-battle <model> --out <folder>
    //                [--name RTAA] [--fps 30] [--prefix cloud_b] [--no-dds] [--no-bake]
    //                [--anims all|none|0,1,5] [--limits auto|none|LIMCL2,BLAVER] [--limits-dir <folder>]
    //                [--weapons all|current|none] [--weapon 0] [--rest zero|frame] [--rest-anim 0[:frame]]
    //                [--60fps] [--loops all|auto|none]
    //
    // Static models (RSD resource, single .P, .TMD, or a battle scene ??AA):
    //   KimeraCS.exe --export-gltf-static <file> --out <folder> [--name X] [--prefix x] [--no-dds] [--no-bake]
    //
    // The report is written to <out>\<name>_export_report.txt. Exit code 0 = success.
    //
    public static class GltfCommandLine
    {
        public const string SWITCH = "--export-gltf-field";
        public const string SWITCH_BATTLE = "--export-gltf-battle";
        public const string SWITCH_STATIC = "--export-gltf-static";

        public static bool Handles(string arg) => arg == SWITCH || arg == SWITCH_BATTLE || arg == SWITCH_STATIC;

        public static int Run(string[] args)
        {
            if (args[0] == SWITCH_BATTLE) return RunBattle(args);
            if (args[0] == SWITCH_STATIC) return RunStatic(args);
            return RunField(args);
        }

        private static int RunStatic(string[] args)
        {
            string reportPath = null;

            try
            {
                string file = args.Length > 1 ? args[1] : "";
                FF7StaticGltfExporter.Options opt = new FF7StaticGltfExporter.Options();

                for (int i = 2; i < args.Length; i++)
                {
                    string a = args[i].ToLowerInvariant();
                    string Next() => i + 1 < args.Length ? args[++i] : "";

                    switch (a)
                    {
                        case "--out": opt.OutputFolder = Next(); break;
                        case "--name": opt.FileName = Next(); break;
                        case "--prefix": opt.TexturePrefix = Next(); break;
                        case "--no-dds": opt.WriteDDS = false; break;
                        case "--no-bake": opt.BakeVertexColors = false; break;
                        case "--report": reportPath = Next(); break;
                        default: throw new ArgumentException("Unknown option " + args[i]);
                    }
                }

                if (!File.Exists(file)) throw new ArgumentException("File not found: " + file);
                if (opt.OutputFolder == "") throw new ArgumentException("--out is required");

                string folder = Path.GetDirectoryName(Path.GetFullPath(file));
                string ext = Path.GetExtension(file).ToUpperInvariant();
                // Without --name each exporter names the file after the model's first piece (an RSD's .p file,
                // a battle scene's first piece); the report follows that name once it is known.
                bool namedReport = reportPath != null;
                Directory.CreateDirectory(opt.OutputFolder);
                if (!namedReport)
                    reportPath = Path.Combine(opt.OutputFolder, (string.IsNullOrWhiteSpace(opt.FileName)
                        ? Path.GetFileNameWithoutExtension(file).ToUpperInvariant() : opt.FileName) + "_export_report.txt");

                FileTools.bDontCheckRepairPolys = true;
                if (FileTools.lstBattleLimitsAnimations == null) FileTools.PrepareLimitsFilterFile();

                FF7StaticGltfExporter.Result res;
                if (ext == ".RSD")
                {
                    if (LoadRSDResourceModel(folder, Path.GetFileNameWithoutExtension(file)) != 0)
                        throw new InvalidOperationException("Could not load " + file + " as an RSD resource.");
                    res = FF7StaticGltfExporter.ExportRSD(fSkeleton, opt);
                }
                else if (ext == ".TMD")
                {
                    TMDModel tmd = new TMDModel();
                    LoadTMDModel(ref tmd, folder, Path.GetFileName(file));
                    res = FF7StaticGltfExporter.ExportTMD(tmd, Path.GetFileName(file).ToUpperInvariant(), opt);
                }
                else if (ext == ".P")
                {
                    PModel m = new PModel();
                    LoadPModel(ref m, folder, Path.GetFileName(file), true);
                    m.resizeX = m.resizeY = m.resizeZ = 1;
                    res = FF7StaticGltfExporter.ExportPModel(m, opt);
                }
                else
                {
                    if (LoadSkeleton(file, true) != 1 || modelType != K_AA_SKELETON || !bSkeleton.IsBattleLocation)
                        throw new InvalidOperationException(file + " is not an RSD, P, TMD or battle scene file.");
                    res = FF7StaticGltfExporter.ExportBattleLocation(bSkeleton, opt);
                }

                if (!namedReport && !string.IsNullOrWhiteSpace(opt.FileName))
                    reportPath = Path.Combine(opt.OutputFolder, opt.FileName + "_export_report.txt");
                WriteReport(reportPath, GltfRigExporter.FormatReport(res));
                return res.Success ? 0 : 1;
            }
            catch (Exception ex)
            {
                return Fail(reportPath, ex);
            }
        }

        private static GltfRigExporter.LoopMode ParseLoops(string s)
        {
            s = s.ToLowerInvariant();
            return s == "all" ? GltfRigExporter.LoopMode.All : s == "none" ? GltfRigExporter.LoopMode.None : GltfRigExporter.LoopMode.Auto;
        }

        private static void WriteReport(string path, string text)
        {
            string dir = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, text);
        }

        private static int Fail(string reportPath, Exception ex)
        {
            string msg = "Export failed: " + ex.Message + Environment.NewLine + ex.StackTrace;
            try
            {
                WriteReport(reportPath ?? Path.Combine(Path.GetTempPath(), "kimera_gltf_export_error.txt"), msg);
            }
            catch { }
            return 2;
        }

        private static int RunField(string[] args)
        {
            string reportPath = null;

            try
            {
                string hrc = args.Length > 1 ? args[1] : "";
                FF7FieldGltfExporter.Options opt = new FF7FieldGltfExporter.Options();
                string restAnim = null, animsArg = null, reportArg = null;

                for (int i = 2; i < args.Length; i++)
                {
                    string a = args[i].ToLowerInvariant();
                    string Next() => i + 1 < args.Length ? args[++i] : "";

                    switch (a)
                    {
                        case "--out": opt.OutputFolder = Next(); break;
                        case "--name": opt.FileName = Next(); break;
                        case "--fps": opt.Fps = float.Parse(Next(), CultureInfo.InvariantCulture); break;
                        case "--prefix": opt.TexturePrefix = Next(); break;
                        case "--no-dds": opt.WriteDDS = false; break;
                        case "--no-bake": opt.BakeVertexColors = false; break;
                        case "--anim-dir": opt.AnimationFolder = Next(); break;
                        case "--anims": animsArg = Next(); break;
                        case "--rest":
                            opt.RestPose = Next().ToLowerInvariant() == "zero"
                                ? FF7FieldGltfExporter.RestPoseMode.AllZero
                                : FF7FieldGltfExporter.RestPoseMode.CurrentFrame;
                            break;
                        case "--rest-anim": restAnim = Next(); break;
                        case "--60fps": opt.To60Fps = true; break;
                        case "--loops": opt.Loops = ParseLoops(Next()); break;
                        case "--report": reportArg = Next(); break;
                        default: throw new ArgumentException("Unknown option " + args[i]);
                    }
                }

                reportPath = reportArg;
                if (!File.Exists(hrc)) throw new ArgumentException("HRC file not found: " + hrc);
                if (opt.OutputFolder == "") throw new ArgumentException("--out is required");
                if (opt.AnimationFolder == "") opt.AnimationFolder = Path.GetDirectoryName(Path.GetFullPath(hrc));

                Directory.CreateDirectory(opt.OutputFolder);

                // No one can answer Kimera's "fix duplicated vertex index?" question here; skip that
                // check (same as File > Don't check duplicated polys/verts).
                FileTools.bDontCheckRepairPolys = true;

                if (LoadSkeleton(hrc, true) != 1 || modelType != K_HRC_SKELETON)
                    throw new InvalidOperationException("Could not load " + hrc + " as a field model.");

                // default (and --name p): the model's first .p file (FFNx loads mesh\field\<p name>.gltf)
                if (string.IsNullOrWhiteSpace(opt.FileName) || string.Equals(opt.FileName, "p", StringComparison.OrdinalIgnoreCase))
                    opt.FileName = FF7FieldGltfExporter.FirstPieceName(fSkeleton);
                if (reportPath == null) reportPath = Path.Combine(opt.OutputFolder, opt.FileName + "_export_report.txt");

                // --anims all: every compatible animation (Ifalna order, so the default idle comes first)
                string animSource = null;
                if (animsArg != null && animsArg.Trim().ToLowerInvariant() == "all")
                    opt.AnimationNames = FF7FieldGltfExporter.CompatibleAnimations(hrc, opt.AnimationFolder, fSkeleton.bones.Count, out animSource);
                else if (animsArg != null)
                    opt.AnimationNames = animsArg.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                // rest frame
                if (restAnim == null && opt.AnimationNames.Count > 0) restAnim = opt.AnimationNames[0];
                if (restAnim != null)
                {
                    string[] parts = restAnim.Split(':');
                    string an = parts[0].Trim();
                    if (an.EndsWith(".a", StringComparison.OrdinalIgnoreCase)) an = an.Substring(0, an.Length - 2);
                    int frame = parts.Length > 1 ? int.Parse(parts[1], CultureInfo.InvariantCulture) : 0;
                    FieldAnimation anim = FF7FieldGltfExporter.ReadAnimation(Path.Combine(opt.AnimationFolder, an + ".A"));
                    opt.RestFrame = anim.frames[Math.Min(frame, anim.frames.Count - 1)];
                }
                else if (fAnimation.frames != null && fAnimation.frames.Count > 0)
                {
                    opt.RestFrame = fAnimation.frames[0];
                }

                FF7FieldGltfExporter.Result res = FF7FieldGltfExporter.Export(fSkeleton, opt);
                if (animSource != null) res.Report.Insert(1, "Animation list: " + animSource);
                WriteReport(reportPath, GltfRigExporter.FormatReport(res));
                return res.Success ? 0 : 1;
            }
            catch (Exception ex)
            {
                return Fail(reportPath, ex);
            }
        }

        private static int RunBattle(string[] args)
        {
            string reportPath = null;

            try
            {
                string model = args.Length > 1 ? args[1] : "";
                FF7BattleGltfExporter.Options opt = new FF7BattleGltfExporter.Options();
                string anims = "all", limits = "auto", limitsDir = null, restAnim = "0";

                for (int i = 2; i < args.Length; i++)
                {
                    string a = args[i].ToLowerInvariant();
                    string Next() => i + 1 < args.Length ? args[++i] : "";

                    switch (a)
                    {
                        case "--out": opt.OutputFolder = Next(); break;
                        case "--name": opt.FileName = Next(); break;
                        case "--fps": opt.Fps = float.Parse(Next(), CultureInfo.InvariantCulture); break;
                        case "--prefix": opt.TexturePrefix = Next(); break;
                        case "--no-dds": opt.WriteDDS = false; break;
                        case "--no-bake": opt.BakeVertexColors = false; break;
                        case "--anims": anims = Next(); break;
                        case "--limits": limits = Next(); break;
                        case "--limits-dir": limitsDir = Next(); break;
                        case "--weapon": opt.CurrentWeapon = int.Parse(Next(), CultureInfo.InvariantCulture); break;
                        case "--weapons":
                            string w = Next().ToLowerInvariant();
                            opt.Weapons = w == "none" ? FF7BattleGltfExporter.WeaponsMode.None
                                        : w == "current" ? FF7BattleGltfExporter.WeaponsMode.Current
                                        : FF7BattleGltfExporter.WeaponsMode.All;
                            break;
                        case "--rest":
                            opt.RestPose = Next().ToLowerInvariant() == "zero"
                                ? FF7BattleGltfExporter.RestPoseMode.AllZero
                                : FF7BattleGltfExporter.RestPoseMode.CurrentFrame;
                            break;
                        case "--rest-anim": restAnim = Next(); break;
                        case "--60fps": opt.To60Fps = true; break;
                        case "--loops": opt.Loops = ParseLoops(Next()); break;
                        case "--report": reportPath = Next(); break;
                        default: throw new ArgumentException("Unknown option " + args[i]);
                    }
                }

                if (!File.Exists(model)) throw new ArgumentException("Model file not found: " + model);
                if (opt.OutputFolder == "") throw new ArgumentException("--out is required");

                string folder = Path.GetDirectoryName(Path.GetFullPath(model));

                Directory.CreateDirectory(opt.OutputFolder);
                bool namedReport = reportPath != null;
                if (!namedReport)
                    reportPath = Path.Combine(opt.OutputFolder, (string.IsNullOrWhiteSpace(opt.FileName)
                        ? Path.GetFileNameWithoutExtension(model).ToUpperInvariant() : opt.FileName) + "_export_report.txt");

                FileTools.bDontCheckRepairPolys = true;

                // The limit break table is normally built when the main window reads its settings.
                if (FileTools.lstBattleLimitsAnimations == null) FileTools.PrepareLimitsFilterFile();

                if (LoadSkeleton(model, true) != 1 || (modelType != K_AA_SKELETON && modelType != K_MAGIC_SKELETON))
                    throw new InvalidOperationException("Could not load " + model + " as a battle or magic model.");
                bool isMagic = modelType == K_MAGIC_SKELETON;

                // default: the model's first piece (Cloud: RTAM; magic: the model's own name), which is what
                // FFNx looks for; the report follows that name
                if (string.IsNullOrWhiteSpace(opt.FileName)) opt.FileName = FF7BattleGltfExporter.FirstPieceName(bSkeleton, isMagic);
                if (!namedReport) reportPath = Path.Combine(opt.OutputFolder, opt.FileName + "_export_report.txt");

                // main animation pack (the one Kimera loads with the model)
                string packName = isMagic ? Path.GetFileNameWithoutExtension(model).ToUpperInvariant() + ".A00"
                                          : Path.GetFileName(model).Substring(0, 2).ToUpperInvariant() + "DA";
                string packFile = Path.Combine(folder, packName);
                if (anims.ToLowerInvariant() != "none" && File.Exists(packFile))
                {
                    opt.AnimationPackFile = packFile;
                    if (anims.ToLowerInvariant() != "all")
                        opt.AnimationIndexes = anims.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                    .Select(s => int.Parse(s, CultureInfo.InvariantCulture)).ToList();
                }

                // limit breaks
                if (!isMagic)
                {
                    string dir = limitsDir ?? FF7BattleGltfExporter.DefaultLimitsFolder(folder);
                    if (limits.ToLowerInvariant() == "auto")
                    {
                        opt.LimitPackFiles = FF7BattleGltfExporter.FindLimitPacks(model, dir);
                        if (opt.LimitPackFiles.Count == 0) opt.LimitsSearchFolder = dir;
                    }
                    else if (limits.ToLowerInvariant() != "none")
                        opt.LimitPackFiles = limits.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(s => Path.Combine(dir, s.ToUpperInvariant().EndsWith(".A00") ? s : s + ".A00"))
                                                   .ToList();
                }

                // rest frame from Kimera's loaded pack
                string[] rp = restAnim.Split(':');
                int ra = int.Parse(rp[0], CultureInfo.InvariantCulture);
                int rf = rp.Length > 1 ? int.Parse(rp[1], CultureInfo.InvariantCulture) : 0;
                if (bAnimationsPack.SkeletonAnimations != null && ra < bAnimationsPack.SkeletonAnimations.Count &&
                    bAnimationsPack.SkeletonAnimations[ra].frames != null && bAnimationsPack.SkeletonAnimations[ra].frames.Count > 0)
                {
                    List<BattleFrame> fr = bAnimationsPack.SkeletonAnimations[ra].frames;
                    opt.RestFrame = fr[Math.Min(rf, fr.Count - 1)];
                    if (bAnimationsPack.WeaponAnimations != null && ra < bAnimationsPack.WeaponAnimations.Count &&
                        bAnimationsPack.WeaponAnimations[ra].frames != null && bAnimationsPack.WeaponAnimations[ra].frames.Count > 0)
                    {
                        List<BattleFrame> wf = bAnimationsPack.WeaponAnimations[ra].frames;
                        opt.RestWeaponFrame = wf[Math.Min(rf, wf.Count - 1)];
                    }
                }

                FF7BattleGltfExporter.Result res = FF7BattleGltfExporter.Export(bSkeleton, isMagic, opt);
                WriteReport(reportPath, GltfRigExporter.FormatReport(res));
                return res.Success ? 0 : 1;
            }
            catch (Exception ex)
            {
                return Fail(reportPath, ex);
            }
        }
    }
}
