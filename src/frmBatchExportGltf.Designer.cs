namespace KimeraCS
{
    partial class FrmBatchExportGltf
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblIntro = new System.Windows.Forms.Label();
            this.gbModels = new System.Windows.Forms.GroupBox();
            this.lblCounts = new System.Windows.Forms.Label();
            this.btnClearList = new System.Windows.Forms.Button();
            this.btnAddAll = new System.Windows.Forms.Button();
            this.txtModels = new System.Windows.Forms.TextBox();
            this.lblModels = new System.Windows.Forms.Label();
            this.btnBrowseSource = new System.Windows.Forms.Button();
            this.txtSource = new System.Windows.Forms.TextBox();
            this.lblSource = new System.Windows.Forms.Label();
            this.gbOutput = new System.Windows.Forms.GroupBox();
            this.rbPerModel = new System.Windows.Forms.RadioButton();
            this.rbOneFolder = new System.Windows.Forms.RadioButton();
            this.btnBrowseOut = new System.Windows.Forms.Button();
            this.txtOut = new System.Windows.Forms.TextBox();
            this.lblOut = new System.Windows.Forms.Label();
            this.gbOptions = new System.Windows.Forms.GroupBox();
            this.cbLoops = new System.Windows.Forms.ComboBox();
            this.lblLimits = new System.Windows.Forms.Label();
            this.txtLimits = new System.Windows.Forms.TextBox();
            this.btnBrowseLimits = new System.Windows.Forms.Button();
            this.lblLimitsHint = new System.Windows.Forms.Label();
            this.lblLoops = new System.Windows.Forms.Label();
            this.chk60fps = new System.Windows.Forms.CheckBox();
            this.chkBake = new System.Windows.Forms.CheckBox();
            this.chkDDS = new System.Windows.Forms.CheckBox();
            this.pnlRest = new System.Windows.Forms.Panel();
            this.rbRestZero = new System.Windows.Forms.RadioButton();
            this.rbRestDefault = new System.Windows.Forms.RadioButton();
            this.lblRest = new System.Windows.Forms.Label();
            this.lvResults = new System.Windows.Forms.ListView();
            this.colModel = new System.Windows.Forms.ColumnHeader();
            this.colType = new System.Windows.Forms.ColumnHeader();
            this.colResult = new System.Windows.Forms.ColumnHeader();
            this.colDetails = new System.Windows.Forms.ColumnHeader();
            this.progress = new System.Windows.Forms.ProgressBar();
            this.lblSummary = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnOpenOut = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.gbModels.SuspendLayout();
            this.gbOutput.SuspendLayout();
            this.gbOptions.SuspendLayout();
            this.pnlRest.SuspendLayout();
            this.SuspendLayout();
            //
            // lblIntro
            //
            this.lblIntro.AutoSize = true;
            this.lblIntro.Location = new System.Drawing.Point(12, 9);
            this.lblIntro.Name = "lblIntro";
            this.lblIntro.Size = new System.Drawing.Size(600, 15);
            this.lblIntro.TabIndex = 0;
            this.lblIntro.Text = "Exports many models at once, each in its own background run of Kimera (the model open in the editor isn't affected).";
            //
            // gbModels
            //
            this.gbModels.Controls.Add(this.lblCounts);
            this.gbModels.Controls.Add(this.btnClearList);
            this.gbModels.Controls.Add(this.btnAddAll);
            this.gbModels.Controls.Add(this.txtModels);
            this.gbModels.Controls.Add(this.lblModels);
            this.gbModels.Controls.Add(this.btnBrowseSource);
            this.gbModels.Controls.Add(this.txtSource);
            this.gbModels.Controls.Add(this.lblSource);
            this.gbModels.Location = new System.Drawing.Point(12, 32);
            this.gbModels.Name = "gbModels";
            this.gbModels.Size = new System.Drawing.Size(736, 196);
            this.gbModels.TabIndex = 1;
            this.gbModels.TabStop = false;
            this.gbModels.Text = "Models";
            //
            // lblSource
            //
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(10, 24);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(84, 15);
            this.lblSource.TabIndex = 0;
            this.lblSource.Text = "Source folder:";
            //
            // txtSource
            //
            this.txtSource.Location = new System.Drawing.Point(120, 21);
            this.txtSource.Name = "txtSource";
            this.txtSource.Size = new System.Drawing.Size(525, 23);
            this.txtSource.TabIndex = 1;
            //
            // btnBrowseSource
            //
            this.btnBrowseSource.Location = new System.Drawing.Point(651, 20);
            this.btnBrowseSource.Name = "btnBrowseSource";
            this.btnBrowseSource.Size = new System.Drawing.Size(75, 25);
            this.btnBrowseSource.TabIndex = 2;
            this.btnBrowseSource.Text = "Browse...";
            this.btnBrowseSource.UseVisualStyleBackColor = true;
            this.btnBrowseSource.Click += new System.EventHandler(this.BtnBrowseSource_Click);
            //
            // lblModels
            //
            this.lblModels.AutoSize = true;
            this.lblModels.Location = new System.Drawing.Point(10, 54);
            this.lblModels.Name = "lblModels";
            this.lblModels.Size = new System.Drawing.Size(400, 15);
            this.lblModels.TabIndex = 3;
            this.lblModels.Text = "Models, one per line (field AAAA, battle RTAA, summon bahamdat.d ...):";
            //
            // txtModels
            //
            this.txtModels.AcceptsReturn = true;
            this.txtModels.Location = new System.Drawing.Point(10, 72);
            this.txtModels.Multiline = true;
            this.txtModels.Name = "txtModels";
            this.txtModels.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtModels.Size = new System.Drawing.Size(400, 112);
            this.txtModels.TabIndex = 4;
            this.txtModels.TextChanged += new System.EventHandler(this.TxtModels_TextChanged);
            //
            // btnAddAll
            //
            this.btnAddAll.Location = new System.Drawing.Point(420, 72);
            this.btnAddAll.Name = "btnAddAll";
            this.btnAddAll.Size = new System.Drawing.Size(150, 25);
            this.btnAddAll.TabIndex = 5;
            this.btnAddAll.Text = "Add all in folder";
            this.btnAddAll.UseVisualStyleBackColor = true;
            this.btnAddAll.Click += new System.EventHandler(this.BtnAddAll_Click);
            //
            // btnClearList
            //
            this.btnClearList.Location = new System.Drawing.Point(420, 103);
            this.btnClearList.Name = "btnClearList";
            this.btnClearList.Size = new System.Drawing.Size(150, 25);
            this.btnClearList.TabIndex = 6;
            this.btnClearList.Text = "Clear list";
            this.btnClearList.UseVisualStyleBackColor = true;
            this.btnClearList.Click += new System.EventHandler(this.BtnClearList_Click);
            //
            // lblCounts
            //
            this.lblCounts.AutoSize = true;
            this.lblCounts.Location = new System.Drawing.Point(420, 138);
            this.lblCounts.Name = "lblCounts";
            this.lblCounts.Size = new System.Drawing.Size(60, 15);
            this.lblCounts.TabIndex = 7;
            this.lblCounts.Text = "0 models";
            //
            // gbOutput
            //
            this.gbOutput.Controls.Add(this.rbPerModel);
            this.gbOutput.Controls.Add(this.rbOneFolder);
            this.gbOutput.Controls.Add(this.btnBrowseOut);
            this.gbOutput.Controls.Add(this.txtOut);
            this.gbOutput.Controls.Add(this.lblOut);
            this.gbOutput.Location = new System.Drawing.Point(12, 234);
            this.gbOutput.Name = "gbOutput";
            this.gbOutput.Size = new System.Drawing.Size(736, 82);
            this.gbOutput.TabIndex = 2;
            this.gbOutput.TabStop = false;
            this.gbOutput.Text = "Output";
            //
            // lblOut
            //
            this.lblOut.AutoSize = true;
            this.lblOut.Location = new System.Drawing.Point(10, 24);
            this.lblOut.Name = "lblOut";
            this.lblOut.Size = new System.Drawing.Size(84, 15);
            this.lblOut.TabIndex = 0;
            this.lblOut.Text = "Output folder:";
            //
            // txtOut
            //
            this.txtOut.Location = new System.Drawing.Point(120, 21);
            this.txtOut.Name = "txtOut";
            this.txtOut.Size = new System.Drawing.Size(525, 23);
            this.txtOut.TabIndex = 1;
            //
            // btnBrowseOut
            //
            this.btnBrowseOut.Location = new System.Drawing.Point(651, 20);
            this.btnBrowseOut.Name = "btnBrowseOut";
            this.btnBrowseOut.Size = new System.Drawing.Size(75, 25);
            this.btnBrowseOut.TabIndex = 2;
            this.btnBrowseOut.Text = "Browse...";
            this.btnBrowseOut.UseVisualStyleBackColor = true;
            this.btnBrowseOut.Click += new System.EventHandler(this.BtnBrowseOut_Click);
            //
            // rbOneFolder
            //
            this.rbOneFolder.AutoSize = true;
            this.rbOneFolder.Checked = true;
            this.rbOneFolder.Location = new System.Drawing.Point(120, 52);
            this.rbOneFolder.Name = "rbOneFolder";
            this.rbOneFolder.Size = new System.Drawing.Size(420, 19);
            this.rbOneFolder.TabIndex = 3;
            this.rbOneFolder.TabStop = true;
            this.rbOneFolder.Text = "Sorted by type: field, world, battle, magic, minigame (like FFNx\'s mesh folder)";
            this.rbOneFolder.UseVisualStyleBackColor = true;
            //
            // rbPerModel
            //
            this.rbPerModel.AutoSize = true;
            this.rbPerModel.Location = new System.Drawing.Point(560, 52);
            this.rbPerModel.Name = "rbPerModel";
            this.rbPerModel.Size = new System.Drawing.Size(130, 19);
            this.rbPerModel.TabIndex = 4;
            this.rbPerModel.Text = "+ a folder per model";
            this.rbPerModel.UseVisualStyleBackColor = true;
            //
            // gbOptions
            //
            this.gbOptions.Controls.Add(this.lblLimitsHint);
            this.gbOptions.Controls.Add(this.btnBrowseLimits);
            this.gbOptions.Controls.Add(this.txtLimits);
            this.gbOptions.Controls.Add(this.lblLimits);
            this.gbOptions.Controls.Add(this.cbLoops);
            this.gbOptions.Controls.Add(this.lblLoops);
            this.gbOptions.Controls.Add(this.chk60fps);
            this.gbOptions.Controls.Add(this.chkBake);
            this.gbOptions.Controls.Add(this.chkDDS);
            this.gbOptions.Controls.Add(this.pnlRest);
            this.gbOptions.Controls.Add(this.lblRest);
            this.gbOptions.Location = new System.Drawing.Point(12, 322);
            this.gbOptions.Name = "gbOptions";
            this.gbOptions.Size = new System.Drawing.Size(736, 162);
            this.gbOptions.TabIndex = 3;
            this.gbOptions.TabStop = false;
            this.gbOptions.Text = "Options (all compatible animations; battle models with all weapons and limit breaks)";
            //
            // lblRest
            //
            this.lblRest.AutoSize = true;
            this.lblRest.Location = new System.Drawing.Point(10, 25);
            this.lblRest.Name = "lblRest";
            this.lblRest.Size = new System.Drawing.Size(62, 15);
            this.lblRest.TabIndex = 0;
            this.lblRest.Text = "Rest pose:";
            //
            // pnlRest
            //
            this.pnlRest.Controls.Add(this.rbRestZero);
            this.pnlRest.Controls.Add(this.rbRestDefault);
            this.pnlRest.Location = new System.Drawing.Point(116, 20);
            this.pnlRest.Name = "pnlRest";
            this.pnlRest.Size = new System.Drawing.Size(610, 26);
            this.pnlRest.TabIndex = 1;
            //
            // rbRestDefault
            //
            this.rbRestDefault.AutoSize = true;
            this.rbRestDefault.Checked = true;
            this.rbRestDefault.Location = new System.Drawing.Point(4, 3);
            this.rbRestDefault.Name = "rbRestDefault";
            this.rbRestDefault.Size = new System.Drawing.Size(270, 19);
            this.rbRestDefault.TabIndex = 0;
            this.rbRestDefault.TabStop = true;
            this.rbRestDefault.Text = "First frame of the default (idle) animation";
            this.rbRestDefault.UseVisualStyleBackColor = true;
            //
            // rbRestZero
            //
            this.rbRestZero.AutoSize = true;
            this.rbRestZero.Location = new System.Drawing.Point(334, 3);
            this.rbRestZero.Name = "rbRestZero";
            this.rbRestZero.Size = new System.Drawing.Size(70, 19);
            this.rbRestZero.TabIndex = 1;
            this.rbRestZero.Text = "All zero";
            this.rbRestZero.UseVisualStyleBackColor = true;
            //
            // chkDDS
            //
            this.chkDDS.AutoSize = true;
            this.chkDDS.Checked = true;
            this.chkDDS.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDDS.Location = new System.Drawing.Point(120, 53);
            this.chkDDS.Name = "chkDDS";
            this.chkDDS.Size = new System.Drawing.Size(240, 19);
            this.chkDDS.TabIndex = 2;
            this.chkDDS.Text = "Write DDS textures (FFNx needs them)";
            this.chkDDS.UseVisualStyleBackColor = true;
            //
            // chkBake
            //
            this.chkBake.AutoSize = true;
            this.chkBake.Checked = true;
            this.chkBake.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBake.Location = new System.Drawing.Point(450, 53);
            this.chkBake.Name = "chkBake";
            this.chkBake.Size = new System.Drawing.Size(260, 19);
            this.chkBake.TabIndex = 3;
            this.chkBake.Text = "Bake vertex colours of untextured parts";
            this.chkBake.UseVisualStyleBackColor = true;
            //
            // chk60fps
            //
            this.chk60fps.AutoSize = true;
            this.chk60fps.Location = new System.Drawing.Point(120, 81);
            this.chk60fps.Name = "chk60fps";
            this.chk60fps.Size = new System.Drawing.Size(300, 19);
            this.chk60fps.TabIndex = 4;
            this.chk60fps.Text = "Convert to 60 fps (field 30 -> 60, battle 15 -> 60)";
            this.chk60fps.UseVisualStyleBackColor = true;
            this.chk60fps.CheckedChanged += new System.EventHandler(this.Chk60fps_CheckedChanged);
            //
            // lblLoops
            //
            this.lblLoops.AutoSize = true;
            this.lblLoops.Location = new System.Drawing.Point(450, 82);
            this.lblLoops.Name = "lblLoops";
            this.lblLoops.Size = new System.Drawing.Size(41, 15);
            this.lblLoops.TabIndex = 5;
            this.lblLoops.Text = "Loops:";
            //
            // cbLoops
            //
            this.cbLoops.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbLoops.Enabled = false;
            this.cbLoops.FormattingEnabled = true;
            this.cbLoops.Items.AddRange(new object[] {
            "Auto (60FPS mod)",
            "All loop",
            "No loops"});
            this.cbLoops.Location = new System.Drawing.Point(497, 78);
            this.cbLoops.Name = "cbLoops";
            this.cbLoops.Size = new System.Drawing.Size(150, 23);
            this.cbLoops.TabIndex = 6;
            //
            // lblLimits
            //
            this.lblLimits.AutoSize = true;
            this.lblLimits.Location = new System.Drawing.Point(10, 113);
            this.lblLimits.Name = "lblLimits";
            this.lblLimits.Size = new System.Drawing.Size(76, 15);
            this.lblLimits.TabIndex = 7;
            this.lblLimits.Text = "Magic anims:";
            //
            // txtLimits
            //
            this.txtLimits.Location = new System.Drawing.Point(120, 110);
            this.txtLimits.Name = "txtLimits";
            this.txtLimits.Size = new System.Drawing.Size(525, 23);
            this.txtLimits.TabIndex = 8;
            //
            // btnBrowseLimits
            //
            this.btnBrowseLimits.Location = new System.Drawing.Point(651, 109);
            this.btnBrowseLimits.Name = "btnBrowseLimits";
            this.btnBrowseLimits.Size = new System.Drawing.Size(75, 25);
            this.btnBrowseLimits.TabIndex = 9;
            this.btnBrowseLimits.Text = "Browse...";
            this.btnBrowseLimits.UseVisualStyleBackColor = true;
            this.btnBrowseLimits.Click += new System.EventHandler(this.BtnBrowseLimits_Click);
            //
            // lblLimitsHint
            //
            this.lblLimitsHint.AutoSize = true;
            this.lblLimitsHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblLimitsHint.Location = new System.Drawing.Point(120, 137);
            this.lblLimitsHint.Name = "lblLimitsHint";
            this.lblLimitsHint.Size = new System.Drawing.Size(560, 15);
            this.lblLimitsHint.TabIndex = 10;
            this.lblLimitsHint.Text = "Limit break and summon animations (.A00, extracted magic.lgp). Empty: next to the models.";
            //
            // lvResults
            //
            this.lvResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colModel,
            this.colType,
            this.colResult,
            this.colDetails});
            this.lvResults.FullRowSelect = true;
            this.lvResults.HideSelection = false;
            this.lvResults.Location = new System.Drawing.Point(12, 490);
            this.lvResults.Name = "lvResults";
            this.lvResults.Size = new System.Drawing.Size(736, 220);
            this.lvResults.TabIndex = 4;
            this.lvResults.UseCompatibleStateImageBehavior = false;
            this.lvResults.View = System.Windows.Forms.View.Details;
            this.lvResults.DoubleClick += new System.EventHandler(this.LvResults_DoubleClick);
            //
            // colModel
            //
            this.colModel.Text = "Model";
            this.colModel.Width = 100;
            //
            // colType
            //
            this.colType.Text = "Type";
            this.colType.Width = 150;
            //
            // colResult
            //
            this.colResult.Text = "Result";
            this.colResult.Width = 110;
            //
            // colDetails
            //
            this.colDetails.Text = "Details (double-click a row for its report)";
            this.colDetails.Width = 350;
            //
            // progress
            //
            this.progress.Location = new System.Drawing.Point(12, 716);
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(736, 14);
            this.progress.TabIndex = 5;
            //
            // lblSummary
            //
            this.lblSummary.AutoSize = true;
            this.lblSummary.Location = new System.Drawing.Point(12, 743);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Size = new System.Drawing.Size(0, 15);
            this.lblSummary.TabIndex = 6;
            //
            // btnStart
            //
            this.btnStart.Location = new System.Drawing.Point(376, 738);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 28);
            this.btnStart.TabIndex = 7;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
            //
            // btnStop
            //
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(457, 738);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 28);
            this.btnStop.TabIndex = 8;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.BtnStop_Click);
            //
            // btnOpenOut
            //
            this.btnOpenOut.Location = new System.Drawing.Point(538, 738);
            this.btnOpenOut.Name = "btnOpenOut";
            this.btnOpenOut.Size = new System.Drawing.Size(129, 28);
            this.btnOpenOut.TabIndex = 9;
            this.btnOpenOut.Text = "Open output folder";
            this.btnOpenOut.UseVisualStyleBackColor = true;
            this.btnOpenOut.Click += new System.EventHandler(this.BtnOpenOut_Click);
            //
            // btnClose
            //
            this.btnClose.Location = new System.Drawing.Point(673, 738);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 28);
            this.btnClose.TabIndex = 10;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            //
            // FrmBatchExportGltf
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(760, 778);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnOpenOut);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.lblSummary);
            this.Controls.Add(this.progress);
            this.Controls.Add(this.lvResults);
            this.Controls.Add(this.gbOptions);
            this.Controls.Add(this.gbOutput);
            this.Controls.Add(this.gbModels);
            this.Controls.Add(this.lblIntro);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmBatchExportGltf";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Batch glTF Export for FFNx";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmBatchExportGltf_FormClosing);
            this.gbModels.ResumeLayout(false);
            this.gbModels.PerformLayout();
            this.gbOutput.ResumeLayout(false);
            this.gbOutput.PerformLayout();
            this.gbOptions.ResumeLayout(false);
            this.gbOptions.PerformLayout();
            this.pnlRest.ResumeLayout(false);
            this.pnlRest.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblIntro;
        private System.Windows.Forms.GroupBox gbModels;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.TextBox txtSource;
        private System.Windows.Forms.Button btnBrowseSource;
        private System.Windows.Forms.Label lblModels;
        private System.Windows.Forms.TextBox txtModels;
        private System.Windows.Forms.Button btnAddAll;
        private System.Windows.Forms.Button btnClearList;
        private System.Windows.Forms.Label lblCounts;
        private System.Windows.Forms.GroupBox gbOutput;
        private System.Windows.Forms.Label lblOut;
        private System.Windows.Forms.TextBox txtOut;
        private System.Windows.Forms.Button btnBrowseOut;
        private System.Windows.Forms.RadioButton rbOneFolder;
        private System.Windows.Forms.RadioButton rbPerModel;
        private System.Windows.Forms.GroupBox gbOptions;
        private System.Windows.Forms.Label lblRest;
        private System.Windows.Forms.Panel pnlRest;
        private System.Windows.Forms.RadioButton rbRestDefault;
        private System.Windows.Forms.RadioButton rbRestZero;
        private System.Windows.Forms.CheckBox chkDDS;
        private System.Windows.Forms.CheckBox chkBake;
        private System.Windows.Forms.CheckBox chk60fps;
        private System.Windows.Forms.Label lblLoops;
        private System.Windows.Forms.ComboBox cbLoops;
        private System.Windows.Forms.Label lblLimits;
        private System.Windows.Forms.TextBox txtLimits;
        private System.Windows.Forms.Button btnBrowseLimits;
        private System.Windows.Forms.Label lblLimitsHint;
        private System.Windows.Forms.ListView lvResults;
        private System.Windows.Forms.ColumnHeader colModel;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colResult;
        private System.Windows.Forms.ColumnHeader colDetails;
        private System.Windows.Forms.ProgressBar progress;
        private System.Windows.Forms.Label lblSummary;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnOpenOut;
        private System.Windows.Forms.Button btnClose;
    }
}
