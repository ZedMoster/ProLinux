using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

namespace CADReader
{
    /// <summary>
    /// 项目拓展方法
    /// </summary>
    static class RevitExtensions
    {
        /// <summary>
        /// 创建基于点的模型
        /// </summary>
        public static int NewInstancePoints(this Document doc, FamilySymbol symbol, List<XYZ> xyzs, Autodesk.Revit.DB.Structure.StructuralType structuralType = Autodesk.Revit.DB.Structure.StructuralType.NonStructural)
        {
            int count = 0;
            foreach (var xyz in xyzs)
            {
                #region 创建实例并更新参数
                Transaction transaction = new Transaction(doc);
                transaction.Start("创建实例");
                transaction.NoFailure();
                if (!symbol.IsActive)
                {
                    symbol.Activate();
                }
                try
                {
                    FamilyInstance instance = doc.Create.NewFamilyInstance(xyz.Flatten(), symbol, doc.ActiveView.GenLevel, structuralType);
                    transaction.Commit();
                    count++;
                }
                catch
                {
                    transaction.RollBack();
                }
                #endregion
            }
            return count;
        }

        /// <summary>
        /// 创建基于线的模型
        /// </summary>
        public static int NewInstanceCurves(this Document doc, FamilySymbol symbol, List<Curve> curves, Autodesk.Revit.DB.Structure.StructuralType structuralType = Autodesk.Revit.DB.Structure.StructuralType.Beam)
        {
            int count = 0;
            foreach (var curve in curves)
            {
                #region 创建实例并更新参数
                Transaction transaction = new Transaction(doc);
                transaction.Start("创建实例");
                transaction.NoFailure();
                if (!symbol.IsActive)
                {
                    symbol.Activate();
                }
                try
                {
                    var level = doc.ActiveView.GenLevel;
                    FamilyInstance instance = doc.Create.NewFamilyInstance(curve, symbol, level, structuralType);

                    #region 道路参数设置
                    if (instance.Category.Name == "结构框架")
                    {
                        var p0 = instance.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM);
                        if (!p0.IsReadOnly)
                        {
                            p0.Set(level.Id);
                            instance.get_Parameter(BuiltInParameter.STRUCTURAL_BEAM_END0_ELEVATION).Set(0);
                            instance.get_Parameter(BuiltInParameter.STRUCTURAL_BEAM_END1_ELEVATION).Set(0);
                        }
                    }
                    #endregion

                    transaction.Commit();
                    count++;
                }
                catch
                {
                    transaction.RollBack();
                }
                #endregion
            }
            return count;
        }

        /// <summary>
        /// 图层名称是否相同
        /// </summary>
        public static bool IsNotEqualLayerName(this Document doc, GeometryObject geoObj, GeometryObject insObj)
        {
            GraphicsStyle graphicsStyle = doc.GetElement(geoObj.GraphicsStyleId) as GraphicsStyle;
            GraphicsStyle _graphicsStyle = doc.GetElement(insObj.GraphicsStyleId) as GraphicsStyle;
            return _graphicsStyle.GraphicsStyleCategory.Name != graphicsStyle.GraphicsStyleCategory.Name;
        }

        /// <summary>
        /// 导入的单位是否正确
        /// </summary>
        public static bool IsDisplayUnits(this Document doc, Reference reference, string units = "米")
        {
            ImportInstance dwg = doc.GetElement(reference) as ImportInstance;
            CADLinkType linkType = dwg.GetTypeId().ToElement(doc) as CADLinkType;

            Task.Show(linkType.get_Parameter(BuiltInParameter.IMPORT_DISPLAY_UNITS).AsValueString());
            return linkType.get_Parameter(BuiltInParameter.IMPORT_DISPLAY_UNITS).AsValueString() == units;
        }

        /// <summary>
        /// 载入指定路径族文件 
        /// * 已开始事务 *
        /// </summary>
        public static bool LoadRfaFilePath(this Document doc, string rfaFilePath)
        {

            // 定义变量判断是否成功
            bool loadingSuccess = false;
            Transaction tran = new Transaction(doc, "loading");
            tran.Start();
            try
            {
                loadingSuccess = doc.LoadFamily(rfaFilePath);
                tran.Commit();
            }
            catch
            {
                tran.RollBack();
            }
            return loadingSuccess;
        }

        /// <summary>
        /// 族文档路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetRfaFilePath(string fileName)
        {
            string thisAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string familyNamePath = string.Format("\\Family\\{0}.rfa", fileName);
            return Path.GetDirectoryName(thisAssemblyPath) + familyNamePath;
        }

        /// <summary>
        /// 获取CAD图纸图层名称
        /// </summary>
        public static string GetLayerName(this Document doc, Reference refer)
        {
            ImportInstance dwg = doc.GetElement(refer) as ImportInstance;
            GeometryObject geoObj = dwg.GetGeometryObjectFromReference(refer);
            GraphicsStyle graphicsStyle = doc.GetElement(geoObj.GraphicsStyleId) as GraphicsStyle;
            return "图层名称：" + graphicsStyle.GraphicsStyleCategory.Name;
        }

