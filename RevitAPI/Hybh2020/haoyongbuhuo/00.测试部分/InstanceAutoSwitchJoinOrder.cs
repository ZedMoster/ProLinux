using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class InstanceAutoSwitchJoinOrder : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            if (Run.Running(Strings.key))
            {
                // 初始化方法
                JoinElements joinElements = new JoinElements();

                // 查看是否已经选中构件
                var selIDs = sel.GetElementIds();
                if (selIDs.Count == 0)
                {
                    try
                    {
                        SelPick.SelRef = sel.PickObject(ObjectType.Element, "<选择需要切换链接顺序的模型>");
                    }
                    catch { }

                    if (SelPick.SelRef == null)
                    {
                        return Result.Cancelled;
                    }
                    Transaction t = new Transaction(doc);
                    t.Start("切换连接顺序");
                    try
                    {
                        var selId = doc.GetElement(SelPick.SelRef).Id;
                        joinElements.SwichJoinElements(doc, selId);
                        t.Commit();
                    }
                    catch (Exception e)
                    {
                        t.RollBack();
                        TaskDialog.Show("提示", e.Message + Strings.error);
                    }
                }

                // 获取当前选中的模型
                else
                {
                    Transaction t = new Transaction(doc);
                    t.Start("切换连接顺序");
                    try
                    {
                        foreach (var item in selIDs)
                        {
                            joinElements.SwichJoinElements(doc, item);
                        }
                        t.Commit();
                    }
                    catch (Exception e)
                    {
                        t.RollBack();
                        TaskDialog.Show("提示", e.Message + Strings.error);
                    }
                }
            }

            return Result.Succeeded;
        }
    }

    public class JoinElements
    {
        /// <summary>
        /// 切换与sel 模型连接的模型连接顺序
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="selId"></param>
        public void SwichJoinElements(Document doc, ElementId selId)
        {
            // 获取当前选中的模型
            var el = doc.GetElement(selId);
            var joinEls = JoinGeometryUtils.GetJoinedElements(doc, el);
            if (joinEls.Count != 0)
            {
                foreach (var item in joinEls)
                {
                    var _el = doc.GetElement(item);
                    JoinGeometryUtils.SwitchJoinOrder(doc, el, _el);
                }
            }
            else
            {
                TaskDialog.Show("提示", "当前选中模型没有与之连接的构件");
            }
        }
    }

}
