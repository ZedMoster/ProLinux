using System.Linq;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class CreateSectionQuickLy : IExternalCommand
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
                #region 设置默认参数
                // 设置剖面左右边距及顶底边距
                var set_LR = 500 / 304.8;
                var set_TB = 200 / 304.8;
                // 设置剖面定位相对定位线的偏移值
                var set_offset = 0;
                // 剖面深度
                double offset = 50 / 304.8;
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
                    TaskDialog.Show("提示", "设置获取此别构构件参照标高" );
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

                XYZ min = new XYZ(-0.5 * w - set_LR - offset, BaseLevelElevation - offset - set_TB, 0 - set_offset);
                XYZ max = new XYZ(0.5 * w + set_LR + offset, TopLevleEla + offset + set_TB, offset);
                XYZ midpoint = stratPnt + 0.5 * v;
                XYZ dir = v.Normalize().Negate(); // 设置剖面方向为反向即同构件绘制的方向一直
                XYZ up = XYZ.BasisZ;
                XYZ viewdir = dir.CrossProduct(up);

                Transform t = Transform.Identity;
                t.Origin = new XYZ(midpoint.X, midpoint.Y, 0);
                t.BasisX = dir;
                t.BasisY = up;
                t.BasisZ = viewdir;

                BoundingBoxXYZ sectionBox = new BoundingBoxXYZ
                {
                    Transform = t,
                    Min = min,
                    Max = max
                };
                #endregion

                #region 创建视图
                Transaction tr = new Transaction(doc, "创建剖面");
                tr.Start();
                try
                {
                    var ViewSectionIns = ViewSection.CreateSection(doc, vft.Id, sectionBox);
                    ViewSectionIns.CropBoxActive = true;
                    ViewSectionIns.DisplayStyle = DisplayStyle.ShadingWithEdges;
                    ViewSectionIns.DetailLevel = ViewDetailLevel.Fine;
                    tr.Commit();
                }
                catch
                {
                    tr.RollBack();
                    TaskDialog.Show("提示", "此构件快速剖面创建失败");
                }
                #endregion
            }

            return Result.Succeeded;
        }
    }
}
