using Autodesk.Revit.DB;

namespace HybhCADModel.Extensions
{
    public static class ElementExtensions
    {
        /// <summary>
        /// ElementId 转 Element
        /// </summary>
        public static Element ToElement(this ElementId elementId, Document doc)
        {
            return doc.GetElement(elementId);
        }

        /// <summary>
        /// Reference 转 Element
        /// </summary>
        public static Element ToElement(this Reference reference, Document doc)
        {
            return doc.GetElement(reference);
        }
    }
}
