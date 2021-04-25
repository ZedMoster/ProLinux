using System;
using System.Collections.Generic;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class DelOverrideInType : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            // 程序是否注册可运行
            if (Run.Running(Strings.key))
            {
                var overrideGraphicSettings = new OverrideGraphicSettings();
                var activeView = uidoc.ActiveView;
                // 设置 结构模型 过滤器 
                List<ElementFilter> filters = new List<ElementFilter>()
                {
                    new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming),
                    new ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns),
                    new ElementCategoryFilter(BuiltInCategory.OST_StructuralFoundation),
                    new ElementCategoryFilter(BuiltInCategory.OST_Floors),
                    new ElementCategoryFilter(BuiltInCategory.OST_Walls),
                    new ElementCategoryFilter(BuiltInCategory.OST_GenericModel),
                    new ElementCategoryFilter(BuiltInCategory.OST_Doors),
                    new ElementCategoryFilter(BuiltInCategory.OST_Windows),
                    new ElementCategoryFilter(BuiltInCategory.OST_Stairs),
                    new ElementCategoryFilter(BuiltInCategory.OST_DuctCurves),
                    new ElementCategoryFilter(BuiltInCategory.OST_PipeCurves),
                    new ElementCategoryFilter(BuiltInCategory.OST_CableTray),
                };

                var allElements = new FilteredElementCollector(doc).WherePasses(new LogicalOrFilter(filters)).WhereElementIsNotElementType().ToElements();

                using (Transaction t = new Transaction(doc))
                {
                    t.Start("图形替换恢复");
                    try
                    {
                        foreach (var item in allElements)
                        {
                            //var overrideGraphicSettings = new OverrideGraphicSettings();
                            //var activeView = uidoc.ActiveView;
                            // 图形替换恢复
                            activeView.SetElementOverrides(item.Id, overrideGraphicSettings);
                        }
                        t.Commit();
                    }
                    catch (Exception e)
                    {
                        TaskDialog.Show("提示", e.Message + Strings.error);
                        t.RollBack();
                    }
                }
            }

            return Result.Succeeded;
        }
    }
}
