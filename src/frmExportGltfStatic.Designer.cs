namespace KimeraCS
{
    partial class FrmExportGltfStatic
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
            this.lblNote = new System.Windows.Forms.Label();
            this.gbOutput = new System.Windows.Forms.GroupBox();
            this.chkBake = new System.Windows.Forms.CheckBox();
            this.chkDDS = new System.Windows.Forms.CheckBox();
            this.txtPrefix = new System.Windows.Forms.TextBox();
            this.lblPrefix = new System.Windows.Forms.Label();
            this.txtFileName = new System.Windows.Forms.TextBox();
            this.lblFileName = new System.Windows.Forms.Label();
            this.btnBrowseOut = new System.Windows.Forms.Button();
            this.txtOutFolder = new System.Windows.Forms.TextBox();
            this.lblOutFolder = new System.Windows.Forms.Label();
            this.txtReport = new System.Windows.Forms.TextBox();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.gbOutput.SuspendLayout();
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
            // lblNote
            //
            this.lblNote.AutoSize = true;
            this.lblNote.Location = new System.Drawing.Point(12, 29);
            this.lblNote.Name = "lblNote";
            this.lblNote.Size = new System.Drawing.Size(500, 15);
            this.lblNote.TabIndex = 1;
            this.lblNote.Text = "Static export: no skeleton, no animations. Stored in game space; the root node turns it upright in viewers.";
            //
            // gbOutput
            //
            this.gbOutput.Controls.Add(this.chkBake);
            this.gbOutput.Controls.Add(this.chkDDS);
            this.gbOutput.Controls.Add(this.txtPrefix);
            this.gbOutput.Controls.Add(this.lblPrefix);
            this.gbOutput.Controls.Add(this.txtFileName);
            this.gbOutput.Controls.Add(this.lblFileName);
            this.gbOutput.Controls.Add(this.btnBrowseOut);
            this.gbOutput.Controls.Add(this.txtOutFolder);
            this.gbOutput.Controls.Add(this.lblOutFolder);
            this.gbOutput.Location = new System.Drawing.Point(12, 52);
            this.gbOutput.Name = "gbOutput";
            this.gbOutput.Size = new System.Drawing.Size(736, 140);
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
            // txtFileName
            //
            this.txtFileName.Location = new System.Drawing.Point(120, 52);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.Size = new System.Drawing.Size(160, 23);
            this.txtFileName.TabIndex = 4;
            this.txtFileName.TextChanged += new System.EventHandler(this.TxtFileName_TextChanged);
            //
            // lblPrefix
            //
            this.lblPrefix.AutoSize = true;
            this.lblPrefix.Location = new System.Drawing.Point(10, 86);
            this.lblPrefix.Name = "lblPrefix";
            this.lblPrefix.Size = new System.Drawing.Size(86, 15);
            this.lblPrefix.TabIndex = 5;
            this.lblPrefix.Text = "Texture prefix:";
            //
            // txtPrefix
            //
            this.txtPrefix.Location = new System.Drawing.Point(120, 83);
            this.txtPrefix.Name = "txtPrefix";
            this.txtPrefix.Size = new System.Drawing.Size(160, 23);
            this.txtPrefix.TabIndex = 6;
            this.txtPrefix.TextChanged += new System.EventHandler(this.TxtPrefix_TextChanged);
            //
            // chkDDS
            //
            this.chkDDS.AutoSize = true;
            this.chkDDS.Checked = true;
            this.chkDDS.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDDS.Location = new System.Drawing.Point(120, 113);
            this.chkDDS.Name = "chkDDS";
            this.chkDDS.Size = new System.Drawing.Size(240, 19);
            this.chkDDS.TabIndex = 7;
            this.chkDDS.Text = "Write DDS textures (FFNx needs them)";
            this.chkDDS.UseVisualStyleBackColor = true;
            //
            // chkBake
            //
            this.chkBake.AutoSize = true;
            this.chkBake.Checked = true;
            this.chkBake.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBake.Location = new System.Drawing.Point(450, 113);
            this.chkBake.Name = "chkBake";
            this.chkBake.Size = new System.Drawing.Size(260, 19);
            this.chkBake.TabIndex = 8;
            this.chkBake.Text = "Bake vertex colours of untextured parts";
            this.chkBake.UseVisualStyleBackColor = true;
            //
            // txtReport
            //
            this.txtReport.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtReport.Location = new System.Drawing.Point(12, 198);
            this.txtReport.Multiline = true;
            this.txtReport.Name = "txtReport";
            this.txtReport.ReadOnly = true;
            this.txtReport.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtReport.Size = new System.Drawing.Size(736, 150);
            this.txtReport.TabIndex = 3;
            this.txtReport.WordWrap = false;
            //
            // btnExport
            //
            this.btnExport.Location = new System.Drawing.Point(592, 356);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(75, 28);
            this.btnExport.TabIndex = 4;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.BtnExport_Click);
            //
            // btnClose
            //
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(673, 356);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 28);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            //
            // FrmExportGltfStatic
            //
            this.AcceptButton = this.btnExport;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(760, 396);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.txtReport);
            this.Controls.Add(this.gbOutput);
            this.Controls.Add(this.lblNote);
            this.Controls.Add(this.lblModel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmExportGltfStatic";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Export glTF for FFNx (static model)";
            this.gbOutput.ResumeLayout(false);
            this.gbOutput.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblModel;
        private System.Windows.Forms.Label lblNote;
        private System.Windows.Forms.GroupBox gbOutput;
        private System.Windows.Forms.Label lblOutFolder;
        private System.Windows.Forms.TextBox txtOutFolder;
        private System.Windows.Forms.Button btnBrowseOut;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.TextBox txtFileName;
        private System.Windows.Forms.Label lblPrefix;
        private System.Windows.Forms.TextBox txtPrefix;
        private System.Windows.Forms.CheckBox chkDDS;
        private System.Windows.Forms.CheckBox chkBake;
        private System.Windows.Forms.TextBox txtReport;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnClose;
    }
}
