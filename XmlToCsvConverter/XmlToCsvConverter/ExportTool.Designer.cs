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
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.butSelectXml = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.ddlEncoding = new System.Windows.Forms.ToolStripComboBox();
            this.lblTableNames = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lsbTables = new System.Windows.Forms.ListBox();
            this.lsbColumns = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.butSelectFolder = new System.Windows.Forms.Button();
            this.toolStrip2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txbLog
            // 
            this.txbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txbLog.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txbLog.ForeColor = System.Drawing.Color.Black;
            this.txbLog.Location = new System.Drawing.Point(0, 371);
            this.txbLog.Multiline = true;
            this.txbLog.Name = "txbLog";
            this.txbLog.ReadOnly = true;
            this.txbLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txbLog.Size = new System.Drawing.Size(584, 69);
            this.txbLog.TabIndex = 16;
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
            this.ddlEncoding});
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(584, 25);
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
            this.ddlEncoding.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ddlEncoding.Name = "ddlEncoding";
            this.ddlEncoding.Size = new System.Drawing.Size(121, 25);
            // 
            // lblTableNames
            // 
            this.lblTableNames.ForeColor = System.Drawing.Color.SteelBlue;
            this.lblTableNames.Location = new System.Drawing.Point(3, 6);
            this.lblTableNames.Name = "lblTableNames";
            this.lblTableNames.Size = new System.Drawing.Size(264, 18);
            this.lblTableNames.TabIndex = 28;
            this.lblTableNames.Text = "Tables in XML file (select to see column names)";
            // 
            // label1
            // 
            this.label1.ForeColor = System.Drawing.Color.SteelBlue;
            this.label1.Location = new System.Drawing.Point(295, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(286, 18);
            this.label1.TabIndex = 32;
            this.label1.Text = "Column Names";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 481);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(584, 22);
            this.statusStrip1.TabIndex = 33;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 17);
            // 
            // lsbTables
            // 
            this.lsbTables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lsbTables.FormattingEnabled = true;
            this.lsbTables.Location = new System.Drawing.Point(3, 27);
            this.lsbTables.Name = "lsbTables";
            this.lsbTables.Size = new System.Drawing.Size(286, 320);
            this.lsbTables.TabIndex = 10;
            this.lsbTables.SelectedIndexChanged += new System.EventHandler(this.lsbTables_SelectedIndexChanged);
            // 
            // lsbColumns
            // 
            this.lsbColumns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lsbColumns.FormattingEnabled = true;
            this.lsbColumns.Location = new System.Drawing.Point(295, 27);
            this.lsbColumns.Name = "lsbColumns";
            this.lsbColumns.Size = new System.Drawing.Size(286, 320);
            this.lsbColumns.TabIndex = 34;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.lblTableNames, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lsbColumns, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lsbTables, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 25);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(584, 346);
            this.tableLayoutPanel1.TabIndex = 35;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.butSelectFolder);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 440);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(0, 6, 12, 0);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(584, 41);
            this.flowLayoutPanel1.TabIndex = 36;
            // 
            // butSelectFolder
            // 
            this.butSelectFolder.AutoSize = true;
            this.butSelectFolder.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.butSelectFolder.Location = new System.Drawing.Point(470, 9);
            this.butSelectFolder.Name = "butSelectFolder";
            this.butSelectFolder.Size = new System.Drawing.Size(99, 23);
            this.butSelectFolder.TabIndex = 0;
            this.butSelectFolder.Text = "Convert to CSV...";
            this.butSelectFolder.UseVisualStyleBackColor = true;
            // 
            // ExportTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 503);
            this.Controls.Add(this.txbLog);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.toolStrip2);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.statusStrip1);
            this.MinimumSize = new System.Drawing.Size(600, 541);
            this.Name = "ExportTool";
            this.Text = "ExportTool";
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txbLog;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripButton butSelectXml;
        private System.Windows.Forms.Label lblTableNames;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox ddlEncoding;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ListBox lsbTables;
        private System.Windows.Forms.ListBox lsbColumns;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button butSelectFolder;
    }
}