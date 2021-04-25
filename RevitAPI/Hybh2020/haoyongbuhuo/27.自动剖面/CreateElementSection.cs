using System.Linq;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class CreateElementSection : IExternalCommand
    {
        public Level LevelTarge { get; set; }
        public XYZ P0 { get; set; }
        public XYZ P1 { get; set; }
        public Element FindSectionView { get; set; }
        public double TopLevleEla { get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            if (Run.Running(Strings.key))
            {
                #region 初始化参数
                // 设置剖面左右边距及顶底边距
                var set_LR = UUtools.MillimetersToUnits(500);
                var set_TB = UUtools.MillimetersToUnits(200);
                // 注册列表操作
                RegistryStorage Registry = new RegistryStorage();
                string sectionName = Registry.OpenAfterStart("Section_Name") ?? "快速剖面";
                var _height = Registry.OpenAfterStart("Section_Offset") ?? "400";
                var _offset = Registry.OpenAfterStart("Section_Offset") ?? "500";
                var dia_offset = double.TryParse(_offset, out double set_offsetMM);
                var dia_height = double.TryParse(_height, out double offsetMM);
                if (!dia_offset)
                {
                    TaskDialog.Show("提示", "全局设置中<剖面偏移>参数输入错误！");
                    return Result.Failed;
                }
                if (!dia_height)
                {
                    TaskDialog.Show("提示", "全局设置中<剖面深度>参数输入错误！");
                    return Result.Failed;
                }
                var set_offset = UUtools.MillimetersToUnits(set_offsetMM);
                var offset = UUtools.MillimetersToUnits(offsetMM);
                #endregion

                #region 选择构件及方向
                try
                {
                    PickFilterLocationCurve pickByLocationCurve = new PickFilterLocationCurve();
                    SelPick.SelRef = sel.PickObject(ObjectType.Element, pickByLocationCurve, "选择构件");
                    P0 = SelPick.SelRef.GlobalPoint;
                }
                catch { }
                if (SelPick.SelRef == null)
                {
                    return Result.Cancelled;
                }

                try
                {
                    // 设置剖面的方向
                    P1 = sel.PickPoint("基于定位点点击设置剖面方向");
                }
                catch { }
                if (P1 == null)
                {
                    return Result.Cancelled;
                }
                #endregion

                // 获取构件的定位线
                var el = doc.GetElement(SelPick.SelRef);
                LocationCurve lc = el.Location as LocationCurve;
                Line line = lc.Curve as Line;

                // 获取视图类型对象
                ViewFamilyType vft = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType))
                    .Cast<ViewFamilyType>().FirstOrDefault<ViewFamilyType>(x => ViewFamily.Section == x.ViewFamily);

                #region 计算视图区域
                if (el is Wall)
                {
                    LevelTarge = doc.GetElement(el.LevelId) as Level;
                }
                else if (el is MEPCurve)
                {
                    LevelTarge = (el as MEPCurve).ReferenceLevel;
                }
                else if (el is FamilyInstance)
                {
                    LevelTarge = (el as FamilyInstance).Host as Level;
                }
                else
                {
                    // 获取构件参照标高
                    TaskDialog.Show("提示", "设置获取此别构构件参照标高");
                    return Result.Failed;
                }

                var BaseLevelElevation = LevelTarge.ProjectElevation;
                GetLevel getLevel = new GetLevel();
                var topLevel = getLevel.NearLevel(LevelTarge, true);
                // 如果默认标高是顶部标高 则默认高度 5000 
                if (topLevel.Id.IntegerValue.Equals(LevelTarge.Id.IntegerValue))
                {
                    //TaskDialog.Show("NO Top", "No Top Level");
                    TopLevleEla = BaseLevelElevation + UUtools.MillimetersToUnits(5000);
                }
                else
                {
                    TopLevleEla = topLevel.ProjectElevation;
                }

                XYZ stratPnt = line.GetEndPoint(0);
                XYZ endPnt = line.GetEndPoint(1);
                XYZ v = endPnt - stratPnt;

                BoundingBoxXYZ bb = el.get_BoundingBox(null);
                double minZ = bb.Min.Z;
                double maxZ = bb.Max.Z;
                // 获取向量的长度
                double w = v.GetLength();

                // 按标高计算
                XYZ min = new XYZ(-0.5 * w - set_LR - offset, BaseLevelElevation - offset - set_TB, 0 - set_offset);
                XYZ max = new XYZ(0.5 * w + set_LR + offset, TopLevleEla + offset + set_TB, offset);

                XYZ midpoint = stratPnt + 0.5 * v;

                // 剖面方向
                var dir = AutoDirection(v, P0, P1);

                Transform t = Transform.Identity;
                t.Origin = new XYZ(midpoint.X, midpoint.Y, 0);
                t.BasisX = dir;
                t.BasisY = XYZ.BasisZ;
                t.BasisZ = dir.CrossProduct(XYZ.BasisZ);

                BoundingBoxXYZ sectionBox = new BoundingBoxXYZ
                {
                    Transform = t,
                    Min = min,
                    Max = max
                };
                #endregion

                #region 创建构件剖面视图
                Transaction tr = new Transaction(doc, "构件剖面");
                tr.Start();
                // 创建剖面视图
                try
                {
                    FindSectionView = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).WhereElementIsNotElementType()
                    .ToElements().ToList().Find(x => x.Name == sectionName);
                    doc.Delete(FindSectionView.Id);
                }
                catch { }

                try
                {
                    var ViewSectionIns = ViewSection.CreateSection(doc, vft.Id, sectionBox);
                    ViewSectionIns.CropBoxActive = true;
                    ViewSectionIns.DisplayStyle = DisplayStyle.ShadingWithEdges;
                    ViewSectionIns.DetailLevel = ViewDetailLevel.Fine;

                    ViewSectionIns.Name = sectionName;
                    tr.Commit();

                    //跳转到改视图
                    uidoc.ActiveView = ViewSectionIns;
                }
                catch
                {
                    tr.RollBack();
                }
                #endregion
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// 获取方向
        /// </summary>
        public XYZ TargDir { get; set; }
        public XYZ AutoDirection(XYZ v, XYZ p0, XYZ p1)
        #region 获取原点到方向的差值得到方向
        {
            // 获取构件的朝向
            var dir = v.Normalize();

            #region 水平向右
            if (dir.IsAlmostEqualTo(XYZ.BasisX))
            {
                //TaskDialog.Show("----", "水平构件-- 左");
                //TaskDialog.Show("dir", dir.X.ToString());
                if (p0.Y < p1.Y)
                {
                    // 上
                    TargDir = v.Normalize().Negate();
                }
                else
                {
                    // 下
                    TargDir = v.Normalize();
                }
            }
            #endregion

            #region 水平向左
            else if (dir.IsAlmostEqualTo(-XYZ.BasisX))
            {
                //TaskDialog.Show("----", "水平构件-- 右");
                //TaskDialog.Show("dir", dir.X.ToString());
                if (p0.Y < p1.Y)
                {
                    // 上
                    TargDir = v.Normalize();
                }
                else
                {
                    // 下
                    TargDir = v.Normalize().Negate();

                }
            }
            #endregion

            #region 判断不水平的构件
            else
            {
                if ( p0.X < p1.X)
                {
                    if (dir.Y >= 0)
                    {
                        TargDir = v.Normalize();
                    }
                    else
                    {
                        TargDir = v.Normalize().Negate();
                    }
                }
                else
                {
                    if (dir.Y >= 0)
                    {
                        TargDir = v.Normalize().Negate();
                    }
                    else
                    {
                        TargDir = v.Normalize();
                    }
                }
            }
            #endregion

            return TargDir;
        }
        #endregion
    }
}
