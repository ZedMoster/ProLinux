using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using CADReader.WPF;

namespace CADReader
{
    /// <summary>
    /// 选择模型类别图元
    /// </summary>
    class PickCADFilter : ISelectionFilter
    {
        public bool AllowElement(Element el)
        {
            if (el.Category.Name.EndsWith(".dwg") || el.Category.Name.EndsWith(".dwg2"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }

    /// <summary>
    /// 指定类别
    /// </summary>
    class PickFilterCategory : ISelectionFilter
    {
        public string CategoryName { get; set; }
        public PickFilterCategory(string cate)
        {
            CategoryName = cate;
        }
        public bool AllowElement(Element elem)
        {
            return elem.Category.Name == CategoryName;
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }

    class Pickdwg
    {
        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="sel"></param>
        /// <param name="reference"></param>
        /// <returns></returns>
        public bool Refer(Selection sel, out Reference reference)
        {
            PickCADFilter pickCAD = new PickCADFilter();
            try
            {
                reference = sel.PickObject(ObjectType.PointOnElement, pickCAD, "选择CAD图元");
                return true;
            }
            catch
            {
                reference = null;
                return false;
            }
        }
        /// <summary>
        /// 获取列表数据
        /// </summary>
        /// <param name="sel"></param>
        /// <param name="references"></param>
        /// <returns></returns>
        public bool RefList(Selection sel, out List<Reference> references)
        {
            PickCADFilter pickCAD = new PickCADFilter();
            try
            {
                references = sel.PickObjects(ObjectType.PointOnElement, pickCAD, "选择多个CAD图元").ToList();
                return true;
            }
            catch
            {
                references = new List<Reference>();
                return false;
            }
        }
    }

    /// <summary>
    /// 关闭警告弹窗
    /// </summary>
    class FailuresPreprocessor : IFailuresPreprocessor
    {
        /// <summary>
        /// 捕获警告
        /// </summary>
        /// <param name="failuresAccessor"></param>
        /// <returns></returns>
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            IList<FailureMessageAccessor> listFma = failuresAccessor.GetFailureMessages();
            if (listFma.Count == 0)
                return FailureProcessingResult.Continue;
            foreach (FailureMessageAccessor fma in listFma)
            {
                if (fma.GetSeverity() == FailureSeverity.Error)
                {
                    if (fma.HasResolutions())
                        failuresAccessor.ResolveFailure(fma);
                }
                if (fma.GetSeverity() == FailureSeverity.Warning)
                {
                    failuresAccessor.DeleteWarning(fma);
                }
            }
            return FailureProcessingResult.ProceedWithCommit;
        }
    }

    /// <summary>
    /// 创建墙体
    /// </summary>
    class WallManage
    {
        public char NameContains { get; set; }
        public WallManage(char containsChar)
        {
            NameContains = containsChar;
        }

        /// <summary>
        /// 获取选择的 Family类型
        /// </summary>
        /// <param name="text"></param>
        /// <param name="doors"></param>
        /// <returns></returns>
        public Family GetFamily(CADTextModel text, List<CADModel> doors, out CADModel selectDoor)
        {
            try
            {
                selectDoor = doors.FirstOrDefault(i => i.SymbolName == text.Text);
                var familyList = selectDoor?.FamilyList;
                var selectName = selectDoor?.SelectFamilyName;
                return familyList.FirstOrDefault(i => i.FamilyName == selectName)?.Family;
            }
            catch (Exception)
            {
                selectDoor = null;
                return null;
            }
        }

        /// <summary>
        /// 创建门窗实例
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="doors"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool CreateFamilySymbol(Document doc, List<CADModel> doors, CADTextModel text)
        {
            var family = GetFamily(text, doors, out CADModel thisDoor);
            if (family == null)
                return false;
            // 获取所有墙体
            var els = doc.TCollector<Wall>(doc.ActiveView.Id);

            #region 获取指定类型名称的类型 FamilySymbol
            FamilySymbol doorType = null;
            var needId = family.GetFamilySymbolIds().FirstOrDefault(
                x => (doc.GetElement(x) as FamilySymbol).Name == text.Text);
            if (needId != null)
                doorType = doc.GetElement(needId) as FamilySymbol;
            if (doorType == null)
            {
                Transaction tcopy = new Transaction(doc);
                tcopy.Start("创建类型");
                var elType = doc.GetElement(family.GetFamilySymbolIds().FirstOrDefault()) as FamilySymbol;
                doorType = elType.Duplicate(text.Text) as FamilySymbol;
                tcopy.Commit();
            }
            #endregion

            try
            {
                //MessageBox.Show($"宽度：{thisDoor.Width}\n高度：{thisDoor.Higth}");
                var width = thisDoor.Width.MillimeterToFeet();
                var hight = thisDoor.Higth.MillimeterToFeet();
                if (width == 0 || hight == 0)
                    return false;

                // 获取门 host 墙体
                var walls = GetTextHostWall(text, els, width);

                #region 找到主体
                Wall wall;
                if (walls.Count >= 2)
                    wall = MergeNewWall(doc, walls);
                else
                    wall = NearWall(els, text);
                if (wall == null)
                    return false;
                #endregion

                #region 创建实例
                var wall_curve = (wall.Location as LocationCurve).Curve;
                var xyz = wall_curve.Project(text.Location).XYZPoint;
                // 创建门
                Transaction t = new Transaction(doc, "创建窗");
                t.Start();
                t.NoFailure();
                if (!doorType.IsActive)
                    doorType.Activate();
                var instance = doc.Create.NewFamilyInstance(xyz, doorType, wall, doc.ActiveView.GenLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                UpdataParameter(instance, width, hight, thisDoor);
                t.Commit();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            #endregion
        }

        /// <summary>
        /// 更新族参数
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <returns></returns>
        private bool UpdataParameter(FamilyInstance familyInstance, double width, double higth, CADModel thisDoor)
        {
            // 底高度
            familyInstance.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).Set(thisDoor.Lhight.MillimeterToFeet());
            var symbol = familyInstance.Symbol;
            // 宽度高度
            symbol.get_Parameter(BuiltInParameter.DOOR_WIDTH).Set(width);
            symbol.get_Parameter(BuiltInParameter.GENERIC_HEIGHT).Set(higth);
            return true;
        }

        /// <summary>
        /// 合并最近的两个墙体
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="walls"></param>
        /// <returns></returns>
        private Wall MergeNewWall(Document doc, List<Wall> walls)
        {
            Wall thisWall;
            var wallCurve = MergeWallCurve(walls);
            Transaction t = new Transaction(doc, "合并墙");
            t.Start();
            t.NoFailure();
            var IsStruct = walls.LastOrDefault().get_Parameter(BuiltInParameter.WALL_STRUCTURAL_SIGNIFICANT).AsInteger();
            var levelId = walls.LastOrDefault().get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).AsElementId();
            thisWall = Wall.Create(doc, wallCurve.Curve, doc.ActiveView.GenLevel.Id, IsStruct == 1);
            thisWall.ChangeTypeId(walls.LastOrDefault().WallType.Id);
            if (levelId != doc.ActiveView.GenLevel.Id)
                thisWall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(levelId);
            // 删除原有墙体
            doc.DelListElement(walls);
            t.Commit();

            return thisWall;
        }

        /// <summary>
        /// 获取最近的墙体
        /// </summary>
        /// <param name="els"></param>
        /// <param name="textModel"></param>
        /// <returns></returns>
        private Wall NearWall(List<Wall> els, CADTextModel textModel)
        {
            Wall thisWall = null;
            if (els.Count == 0)
                return thisWall;
            else
            {
                var location = textModel.Location;
                var textRotation = textModel.Rotation;
                var minDistance = 1000.0.MillimeterToFeet();
                foreach (var wall in els)
                {
                    var wallCurve = (wall.Location as LocationCurve).Curve;
                    if (wallCurve is Line wallLine)
                    {
                        var rota = wallLine.LineRotation(XYZ.BasisX).IsAlmostEqualTo(textRotation);
                        var rota_n = wallLine.LineRotation(XYZ.BasisX.Negate()).IsAlmostEqualTo(textRotation);
                        if (rota || rota_n)
                        {
                            var intersectionResult = wallLine.Flatten().Project(location.Flatten());
                            if (intersectionResult.Distance < minDistance)
                            {
                                minDistance = intersectionResult.Distance;
                                thisWall = wall;
                            }
                        }
                    }
                }
            }
            return thisWall;
        }

        /// <summary>
        /// 合并墙体定位线
        /// </summary>
        /// <param name="walls"></param>
        /// <returns></returns>
        private WallCurve MergeWallCurve(List<Wall> walls)
        {
            List<XYZ> xyzs = new List<XYZ>();
            foreach (var item in walls)
            {
                var curve = (item.Location as LocationCurve).Curve;
                xyzs.Add(curve.GetEndPoint(0));
                xyzs.Add(curve.GetEndPoint(1));
            }
            List<WallCurve> wallCurves = new List<WallCurve>();
            List<int> cout = new List<int>();
            for (int i = 0; i < xyzs.Count; i++)
            {
                for (int j = 0; j < xyzs.Count; j++)
                {
                    if (i == j)
                        continue;
                    if (cout.Contains(j))
                        continue;
                    try
                    {
                        var dis = xyzs[i].DistanceTo(xyzs[j]);
                        Curve curve = Line.CreateBound(xyzs[j], xyzs[i]);
                        wallCurves.Add(new WallCurve() { Length = dis, Curve = curve });
                        cout.Add(i);
                    }
                    catch { }
                }
            }
            //MessageBox.Show(wallCurves.Count.ToString());
            return wallCurves.OrderBy(i => i.Length).LastOrDefault();
        }

        /// <summary>
        /// CAD图纸文字 Host墙体
        /// </summary>
        /// <param name="text"></param>
        /// <param name="els"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private List<Wall> GetTextHostWall(CADTextModel text, List<Wall> els, double width)
        {
            List<Wall> walls = new List<Wall>();
            var curves = text.Location.CreateCurves(width);
            if (els.Count == 0)
                return walls;

            foreach (var item in els)
            {
                var b = IsInside(item, curves, text.Rotation);
                if (b)
                    walls.Add(item);
            }
            // 等距墙体
            return EqualsWalls(walls, text.Location);
        }

        /// <summary>
        /// 等距的墙体
        /// </summary>
        /// <param name="walls"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        private List<Wall> EqualsWalls(List<Wall> walls, XYZ location)
        {
            List<Wall> newWalls = new List<Wall>();
            double minDistance = MinDistance(walls, location);
            foreach (var wall in walls)
            {
                var curve = wall.WallCurve().Flatten();
                curve.MakeUnbound();
                var distance = curve.Project(location.Flatten()).Distance;
                if (distance.IsAlmostEqualTo(minDistance, 0.0001))
                    newWalls.Add(wall);
            }
            return newWalls;
        }

        /// <summary>
        /// 距离最小值
        /// </summary>
        /// <param name="walls"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        private double MinDistance(List<Wall> walls, XYZ location)
        {
            double minDistance = double.MaxValue;
            foreach (var wall in walls)
            {
                var curve = wall.WallCurve().Flatten();
                curve.MakeUnbound();
                var distance = curve.Project(location.Flatten()).Distance;
                if (distance < minDistance)
                    minDistance = distance;
            }
            return minDistance == double.MaxValue ? 0 : minDistance;
        }

        /// <summary>
        /// 判断墙体是否在范围内
        /// </summary>
        /// <returns> 在内部：true</returns>
        private bool IsInside(Wall wall, List<Curve> curves, double rotation = 0)
        {
            var wallCurve = (wall.Location as LocationCurve).Curve;
            if (wallCurve is Line wallLine)
            {
                if (!wallLine.LineRotation(XYZ.BasisX).IsAlmostEqualTo(rotation)
                    && !wallLine.LineRotation(XYZ.BasisX.Negate()).IsAlmostEqualTo(rotation))
                    return false;
                #region  判断点是否在一个闭合轮廓内
                int insertCount = 0;
                foreach (var rayLine in curves)
                {
                    var interResult = (wallCurve as Line).Flatten().Intersect(rayLine);
                    if (interResult == SetComparisonResult.Overlap)
                        insertCount += 1;
                }
                #endregion
                if (insertCount != 0)
                    return insertCount % 2 == 1;
                else
                {
                    var p0 = (wallCurve as Line).Flatten().GetEndPoint(0);
                    var p1 = (wallCurve as Line).Flatten().GetEndPoint(1);
                    var bool_0 = IsInsideCurves(p0, curves);
                    var bool_1 = IsInsideCurves(p1, curves);
                    return bool_0 && bool_1;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断点是否在一个闭合轮廓内
        /// </summary>
        /// <returns> 在内部：true</returns>
        private bool IsInsideCurves(XYZ TargetPoint, List<Curve> curves)
        {
            int insertCount = 0;
            Line rayLine = Line.CreateBound(TargetPoint.Flatten(), TargetPoint.Flatten().Add(XYZ.BasisX * 10000));
            foreach (var item in curves)
            {
                var interResult = rayLine.Intersect((item as Line).Flatten());
                if (interResult == SetComparisonResult.Overlap)
                    insertCount += 1;
            }
            return insertCount % 2 == 1;
        }
    }

    /// <summary>
    /// 宽度列表
    /// </summary>
    public class EnumCAD
    {
        /// <summary>
        /// Wallwidth 所有的值
        /// </summary>
        public List<int> EnumValues { get; set; }

        public EnumCAD()
        {
            List<int> Evalues = new List<int>();
            foreach (var val in Enum.GetValues(typeof(Elementwidth)))
            {
                Evalues.Add((int)(val));
            }
            this.EnumValues = Evalues;
        }

        /// <summary>
        /// =宽度=
        /// </summary>
        public enum Elementwidth
        {
            a = 100,
            b = 150,
            c = 200,
            d = 250,
            e = 300,
            f = 350,
            g = 400,
            h = 450,
            i = 500,
            j = 550,
            k = 600,
        };
    }

    /// <summary>
    /// 创建屋顶
    /// </summary>
    class RoofManage
    {
        /// <summary>
        /// 获取 CurveArray
        /// </summary>
        /// <param name="curves"> Curve 列表</param>
        /// <returns></returns>
        public CurveArray ListCurveToArray(List<Curve> curves)
        {
            CurveArray curveArray = new CurveArray();
            foreach (var curve in curves)
            {
                if (curve is Line)
                    curveArray.Append(curve);
            }
            return curveArray;
        }

        /// <summary>
        /// 获取选择的墙的边界线
        /// </summary>
        /// <param name="uidoc"></param>
        /// <param name="curveArray"></param>
        /// <returns></returns>
        public List<Wall> GetCurveArrayInWalls(UIDocument uidoc, out CurveArray curveArray)
        {
            curveArray = new CurveArray();
            var doc = uidoc.Document;
            List<Wall> els = new List<Wall>();
            ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();
            if (selectedIds.Count != 0)
            {
                foreach (ElementId id in selectedIds)
                {
                    Element element = doc.GetElement(id);
                    if (element is Wall wall)
                    {
                        LocationCurve wallCurve = wall.Location as LocationCurve;
                        curveArray.Append(wallCurve.Curve);
                        els.Add(wall);
                    }
                }
            }
            else
            {
                var refs = new List<Reference>();
                PickFilterCategory pickFilter = new PickFilterCategory("墙");
                try
                {
                    refs = uidoc.Selection.PickObjects(ObjectType.Element, pickFilter, "选择墙体作为创建楼板的边界线").ToList();
                }
                catch { }
                if (refs.Count != 0)
                {
                    foreach (Reference reference in refs)
                    {
                        var wall = doc.GetElement(reference) as Wall;
                        LocationCurve wallCurve = wall.Location as LocationCurve;
                        curveArray.Append(wallCurve.Curve);
                        els.Add(wall);
                    }
                }
            }
            return els;
        }

        /// <summary>
        /// 创建屋顶
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="footprint"></param>
        /// <param name="level"></param>
        /// <param name="roofType"></param>
        /// <param name="slopAngle"></param>
        public bool CreateRoof(Document doc, CurveArray footprint, Level level, RoofType roofType, double slopAngle)
        {
            bool push = true;
            Transaction tran = new Transaction(doc, "创建屋顶");
            tran.Start();
            try
            {
                ModelCurveArray footPrintToModelCurveMapping = new ModelCurveArray();
                FootPrintRoof footprintRoof = doc.Create.NewFootPrintRoof(footprint, level, roofType, out footPrintToModelCurveMapping);
                ModelCurveArrayIterator iterator = footPrintToModelCurveMapping.ForwardIterator();
                iterator.Reset();
                while (iterator.MoveNext())
                {
                    ModelCurve modelCurve = iterator.Current as ModelCurve;
                    footprintRoof.set_DefinesSlope(modelCurve, true);
                    footprintRoof.set_SlopeAngle(modelCurve, Math.Tan(slopAngle.AngleToRadians()));
                }
                tran.Commit();
            }
            catch (Exception)
            {
                tran.RollBack();
                push = false;
            }
            return push;
        }

        /// <summary>
        /// 创建依据楼板名称厚度创建新的类型  存在相同的更新厚度参数
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="floorName"> 楼板类型名称</param>
        /// <param name="width"> 楼板厚度</param>
        /// <returns></returns>
        public RoofType CreateNewType(Document doc, string typeName, double width, Material material)
        {
            Transaction trans = new Transaction(doc, "新建类型");
            trans.Start();

            // 创建滤器
            var Types = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Roofs).WhereElementIsElementType().ToElements();
            // 判断是否已经存在该类型
            var oldFloorType = Types.Where(x => x.Name == typeName).ToList();

            if (oldFloorType.Count() == 0)
            {
                var newFloorType = Types[0] as RoofType;
                var newtype = newFloorType.Duplicate(typeName) as RoofType;
                var createSingleLayerCompoundStructure = CompoundStructure.CreateSingleLayerCompoundStructure(
                    MaterialFunctionAssignment.Structure, width.MillimeterToFeet(), material.Id);
                createSingleLayerCompoundStructure.EndCap = EndCapCondition.NoEndCap;
                // 设置复合结构
                newtype.SetCompoundStructure(createSingleLayerCompoundStructure);
                trans.Commit();

                return newtype;
            }
            else
            {
                // 已存在类型 更新厚度参数
                var newType = oldFloorType.First() as RoofType;
                var createSingleLayerCompoundStructure = CompoundStructure.CreateSingleLayerCompoundStructure(
                    MaterialFunctionAssignment.Structure, width, material.Id);
                createSingleLayerCompoundStructure.EndCap = EndCapCondition.NoEndCap;
                newType.SetCompoundStructure(createSingleLayerCompoundStructure);
                trans.Commit();
                return newType;
            }
        }
    }

    /// <summary>
    /// 创建停车位
    /// </summary>
    class ParkingManage
    {
        /// <summary>
        /// 停车位族名称
        /// </summary>
        public string FamilyName { get; set; }
        public ParkingManage(string familyName)
        {
            FamilyName = familyName;
        }

        /// <summary>
        /// 创建矩形停车位
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="xyzs"></param>
        /// <param name="texts"></param>
        /// <returns></returns>
        public bool CreateRectangleParking(Document doc, List<XYZ> xyzs, List<CADTextModel> texts)
        {
            // 获取尺寸参数
            var p0 = xyzs[0];
            var p1 = xyzs[1];
            var p2 = xyzs[2];
            var locationPoint = (p0 + p2) / 2;
            // 尺寸参数
            double b = p1.DistanceTo(p2);
            double h = p1.DistanceTo(p0);

            #region 获取内部停车位编号名称
            string cateNum = "停车位";
            for (int i = 0; i < texts.Count; i++)
            {
                if (IsInside(texts[i].Location, xyzs))
                {
                    cateNum = texts[i].Text;
                    break;
                }
            }
            var type = GetFamilySymbol(doc, BuiltInCategory.OST_Parking, FamilyName.Split('.')[0], cateNum);
            if (type == null)
                return false;
            #endregion

            #region 创建族文件更新参数
            Transaction tranC = new Transaction(doc, "创建模型");
            tranC.Start();
            tranC.NoFailure();
            // 创建实例
            if (!type.IsActive)
                type.Activate();
            var instance = doc.Create.NewFamilyInstance(locationPoint, type, doc.ActiveView.GenLevel, StructuralType.NonStructural);
            // 更新参数
            var push = UpdateParatemer(instance, locationPoint, xyzs, b, h, cateNum);
            tranC.Commit();
            #endregion

            return push;
        }

        /// <summary>
        /// 判断点是否在一个闭合轮廓内
        /// </summary>
        /// <returns> true 在内部</returns>
        public bool IsInside(XYZ TargetPoint, List<XYZ> xyzs)
        {
            int insertCount = 0;

            #region  判断点是否在一个闭合轮廓内
            var TarPoint = new XYZ(TargetPoint.X, TargetPoint.Y, 0);
            Line rayLine = Line.CreateBound(TarPoint, TarPoint.Add(XYZ.BasisX * 10000));
            for (int i = 1; i < xyzs.Count; i++)
            {
                XYZ start = xyzs[i - 1];
                XYZ end = xyzs[i];
                var line = Line.CreateBound(new XYZ(start.X, start.Y, 0), new XYZ(end.X, end.Y, 0));
                var interResult = line.Intersect(rayLine);
                if (interResult == SetComparisonResult.Overlap)
                    insertCount += 1;
            }
            #endregion

            return insertCount % 2 == 1;
        }

        /// <summary>
        /// 更新族实例参数
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="loc"></param>
        /// <param name="xyzs"></param>
        /// <param name="b"></param>
        /// <param name="h"></param>
        /// <param name="cateNum"></param>
        /// <returns></returns>
        public bool UpdateParatemer(FamilyInstance instance, XYZ loc, List<XYZ> xyzs, double b, double h, string cateNum)
        {
            try
            {
                // 参数名称
                string paraterNameB = "停车场宽度";
                string paraterNameH = "停车场长度";
                string Paraternumber = "编号";

                // 实例参数
                var p_b = instance.LookupParameter(paraterNameB);
                var p_h = instance.LookupParameter(paraterNameH);
                if (p_b != null && p_h != null)
                {
                    p_b.Set(b);
                    p_h.Set(h);
                }
                // 类型参数
                var p_s_b = instance.Symbol.LookupParameter(paraterNameB);
                var p_s_h = instance.Symbol.LookupParameter(paraterNameH);
                if (p_s_b != null && p_s_h != null)
                {
                    p_s_b.Set(b);
                    p_s_h.Set(h);
                }
                // 编号参数
                var p_n = instance.LookupParameter(Paraternumber);
                if (p_n != null)
                {
                    p_n.Set(cateNum);
                }
                else
                {
                    var p_n_ins = instance.Symbol.LookupParameter(Paraternumber);
                    if (p_n_ins != null)
                        p_n_ins.Set(cateNum);
                    else
                        instance.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(cateNum);
                }
                instance.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(0);
                // 旋转轴及转动角度
                var axis = Line.CreateBound(loc, loc + XYZ.BasisZ);
                instance.Location.Rotate(axis, GetRoationBaseY(xyzs));
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取族类型名称获取指定族类型
        /// </summary>
        /// <param name="doc"> </param>
        /// <param name="familyName"> 族文件名称 FamilyName</param>
        /// <param name="name"> 族类型名称 FamilyName</param>
        /// <returns> 类型 FamilySymbol</returns>
        public FamilySymbol GetFamilySymbol(Document doc, BuiltInCategory builtInCategory, string familyName, string name)
        {
            // 定义变量
            FamilySymbol symbol = null;
            // 载入的family 
            Family family = null;
            // 获取所有的结构框架类型
            var elsType = new FilteredElementCollector(doc).OfCategory(builtInCategory).WhereElementIsElementType().ToList();

            // 判断是否需要载入族文件
            bool loadingFamily = elsType.Any(x => (x as FamilySymbol).FamilyName == familyName);
            if (loadingFamily)
            {
                // 通过族类型及名称获取指定的类型
                symbol = elsType.FirstOrDefault(x => (x as FamilySymbol).Name == name) as FamilySymbol;
                // 创建类型
                if (symbol == null)
                {
                    var elType = elsType.FirstOrDefault(x => (x as FamilySymbol).FamilyName == familyName) as ElementType;
                    Transaction tcopy = new Transaction(doc);
                    tcopy.Start("创建类型");
                    symbol = elType.Duplicate(name) as FamilySymbol;
                    tcopy.Commit();
                }
            }
            else
            {
                // 载入族文件
                Transaction tload = new Transaction(doc);
                tload.Start("加载族文件");
                string familyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + string.Format("\\Family\\{0}", FamilyName);
                doc.LoadFamily(familyPath, out family);
                // 更新类型名称
                family.Name = familyName;
                tload.Commit();
                // 获取载入类型id
                var needId = family.GetFamilySymbolIds().FirstOrDefault(x => (doc.GetElement(x) as FamilySymbol).Name == name);
                if (needId != null)
                {
                    // 获取指定类型
                    symbol = doc.GetElement(needId) as FamilySymbol;
                }
                // 复制新类型
                if (symbol == null)
                {
                    Transaction tcopy = new Transaction(doc);
                    tcopy.Start("创建类型");
                    var elType = doc.GetElement(family.GetFamilySymbolIds().FirstOrDefault()) as FamilySymbol;
                    symbol = elType.Duplicate(name) as FamilySymbol;
                    tcopy.Commit();
                }
            }

            // 获取指定指定族的族类型
            return symbol;
        }

        /// <summary>
        /// 获取矩形旋转的角度
        /// </summary>
        /// <param name="YXZs"> 矩形定位点</param>
        /// <returns></returns>
        public double GetRoationBaseY(List<XYZ> YXZs)
        {
            // 定义参数
            double Angle = 0.00;
            // 获取尺寸参数
            var p0 = YXZs[0];
            var p1 = YXZs[1];
            var p2 = YXZs[2];
            var localPoint = (p0 + p2) / 2;
            var LineMidPoint = (p1 + p2) / 2;

            #region 选择定位构件
            Line line = Line.CreateBound(localPoint, LineMidPoint);
            var lineDirection = line.Direction;
            var _angle = lineDirection.AngleTo(XYZ.BasisX);

            if (_angle > Math.PI / 4)
            {
                if (localPoint.Y > LineMidPoint.Y)
                {
                    Angle = lineDirection.AngleTo(XYZ.BasisY.Negate()) * -1;
                }
                else
                {
                    Angle = lineDirection.AngleTo(XYZ.BasisY.Negate());
                }
            }
            else
            {
                if (localPoint.X > LineMidPoint.X)
                {
                    Angle = _angle;
                }
                else
                {
                    Angle = -_angle;
                }
            }
            #endregion
            return Angle;
        }
    }

    /// <summary>
    /// 创建地形
    /// </summary>
    class TopographyManage
    {
        /// <summary>
        /// 创建楼板
        /// </summary>
        /// <param name="doc"> 当前文档</param>
        /// <param name="curveArray"> 楼板边界线</param>
        /// <param name="floorType"> 楼板类型</param>
        /// <param name="elva"> 偏移值 默认为0</param>
        /// <returns></returns>
        public bool CreateFloor(Document doc, CurveArray curveArray, FloorType floorType, double elva = 0.0)
        {
            bool push = true;
            Transaction t = new Transaction(doc);
            t.Start("创建楼板");

            // 关闭警告
            FailureHandlingOptions fho = t.GetFailureHandlingOptions();
            fho.SetFailuresPreprocessor(new FailuresPreprocessor());
            t.SetFailureHandlingOptions(fho);

            try
            {
                var floor = doc.Create.NewFloor(curveArray, floorType, doc.ActiveView.GenLevel, false);
                // 设置标高偏移值
                floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).Set(elva.MillimeterToFeet());
                t.Commit();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "提示");
                t.RollBack();
                push = false;
            }
            return push;
        }

        /// <summary>
        /// 获取 CurveArray
        /// </summary>
        /// <param name="curves"> Curve 列表</param>
        /// <returns></returns>
        public CurveArray ListCurveToArray(List<Curve> curves)
        {
            CurveArray curveArray = new CurveArray();
            foreach (var curve in curves)
            {
                curveArray.Append(curve);
            }
            return curveArray;
        }

        /// <summary>
        /// 创建依据楼板名称厚度创建新的类型  存在相同的更新厚度参数
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="floorName"> 楼板类型名称</param>
        /// <param name="width"> 楼板厚度</param>
        /// <returns></returns>
        public FloorType CreateNewFloorType(Document doc, string floorName, double width, Material material)
        {
            Transaction trans = new Transaction(doc, "新建类型");
            trans.Start();

            // 创建滤器
            var floorTypes = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Floors).WhereElementIsElementType().ToElements();
            // 判断是否已经存在该类型
            var oldFloorType = floorTypes.Where(x => x.Name == floorName).ToList();

            if (oldFloorType.Count() == 0)
            {
                var newFloorType = floorTypes[0] as FloorType;
                // 创建新类型，使用Duplicate方法
                var newtype = newFloorType.Duplicate(floorName) as FloorType;
                // 创建复合结构
                var createSingleLayerCompoundStructure = CompoundStructure.CreateSingleLayerCompoundStructure(
                    MaterialFunctionAssignment.Structure, width.MillimeterToFeet(), material.Id);
                createSingleLayerCompoundStructure.EndCap = EndCapCondition.NoEndCap;
                // 设置复合结构
                newtype.SetCompoundStructure(createSingleLayerCompoundStructure);
                trans.Commit();

                return newtype;
            }
            else
            {
                // 已存在类型 更新厚度参数
                var newFloorType = oldFloorType.First() as FloorType;
                var createSingleLayerCompoundStructure = CompoundStructure.CreateSingleLayerCompoundStructure(
                    MaterialFunctionAssignment.Structure, width, material.Id);
                createSingleLayerCompoundStructure.EndCap = EndCapCondition.NoEndCap;
                newFloorType.SetCompoundStructure(createSingleLayerCompoundStructure);
                trans.Commit();
                return newFloorType;
            }
        }
    }
}
