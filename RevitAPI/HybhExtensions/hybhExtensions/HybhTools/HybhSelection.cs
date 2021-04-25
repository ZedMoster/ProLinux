using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace Xml
{
    /// <summary>
    /// 选择指定类型名称的图元
    /// </summary>
    public class SelectionFilterByCategory : ISelectionFilter
    {
        /// <summary>
        /// 可选类别名称
        /// </summary>
        public string CategoryName { get; set; }
        public SelectionFilterByCategory(string categoryName)
        {
            CategoryName = categoryName;
        }
        public bool AllowElement(Element el)
        {
            return el.Category.Name == CategoryName;
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }

    /// <summary>
    /// 选择指定列表类别名称的图元
    /// </summary>
    public class SelectionFilterByCategoryList : ISelectionFilter
    {
        /// <summary>
        /// 可选类别名称列表
        /// </summary>
        public List<string> CategoryNameList { get; set; }
        public SelectionFilterByCategoryList(List<string> categoryNameList)
        {
            CategoryNameList = categoryNameList;
        }
        public bool AllowElement(Element el)
        {
            return CategoryNameList.Any(i => i == el.Category.Name);
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }

    /// <summary>
    /// 选择模型类别图元
    /// </summary>
    public class SelectionFilterCategoryEndsWith : ISelectionFilter
    {
        public string CategoryNameEndsWith { get; set; }
        public SelectionFilterCategoryEndsWith(string endsWith = ".dwg")
        {
            CategoryNameEndsWith = endsWith;
        }
        public bool AllowElement(Element el)
        {
            // 重复导入图纸情况 *.dwg2 
            return el.Category.Name.ToLower().EndsWith(".dwg");
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}
