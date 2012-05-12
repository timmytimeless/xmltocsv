using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Linq;
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
                                         ((KeyValuePair<string, Encoding>)ddlEncoding.SelectedItem).Value);
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

            try
            {
                OpenXmlFile(e);
            }
            catch (XmlException)
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

        private void OpenXmlFile(CancelEventArgs e, bool autoRenameWhenNamingConflict = false)
        {
            try
            {
                using (var xmlToCsvUsingDataSet = new XmlToCsvUsingDataSet(openFileDialog1.FileName, autoRenameWhenNamingConflict))
                {
                    _xmlToCsvContext = new XmlToCsvContext(xmlToCsvUsingDataSet);
                }

                lblStatus.Text = openFileDialog1.FileName;

                txbLog.Text += @"Opening file '" + openFileDialog1.FileName + @"' completed." + Environment.NewLine;

                foreach (string item in _xmlToCsvContext.Strategy.TableNameCollection)
                {
                    lsbTables.Items.Add(item);
                }

                butSelectFolder.Enabled = lsbTables.Items.Count > 0;
            }
            catch (DuplicateNameException ex)
            {
                DialogResult result = MessageBox.Show(@"The XML data contains conflicting names between table and column names. Do you want to continue by renaming the conflicting elements in the resulting CSV?",
                    @"Duplicate Name Conflict", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                {
                    //e.Cancel = true;
                    OpenXmlFile(e, true);
                }

                txbLog.Text += @"DuplicateName Execption: " + ex.Message + Environment.NewLine;
            }
            catch (XmlException)
            {
                DialogResult result = MessageBox.Show(this,
                                                      @"Invalid XML file. Please make sure the XML file is XML-compliant.",
                                                      @"Invalid XML error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
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
                                                      @"An argument provided is invalid. The error message: " + ex.Message,
                                                      @"Argument Exception", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                txbLog.Text += @"Argument Execption: " + ex.Message + Environment.NewLine;

                if (result == DialogResult.Retry)
                {
                    e.Cancel = true;
                }
            }
            catch (InvalidOperationException ex)
            {
                DialogResult result = MessageBox.Show(this,
                                                      @"Invalid operation. The error message: " + ex.Message,
                                                      @"Invalid operation", MessageBoxButtons.RetryCancel,
                                                      MessageBoxIcon.Error);
                txbLog.Text += @"Invalid Operation Execption: " + ex.Message + Environment.NewLine;

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

        private void Save()
        {
            txbLog.Text += @"Starting XML to CSV conversion:" + Environment.NewLine;

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
                    _xmlToCsvContext.Execute(tableName, folderBrowserDialog1.SelectedPath + @"\\" + tableName + ".csv", GetEncoding(ddlEncoding.SelectedItem.ToString()));
                    txbLog.Text += @"Saving  '" + tableName + @"' to CSV completed." + Environment.NewLine;
                    txbLog.Refresh();
                }

                MessageBox.Show(@"Saving tables in XML to CSV document completed succesfully.");
            }
        }

        private void ButSelectFolderClick(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                txbLog.Text += @"Destination folder for saving all tables: " + Environment.NewLine +
                               folderBrowserDialog1.SelectedPath + Environment.NewLine;

                Save();
            }
        }

        private void PopulateEncodingOptions()
        {
            var dic = new List<string> { "Default", "ASCII", "Unicode", "UTF8", "UTF32", "BigEndianUnicode" };

            foreach (var pair in dic)
            {
                ddlEncoding.Items.Add(pair);
            }

            ddlEncoding.SelectedIndex = 0;
        }

        private static Encoding GetEncoding(string name)
        {
            if (name == "Default" || name == "UTF8")
            {
                return Encoding.UTF8;
            }

            if (name == "ASCII")
            {
                return Encoding.ASCII;
            }

            if (name == "Unicode")
            {
                return Encoding.Unicode;
            }

            if (name == "UTF32")
            {
                return Encoding.UTF32;
            }

            if (name == "BigEndianUnicode")
            {
                return Encoding.BigEndianUnicode;
            }

            throw new ArgumentOutOfRangeException("name", @"No valid encoding selected");
        }

        private void lsbTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            lsbColumns.Items.Clear();

            var ds = (DataSet)((dynamic)_xmlToCsvContext.Strategy).XmlDataSet;
            DataTable selectedTable = ds.Tables.Cast<DataTable>().Single(n => n.TableName == lsbTables.SelectedItem.ToString());

            foreach (DataColumn column in selectedTable.Columns)
            {
                lsbColumns.Items.Add(column.ColumnName);
            }
        }
    }
}