namespace KimeraCS
{
    partial class FrmExportGltf
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
            this.lblAnimNote = new System.Windows.Forms.Label();
            this.btnBrowseAnim = new System.Windows.Forms.Button();
            this.txtAnimFolder = new System.Windows.Forms.TextBox();
            this.lblAnimFolder = new System.Windows.Forms.Label();
            this.txtExtra = new System.Windows.Forms.TextBox();
            this.lblExtra = new System.Windows.Forms.Label();
            this.lblSelectedCount = new System.Windows.Forms.Label();
            this.btnSelectNone = new System.Windows.Forms.Button();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.clbAnimations = new System.Windows.Forms.CheckedListBox();
            this.lblDBList = new System.Windows.Forms.Label();
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
            this.chkBake = new System.Windows.Forms.CheckBox();
            this.chk60fps = new System.Windows.Forms.CheckBox();
            this.lblLoops = new System.Windows.Forms.Label();
            this.cbLoops = new System.Windows.Forms.ComboBox();
            this.chkDDS = new System.Windows.Forms.CheckBox();
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
            this.gbAnimations.Controls.Add(this.lblAnimNote);
            this.gbAnimations.Controls.Add(this.btnBrowseAnim);
            this.gbAnimations.Controls.Add(this.txtAnimFolder);
            this.gbAnimations.Controls.Add(this.lblAnimFolder);
            this.gbAnimations.Controls.Add(this.txtExtra);
            this.gbAnimations.Controls.Add(this.lblExtra);
            this.gbAnimations.Controls.Add(this.lblSelectedCount);
            this.gbAnimations.Controls.Add(this.btnSelectNone);
            this.gbAnimations.Controls.Add(this.btnSelectAll);
            this.gbAnimations.Controls.Add(this.clbAnimations);
            this.gbAnimations.Controls.Add(this.lblDBList);
            this.gbAnimations.Location = new System.Drawing.Point(12, 32);
            this.gbAnimations.Name = "gbAnimations";
            this.gbAnimations.Size = new System.Drawing.Size(736, 264);
            this.gbAnimations.TabIndex = 1;
            this.gbAnimations.TabStop = false;
            this.gbAnimations.Text = "Animations";
            //
            // lblDBList
            //
            this.lblDBList.AutoSize = true;
            this.lblDBList.Location = new System.Drawing.Point(10, 20);
            this.lblDBList.Name = "lblDBList";
            this.lblDBList.Size = new System.Drawing.Size(232, 15);
            this.lblDBList.TabIndex = 0;
            this.lblDBList.Text = "Compatible animations (Ifalna database):";
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
            // lblExtra
            //
            this.lblExtra.AutoSize = true;
            this.lblExtra.Location = new System.Drawing.Point(325, 20);
            this.lblExtra.Name = "lblExtra";
            this.lblExtra.Size = new System.Drawing.Size(290, 15);
            this.lblExtra.TabIndex = 5;
            this.lblExtra.Text = "Extra animations (one per line, with or without .a):";
            //
            // txtExtra
            //
            this.txtExtra.AcceptsReturn = true;
            this.txtExtra.Location = new System.Drawing.Point(325, 38);
            this.txtExtra.Multiline = true;
            this.txtExtra.Name = "txtExtra";
            this.txtExtra.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtExtra.Size = new System.Drawing.Size(401, 120);
            this.txtExtra.TabIndex = 6;
            //
            // lblAnimFolder
            //
            this.lblAnimFolder.AutoSize = true;
            this.lblAnimFolder.Location = new System.Drawing.Point(325, 166);
            this.lblAnimFolder.Name = "lblAnimFolder";
            this.lblAnimFolder.Size = new System.Drawing.Size(154, 15);
            this.lblAnimFolder.TabIndex = 7;
            this.lblAnimFolder.Text = "Animation folder (.a files):";
            //
            // txtAnimFolder
            //
            this.txtAnimFolder.Location = new System.Drawing.Point(325, 184);
            this.txtAnimFolder.Name = "txtAnimFolder";
            this.txtAnimFolder.Size = new System.Drawing.Size(320, 23);
            this.txtAnimFolder.TabIndex = 8;
            this.txtAnimFolder.Leave += new System.EventHandler(this.TxtAnimFolder_Leave);
            //
            // btnBrowseAnim
            //
            this.btnBrowseAnim.Location = new System.Drawing.Point(651, 183);
            this.btnBrowseAnim.Name = "btnBrowseAnim";
            this.btnBrowseAnim.Size = new System.Drawing.Size(75, 25);
            this.btnBrowseAnim.TabIndex = 9;
            this.btnBrowseAnim.Text = "Browse...";
            this.btnBrowseAnim.UseVisualStyleBackColor = true;
            this.btnBrowseAnim.Click += new System.EventHandler(this.BtnBrowseAnim_Click);
            //
            // lblAnimNote
            //
            this.lblAnimNote.AutoSize = true;
            this.lblAnimNote.Location = new System.Drawing.Point(325, 216);
            this.lblAnimNote.Name = "lblAnimNote";
            this.lblAnimNote.Size = new System.Drawing.Size(380, 15);
            this.lblAnimNote.TabIndex = 10;
            this.lblAnimNote.Text = "Exported exactly as stored: one key per .a frame.";
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
            this.lblFileName.Size = new System.Drawing.Size(88, 15);
            this.lblFileName.TabIndex = 3;
            this.lblFileName.Text = "File name (.p):";
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
            this.lblFileNameHint.Text = "FFNx loads mesh\\field\\<name>.gltf; any one of the model\'s .p names works.";
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
            this.lblPrefixHint.Size = new System.Drawing.Size(430, 15);
            this.lblPrefixHint.TabIndex = 8;
            this.lblPrefixHint.Text = "Textures: <prefix>_0, <prefix>_1 ... must be unique per model.";
            //
            // gbOptions
            //
            this.gbOptions.Controls.Add(this.cbLoops);
            this.gbOptions.Controls.Add(this.lblLoops);
            this.gbOptions.Controls.Add(this.chk60fps);
            this.gbOptions.Controls.Add(this.chkBake);
            this.gbOptions.Controls.Add(this.chkDDS);
            this.gbOptions.Controls.Add(this.rbRestZero);
            this.gbOptions.Controls.Add(this.rbRestCurrent);
            this.gbOptions.Controls.Add(this.lblRest);
            this.gbOptions.Controls.Add(this.lblFpsHint);
            this.gbOptions.Controls.Add(this.cbFps);
            this.gbOptions.Controls.Add(this.lblFps);
            this.gbOptions.Location = new System.Drawing.Point(12, 420);
            this.gbOptions.Name = "gbOptions";
            this.gbOptions.Size = new System.Drawing.Size(736, 142);
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
            this.lblFpsHint.Size = new System.Drawing.Size(520, 15);
            this.lblFpsHint.TabIndex = 2;
            this.lblFpsHint.Text = "Timestamps only (speed in viewers). Field animations are 30 fps.";
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
            // rbRestCurrent
            //
            this.rbRestCurrent.AutoSize = true;
            this.rbRestCurrent.Checked = true;
            this.rbRestCurrent.Location = new System.Drawing.Point(120, 53);
            this.rbRestCurrent.Name = "rbRestCurrent";
            this.rbRestCurrent.Size = new System.Drawing.Size(103, 19);
            this.rbRestCurrent.TabIndex = 4;
            this.rbRestCurrent.TabStop = true;
            this.rbRestCurrent.Text = "Current frame";
            this.rbRestCurrent.UseVisualStyleBackColor = true;
            //
            // rbRestZero
            //
            this.rbRestZero.AutoSize = true;
            this.rbRestZero.Location = new System.Drawing.Point(450, 53);
            this.rbRestZero.Name = "rbRestZero";
            this.rbRestZero.Size = new System.Drawing.Size(180, 19);
            this.rbRestZero.TabIndex = 5;
            this.rbRestZero.Text = "All zero (CrossSlash style)";
            this.rbRestZero.UseVisualStyleBackColor = true;
            //
            // chkDDS
            //
            this.chkDDS.AutoSize = true;
            this.chkDDS.Checked = true;
            this.chkDDS.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDDS.Location = new System.Drawing.Point(120, 82);
            this.chkDDS.Name = "chkDDS";
            this.chkDDS.Size = new System.Drawing.Size(240, 19);
            this.chkDDS.TabIndex = 6;
            this.chkDDS.Text = "Write DDS textures (FFNx needs them)";
            this.chkDDS.UseVisualStyleBackColor = true;
            //
            // chkBake
            //
            this.chkBake.AutoSize = true;
            this.chkBake.Checked = true;
            this.chkBake.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBake.Location = new System.Drawing.Point(450, 82);
            this.chkBake.Name = "chkBake";
            this.chkBake.Size = new System.Drawing.Size(260, 19);
            this.chkBake.TabIndex = 7;
            this.chkBake.Text = "Bake vertex colors of untextured parts";
            this.chkBake.UseVisualStyleBackColor = true;
            //
            // chk60fps
            //
            this.chk60fps.AutoSize = true;
            this.chk60fps.Location = new System.Drawing.Point(120, 112);
            this.chk60fps.Name = "chk60fps";
            this.chk60fps.Size = new System.Drawing.Size(280, 19);
            this.chk60fps.TabIndex = 8;
            this.chk60fps.Text = "Convert 30 -> 60 fps (adds an in-between key)";
            this.chk60fps.UseVisualStyleBackColor = true;
            this.chk60fps.CheckedChanged += new System.EventHandler(this.Chk60fps_CheckedChanged);
            //
            // lblLoops
            //
            this.lblLoops.AutoSize = true;
            this.lblLoops.Location = new System.Drawing.Point(450, 113);
            this.lblLoops.Name = "lblLoops";
            this.lblLoops.Size = new System.Drawing.Size(41, 15);
            this.lblLoops.TabIndex = 9;
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
            this.cbLoops.Location = new System.Drawing.Point(497, 109);
            this.cbLoops.Name = "cbLoops";
            this.cbLoops.Size = new System.Drawing.Size(150, 23);
            this.cbLoops.TabIndex = 10;
            //
            // txtReport
            //
            this.txtReport.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtReport.Location = new System.Drawing.Point(12, 568);
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
            this.btnExport.Location = new System.Drawing.Point(592, 726);
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
            this.btnClose.Location = new System.Drawing.Point(673, 726);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 28);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            //
            // FrmExportGltf
            //
            this.AcceptButton = this.btnExport;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(760, 766);
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
            this.Name = "FrmExportGltf";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Export glTF for FFNx";
            this.gbAnimations.ResumeLayout(false);
            this.gbAnimations.PerformLayout();
            this.gbOutput.ResumeLayout(false);
            this.gbOutput.PerformLayout();
            this.gbOptions.ResumeLayout(false);
            this.gbOptions.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblModel;
        private System.Windows.Forms.GroupBox gbAnimations;
        private System.Windows.Forms.Label lblDBList;
        private System.Windows.Forms.CheckedListBox clbAnimations;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnSelectNone;
        private System.Windows.Forms.Label lblSelectedCount;
        private System.Windows.Forms.Label lblExtra;
        private System.Windows.Forms.TextBox txtExtra;
        private System.Windows.Forms.Label lblAnimFolder;
        private System.Windows.Forms.TextBox txtAnimFolder;
        private System.Windows.Forms.Button btnBrowseAnim;
        private System.Windows.Forms.Label lblAnimNote;
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
        private System.Windows.Forms.RadioButton rbRestCurrent;
        private System.Windows.Forms.RadioButton rbRestZero;
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
