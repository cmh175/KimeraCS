using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace KimeraCS
{

    using static FF7FieldSkeleton;
    using static FF7FieldAnimation;
    using static FF7FieldRSDResource;

    using static FileTools;

    //
    // Export window for FF7FieldGltfExporter (File > Export glTF for FFNx...).
    //
    public partial class FrmExportGltf : Form
    {
        // Remembered while Kimera is open.
        private static string lastOutFolder = "";
        private static string lastExtra = "";
        private static int lastFps = 0;
        private static bool lastDDS = true, lastBake = true, lastRestZero = false, last60fps = false;
        private static int lastLoops = 1;      // All loop: safe with the 60FPS mod's frame counts

        private readonly FieldSkeleton skeleton;
        private readonly FieldFrame? currentFrame;
        private readonly string modelName;
        private readonly List<string> pNames = new List<string>();
        private readonly List<string> dbAnimNames = new List<string>();
        private readonly HashSet<string> checkedNames = new HashSet<string>();

        private bool prefixEdited = false;
        private bool settingPrefix = false;
        private bool fillingList = false;

        public FrmExportGltf(FieldSkeleton fSkeleton, FieldAnimation fAnimation, int frameIndex,
                             string strModelFullPath, int kimeraFps)
        {
            InitializeComponent();

            skeleton = fSkeleton;
            modelName = Path.GetFileNameWithoutExtension(fSkeleton.fileName ?? "").ToUpperInvariant();

            string modelFolder = string.IsNullOrEmpty(strModelFullPath) ? "" : Path.GetDirectoryName(strModelFullPath);

            // current frame of the viewer (the default rest pose)
            string animName = Path.GetFileNameWithoutExtension(fAnimation.strFieldAnimationFile ?? "").ToUpperInvariant();
            if (fAnimation.frames != null && fAnimation.frames.Count > 0)
            {
                frameIndex = Math.Max(0, Math.Min(frameIndex, fAnimation.frames.Count - 1));
                currentFrame = FF7FieldAnimation.CopyfFrame(fAnimation.frames[frameIndex]);
                rbRestCurrent.Text = "Current frame (" + (animName == "" || animName == "DUMMY" ? "Kimera default pose" : animName) +
                                     ", frame " + frameIndex + ")";
            }
            else
            {
                rbRestCurrent.Enabled = false;
                rbRestZero.Checked = true;
            }

            lblModel.Text = "Model: " + fSkeleton.fileName + "  (" + fSkeleton.name + ", " + fSkeleton.bones.Count + " bones)" +
                            (modelFolder != "" ? "   " + modelFolder : "");

            // .p names of the model: FFNx looks for mesh\field\<p name>.gltf
            foreach (FieldBone bone in fSkeleton.bones)
                for (int ri = 0; ri < bone.nResources; ri++)
                {
                    FieldRSDResource res = bone.fRSDResources[ri];
                    string p = Path.GetFileNameWithoutExtension(res.Model.fileName ?? res.res_file ?? "").ToUpperInvariant();
                    if (p != "" && !pNames.Contains(p)) pNames.Add(p);
                }
            cbFileName.Items.AddRange(pNames.ToArray());
            if (pNames.Count > 0) cbFileName.SelectedIndex = 0;
            else cbFileName.Text = modelName;

            // animations from the Ifalna database
            if (lstCharLGPRegisters != null)
            {
                foreach (STCharLGPRegister reg in lstCharLGPRegisters)
                    if (reg.fileName != null && reg.fileName.ToUpperInvariant() == modelName && reg.lstAnims != null)
                        foreach (string a in reg.lstAnims)
                        {
                            string n = a.Trim().ToUpperInvariant();
                            if (n.EndsWith(".A")) n = n.Substring(0, n.Length - 2);
                            if (n != "" && !dbAnimNames.Contains(n)) dbAnimNames.Add(n);
                        }
            }
            if (dbAnimNames.Count == 0)
                lblDBList.Text = "No Ifalna entry for " + modelName + "; add names on the right.";

            if (animName != "" && animName != "DUMMY") checkedNames.Add(animName);

            txtAnimFolder.Text = modelFolder;
            FillAnimationList();

            // remembered settings
            txtOutFolder.Text = lastOutFolder;
            txtExtra.Text = lastExtra;
            int fps = lastFps > 0 ? lastFps : (kimeraFps >= 60 ? 60 : 30);
            cbFps.SelectedItem = fps.ToString();
            chkDDS.Checked = lastDDS;
            chkBake.Checked = lastBake;
            if (lastRestZero || !rbRestCurrent.Enabled) rbRestZero.Checked = true;
            cbLoops.SelectedIndex = lastLoops;
            chk60fps.Checked = last60fps;
            if (kimeraFps == 15)
                lblFpsHint.Text += " Kimera plays at 15.";
        }

        // ---------------------------------------------------------------------------------------
        // Animation list
        // ---------------------------------------------------------------------------------------
        private string DescribeAnimation(string name)
        {
            string path = Path.Combine(txtAnimFolder.Text, name + ".A");
            try
            {
                if (!File.Exists(path)) return name + "   (not found)";
                using (BinaryReader br = new BinaryReader(File.OpenRead(path)))
                {
                    br.ReadInt32();
                    int frames = br.ReadInt32();
                    int bones = br.ReadInt32();
                    string s = name + "   " + frames + (frames == 1 ? " frame" : " frames");
                    if (!(bones == skeleton.bones.Count || (bones == 0 && skeleton.bones.Count == 1)))
                        s += "  (" + bones + " bones, doesn't fit)";
                    return s;
                }
            }
            catch
            {
                return name + "   (can't read)";
            }
        }

        private void FillAnimationList()
        {
            fillingList = true;
            clbAnimations.BeginUpdate();
            clbAnimations.Items.Clear();
            foreach (string n in dbAnimNames)
                clbAnimations.Items.Add(DescribeAnimation(n), checkedNames.Contains(n));
            clbAnimations.EndUpdate();
            fillingList = false;
            UpdateSelectedCount();
        }

        private void UpdateSelectedCount()
        {
            lblSelectedCount.Text = checkedNames.Count(n => dbAnimNames.Contains(n)) + " of " + dbAnimNames.Count + " selected";
        }

        private void ClbAnimations_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (fillingList) return;
            string n = dbAnimNames[e.Index];
            if (e.NewValue == CheckState.Checked) checkedNames.Add(n);
            else checkedNames.Remove(n);
            UpdateSelectedCount();
        }

        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            foreach (string n in dbAnimNames) checkedNames.Add(n);
            FillAnimationList();
        }

        private void BtnSelectNone_Click(object sender, EventArgs e)
        {
            checkedNames.Clear();
            FillAnimationList();
        }

        private void TxtAnimFolder_Leave(object sender, EventArgs e)
        {
            FillAnimationList();
        }

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

        private void BtnBrowseAnim_Click(object sender, EventArgs e)
        {
            string f = PickFolder(txtAnimFolder.Text, "Folder with the field animation (.a) files");
            if (f == null) return;
            txtAnimFolder.Text = f;
            FillAnimationList();
        }

        private void BtnBrowseOut_Click(object sender, EventArgs e)
        {
            string f = PickFolder(txtOutFolder.Text, "Output folder for the glTF export");
            if (f != null) txtOutFolder.Text = f;
        }

        // 30 -> 60 fps: timestamps are then always 1/60 s, so the frame rate choice doesn't apply.
        private void Chk60fps_CheckedChanged(object sender, EventArgs e)
        {
            cbLoops.Enabled = chk60fps.Checked;
            cbFps.Enabled = !chk60fps.Checked;
        }

        // ---------------------------------------------------------------------------------------
        // File name / texture prefix
        // ---------------------------------------------------------------------------------------
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
        private List<string> CollectAnimationNames()
        {
            List<string> names = new List<string>();
            foreach (string n in dbAnimNames) if (checkedNames.Contains(n)) names.Add(n);

            foreach (string raw in txtExtra.Text.Split(new[] { '\r', '\n', ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string n = raw.Trim().ToUpperInvariant();
                if (n.EndsWith(".A")) n = n.Substring(0, n.Length - 2);
                if (n != "" && !names.Contains(n)) names.Add(n);
            }
            return names;
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            string outFolder = txtOutFolder.Text.Trim();
            string fileName = cbFileName.Text.Trim();
            if (fileName.EndsWith(".gltf", StringComparison.OrdinalIgnoreCase)) fileName = fileName.Substring(0, fileName.Length - 5);

            if (outFolder == "")
            {
                MessageBox.Show("Choose an output folder first.", "Export glTF for FFNx", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (fileName == "" || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                MessageBox.Show("Enter a valid file name (one of the model's .p names).", "Export glTF for FFNx",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (pNames.Count > 0 && !pNames.Contains(fileName.ToUpperInvariant()) &&
                MessageBox.Show(fileName + " isn't one of this model's .p files (" + string.Join(", ", pNames) + ").\n" +
                                "FFNx only loads a gltf named after a .p file the game loads. Export anyway?",
                                "Export glTF for FFNx", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            List<string> anims = CollectAnimationNames();
            if (anims.Count == 0 &&
                MessageBox.Show("No animations selected. In FFNx the model will be drawn in its rest pose.\nExport anyway?",
                                "Export glTF for FFNx", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            string gltfPath = Path.Combine(outFolder, fileName + ".gltf");
            if (File.Exists(gltfPath) &&
                MessageBox.Show(gltfPath + " already exists. Overwrite it (and its .bin and textures)?",
                                "Export glTF for FFNx", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            FF7FieldGltfExporter.Options opt = new FF7FieldGltfExporter.Options
            {
                OutputFolder = outFolder,
                FileName = fileName,
                Fps = chk60fps.Checked ? 30 : int.Parse((string)cbFps.SelectedItem ?? "30"),    // 30 -> 60 when converting
                TexturePrefix = txtPrefix.Text.Trim() == "" ? fileName : txtPrefix.Text.Trim(),
                WriteDDS = chkDDS.Checked,
                BakeVertexColors = chkBake.Checked,
                RestPose = rbRestZero.Checked ? FF7FieldGltfExporter.RestPoseMode.AllZero : FF7FieldGltfExporter.RestPoseMode.CurrentFrame,
                RestFrame = currentFrame,
                AnimationFolder = txtAnimFolder.Text.Trim(),
                AnimationNames = anims,
                To60Fps = chk60fps.Checked,
                Loops = cbLoops.SelectedIndex == 1 ? GltfRigExporter.LoopMode.All
                      : cbLoops.SelectedIndex == 2 ? GltfRigExporter.LoopMode.None
                      : GltfRigExporter.LoopMode.Auto,
            };

            // remember for next time
            lastOutFolder = outFolder;
            lastExtra = txtExtra.Text;
            lastFps = opt.Fps == 60 ? 60 : 30;
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

            FF7FieldGltfExporter.Result res;
            try
            {
                res = FF7FieldGltfExporter.Export(skeleton, opt);
            }
            finally
            {
                btnExport.Enabled = true;
                Cursor.Current = oldCursor;
            }

            string report = FF7FieldGltfExporter.FormatReport(res);
            txtReport.Text = report;
            try
            {
                File.WriteAllText(Path.Combine(outFolder, fileName + "_export_report.txt"), report);
            }
            catch { }

            if (!res.Success)
                MessageBox.Show("The export finished with errors. See the report.", "Export glTF for FFNx",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
