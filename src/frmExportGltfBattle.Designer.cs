namespace KimeraCS
{
    partial class FrmExportGltfBattle
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
            this.lblModel = new System.Windows.Forms.Label();
            this.gbAnimations = new System.Windows.Forms.GroupBox();
            this.btnBrowseLimits = new System.Windows.Forms.Button();
            this.txtLimitsFolder = new System.Windows.Forms.TextBox();
            this.lblLimitsFolder = new System.Windows.Forms.Label();
            this.clbLimits = new System.Windows.Forms.CheckedListBox();
            this.lblLimits = new System.Windows.Forms.Label();
            this.lblSelectedCount = new System.Windows.Forms.Label();
            this.btnSelectNone = new System.Windows.Forms.Button();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.clbAnimations = new System.Windows.Forms.CheckedListBox();
            this.lblPack = new System.Windows.Forms.Label();
            this.gbOutput = new System.Windows.Forms.GroupBox();
            this.lblPrefixHint = new System.Windows.Forms.Label();
            this.txtPrefix = new System.Windows.Forms.TextBox();
            this.lblPrefix = new System.Windows.Forms.Label();
            this.lblFileNameHint = new System.Windows.Forms.Label();
            this.cbFileName = new System.Windows.Forms.ComboBox();
            this.lblFileName = new System.Windows.Forms.Label();
            this.btnBrowseOut = new System.Windows.Forms.Button();
            this.txtOutFolder = new System.Windows.Forms.TextBox();
            this.lblOutFolder = new System.Windows.Forms.Label();
            this.gbOptions = new System.Windows.Forms.GroupBox();
            this.rbWeaponsNone = new System.Windows.Forms.RadioButton();
            this.rbWeaponsCurrent = new System.Windows.Forms.RadioButton();
            this.rbWeaponsAll = new System.Windows.Forms.RadioButton();
            this.lblWeapons = new System.Windows.Forms.Label();
            this.chkBake = new System.Windows.Forms.CheckBox();
            this.chk60fps = new System.Windows.Forms.CheckBox();
            this.lblLoops = new System.Windows.Forms.Label();
            this.cbLoops = new System.Windows.Forms.ComboBox();
            this.chkDDS = new System.Windows.Forms.CheckBox();
            this.pnlRest = new System.Windows.Forms.Panel();
            this.rbRestZero = new System.Windows.Forms.RadioButton();
            this.rbRestCurrent = new System.Windows.Forms.RadioButton();
            this.lblRest = new System.Windows.Forms.Label();
            this.lblFpsHint = new System.Windows.Forms.Label();
            this.cbFps = new System.Windows.Forms.ComboBox();
            this.lblFps = new System.Windows.Forms.Label();
            this.txtReport = new System.Windows.Forms.TextBox();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.gbAnimations.SuspendLayout();
            this.gbOutput.SuspendLayout();
            this.gbOptions.SuspendLayout();
            this.pnlRest.SuspendLayout();
            this.SuspendLayout();
            //
            // lblModel
            //
            this.lblModel.AutoSize = true;
            this.lblModel.Location = new System.Drawing.Point(12, 9);
            this.lblModel.Name = "lblModel";
            this.lblModel.Size = new System.Drawing.Size(44, 15);
            this.lblModel.TabIndex = 0;
            this.lblModel.Text = "Model:";
            //
            // gbAnimations
            //
            this.gbAnimations.Controls.Add(this.btnBrowseLimits);
            this.gbAnimations.Controls.Add(this.txtLimitsFolder);
            this.gbAnimations.Controls.Add(this.lblLimitsFolder);
            this.gbAnimations.Controls.Add(this.clbLimits);
            this.gbAnimations.Controls.Add(this.lblLimits);
            this.gbAnimations.Controls.Add(this.lblSelectedCount);
            this.gbAnimations.Controls.Add(this.btnSelectNone);
            this.gbAnimations.Controls.Add(this.btnSelectAll);
            this.gbAnimations.Controls.Add(this.clbAnimations);
            this.gbAnimations.Controls.Add(this.lblPack);
            this.gbAnimations.Location = new System.Drawing.Point(12, 32);
            this.gbAnimations.Name = "gbAnimations";
            this.gbAnimations.Size = new System.Drawing.Size(736, 264);
            this.gbAnimations.TabIndex = 1;
            this.gbAnimations.TabStop = false;
            this.gbAnimations.Text = "Animations";
            //
            // lblPack
            //
            this.lblPack.AutoSize = true;
            this.lblPack.Location = new System.Drawing.Point(10, 20);
            this.lblPack.Name = "lblPack";
            this.lblPack.Size = new System.Drawing.Size(100, 15);
            this.lblPack.TabIndex = 0;
            this.lblPack.Text = "Animation pack:";
            //
            // clbAnimations
            //
            this.clbAnimations.CheckOnClick = true;
            this.clbAnimations.FormattingEnabled = true;
            this.clbAnimations.IntegralHeight = false;
            this.clbAnimations.Location = new System.Drawing.Point(10, 38);
            this.clbAnimations.Name = "clbAnimations";
            this.clbAnimations.Size = new System.Drawing.Size(300, 184);
            this.clbAnimations.TabIndex = 1;
            this.clbAnimations.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ClbAnimations_ItemCheck);
            //
            // btnSelectAll
            //
            this.btnSelectAll.Location = new System.Drawing.Point(10, 228);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(95, 25);
            this.btnSelectAll.TabIndex = 2;
            this.btnSelectAll.Text = "Select All";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.BtnSelectAll_Click);
            //
            // btnSelectNone
            //
            this.btnSelectNone.Location = new System.Drawing.Point(111, 228);
            this.btnSelectNone.Name = "btnSelectNone";
            this.btnSelectNone.Size = new System.Drawing.Size(95, 25);
            this.btnSelectNone.TabIndex = 3;
            this.btnSelectNone.Text = "Select None";
            this.btnSelectNone.UseVisualStyleBackColor = true;
            this.btnSelectNone.Click += new System.EventHandler(this.BtnSelectNone_Click);
            //
            // lblSelectedCount
            //
            this.lblSelectedCount.AutoSize = true;
            this.lblSelectedCount.Location = new System.Drawing.Point(212, 233);
            this.lblSelectedCount.Name = "lblSelectedCount";
            this.lblSelectedCount.Size = new System.Drawing.Size(62, 15);
            this.lblSelectedCount.TabIndex = 4;
            this.lblSelectedCount.Text = "0 selected";
            //
            // lblLimits
            //
            this.lblLimits.AutoSize = true;
            this.lblLimits.Location = new System.Drawing.Point(325, 20);
            this.lblLimits.Name = "lblLimits";
            this.lblLimits.Size = new System.Drawing.Size(250, 15);
            this.lblLimits.TabIndex = 5;
            this.lblLimits.Text = "Limit breaks (exported as <PACK>_00, _01 ...):";
            //
            // clbLimits
            //
            this.clbLimits.CheckOnClick = true;
            this.clbLimits.FormattingEnabled = true;
            this.clbLimits.IntegralHeight = false;
            this.clbLimits.Location = new System.Drawing.Point(325, 38);
            this.clbLimits.Name = "clbLimits";
            this.clbLimits.Size = new System.Drawing.Size(401, 120);
            this.clbLimits.TabIndex = 6;
            //
            // lblLimitsFolder
            //
            this.lblLimitsFolder.AutoSize = true;
            this.lblLimitsFolder.Location = new System.Drawing.Point(325, 166);
            this.lblLimitsFolder.Name = "lblLimitsFolder";
            this.lblLimitsFolder.Size = new System.Drawing.Size(230, 15);
            this.lblLimitsFolder.TabIndex = 7;
            this.lblLimitsFolder.Text = "Limit break folder (.A00 files, magic.lgp):";
            //
            // txtLimitsFolder
            //
            this.txtLimitsFolder.Location = new System.Drawing.Point(325, 184);
            this.txtLimitsFolder.Name = "txtLimitsFolder";
            this.txtLimitsFolder.Size = new System.Drawing.Size(320, 23);
            this.txtLimitsFolder.TabIndex = 8;
            this.txtLimitsFolder.Leave += new System.EventHandler(this.TxtLimitsFolder_Leave);
            //
            // btnBrowseLimits
            //
            this.btnBrowseLimits.Location = new System.Drawing.Point(651, 183);
            this.btnBrowseLimits.Name = "btnBrowseLimits";
            this.btnBrowseLimits.Size = new System.Drawing.Size(75, 25);
            this.btnBrowseLimits.TabIndex = 9;
            this.btnBrowseLimits.Text = "Browse...";
            this.btnBrowseLimits.UseVisualStyleBackColor = true;
            this.btnBrowseLimits.Click += new System.EventHandler(this.BtnBrowseLimits_Click);
            //
            // gbOutput
            //
            this.gbOutput.Controls.Add(this.lblPrefixHint);
            this.gbOutput.Controls.Add(this.txtPrefix);
            this.gbOutput.Controls.Add(this.lblPrefix);
            this.gbOutput.Controls.Add(this.lblFileNameHint);
            this.gbOutput.Controls.Add(this.cbFileName);
            this.gbOutput.Controls.Add(this.lblFileName);
            this.gbOutput.Controls.Add(this.btnBrowseOut);
            this.gbOutput.Controls.Add(this.txtOutFolder);
            this.gbOutput.Controls.Add(this.lblOutFolder);
            this.gbOutput.Location = new System.Drawing.Point(12, 302);
            this.gbOutput.Name = "gbOutput";
            this.gbOutput.Size = new System.Drawing.Size(736, 112);
            this.gbOutput.TabIndex = 2;
            this.gbOutput.TabStop = false;
            this.gbOutput.Text = "Output";
            //
            // lblOutFolder
            //
            this.lblOutFolder.AutoSize = true;
            this.lblOutFolder.Location = new System.Drawing.Point(10, 24);
            this.lblOutFolder.Name = "lblOutFolder";
            this.lblOutFolder.Size = new System.Drawing.Size(84, 15);
            this.lblOutFolder.TabIndex = 0;
            this.lblOutFolder.Text = "Output folder:";
            //
            // txtOutFolder
            //
            this.txtOutFolder.Location = new System.Drawing.Point(120, 21);
            this.txtOutFolder.Name = "txtOutFolder";
            this.txtOutFolder.Size = new System.Drawing.Size(525, 23);
            this.txtOutFolder.TabIndex = 1;
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
            // lblFileName
            //
            this.lblFileName.AutoSize = true;
            this.lblFileName.Location = new System.Drawing.Point(10, 55);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(63, 15);
            this.lblFileName.TabIndex = 3;
            this.lblFileName.Text = "File name:";
            //
            // cbFileName
            //
            this.cbFileName.FormattingEnabled = true;
            this.cbFileName.Location = new System.Drawing.Point(120, 52);
            this.cbFileName.Name = "cbFileName";
            this.cbFileName.Size = new System.Drawing.Size(120, 23);
            this.cbFileName.TabIndex = 4;
            this.cbFileName.TextChanged += new System.EventHandler(this.CbFileName_TextChanged);
            //
            // lblFileNameHint
            //
            this.lblFileNameHint.AutoSize = true;
            this.lblFileNameHint.Location = new System.Drawing.Point(250, 55);
            this.lblFileNameHint.Name = "lblFileNameHint";
            this.lblFileNameHint.Size = new System.Drawing.Size(420, 15);
            this.lblFileNameHint.TabIndex = 5;
            this.lblFileNameHint.Text = "FFNx looks the model up by its first piece (Cloud: RTAM).";
            //
            // lblPrefix
            //
            this.lblPrefix.AutoSize = true;
            this.lblPrefix.Location = new System.Drawing.Point(10, 86);
            this.lblPrefix.Name = "lblPrefix";
            this.lblPrefix.Size = new System.Drawing.Size(86, 15);
            this.lblPrefix.TabIndex = 6;
            this.lblPrefix.Text = "Texture prefix:";
            //
            // txtPrefix
            //
            this.txtPrefix.Location = new System.Drawing.Point(120, 83);
            this.txtPrefix.Name = "txtPrefix";
            this.txtPrefix.Size = new System.Drawing.Size(120, 23);
            this.txtPrefix.TabIndex = 7;
            this.txtPrefix.TextChanged += new System.EventHandler(this.TxtPrefix_TextChanged);
            //
            // lblPrefixHint
            //
            this.lblPrefixHint.AutoSize = true;
            this.lblPrefixHint.Location = new System.Drawing.Point(250, 86);
            this.lblPrefixHint.Name = "lblPrefixHint";
            this.lblPrefixHint.Size = new System.Drawing.Size(360, 15);
            this.lblPrefixHint.TabIndex = 8;
            this.lblPrefixHint.Text = "Textures: <prefix>_0, <prefix>_1 ... must be unique per model.";
            //
            // gbOptions
            //
            this.gbOptions.Controls.Add(this.rbWeaponsNone);
            this.gbOptions.Controls.Add(this.rbWeaponsCurrent);
            this.gbOptions.Controls.Add(this.rbWeaponsAll);
            this.gbOptions.Controls.Add(this.lblWeapons);
            this.gbOptions.Controls.Add(this.cbLoops);
            this.gbOptions.Controls.Add(this.lblLoops);
            this.gbOptions.Controls.Add(this.chk60fps);
            this.gbOptions.Controls.Add(this.chkBake);
            this.gbOptions.Controls.Add(this.chkDDS);
            this.gbOptions.Controls.Add(this.pnlRest);
            this.gbOptions.Controls.Add(this.lblRest);
            this.gbOptions.Controls.Add(this.lblFpsHint);
            this.gbOptions.Controls.Add(this.cbFps);
            this.gbOptions.Controls.Add(this.lblFps);
            this.gbOptions.Location = new System.Drawing.Point(12, 420);
            this.gbOptions.Name = "gbOptions";
            this.gbOptions.Size = new System.Drawing.Size(736, 170);
            this.gbOptions.TabIndex = 3;
            this.gbOptions.TabStop = false;
            this.gbOptions.Text = "Options";
            //
            // lblFps
            //
            this.lblFps.AutoSize = true;
            this.lblFps.Location = new System.Drawing.Point(10, 24);
            this.lblFps.Name = "lblFps";
            this.lblFps.Size = new System.Drawing.Size(69, 15);
            this.lblFps.TabIndex = 0;
            this.lblFps.Text = "Frame rate:";
            //
            // cbFps
            //
            this.cbFps.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFps.FormattingEnabled = true;
            this.cbFps.Items.AddRange(new object[] {
            "15",
            "30",
            "60"});
            this.cbFps.Location = new System.Drawing.Point(120, 21);
            this.cbFps.Name = "cbFps";
            this.cbFps.Size = new System.Drawing.Size(60, 23);
            this.cbFps.TabIndex = 1;
            //
            // lblFpsHint
            //
            this.lblFpsHint.AutoSize = true;
            this.lblFpsHint.Location = new System.Drawing.Point(190, 24);
            this.lblFpsHint.Name = "lblFpsHint";
            this.lblFpsHint.Size = new System.Drawing.Size(360, 15);
            this.lblFpsHint.TabIndex = 2;
            this.lblFpsHint.Text = "Timestamps only (speed in viewers). Battle animations are 15 fps.";
            //
            // lblRest
            //
            this.lblRest.AutoSize = true;
            this.lblRest.Location = new System.Drawing.Point(10, 55);
            this.lblRest.Name = "lblRest";
            this.lblRest.Size = new System.Drawing.Size(62, 15);
            this.lblRest.TabIndex = 3;
            this.lblRest.Text = "Rest pose:";
            //
            // pnlRest
            //
            this.pnlRest.Controls.Add(this.rbRestZero);
            this.pnlRest.Controls.Add(this.rbRestCurrent);
            this.pnlRest.Location = new System.Drawing.Point(116, 50);
            this.pnlRest.Name = "pnlRest";
            this.pnlRest.Size = new System.Drawing.Size(610, 26);
            this.pnlRest.TabIndex = 4;
            //
            // rbRestCurrent
            //
            this.rbRestCurrent.AutoSize = true;
            this.rbRestCurrent.Checked = true;
            this.rbRestCurrent.Location = new System.Drawing.Point(4, 3);
            this.rbRestCurrent.Name = "rbRestCurrent";
            this.rbRestCurrent.Size = new System.Drawing.Size(103, 19);
            this.rbRestCurrent.TabIndex = 0;
            this.rbRestCurrent.TabStop = true;
            this.rbRestCurrent.Text = "Current frame";
            this.rbRestCurrent.UseVisualStyleBackColor = true;
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
            // lblWeapons
            //
            this.lblWeapons.AutoSize = true;
            this.lblWeapons.Location = new System.Drawing.Point(10, 84);
            this.lblWeapons.Name = "lblWeapons";
            this.lblWeapons.Size = new System.Drawing.Size(59, 15);
            this.lblWeapons.TabIndex = 5;
            this.lblWeapons.Text = "Weapons:";
            //
            // rbWeaponsAll
            //
            this.rbWeaponsAll.AutoSize = true;
            this.rbWeaponsAll.Checked = true;
            this.rbWeaponsAll.Location = new System.Drawing.Point(120, 82);
            this.rbWeaponsAll.Name = "rbWeaponsAll";
            this.rbWeaponsAll.Size = new System.Drawing.Size(39, 19);
            this.rbWeaponsAll.TabIndex = 6;
            this.rbWeaponsAll.TabStop = true;
            this.rbWeaponsAll.Text = "All";
            this.rbWeaponsAll.UseVisualStyleBackColor = true;
            //
            // rbWeaponsCurrent
            //
            this.rbWeaponsCurrent.AutoSize = true;
            this.rbWeaponsCurrent.Location = new System.Drawing.Point(250, 82);
            this.rbWeaponsCurrent.Name = "rbWeaponsCurrent";
            this.rbWeaponsCurrent.Size = new System.Drawing.Size(95, 19);
            this.rbWeaponsCurrent.TabIndex = 7;
            this.rbWeaponsCurrent.Text = "Only current";
            this.rbWeaponsCurrent.UseVisualStyleBackColor = true;
            //
            // rbWeaponsNone
            //
            this.rbWeaponsNone.AutoSize = true;
            this.rbWeaponsNone.Location = new System.Drawing.Point(450, 82);
            this.rbWeaponsNone.Name = "rbWeaponsNone";
            this.rbWeaponsNone.Size = new System.Drawing.Size(54, 19);
            this.rbWeaponsNone.TabIndex = 8;
            this.rbWeaponsNone.Text = "None";
            this.rbWeaponsNone.UseVisualStyleBackColor = true;
            //
            // chkDDS
            //
            this.chkDDS.AutoSize = true;
            this.chkDDS.Checked = true;
            this.chkDDS.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDDS.Location = new System.Drawing.Point(120, 110);
            this.chkDDS.Name = "chkDDS";
            this.chkDDS.Size = new System.Drawing.Size(240, 19);
            this.chkDDS.TabIndex = 9;
            this.chkDDS.Text = "Write DDS textures (FFNx needs them)";
            this.chkDDS.UseVisualStyleBackColor = true;
            //
            // chkBake
            //
            this.chkBake.AutoSize = true;
            this.chkBake.Checked = true;
            this.chkBake.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBake.Location = new System.Drawing.Point(450, 110);
            this.chkBake.Name = "chkBake";
            this.chkBake.Size = new System.Drawing.Size(260, 19);
            this.chkBake.TabIndex = 10;
            this.chkBake.Text = "Bake vertex colors of untextured parts";
            this.chkBake.UseVisualStyleBackColor = true;
            //
            // chk60fps
            //
            this.chk60fps.AutoSize = true;
            this.chk60fps.Location = new System.Drawing.Point(120, 140);
            this.chk60fps.Name = "chk60fps";
            this.chk60fps.Size = new System.Drawing.Size(300, 19);
            this.chk60fps.TabIndex = 11;
            this.chk60fps.Text = "Convert 15 -> 60 fps (adds 3 in-between keys)";
            this.chk60fps.UseVisualStyleBackColor = true;
            this.chk60fps.CheckedChanged += new System.EventHandler(this.Chk60fps_CheckedChanged);
            //
            // lblLoops
            //
            this.lblLoops.AutoSize = true;
            this.lblLoops.Location = new System.Drawing.Point(450, 141);
            this.lblLoops.Name = "lblLoops";
            this.lblLoops.Size = new System.Drawing.Size(41, 15);
            this.lblLoops.TabIndex = 12;
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
            this.cbLoops.Location = new System.Drawing.Point(497, 137);
            this.cbLoops.Name = "cbLoops";
            this.cbLoops.Size = new System.Drawing.Size(150, 23);
            this.cbLoops.TabIndex = 13;
            //
            // txtReport
            //
            this.txtReport.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtReport.Location = new System.Drawing.Point(12, 596);
            this.txtReport.Multiline = true;
            this.txtReport.Name = "txtReport";
            this.txtReport.ReadOnly = true;
            this.txtReport.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtReport.Size = new System.Drawing.Size(736, 150);
            this.txtReport.TabIndex = 4;
            this.txtReport.WordWrap = false;
            //
            // btnExport
            //
            this.btnExport.Location = new System.Drawing.Point(592, 754);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(75, 28);
            this.btnExport.TabIndex = 5;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.BtnExport_Click);
            //
            // btnClose
            //
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(673, 754);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 28);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            //
            // FrmExportGltfBattle
            //
            this.AcceptButton = this.btnExport;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(760, 794);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.txtReport);
            this.Controls.Add(this.gbOptions);
            this.Controls.Add(this.gbOutput);
            this.Controls.Add(this.gbAnimations);
            this.Controls.Add(this.lblModel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmExportGltfBattle";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Export glTF for FFNx (battle / summon)";
            this.gbAnimations.ResumeLayout(false);
            this.gbAnimations.PerformLayout();
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

        private System.Windows.Forms.Label lblModel;
        private System.Windows.Forms.GroupBox gbAnimations;
        private System.Windows.Forms.Label lblPack;
        private System.Windows.Forms.CheckedListBox clbAnimations;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnSelectNone;
        private System.Windows.Forms.Label lblSelectedCount;
        private System.Windows.Forms.Label lblLimits;
        private System.Windows.Forms.CheckedListBox clbLimits;
        private System.Windows.Forms.Label lblLimitsFolder;
        private System.Windows.Forms.TextBox txtLimitsFolder;
        private System.Windows.Forms.Button btnBrowseLimits;
        private System.Windows.Forms.GroupBox gbOutput;
        private System.Windows.Forms.Label lblOutFolder;
        private System.Windows.Forms.TextBox txtOutFolder;
        private System.Windows.Forms.Button btnBrowseOut;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.ComboBox cbFileName;
        private System.Windows.Forms.Label lblFileNameHint;
        private System.Windows.Forms.Label lblPrefix;
        private System.Windows.Forms.TextBox txtPrefix;
        private System.Windows.Forms.Label lblPrefixHint;
        private System.Windows.Forms.GroupBox gbOptions;
        private System.Windows.Forms.Label lblFps;
        private System.Windows.Forms.ComboBox cbFps;
        private System.Windows.Forms.Label lblFpsHint;
        private System.Windows.Forms.Label lblRest;
        private System.Windows.Forms.Panel pnlRest;
        private System.Windows.Forms.RadioButton rbRestCurrent;
        private System.Windows.Forms.RadioButton rbRestZero;
        private System.Windows.Forms.Label lblWeapons;
        private System.Windows.Forms.RadioButton rbWeaponsAll;
        private System.Windows.Forms.RadioButton rbWeaponsCurrent;
        private System.Windows.Forms.RadioButton rbWeaponsNone;
        private System.Windows.Forms.CheckBox chkDDS;
        private System.Windows.Forms.CheckBox chkBake;
        private System.Windows.Forms.CheckBox chk60fps;
        private System.Windows.Forms.Label lblLoops;
        private System.Windows.Forms.ComboBox cbLoops;
        private System.Windows.Forms.TextBox txtReport;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnClose;
    }
}
