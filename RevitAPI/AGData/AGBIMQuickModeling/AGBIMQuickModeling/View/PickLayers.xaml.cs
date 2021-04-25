using System;
using System.Windows;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace CADReader.WPF
{
    /// <summary>
    /// PickLayers.xaml 的交互逻辑
    /// </summary>
    public partial class PickLayers : Window
    {
        public Selection Sel { get; set; }
        public PickReferences pickReferences { get; set; }
        public PickLayers(Selection sel)
        {
            InitializeComponent();

            this.Sel = sel;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (pickReferences.NotRun())
            {
                MessageBox.Show("确认所有信息均已选择", "警告");
                return;
            }

            this.Close();
            this.IsHitTestVisible = false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Pick_1_Click(object sender, RoutedEventArgs e)
        {
            pickReferences.PickedBox = Sel.PickBox(PickBoxStyle.Crossing, "准确框选建筑明细表中的建筑型号及高度数据列");
            Layer_1.Text = pickReferences.PickedBox == null ? "...." : "OK";
        }

        private void Pick_2_Click(object sender, RoutedEventArgs e)
        {
            Pickdwg pickdwg = new Pickdwg();
            pickdwg.Refer(Sel, out Reference ReferenceText);
            pickReferences.ReferenceText = ReferenceText;
            Layer_1.Text = pickReferences.ReferenceText == null ? "...." : "OK";
        }

        private void Pick_3_Click(object sender, RoutedEventArgs e)
        {
            Pickdwg pickdwg = new Pickdwg();
            pickdwg.Refer(Sel, out Reference ReferencePolyline);
            pickReferences.ReferencePolyline = ReferencePolyline;
            Layer_1.Text = pickReferences.ReferencePolyline == null ? "...." : "OK";
        }

        private void Pick_4_Click(object sender, RoutedEventArgs e)
        {
            Pickdwg pickdwg = new Pickdwg();
            pickdwg.Refer(Sel, out Reference ReferenceName);
            pickReferences.ReferenceName = ReferenceName;
            Layer_1.Text = pickReferences.ReferenceName == null ? "...." : "OK";
        }
    }
}
