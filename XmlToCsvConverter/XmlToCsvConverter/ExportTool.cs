using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Moor.XmlConversionLibrary.XmlToCsvStrategy;

namespace Moor.XmlToCsvConverter
{
    public partial class ExportTool : Form
    {
        private XmlToCsvContext _xmlToCsvContext;

        public ExportTool()
        {
            InitializeComponent();

            openFileDialog1.FileOk += OpenFileDialog1FileOk;
            saveFileDialog1.FileOk += SaveFileDialog1FileOk;

            butSelectFolder.Click += ButSelectFolderClick;
            chbSaveAllTables.CheckedChanged += ChbSaveAllTablesCheckedChanged;
            butSaveCsv.Click += ButSaveCsvClick;
            txbFilePath.TextChanged += TxbFilePathTextChanged;
            butSelectXml.Click += ButSelectXmlClick;
            Load += ExportTool_Load;
        }

        private void ExportTool_Load(object sender, EventArgs e)
        {
            PopulateEncodingOptions();
        }

        private void SaveFileDialog1FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                _xmlToCsvContext.Execute(@lsbTables.SelectedItem.ToString(), @saveFileDialog1.FileName,
                                         ((KeyValuePair<string, Encoding>) ddlEncoding.SelectedItem).Value);
                txbLog.Text += @"Saving  '" + lsbTables.SelectedItem + @"' to CSV completed." + Environment.NewLine;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(this, @"No table was selected. Please select a table.", @"No table selected error.",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                txbLog.Text += @"No table was selected. Please select a table." + Environment.NewLine;
            }
        }

        private void OpenFileDialog1FileOk(object sender, CancelEventArgs e)
        {
            lsbTables.Items.Clear();

            var taskA = Task.Factory.StartNew(() => OpenXmlFile(e));

            try
            {
                taskA.Wait();
            }
            catch (XmlException ex)
            {
                DialogResult result = MessageBox.Show(this,
                                                      @"Invalid XML file. Please make sure the XML file is XML-compliant.",
                                                      @"Invalid XML error", MessageBoxButtons.RetryCancel,
                                                      MessageBoxIcon.Error);
                txbLog.Text += @"Invalid XML file. Please make sure the XML file is XML-compliant. Opening file '" +
                               openFileDialog1.FileName + @"' did not complete." + Environment.NewLine;

                if (result == DialogResult.Retry)
                {
                    e.Cancel = true;
                }
            }
        }

        private void OpenXmlFile(CancelEventArgs e)
        {
            try
            {
                XmlToCsvUsingDataSet xmlToCsvUsingDataSet = new XmlToCsvUsingDataSet(openFileDialog1.FileName);

                _xmlToCsvContext = new XmlToCsvContext(xmlToCsvUsingDataSet);
                txbFilePath.Text = openFileDialog1.FileName;

                txbLog.Text += @"Opening file '" + openFileDialog1.FileName + @"' completed." + Environment.NewLine;

                foreach (string item in _xmlToCsvContext.Strategy.TableNameCollection)
                {
                    lsbTables.Items.Add(item);
                }
            }
            catch (XmlException ex)
            {
                DialogResult result = MessageBox.Show(this,
                                                      @"Invalid XML file. Please make sure the XML file is XML-compliant.",
                                                      @"Invalid XML error", MessageBoxButtons.RetryCancel,
                                                      MessageBoxIcon.Error);
                txbLog.Text += @"Invalid XML file. Please make sure the XML file is XML-compliant. Opening file '" +
                               openFileDialog1.FileName + @"' did not complete." + Environment.NewLine;

                if (result == DialogResult.Retry)
                {
                    e.Cancel = true;
                }
            }
            catch (ArgumentException ex)
            {
                DialogResult result = MessageBox.Show(this,
                                                      @"We have aparently an argument with this computer. He says: " + ex.Message,
                                                      @"Argument Exception", MessageBoxButtons.RetryCancel,
                                                      MessageBoxIcon.Error);
                txbLog.Text += @"Argument Execption: " + ex.Message + Environment.NewLine;

                if (result == DialogResult.Retry)
                {
                    e.Cancel = true;
                }
            }


        }

        private void ButSelectXmlClick(object sender, EventArgs e)
        {
            txbLog.Text += @"Opening file..." + Environment.NewLine;
            DialogResult result = openFileDialog1.ShowDialog(this);

            if (result == DialogResult.Cancel)
            {
                txbLog.Text += @"Opening file cancelled by user." + Environment.NewLine;
            }
        }

        private void TxbFilePathTextChanged(object sender, EventArgs e)
        {
            butSaveCsv.Enabled = !string.IsNullOrEmpty(txbFilePath.Text);
            chbSaveAllTables.Enabled = true;
        }

        private void ButSaveCsvClick(object sender, EventArgs e)
        {
            txbLog.Text += @"Starting XML to CSV conversion:" + Environment.NewLine;

            if (!chbSaveAllTables.Checked)
            {
                txbLog.Text += @"Convert single XML table" + Environment.NewLine;
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.DefaultExt = "csv";
                saveFileDialog1.Filter = @"CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialog1.ShowDialog(this);
            }
            else
            {
                if (string.IsNullOrEmpty(folderBrowserDialog1.SelectedPath))
                {
                    MessageBox.Show(this, @"No destination folder selected. Please select a destination folder.",
                                    @"No destination folder selected", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txbLog.Text += @"No destination folder selected. Please select a destination folder." +
                                   Environment.NewLine;
                }
                else
                {
                    foreach (string tableName in lsbTables.Items)
                    {
                        _xmlToCsvContext.Execute(tableName,
                                                 folderBrowserDialog1.SelectedPath + @"\\" + tableName + ".csv",
                                                 ((KeyValuePair<string, Encoding>) ddlEncoding.SelectedItem).Value);
                        txbLog.Text += @"Saving  '" + tableName + @"' to CSV completed." + Environment.NewLine;
                        txbLog.Refresh();
                    }

                    MessageBox.Show(@"Saving all tables completed succesfully.");
                }
            }
        }

        private void ChbSaveAllTablesCheckedChanged(object sender, EventArgs e)
        {
            lsbTables.Enabled = !chbSaveAllTables.Checked;
            butSelectFolder.Enabled = chbSaveAllTables.Checked;
            butSaveCsv.Enabled = !chbSaveAllTables.Checked;
        }

        private void ButSelectFolderClick(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                txbLog.Text += @"Destination folder for saving all tables: " + Environment.NewLine +
                               folderBrowserDialog1.SelectedPath + Environment.NewLine;
            }

            ButSaveCsvClick(sender, e);
        }

        private void PopulateEncodingOptions()
        {
            ddlEncoding.DisplayMember = "Key";

            var dic = new Dictionary<string, Encoding>
                          {
                              {"Default", Encoding.Default},
                              {"ASCII", Encoding.ASCII},
                              {"Unicode", Encoding.Unicode},
                              {"UTF8", Encoding.UTF8},
                              {"UTF32", Encoding.UTF32},
                              {"BigEndianUnicode", Encoding.BigEndianUnicode}
                          };


            foreach (var pair in dic)
            {
                ddlEncoding.Items.Add(pair);
            }

            ddlEncoding.SelectedIndex = 0;
        }
    }
}