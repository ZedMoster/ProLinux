using System;

using Autodesk.Revit.DB;

namespace Xml
{
    public static class HybhStringExtensions
    {
        /// <summary>
        /// 获取文字拼音首字母字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns> char 字符</returns>
        public static char FristChar(this string str)
        {
            char PYstr = char.MinValue;
            foreach (char item in str.ToCharArray())
            {
                if (Microsoft.International.Converters.PinYinConverter.ChineseChar.IsValidChar(item))
                {
                    Microsoft.International.Converters.PinYinConverter.ChineseChar cc = new Microsoft.International.Converters.PinYinConverter.ChineseChar(item);
                    PYstr = cc.Pinyins[0][0];
                }
                else
                {
                    PYstr = item.ToString()[0];
                }
            }
            return PYstr;
        }

        /// <summary>
        /// 字符串转数字
        /// </summary>
        /// <param name="value"> 数字字符串</param>
        /// <returns> double </returns>
        public static double ToDouble(this string value)
        {
            return Convert.ToDouble(value);
        }

        /// <summary>
        /// 字符串转整数
        /// </summary>
        /// <param name="value"> 整数字符串</param>
        /// <returns> int </returns>
        public static int ToInteger(this string value)
        {
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// 字符串转 ElementId
        /// </summary>
        /// <param name="value"> ElementId 字符串</param>
        /// <returns> ElementId </returns>
        public static ElementId ToElementId(this string value)
        {
            return new ElementId(ToInteger(value));
        }
    }
}
