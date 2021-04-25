using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using System.Globalization;
using System.Windows;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class CalculateVolume : IExternalCommand
    {
        //public List<Element> allElements { get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;
            // 右键完成多选
            ToFinish toFinish = new ToFinish();

            // 程序是否注册可运行
            Run run = new Run();
            run._key = "robot";
            if (run.Running())
            {
                //// 设置 结构梁柱 过滤器
                //List<ElementFilter> filters = new List<ElementFilter>()
                //{
                //    new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming),
                //    new ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns),
                //    new ElementCategoryFilter(BuiltInCategory.OST_Floors),
                //    new ElementCategoryFilter(BuiltInCategory.OST_Walls),
                //};
                //var allElements = new FilteredElementCollector(doc).WherePasses(new LogicalOrFilter(filters)).WhereElementIsNotElementType().ToElements();

                // 对齐的梁或板
                PickByListCategorySelectionFilter selectFilter = new PickByListCategorySelectionFilter();
                selectFilter.ListCategoryName = new List<string>() { "结构框架", "楼板", "结构柱", "墙" };
                List<Reference> selIDs = new List<Reference>();
                try
                {
                    toFinish.Subscribe();
                    selIDs = sel.PickObjects(ObjectType.Element, selectFilter, "<选择结构模型>").ToList();
                    toFinish.Unsubscribe();
                }
                catch { }

                if (selIDs.Count == 0)
                {
                    return Result.Failed;
                }

                // 创建列表
                List<Element> allElements = new List<Element>();
                foreach (var elId in selIDs)
                {
                    Element el = doc.GetElement(elId);
                    allElements.Add(el);
                }

                SolidByUnion solidByUnion = new SolidByUnion();
                var solid = solidByUnion.ByUnion(allElements);
                if (solid != null)
                {
                    double volume = UnitUtils.Convert(solid.Volume, DisplayUnitType.DUT_CUBIC_FEET, DisplayUnitType.DUT_CUBIC_METERS);
                    MessageBox.Show("体积", string.Format("所选择的结构构件的体积和：\n\n{0:0.000} m3", volume));
                }

            }

            return Result.Succeeded;

        }
    }
}
