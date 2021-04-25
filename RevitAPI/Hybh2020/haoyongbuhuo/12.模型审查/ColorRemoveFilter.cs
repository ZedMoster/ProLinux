using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class ColorRemoveFilter : IExternalCommand
    {
        readonly RegistryStorage Registry = new RegistryStorage();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (Run.Running(Strings.key))
            {
                var key = "模型审查";
                try
                {
                    // 读取是否删除视图中全部的过滤器 DelAllFilter
                    var delAllFilter = Registry.OpenAfterStart("delAllFilter") == "True";
                    // 删除过滤器
                    ViewFilterRule filterRule = new ViewFilterRule();
                    filterRule.DelFilter(doc, key, delAllFilter);
                }
                catch (Exception e)
                {
                    TaskDialog.Show("提示", e.Message + Strings.error);
                }
            }
            return Result.Succeeded;


        }
    }
}
