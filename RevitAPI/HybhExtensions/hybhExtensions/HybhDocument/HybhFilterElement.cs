using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace Xml
{
    public static class FilterElementExtensions
    {
        /// <summary>
        /// 过滤器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static List<T> TCollector<T>(this Document doc)
        {
            return new FilteredElementCollector(doc).OfClass(typeof(T)).Cast<T>().ToList();
        }

        /// <summary>
        /// 过滤器-当前视图
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="doc"></param>
        /// <param name="viewId"></param>
        /// <returns></returns>
        public static List<T> TCollector<T>(this Document doc, ElementId viewId)
        {
            return new FilteredElementCollector(doc, viewId).OfClass(typeof(T)).Cast<T>().ToList();
        }

        /// <summary>
        /// 过滤器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="doc"></param>
        /// /// <param name="builtInCategory"></param>
        /// <returns></returns>
        public static List<T> TCollector<T>(this Document doc, BuiltInCategory builtInCategory)
        {
            return new FilteredElementCollector(doc).OfClass(typeof(T)).OfCategory(builtInCategory).Cast<T>().ToList();
        }

        /// <summary>
        /// 过滤器-当前视图
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="doc"></param>
        /// <param name="viewId"></param>
        /// <param name="builtInCategory"></param>
        /// <returns></returns>
        public static List<T> TCollector<T>(this Document doc, ElementId viewId, BuiltInCategory builtInCategory)
        {
            return new FilteredElementCollector(doc, viewId).OfClass(typeof(T)).OfCategory(builtInCategory).Cast<T>().ToList();
        }
    }
}
