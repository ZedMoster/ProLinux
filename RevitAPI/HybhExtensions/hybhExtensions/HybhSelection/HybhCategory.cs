using Autodesk.Revit.DB;

namespace Xml
{
    public static class HybhCategoryExtensions
    {
        /// <summary>
        /// 获取分类名称
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="builtInCategory"></param>
        /// <returns> 获取 BuiltInCategory 分类名称</returns>
        public static string CategoryName(this BuiltInCategory builtInCategory, Document doc)
        {
            return doc.Settings.Categories.get_Item(builtInCategory).Name;
        }
    }
}
