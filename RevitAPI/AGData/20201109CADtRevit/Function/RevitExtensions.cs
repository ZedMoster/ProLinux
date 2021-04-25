using System;
using System.Collections.Generic;
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
        /// 点xyz是否在box的区域内
        /// </summary>
        public static bool InBox(this Autodesk.Revit.UI.Selection.PickedBox box, XYZ point)
        {
            return box.Min.Y < box.Max.Y ?
                (point.X >= box.Min.X && point.Y >= box.Min.Y) && (point.X <= box.Max.X && point.Y <= box.Max.Y) :
                (point.X >= box.Min.X && point.Y >= box.Max.Y) && (point.X <= box.Max.X && point.Y <= box.Min.Y);
        }

        /// <summary>
        /// 查找内部的文字
        /// </summary>
        /// <param name="texts"></param>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static string InslideText(this List<CADTextModel> texts, List<Curve> curves)
        {
            string text = string.Empty;
            for (int i = 0; i < texts.Count; i++)
            {
                XYZ xyz = texts[i].Location;
                if (IsInsideCurves(xyz, curves))
                {
                    text = texts[i].Text;
                    break;
                }
            }
            return text;
        }

        /// <summary>
        /// 判断点是否在一个闭合轮廓内
        /// </summary>
        /// <returns> 在内部：true</returns>
        private static bool IsInsideCurves(XYZ TargetPoint, List<Curve> curves)
        {
            int insertCount = 0;
            #region  判断点是否在一个闭合轮廓内
            Line rayLine = Line.CreateBound(TargetPoint.Flatten(), TargetPoint.Flatten().Add(XYZ.BasisX * 10000));
            foreach (var item in curves)
            {
                var interResult = rayLine.Intersect((item as Line).Flatten());
                if (interResult == SetComparisonResult.Overlap)
                    insertCount += 1;
            }
            #endregion

            return insertCount % 2 == 1;
        }

        /// <summary>
        /// 获取墙体定位线
        /// </summary>
        /// <param name="wall"></param>
        /// <returns></returns>
        public static Curve WallCurve(this Wall wall)
        {
            return (wall.Location as LocationCurve).Curve;
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
        /// 获取文字开头字母
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static char FristChar(this string str)
        {
            char PYstr = char.MinValue;
            foreach (char item in str.ToCharArray())
            {
                if (Microsoft.International.Converters.PinYinConverter.ChineseChar.IsValidChar(item))
                {
                    Microsoft.International.Converters.PinYinConverter.ChineseChar cc = new Microsoft.International.Converters.PinYinConverter.ChineseChar(item);
                    PYstr = cc.Pinyins[0][0];
                }
                else
                    PYstr = item.ToString()[0];
            }
            return PYstr;
        }

        /// <summary>
        /// double 范围内相等
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool IsAlmostEqualTo(this double a, double b, double tolerance = 0.0001)
        {
            return new XYZ(0, 0, a).IsAlmostEqualTo(new XYZ(0, 0, b), tolerance);
        }

        /// <summary>
        /// 获取分类名称
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="builtInCategory"></param>
        /// <returns></returns>
        public static string CategoryName(this Document doc, BuiltInCategory builtInCategory)
        {
            return doc.Settings.Categories.get_Item(builtInCategory).Name;
        }

        /// <summary>
        /// 删除列表中的模型
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elements"></param>
        public static void DelListElement<T>(this Document doc, List<T> elements) where T : Element
        {
            if (elements.Count == 0)
                return;
            foreach (var el in elements)
            {
                doc.Delete(el.Id);
            }
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
        /// 创建参照线
        /// </summary>
        /// <param name="xyz"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static List<Curve> CreateCurves(this XYZ xyz, double a)
        {
            a *= 0.78; // >0.5
            List<Curve> curves = new List<Curve>();
            var p0 = xyz + new XYZ(-a, a, 0);
            var p1 = xyz + new XYZ(a, a, 0);
            var p2 = xyz + new XYZ(a, -a, 0);
            var p3 = xyz + new XYZ(-a, -a, 0);
            Curve curve_0 = Line.CreateBound(p0.Flatten(), p1.Flatten());
            Curve curve_1 = Line.CreateBound(p1.Flatten(), p2.Flatten());
            Curve curve_2 = Line.CreateBound(p2.Flatten(), p3.Flatten());
            Curve curve_3 = Line.CreateBound(p3.Flatten(), p0.Flatten());
            curves.Add(curve_0);
            curves.Add(curve_1);
            curves.Add(curve_2);
            curves.Add(curve_3);

            return curves;
        }

        /// <summary>
        /// 获取Line 基于Basis 的旋转弧度值
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static double LineRotation(this Line line, XYZ Basis)
        {
            return (line.GetEndPoint(1) - line.GetEndPoint(0)).Flatten().AngleTo(Basis);
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
        /// 过滤器-当前视图
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="doc"></param>
        /// <param name="viewId"></param>
        /// <returns></returns>
        public static List<T> TCollector<T>(this Document doc, ElementId viewId, bool elementType = false)
        {
            if (elementType)
            {
                return new FilteredElementCollector(doc, viewId).OfClass(typeof(T)).WhereElementIsElementType().Cast<T>().ToList();
            }
            else
            {
                return new FilteredElementCollector(doc, viewId).OfClass(typeof(T)).WhereElementIsNotElementType().Cast<T>().ToList();
            }
        }

        /// <summary>
        /// 定位点坐标转换
        /// </summary>
        /// <param name="xyz"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static XYZ TransForm(this XYZ xyz, Transform transform)
        {
            return transform.OfPoint(xyz);
        }

        /// <summary>
        /// 获取给定标高底部或顶部的level -element
        /// </summary>
        /// <param name="level"> 参照标高</param>
        /// <param name="top"> true-顶部标高  false-底部标高</param>
        /// <returns> Level element</returns>
        public static Level NearLevel(this Level level, bool top)
        {
            var L = level.ProjectElevation;

            var eles = new FilteredElementCollector(level.Document).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType();
            List<LevelParamter> mineParaters = new List<LevelParamter>();
            foreach (var item in eles)
            {
                var el = item as Level;
                var elevation = el.ProjectElevation;
                mineParaters.Add(new LevelParamter { MLevel = el, MProjectElevation = elevation });
            }
            #region 获取参照标高
            if (mineParaters.Count > 0)
            {
                if (top)
                {
                    try
                    {
                        var res = mineParaters.Where(i => i.MProjectElevation > L).OrderBy(i => i.MProjectElevation).First();
                        return res.MLevel;
                    }
                    catch { return level; }

                }
                else
                {
                    try
                    {
                        var res = mineParaters.Where(i => i.MProjectElevation < L).OrderBy(i => i.MProjectElevation).Last();
                        return res.MLevel;
                    }
                    catch { return level; }
                }
            }
            else
            {
                return level;
            }
            #endregion
        }

        /// <summary>
        /// 逆时针旋转90° 且Z轴置为0
        /// </summary>
        /// <param name="xyz"></param>
        /// <returns></returns>
        public static XYZ Rotation(this XYZ xyz)
        {
            return new XYZ(-xyz.Y, xyz.X, 0);
        }

        /// <summary>
        /// 向量拍平
        /// </summary>
        /// <param name="xyz"></param>
        /// <returns></returns>
        public static XYZ Flatten(this XYZ xyz)
        {
            return new XYZ(xyz.X, xyz.Y, 0);
        }

        /// <summary>
        /// Line 拍平
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static Line Flatten(this Curve line)
        {
            return Line.CreateBound(line.GetEndPoint(0).Flatten(), line.GetEndPoint(1).Flatten());
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

        /// <summary>
        /// 角度 <--> 弧度
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double AngleToRadians(this double value)
        {
            return (Math.PI / 180.0) * value;
        }
    }
}
