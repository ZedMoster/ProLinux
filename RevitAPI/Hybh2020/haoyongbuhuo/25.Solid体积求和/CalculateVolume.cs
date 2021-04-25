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
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;


            // 程序是否注册可运行
            if (Run.Running(Strings.key))
            {
                #region 设置 结构梁柱 过滤器
                //List<ElementFilter> filters = new List<ElementFilter>()
                //{
                //    new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming),
                //    new ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns),
                //    new ElementCategoryFilter(BuiltInCategory.OST_Floors),
                //    new ElementCategoryFilter(BuiltInCategory.OST_Walls),
                //};
                //var allElements = new FilteredElementCollector(doc).WherePasses(
                //    new LogicalOrFilter(filters)).WhereElementIsNotElementType().ToElements();
                #endregion

                // 创建列表
                List<Element> allElements = new List<Element>();
                // 结构模型
                List<string> AllowCategoryName = new List<string> { "结构框架", "楼板", "结构柱", "墙" };

                if (sel.GetElementIds().Count == 0)
                {
                    #region 选择构件-PickObjects
                    // 对齐的梁或板
                    PickByListCategorySelectionFilter selectFilter = new PickByListCategorySelectionFilter
                    {
                        ListCategoryName = AllowCategoryName
                    };

                    try
                    {
                        // 右键完成多选
                        ToFinish toFinish = new ToFinish();

                        toFinish.Subscribe();
                        SelPick.SelRefsList = sel.PickObjects(ObjectType.Element, selectFilter, "<选择结构模型>").ToList();
                        toFinish.Unsubscribe();
                    }
                    catch { SelPick.SelRefsList = new List<Reference>(); }

                    if (SelPick.SelRefsList.Count == 0)
                    {
                        return Result.Cancelled;
                    }
                    foreach (var elId in SelPick.SelRefsList)
                    {
                        Element el = doc.GetElement(elId);
                        allElements.Add(el);
                    }
                    #endregion
                }
                else
                {
                    foreach (var item in sel.GetElementIds().ToList())
                    {
                        var el = doc.GetElement(item);
                        // 过滤 结构模型
                        if (AllowCategoryName.Contains(el.Category.Name))
                        {
                            allElements.Add(el);
                        }
                    }
                }

                // 合并所有的结构实体
                SolidByUnion solidByUnion = new SolidByUnion();
                var solid = solidByUnion.ByUnion(allElements);
                if (solid != null)
                {
                    double volume = UnitUtils.Convert(solid.Volume, DisplayUnitType.DUT_CUBIC_FEET, DisplayUnitType.DUT_CUBIC_METERS);
                    MessageBox.Show(string.Format("所选择的结构构件的体积和：\n\n{0:0.000}  m3", volume), "体积");
                }
            }

            return Result.Succeeded;
        }
    }
}
