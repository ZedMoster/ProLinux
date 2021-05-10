using System.Collections.Generic;
using System.IO;

using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace HelperExcel
{
    /// <summary>HelperExcel.Extensions
    /// 读取带表头的表格数据
    /// </summary>
    public class ReadExcel
    {
        /// <summary>
        /// 文件位置
        /// </summary>
        private string FilePath { get; set; }

        /// <summary>
        /// 读取表格数据
        /// </summary>
        /// <param name="filepath"> excel 文件位置</param>
        public ReadExcel(string filepath)
        {
            FilePath = filepath;
        }

        /// <summary>
        /// 读取excel获取DataTable
        /// </summary>
        /// <returns> 读取excel 数据，返回 Dictionary 数据</returns>
        public Dictionary<string, List<Dictionary<string, string>>> GetDataTableToDict(string sheetname = null)
        {
            // 判断文件是否存在
            if(!System.IO.File.Exists(FilePath))
            {
                throw new System.Exception($"文件不存在：{FilePath}");
            }

            // 初始化结构数据
            Dictionary<string, List<Dictionary<string, string>>> excelData = new Dictionary<string, List<Dictionary<string, string>>>();
            // 读取表格内容
            using(FileStream fs = File.OpenRead(FilePath))
            {
                IWorkbook wk = new XSSFWorkbook(fs);

                if(string.IsNullOrWhiteSpace(sheetname))
                {
                    // 获取每一页数据
                    for(int i = 0; i < wk.NumberOfSheets; i++)
                    {
                        var _sheet = wk.GetSheetAt(i);
                        var sheetData = GetSheetData(_sheet);
                        excelData.Add(_sheet.SheetName, sheetData);
                    }
                }
                else
                {
                    // 获取指定页数据
                    ISheet sheet = null;
                    for(int i = 0; i < wk.NumberOfSheets; i++)
                    {
                        sheet = wk.GetSheetAt(i);
                        if(sheet.SheetName == sheetname)
                        {
                            break;
                        }
                    }
                    if(sheet == null)
                    {
                        throw new System.Exception($"确认Excel文件中存在页名：{sheetname}");
                    }
                    var sheetData = GetSheetData(sheet);
                    excelData.Add(sheet.SheetName, sheetData);
                }
            }

            return excelData;
        }

        /// <summary>
        /// 获取页数据
        /// </summary>
        /// <param name="sheet"> ISheet接口字段</param>
        /// <returns> 页数据：按首行名称格式化</returns>
        private List<Dictionary<string, string>> GetSheetData(ISheet sheet)
        {
            // 获取首行数据为标签名称
            List<string> rowNames = new List<string>();

            #region 获取首行做为键值对的键
            IRow _row_names = sheet.GetRow(0);
            for(int s = 0; s < _row_names.LastCellNum; s++)
            {
                ICell cell = _row_names.GetCell(s);
                rowNames.Add(cell.StringCellValue);
            }
            #endregion

            // 获取sheet页的数据集
            List<Dictionary<string, string>> sheetData = new List<Dictionary<string, string>>();

            #region 获取行数据
            for(int j = 1; j <= sheet.LastRowNum; j++)
            {
                // 每一行的键值对
                Dictionary<string, string> keyValue = new Dictionary<string, string>();
                IRow row = sheet.GetRow(j);
                if(row == null)
                {
                    continue;
                }
                for(int k = 0; k <= rowNames.Count - 1; k++)
                {
                    string value = "";
                    ICell cell = row.GetCell(k);
                    if(cell != null)
                    {
                        if(cell.CellType != CellType.Formula)
                        {
                            value = cell.ToString();
                        }
                        else
                        {
                            value = cell.NumericCellValue.ToString();
                        }
                    }
                    // 获取列名称及单元格值
                    keyValue.Add(rowNames[k], value);
                }
                // 添加行 数据
                sheetData.Add(keyValue);
            }
            #endregion

            return sheetData;
        }
    }
}
