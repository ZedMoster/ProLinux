using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class CreateBeamsReadParaCAD : IExternalCommand
    {
        // 注册列表缓存数据
        readonly RegistryStorage RegistryStorage = new RegistryStorage();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            // 程序是否注册可运行
            if (Run.Running(Strings.key))
            {
                if (Run.Is3DViewCanNotWork(doc))
                {
                    return Result.Failed;
                }

                // 获取标高
                var level = doc.ActiveView.GenLevel;
                // 选择链接CAD图纸
                PickCADFilter CADFilter = new PickCADFilter();

                #region 选中梁定位线
                try
                {
                    ToFinish toFinish = new ToFinish();
                    toFinish.Subscribe();
                    // 获取图形定位点
                    SelPick.SelRefsList = sel.PickObjects(ObjectType.PointOnElement, CADFilter, "<请点选梁定位线(默认Y轴对正方式 左)>").ToList();
                    toFinish.Unsubscribe();
                }
                catch { SelPick.SelRefsList = new List<Reference>(); }

                if (SelPick.SelRefsList.Count == 0)
                {
                    TaskDialog.Show("提示", "取消本次操作！未选择边界线");
                    return Result.Cancelled;
                }

                List<Curve> Curves = new List<Curve>();
                foreach (var refCurve in SelPick.SelRefsList)
                {
                    ImportInstance dwg = doc.GetElement(refCurve) as ImportInstance;
                    var geoObj = (dwg as Element).GetGeometryObjectFromReference(refCurve);

                    if (geoObj is Curve)
                    {
                        var curve = geoObj as Curve;
                        Curves.Add(curve);
                    }
                }
                #endregion

                ImportInstance _dwg = doc.GetElement(SelPick.SelRefsList[0]) as ImportInstance;
                Transform transform = _dwg.GetTransform();

                // 保存本次识别的 集中标注的信息
                var familyName = RegistryStorage.OpenAfterStart("BEAM_NAME_18");
                var B = RegistryStorage.OpenAfterStart("BEAM_NAME_18_B");
                var H = RegistryStorage.OpenAfterStart("BEAM_NAME_18_H");

                TransactionGroup T = new TransactionGroup(doc);
                T.Start("CAD梁");
                List<bool> Push = new List<bool>();
                NewFamilyInstanceTwoPoint twoPoint = new NewFamilyInstanceTwoPoint();

                #region 创建实例
                try
                {
                    // 文字参数转double类型
                    double.TryParse(B, out double width);
                    double.TryParse(H, out double heigh);
                    // 获取族类型
                    var symbol = twoPoint.GetFamilySymbol(doc, familyName);
                    // 读取梁参数名称
                    var b_string = RegistryStorage.OpenAfterStart("BeamB") ?? "b";
                    var h_string = RegistryStorage.OpenAfterStart("BeamH") ?? "h";

                    foreach (var _curve in Curves)
                    {
                        var coordinates = _curve.Tessellate();
                        // ProjectElevation 项目基点修改
                        var Elevation = level.ProjectElevation;
                        XYZ p1 = transform.OfPoint(_curve.GetEndPoint(0)) + new XYZ(0, 0, Elevation); // 起点
                        XYZ p2 = transform.OfPoint(_curve.GetEndPoint(1)) + new XYZ(0, 0, Elevation); // 终点

                        #region 创建梁实例
                        try
                        {
                            // 创建族实例
                            var instance = twoPoint.CreateBeamWithTwoPoints(doc, symbol, level, p1, p2);
                            if (instance != null)
                            {
                                // 更新实例或类型参数
                                var push = twoPoint.UpDateInstanceOrSymbolParater(doc, instance, b_string, h_string, width, heigh);
                                Push.Add(push);
                            }
                        }
                        catch (Exception e)
                        {
                            TaskDialog.Show("提示", e.Message + Strings.error);
                        }
                        #endregion
                    }
                }
                catch(Exception e)
                {
                    TaskDialog.Show("提示", e.Message + Strings.error);
                }
                #endregion

                #region 是否提交事务组
                if (Push.Any(x => x == true))
                {
                    T.Assimilate();
                }
                else
                {
                    T.RollBack();
                }
                #endregion

            }

            return Result.Succeeded;
        }
    }

    class NewFamilyInstanceTwoPoint
    {
        /// <summary>
        /// 判断点是不是再直线（线段）上
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsPointOnLine(XYZ start, XYZ end, XYZ p)
        {
            var length = start.DistanceTo(end);
            var part_0 = start.DistanceTo(p);
            var part_1 = end.DistanceTo(p);
            if ((part_0 + part_1).Equals(length))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 更新梁参数
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="instance"></param>
        /// <param name="b_string"></param>
        /// <param name="h_string"></param>
        /// <param name="width"></param>
        /// <param name="heigh"></param>
        /// <returns></returns>
        public bool UpDateInstanceOrSymbolParater(Document doc, FamilyInstance instance, string b_string, string h_string, double width, double heigh, double zh = 0)
        {
            bool Push = false;
            // 更新实例参数
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
                // 更新 Y轴对正 -- 左
                instance.get_Parameter(BuiltInParameter.Z_OFFSET_VALUE).Set(0); // Z 轴偏移值 Z_OFFSET_VALUE
                instance.get_Parameter(BuiltInParameter.Y_JUSTIFICATION).Set(0); //Y 轴对正 Y_JUSTIFICATION
                instance.get_Parameter(BuiltInParameter.STRUCTURAL_BEAM_END0_ELEVATION).Set(0); // 起点标高偏移 STRUCTURAL_BEAM_END0_ELEVATION
                instance.get_Parameter(BuiltInParameter.STRUCTURAL_BEAM_END1_ELEVATION).Set(0); // 终点标高偏移 STRUCTURAL_BEAM_END1_ELEVATION
                instance.get_Parameter(BuiltInParameter.Z_OFFSET_VALUE).Set(UUtools.MillimetersToUnits(zh * 1000)); // 终点标高偏移 STRUCTURAL_BEAM_END1_ELEVATION
                t.Commit();
                Push = true;
            }
            catch
            {
                t.RollBack();
                TaskDialog.Show("提示", "需要在属性设置中设置梁的宽度及高度的参数名称");
            }
            #endregion

            return Push;
        }


        public FamilySymbol Symbol { get; set; }
        /// <summary>
        /// 获取特定名称的族类型，不存在则自动复制类型
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="familyname"></param>
        /// <returns></returns>
        public FamilySymbol GetFamilySymbol(Document doc, string familyname)
        {
            // 载入所需的族文件
            DoLoadFamily doLoadFamily = new DoLoadFamily();
            try
            {
                var Rfa = "StructureBeam";
                var have = doLoadFamily.IsHaveFamily(doc, BuiltInCategory.OST_StructuralFraming, Rfa);
                if (!have)
                {
                    doLoadFamily.LoadFamily(doc, Rfa + ".rfa", "hybh混凝土_矩形梁");
                }
            }
            catch
            {
                TaskDialog.Show("提示", "项目中不存在梁族且载入族失败\n");
            }

            // 获取 梁类型
            var BeamType = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming).WhereElementIsElementType().ToElements();
            // 获取所有的类型名称 
            foreach (Element el in BeamType)
            {
                var Familytype = el as ElementType;
                var _FamilytypeName = Familytype.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString();
                if (_FamilytypeName == familyname)
                {
                    Symbol = Familytype as FamilySymbol;
                }
            }
            if (Symbol != null)
            {
                return Symbol;
            }
            else
            {
                var elementType = BeamType.First() as ElementType;
                Transaction t = new Transaction(doc);
                t.Start("创建类型");
                Symbol = elementType.Duplicate(familyname) as FamilySymbol;
                t.Commit();
                return Symbol;
            }
        }

        /// <summary>
        /// 通过族类型，视图标高，定位点 参数自动创建结构梁
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="Symbol"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public FamilyInstance CreateBeamWithTwoPoints(Document doc, FamilySymbol Symbol, Level _level, XYZ p1, XYZ p2)
        {
            XYZ start = null;
            XYZ end = null;

            #region 判断起点终点确定绘制方向
            if (p1.X == p2.X)
            {
                if (p1.Y < p2.Y)
                {
                    start = p1;
                    end = p2;
                }
                else
                {
                    start = p2;
                    end = p1;
                }
            }
            else
            {
                if (p1.X < p2.X)
                {
                    start = p1;
                    end = p2;
                }
                else
                {
                    start = p2;
                    end = p1;
                }
            }
            #endregion

            Curve curve = Line.CreateBound(start, end);
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
                // 创建结构梁实例
                var instance = doc.Create.NewFamilyInstance(curve, Symbol, _level, StructuralType.Beam);
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
    }
}
