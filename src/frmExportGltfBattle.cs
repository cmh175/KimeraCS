using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace KimeraCS
{

    using static FF7BattleSkeleton;
    using static FF7BattleAnimation;
    using static FF7BattleAnimationsPack;

    //
    // Export window for battle and magic/summon models (File > Export glTF for FFNx...).
    //
    public partial class FrmExportGltfBattle : Form
    {
        // Remembered while Kimera is open.
        private static string lastOutFolder = "";
        private static int lastFps = 0;
        private static bool lastDDS = true, lastBake = true, lastRestZero = false, last60fps = false;
        private static int lastLoops = 0;

        private readonly BattleSkeleton skeleton;
        private readonly bool isMagic;
        private readonly string packFile;
        private readonly BattleFrame? restFrame, restWeaponFrame;
        private readonly int currentWeapon;
        private readonly List<int> packIndexes = new List<int>();          // list row -> pack index
        private readonly HashSet<int> checkedIndexes = new HashSet<int>();
        private readonly List<string> limitFiles = new List<string>();      // limit list row -> file

        private bool prefixEdited = false, settingPrefix = false, fillingList = false;

        public FrmExportGltfBattle(BattleSkeleton bSkeleton, bool bIsMagic, BattleAnimationsPack loadedPack,
                                   int animIndex, int frameIndex, int weaponIndex, string strModelFullPath, int kimeraFps)
        {
            InitializeComponent();

            skeleton = bSkeleton;
            isMagic = bIsMagic;
            currentWeapon = weaponIndex;
            string modelFolder = string.IsNullOrEmpty(strModelFullPath) ? "" : Path.GetDirectoryName(strModelFullPath);
            string modelName = Path.GetFileName(bSkeleton.fileName ?? "").ToUpperInvariant();
            string baseName = isMagic ? Path.GetFileNameWithoutExtension(modelName) : (modelName.Length >= 2 ? modelName.Substring(0, 2) : modelName);

            int weapons = isMagic ? 0 : bSkeleton.wpModels.Count(w => w.Polys != null);
            lblModel.Text = "Model: " + modelName + "  (" + (isMagic ? "magic/summon" : "battle") + ", " + bSkeleton.nBones + " bones" +
                            (isMagic ? "" : ", " + weapons + " weapons") + ")" + (modelFolder != "" ? "   " + modelFolder : "");

            // current frame of the viewer (the default rest pose)
            if (loadedPack.SkeletonAnimations != null && animIndex >= 0 && animIndex < loadedPack.SkeletonAnimations.Count &&
                loadedPack.SkeletonAnimations[animIndex].frames != null && loadedPack.SkeletonAnimations[animIndex].frames.Count > 0)
            {
                List<BattleFrame> fr = loadedPack.SkeletonAnimations[animIndex].frames;
                int f = Math.Max(0, Math.Min(frameIndex, fr.Count - 1));
                restFrame = CopybFrame(fr[f]);
                if (loadedPack.WeaponAnimations != null && animIndex < loadedPack.WeaponAnimations.Count &&
                    loadedPack.WeaponAnimations[animIndex].frames != null && loadedPack.WeaponAnimations[animIndex].frames.Count > 0)
                {
                    List<BattleFrame> wf = loadedPack.WeaponAnimations[animIndex].frames;
                    restWeaponFrame = CopybFrame(wf[Math.Min(f, wf.Count - 1)]);
                }
                rbRestCurrent.Text = "Current frame (ANIM_" + animIndex.ToString("00") + ", frame " + f + ")";
            }
            else
            {
                rbRestCurrent.Enabled = false;
                rbRestZero.Checked = true;
            }

            // file names: the skeleton file, or (battle) one of the part files. Magic parts are
            // <name>.P00 ...; FFNx drops the extension, which leaves the magic model's own name.
            cbFileName.Items.Add(isMagic ? baseName : modelName);
            if (!isMagic)
                for (int bi = 0; bi < bSkeleton.bones.Count; bi++)
                    if (bSkeleton.bones[bi].hasModel != 0)
                        cbFileName.Items.Add(FF7BattleGltfExporter.BattlePartName(baseName, bi));
            cbFileName.SelectedIndex = 0;

            // animation pack (read from disk, like the export does)
            string packName = isMagic ? baseName + ".A00" : baseName + "DA";
            packFile = Path.Combine(modelFolder, packName);
            if (File.Exists(packFile))
            {
                try
                {
                    BattleAnimationsPack pack = FF7BattleGltfExporter.ReadPack(packFile, bSkeleton.nBones, bSkeleton.nsSkeletonAnims,
                                                                               isMagic ? 0 : bSkeleton.nsWeaponsAnims, false);
                    for (int ai = 0; ai < pack.SkeletonAnimations.Count; ai++)
                    {
                        int n = pack.SkeletonAnimations[ai].frames?.Count ?? 0;
                        if (n == 0) continue;
                        packIndexes.Add(ai);
                        checkedIndexes.Add(ai);
                    }
                    lblPack.Text = "Animation pack " + packName + ": " + packIndexes.Count + " animations (ANIM_00 ...)";
                    fillingList = true;
                    foreach (int ai in packIndexes)
                    {
                        int n = pack.SkeletonAnimations[ai].frames.Count;
                        clbAnimations.Items.Add("ANIM_" + ai.ToString("00") + "   " + n + (n == 1 ? " frame" : " frames"), true);
                    }
                    fillingList = false;
                }
                catch (Exception ex)
                {
                    lblPack.Text = "Can't read " + packName + ": " + ex.Message;
                }
            }
            else
            {
                lblPack.Text = "No animation pack (" + packName + ") next to the model.";
                packFile = "";
            }
            UpdateSelectedCount();

            // limit breaks (battle only)
            if (isMagic)
            {
                clbLimits.Enabled = txtLimitsFolder.Enabled = btnBrowseLimits.Enabled = false;
                lblLimits.Text = "Limit breaks: battle characters only.";
                rbWeaponsAll.Enabled = rbWeaponsCurrent.Enabled = rbWeaponsNone.Enabled = false;
            }
            else
            {
                txtLimitsFolder.Text = FF7BattleGltfExporter.DefaultLimitsFolder(modelFolder);
                FillLimits();
                if (weapons == 0) rbWeaponsAll.Enabled = rbWeaponsCurrent.Enabled = rbWeaponsNone.Enabled = false;
                if (weaponIndex >= 0 && weaponIndex < bSkeleton.wpModels.Count)
                    rbWeaponsCurrent.Text = "Only current (" + baseName + "C" + (char)('K' + weaponIndex) + ")";
                else rbWeaponsCurrent.Enabled = false;
            }

            // remembered settings
            txtOutFolder.Text = lastOutFolder;
            cbFps.SelectedItem = (lastFps > 0 ? lastFps : (kimeraFps == 15 || kimeraFps == 60 ? kimeraFps : 30)).ToString();
            chkDDS.Checked = lastDDS;
            chkBake.Checked = lastBake;
            if (lastRestZero || !rbRestCurrent.Enabled) rbRestZero.Checked = true;
            cbLoops.SelectedIndex = lastLoops;
            chk60fps.Checked = last60fps;
        }

        // ---------------------------------------------------------------------------------------
        // Lists
        // ---------------------------------------------------------------------------------------
        private void UpdateSelectedCount()
        {
            lblSelectedCount.Text = checkedIndexes.Count + " of " + packIndexes.Count + " selected";
        }

        private void ClbAnimations_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (fillingList) return;
            int ai = packIndexes[e.Index];
            if (e.NewValue == CheckState.Checked) checkedIndexes.Add(ai);
            else checkedIndexes.Remove(ai);
            BeginInvoke((Action)UpdateSelectedCount);
        }

        private void SetAll(bool on)
        {
            fillingList = true;
            for (int i = 0; i < clbAnimations.Items.Count; i++) clbAnimations.SetItemChecked(i, on);
            fillingList = false;
            checkedIndexes.Clear();
            if (on) foreach (int ai in packIndexes) checkedIndexes.Add(ai);
            UpdateSelectedCount();
        }

        private void BtnSelectAll_Click(object sender, EventArgs e) => SetAll(true);
        private void BtnSelectNone_Click(object sender, EventArgs e) => SetAll(false);

        private void FillLimits()
        {
            clbLimits.Items.Clear();
            limitFiles.Clear();
            foreach (string f in FF7BattleGltfExporter.FindLimitPacks(skeleton.fileName, txtLimitsFolder.Text.Trim()))
            {
                string desc = Path.GetFileNameWithoutExtension(f).ToUpperInvariant();
                try
                {
                    BattleAnimationsPack p = FF7BattleGltfExporter.ReadPack(f, skeleton.nBones, 8, 8, true);
                    desc += "   " + p.SkeletonAnimations.Count(a => a.frames != null && a.frames.Count > 0) + " animations";
                }
                catch { desc += "   (can't read)"; }
                limitFiles.Add(f);
                clbLimits.Items.Add(desc, true);
            }
            if (limitFiles.Count == 0)
                lblLimits.Text = "Limit breaks: none found for " + Path.GetFileName(skeleton.fileName).ToUpperInvariant() + " in this folder.";
            else
                lblLimits.Text = "Limit breaks (exported as <PACK>_00, _01 ...):";
        }

        private void TxtLimitsFolder_Leave(object sender, EventArgs e) => FillLimits();

        private static string PickFolder(string current, string description)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = description;
                fbd.UseDescriptionForTitle = true;
                if (Directory.Exists(current)) fbd.SelectedPath = current;
                return fbd.ShowDialog() == DialogResult.OK ? fbd.SelectedPath : null;
            }
        }

        private void BtnBrowseLimits_Click(object sender, EventArgs e)
        {
            string f = PickFolder(txtLimitsFolder.Text, "Folder with the limit break (.A00) files (magic.lgp)");
            if (f == null) return;
            txtLimitsFolder.Text = f;
            FillLimits();
        }

        private void BtnBrowseOut_Click(object sender, EventArgs e)
        {
            string f = PickFolder(txtOutFolder.Text, "Output folder for the glTF export");
            if (f != null) txtOutFolder.Text = f;
        }

        private void Chk60fps_CheckedChanged(object sender, EventArgs e)
        {
            cbLoops.Enabled = chk60fps.Checked;
        }

        private void CbFileName_TextChanged(object sender, EventArgs e)
        {
            if (prefixEdited) return;
            settingPrefix = true;
            txtPrefix.Text = cbFileName.Text.Trim();
            settingPrefix = false;
        }

        private void TxtPrefix_TextChanged(object sender, EventArgs e)
        {
            if (!settingPrefix) prefixEdited = true;
        }

        // ---------------------------------------------------------------------------------------
        // Export
        // ---------------------------------------------------------------------------------------
        private void BtnExport_Click(object sender, EventArgs e)
        {
            string outFolder = txtOutFolder.Text.Trim();
            string fileName = cbFileName.Text.Trim();
            if (fileName.EndsWith(".gltf", StringComparison.OrdinalIgnoreCase)) fileName = fileName.Substring(0, fileName.Length - 5);

            if (outFolder == "")
            {
                MessageBox.Show("Choose an output folder first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (fileName == "" || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                MessageBox.Show("Enter a valid file name.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<string> limits = new List<string>();
            for (int i = 0; i < clbLimits.Items.Count; i++) if (clbLimits.GetItemChecked(i)) limits.Add(limitFiles[i]);

            if (checkedIndexes.Count == 0 && limits.Count == 0 &&
                MessageBox.Show("No animations selected. Export anyway?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            string gltfPath = Path.Combine(outFolder, fileName + ".gltf");
            if (File.Exists(gltfPath) &&
                MessageBox.Show(gltfPath + " already exists. Overwrite it (and its .bin and textures)?",
                                Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            FF7BattleGltfExporter.Options opt = new FF7BattleGltfExporter.Options
            {
                OutputFolder = outFolder,
                FileName = fileName,
                Fps = int.Parse((string)cbFps.SelectedItem ?? "30"),
                TexturePrefix = txtPrefix.Text.Trim() == "" ? fileName : txtPrefix.Text.Trim(),
                WriteDDS = chkDDS.Checked,
                BakeVertexColors = chkBake.Checked,
                RestPose = rbRestZero.Checked ? FF7BattleGltfExporter.RestPoseMode.AllZero : FF7BattleGltfExporter.RestPoseMode.CurrentFrame,
                RestFrame = restFrame,
                RestWeaponFrame = restWeaponFrame,
                Weapons = rbWeaponsNone.Checked ? FF7BattleGltfExporter.WeaponsMode.None
                        : rbWeaponsCurrent.Checked ? FF7BattleGltfExporter.WeaponsMode.Current
                        : FF7BattleGltfExporter.WeaponsMode.All,
                CurrentWeapon = currentWeapon,
                AnimationPackFile = checkedIndexes.Count > 0 ? packFile : "",
                AnimationIndexes = checkedIndexes.OrderBy(i => i).ToList(),
                LimitPackFiles = limits,
                DoubleFrameRate = chk60fps.Checked,
                Loops = cbLoops.SelectedIndex == 1 ? GltfRigExporter.LoopMode.All
                      : cbLoops.SelectedIndex == 2 ? GltfRigExporter.LoopMode.None
                      : GltfRigExporter.LoopMode.Auto,
            };

            lastOutFolder = outFolder;
            lastFps = (int)opt.Fps;
            lastDDS = chkDDS.Checked;
            lastBake = chkBake.Checked;
            lastRestZero = rbRestZero.Checked;
            last60fps = chk60fps.Checked;
            lastLoops = Math.Max(0, cbLoops.SelectedIndex);

            Cursor oldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            btnExport.Enabled = false;
            txtReport.Text = "Exporting...";
            txtReport.Refresh();

            FF7BattleGltfExporter.Result res;
            try
            {
                res = FF7BattleGltfExporter.Export(skeleton, isMagic, opt);
            }
            finally
            {
                btnExport.Enabled = true;
                Cursor.Current = oldCursor;
            }

            string report = GltfRigExporter.FormatReport(res);
            txtReport.Text = report;
            try
            {
                File.WriteAllText(Path.Combine(outFolder, fileName + "_export_report.txt"), report);
            }
            catch { }

            if (!res.Success)
                MessageBox.Show("The export finished with errors. See the report.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
