using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

            openFileDialog1.FileOk += openFileDialog1_FileOk;
            saveFileDialog1.FileOk += saveFileDialog1_FileOk;

            butSelectFolder.Click += butSelectFolder_Click;
            chbSaveAllTables.CheckedChanged += chbSaveAllTables_CheckedChanged;
            butSaveCsv.Click += butSaveCsv_Click;
            txbFilePath.TextChanged += txbFilePath_TextChanged;
            butSelectXml.Click += butSelectXml_Click;
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                _xmlToCsvContext.Execute(@lsbTables.SelectedItem.ToString(), @saveFileDialog1.FileName);
                txbLog.Text += "Saving  '" + lsbTables.SelectedItem + "' to CSV completed." + Environment.NewLine;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(this, "No table was selected. Please select a table.", "No table selected error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txbLog.Text += "No table was selected. Please select a table." + Environment.NewLine;
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            lsbTables.Items.Clear();

            try
            {
                Thread backgroundThread = new Thread(OpenXmlFile);
                backgroundThread.Start();
            }
            catch (XmlException)
            {
                DialogResult result = MessageBox.Show(this, "Invalid XML file. Please make sure the XML file is XML-compliant.", "Invalid XML error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                txbLog.Text += "Invalid XML file. Please make sure the XML file is XML-compliant. Opening file '" + openFileDialog1.FileName + "' did not complete." + Environment.NewLine;

                if (result == DialogResult.Retry)
                {
                    e.Cancel = true;
                }
            }
        }

        private void OpenXmlFile()
        {
            _xmlToCsvContext = new XmlToCsvContext(new XmlToCsvUsingDataSet(openFileDialog1.FileName));
            txbFilePath.Text = openFileDialog1.FileName;

            txbLog.Text += "Opening file '" + openFileDialog1.FileName + "' completed." + Environment.NewLine;

            foreach (var item in _xmlToCsvContext.Strategy.TableNameCollection)
            {
                lsbTables.Items.Add(item);
            }
        }

        private void butSelectXml_Click(object sender, EventArgs e)
        {
            txbLog.Text += "Opening file..." + Environment.NewLine;
            DialogResult result = openFileDialog1.ShowDialog(this);

            if (result == DialogResult.Cancel)
            {
                txbLog.Text += "Opening file cancelled by user." + Environment.NewLine;
            }
        }

        private void txbFilePath_TextChanged(object sender, EventArgs e)
        {
            butSaveCsv.Enabled = !string.IsNullOrEmpty(txbFilePath.Text);
            chbSaveAllTables.Enabled = true;
        }

        private void butSaveCsv_Click(object sender, EventArgs e)
        {
            txbLog.Text += "Starting XML to CSV conversion:" + Environment.NewLine;

            if (!chbSaveAllTables.Checked)
            {
                txbLog.Text += "Convert single XML table" + Environment.NewLine;
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.DefaultExt = "csv";
                saveFileDialog1.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialog1.ShowDialog(this);
            }
            else
            {
                if (string.IsNullOrEmpty(folderBrowserDialog1.SelectedPath))
                {
                    MessageBox.Show(this, "No destination folder selected. Please select a destination folder.",
                                    "No destination folder selected", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txbLog.Text += "No destination folder selected. Please select a destination folder." +
                                   Environment.NewLine;
                }
                else
                {
                    foreach (string tableName in lsbTables.Items)
                    {
                        _xmlToCsvContext.Execute(tableName, folderBrowserDialog1.SelectedPath + @"\\" + tableName + ".csv");
                        txbLog.Text += "Saving  '" + tableName + "' to CSV completed." + Environment.NewLine;
                        txbLog.Refresh();
                    }

                    MessageBox.Show("Saving all tables completed succesfully.");
                }
            }
        }

        private void chbSaveAllTables_CheckedChanged(object sender, EventArgs e)
        {
            lsbTables.Enabled = !chbSaveAllTables.Checked;
            butSelectFolder.Enabled = chbSaveAllTables.Checked;
        }

        private void butSelectFolder_Click(object sender, EventArgs e)
        {

            DialogResult result = folderBrowserDialog1.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                txbLog.Text += "Destination folder for saving all tables: " + Environment.NewLine + folderBrowserDialog1.SelectedPath + Environment.NewLine;
            }
        }
    }
}
