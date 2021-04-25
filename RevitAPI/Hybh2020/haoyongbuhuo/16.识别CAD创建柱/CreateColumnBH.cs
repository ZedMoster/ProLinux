using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class CreateColumnBH : IExternalCommand
    {
        // 注册列表缓存数据
        readonly RegistryStorage RegistryStorage = new RegistryStorage();
        private const string familyName = "hybh混凝土_矩形柱";
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

                // 定义全局变量
                XYZ loctionPont = null;
                double b = double.NaN;
                double h = double.NaN;
                XYZ midPoint = null;

                // 选择CAD图纸
                PickCADFilter pickCADFilter = new PickCADFilter();

                #region 选择结构柱填充
                try
                {

                    SelPick.SelRef = sel.PickObject(ObjectType.PointOnElement, pickCADFilter, "选择结构柱填充");
                }
                catch { }

                if (SelPick.SelRef == null)
                {
                    return Result.Failed;
                }
                ImportInstance dwg = doc.GetElement(SelPick.SelRef) as ImportInstance;
                var geoObj = dwg.GetGeometryObjectFromReference(SelPick.SelRef);
                Transform transform = dwg.GetTransform();
                #endregion

                #region 获取矩形填充的参数信息
                // 获取结构柱填充
                if (geoObj is PlanarFace)
                {
                    #region 判断截面形状获取定位点及尺寸参数
                    var planarFace = geoObj as PlanarFace;
                    var curveLoop = planarFace.GetEdgesAsCurveLoops().FirstOrDefault();
                    // 判断是不是矩形
                    Plane xPlane = Plane.CreateByNormalAndOrigin(new XYZ(0, 0, 1), XYZ.Zero);
                    var isRectangular = curveLoop.IsRectangular(xPlane);

                    List<XYZ> points = new List<XYZ>();
                    List<double> doubles = new List<double>();
                    // 获取边界的迭代器
                    var c = curveLoop.GetCurveLoopIterator();
                    c.Reset();
                    while (c.MoveNext())
                    {
                        var curve = c.Current;
                        var pnt_0 = curve.GetEndPoint(0).GetLength();
                        var pnt_1 = curve.GetEndPoint(1).GetLength();

                        if (!doubles.Contains(pnt_0))
                        {
                            doubles.Add(pnt_0);
                            points.Add(curve.GetEndPoint(0));
                        }
                        if (!doubles.Contains(pnt_1))
                        {
                            doubles.Add(pnt_1);
                            points.Add(curve.GetEndPoint(1));
                        }
                    }

                    if (points.Count != 4 && isRectangular)
                    {
                        TaskDialog.Show("提示", "所需填充不是矩形！");
                        return Result.Failed;
                    }

                    var p0 = transform.OfPoint(points[0]);
                    var p1 = transform.OfPoint(points[1]);
                    var p2 = transform.OfPoint(points[2]);
                    #endregion

                    // 边线中心点
                    midPoint = (p0 + p1) / 2;
                    // 定位点
                    loctionPont = (p0 + p2) / 2;
                    // 尺寸参数
                    b = p1.DistanceTo(p2);
                    h = p1.DistanceTo(p0);

                }
                else if (geoObj is PolyLine)
                {
                    var polyLine = geoObj as PolyLine;

                    if (polyLine.NumberOfCoordinates == 5)
                    {
                        var p0 = transform.OfPoint(polyLine.GetCoordinate(0));
                        var p1 = transform.OfPoint(polyLine.GetCoordinate(1));
                        var p2 = transform.OfPoint(polyLine.GetCoordinate(2));

                        midPoint = (p0 + p1) / 2;
                        loctionPont = (p0 + p2) / 2;
                        b = p1.DistanceTo(p2);
                        h = p0.DistanceTo(p1);
                    }
                }

                if (loctionPont == null || b == 0 || h == 0 || midPoint == null)
                {
                    TaskDialog.Show("提示", "柱截面读取获取错误");
                    return Result.Failed;
                }
                #endregion

                #region 识别柱编号文字
                CAD cad = new CAD();
                // 获取选中 梁 标注的信息
                try
                {

                    SelPick.SelRef_two = sel.PickObject(ObjectType.PointOnElement, pickCADFilter, "<请选择矩形结构柱标注名称>");
                }
                catch { }
                if (SelPick.SelRef_two == null)
                {
                    return Result.Cancelled;
                }

                var cadText = cad.GetCADText(SelPick.SelRef_two, doc);
                if (cadText == "--")
                {
                    TaskDialog.Show("提示", "CAD图纸文字识别错误");
                    return Result.Failed;
                }
                #endregion

                // 获取标高
                var level = doc.ActiveView.GenLevel;
                var B_name = RegistryStorage.OpenAfterStart("ColumnB");
                var H_name = RegistryStorage.OpenAfterStart("ColumnH");
                TransactionGroup T = new TransactionGroup(doc, "创建矩形柱");
                T.Start();
                bool Push = false;


                #region 创建结构柱
                // 获取族类型
                var familySymbol = GetElement(doc, cadText);
                // 获取 当前视图底部标高
                GetLevel getLevel = new GetLevel();
                var levelBase = getLevel.NearLevel(level, false);

                // 创建族实例
                var instance = CreateNewInstance(doc, loctionPont, familySymbol, levelBase);

                Transaction t2 = new Transaction(doc);
                t2.Start("更新参数");
                instance.LookupParameter(B_name).Set(b);
                instance.LookupParameter(H_name).Set(h);
                t2.Commit();

                try
                {
                    #region 旋转构件角度
                    Transaction t3 = new Transaction(doc);
                    t3.Start("旋转构件");
                    Line line = Line.CreateBound(loctionPont, midPoint);
                    var angle = ToRotaLineAngle(line);
                    // 选择角度
                    var axis = Line.CreateBound(loctionPont, new XYZ(loctionPont.X, loctionPont.Y, loctionPont.Z + 1));
                    var rotated = instance.Location.Rotate(axis, angle);
                    t3.Commit();
                    #endregion

                    Push = true;
                }
                catch { }
                #endregion

                #region 是否提交事务组
                if (Push)
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

        /// <summary>
        /// 获取XYZ(0,1,0)与element向量的旋转弧度
        /// </summary>
        /// <param name="line"> </param>
        /// <returns></returns>
        public double ToRotaLineAngle(Line line)
        {
            var lineDirection = line.Direction;
            // 套管初始方向 Y--(1,0,0) X--(0,1,0)
            var familyDirection = new XYZ(1, 0, 0);
            var angle = familyDirection.AngleTo(lineDirection);
            return angle;
        }

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="xYZ"></param>
        /// <param name="familySymbol"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        private FamilyInstance CreateNewInstance(Document doc, XYZ xYZ, FamilySymbol familySymbol, Level level)
        {

            using (Transaction transaction = new Transaction(doc))
            {
                transaction.Start("创建-结构柱");

                // 关闭警告
                FailureHandlingOptions fho = transaction.GetFailureHandlingOptions();
                fho.SetFailuresPreprocessor(new FailuresPreprocessor());
                transaction.SetFailureHandlingOptions(fho);

                if (!familySymbol.IsActive)
                {
                    familySymbol.Activate();
                }
                var familyInstance = doc.Create.NewFamilyInstance(xYZ, familySymbol, level, Autodesk.Revit.DB.Structure.StructuralType.Column);
                transaction.Commit();
                return familyInstance;
            }
        }

        private void IsHaveFamily(Document doc)
        {
            #region 判断族是否存在
            bool ishave = false;
            var els = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralColumns).WhereElementIsElementType().ToElements();
            foreach (var _family in els)
            {
                var Familytype = _family as ElementType;
                if (familyName == Familytype.FamilyName)
                {
                    ishave = true;
                }
            }
            #endregion

            #region 载入族文件
            if (!ishave)
            {
                Transaction t = new Transaction(doc);
                t.Start("加载族文件");
                var familyPath = @"C:\ProgramData\Autodesk\ApplicationPlugins\hybh2020.bundle\Contents\2018\Family\ColumnBH.rfa";
                doc.LoadFamily(familyPath, out Family family);
                family.Name = familyName;
                t.Commit();
            }
            #endregion
        }

        private FamilySymbol GetElement(Document doc, string name)
        {
            IsHaveFamily(doc);

            FamilySymbol familySymbol = null;

            var elsType = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralColumns).WhereElementIsElementType().ToElements();
            //TaskDialog.Show("0", elsType.Count.ToString());

            List<ElementType> elementType = new List<ElementType>();
            foreach (var _family in elsType)
            {
                var Familytype = _family as ElementType;

                if (Familytype.Name == name)
                {
                    familySymbol = _family as FamilySymbol;
                }
                //TaskDialog.Show("name", Familytype.FamilyName);

                if (Familytype.FamilyName == familyName)
                {
                    elementType.Add(Familytype);
                }
            }

            if (familySymbol != null)
            {
                return familySymbol;
            }
            else
            {
                var elType = elementType.First();
                Transaction t = new Transaction(doc);
                t.Start("创建类型");
                familySymbol = elType.Duplicate(name) as FamilySymbol;
                t.Commit();
                return familySymbol;
            }

        }
    }
}
