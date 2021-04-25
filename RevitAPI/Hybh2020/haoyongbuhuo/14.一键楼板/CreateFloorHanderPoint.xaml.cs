using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace hybh
{
    /// <summary>
    /// WPFAutoCreateFloorPoint.xaml 的交互逻辑             不能传递参数到 事件
    /// </summary>
    public partial class WPFCreateFloorHanderPoint : Window
    {
        readonly RegistryStorage registryStorage = new RegistryStorage();

        readonly AutoFloor MyCommand = null;
        readonly ExternalEvent Hander = null;
        public WPFCreateFloorHanderPoint(List<SelectElementByName> listData)
        {
            InitializeComponent();

            #region 设置参数
            tp.ItemsSource = listData;
            var H = registryStorage.OpenAfterStart("WPFCreateFloorCAD_elva");
            var I = registryStorage.OpenAfterStart("WPFCreateFloorCAD_index");
            var S = registryStorage.OpenAfterStart("WPFCreateFloorCAD_struct");

            var S1 = registryStorage.OpenAfterStart("WPFCreateFloorCAD_typeName");
            var W1 = registryStorage.OpenAfterStart("WPFCreateFloorCAD_typeWidth");
            var T1 = registryStorage.OpenAfterStart("WPFCreateFloorCAD_Newtype");

            // 设置楼板类型
            try
            {
                int.TryParse(I, out int index);
                tp.SelectedIndex = index;
            }
            catch
            {
                tp.SelectedIndex = 0;
                // 超出列表长度的值 
                registryStorage.SaveBeforeExit("WPFCreateFloorCAD_index", "0");
            }

            // 设置偏移值
            this.elva.Text = H;

            // 设置是否为结构板
            if (S == "True")
            {
                this.IsStruct.IsChecked = true;
            }
            else
            {
                this.IsStruct.IsChecked = false;
            }

            typeName.Text = S1;
            typeWidth.Text = W1;
            // 是否新建
            if (T1 == "True")
            {
                this.New.IsChecked = true;
            }
            else
            {
                this.New.IsChecked = false;
            }
            #endregion

            MyCommand = new AutoFloor();
            Hander = ExternalEvent.Create(MyCommand);
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            if (New.IsChecked.HasValue)
            {
                var bwidth = double.TryParse(typeWidth.Text, out double _typeWidth);
                var belva = double.TryParse(elva.Text, out double _elva);
                if (!bwidth || !belva)
                {
                    MessageBox.Show("输入正确的数值！");
                    return;

                }
                MyCommand.Elva = _elva;
                MyCommand.TypeWidth = _typeWidth;
            }

            MyCommand.IsStruct = IsStruct.IsChecked.Value;
            MyCommand.NewType = New.IsChecked.Value;
            MyCommand.TypeName = typeName.Text;
            MyCommand.ThisFloorType = (FloorType)tp.SelectedItem;
            MyCommand.Index = tp.SelectedIndex;

            Hander.Raise();
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            registryStorage.SaveBeforeExit("WPFCreateFloorCAD_elva", this.elva.Text);
            registryStorage.SaveBeforeExit("WPFCreateFloorCAD_struct", this.IsStruct.IsChecked.Value.ToString());
            registryStorage.SaveBeforeExit("WPFCreateFloorCAD_index", tp.SelectedIndex.ToString());

            registryStorage.SaveBeforeExit("WPFCreateFloorCAD_typeName", typeName.Text);
            registryStorage.SaveBeforeExit("WPFCreateFloorCAD_typeWidth", typeWidth.Text);
            registryStorage.SaveBeforeExit("WPFCreateFloorCAD_Newtype", New.IsChecked.Value.ToString());
            this.Close();
        }
    }

    public class AutoFloor : IExternalEventHandler
    {
        #region 设置wpf参数
        public double Elva { get; set; }
        public double TypeWidth { get; set; }
        public bool IsStruct { get; set; }
        public bool NewType { get; set; }
        public string TypeName { get; set; }
        public FloorType ThisFloorType { get; set; }
        public int Index { get; set; }
        #endregion

        public XYZ Point { get; set; }

        public string GetName()
        {
            return "一键楼板";
        }

        public void Execute(UIApplication app)
        {
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = app.ActiveUIDocument.Document;
            Selection sel = uidoc.Selection;

            #region 判断视图
            var view = doc.ActiveView;
            if (view.ViewType == ViewType.ThreeD)
            {
                TaskDialog.Show("提示", "不能在3D视图创建标记！");
                return;
            }
            #endregion

            #region 获取所有的模型实体
            // 过滤器-按类别
            List<ElementFilter> filters = new List<ElementFilter>()
                {
                    new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming),
                    new ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns),
                    new ElementCategoryFilter(BuiltInCategory.OST_Walls),
                };
            // 获取当前视图中可见且指定类别构件的列表
            var allElements = new FilteredElementCollector(doc, uidoc.ActiveView.Id).WherePasses(
                new LogicalOrFilter(filters)).WhereElementIsNotElementType().ToElements().ToList();

            // 获取当前项目中的梁
            if (allElements.Count == 0)
            {
                TaskDialog.Show("提示", "当前视图未找到梁柱是否可见");
                return;
            }

            // 合并所有的结构实体
            SolidByUnion solidByUnion = new SolidByUnion();
            #endregion

            var solidEls = solidByUnion.ByUnion(allElements);
            //TaskDialog.Show("边个数1：", solidEls.Edges.Size.ToString());

            #region 创建平面与实体相交获取 surface
            var Z = view.GenLevel.ProjectElevation;
            // 创建标高平面 方向为垂直向下
            Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ.Negate(), new XYZ(0, 0, Z));
            // 获取标高与平面相切的实体
            Solid cast = BooleanOperationsUtils.CutWithHalfSpace(solidEls, plane);

            PlanarFace CutFace = null;
            // 获取剪切后实体的上表面 ：XYZ.BasisZ
            foreach (Face face in cast.Faces)
            {
                PlanarFace planFace = face as PlanarFace;
                if (planFace == null) continue;
                if (planFace.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ) && planFace.Origin.Z == Z)
                {
                    CutFace = planFace;
                }
            }
            if (CutFace == null)
            {
                TaskDialog.Show("提示", "未知错误不能创建楼板" );
                return;
            }
            // 获取所有的边
            var curveLoops = CutFace.GetEdgesAsCurveLoops().ToList();

            List<CurveLoop> orderedcurveloops = curveLoops.OrderBy(l => l.GetExactLength()).ToList();
            orderedcurveloops.RemoveAt(orderedcurveloops.Count - 1);
            #endregion

            #region 保存本项目本楼层数据
            // 文件名
            var title = doc.Title.Split('.')[0];
            #endregion

            if (orderedcurveloops.Count == 0)
            {
                TaskDialog.Show("提示", "梁柱所围成的区域不是空心");
                return;
            }
            #region 点击创建楼板
            try
            {
                Point = sel.PickPoint("点击创建楼板区域内的一点");
            }
            catch { }
            if (Point == null)
            {
                // 取消创建
                return;
            }
            #endregion

            TransactionGroup T = new TransactionGroup(doc, "一键楼板");
            T.Start();
            // 是否提交事务组
            bool Push = false;
            foreach (CurveLoop curveloop in orderedcurveloops)
            {
                #region 判断点是否在楼板闭合区域内
                bool isInslide = IsInside(Point, curveloop);
                if (isInslide)
                {
                    continue;
                }
                #endregion

                #region 创建楼板需要使用CurveArray参数
                var result = new CurveArray();
                foreach (Curve curve in curveloop)
                {
                    result.Append(curve);
                }
                #endregion

                #region 创建楼板
                Transaction t = new Transaction(doc);
                t.Start("新建楼板");
                try
                {
                    // 关闭警告
                    FailureHandlingOptions fho = t.GetFailureHandlingOptions();
                    fho.SetFailuresPreprocessor(new FailuresPreprocessor());
                    t.SetFailureHandlingOptions(fho);

                    var floor = doc.Create.NewFloor(result, ThisFloorType, doc.ActiveView.GenLevel, IsStruct);
                    floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).Set(UUtools.MillimetersToUnits(Elva));

                    t.Commit();
                    Push = true;
                }
                catch
                {
                    t.RollBack();
                }
                #endregion
            }

            #region 判断是否创建成功提交事务组
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

        /// <summary>
        /// 判断点是否在一个闭合轮廓外部
        /// </summary>
        /// <returns> true 在外部  false 在内部</returns>
        public bool IsInside(XYZ TargetPoint, CurveLoop curveloop)
        {
            int insertCount = 0;

            #region  判断点是否在一个闭合轮廓内:通过判断点的射线与边界相交点的个数来判断
            var TarPoint = new XYZ(TargetPoint.X, TargetPoint.Y, 0);
            Autodesk.Revit.DB.Line rayLine = Autodesk.Revit.DB.Line.CreateBound(TarPoint, TarPoint.Add(XYZ.BasisX * 10000));
            var i = curveloop.GetCurveLoopIterator();
            i.Reset();
            while (i.MoveNext())
            {
                var curve = i.Current;
                var line = Autodesk.Revit.DB.Line.CreateBound(new XYZ(curve.GetEndPoint(0).X, curve.GetEndPoint(0).Y, 0),
                    new XYZ(curve.GetEndPoint(1).X, curve.GetEndPoint(1).Y, 0));
                var interResult = line.Intersect(rayLine);
                if (interResult == SetComparisonResult.Overlap)
                {
                    insertCount += 1;
                }

            }
            #endregion

            #region 如果次数为偶数就在外面，次数为奇数就在里面
            if (insertCount % 2 == 0)
            {
                // 在外面
                return true;
            }
            else
            {
                // 在里面
                return false;
            }
            #endregion
        }

        /// <summary>
        /// 创建依据楼板名称厚度创建新的类型
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="floorName"> 楼板类型名称</param>
        /// <param name="width"> 楼板厚度</param>
        /// <returns></returns>
        public FloorType CreateNewFloorType(Document doc, string floorName, double width)
        {
            Transaction trans = new Transaction(doc, "新建类型");
            trans.Start();

            // 创建滤器
            var floorTypes = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Floors).WhereElementIsElementType().ToList();
            // 判断是否已经存在该类型
            var floorIn = from tp in floorTypes
                          where tp.Name == floorName
                          select tp;

            if (floorIn.Count() == 0)
            {
                var newFloorType = floorTypes[0] as FloorType;
                // 创建新类型，使用Duplicate方法
                var newtype = newFloorType.Duplicate(floorName) as FloorType;
                // 获取材质id
                var materialId = newtype.GetCompoundStructure().GetMaterialId(0);
                // 创建复合结构
                var createSingleLayerCompoundStructure = CompoundStructure.CreateSingleLayerCompoundStructure(MaterialFunctionAssignment.Structure, width, materialId);
                createSingleLayerCompoundStructure.EndCap = EndCapCondition.NoEndCap;
                // 设置复合结构
                newtype.SetCompoundStructure(createSingleLayerCompoundStructure);
                trans.Commit();

                return newtype;
            }
            else
            {
                try
                {
                    // 已存在类型 更新厚度参数
                    var newFloorType = floorIn.ToList()[0] as FloorType;
                    var materialId = newFloorType.GetCompoundStructure().GetMaterialId(0);
                    var createSingleLayerCompoundStructure = CompoundStructure.CreateSingleLayerCompoundStructure(MaterialFunctionAssignment.Structure, width, materialId);
                    createSingleLayerCompoundStructure.EndCap = EndCapCondition.NoEndCap;
                    newFloorType.SetCompoundStructure(createSingleLayerCompoundStructure);
                    trans.Commit();
                    return newFloorType;
                }
                catch (Exception)
                {
                    trans.RollBack();
                    TaskDialog.Show("提示", "创建楼板类型出错！");
                    return null;
                }
            }
        }
    }
}
