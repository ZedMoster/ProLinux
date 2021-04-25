using System.Collections.Generic;

namespace Xml.HybhModel
{
    public class ExcelDateBase
    {
        /// <summary>
        /// 列数据计数
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// 页名
        /// </summary>
        public string SheetName { get; set; }
        /// <summary>
        /// 行数据列表
        /// </summary>
        public List<string> RowData { get; set; }
    }
}
