using System.Collections.Generic;

using Newtonsoft.Json;

namespace HelperExcel.Extensions
{
    /// <summary>
    /// 格式化字典内容到json字符串
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// 读取excel获取DataTable
        /// </summary>
        /// <returns> 读取excel 数据, 返回jsonStirng </returns>
        public static string ToJsonString(this Dictionary<string, List<Dictionary<string, string>>> excelData)
        {
            // 格式化 Json 数据
            return JsonConvert.SerializeObject(excelData);
        }
    }
}
