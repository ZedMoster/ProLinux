using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using Line = Autodesk.Revit.DB.Line;

namespace CADReader.WPF
{
    /// <summary>
    /// wpfParking.xaml 的交互逻辑
    /// </summary>
    public partial class WPFParking : Window
    {
        readonly NewParking MyCommand = null;
        readonly ExternalEvent Hander = null;
        public Document Doc { get; set; }
        public WPFParking(Document doc)
        {
            InitializeComponent();
            Doc = doc;
            // 数据绑定族名称
            FamilyList.ItemsSource = GetSelectedValues(doc);

            MyCommand = new NewParking();
            Hander = ExternalEvent.Create(MyCommand);
        }

        /// <summary>
        /// 确认创建
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Create_Click(object sender, RoutedEventArgs e)
        {
            // 传递窗口输入参数
            MyCommand.ParkingWidth = parkingWidth.Text;
            MyCommand.ParkingLength = parkingLength.Text;
            MyCommand.ToGroup = toGroup.IsChecked.Value;
            MyCommand.GroupLength = groupLength.Text;
            MyCommand.GroupNumber = groupNumber.Text;
            MyCommand.Symbol = SymbolList.SelectedValue as FamilySymbol;

            if (MyCommand.Symbol == null)
            {
                MessageBox.Show("请选择停车位族类型", "提示");
                return;
            }

            Hander.Raise();
        }

        /// <summary>
        /// 关闭创建
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 获取指定类别的族
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        const string FamilyName = "停车位.rfa";
        private List<SelectFamily> GetSelectedValues(Document doc)
        {
            #region 固定参数
            string familyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + string.Format("\\Family\\{0}", FamilyName);
            string familyName = FamilyName.Split('.')[0];
            var builtInCategory = BuiltInCategory.OST_Parking;
            #endregion

            List<SelectFamily> selectFamilies = new List<SelectFamily>();
            // 获取所有的类型
            var elsType = new FilteredElementCollector(doc).OfCategory(builtInCategory).WhereElementIsElementType().ToList();
            if (elsType.Count == 0)
            {
                #region 载入族文件
                Transaction tload = new Transaction(doc);
                tload.Start("载入族文件");
                bool push = false;
                Family family = null;
                try { push = doc.LoadFamily(familyPath, out family); }
                catch { }

                if (push)
                {
                    // 更新类型名称
                    family.Name = familyName;
                    tload.Commit();
                    elsType = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Parking).WhereElementIsElementType().ToList();
                }
                else
                {
                    tload.RollBack();
                }
                #endregion
            }
            // 当前项目的族类型
            if (elsType.Count == 0)
            {
                return selectFamilies;
            }
            // 获取所有的停车位数据
            List<string> familyNames = new List<string>();
            foreach (var item in elsType)
            {
                var _symbol = item as FamilySymbol;
                var _family = _symbol.Family;
                var _familyName = _symbol.FamilyName;
                if (!familyNames.Contains(_familyName))
                {
                    selectFamilies.Add(new SelectFamily()
                    {
                        Family = _family,
                        FamilyName = _familyName,
                    });
                    familyNames.Add(_familyName);
                }
            }

