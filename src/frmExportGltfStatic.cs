using System;
using System.IO;
using System.Windows.Forms;

namespace KimeraCS
{
    //
    // Export window for static models (RSD resource, single P / 3DS, TMD, battle scene).
    // The caller passes the export to run; the window only collects the output options.
    //
    public partial class FrmExportGltfStatic : Form
    {
        // Remembered while Kimera is open.
        private static string lastOutFolder = "";
        private static bool lastDDS = true, lastBake = true;

        private readonly Func<FF7StaticGltfExporter.Options, GltfRigExporter.Result> export;
        private bool prefixEdited = false, settingPrefix = false;

        public FrmExportGltfStatic(string description, string defaultFileName,
                                   Func<FF7StaticGltfExporter.Options, GltfRigExporter.Result> exportFunc)
        {
            InitializeComponent();

            export = exportFunc;
            lblModel.Text = "Model: " + description;
            txtFileName.Text = defaultFileName.ToUpperInvariant();
            txtOutFolder.Text = lastOutFolder;
            chkDDS.Checked = lastDDS;
            chkBake.Checked = lastBake;
        }

        private void TxtFileName_TextChanged(object sender, EventArgs e)
        {
            if (prefixEdited) return;
            settingPrefix = true;
            txtPrefix.Text = txtFileName.Text.Trim();
            settingPrefix = false;
        }

        private void TxtPrefix_TextChanged(object sender, EventArgs e)
        {
            if (!settingPrefix) prefixEdited = true;
        }

        private void BtnBrowseOut_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Output folder for the glTF export";
                fbd.UseDescriptionForTitle = true;
                if (Directory.Exists(txtOutFolder.Text)) fbd.SelectedPath = txtOutFolder.Text;
                if (fbd.ShowDialog() == DialogResult.OK) txtOutFolder.Text = fbd.SelectedPath;
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            string outFolder = txtOutFolder.Text.Trim();
            string fileName = txtFileName.Text.Trim();
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

            string gltfPath = Path.Combine(outFolder, fileName + ".gltf");
            if (File.Exists(gltfPath) &&
                MessageBox.Show(gltfPath + " already exists. Overwrite it (and its .bin and textures)?",
                                Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            FF7StaticGltfExporter.Options opt = new FF7StaticGltfExporter.Options
            {
                OutputFolder = outFolder,
                FileName = fileName,
                TexturePrefix = txtPrefix.Text.Trim() == "" ? fileName : txtPrefix.Text.Trim(),
                WriteDDS = chkDDS.Checked,
                BakeVertexColors = chkBake.Checked,
            };

            lastOutFolder = outFolder;
            lastDDS = chkDDS.Checked;
            lastBake = chkBake.Checked;

            Cursor oldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            btnExport.Enabled = false;
            txtReport.Text = "Exporting...";
            txtReport.Refresh();

            GltfRigExporter.Result res;
            try
            {
                res = export(opt);
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
