using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class CreateGrids : IExternalCommand
    {
        //X方向的向量
        private readonly XYZ vectX = new XYZ(-1, 0, 0);
        private readonly XYZ vectX2 = new XYZ(1, 0, 0);
        //Y方向的向量
        private readonly XYZ vectY = new XYZ(0, 1, 0);
        private readonly XYZ vectY2 = new XYZ(0, -1, 0);

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // 程序是否注册可运行
            if (Run.Running(Strings.key))
            {
                var view = doc.ActiveView;
                if (view.ViewType == ViewType.ThreeD)
                {
                    TaskDialog.Show("提示", "不能在3D视图创建标记！");
                    return Result.Failed;
                }

                WPFCreateGrids wPFCreateGrids = new WPFCreateGrids();
                wPFCreateGrids.ShowDialog();

                // 判断是否运行
                if (!wPFCreateGrids.IsHitTestVisible)
                {
                    return Result.Failed;
                }

                var kj = wPFCreateGrids.KJ.Text;
                var js = wPFCreateGrids.JS.Text;
                var kj_num = wPFCreateGrids.KJ_Copy.Text;
                var js_num = wPFCreateGrids.JS_Copy.Text;

                var _Grids = new FilteredElementCollector(doc).OfClass(typeof(Grid)).OfCategory(BuiltInCategory.OST_Grids).WhereElementIsNotElementType().ToElements();
                var _run = from el in _Grids
                           where el.Name == kj_num || el.Name == js_num
                           select el;

                if (_run.ToList().Count() > 0)
                {
                    TaskDialog.Show("提示", "项目中已存在相同编号的轴网");
                    return Result.Failed;
                }

                // 获取开间的 X 坐标值列表
                var Xlist = ElevationList(kj);
                // 获取进深的 Y 坐标值列表
                var Ylist = ElevationList(js);

                var Xmax = Xlist.Max();
                var Ymax = Ylist.Max();

                // 端部长度
                double L = 4000 / 304.8 - Ylist[0];

                // 列表 grid element
                var xGrid = new List<Grid>();
                var yGrid = new List<Grid>();

                TransactionGroup T = new TransactionGroup(doc);
                T.Start("创建轴网");

                Transaction t = new Transaction(doc);
                t.Start("创建轴网");
                #region 创建轴网轴线
                try
                {
                    // 创建 Y 方向轴网 初始name 1
                    // name = 1
                    var startPointY = new XYZ(0, -L, 0);
                    var endPointY = new XYZ(0, Ymax + L, 0);

                    Line lineY = Line.CreateBound(startPointY, endPointY);
                    Grid gridY = Grid.Create(doc, lineY);
                    yGrid.Add(gridY);
                    gridY.Name = kj_num;

                    foreach (var item in Xlist)
                    {
                        var startPoint = new XYZ(item, -L, 0);
                        var endPoint = new XYZ(item, Ymax + L, 0);

                        Line line = Line.CreateBound(startPoint, endPoint);
                        Grid grid = Grid.Create(doc, line);
                        yGrid.Add(grid);
                    }

                    // 创建 X 方向轴网 初始name A
                    // name = A
                    var startPointX = new XYZ(-L, 0, 0);
                    var endPointX = new XYZ(Xmax + L, 0, 0);

                    Line lineX = Line.CreateBound(startPointX, endPointX);
                    Grid gridX = Grid.Create(doc, lineX);
                    yGrid.Add(gridY);
                    gridX.Name = js_num;

                    foreach (var item in Ylist)
                    {
                        var startPoint = new XYZ(-L, item, 0);
                        var endPoint = new XYZ(Xmax + L, item, 0);
                        Line line = Line.CreateBound(startPoint, endPoint);
                        Grid grid = Grid.Create(doc, line);
                        xGrid.Add(grid);
                    }

                    t.Commit();
                }
                catch (Exception e)
                {
                    T.RollBack();
                    TaskDialog.Show("提示", e.Message + Strings.error);
                }
                #endregion

                GridPlan gridPlan = new GridPlan();
                gridPlan.MakeGridPlan(doc);

                #region 自动标注轴网
                //开启事务
                Transaction ts = new Transaction(doc, "自动标注");
                ts.Start("自动标注");
                try
                {
                    List<Grid> grids = new List<Grid>();//存放X方向的轴线
                    List<Grid> gridsX = new List<Grid>();//存放X方向的轴线
                    List<Grid> gridsY = new List<Grid>();//存放Y反向的轴线
                    ReferenceArray reffX = new ReferenceArray();//存放X方向轴线的引用
                    ReferenceArray reffY = new ReferenceArray();//存放Y方向的轴线的引用
                    Line x0Line = null;//X方向的标注线
                    Line y0Line = null;//Y方向的标注线
                    Line x1Line = null;//X方向的标注线
                    Line y1Line = null;//Y方向的标注线

                    FilteredElementCollector coll = new FilteredElementCollector(doc);
                    coll.OfClass(typeof(Grid)).OfCategory(BuiltInCategory.OST_Grids);

                    foreach (Grid item in coll)
                    {
                        Grid temp = item;
                        grids.Add(temp);
                    }

                    var num = 800 / 304.8 * 2;
                    //遍历所有的轴线，进行XY方向的分类
                    List<Grid> gridsX0 = new List<Grid>();//存放X方向的轴线
                    List<Grid> gridsY0 = new List<Grid>();//存放Y反向的轴线
                    foreach (Grid gr in coll)
                    {
                        Grid temp = gr;
                        Line line = temp.Curve as Line;
                        if (line.Direction.IsAlmostEqualTo(vectX) || line.Direction.IsAlmostEqualTo(vectX2))
                        {
                            gridsX.Add(temp);
                            reffX.Append(new Reference(temp));
                        }
                        if (line.Direction.IsAlmostEqualTo(vectY) || line.Direction.IsAlmostEqualTo(vectY2))
                        {
                            gridsY.Add(temp);
                            reffY.Append(new Reference(temp));
                        }
                    }

                    // 获取轴网box
                    var maxMinXyzs = GetPoint(grids);

                    // 标记个数
                    int N_X = gridsX.Count() - 1;
                    int N_Y = gridsY.Count() - 1;

                    // 标记轴网尺寸-第二道轴网分割 - OK
                    if (gridsX.Count >= 2)
                    {
                        var length = (gridsX[0].Curve as Line).Length;

                        x0Line = Line.CreateBound((gridsX[0].Curve as Line).Origin + new XYZ(num, 0, 0), (gridsX[1].Curve as Line).Origin + new XYZ(num, 0, 0));
                        x1Line = Line.CreateBound((gridsX[1].Curve as Line).Origin + new XYZ(length - num, 0, 0), (gridsX[0].Curve as Line).Origin + new XYZ(length - num, 0, 0));
                    }
                    if (gridsY.Count >= 2)
                    {

                        var length = (gridsY[0].Curve as Line).Length;

                        y0Line = Line.CreateBound((gridsY[0].Curve as Line).Origin + new XYZ(0, num, 0), (gridsY[1].Curve as Line).Origin + new XYZ(0, num, 0));
                        y1Line = Line.CreateBound((gridsY[1].Curve as Line).Origin + new XYZ(0, length - num, 0), (gridsY[0].Curve as Line).Origin + new XYZ(0, length - num, 0));
                    }

                    //获取外侧尺寸标注的引用 第三道为墙整体跨度
                    List<Grid> gridsX3 = new List<Grid>();//存放X方向的轴线
                    List<Grid> gridsY3 = new List<Grid>();//存放Y反向的轴线
                    ReferenceArray reffX3 = new ReferenceArray();//存放X方向轴线的引用
                    ReferenceArray reffY3 = new ReferenceArray();//存放X方向轴线的引用

                    Line x0Line3 = null;//X方向的标注线
                    Line y0Line3 = null;//Y方向的标注线
                    Line x1Line3 = null;//X方向的标注线
                    Line y1Line3 = null;//Y方向的标注线

                    var num3 = 800 / 304.8;

                    gridsX3 = GetMaxMin(gridsX, "X");
                    foreach (var temp in gridsX3)
                    {
                        Line line = temp.Curve as Line;
                        if (line.Direction.IsAlmostEqualTo(vectX) || line.Direction.IsAlmostEqualTo(vectX2))
                        {
                            reffX3.Append(new Reference(temp));
                        }
                    }

                    gridsY3 = GetMaxMin(gridsY, "Y");
                    foreach (var temp in gridsY3)
                    {
                        Line line = temp.Curve as Line;
                        if (line.Direction.IsAlmostEqualTo(vectY) || line.Direction.IsAlmostEqualTo(vectY2))
                        {
                            reffY3.Append(new Reference(temp));
                        }

                    }
                    if (gridsX3.Count == 2)
                    {
                        var length = (gridsX3[0].Curve as Line).Length;

                        x0Line3 = Line.CreateBound((gridsX3[0].Curve as Line).Origin + new XYZ(num3, 0, 0), (gridsX3[1].Curve as Line).Origin + new XYZ(num3, 0, 0));
                        x1Line3 = Line.CreateBound((gridsX3[1].Curve as Line).Origin + new XYZ(length - num3, 0, 0), (gridsX3[0].Curve as Line).Origin + new XYZ(length - num3, 0, 0));

                        //TaskDialog.Show("1", x0Line3.ToString());
                    }
                    if (gridsY3.Count == 2)
                    {
                        var length = (gridsY[0].Curve as Line).Length;

                        y0Line3 = Line.CreateBound((gridsY3[0].Curve as Line).Origin + new XYZ(0, num3, 0), (gridsY3[1].Curve as Line).Origin + new XYZ(0, num3, 0));
                        y1Line3 = Line.CreateBound((gridsY3[1].Curve as Line).Origin + new XYZ(0, length - num3, 0), (gridsY3[0].Curve as Line).Origin + new XYZ(0, length - num3, 0));
                    }


                    //获取外侧尺寸标注的引用 第一道门窗分割

                    Line x0Line1 = null;//X方向的标注线
                    Line y0Line1 = null;//Y方向的标注线
                    Line x1Line1 = null;//X方向的标注线
                    Line y1Line1 = null;//Y方向的标注线


                    var num1 = 800 / 304.8 * 3;

                    if (gridsX3.Count == 2)
                    {
                        var length = (gridsX3[0].Curve as Line).Length;

                        x0Line1 = Line.CreateBound((gridsX3[0].Curve as Line).Origin + new XYZ(num1, 0, 0), (gridsX3[1].Curve as Line).Origin + new XYZ(num1, 0, 0));
                        x1Line1 = Line.CreateBound((gridsX3[1].Curve as Line).Origin + new XYZ(length - num1, 0, 0), (gridsX3[0].Curve as Line).Origin + new XYZ(length - num1, 0, 0));

                        //TaskDialog.Show("1", x0Line3.ToString());
                    }
                    if (gridsY3.Count == 2)
                    {
                        var length = (gridsY[0].Curve as Line).Length;

                        y0Line1 = Line.CreateBound((gridsY3[0].Curve as Line).Origin + new XYZ(0, num1, 0), (gridsY3[1].Curve as Line).Origin + new XYZ(0, num1, 0));
                        y1Line1 = Line.CreateBound((gridsY3[1].Curve as Line).Origin + new XYZ(0, length - num1, 0), (gridsY3[0].Curve as Line).Origin + new XYZ(0, length - num1, 0));
                    }

                    // 所有门窗
                    List<ElementFilter> filters = new List<ElementFilter>() { new ElementCategoryFilter(BuiltInCategory.OST_Windows),
                                                            new ElementCategoryFilter(BuiltInCategory.OST_Doors),
                                                        };
                    var all = new FilteredElementCollector(doc).WherePasses(new LogicalOrFilter(filters)).WhereElementIsNotElementType().ToElements();

                    // 所有HOST墙体
                    List<Wall> walls = new List<Wall>();
                    List<ElementId> ids = new List<ElementId>();
                    foreach (var item in all)
                    {
                        var instance = item as FamilyInstance;
                        var wall = instance.Host as Wall;

                        if (!ids.Contains(wall.Id))
                        {
                            // 设置当前视图中的门窗
                            if (view.GenLevel.Id == wall.LevelId)
                            {
                                // 墙定位线 距离两个极坐标点的距离为 0
                                Line line = (wall.Location as LocationCurve).Curve as Line;
                                line.MakeUnbound();
                                if (Math.Round(line.Distance(maxMinXyzs[0]), 0) == 0 || Math.Round(line.Distance(maxMinXyzs[1]), 0) == 0)
                                {
                                    walls.Add(wall);
                                    ids.Add(wall.Id);
                                }
                            }
                        }
                    }

                    // 第二道轴网分割 - X向
                    doc.Create.NewDimension(view, x0Line, reffX);
                    doc.Create.NewDimension(view, x1Line, reffX);
                    // 第二道轴网分割 - Y向
                    doc.Create.NewDimension(view, y0Line, reffY);
                    doc.Create.NewDimension(view, y1Line, reffY);

                    // 第三道轴网分割 - X向
                    doc.Create.NewDimension(view, x0Line3, reffX3);
                    doc.Create.NewDimension(view, x1Line3, reffX3);
                    // 第三道轴网分割 - Y向
                    doc.Create.NewDimension(view, y0Line3, reffY3);
                    doc.Create.NewDimension(view, y1Line3, reffY3);

                    if (walls.Count != 0)
                    {
                        foreach (var item in walls)
                        {
                            var DimWalls = DimWallsHost(item, reffX, reffY);
                            var line1 = DimWalls.Keys.First();
                            var reff1 = DimWalls.Values.First();

                            line1.MakeUnbound();
                            if (IsXY)
                            {
                                var valueX0 = line1.Distance(maxMinXyzs[1]);
                                var valueX1 = line1.Distance(maxMinXyzs[0]);

                                //TaskDialog.Show("1", (valueX0 > valueX1).ToString());
                                if (valueX0 > valueX1)
                                {
                                    var dim = doc.Create.NewDimension(view, y0Line1, reff1);
                                    dim.get_Parameter(BuiltInParameter.DIM_LEADER).Set(0);
                                    var segments = dim.Segments;
                                    for (int i = 1; i < segments.Size; i++)
                                    {
                                        var a = segments.get_Item(i - 1);
                                        var b = segments.get_Item(i);
                                        if ((a.Value + b.Value) <= 1250 / 304.8)
                                        {
                                            var c = b.TextPosition;
                                            b.TextPosition = new XYZ(c.X, c.Y + 800 / 304.8, 0);
                                            i++;
                                        }
                                    }
                                }
                                else
                                {
                                    var dim = doc.Create.NewDimension(view, y1Line1, reff1);
                                    dim.get_Parameter(BuiltInParameter.DIM_LEADER).Set(0);
                                    var segments = dim.Segments;
                                    for (int i = 1; i < segments.Size; i++)
                                    {
                                        var a = segments.get_Item(i - 1);
                                        var b = segments.get_Item(i);
                                        if ((a.Value + b.Value) <= 1250 / 304.8)
                                        {
                                            var c = b.TextPosition;
                                            b.TextPosition = new XYZ(c.X, c.Y - 800 / 304.8, 0);
                                            i++;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var valueY0 = line1.Distance(maxMinXyzs[0]);
                                var valueY1 = line1.Distance(maxMinXyzs[1]);
                                //TaskDialog.Show("1", (valueY0 > valueY1).ToString());
                                if (valueY0 > valueY1)
                                {
                                    var dim = doc.Create.NewDimension(view, x1Line1, reff1);
                                    dim.get_Parameter(BuiltInParameter.DIM_LEADER).Set(0);
                                    var segments = dim.Segments;
                                    for (int i = 1; i < segments.Size; i++)
                                    {
                                        var a = segments.get_Item(i - 1);
                                        var b = segments.get_Item(i);
                                        if ((a.Value + b.Value) <= 1250 / 304.8)
                                        {
                                            var c = b.TextPosition;
                                            b.TextPosition = new XYZ(c.X - 800 / 304.8, c.Y, 0);
                                            i++;
                                        }
                                    }
                                }
                                else
                                {
                                    var dim = doc.Create.NewDimension(view, x0Line1, reff1);

                                    dim.get_Parameter(BuiltInParameter.DIM_LEADER).Set(0);
                                    var segments = dim.Segments;
                                    for (int i = 1; i < segments.Size; i++)
                                    {
                                        var a = segments.get_Item(i - 1);
                                        var b = segments.get_Item(i);
                                        if ((a.Value + b.Value) <= 1250 / 304.8)
                                        {
                                            var c = b.TextPosition;
                                            b.TextPosition = new XYZ(c.X + 800 / 304.8, c.Y, 0);
                                            i++;
                                        }
                                    }
                                }

                            }
                        }
                    }

                    ts.Commit();

                    T.Assimilate();
                }
                catch
                {
                    ts.RollBack();
                    T.RollBack();
                }
                #endregion
            }

            return Result.Succeeded;
        }


        /// <summary>
        /// 构建 值 列表
        /// </summary>
        /// <param name="words"> 获取文字转标高</param>
        /// <returns></returns>
        private List<double> ElevationList(string words)
        {
            double elevation = 0.0;
            string[] strArray = words.Split(',', '，', ' ');
            List<double> elevaList = new List<double>();
            foreach (string strheight in strArray)  // 遍历输入
            {
                if (strheight.Contains("*"))
                {
                    string[] arry = strheight.Split('*');
                    bool resBool = double.TryParse(arry[0], out double res);  // 层高
                    bool numBool = int.TryParse(arry[1], out int num);  // 个数

                    if (numBool && resBool)
                    {
                        for (int i = 0; i < num; i++)
                        {
                            elevation += res / 304.8;
                            elevaList.Add(elevation);
                        }
                    }
                }

                else
                {
                    bool resBool = double.TryParse(strheight, out double res);
                    //TaskDialog.Show("all", res.ToString());
                    if (resBool)
                    {
                        elevation += res / 304.8;
                        elevaList.Add(elevation);
                    }
                }
            }

            return elevaList;
        }

        /// <summary>
        /// 设置轴网端点位置 - 4000/304.8
        /// </summary>
        /// <param name="grids"></param>
        /// <param name="xYZ"></param>
        private List<Grid> SetGridsPoint(Document doc, List<Grid> grids, List<XYZ> maxMinXyzs, string Origin)
        {
            List<Grid> gridList = new List<Grid>();

            // 间距
            var lenth = 4000 / 304.8;
            var minXyz = maxMinXyzs[0];
            var maxXyz = maxMinXyzs[1];
            if (Origin == "X")
            {
                foreach (var item in grids)
                {
                    Grid temp = item;
                    string name = temp.Name;

                    var MinPnt = temp.GetExtents().MinimumPoint;
                    var MaxPnt = temp.GetExtents().MaximumPoint;

                    Line newLine = Line.CreateBound(new XYZ(minXyz.X - lenth, MinPnt.Y, 0), new XYZ(maxXyz.X + lenth, MaxPnt.Y, 0));
                    Grid lineGrid = Grid.Create(doc, newLine);
                    doc.Delete(temp.Id);
                    lineGrid.Name = name;

                    gridList.Add(lineGrid);
                }

            }
            else
            {
                foreach (var item in grids)
                {
                    Grid temp = item;
                    string name = temp.Name;

                    var MinPnt = temp.GetExtents().MinimumPoint;
                    var MaxPnt = temp.GetExtents().MaximumPoint;

                    Line newLine = Line.CreateBound(new XYZ(MinPnt.X, minXyz.Y - lenth, 0), new XYZ(MaxPnt.X, maxXyz.Y + lenth, 0));
                    Grid lineGrid = Grid.Create(doc, newLine);
                    doc.Delete(temp.Id);
                    lineGrid.Name = name;

                    gridList.Add(lineGrid);
                }
            }

            return gridList;
        }

        private IList<Element> ElementByType(Document doc, Element element, BuiltInCategory builtInCategory)
        {
            // element 相交模型过滤器 无法获取空心模型
            var elementIntersects = new ElementIntersectsElementFilter(element);
            // 获取指定类别的模型
            var instersectsElements = new FilteredElementCollector(doc).OfCategory(
                builtInCategory).WherePasses(elementIntersects).ToElements();
            return instersectsElements;
        }

        /// <summary>
        /// 获取端部 轴网
        /// </summary>
        /// <param name="Grids"></param>
        /// <param name="Origin"></param>
        /// <returns></returns>
        private List<Grid> GetMaxMin(List<Grid> Grids, string Origin)
        {
            List<Grid> grids = new List<Grid>();
            Dictionary<double, Grid> pairs = new Dictionary<double, Grid>();
            List<double> vs = new List<double>();
            if (Origin == "X")
            {
                foreach (var item in Grids)
                {
                    var points = (item.Curve as Line).Tessellate();
                    var va = (points[0].Y + points[1].Y) / 2;
                    pairs[va] = item;
                    vs.Add(va);
                }
            }
            else
            {
                foreach (var item in Grids)
                {
                    var points = (item.Curve as Line).Tessellate();
                    var va = (points[0].X + points[1].X) / 2;
                    pairs[va] = item;
                    vs.Add(va);
                }
            }
            // 排序
            vs.Sort();
            grids.Add(pairs[vs.Min()]);
            grids.Add(pairs[vs.Max()]);

            return grids;
        }

        /// <summary>
        /// 获取BOX点位
        /// </summary>
        /// <param name="Grids"></param>
        /// <returns></returns>
        private List<XYZ> GetPoint(List<Grid> Grids)
        {
            List<XYZ> MinMax = new List<XYZ>();
            Dictionary<double, Grid> pairsX = new Dictionary<double, Grid>();
            List<double> vsX = new List<double>();
            Dictionary<double, Grid> pairsY = new Dictionary<double, Grid>();
            List<double> vsY = new List<double>();

            foreach (var item in Grids)
            {
                Grid temp = item;
                Line line = temp.Curve as Line;
                if (line.Direction.IsAlmostEqualTo(vectX) || line.Direction.IsAlmostEqualTo(vectX2))
                {
                    var points = (item.Curve as Line).Tessellate();
                    var va = (points[0].Y + points[1].Y) / 2;
                    pairsX[va] = item;
                    vsX.Add(va);
                }
                if (line.Direction.IsAlmostEqualTo(vectY) || line.Direction.IsAlmostEqualTo(vectY2))
                {
                    var points = (item.Curve as Line).Tessellate();
                    var va = (points[0].X + points[1].X) / 2;
                    pairsY[va] = item;
                    vsY.Add(va);
                }
            }

            // 排序
            vsX.Sort();
            vsY.Sort();

            var minX = pairsX[vsX.Min()];
            var minY = pairsY[vsY.Min()];
            var maxX = pairsX[vsX.Max()];
            var maxY = pairsY[vsY.Max()];

            var minLineX = minX.Curve;
            var minLineY = minY.Curve;
            var maxLineX = maxX.Curve;
            var maxLineY = maxY.Curve;

            IntersectionResultArray resultArrayMin = new IntersectionResultArray();
            IntersectionResultArray resultArrayMax = new IntersectionResultArray();
            minLineX.Intersect(minLineY, out resultArrayMin);
            maxLineX.Intersect(maxLineY, out resultArrayMax);

            var intersectPointMin = resultArrayMin.get_Item(0).XYZPoint;
            var intersectPointMax = resultArrayMax.get_Item(0).XYZPoint;
            MinMax.Add(intersectPointMin);
            MinMax.Add(intersectPointMax);

            //TaskDialog.Show("0", intersectPointMin.ToString());
            //TaskDialog.Show("1", intersectPointMax.ToString());

            return MinMax;
        }

        public bool IsXY { get; set; }
        /// <summary>
        /// 墙标记 参数获取
        /// </summary>
        /// <param name="wall"></param>
        /// <returns></returns>
        private Dictionary<Line, ReferenceArray> DimWallsHost(Wall wall, ReferenceArray referenceX, ReferenceArray referenceY)
        {
            // 返回列表
            Dictionary<Line, ReferenceArray> pairs = new Dictionary<Line, ReferenceArray>();
            // 删除极值
            Dictionary<double, Reference> rf = new Dictionary<double, Reference>();

            ReferenceArray refArry = new ReferenceArray();
            ReferenceArray array = new ReferenceArray();

            Line wallLine = (wall.Location as LocationCurve).Curve as Line;
            XYZ wallDir = ((wall.Location as LocationCurve).Curve as Line).Direction;
            // 轴网
            if (wallDir.IsAlmostEqualTo(vectY) || wallDir.IsAlmostEqualTo(vectY2))
            {
                IsXY = false;
                foreach (Reference item in referenceX)
                {
                    refArry.Append(item);
                }
            }
            else
            {
                IsXY = true;
                foreach (Reference item in referenceY)
                {
                    refArry.Append(item);
                }
            }
            // 洞口
            Options opt = new Options
            {
                ComputeReferences = true,
                DetailLevel = ViewDetailLevel.Fine
            };
            GeometryElement gelem = wall.get_Geometry(opt);

            foreach (GeometryObject gobj in gelem)
            {
                if (gobj is Solid)
                {
                    Solid solid = gobj as Solid;
                    foreach (Face face in solid.Faces)
                    {
                        if (face is PlanarFace)
                        {
                            XYZ faceDir = face.ComputeNormal(new UV());
                            if (faceDir.IsAlmostEqualTo(wallDir) || faceDir.IsAlmostEqualTo(-wallDir))
                            {
                                array.Append(face.Reference);
                                var f = face as PlanarFace;
                                if (f.FaceNormal.IsAlmostEqualTo(vectY) || f.FaceNormal.IsAlmostEqualTo(vectY2))
                                {
                                    rf[f.Origin.Y] = face.Reference;
                                }
                                else
                                {
                                    rf[f.Origin.X] = face.Reference;
                                }
                            }
                        }
                    }
                }
            }

            // 删除端部arry
            var keys_min = rf.Keys.Min();
            var keys_max = rf.Keys.Max();

            rf.Remove(keys_min);
            rf.Remove(keys_max);
            foreach (var item in rf.Keys)
            {
                var arr = rf[item];
                refArry.Append(arr);
            }

            pairs[wallLine] = refArry;

            return pairs;
        }

    }

    /// <summary>
    /// 设置轴网
    /// </summary>
    class GridPlan
    {
        //X方向的向量
        public XYZ vectX = new XYZ(-1, 0, 0);
        public XYZ vectX2 = new XYZ(1, 0, 0);
        //Y方向的向量
        public XYZ vectY = new XYZ(0, 1, 0);
        public XYZ vectY2 = new XYZ(0, -1, 0);
        public void MakeGridPlan(Document doc)
        {
            try
            {
                //开启事务
                using (Transaction ts = new Transaction(doc))
                {
                    ts.Start("自动标注");

                    List<Grid> grids = new List<Grid>();//存放X方向的轴线
                    List<Grid> gridsX = new List<Grid>();//存放X方向的轴线
                    List<Grid> gridsY = new List<Grid>();//存放Y反向的轴线

                    FilteredElementCollector coll = new FilteredElementCollector(doc);
                    coll.OfClass(typeof(Grid)).OfCategory(BuiltInCategory.OST_Grids);

                    foreach (Grid item in coll)
                    {
                        Grid temp = item as Grid;
                        grids.Add(temp);
                    }
                    // 获取轴网box
                    var maxMinXyzs = GetPoint(grids);

                    //遍历所有的轴线，进行XY方向的分类
                    List<Grid> gridsX0 = new List<Grid>();//存放X方向的轴线
                    List<Grid> gridsY0 = new List<Grid>();//存放Y反向的轴线
                    foreach (Grid gr in coll)
                    {
                        Grid temp = gr as Grid;
                        Line line = temp.Curve as Line;
                        if (line.Direction.IsAlmostEqualTo(vectX) || line.Direction.IsAlmostEqualTo(vectX2))
                        {
                            gridsX0.Add(temp);
                        }
                        if (line.Direction.IsAlmostEqualTo(vectY) || line.Direction.IsAlmostEqualTo(vectY2))
                        {
                            gridsY0.Add(temp);
                        }
                    }

                    // 设置轴网端点的位置 reffX.Append(new Reference(temp));
                    gridsX = SetGridsPoint(doc, gridsX0, maxMinXyzs, "X");
                    gridsY = SetGridsPoint(doc, gridsY0, maxMinXyzs, "Y");

                    ts.Commit();
                }
            }
            catch (Exception)
            {
                TaskDialog.Show("提示", "确定轴网全部相交！");
            }

        }

        /// <summary>
        /// 设置轴网端点位置 - 4000/304.8
        /// </summary>
        /// <param name="grids"></param>
        /// <param name="xYZ"></param>
        public List<Grid> SetGridsPoint(Document doc, List<Grid> grids, List<XYZ> maxMinXyzs, string Origin)
        {
            List<Grid> gridList = new List<Grid>();

            // 间距
            var lenth = 4000 / 304.8;
            var minXyz = maxMinXyzs[0];
            var maxXyz = maxMinXyzs[1];
            if (Origin == "X")
            {
                foreach (var item in grids)
                {
                    Grid temp = item as Grid;
                    string name = temp.Name;

                    var MinPnt = temp.GetExtents().MinimumPoint;
                    var MaxPnt = temp.GetExtents().MaximumPoint;

                    Line newLine = Line.CreateBound(new XYZ(minXyz.X - lenth, MinPnt.Y, 0), new XYZ(maxXyz.X + lenth, MaxPnt.Y, 0));
                    Grid lineGrid = Grid.Create(doc, newLine);
                    doc.Delete(temp.Id);
                    lineGrid.Name = name;

                    gridList.Add(lineGrid);
                }
            }
            else
            {
                foreach (var item in grids)
                {
                    Grid temp = item as Grid;
                    string name = temp.Name;

                    var MinPnt = temp.GetExtents().MinimumPoint;
                    var MaxPnt = temp.GetExtents().MaximumPoint;

                    Line newLine = Line.CreateBound(new XYZ(MinPnt.X, minXyz.Y - lenth, 0), new XYZ(MaxPnt.X, maxXyz.Y + lenth, 0));
                    Grid lineGrid = Grid.Create(doc, newLine);
                    doc.Delete(temp.Id);
                    lineGrid.Name = name;

                    gridList.Add(lineGrid);
                }
            }

            return gridList;
        }

        /// <summary>
        /// 获取BOX点位
        /// </summary>
        /// <param name="Grids"></param>
        /// <returns></returns>
        public List<XYZ> GetPoint(List<Grid> Grids)
        {
            List<XYZ> MinMax = new List<XYZ>();
            Dictionary<double, Grid> pairsX = new Dictionary<double, Grid>();
            List<double> vsX = new List<double>();
            Dictionary<double, Grid> pairsY = new Dictionary<double, Grid>();
            List<double> vsY = new List<double>();

            foreach (var item in Grids)
            {
                Grid temp = item as Grid;
                Line line = temp.Curve as Line;
                if (line.Direction.IsAlmostEqualTo(vectX) || line.Direction.IsAlmostEqualTo(vectX2))
                {
                    var points = (item.Curve as Line).Tessellate();
                    var va = (points[0].Y + points[1].Y) / 2;
                    pairsX[va] = item;
                    vsX.Add(va);
                }
                if (line.Direction.IsAlmostEqualTo(vectY) || line.Direction.IsAlmostEqualTo(vectY2))
                {
                    var points = (item.Curve as Line).Tessellate();
                    var va = (points[0].X + points[1].X) / 2;
                    pairsY[va] = item;
                    vsY.Add(va);
                }
            }

            // 排序
            vsX.Sort();
            vsY.Sort();

            var minX = pairsX[vsX.Min()];
            var minY = pairsY[vsY.Min()];
            var maxX = pairsX[vsX.Max()];
            var maxY = pairsY[vsY.Max()];

            var minLineX = minX.Curve;
            var minLineY = minY.Curve;
            var maxLineX = maxX.Curve;
            var maxLineY = maxY.Curve;

            IntersectionResultArray resultArrayMin = new IntersectionResultArray();
            IntersectionResultArray resultArrayMax = new IntersectionResultArray();
            minLineX.Intersect(minLineY, out resultArrayMin);
            maxLineX.Intersect(maxLineY, out resultArrayMax);

            var intersectPointMin = resultArrayMin.get_Item(0).XYZPoint;
            var intersectPointMax = resultArrayMax.get_Item(0).XYZPoint;
            MinMax.Add(intersectPointMin);
            MinMax.Add(intersectPointMax);

            return MinMax;
        }
    }
}