            return selectFamilies;
        }
        /// <summary>
        /// 获取族-类型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FamilyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var _falimy = FamilyList.SelectedValue as Family;
            if (_falimy == null)
            {
                return;
            }
            // 族类型数据
            SymbolList.ItemsSource = SelectedFamily(_falimy);
        }

        /// <summary>
        /// 选中族类型
        /// </summary>
        /// <param name="_falimy"></param>
        /// <returns></returns>
        public List<SelectFamily> SelectedFamily(Family _falimy)
        {
            List<SelectFamily> selectedFamily = new List<SelectFamily>();
            foreach (ElementId id in _falimy.GetFamilySymbolIds())
            {
                var el = Doc.GetElement(id) as FamilySymbol;
                selectedFamily.Add(new SelectFamily() { Symbol = el, SymbolName = el.Name });
            }
            return selectedFamily;
        }

        /// <summary>
        /// 仅输入数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }

        /// <summary>
        /// 成组参数设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToGroup_Click(object sender, RoutedEventArgs e)
        {
            groupLength.IsEnabled = toGroup.IsChecked.Value;
            groupNumber.IsEnabled = toGroup.IsChecked.Value;
        }

        private void Close_Window(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    /// <summary>
    /// 创建停车位
    /// </summary>
    class NewParking : IExternalEventHandler
    {
        /// <summary>
        /// 停车位宽度
        /// </summary>
        public string ParkingWidth { get; set; }
        /// <summary>
        /// 停车位长度
        /// </summary>
        public string ParkingLength { get; set; }
        /// <summary>
        /// 是否成组布置
        /// </summary>
        public bool ToGroup { get; set; }
        /// <summary>
        /// 成组间距
        /// </summary>
        public string GroupLength { get; set; }
        /// <summary>
        /// 每组个数
        /// </summary>
        public string GroupNumber { get; set; }
        /// <summary>
        /// 族类型
        /// </summary>
        public FamilySymbol Symbol { get; set; }

        /// <summary>
        /// 点击两个点创建停车位
        /// </summary>
        /// <param name="app"></param>
        public void Execute(UIApplication app)
        {
            // 定义变量
            Document doc = app.ActiveUIDocument.Document;
            Selection sel = app.ActiveUIDocument.Selection;

            #region 点击起点及终点 创建临时参照面方便水平垂直绘制
            var start = sel.PickPoint("设置起点");
            Transaction tCLine = new Transaction(doc, "cLine");
            tCLine.Start();
            XYZ bubbleEnd = start;
            XYZ freeEnd = new XYZ(bubbleEnd.X + 0.1, bubbleEnd.Y, bubbleEnd.Z);
            ReferencePlane refPlane = doc.Create.NewReferencePlane(bubbleEnd, freeEnd, XYZ.BasisZ, doc.ActiveView);
            tCLine.Commit();
            var end = sel.PickPoint("设置终点");
            Transaction tranDel = new Transaction(doc, "删除参照");
            tranDel.Start();
            doc.Delete(refPlane.Id);
            tranDel.Commit();
            #endregion

            // 数据转换
            double.TryParse(ParkingWidth, out double width);
            double.TryParse(ParkingLength, out double length);
            double.TryParse(GroupLength, out double groupLength);
            int.TryParse(GroupNumber, out int groupNumber);
            // 计算停车位定位点
            var res = GetLocation(start, end, ToGroup,
                width.MillimeterToFeet(),
                length.MillimeterToFeet(),
                groupLength.MillimeterToFeet(),
                groupNumber, out double angle);

            if (res.FirstOrDefault() == null) return;


            #region 布置车位
            TransactionGroup transactionGroup = new TransactionGroup(doc, "布置车位");
            transactionGroup.Start();
            bool push = false;
            foreach (XYZ loc in res)
            {
                #region 创建族文件更新参数
                Transaction tranC = new Transaction(doc, "创建模型");
                tranC.Start();
                // 创建实例
                if (!Symbol.IsActive)
                {
                    Symbol.Activate();
                }
                var instance = doc.Create.NewFamilyInstance(loc, Symbol,
                               doc.ActiveView.GenLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                if (instance != null)
                {
                    UpdateParatemer(instance, loc, width.MillimeterToFeet(), length.MillimeterToFeet(), angle);
                    tranC.Commit();
                    push = true;
                }
                #endregion
            }
            if (push)
            {
                transactionGroup.Assimilate();
            }
            else
            {
                transactionGroup.RollBack();
            }
            #endregion
        }

        /// <summary>
        /// 名称
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return "布置车位";
        }

        /// <summary>
        /// 获取定位点
        /// </summary>
        /// <returns></returns>
        private List<XYZ> GetLocation(XYZ _start, XYZ _end, bool toGroup, double width, double lengh, double groupLength, int groupNumber, out double angle)
        {
            List<XYZ> xyzs = new List<XYZ>();
            // Flatten
            XYZ start = _start.Flatten();
            XYZ end = _end.Flatten();
            // 间距
            double dis = start.DistanceTo(end);
            // 定位向量
            XYZ p = end - start;
            // 方向
            var normalize = p.Normalize();
            // 垂直方向的方向
            var dir = normalize.Rotation();
            // 停车位长度向量
            var parkingH = dir * lengh * 0.5;
            // 停车位宽度向量
            var parkingB = normalize * width;
            // 间距向量 
            var parkingN = normalize * groupLength;
            angle = GetRoation(start, end);
            if (toGroup)
            {
                double _length = 0.0;
                int i = 0;
                XYZ groupStart = start + (parkingH - parkingB * 0.5) - parkingN;
                // 成组布置停车位
                while (_length < dis)
                {
                    XYZ _xyz;
                    if (i % groupNumber == 0)
                    {
                        // 获取定位点
                        _xyz = groupStart + parkingB + parkingN;
                    }
                    else
                    {
                        _xyz = groupStart + parkingB;
                    }
                    if (_xyz != null)
                    {
                        xyzs.Add(_xyz);
                        // 最新布置点
                        groupStart = _xyz;
                    }
                    // 布置个数
                    i++;
                    // 最新定位距离
                    _length = start.DistanceTo(_xyz);
                }
            }
            else
            {
                // 不成组布置停车位
                var num = Math.Floor(dis / width);
                if (num < 1)
                {
                    return xyzs;
                }
                // 获取定位点
                for (int i = 0; i < num; i++)
                {
                    // 获取定位点
                    XYZ _xyz = start + (i * parkingB) + (parkingH + parkingB * 0.5);
                    xyzs.Add(_xyz);
                }
            }

            return xyzs;
        }

        /// <summary>
        /// 获取curve（两个定位点）选择角度
        /// </summary>
        /// <param name="YXZs"> 矩形定位点</param>
        /// <returns></returns>
        private double GetRoation(XYZ start, XYZ end)
        {
            // 定义参数
            double Angle = 0.00;
            XYZ p = end - start;
            var normalize = p.Normalize();
            var _angle = normalize.AngleTo(XYZ.BasisX);
            #region 选择定位构件
            if (normalize.IsAlmostEqualTo(XYZ.BasisY) || normalize.IsAlmostEqualTo(XYZ.BasisY.Negate()))
            {
                // 下
                if (start.Y > end.Y)
                {
                    Angle = _angle;
                }
                else
                {
                    Angle = -_angle;
                }
            }
            else
            {
                // 左
                if (start.X > end.X)
                {
                    // 左下
                    if (start.Y > end.Y)
                    {
                        Angle = normalize.AngleTo(XYZ.BasisX.Negate());
                    }
                    else
                    {
                        Angle = normalize.AngleTo(XYZ.BasisX.Negate()) * -1;
                    }
                }
                // 右
                else
                {
                    // 右下
                    if (start.Y > end.Y)
                    {
                        Angle = -_angle;
                    }
                    else
                    {
                        Angle = _angle;
                    }
                }
            }
            #endregion

            return Angle;
        }

        /// <summary>
        /// 更新族实例参数
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="loc"></param>
        /// <param name="b"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        private bool UpdateParatemer(FamilyInstance instance, XYZ loc, double b, double h, double angle)
        {
            try
            {
                // 参数名称
                string paraterNameB = "停车场宽度";
                string paraterNameH = "停车场长度";

                // 设置 偏移 为0
                instance.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(0);
                // 实例参数
                var p_b = instance.LookupParameter(paraterNameB);
                var p_h = instance.LookupParameter(paraterNameH);
                if (p_b != null && p_h != null)
                {
                    //MessageBox.Show("更新参数--实例");
                    p_b.Set(b);
                    p_h.Set(h);
                }
                // 类型参数
                var p_s_b = instance.Symbol.LookupParameter(paraterNameB);
                var p_s_h = instance.Symbol.LookupParameter(paraterNameH);
                if (p_s_b != null && p_s_h != null)
                {
                    //MessageBox.Show("更新参数--类型");
                    p_s_b.Set(b);
                    p_s_h.Set(h);
                }

                //// 编号参数
                //var p_n = instance.LookupParameter(Paraternumber);
                //if (p_n != null)
                //{
                //    p_n.Set(cateNum);
                //}
                //else
                //{
                //    instance.Symbol.LookupParameter(Paraternumber).Set(cateNum);
                //}

                // 旋转轴及转动角度
                var axis = Line.CreateBound(loc, loc + XYZ.BasisZ);
                instance.Location.Rotate(axis, angle);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
