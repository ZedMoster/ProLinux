using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class UpdateSymbolName : IExternalCommand
    {
        public string CategoryNameString { get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            if (!Run.Running(Strings.key))
            {
                return Result.Failed;
            }

            #region 获取设置的类型名称
            RegistryStorage Registry = new RegistryStorage();

            var index = Registry.OpenAfterStart("CategoryNameIndex");
            switch (index)
            {
                case "0":
                    CategoryNameString = "结构框架";
                    break;
                case "1":
                    CategoryNameString = "结构柱";
                    break;
                default:
                    CategoryNameString = "结构框架";
                    break;
            }

            if (CategoryNameString == null)
            {
                return Result.Failed;
            }
            #endregion

            #region 选择的构件element列表  CategoryName = "结构框架"
            List<ElementId> selIds = sel.GetElementIds().ToList();
            // 所有选择的构件
            List<Element> els = new List<Element>();
            if (selIds.Count == 0)
            {
                var newfilter = new PickByCategorySelectionFilter { CategoryName = CategoryNameString };
                try
                {
                    // 右键完成多选
                    ToFinish toFinish = new ToFinish();
                    toFinish.Subscribe();
                    SelPick.SelRefsList = sel.PickObjects(ObjectType.Element, newfilter, "<请选择审查的构件>").ToList();
                    toFinish.Unsubscribe();
                }
                catch { SelPick.SelRefsList = new List<Reference>(); }

                if (SelPick.SelRefsList.Count == 0)
                {
                    return Result.Cancelled;
                }
                foreach (var item in SelPick.SelRefsList)
                {
                    var el = doc.GetElement(item);
                    els.Add(el);

                    // ui 选择构件id添加
                    selIds.Add(el.Id);
                }
            }

            else
            {
                foreach (var item in selIds)
                {
                    // 过滤掉不是element 的图元
                    try
                    {
                        els.Add(doc.GetElement(item));
                    }
                    catch { }
                }
            }
            #endregion

            if (els.Count == 0)
            {
                return Result.Failed;
            }

            #region 选择CAD图层获取文字-族类型名称
            // 获取选中elID
            try
            {
                SelPick.SelRef = sel.PickObject(ObjectType.PointOnElement, "<请选择图纸对应的类别名称TEXT>");
            }
            catch { }
            if (SelPick.SelRef == null)
            {
                return Result.Failed;
            }

            CAD GetCADText = new CAD();
            var _name = GetCADText.GetCADText(SelPick.SelRef, doc);
            if (_name == "--")
            {
                return Result.Failed;
            }


            // 获取到名称
            var changedSymbolName = _name.Split(' ', '(', 'x', 'X')[0];
            #endregion

            #region 判断构件是否包含了新的类型名称
            var instance = els.FirstOrDefault() as FamilyInstance;
            ElementType symbol = instance.Symbol;
            List<ElementId> symbolList = symbol.GetSimilarTypes().ToList();

            // 新建的族类型
            ElementType familySymbol = null;
            ElementId elementId = null;

            foreach (var symbolId in symbolList)
            {
                var elementType = doc.GetElement(symbolId) as ElementType;
                if (elementType.Name == changedSymbolName)
                {
                    familySymbol = elementType;
                    elementId = symbolId;
                    break;
                }
            }
            #endregion

            #region 更新族类型名称
            bool Push = false;
            TransactionGroup T = new TransactionGroup(doc, "替换类型");
            T.Start();
            if (familySymbol == null || elementId == null)
            {
                foreach (var el in els)
                {
                    Transaction transaction = new Transaction(doc, "新增类型");
                    transaction.Start();
                    try
                    {
                        //复制生成新的族类型
                        familySymbol = symbol.Duplicate(changedSymbolName);
                        //获取新的族类型的ID
                        elementId = familySymbol.Id;
                        //依据ID改变族类型
                        el.ChangeTypeId(elementId);
                        transaction.Commit();
                        Push = true;
                    }
                    catch
                    {
                        transaction.RollBack();
                    }
                }
            }
            else
            {
                foreach (var el in els)
                {
                    Transaction transaction = new Transaction(doc, "替换类型");
                    transaction.Start();
                    try
                    {
                        //依据ID改变族类型
                        el.ChangeTypeId(elementId);
                        transaction.Commit();
                        Push = true;
                    }
                    catch
                    {
                        transaction.RollBack();
                    }
                }
            }

            if (Push)
            {
                T.Assimilate();
            }
            else
            {
                T.RollBack();
            }
            #endregion

            // ui继续选中构件
            sel.SetElementIds(selIds);

            return Result.Succeeded;
        }
    }
}
