using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace AGBIMMunicipalPipeline
{
    public static class DictExtensions
    {
        /// <summary>
        /// 定位点转换
        /// </summary>
        public static XYZ GetPointByEXP(this List<Dictionary<string, string>> keyValuePairs, string exp,
            double x = 0, double y = 0, double z = 0)
        {
            XYZ point = XYZ.Zero;

            foreach(var keyValue in keyValuePairs)
            {
                string EXP_NO = keyValue["EXP_NO"];
                if(exp == EXP_NO)
                {
                    point = keyValue.GetPoint(x, y, z);
                    break;
                }
            }

            return point;
        }

        /// <summary>
        /// 定位点转换
        /// </summary>
        public static XYZ GetPoint(this Dictionary<string, string> keyValue, double x = 0, double y = 0, double z = 0)
        {
            XYZ point = XYZ.Zero;
            string X = keyValue["X"];
            string Y = keyValue["Y"];
            string SUR_H = keyValue["SUR_H"];
            // 点转换
            try
            {
                point = new XYZ((X.ToFloat() - x) / 0.3048, (Y.ToFloat() - y) / 0.3048, (SUR_H.ToFloat() - z) / 0.3048);
            }
            catch(Exception e)
            {
                _ = e.Message;
            }

            return point;
        }

        /// <summary>
        /// 字符串转数字
        /// </summary>
        public static double ToFloat(this string value, bool mmtoFeet = false)
        {
            return mmtoFeet ? Convert.ToDouble(value) / 304.8 : Convert.ToDouble(value);
        }

        public static string GetValue(this Dictionary<string, string> keyValue, string key)
        {
            keyValue.TryGetValue(key, out string value);
            return value;
        }
    }
}
