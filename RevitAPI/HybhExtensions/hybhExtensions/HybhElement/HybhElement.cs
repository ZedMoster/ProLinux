using Autodesk.Revit.DB;

namespace Xml
{
    public static class ElementExtensions
    {
        /// <summary>
        /// 通过名称更新参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="el"></param>
        /// <param name="parameterName"></param>
        /// <param name="parameterValue"></param>
        /// <returns> 是否成功更新参数</returns>
        public static bool SetParameterByName<T>(this Element el, string parameterName, T parameterValue)
        {
            bool push = false;
            var manager = el.LookupParameter(parameterName);
            if (manager != null)
            {
                if (parameterValue is double para_double)
                {
                    manager.Set(para_double);
                }
                else if (parameterValue is int para_int)
                {
                    manager.Set(para_int);
                }
                else if (parameterValue is string para_string)
                {
                    manager.Set(para_string);
                }
                else if (parameterValue is ElementId para_ElementId)
                {
                    manager.Set(para_ElementId);
                }
                else
                {
                    ShowResult.Print($"Element对象参数 {parameterName} 更新失败");
                    return push;
                }
                push = true;
            }
            return push;
        }

        /// <summary>
        /// 通过名称获取参数值
        /// </summary>
        /// <param name="el"> 对象</param>
        /// <param name="parameterName"> 参数名称</param>
        /// <returns> string:参数名称对应的参数值 </returns>
        public static string GetParameterValueByName(this Element el, string parameterName)
        {
            var parameter = el.LookupParameter(parameterName);
            if (parameter == null)
            {
                ShowResult.Print($"Element对象参数 {parameterName} 名称不存在");
                return null;
            }
            switch (parameter.StorageType)
            {
                case StorageType.Double:
                    return parameter.AsValueString();
                case StorageType.ElementId:
                    return parameter.AsElementId().IntegerValue.ToString();
                case StorageType.Integer:
                    return parameter.AsValueString();
                case StorageType.None:
                    return parameter.AsValueString();
                case StorageType.String:
                    return parameter.AsString();
                default:
                    return null;
            }
        }

        /// <summary>
        /// 获取模型的定位点
        /// </summary>
        /// <returns></returns>
        public static XYZ GetLocationPoint(this Element el)
        {
            return (el.Location as LocationPoint)?.Point;
        }

        /// <summary>
        /// 获取模型的定位线
        /// </summary>
        /// <returns></returns>
        public static Curve GetLocationCurve(this Element el)
        {
            return (el.Location as LocationCurve)?.Curve;
        }
    }
}
