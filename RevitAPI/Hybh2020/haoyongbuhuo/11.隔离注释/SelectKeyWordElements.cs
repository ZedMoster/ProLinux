using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class SelectKeyWordElements : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            // 程序是否注册可运行
            if (Run.Running(Strings.key))
            {
                // 获取注释参数值
                var InParameter = BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS;

                // 过滤器
                List<ElementFilter> filters = new List<ElementFilter>() { 
                    new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming),
                    new ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns),
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

                var all = new FilteredElementCollector(doc).WherePasses(
                    new LogicalOrFilter(filters)).WhereElementIsNotElementType().ToElements().ToList();

                //Window window = new WPFSelectKeyWordElements(all);s
                //window.ShowDialog();

                // 手动选择
                PickByListCategorySelectionFilter newfilter = new PickByListCategorySelectionFilter
                {
                    ListCategoryName = new List<string>() {
                        "结构框架", "结构柱", "楼板", "墙", "常规模型", "门", "窗", "楼梯", "风管", "管道", "电缆桥架" 
                    }
                };
                try
                {
                    SelPick.SelRef = sel.PickObject(ObjectType.Element, newfilter, "<选择需要指定分区的一个模型(获取注释值)>");
                }
                catch { }
                if (SelPick.SelRef == null)
                {
                    return Result.Cancelled;
                }
                var el = doc.GetElement(SelPick.SelRef);
                var keyWord = el.get_Parameter(InParameter).AsString();
                var need = GetInWordsElements(all, keyWord, InParameter);

                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("隔离注释图元");
                    doc.ActiveView.IsolateElementsTemporary(need);
                    trans.Commit();
                }
            }

            return Result.Succeeded;
        }

        public List<ElementId> GetInWordsElements(List<Element> all, string word, BuiltInParameter InParameter)
        {
            List<ElementId> need = new List<ElementId>();
            // ALL_MODEL_INSTANCE_COMMENTS 注释
            foreach (var item in all)
            {
                var s = item.get_Parameter(InParameter).AsString();
                if (s == word)
                {
                    need.Add(item.Id);
                }
            }
            return need;
        }
    }
}
