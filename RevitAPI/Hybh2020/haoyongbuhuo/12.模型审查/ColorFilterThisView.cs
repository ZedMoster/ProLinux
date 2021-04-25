using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class ColorFilterThisView : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.ActiveView;

            if (Run.Running(Strings.key))
            {
                // 设置过滤器类别
                List<ElementId> categories = new List<ElementId>
                {
                    new ElementId(BuiltInCategory.OST_Walls),
                    new ElementId(BuiltInCategory.OST_Floors),
                    new ElementId(BuiltInCategory.OST_StructuralFraming),
                    new ElementId(BuiltInCategory.OST_StructuralColumns),
                    new ElementId(BuiltInCategory.OST_StructuralFoundation),
                    new ElementId(BuiltInCategory.OST_GenericModel),
                    new ElementId(BuiltInCategory.OST_Doors),
                    new ElementId(BuiltInCategory.OST_Windows),
                    new ElementId(BuiltInCategory.OST_Stairs),
                    new ElementId(BuiltInCategory.OST_DuctCurves),
                    new ElementId(BuiltInCategory.OST_PipeCurves),
                    new ElementId(BuiltInCategory.OST_CableTray)
                };

                #region 设置颜色
                // 构件添加颜色
                GetRegistryBackgroundColor backgroundColor_no = new GetRegistryBackgroundColor
                {
                    // No -- 橘色
                    _r = 255,
                    _g = 70,
                    _b = 0
                };

                Color color_NO = backgroundColor_no.BackgroundColor("F1_r", "F1_g", "F1_b");

                // 构件添加颜色
                GetRegistryBackgroundColor backgroundColor_ok = new GetRegistryBackgroundColor
                {
                    _r = 0,
                    _g = 128,
                    _b = 0
                };

                Color color_OK = backgroundColor_ok.BackgroundColor("F0_r", "F0_g", "F0_b");
                #endregion

                var key = "模型审查";
                TransactionGroup T = new TransactionGroup(doc);
                T.Start("审查过滤器");
                try
                {
                    // 设置过滤器
                    ViewFilterRule filterRule = new ViewFilterRule();
                    filterRule.EqualsRule(doc, view, categories, key, "NO", color_NO);
                    filterRule.EqualsRule(doc, view, categories, key, "OK", color_OK);
                    T.Assimilate();
                }
                catch (Exception e)
                {
                    T.RollBack();
                    TaskDialog.Show("提示", e.Message + Strings.error);
                }
                return Result.Succeeded;
            }
            else
            {
                return Result.Failed;
            }

            
        }
    }
}
