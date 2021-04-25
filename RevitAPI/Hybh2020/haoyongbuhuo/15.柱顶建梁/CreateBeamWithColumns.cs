using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections;
using System.Collections.Generic;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class CreateBeamWithColumns : IExternalCommand
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
                try
                {
                    #region 读取梁参数名称
                    RegistryStorage Registry = new RegistryStorage();
                    // 读取梁参数名称
                    var b_string = Registry.OpenAfterStart("BeamB") ?? "b";
                    var h_string = Registry.OpenAfterStart("BeamH") ?? "h";
                    #endregion

                    // 获取 梁类型
                    var BeamType = new FilteredElementCollector(doc).OfCategory(
                        BuiltInCategory.OST_StructuralFraming).WhereElementIsElementType().ToElements();
                    // 类型名称键值对
                    List<SelectElementByName> listTypeSystem = new List<SelectElementByName>();

                    // 获取所有的类型名称 
                    foreach (Element el in BeamType)
                    {
                        var Familytype = el as ElementType;
                        listTypeSystem.Add(new SelectElementByName { HybhElement = Familytype, HybhElName = Familytype.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString() +":" + Familytype.Name });
                    }

                    var window = new WPFCreateBeamWithColumns(listTypeSystem);
                    window.ShowDialog();
                    if (window.IsHitTestVisible)
                    {
                        // 宽度 高度
                        double.TryParse(window.BeamInput_width.Text, out double width);
                        double.TryParse(window.BeamInput_heigh.Text, out double heigh);
                        // 类型
                        var Symbol = window.tp.SelectedValue as FamilySymbol;
                        // 是否UI已经选中构件
                        var uiSelIds = sel.GetElementIds();
                        if (uiSelIds.Count ==0)
                        {
                            // 手动选择 
                            var newfilter = new PickByCategorySelectionFilter
                            {
                                CategoryName = "结构柱"
                            };
                            try
                            {
                                SelPick.SelRef = sel.PickObject(ObjectType.Element, newfilter, "<依次点选结构柱-1>");
                                SelPick.SelRef_two = sel.PickObject(ObjectType.Element, newfilter, "<依次点选结构柱-2>");
                            }
                            catch { }

                            if (SelPick.SelRef == null || SelPick.SelRef_two == null)
                            {
                                return Result.Failed;
                            }

                            TransactionGroup T = new TransactionGroup(doc);
                            T.Start("柱定成梁");
                            try
                            {
                                // 获取 定位的结构柱
                                var el0 = doc.GetElement(SelPick.SelRef);
                                var el1 = doc.GetElement(SelPick.SelRef_two);

                                var instance = CreateBeamWithTwoColumns(doc, Symbol, el0, el1);
                                Transaction t = new Transaction(doc);
                                t.Start("更新参数");

                                #region 更新梁参数值
                                try
                                {
                                    // 获取梁类型参数
                                    var elType = instance.Symbol;
                                    var par_b = elType.LookupParameter(b_string);
                                    var par_h = elType.LookupParameter(h_string);
                                    // 判断参数是否为类型参数
                                    if (par_b == null && par_h == null)
                                    {
                                        instance.LookupParameter(b_string).Set(UUtools.MillimetersToUnits(width));
                                        instance.LookupParameter(h_string).Set(UUtools.MillimetersToUnits(heigh));
                                    }
                                    else
                                    {
                                        par_b.Set(UUtools.MillimetersToUnits(width));
                                        par_h.Set(UUtools.MillimetersToUnits(heigh));
                                    }
                                    t.Commit();
                                }
                                catch
                                {
                                    t.RollBack();
                                    TaskDialog.Show("提示", "需要在属性设置中设置梁的宽度及高度的参数名称");
                                }
                                #endregion

                                T.Assimilate();
                            }
                            catch (Exception e)
                            {
                                T.RollBack();
                                TaskDialog.Show("提示", e.Message + Strings.error);
                            }
                        }
                        else
                        {
                            if (uiSelIds.Count == 1)
                            {
                                TaskDialog.Show("提示", "至少需要先选择两个结构柱");
                            }
                            else
                            {
                                TransactionGroup T = new TransactionGroup(doc);
                                T.Start("柱定成梁");
                                try
                                {
                                    // 创建排序列表
                                    SortedList sortedList = new SortedList();
                                    foreach (var item in uiSelIds)
                                    {
                                        var el = doc.GetElement(item);
                                        var oldLocation = el.Location as LocationPoint;
                                        var oldPoint = oldLocation.Point;
                                        // 获取柱顶部标高
                                        var levelId = el.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).AsElementId();
                                        var level = doc.GetElement(levelId) as Level;
                                        // 项目定位点高程  ProjectElevation 项目基点修改
                                        var Elevation = level.ProjectElevation; 
                                        // 设置基础定位点
                                        XYZ xYZ = new XYZ(-1000, -1000, Elevation);
                                        XYZ newlocationPoint = new XYZ(oldPoint.X, oldPoint.Y, Elevation);
                                        var distance = newlocationPoint.DistanceTo(xYZ);
                                        sortedList.Add(distance, el);
                                    }
                                    // 柱排序列表
                                    var values = sortedList.Values;
                                    // 柱子element 列表
                                    List<Element> elementList = new List<Element>();
                                    if (values.Count != 0)
                                    {
                                        foreach (var item in values)
                                        {
                                            var el = item as Element;
                                            elementList.Add(el);
                                        }
                                        // 两个一组创建梁
                                        for (int i = 1; i < values.Count; i++)
                                        {
                                            var el0 = elementList[i - 1];
                                            var el1 = elementList[i];
                                            var instance = CreateBeamWithTwoColumns(doc, Symbol, el0, el1);

                                            Transaction t = new Transaction(doc);
                                            t.Start("更新参数");
                                            try
                                            {
                                                // 获取梁类型参数
                                                var elType = instance.Symbol;
                                                var par_b = elType.LookupParameter(b_string);
                                                var par_h = elType.LookupParameter(h_string);
                                                if (par_b == null && par_h == null)
                                                {
                                                    instance.LookupParameter(b_string).Set(UUtools.MillimetersToUnits(width));
                                                    instance.LookupParameter(h_string).Set(UUtools.MillimetersToUnits(heigh));
                                                }
                                                else
                                                {
                                                    par_b.Set(UUtools.MillimetersToUnits(width));
                                                    par_h.Set(UUtools.MillimetersToUnits(heigh));
                                                }
                                                t.Commit();
                                            }
                                            catch
                                            {
                                                t.RollBack();
                                                TaskDialog.Show("提示", "需要在属性设置中设置梁的宽度及高度的参数名称");
                                            }
                                        }
                                        T.Assimilate();
                                    }
                                    else
                                    {
                                        T.RollBack();
                                    }
                                }
                                catch (Exception e)
                                {
                                    T.RollBack();
                                    TaskDialog.Show("提示", e.Message + Strings.error);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    TaskDialog.Show("提示", e.Message + Strings.error);
                    return Result.Failed;
                }
            }

            return Result.Succeeded;
        }

        public FamilyInstance CreateBeamWithTwoColumns(Document doc, FamilySymbol Symbol, Element el0, Element el1)
        {
            // 获取柱顶部标高
            var levelId = el0.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).AsElementId();
            var level = doc.GetElement(levelId) as Level;
            // 获取定位点
            var p1 = CreateNewPointWithColumn(el0, level);
            var p2 = CreateNewPointWithColumn(el1, level);

            Curve curve = Line.CreateBound(p1, p2);
            // 开启事务
            Transaction transaction = new Transaction(doc);
            transaction.Start("创建梁");

            try
            {
                if (!Symbol.IsActive)
                {
                    Symbol.Activate();
                }

                // 关闭警告
                FailureHandlingOptions fho = transaction.GetFailureHandlingOptions();
                fho.SetFailuresPreprocessor(new FailuresPreprocessor());
                transaction.SetFailureHandlingOptions(fho);
                var instance = doc.Create.NewFamilyInstance(curve, Symbol, level, StructuralType.Beam);
                transaction.Commit();

                return instance;
 
            }
            catch (Exception e)
            {
                transaction.RollBack();
                TaskDialog.Show("提示", e.Message + Strings.error);
                return null;
            }
        }

        /// <summary>
        /// 获取结构柱定位点
        /// </summary>
        /// <param name="el"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public XYZ CreateNewPointWithColumn(Element el, Level level)
        {
            XYZ newlocationPoint;
            // 项目定位点高程
            var Elevation = level.ProjectElevation; // ProjectElevation 项目基点修改
            var oldLocation = el.Location as LocationPoint;
            if (null != oldLocation)
            {
                var oldPoint = oldLocation.Point;

                newlocationPoint = new XYZ(oldPoint.X, oldPoint.Y, Elevation);
                return newlocationPoint;
            }
            else
            {
                return null;
            }
        }
    }
}
