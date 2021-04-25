using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class InstanceVoidCutUtils : IExternalCommand
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
                Categories groups = doc.Settings.Categories;
                // 获取所有的标高
                FilteredElementCollector filteredElementCollectors = new FilteredElementCollector(doc);
                var levels = filteredElementCollectors.OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().ToElements().ToList();

                var window = new WPFInstanceCutUtils(groups, levels);
                window.ShowDialog();
                // 判断程序是否继续运行
                if (window.IsHitTestVisible)
                {
                    var eleACategory = window.elementA.SelectedValue;
                    var eleBCategory = window.elementB.SelectedValue;
                    var Sellevel = window.SelectLevel.SelectedValue as Level;

                    var isCut = window.toCut.IsChecked.Value;
                    var isJoin = window.toJoin.IsChecked.Value;
                    // 标高?
                    var throughtLevel = window.InLevel.IsChecked.Value;

                    // 定义模型存储列表
                    List<Element> elementA = new List<Element>();
                    List<Element> elementB = new List<Element>();

                    #region 是否手动选择模型
                    if (sel.GetElementIds().Count == 0)
                    {
                        elementA = new FilteredElementCollector(doc).OfCategory((BuiltInCategory)eleACategory).WhereElementIsNotElementType().ToElements().ToList();
                        elementB = new FilteredElementCollector(doc).OfCategory((BuiltInCategory)eleBCategory).WhereElementIsNotElementType().ToElements().ToList();
                    }
                    else
                    {
                        var categoryAName = groups.get_Item((BuiltInCategory)eleACategory).Name;
                        var categoryBName = groups.get_Item((BuiltInCategory)eleBCategory).Name;

                        foreach (var item in sel.GetElementIds())
                        {
                            var el = doc.GetElement(item);
                            if (el.Category.Name == categoryAName)
                            {
                                elementA.Add(el);
                            }
                            else if (el.Category.Name == categoryBName)
                            {
                                elementB.Add(el);
                            }
                        }
                    }

                    if (elementA.Count == 0 || elementB.Count == 0)
                    {
                        return Result.Failed;
                    }
                    #endregion

                    using (Transaction transaction = new Transaction(doc))
                    {
                        transaction.Start("自动剪切模型");
                        if (isCut)
                        {
                            var num = 0;
                            // 优化算法////////////////////////////////////////////////////////
                            List<Element> ListElementA = new List<Element>();

                            #region 按标高过滤构件
                            if (throughtLevel)
                            {
                                GetLevel getLevel = new GetLevel();
                                var TopLevel = getLevel.NearLevel(Sellevel, true);
                                var select = from el in elementA
                                             where el.LevelId.IntegerValue == TopLevel.Id.IntegerValue
                                             select el;
                                ListElementA = select.ToList();
                            }
                            else
                            {
                                ListElementA = elementA;
                            }
                            if (ListElementA.Count == 0)
                            {
                                transaction.RollBack();
                                return Result.Failed;
                            }
                            #endregion

                            foreach (var elA in ListElementA)
                            {
                                #region 按标高过滤构件
                                List<Element> ListElementB = new List<Element>();
                                if (throughtLevel)
                                {
                                    var select = from el in elementB
                                                 where el.LevelId.IntegerValue == Sellevel.Id.IntegerValue
                                                 select el;
                                    ListElementB = select.ToList();
                                }
                                else
                                {
                                    ListElementB = elementB;
                                }
                                if (ListElementB.Count == 0)
                                {
                                    transaction.RollBack();
                                    return Result.Failed;
                                }
                                #endregion

                                foreach (var elB in ListElementB)
                                {
                                    // 判断模型定位是否再同一楼层
                                    try
                                    {
                                        var isGoodjobCut = CutTwoElement(doc, elB, elA);
                                        num += 1;
                                    }
                                    catch { }
                                }
                            }
                            var keyWords = string.Format("自动剪切洞口：{0}个", num);
                            if (num != 0)
                            {
                                TaskDialog.Show("统计", keyWords);
                            }
                        }

                        if (isJoin)
                        {
                            var num = 0;
                            // 创建相交模型过滤器
                            GetInstersectsElements GetInstersectsElements = new GetInstersectsElements();
                            // 优化算法////////////////////////////////////////////////////////
                            List<Element> ListElementB = new List<Element>();

                            #region 按标高过滤构件
                            if (throughtLevel)
                            {
                                var select = from el in elementB
                                             where el.LevelId.IntegerValue == Sellevel.Id.IntegerValue
                                             select el;
                                ListElementB = select.ToList();
                            }
                            else
                            {
                                ListElementB = elementB;
                            }
                            if (ListElementB.Count == 0)
                            {
                                transaction.RollBack();
                                return Result.Failed;
                            }
                            #endregion

                            foreach (var elB in ListElementB)
                            {
                                elementA = GetInstersectsElements.ElementByType(doc, elB, (BuiltInCategory)eleACategory).ToList();
                                foreach (var elA in elementA)
                                {
                                    try
                                    {
                                        var isGoodjobJoin = JoinTwoElement(doc, elB, elA);
                                        num += 1;
                                    }
                                    catch { }
                                }
                            }
                            var keyWords = string.Format("自动链接模型：{0}个", num);
                            if (num != 0)
                            {
                                TaskDialog.Show("统计", keyWords);
                            }
                        }

                        transaction.Commit();
                    }
                }
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// 用eleB 剪切 eleA
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="eleA"></param>
        /// <param name="eleB"></param>
        /// <returns></returns>
        public bool CutTwoElement(Document doc, Element eleA, Element eleB)
        {
            // 判断是否可以被剪切
            bool IsTrue = Autodesk.Revit.DB.InstanceVoidCutUtils.CanBeCutWithVoid(eleA);
            if (IsTrue)
            {
                Autodesk.Revit.DB.InstanceVoidCutUtils.AddInstanceVoidCut(doc, eleA, eleB);
            }
            return IsTrue;
        }

        /// <summary>
        /// 两个对象进行链接
        /// </summary>
        /// <param name="doc">Document</param>
        /// <param name="eleA">对象A</param>
        /// <param name="eleB">对象B</param>
        /// <returns></returns>
        public bool JoinTwoElement(Document doc, Element eleA, Element eleB)
        {
            try
            {
                JoinGeometryUtils.JoinGeometry(doc, eleA, eleB);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
