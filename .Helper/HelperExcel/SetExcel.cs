using System;
using System.IO;

using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace HelperExcel
{
    /// <summary>
    /// 修改表格单元格数据
    /// </summary>
    public class SetExcel
    {
        /// <summary>
        /// 文件地址
        /// </summary>
        private string ExcelPath { get; set; }

        /// <summary>
        /// 表页名称
        /// </summary>
        private ISheet Sheet { get; set; }

        /// <summary>
        /// 表数据
        /// </summary>
        private IWorkbook Workbook { get; set; }

        /// <summary>
        /// 行数
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// 修改表格单元格数据
        /// </summary>
        /// <param name="excelPath"> 文件地址</param>
        /// <param name="sheetname"> 表页名称</param>
        public SetExcel(string excelPath, string sheetname)
        {
            ExcelPath = excelPath ?? throw new ArgumentNullException(nameof(excelPath));

            // 判断文件是否存在
            if(!System.IO.File.Exists(ExcelPath))
            {
                throw new System.Exception($"本机不存在此文件内容:{ExcelPath}");
            }

            // 读取文件
            using(FileStream fs = new FileStream(ExcelPath, FileMode.Open, FileAccess.Read))
            {
                Workbook = new XSSFWorkbook(fs);
            }
            // 获取表格页数据
            Sheet = Workbook.GetSheet(sheetname);
            if(Sheet == null)
            {
                throw new System.Exception($"当前表格中不包含页名称:{sheetname}");
            }
            // 获取表格最大行数
            RowNumber = Sheet.LastRowNum;
        }

        /// <summary>
        /// 更改xlsx 表格单元格 内容
        /// </summary>
        /// <param name="value"> 修改后的字符串</param>
        /// <param name="row"> 行号(起始:0) 不考虑列名行</param>
        /// <param name="column"> 列号(起始:0)</param>
        public void SetValueString(string value, int row, int column = 0)
        {
            // 读取当前表数据 删除列名
            Console.WriteLine(Sheet);
            var _row = Sheet.GetRow(row + 1);
            if(_row == null)
            {
                throw new Exception("表格行为空");
            }
            ICell cell = _row.GetCell(column);
            if(cell != null)
            {
                // 更新单元格内容
                cell.SetCellValue(value);
            }
            else
            {
                // 新增单元格
                _row.CreateCell(column).SetCellValue(value);
            }

            // 写入文件
            using(FileStream fs = new FileStream(ExcelPath, FileMode.Create, FileAccess.Write))
            {
                Workbook.Write(fs);
            }

            //Console.WriteLine("Good job");
        }
    }
}
