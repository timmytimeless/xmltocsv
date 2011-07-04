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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportTool));
            this.txbLog = new System.Windows.Forms.TextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.lsbTables = new System.Windows.Forms.ListBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.butSelectXml = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.ddlEncoding = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.butSelectFolder = new System.Windows.Forms.ToolStripButton();
            this.lblTableNames = new System.Windows.Forms.Label();
            this.gv = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gv)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txbLog
            // 
            this.txbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txbLog.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txbLog.ForeColor = System.Drawing.Color.Black;
            this.txbLog.Location = new System.Drawing.Point(0, 239);
            this.txbLog.Multiline = true;
            this.txbLog.Name = "txbLog";
            this.txbLog.ReadOnly = true;
            this.txbLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txbLog.Size = new System.Drawing.Size(568, 264);
            this.txbLog.TabIndex = 16;
            // 
            // lsbTables
            // 
            this.lsbTables.Dock = System.Windows.Forms.DockStyle.Top;
            this.lsbTables.FormattingEnabled = true;
            this.lsbTables.Location = new System.Drawing.Point(0, 38);
            this.lsbTables.Name = "lsbTables";
            this.lsbTables.Size = new System.Drawing.Size(568, 147);
            this.lsbTables.TabIndex = 10;
            this.lsbTables.SelectedIndexChanged += new System.EventHandler(this.lsbTables_SelectedIndexChanged);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // toolStrip2
            // 
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.butSelectXml,
            this.toolStripSeparator1,
            this.toolStripLabel1,
            this.ddlEncoding,
            this.toolStripSeparator2,
            this.butSelectFolder});
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(568, 25);
            this.toolStrip2.TabIndex = 22;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // butSelectXml
            // 
            this.butSelectXml.BackColor = System.Drawing.SystemColors.Control;
            this.butSelectXml.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.butSelectXml.Image = ((System.Drawing.Image)(resources.GetObject("butSelectXml.Image")));
            this.butSelectXml.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butSelectXml.Name = "butSelectXml";
            this.butSelectXml.Size = new System.Drawing.Size(97, 22);
            this.butSelectXml.Text = "Select XML file...";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(57, 22);
            this.toolStripLabel1.Text = "Encoding";
            // 
            // ddlEncoding
            // 
            this.ddlEncoding.Name = "ddlEncoding";
            this.ddlEncoding.Size = new System.Drawing.Size(121, 25);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // butSelectFolder
            // 
            this.butSelectFolder.BackColor = System.Drawing.SystemColors.Control;
            this.butSelectFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.butSelectFolder.Enabled = false;
            this.butSelectFolder.Image = ((System.Drawing.Image)(resources.GetObject("butSelectFolder.Image")));
            this.butSelectFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butSelectFolder.Name = "butSelectFolder";
            this.butSelectFolder.Size = new System.Drawing.Size(100, 22);
            this.butSelectFolder.Text = "Convert to CSV...";
            // 
            // lblTableNames
            // 
            this.lblTableNames.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTableNames.ForeColor = System.Drawing.Color.SteelBlue;
            this.lblTableNames.Location = new System.Drawing.Point(0, 25);
            this.lblTableNames.Name = "lblTableNames";
            this.lblTableNames.Size = new System.Drawing.Size(568, 13);
            this.lblTableNames.TabIndex = 28;
            this.lblTableNames.Text = "Tables in XML file (select to see column names)";
            // 
            // gv
            // 
            this.gv.AllowUserToAddRows = false;
            this.gv.AllowUserToDeleteRows = false;
            this.gv.AllowUserToOrderColumns = true;
            this.gv.AllowUserToResizeRows = false;
            this.gv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gv.Dock = System.Windows.Forms.DockStyle.Top;
            this.gv.Location = new System.Drawing.Point(0, 198);
            this.gv.Name = "gv";
            this.gv.Size = new System.Drawing.Size(568, 41);
            this.gv.TabIndex = 31;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.ForeColor = System.Drawing.Color.SteelBlue;
            this.label1.Location = new System.Drawing.Point(0, 185);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(568, 13);
            this.label1.TabIndex = 32;
            this.label1.Text = "Column Names";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 481);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(568, 22);
            this.statusStrip1.TabIndex = 33;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 17);
            // 
            // ExportTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 503);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.txbLog);
            this.Controls.Add(this.gv);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lsbTables);
            this.Controls.Add(this.lblTableNames);
            this.Controls.Add(this.toolStrip2);
            this.MinimumSize = new System.Drawing.Size(380, 541);
            this.Name = "ExportTool";
            this.ShowIcon = false;
            this.Text = "ExportTool";
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gv)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txbLog;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ListBox lsbTables;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripButton butSelectXml;
        private System.Windows.Forms.Label lblTableNames;
        private System.Windows.Forms.DataGridView gv;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox ddlEncoding;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton butSelectFolder;
    }
}