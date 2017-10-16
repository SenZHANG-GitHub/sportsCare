using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;
using Microsoft.Win32;
using MSEXCEL = Microsoft.Office.Interop.Excel;
using System.Reflection;

namespace sportsCare
{
    /// <summary>
    /// ShowTable.xaml 的交互逻辑
    /// </summary>
    public partial class ShowTable : Window
    {
        private DataTable tbl = new DataTable();
        public ShowTable()
        {
            InitializeComponent();
        }

        public ShowTable(DataTable tbl1)
        {
            InitializeComponent();
            tbl = tbl1;
            GridView tgridview = (GridView)this.listview1.View;
            if (tbl1.Columns.Count == 2)
            {
                foreach (DataColumn col in tbl1.Columns)
                {
                    GridViewColumn gvc = new GridViewColumn();
                    gvc.Header = col.ColumnName;
                    gvc.Width = 200;
                    Binding binding = new Binding();
                    binding.Path = new PropertyPath(col.ColumnName);
                    gvc.DisplayMemberBinding = binding;
                    tgridview.Columns.Add(gvc);
                }
            }
            else
            {
                foreach (DataColumn col in tbl1.Columns)
                {
                    GridViewColumn gvc = new GridViewColumn();
                    gvc.Header = col.ColumnName;
                    Binding binding = new Binding();
                    binding.Path = new PropertyPath(col.ColumnName);
                    gvc.DisplayMemberBinding = binding;
                    tgridview.Columns.Add(gvc);
                }
            }

            this.listview1.ItemsSource = tbl1.DefaultView;
        }
        private void saveAsExcel_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog savedialog = new SaveFileDialog();
            savedialog.Filter = "Excel|*.xlsx|所有文件|*.*";
            savedialog.DefaultExt = "xlsx";
            savedialog.AddExtension = true;

            savedialog.OverwritePrompt = true;
            savedialog.Title = "导出";
            savedialog.ValidateNames = true;


            if (savedialog.ShowDialog().Value)
            {
                MSEXCEL.Application excelApp;
                MSEXCEL.Workbook excelDoc;
                excelApp = new MSEXCEL.ApplicationClass();
                excelDoc = excelApp.Workbooks.Add(System.Type.Missing);
                MSEXCEL.Worksheet excelWS = (MSEXCEL.Worksheet)excelDoc.Worksheets[1];

                for (int i = 0; i < tbl.Rows.Count; i++)
                {
                    for (int j = 0; j < tbl.Columns.Count; j++)
                    {
                        excelWS.Cells[i + 1, j + 1] = tbl.Rows[i][j].ToString();
                    }
                }

                excelDoc.SaveAs(savedialog.FileName);
                excelDoc.Close();
                excelApp.Quit();

                MessageBox.Show("文件已导出");
            }


        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
