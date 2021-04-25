using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

using Xml.HybhModel;

namespace Xml.HybhLib
{
    public class HybhNPOIExcel
    {
        /// <summary>
        /// 读取excel
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<ExcelDateBase> OpenExcel(string path, string SheetName)
        {
            List<ExcelDateBase> data = new List<ExcelDateBase>();
            try
            {
                using (FileStream fs = File.OpenRead(path))
                {
                    IWorkbook wk = new XSSFWorkbook(fs);
                    ISheet sheet = wk.GetSheetAt(0);
                    for (int i = 0; i < wk.NumberOfSheets; i++)
                    {
                        if (wk.GetSheetAt(i).SheetName == SheetName)
                        {
                            sheet = wk.GetSheetAt(i);
                            break;
                        }
                    }
                    if (sheet.SheetName != SheetName)
                    {
                        MessageBox.Show($"未找到名称{SheetName}的页表！");
                        return data;
                    }

                    for (int j = 0; j <= sheet.LastRowNum; j++)
                    {
                        //读取当前行数据
                        IRow row = sheet.GetRow(j);
                        if (row != null)
                        {
                            List<string> _row = new List<string>();
                            for (int k = 0; k <= row.LastCellNum; k++)
                            {
                                //读取当前列数据
                                ICell cell = row.GetCell(k);
                                if (cell != null)
                                {
                                    _row.Add(cell.ToString());
                                }
                                else
                                {
                                    _row.Add(string.Empty);
                                }
                            }
                            data.Add(new ExcelDateBase() { RowData = _row, Count = _row.Count });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("警告", "文件被其他应用打开，请关闭对该文件的引用\n" + e.Message);
            }
            return data;
        }
    }
}
