using System;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
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
        }

        /// <summary>
        /// 更改xlsx 表格单元格 颜色
        /// </summary>
        /// <param name="row"> 行号(起始:0) 不考虑列名行</param>
        /// <param name="column"> 列号(起始:0)</param>
        public void SetColorString(int row, int column = 0)
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
                //创建字体
                IFont font = Workbook.CreateFont();
                //红色
                font.Color = HSSFColor.Red.Index;
                font.FontHeight = 12;
                font.FontName = "宋体";
                font.IsBold = true;
                //样式
                ICellStyle style = Workbook.CreateCellStyle();
                //给样式设置字体
                style.SetFont(font);
                style.Alignment = HorizontalAlignment.Center;
                style.VerticalAlignment = VerticalAlignment.Center;
                // 更新颜色
                cell.CellStyle = style;
            }
            // 写入文件
            using(FileStream fs = new FileStream(ExcelPath, FileMode.Create, FileAccess.Write))
            {
                Workbook.Write(fs);
            }
        }
    }
}
