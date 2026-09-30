using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KimeraCS
{
    //
    // Batch glTF export (Tools > Batch glTF Export...). Every model is exported by a separate
    // KimeraCS.exe process using the command-line export (Core\GltfCommandLine.cs), so the model open
    // in the editor is untouched and a model that fails can't stop the batch.
    //
    //   field / world / mini-game (.HRC): every compatible animation, file named after the first .p
    //   battle (??AA): all animations, all weapons, limit breaks; battle scenes: static export
    //   magic / summon (.D): all animations;  .RSD / .TMD / .P: static export
    //
    // Output is sorted into type folders like FFNx's mesh folder: field\, world\, battle\, magic\,
    // minigame\ (each with its own textures\), reports in _reports\. FFNx 1.24.0 only reads
    // mesh\field\ (for every .p file it loads); the other names follow the game's .lgp files.
    //
    public partial class FrmBatchExportGltf : Form
    {
        // Remembered while Kimera is open.
        private static string lastSource = "", lastOut = "", lastModels = "";
        private static bool lastPerModel = false, lastRestZero = false, lastDDS = true, lastBake = true, last60fps = false;
        private static int lastLoops = 0;

        private const int TIMEOUT_MINUTES = 10;

        private enum Kind { Field, Battle, Magic, Static }

        private class Job
        {
            public string Entry;          // as typed
            public string Name;           // display / output name
            public string File;           // full path
            public Kind Kind;
            public string KindText;
            public string Category;       // output type folder: field, world, battle, magic, minigame
            public string Error;          // could not resolve
            public ListViewItem Row;
            public string Report;         // report file
        }

        private CancellationTokenSource cancel;
        private readonly List<Process> running = new List<Process>();
        private bool busy = false;

        public FrmBatchExportGltf(string defaultSourceFolder)
        {
            InitializeComponent();

            txtSource.Text = lastSource != "" ? lastSource : (defaultSourceFolder ?? "");
            txtOut.Text = lastOut;
            txtModels.Text = lastModels;
            rbPerModel.Checked = lastPerModel;
            rbRestZero.Checked = lastRestZero;
            chkDDS.Checked = lastDDS;
            chkBake.Checked = lastBake;
            cbLoops.SelectedIndex = lastLoops;
            chk60fps.Checked = last60fps;
            UpdateCounts();
        }

        // ---------------------------------------------------------------------------------------
        // Model list
        // ---------------------------------------------------------------------------------------
        private List<string> Entries()
        {
            return txtModels.Text.Split(new[] { '\r', '\n', ',', ';', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(s => s.Trim()).Where(s => s != "")
                                 .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private void UpdateCounts()
        {
            int n = Entries().Count;
            lblCounts.Text = n + (n == 1 ? " model" : " models");
        }

        private void TxtModels_TextChanged(object sender, EventArgs e) => UpdateCounts();

        private static bool IsBattleSkeletonName(string fileName) => Regex.IsMatch(fileName, "^..aa$", RegexOptions.IgnoreCase);

        private void BtnAddAll_Click(object sender, EventArgs e)
        {
            string dir = txtSource.Text.Trim();
            if (!Directory.Exists(dir))
            {
                MessageBox.Show("Choose an existing source folder first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<string> found = new List<string>();
            foreach (string f in Directory.GetFiles(dir).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                string name = Path.GetFileName(f), ext = Path.GetExtension(f).ToLowerInvariant();
                if (ext == ".hrc") found.Add(Path.GetFileNameWithoutExtension(f).ToUpperInvariant());
                else if (ext == "" && IsBattleSkeletonName(name)) found.Add(name.ToUpperInvariant());
                else if (ext == ".d") found.Add(name.ToLowerInvariant());
            }

            List<string> all = Entries();
            foreach (string f in found) if (!all.Contains(f, StringComparer.OrdinalIgnoreCase)) all.Add(f);
            txtModels.Text = string.Join(Environment.NewLine, all);

            if (found.Count == 0)
                MessageBox.Show("No field (.hrc), battle (??aa) or summon (.d) skeleton files in that folder.", Text,
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnClearList_Click(object sender, EventArgs e) => txtModels.Text = "";

        private Job Resolve(string entry, string dir)
        {
            Job j = new Job { Entry = entry };
            string ext = Path.GetExtension(entry).ToLowerInvariant();
            string p;

            string Find(string fileName)
            {
                string path = Path.Combine(dir, fileName);
                return File.Exists(path) ? path : null;
            }

            if (ext == ".hrc" && (p = Find(entry)) != null) { j.Kind = Kind.Field; j.File = p; }
            else if (ext == ".d" && (p = Find(entry)) != null) { j.Kind = Kind.Magic; j.File = p; }
            else if ((ext == ".rsd" || ext == ".tmd" || ext == ".p") && (p = Find(entry)) != null) { j.Kind = Kind.Static; j.File = p; }
            else if ((p = Find(entry + ".hrc")) != null) { j.Kind = Kind.Field; j.File = p; }
            else if (IsBattleSkeletonName(entry) && (p = Find(entry)) != null) { j.Kind = Kind.Battle; j.File = p; }
            else if ((p = Find(entry + ".d")) != null) { j.Kind = Kind.Magic; j.File = p; }
            else
            {
                j.Error = "not found (no " + entry + ".hrc, battle skeleton or .d file in the source folder)";
                j.Name = entry.ToUpperInvariant();
                j.KindText = "?";
                return j;
            }

            j.Name = Path.GetFileNameWithoutExtension(j.File).ToUpperInvariant();
            j.KindText = j.Kind == Kind.Field ? "field" : j.Kind == Kind.Magic ? "summon/magic" : j.Kind == Kind.Static ? "static" : "battle";
            j.Category = CategoryOf(j.Kind, dir);

            // battle scenes are battle skeletons with no bones -> static export
            if (j.Kind == Kind.Battle)
            {
                try
                {
                    using (BinaryReader br = new BinaryReader(File.OpenRead(j.File)))
                    {
                        br.BaseStream.Position = 12;
                        if (br.ReadInt32() == 0) { j.Kind = Kind.Static; j.KindText = "battle scene"; j.Category = "battle"; }
                    }
                }
                catch { }
            }
            return j;
        }

        // Output type folder, from the kind of file and the source folder's name (an extracted .lgp:
        // char, world_us, battle, magic, chocobo, high-us, condor, snowboard-us ...).
        private static readonly string[] MINIGAME_FOLDERS = { "chocobo", "high", "condor", "snowboard", "coaster", "sub", "minigame" };

        private static string CategoryOf(Kind kind, string sourceDir)
        {
            string folder = Path.GetFileName(sourceDir.TrimEnd('\\', '/')).ToLowerInvariant();
            bool minigame = MINIGAME_FOLDERS.Any(m => folder.StartsWith(m));

            switch (kind)
            {
                case Kind.Magic: return "magic";
                case Kind.Battle: return folder.Contains("summon") ? "magic" : "battle";
                default:
                    if (folder.Contains("world")) return "world";
                    if (minigame) return "minigame";
                    if (folder.Contains("battle")) return "battle";
                    if (folder.Contains("magic") || folder.Contains("summon")) return "magic";
                    return "field";
            }
        }

        // ---------------------------------------------------------------------------------------
        // Folders
        // ---------------------------------------------------------------------------------------
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

        private void BtnBrowseSource_Click(object sender, EventArgs e)
        {
            string f = PickFolder(txtSource.Text, "Folder with the model files (an extracted .lgp, e.g. char or battle)");
            if (f != null) txtSource.Text = f;
        }

        private void BtnBrowseOut_Click(object sender, EventArgs e)
        {
            string f = PickFolder(txtOut.Text, "Output folder for the glTF files");
            if (f != null) txtOut.Text = f;
        }

        private void BtnOpenOut_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(txtOut.Text.Trim()))
                Process.Start(new ProcessStartInfo(txtOut.Text.Trim()) { UseShellExecute = true });
        }

        private void Chk60fps_CheckedChanged(object sender, EventArgs e) => cbLoops.Enabled = chk60fps.Checked;

        private void LvResults_DoubleClick(object sender, EventArgs e)
        {
            if (lvResults.SelectedItems.Count == 0) return;
            if (lvResults.SelectedItems[0].Tag is string report && File.Exists(report))
                Process.Start(new ProcessStartInfo(report) { UseShellExecute = true });
        }

        // ---------------------------------------------------------------------------------------
        // Run
        // ---------------------------------------------------------------------------------------
        private void SetBusy(bool on)
        {
            busy = on;
            btnStart.Enabled = !on;
            btnStop.Enabled = on;
            gbModels.Enabled = gbOutput.Enabled = gbOptions.Enabled = !on;
        }

        private List<string> Arguments(Job j, string outRoot)
        {
            // <out>\<type>\ (like FFNx's mesh folder), or <out>\<type>\<model>\ with a folder per model
            string typeDir = Path.Combine(outRoot, j.Category);
            string outDir = rbPerModel.Checked ? Path.Combine(typeDir, j.Name) : typeDir;
            j.Report = Path.Combine(outRoot, "_reports", j.Category, j.Name + "_export_report.txt");

            List<string> a = new List<string>();
            switch (j.Kind)
            {
                case Kind.Field:
                    a.AddRange(new[] { GltfCommandLine.SWITCH, j.File, "--out", outDir, "--name", "p", "--anims", "all" });
                    break;
                case Kind.Battle:
                case Kind.Magic:
                    a.AddRange(new[] { GltfCommandLine.SWITCH_BATTLE, j.File, "--out", outDir });
                    break;
                default:
                    a.AddRange(new[] { GltfCommandLine.SWITCH_STATIC, j.File, "--out", outDir });
                    break;
            }

            if (j.Kind != Kind.Static)
            {
                if (rbRestZero.Checked) a.AddRange(new[] { "--rest", "zero" });
                if (chk60fps.Checked)
                    a.AddRange(new[] { "--60fps", "--loops", cbLoops.SelectedIndex == 1 ? "all" : cbLoops.SelectedIndex == 2 ? "none" : "auto" });
            }
            if (!chkDDS.Checked) a.Add("--no-dds");
            if (!chkBake.Checked) a.Add("--no-bake");
            a.AddRange(new[] { "--report", j.Report });
            return a;
        }

        private static void SetRow(Job j, string result, string details, Color color)
        {
            j.Row.SubItems[2].Text = result;
            j.Row.SubItems[3].Text = details;
            j.Row.ForeColor = color;
            j.Row.Tag = j.Report;
        }

        private static string Summarize(string report, out int warnings, out int errors)
        {
            warnings = errors = 0;
            if (report == null || !File.Exists(report)) return "no report";
            string[] lines = File.ReadAllLines(report);
            if (lines.Length > 0 && lines[0].StartsWith("Export failed")) { errors = 1; return lines[0]; }

            string section = "";
            int anims = 0;
            string file = null;
            foreach (string l in lines)
            {
                if (l == "Warnings:" || l == "Errors:" || l == "Files written:") { section = l; continue; }
                if (l.Trim() == "") continue;
                if (section == "Warnings:") warnings++;
                else if (section == "Errors:") errors++;
                else if (section == "Files written:" && file == null) file = Path.GetFileName(l.Trim());
                else if (section == "" && Regex.IsMatch(l, @"^  \S+: \d+ frame\(s\)")) anims++;
            }
            return (file ?? "") + ", " + anims + (anims == 1 ? " animation" : " animations") +
                   (warnings > 0 ? ", " + warnings + " warning(s)" : "") + (errors > 0 ? ", " + errors + " error(s)" : "");
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            string dir = txtSource.Text.Trim(), outRoot = txtOut.Text.Trim();
            List<string> entries = Entries();

            if (!Directory.Exists(dir)) { MessageBox.Show("Choose an existing source folder.", Text); return; }
            if (outRoot == "") { MessageBox.Show("Choose an output folder.", Text); return; }
            if (entries.Count == 0) { MessageBox.Show("Add at least one model to the list.", Text); return; }

            lastSource = dir; lastOut = outRoot; lastModels = txtModels.Text;
            lastPerModel = rbPerModel.Checked; lastRestZero = rbRestZero.Checked;
            lastDDS = chkDDS.Checked; lastBake = chkBake.Checked; last60fps = chk60fps.Checked;
            lastLoops = Math.Max(0, cbLoops.SelectedIndex);

            Directory.CreateDirectory(outRoot);

            List<Job> jobs = entries.Select(en => Resolve(en, dir)).ToList();
            lvResults.Items.Clear();
            foreach (Job j in jobs)
            {
                j.Row = new ListViewItem(new[] { j.Name, j.Error == null ? j.KindText + " -> " + j.Category : j.KindText,
                                                 j.Error == null ? "waiting" : "skipped", j.Error ?? "" });
                if (j.Error != null) j.Row.ForeColor = Color.Firebrick;
                lvResults.Items.Add(j.Row);
            }

            Queue<Job> queue = new Queue<Job>(jobs.Where(j => j.Error == null));
            int total = queue.Count, done = 0, ok = 0, withWarnings = 0, failed = jobs.Count(j => j.Error != null);
            progress.Maximum = Math.Max(1, total);
            progress.Value = 0;

            cancel = new CancellationTokenSource();
            SetBusy(true);
            string exe = Application.ExecutablePath;

            async Task Worker()
            {
                while (queue.Count > 0 && !cancel.IsCancellationRequested)
                {
                    Job j = queue.Dequeue();
                    SetRow(j, "exporting...", "", SystemColors.WindowText);
                    j.Row.EnsureVisible();

                    ProcessStartInfo psi = new ProcessStartInfo(exe) { UseShellExecute = false, CreateNoWindow = true };
                    foreach (string a in Arguments(j, outRoot)) psi.ArgumentList.Add(a);
                    try { if (File.Exists(j.Report)) File.Delete(j.Report); } catch { }

                    int exit = -1;
                    string timedOut = null;
                    using (Process p = Process.Start(psi))
                    {
                        running.Add(p);
                        using (CancellationTokenSource limit = CancellationTokenSource.CreateLinkedTokenSource(cancel.Token))
                        {
                            limit.CancelAfter(TimeSpan.FromMinutes(TIMEOUT_MINUTES));
                            try { await p.WaitForExitAsync(limit.Token); exit = p.ExitCode; }
                            catch (OperationCanceledException)
                            {
                                try { p.Kill(true); } catch { }
                                timedOut = cancel.IsCancellationRequested ? "stopped" : "timed out after " + TIMEOUT_MINUTES + " minutes";
                            }
                        }
                        running.Remove(p);
                    }

                    string details = Summarize(j.Report, out int warnings, out int errors);
                    if (timedOut != null) { SetRow(j, timedOut, "", Color.Firebrick); failed++; }
                    else if (exit != 0 || errors > 0) { SetRow(j, "failed", details, Color.Firebrick); failed++; }
                    else if (warnings > 0) { SetRow(j, "done (warnings)", details, Color.DarkGoldenrod); withWarnings++; }
                    else { SetRow(j, "done", details, Color.DarkGreen); ok++; }

                    done++;
                    progress.Value = Math.Min(progress.Maximum, done);
                    lblSummary.Text = done + " of " + total + " exported";
                }
            }

            int workers = Math.Max(1, Math.Min(4, Environment.ProcessorCount / 2));
            await Task.WhenAll(Enumerable.Range(0, workers).Select(_ => Worker()));

            foreach (Job j in queue) SetRow(j, "not run", "", SystemColors.GrayText);
            lblSummary.Text = (cancel.IsCancellationRequested ? "Stopped. " : "Finished. ") +
                              ok + " done, " + withWarnings + " with warnings, " + failed + " failed or skipped.";
            SetBusy(false);
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            cancel?.Cancel();
            btnStop.Enabled = false;
        }

        private void BtnClose_Click(object sender, EventArgs e) => Close();

        private void FrmBatchExportGltf_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!busy) return;
            if (MessageBox.Show("A batch export is running. Stop it and close?", Text, MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }
            cancel?.Cancel();
            foreach (Process p in running.ToList()) { try { p.Kill(true); } catch { } }
        }
    }
}
