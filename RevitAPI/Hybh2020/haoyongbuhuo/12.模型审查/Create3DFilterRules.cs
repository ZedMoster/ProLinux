using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class Create3DFilterRules : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

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
                TransactionGroup T = new TransactionGroup(doc, "审查视图");
                T.Start();
                try
                {
                    var key = "模型审查";
                    var _view = Create3DView(doc, key);

                    ViewFilterRule filterRule = new ViewFilterRule();
                    // 设置过滤器
                    filterRule.EqualsRule(doc, _view, categories, key, "NO", color_NO);
                    filterRule.EqualsRule(doc, _view, categories, key, "OK", color_OK);

                    T.Assimilate();
                    // 设置当前视图未创建的视图
                    uidoc.ActiveView = _view;

                }
                catch (Exception e)
                {
                    T.RollBack();
                    TaskDialog.Show("提示", e.Message + Strings.error);
                }
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// 创建三维视图 指定 名称 及显示样式为着色
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="name"> 视图名称</param>
        /// <returns></returns>
        public View Create3DView(Document doc, string name)
        {
            var col = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).WhereElementIsNotElementType().ToElements().ToList();
            foreach (var item in col)
            {
                if (item.Name == name)
                {
                    _View3D = item as View;
                }
            }

            if (_View3D == null)
            {
                Transaction tc = new Transaction(doc, "view3D");
                tc.Start();
                var viewtype = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>() 
                    .FirstOrDefault<ViewFamilyType>(x => ViewFamily.ThreeDimensional == x.ViewFamily);
                var view3d = View3D.CreateIsometric(doc, viewtype.Id);
                view3d.Name = name;
                _View3D = view3d;

                // 设置视图显示样式
                _View3D.DisplayStyle = DisplayStyle.ShadingWithEdges;
                _View3D.DetailLevel = ViewDetailLevel.Fine;
                //view3D.get_Parameter(BuiltInParameter.VIEW_DETAIL_LEVEL).Set((int)ViewDetailLevel.Fine);
                //view3D.get_Parameter(BuiltInParameter.MODEL_GRAPHICS_STYLE).Set((int)DisplayStyle.Shading); // 着色 不显示显示边界
                ////view.get_Parameter(BuiltInParameter.MODEL_GRAPHICS_STYLE).Set((int)DisplayStyle.ShadingWithEdges); // 着色显示边界

                tc.Commit();
            }

            return _View3D;
        }
        public View _View3D { get; set; }

    }
}
