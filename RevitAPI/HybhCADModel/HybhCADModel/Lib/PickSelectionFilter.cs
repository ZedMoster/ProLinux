using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace HybhCADModel.Lib
{
    /// <summary>
    /// 选择图纸
    /// </summary>
    class FilterCAD : ISelectionFilter
    {
        public bool AllowElement(Element el)
        {
            return el.Category.Name.ToLower().EndsWith(".dwg");
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }

    /// <summary>
    /// 指定类别名称
    /// </summary>
    class FilterCategory : ISelectionFilter
    {
        private string CategoryName { get; }
        public FilterCategory(string cate)
        {
            CategoryName = cate;
        }
        public bool AllowElement(Element elem)
        {
            return elem.Category.Name == CategoryName;
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