        /// <summary>
        /// 点xyz是否在box的区域内
        /// </summary>
        public static bool InBox(this Autodesk.Revit.UI.Selection.PickedBox box, XYZ point)
        {
            var _min = new XYZ(box.Min.X <= box.Max.X ? box.Min.X : box.Max.X, box.Min.Y <= box.Max.Y ? box.Min.Y : box.Max.Y, 0);
            var _max = new XYZ(box.Min.X > box.Max.X ? box.Min.X : box.Max.X, box.Min.Y > box.Max.Y ? box.Min.Y : box.Max.Y, 0);
            return point.X >= _min.X && point.Y >= _min.Y && point.X <= _max.X && point.Y <= _max.Y;
        }

        /// <summary>
        /// 判断点是否再Solid内
        /// </summary>
        public static bool IsPointInSolid(this Solid solid, XYZ point)
        {
            if (solid != null)
            {
                SolidCurveIntersectionOptions sco = new SolidCurveIntersectionOptions();
                sco.ResultType = SolidCurveIntersectionMode.CurveSegmentsInside;
                Line line = Line.CreateBound(point, point.Add(XYZ.BasisZ));
                double tolerance = 0.00001;
                SolidCurveIntersection sci = solid.IntersectWithCurve(line, sco);
                for (int i = 0; i < sci.SegmentCount; i++)
                {
                    Curve curve = sci.GetCurveSegment(i);
                    curve.MakeUnbound();
                    var result = curve.Project(point);
                    if (point.IsAlmostEqualTo(result.XYZPoint, tolerance))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 点在curves围成区域内
        /// </summary>
        public static bool IsPointInCurves(this List<Curve> curves, XYZ point)
        {
            int insertCount = 0;
            Line rayLine = Line.CreateBound(point.Flatten(), point.Flatten().Add(XYZ.BasisY * 10000));
            foreach (var item in curves)
            {
                var interResult = rayLine.Intersect((item as Line).Flatten());
                if (interResult == SetComparisonResult.Overlap)
                {
                    insertCount += 1;
                }
            }
            return insertCount % 2 == 1;
        }

        /// <summary>
        /// curves转CurveArray
        /// </summary>
        public static CurveArray ToCurveArray(this List<Curve> curves)
        {
            CurveArray curveArray = new CurveArray();
            CurveLoop _loop = CurveLoop.Create(curves);
            if (!_loop.IsOpen())
            {
                foreach (var curve in curves)
                {
                    curveArray.Append(curve);
                }
            }
            return curveArray;
        }

        /// <summary>
        /// ID 转 Element
        /// </summary>
        public static Element ToElement(this ElementId elementId, Document doc)
        {
            return doc.GetElement(elementId);
        }

        /// <summary>
        /// Color string to Color
        /// </summary>
        public static Color ToColor(this string colorName)
        {
            var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorName);
            return new Color(color.R, color.G, color.B);
        }

        /// <summary>
        /// CurveLoop 转 solid
        /// </summary>
        public static Solid CurvesToSolid(this CurveLoop baseLoop, double height = 10)
        {
            Solid solid = null;
            try
            {
                List<CurveLoop> loopList = new List<CurveLoop>() { baseLoop };
                solid = GeometryCreationUtilities.CreateExtrusionGeometry(loopList, XYZ.BasisZ, height);
            }
            catch { }    // Curve Loops do not satisfy the input requirements.
            return solid;
        }

        /// <summary>
        /// 忽略警告弹窗提示
        /// </summary>
        /// <param name="t"></param>
        public static void NoFailure(this Transaction t)
        {
            FailureHandlingOptions fho = t.GetFailureHandlingOptions().SetFailuresPreprocessor(new FailuresPreprocessor());
            t.SetFailureHandlingOptions(fho);
        }

        /// <summary>
        /// 将数字字符串转为数字
        /// </summary>
        /// <param name="str"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Parse(this string str, double x = 1)
        {
            var t = double.TryParse(str, out double res);
            return t ? (res * x) : 0.0;
        }

        /// <summary>
        /// 过滤器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static List<T> TCollector<T>(this Document doc, bool elementType = false)
        {
            if (elementType)
            {
                return new FilteredElementCollector(doc).OfClass(typeof(T)).WhereElementIsElementType().Cast<T>().ToList();
            }
            else
            {
                return new FilteredElementCollector(doc).OfClass(typeof(T)).WhereElementIsNotElementType().Cast<T>().ToList();
            }
        }

        /// <summary>
        /// 向量拍平
        /// </summary>
        /// <param name="xyz"></param>
        /// <returns></returns>
        public static XYZ Flatten(this XYZ xyz, double z = 0)
        {
            return new XYZ(xyz.X, xyz.Y, z);
        }

        /// <summary>
        /// Line 拍平
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static Line Flatten(this Curve line, double z = 0)
        {
            return Line.CreateBound(line.GetEndPoint(0).Flatten(z), line.GetEndPoint(1).Flatten(z));
        }

        /// <summary>
        /// 毫米 <--> 英尺
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double MillimeterToFeet(this double value)
        {
            return UnitUtils.Convert(value, DisplayUnitType.DUT_MILLIMETERS, DisplayUnitType.DUT_DECIMAL_FEET);
        }

        /// <summary>
        /// 英尺 <--> 毫米
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double FeetToMillimeter(this double value)
        {
            return UnitUtils.Convert(value, DisplayUnitType.DUT_DECIMAL_FEET, DisplayUnitType.DUT_MILLIMETERS);
        }
    }
}
