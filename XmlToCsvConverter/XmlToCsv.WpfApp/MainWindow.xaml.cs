using System;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using Microsoft.Win32;
using Moor.XmlConversionLibrary.XmlToCsvStrategy;

namespace XmlToCsv.WpfApp
{
    public partial class MainWindow : Window
    {
        private XmlToCsvContext _xmlToCsvContext;

        public DataSet XmlDataSet
        {
            get
            {
                var converter = (XmlToCsvUsingDataSet)_xmlToCsvContext.Strategy;
                return converter.XmlDataSet;
            }
        }

        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                txbSelectedXmlFile.Text = _fileName;
            }
        }

        public DataColumn SelectedDataColumn { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            trv.MouseDoubleClick += trv_MouseDoubleClick;
        }

        void trv_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var item = (TreeViewItem)trv.SelectedItem;
            item.IsExpanded = true;

            var textBox = new TextBox();
            string oldName = textBox.Text;

            item.Header = textBox;

            if (item.Tag.GetType() == typeof(DataTable))
            {
                var table = (DataTable)item.Tag;
                textBox.Text = table.TableName;

                textBox.LostFocus += (o, ea) =>
                               {
                                   item.Header = textBox.Text;
                                   table.TableName = textBox.Text;
                               };
                textBox.PreviewKeyDown += (o, ea) =>
                {
                    if (ea.Key == Key.Return)
                    {
                        item.Header = textBox.Text;
                        table.TableName = textBox.Text;
                        ea.Handled = true;
                    }
                };

                CreateElementNameChangeLogEntry(table.TableName, oldName);
            }
            else if (item.Tag.GetType() == typeof(DataColumn))
            {
                var col = (DataColumn)item.Tag;
                textBox.Text = col.ColumnName;

                textBox.LostFocus += (o, ea) =>
                {
                    item.Header = textBox.Text;
                    col.ColumnName = textBox.Text;
                };

                textBox.PreviewKeyDown += (o, ea) =>
                {
                    if (ea.Key == Key.Return)
                    {
                        item.Header = textBox.Text;
                        col.ColumnName = textBox.Text;
                        ea.Handled = true;
                    }
                };

                CreateElementNameChangeLogEntry(col.ColumnName, oldName);
            }


        }

        private void CreateElementNameChangeLogEntry(string elementName, string oldName)
        {
            txbLog.Text += string.Format("Saved changes column name from {0} to {1}{2}", oldName, elementName, Environment.NewLine);
        }

        private void butOpenFile_Click(object sender, RoutedEventArgs e)
        {
            txbLog.Text = string.Empty;

            var fileDialog = new OpenFileDialog();

            bool? result = fileDialog.ShowDialog();

            if (result == true)
            {
                FileName = fileDialog.FileName;

                try
                {
                    OpenXmlFile();
                }
                catch (XmlException)
                {
                    const string messageBoxText = "Invalid XML file. Please make sure the XML file is XML-compliant.";
                    const string caption = "Invalid XML error";
                    MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Error);
                    txbLog.Text = messageBoxText + " Opening file '" + FileName + "' did not complete." + Environment.NewLine;
                }
            }
        }

        private void OpenXmlFile(bool autoRenameWhenNamingConflict = false)
        {
            try
            {
                using (var xmlToCsvUsingDataSet = new XmlToCsvUsingDataSet(FileName, autoRenameWhenNamingConflict))
                {
                    _xmlToCsvContext = new XmlToCsvContext(xmlToCsvUsingDataSet);
                }

                txbLog.Text += @"-Opening file '" + FileName + @"' completed." + Environment.NewLine;
                CreateTreeview();
            }
            catch (DuplicateNameException ex)
            {
                txbLog.Text += @"-DuplicateName Execption: " + ex.Message + Environment.NewLine;
                const string messageBoxText = @"The XML data contains conflicting names between table and column names. Do you want to continue by renaming the conflicting elements in the resulting CSV?";
                const string caption = @"Duplicate Name Conflict";
                var result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    OpenXmlFile(true);
                    txbLog.Text += @"-DuplicateName Execption resolved by auto-renaming conflicting table name: " + Environment.NewLine;
                }
            }
            catch (XmlException)
            {
                const string messageBoxText = "-Invalid XML file. Please make sure the XML file is XML-compliant.";
                const string caption = "Invalid XML error";
                txbLog.Text = messageBoxText + " Opening file '" + FileName + "' did not complete." + Environment.NewLine;
                MessageBox.Show(txbLog.Text, caption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ArgumentException ex)
            {
                const string messageBoxText = "-An argument provided is invalid. The error message: ";
                const string caption = "Argument Exception";
                txbLog.Text = messageBoxText + " Argument Execption: " + ex.Message + Environment.NewLine;
                MessageBox.Show(txbLog.Text, caption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidOperationException ex)
            {
                string messageBoxText = "-Invalid operation. The error message: " + ex.Message;
                const string caption = "Invalid operation";
                txbLog.Text = messageBoxText + Environment.NewLine;
                MessageBox.Show(txbLog.Text, caption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateTreeview()
        {
            //trv.ItemsSource = XmlDataSet.Tables;
            trv.Items.Clear();
            foreach (DataTable table in XmlDataSet.Tables)
            {
                var treeItem = new TreeViewItem { Tag = table, Header = table.TableName, IsExpanded = true };
                trv.Items.Add(treeItem);

                foreach (DataColumn col in table.Columns)
                {
                    var itemColumn = new TreeViewItem { Tag = col, Header = col.ColumnName, IsExpanded = true };
                    treeItem.Items.Add(itemColumn);
                }
            }
        }

        private void trv_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = ((TreeViewItem)e.NewValue);

            if (item != null && item.Tag.GetType() == typeof(DataColumn))
            {
                var col = (DataColumn)item.Tag;
                txbValue.Text = col.ColumnName;
                SelectedDataColumn = col;
            }
        }

        private void butSaveColumnName_Click(object sender, RoutedEventArgs e)
        {
            var item = (DataColumn)((TreeViewItem)trv.SelectedItem).Tag;
            string oldName = item.ColumnName;
            item.ColumnName = txbValue.Text;

            CreateElementNameChangeLogEntry(item.ColumnName, oldName);
            CreateTreeview();
        }

        private void txbValue_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
    }


}