namespace Moor.XmlToCsvConverter
{
    partial class ExportTool
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
            this.butSelectFolder = new System.Windows.Forms.Button();
            this.chbSaveAllTables = new System.Windows.Forms.CheckBox();
            this.txbLog = new System.Windows.Forms.TextBox();
            this.butSelectXml = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.lsbTables = new System.Windows.Forms.ListBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.butSaveCsv = new System.Windows.Forms.Button();
            this.txbFilePath = new System.Windows.Forms.TextBox();
            this.ddlEncoding = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // butSelectFolder
            // 
            this.butSelectFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butSelectFolder.Enabled = false;
            this.butSelectFolder.Location = new System.Drawing.Point(240, 186);
            this.butSelectFolder.Name = "butSelectFolder";
            this.butSelectFolder.Size = new System.Drawing.Size(131, 23);
            this.butSelectFolder.TabIndex = 15;
            this.butSelectFolder.Text = "Select destination...";
            this.butSelectFolder.UseVisualStyleBackColor = true;
            // 
            // chbSaveAllTables
            // 
            this.chbSaveAllTables.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chbSaveAllTables.AutoSize = true;
            this.chbSaveAllTables.Enabled = false;
            this.chbSaveAllTables.Location = new System.Drawing.Point(240, 162);
            this.chbSaveAllTables.Name = "chbSaveAllTables";
            this.chbSaveAllTables.Size = new System.Drawing.Size(129, 17);
            this.chbSaveAllTables.TabIndex = 14;
            this.chbSaveAllTables.Text = "save all tables to CSV";
            this.chbSaveAllTables.UseVisualStyleBackColor = true;
            // 
            // txbLog
            // 
            this.txbLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txbLog.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txbLog.ForeColor = System.Drawing.Color.Black;
            this.txbLog.Location = new System.Drawing.Point(0, 267);
            this.txbLog.Multiline = true;
            this.txbLog.Name = "txbLog";
            this.txbLog.ReadOnly = true;
            this.txbLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txbLog.Size = new System.Drawing.Size(372, 247);
            this.txbLog.TabIndex = 16;
            // 
            // butSelectXml
            // 
            this.butSelectXml.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butSelectXml.Location = new System.Drawing.Point(240, 4);
            this.butSelectXml.Name = "butSelectXml";
            this.butSelectXml.Size = new System.Drawing.Size(131, 23);
            this.butSelectXml.TabIndex = 11;
            this.butSelectXml.Text = "Select XML file...";
            this.butSelectXml.UseVisualStyleBackColor = true;
            // 
            // lsbTables
            // 
            this.lsbTables.Dock = System.Windows.Forms.DockStyle.Left;
            this.lsbTables.FormattingEnabled = true;
            this.lsbTables.Location = new System.Drawing.Point(0, 0);
            this.lsbTables.Name = "lsbTables";
            this.lsbTables.Size = new System.Drawing.Size(235, 247);
            this.lsbTables.TabIndex = 10;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // butSaveCsv
            // 
            this.butSaveCsv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butSaveCsv.Enabled = false;
            this.butSaveCsv.Location = new System.Drawing.Point(240, 215);
            this.butSaveCsv.Name = "butSaveCsv";
            this.butSaveCsv.Size = new System.Drawing.Size(131, 23);
            this.butSaveCsv.TabIndex = 13;
            this.butSaveCsv.Text = "Save CSV file...";
            this.butSaveCsv.UseVisualStyleBackColor = true;
            // 
            // txbFilePath
            // 
            this.txbFilePath.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txbFilePath.Location = new System.Drawing.Point(0, 247);
            this.txbFilePath.Name = "txbFilePath";
            this.txbFilePath.ReadOnly = true;
            this.txbFilePath.Size = new System.Drawing.Size(372, 20);
            this.txbFilePath.TabIndex = 12;
            // 
            // ddlEncoding
            // 
            this.ddlEncoding.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ddlEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlEncoding.FormattingEnabled = true;
            this.ddlEncoding.Location = new System.Drawing.Point(240, 131);
            this.ddlEncoding.Name = "ddlEncoding";
            this.ddlEncoding.Size = new System.Drawing.Size(131, 21);
            this.ddlEncoding.TabIndex = 17;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(242, 112);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Encoding";
            // 
            // ExportTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(372, 514);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ddlEncoding);
            this.Controls.Add(this.butSelectFolder);
            this.Controls.Add(this.chbSaveAllTables);
            this.Controls.Add(this.butSelectXml);
            this.Controls.Add(this.lsbTables);
            this.Controls.Add(this.butSaveCsv);
            this.Controls.Add(this.txbFilePath);
            this.Controls.Add(this.txbLog);
            this.MinimumSize = new System.Drawing.Size(380, 541);
            this.Name = "ExportTool";
            this.Text = "ExportTool";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button butSelectFolder;
        private System.Windows.Forms.CheckBox chbSaveAllTables;
        private System.Windows.Forms.TextBox txbLog;
        private System.Windows.Forms.Button butSelectXml;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ListBox lsbTables;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button butSaveCsv;
        private System.Windows.Forms.TextBox txbFilePath;
        private System.Windows.Forms.ComboBox ddlEncoding;
        private System.Windows.Forms.Label label1;
    }
}