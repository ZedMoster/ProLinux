using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using CADReader.Handler;

namespace CADReader.View
{
    /// <summary>
    /// WPFHowToWork.xaml 的交互逻辑
    /// </summary>
    public partial class WPFHowNewBoxExterior : Window
    {
        private readonly EventHandlerWallBox Command = null;
        private readonly ExternalEvent Hander = null;
        private Document Doc { get; set; }
        private Selection Sel { get; set; }
        private Pickdwg Pickdwg { get; set; }
        public ToDo ToDo { get; set; }
        public WPFHowNewBoxExterior(UIDocument uidoc)
        {
            InitializeComponent();

            Command = new EventHandlerWallBox();
            Hander = ExternalEvent.Create(Command);

            Doc = uidoc.Document;
            Sel = uidoc.Selection;
            Pickdwg = new Pickdwg();
            ToDo = new ToDo();
        }

        /// <summary>
        /// 框选明细表
        /// </summary>
        private void Pick_1_Click(object sender, RoutedEventArgs e)
        {
            PickedBox pickedBox = Sel.PickBox(PickBoxStyle.Directional, "请框选建筑明细表全部文字内容");
            if (pickedBox == null)
            {
                MessageBox.Show("已取消框选建筑明细表", "提示");
                return;
            }
            ToDo.PickedBox = pickedBox;
            ShowData.Background = new SolidColorBrush(Colors.LightBlue);
        }

        /// <summary>
        /// 选择明细表中文字图层
        /// </summary>
        private void Pick_2_Click(object sender, RoutedEventArgs e)
        {
            // 明细表文字
            Pickdwg.Refer(Sel, out Reference ReferenceText, "请点选建筑明细表中的文字图层");
            if (ReferenceText == null)
            {
                MessageBox.Show("未选择图层", "警告");
                return;
            }
            // 导入图纸比例：米    IMPORT_DISPLAY_UNITS
            if (!Doc.IsDisplayUnits(ReferenceText))
            {
                MessageBox.Show("链接图纸比例设置出错\n请删除已导入图纸并重新链接时设置单位：米", "警告");
                Close();
                return;
            }

            Layer_2.Text = Doc.GetLayerName(ReferenceText);
            Layer_2.Background = new SolidColorBrush(Colors.LightGreen);
            ToDo.ReferenceText = ReferenceText;
        }

        /// <summary>
        /// 选择明细表中表格线
        /// </summary>
        private void Pick_3_Click(object sender, RoutedEventArgs e)
        {
            // 选择明细表边界
            Pickdwg.Refer(Sel, out Reference ReferenceLine, "请点选建筑明细表中的边线图层");
            if (ReferenceLine == null)
            {
                MessageBox.Show("未选择图层", "警告");
                Close();
                return;
            }

            // 导入图纸比例：米    IMPORT_DISPLAY_UNITS
            if (!Doc.IsDisplayUnits(ReferenceLine))
            {
                MessageBox.Show("链接图纸比例设置出错\n请删除已导入图纸并重新链接时设置单位：米", "警告");
                Close();
                return;
            }

            Layer_3.Text = Doc.GetLayerName(ReferenceLine);
            Layer_3.Background = new SolidColorBrush(Colors.LightGreen);
            ToDo.ReferenceLine = ReferenceLine;
        }

        /// <summary>
        /// 请点选建筑外轮廓定位线
        /// </summary>
        private void Pick_4_Click(object sender, RoutedEventArgs e)
        {
            Pickdwg.Refer(Sel, out Reference ReferenceCurves, "请点选建筑外轮廓定位线");
            if (ReferenceCurves == null)
            {
                MessageBox.Show("未选择图层", "提示");
                return;
            }
            // 导入图纸比例：米    IMPORT_DISPLAY_UNITS
            if (!Doc.IsDisplayUnits(ReferenceCurves))
            {
                MessageBox.Show("链接图纸比例设置出错\n请删除已导入图纸并重新链接时设置单位：米", "警告");
                Close();
                return;
            }

            Layer_4.Text = Doc.GetLayerName(ReferenceCurves);
            Layer_4.Background = new SolidColorBrush(Colors.LightGreen);
            ToDo.ReferenceCurves = ReferenceCurves;
        }

        /// <summary>
        /// 请点选建筑外楼栋编号图层
        /// </summary>
        private void Pick_5_Click(object sender, RoutedEventArgs e)
        {
            Pickdwg.Refer(Sel, out Reference referenceCategory, "请点选建筑外楼栋编号图层");
            if (referenceCategory == null)
            {
                MessageBox.Show("未选择图层", "提示");
                return;
            }
            // 导入图纸比例：米    IMPORT_DISPLAY_UNITS
            if (!Doc.IsDisplayUnits(referenceCategory))
            {
                MessageBox.Show("链接图纸比例设置出错\n请删除已导入图纸并重新链接时设置单位：米", "警告");
                Close();
                return;
            }

            Layer_5.Text = Doc.GetLayerName(referenceCategory);
            Layer_5.Background = new SolidColorBrush(Colors.LightGreen);
            ToDo.ReferenceCategory = referenceCategory;
        }

        /// <summary>
        /// 显示数据
        /// </summary>
        private void Show_Click(object sender, RoutedEventArgs e)
        {
            if (ToDo.PickedBox == null || ToDo.ReferenceText == null || ToDo.ReferenceLine == null)
            {
                MessageBox.Show("需要完成左侧1~3步骤才可获取表格数据", "提示");
                return;
            }
            CADElement CADModel = new CADElement();
            CADText CADtext = new CADText();
            // 框选文字
            List<CADTextModel> Layertext = CADtext.GetBoxText(Doc, ToDo.ReferenceText);
            List<CADTextModel> boxtexts = Layertext.Where(i => ToDo.PickedBox.InBox(i.Location)).ToList();
            // 表格线
            List<Line> lines = CADModel.GetLayerLines(Doc, ToDo.ReferenceLine);
            WPFViewSchedule viewSchedule = new WPFViewSchedule(boxtexts, lines);
            viewSchedule.ShowDialog();
            ToDo.ScheduleDatas = viewSchedule.ScheduleViewData.ItemsSource as List<ScheduleDatas>;
            // 高度列表
            var result = int.TryParse(viewSchedule.indexRow.Text, out int index);
            if (!result)
            {
                MessageBox.Show("显示表格数据后左下角确定建筑高度列序号", "提示");
                return;
            }
            ToDo.Index = index;
            ShowData.Background = new SolidColorBrush(Colors.LightGreen);
        }

        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (ToDo.PickedBox == null || ToDo.ReferenceText == null || ToDo.ReferenceLine == null ||
                ToDo.ReferenceCurves == null || ToDo.ReferenceCategory == null || ToDo.ScheduleDatas == null)
            {
                MessageBox.Show("存在运行的步骤", "警告");
                return;
            }
            Command.ToDoData = ToDo;
            Hander.Raise();
            // 关闭窗口
            Close();
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NO_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
