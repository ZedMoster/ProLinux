using System.Collections.Generic;
using System.IO;

using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace ConsoleApp1
{
    public class ReadExcel
    {
        public string FilePath { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="filepath"> excel 文件位置</param>
        public ReadExcel(string filepath)
        {
            FilePath = filepath;
        }

        /// <summary>
        /// 读取excel获取DataTable
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, string>>> GetDataTable()
        {
            if(!FilePath.EndsWith(".xlsx"))
            {
                throw new System.Exception("file is not than 2010 excel file, beacause not endwith 'xlsx'. ");
            }

            Dictionary<string, List<Dictionary<string, string>>> excelData = new Dictionary<string, List<Dictionary<string, string>>>();

            using(FileStream fs = File.OpenRead(FilePath))
            {
                IWorkbook wk = new XSSFWorkbook(fs);

                #region 读取页

                for(int i = 0; i < wk.NumberOfSheets; i++)
                {
                    var sheet = wk.GetSheetAt(i);

                    #region 获取excel表头数据
                    List<string> rowNames = new List<string>();
                    IRow _row_names = sheet.GetRow(0);
                    for(int s = 0; s < _row_names.LastCellNum; s++)
                    {
                        ICell cell = _row_names.GetCell(s);
                        rowNames.Add(cell.StringCellValue);
                    }

                    #endregion

                    List<Dictionary<string, string>> sheetData = new List<Dictionary<string, string>>();
                    for(int j = 1; j <= sheet.LastRowNum; j++)
                    {
                        // 读取当前行数据
                        Dictionary<string, string> keyValue = new Dictionary<string, string>();
                        IRow row = sheet.GetRow(j);
                        if(row == null)
                        {
                            continue;
                        }

                        for(int k = 0; k <= row.LastCellNum; k++)
                        {
                            // 读取当前行对应的列数据
                            ICell cell = row.GetCell(k);
                            if(cell != null)
                            {
                                keyValue.Add(rowNames[k], cell.ToString());
                            }
 
                        }
                        sheetData.Add(keyValue);
                    }

                    excelData.Add(sheet.SheetName, sheetData);
                }

                #endregion
            }

            return excelData;
        }
    }
}
