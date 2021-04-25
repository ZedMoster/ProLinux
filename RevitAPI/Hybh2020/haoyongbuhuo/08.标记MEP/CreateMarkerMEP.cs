using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class CreateMarkerMEP : IExternalCommand
    {
        public TagOrientation Tagorn { get; set; }
        public XYZ SelPoint { get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            // 程序是否注册可运行
            if (Run.Running(Strings.key))
            {
                var view = doc.ActiveView;
                if (view.ViewType == ViewType.ThreeD)
                {
                    TaskDialog.Show(title: "提示", "不能在3D视图创建标记！");
                    return Result.Failed;
                }

                TransactionGroup transactionGroup = new TransactionGroup(doc, "MEP标记");
                transactionGroup.Start();
                // 载入所需的族文件
                DoLoadFamily doLoadFamily = new DoLoadFamily();
                try
                {
                    var Rfa1 = "MEPCableTrayTags";
                    var Rfa2 = "MEPDuctTags";
                    var Rfa3 = "MEPPipeTags";

                    var have1 = doLoadFamily.IsHaveFamily(doc,BuiltInCategory.OST_CableTrayTags, Rfa1);
                    if (!have1)
                    {
                        doLoadFamily.LoadFamily(doc, Rfa1 + ".rfa", "hybh电缆桥架注释");
                    }
                    var have2 = doLoadFamily.IsHaveFamily(doc, BuiltInCategory.OST_DuctTags, Rfa2);
                    if (!have2)
                    {
                        doLoadFamily.LoadFamily(doc, Rfa2 + ".rfa", "hybh风管注释");
                    }
                    var have3 = doLoadFamily.IsHaveFamily(doc, BuiltInCategory.OST_PipeTags, Rfa3);
                    if (!have3)
                    {
                        doLoadFamily.LoadFamily(doc, Rfa3 + ".rfa", "hybh管道注释");
                    }
                }
                catch
                {
                    transactionGroup.RollBack();
                    TaskDialog.Show("提示", "自带族需要自行修改定位信息");
                    return Result.Failed;
                }


                // 手动选择MEP管线
                PickByListCategorySelectionFilter newfilter = new PickByListCategorySelectionFilter
                {
                    ListCategoryName = new List<string>() { "管道", "风管", "电缆桥架" }
                };
                try
                {
                    ToFinish toFinish = new ToFinish();
                    toFinish.Subscribe();
                    SelPick.SelRefsList = sel.PickObjects(ObjectType.Element, newfilter, "<选择需要标记的MEP管线>").ToList();
                    toFinish.Unsubscribe();
                }
                catch { SelPick.SelRefsList = new List<Reference>(); }

                if (SelPick.SelRefsList.Count == 0)
                {
                    transactionGroup.RollBack();
                    return Result.Cancelled;
                }

                // # MEP elements
                List<Element> els = new List<Element>();
                foreach (var elId in SelPick.SelRefsList)
                {
                    Element el = doc.GetElement(elId);
                    els.Add(el);
                }
                // 获取标记族
                var pipeTags = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_PipeTags).WhereElementIsElementType().ToElements();
                var ductTags = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctTags).WhereElementIsElementType().ToElements();
                var cableTrayTags = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_CableTrayTags).WhereElementIsElementType().ToElements();

                // 获取箭头样式
                IList<Element> elementArrow = new List<Element>();
                var elementsType = new FilteredElementCollector(doc).WhereElementIsElementType().ToElements();
                foreach (var item in elementsType)
                {
                    var familyName = item.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString();
                    if (familyName == "箭头" && item.Name != "箭头")
                    {
                        //TaskDialog.Show("1", item.Name);
                        elementArrow.Add(item);
                    }
                }
                WPFCreateMarkerMEP window = new WPFCreateMarkerMEP(elementArrow, pipeTags, ductTags, cableTrayTags);
                window.ShowDialog();
                if (window.IsHitTestVisible)
                {
                    // Tagorn ={"水平":TagOrientation.Horizontal, "垂直":TagOrientation.Vertical}
                    var checkbox = window.NewArrow.IsChecked.Value;
                    var newArrow = window.Arrow.SelectedValue as Element;
                    var pipeTag = window.PipeType.SelectedValue as Element;
                    var ductTag = window.DuctType.SelectedValue as Element;
                    var cableTrayTag = window.CableTrayType.SelectedValue as Element;

                    var resultOne = double.TryParse(window.InputtextOne.Text, out double inputOne);
                    if (!resultOne)
                    {
                        transactionGroup.RollBack();
                        TaskDialog.Show("提示", "标记距离应输入数字!");
                        return Result.Failed;
                    }

                    var boolTagorn = GetTagorn(els);
                    //TaskDialog.Show("1", boolTagorn.ToString());

                    try
                    {
                        using (Transaction t = new Transaction(doc))
                        {
                            t.Start("创建管道标记");
                            // 更改标记 引线箭头
                            if (checkbox)
                            {
                                try
                                {
                                    pipeTag.get_Parameter(BuiltInParameter.LEADER_ARROWHEAD).Set(newArrow.Id);
                                }
                                catch { }
                                try
                                {
                                    ductTag.get_Parameter(BuiltInParameter.LEADER_ARROWHEAD).Set(newArrow.Id);
                                }
                                catch { }
                                try
                                {
                                    cableTrayTag.get_Parameter(BuiltInParameter.LEADER_ARROWHEAD).Set(newArrow.Id);
                                }
                                catch { }
                            }

                            // 手动设置标记的位置
                            try
                            {
                                SelPoint = uidoc.Selection.PickPoint("<点选标记放置位置>");
                            }
                            catch { }
                            
                            if (SelPoint == null)
                            {
                                return Result.Cancelled;
                            }

                            var num = els.Count;

                            XYZ elbowPnt;
                            XYZ headerPnt;
                            var locationPot = MillimetersToUnits(inputOne);
                            if (boolTagorn)
                            {
                                elbowPnt = SelPoint + new XYZ(0, locationPot, 0.0);
                                headerPnt = SelPoint + new XYZ(locationPot / 2, locationPot, 0.0);
                            }
                            else
                            {
                                elbowPnt = SelPoint + new XYZ(-locationPot, 0.0, 0.0);
                                headerPnt = SelPoint + new XYZ(-locationPot, locationPot / 2, 0.0);
                            }

                            var pipeMidPoints = SelPoint;

                            // 设置创建标记的参数值
                            var tagMode = TagMode.TM_ADDBY_CATEGORY;
                            List<Element> tags = new List<Element>();
                            // 标记 方向
                            Dictionary<double, Element> keyValues = new Dictionary<double, Element>();
                            foreach (var el in els)
                            {
                                var pipeCurve = el.Location as LocationCurve;
                                var pipeMid = pipeCurve.Curve.Evaluate(0.5, true);
                                var pipeRef = new Reference(el);

                                if (boolTagorn)
                                {
                                    Tagorn = TagOrientation.Horizontal;
                                }
                                else
                                {
                                    Tagorn = TagOrientation.Vertical;
                                }

                                // 创建管道标记
                                var newTag = IndependentTag.Create(doc, view.Id, pipeRef, true, tagMode, Tagorn, pipeMid);

                                // 修改标记为指定类型
                                var elType = el.Category.Name;
                                if (elType == "管道")
                                {
                                    newTag.ChangeTypeId(pipeTag.Id);
                                }
                                else if (elType == "风管")
                                {
                                    newTag.ChangeTypeId(ductTag.Id);
                                }
                                else if (elType == "电缆桥架")
                                {
                                    newTag.ChangeTypeId(cableTrayTag.Id);
                                }
                                // 标记 坐标值
                                double midPntBase;
                                if (boolTagorn)
                                {
                                    midPntBase = pipeMid.Y;
                                    keyValues[midPntBase] = newTag;
                                }
                                else
                                {
                                    midPntBase = pipeMid.X;
                                    keyValues[midPntBase] = newTag;
                                }
                            }

                            // 修改标签位置
                            tags = GetDictionaryElement(keyValues, boolTagorn);
                            for (int i = 0; i < tags.Count; i++)
                            {
                                XYZ Pot;
                                if (boolTagorn)
                                {
                                    Pot = new XYZ(0, MillimetersToUnits(inputOne) * i, 0);

                                }
                                else
                                {
                                    Pot = new XYZ(MillimetersToUnits(inputOne) * i, 0, 0);
                                    //Pot = new XYZ(0, MillimetersToUnits(inputTwo) * i, 0);

                                }
                                var Tag = tags[i] as IndependentTag;
                                Tag.LeaderEndCondition = LeaderEndCondition.Free;
                                Tag.LeaderEnd = pipeMidPoints;
                                Tag.LeaderElbow = elbowPnt + Pot;
                                Tag.TagHeadPosition = headerPnt + Pot;

                                Tag.LeaderEndCondition = LeaderEndCondition.Attached;

                            }
                            // 提交事务
                            t.Commit();
                        }

                    }
                    catch (Exception e)
                    {
                        // 防止程序运行出错
                        TaskDialog.Show("提示", e.Message + Strings.error);
                        return Result.Failed;
                    }
                    transactionGroup.Assimilate();
                }
                else
                {
                    transactionGroup.RollBack();
                }
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// 毫米 转 英尺
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public double MillimetersToUnits(double value)
        {
            return UnitUtils.Convert(value, DisplayUnitType.DUT_MILLIMETERS, DisplayUnitType.DUT_DECIMAL_FEET);
        }

        public bool targorn { get; set; }
        public bool GetTagorn(IList<Element> els)
        {
            var elCurve = els[0].Location as LocationCurve;
            var elLine = elCurve.Curve as Line;
            var TagornX = elLine.Direction.X;
            if (TagornX == 1|| TagornX == -1 )
            {
                // 水平
                targorn = true;
            }

            return targorn;
        }
        /// <summary>
        /// 自带按键排序
        /// </summary>
        /// <param name="tags">字典键值对</param>
        /// <param name="tagorn">方向对应排序</param>
        /// <returns>返回值列表</returns>
        public List<Element> GetDictionaryElement(Dictionary<double, Element> tags, bool tagorn)
        {
            List<double> needList = new List<double>();
            List<Element> TagsElement = new List<Element>();
            var keyColl = tags.Keys;
            foreach (var item in keyColl)
            {
                needList.Add(item);
            }

            if (tagorn)
            {
                needList.Sort();
                
                foreach (var item in needList)
                {
                    TagsElement.Add(tags[item]);
                }
            }
            else
            {
                needList.Sort();
                foreach (var item in needList)
                {
                    TagsElement.Add(tags[item]);
                }
            }

            return TagsElement;
        }

        /// <summary>
        /// 得到所有标记定位点
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="xYZ"></param>
        /// <param name="tagorn"></param>
        /// <param name="locationPot"></param>
        /// <returns></returns>
        public IList<XYZ> GetLocationPoint(IList<Element> elements, XYZ xYZ, bool tagorn, double locationPot)
        {
            List<XYZ> Pnts = new List<XYZ>();

            Dictionary<double, XYZ> midPointsDict = new Dictionary<double, XYZ>();
            if (tagorn)
            {
                // 获取水平点位
                foreach (var el in elements)
                {
                    var pipeLocation = el.Location as LocationCurve;
                    var pipeCurve = pipeLocation.Curve;
                    var pipeMidPnt = pipeCurve.Project(xYZ).XYZPoint;
                    midPointsDict.Add(pipeMidPnt.X, pipeMidPnt);
                }
            }
            else
            {
                // 获取垂直点位
                foreach (var el in elements)
                {
                    var pipeLocation = el.Location as LocationCurve;
                    var pipeCurve = pipeLocation.Curve;
                    var pipeMidPnt = pipeCurve.Project(xYZ).XYZPoint;
                    midPointsDict.Add(pipeMidPnt.Y, pipeMidPnt);
                }
            }
            
            List<double> needList = new List<double>();
            List<XYZ> midPoints = new List<XYZ>();
            var keyColl = midPointsDict.Keys;
            foreach (var item in keyColl)
            {
                needList.Add(item);
                //TaskDialog.Show("1", getPipeMidPoints[item].ToString());
            }
            if (tagorn)
            {
                needList.Reverse();
                foreach (var item in needList)
                {
                    midPoints.Add(midPointsDict[item]);
                }
                XYZ elbowPnt = midPoints[0] + new XYZ(0, locationPot, 0.0);
                XYZ headerPnt = midPoints[0] + new XYZ(locationPot / 2, locationPot, 0.0);
                
                Pnts.Add(elbowPnt);
                Pnts.Add(headerPnt);
                Pnts.Add(midPoints[0]);
            }
            else
            {
                needList.Sort();
                foreach (var item in needList)
                {
                    midPoints.Add(midPointsDict[item]);
                }
                XYZ elbowPnt = midPoints[0] + new XYZ(-locationPot, 0.0, 0.0);
                XYZ headerPnt = midPoints[0] + new XYZ(-locationPot, locationPot / 2, 0.0);

                Pnts.Add(elbowPnt);
                Pnts.Add(headerPnt);
                Pnts.Add(midPoints[0]);
            }

            return Pnts;
        }
    }
}
