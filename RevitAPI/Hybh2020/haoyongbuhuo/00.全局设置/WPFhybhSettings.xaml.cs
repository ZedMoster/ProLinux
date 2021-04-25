using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace hybh
{
    /// <summary>
    /// WPFhybhSettings.xaml 的交互逻辑
    /// </summary>
    public partial class WPFhybhSettings : Window
    {
        // 颜色选择可视化窗口
        readonly ColorDialog ColorDialog = new ColorDialog();
        // 注册列表操作
        readonly RegistryStorage Registry = new RegistryStorage();
        public WPFhybhSettings()
        {
            InitializeComponent();

            #region 设置类别名称列表
            List<SelectElementByName> listCategoryName = new List<SelectElementByName>
            {
                new SelectElementByName { HybhKey = "梁", HybhValue = "结构框架" },
                new SelectElementByName { HybhKey = "柱", HybhValue = "结构柱" }
            };

            CategoryList.ItemsSource = listCategoryName;

            // 读取上一次设置的值
            try
            {
                // 设置系统类型索引
                int.TryParse(Registry.OpenAfterStart("CategoryNameIndex"), out int index);
                CategoryList.SelectedIndex = index;
            }
            catch { CategoryList.SelectedIndex = 0; }
            #endregion

            #region 背景色
            // 背景色颜色设置  B0_r, B0_g, B0_b    B1_r, B1_g, B1_b
            var a0 = int.TryParse(Registry.OpenAfterStart("B0_r"), out int B0_r);
            var b0 = int.TryParse(Registry.OpenAfterStart("B0_g"), out int B0_g);
            var c0 = int.TryParse(Registry.OpenAfterStart("B0_b"), out int B0_b);
            if (a0 && b0 && c0)
            {
                Brush brush = new SolidColorBrush(Color.FromRgb((byte)B0_r, (byte)B0_g, (byte)B0_b));
                B0_color.Background = brush;
            }
            else
            {
                Brush brush = new SolidColorBrush(Color.FromRgb(8, 8, 8));
                B0_color.Background = brush;
            }

            var a1 = int.TryParse(Registry.OpenAfterStart("B1_r"), out int B1_r);
            var b1 = int.TryParse(Registry.OpenAfterStart("B1_g"), out int B1_g);
            var c1 = int.TryParse(Registry.OpenAfterStart("B1_b"), out int B1_b);
            if (a1 && b1 && c1)
            {
                Brush brush = new SolidColorBrush(Color.FromRgb((byte)B1_r, (byte)B1_g, (byte)B1_b));
                B1_color.Background = brush;
            }
            else
            {
                Brush brush = new SolidColorBrush(Color.FromRgb(245, 245, 245));
                B1_color.Background = brush;
            }
            #endregion

            #region 审查模型
            var a2 = int.TryParse(Registry.OpenAfterStart("F0_r"), out int F0_r);
            var b2 = int.TryParse(Registry.OpenAfterStart("F0_g"), out int F0_g);
            var c2 = int.TryParse(Registry.OpenAfterStart("F0_b"), out int F0_b);
            if (a2 && b2 && c2)
            {
                Brush brush = new SolidColorBrush(Color.FromRgb((byte)F0_r, (byte)F0_g, (byte)F0_b));
                filter0_color.Background = brush;
            }
            else
            {
                Brush brush = new SolidColorBrush(Color.FromRgb(0, 128, 0));
                filter0_color.Background = brush;
            }

            var a3 = int.TryParse(Registry.OpenAfterStart("F1_r"), out int F1_r);
            var b3 = int.TryParse(Registry.OpenAfterStart("F1_g"), out int F1_g);
            var c3 = int.TryParse(Registry.OpenAfterStart("F1_b"), out int F1_b);
            if (a3 && b3 && c3)
            {
                Brush brush = new SolidColorBrush(Color.FromRgb((byte)F1_r, (byte)F1_g, (byte)F1_b));
                filter1_color.Background = brush;
            }
            else
            {
                Brush brush = new SolidColorBrush(Color.FromRgb(255, 70, 0));
                filter1_color.Background = brush;
            }
            // 删除视图过滤器
            delAllFilter.IsChecked = Registry.OpenAfterStart("delAllFilter") == "True";
            #endregion

            #region 梁高度参数名称
            BeamB.Text = Registry.OpenAfterStart("BeamB") ?? "b";
            BeamH.Text = Registry.OpenAfterStart("BeamH") ?? "h";
            #endregion

            #region 定位点构件偏移值 OffsetValue
            OffsetValue.Text = Registry.OpenAfterStart("OffsetValue")?? "0";
            #endregion

            #region 柱宽度长度参数名称
            ColumnB.Text = Registry.OpenAfterStart("ColumnB") ?? "b";
            ColumnH.Text = Registry.OpenAfterStart("ColumnH") ?? "h";
            #endregion

            #region 剖面宽度及深度参数 SectionB SectionH
            SectionName.Text = Registry.OpenAfterStart("Section_Name") ?? "快速剖面";
            SectionOffset.Text = Registry.OpenAfterStart("Section_Offset") ?? "400";
            SectionHeight.Text = Registry.OpenAfterStart("Section_Height") ?? "600";
            #endregion

            #region 设置对齐到楼板顶面还是底面
            List<SelectElementByName> listFace = new List<SelectElementByName>
            {
                new SelectElementByName { HybhKey = "板顶", HybhValue = "0" },
                new SelectElementByName { HybhKey = "板底", HybhValue = "1" }
            };

            FloorFace.ItemsSource = listFace;
            // 读取上一次设置的值
            try
            {
                // 设置系统类型索引
                int.TryParse(Registry.OpenAfterStart("FloorFaceIndex"), out int index);
                FloorFace.SelectedIndex = index;
            }
            catch { FloorFace.SelectedIndex = 0; }
            #endregion
        }

        private void NO_Click(object sender, RoutedEventArgs e)
        #region 取消
        {
            this.Close();
        }
        #endregion

        private void B0_Click(object sender, RoutedEventArgs e)
        #region 背景色颜色设置  B0_r, B0_g, B0_b    B1_r, B1_g, B1_b
        {
            var rtn = ColorDialog.SelectColorDialog();
            if (rtn == null)
            {
                // 取消颜色设置
                return;
            }
            Brush brush = new SolidColorBrush(Color.FromRgb(rtn.Red, rtn.Green, rtn.Blue));
            B0_color.Background = brush;

            Registry.SaveBeforeExit("B0_r", rtn.Red.ToString());
            Registry.SaveBeforeExit("B0_g", rtn.Green.ToString());
            Registry.SaveBeforeExit("B0_b", rtn.Blue.ToString());
        }

        private void B1_Click(object sender, RoutedEventArgs e)
        {
            var rtn = ColorDialog.SelectColorDialog();
            if (rtn == null)
            {
                // 取消颜色设置
                return;
            }
            Brush brush = new SolidColorBrush(Color.FromRgb(rtn.Red, rtn.Green, rtn.Blue));
            B1_color.Background = brush;

            Registry.SaveBeforeExit("B1_r", rtn.Red.ToString());
            Registry.SaveBeforeExit("B1_g", rtn.Green.ToString());
            Registry.SaveBeforeExit("B1_b", rtn.Blue.ToString());
        }
        #endregion

        private void Filter0_Click(object sender, RoutedEventArgs e)
        #region 审查模型颜色设置
        {
            var rtn = ColorDialog.SelectColorDialog();
            Brush brush = new SolidColorBrush(Color.FromRgb(rtn.Red, rtn.Green, rtn.Blue));
            filter0_color.Background = brush;

            Registry.SaveBeforeExit("F0_r", rtn.Red.ToString());
            Registry.SaveBeforeExit("F0_g", rtn.Green.ToString());
            Registry.SaveBeforeExit("F0_b", rtn.Blue.ToString());
        }

        private void Filter1_Click(object sender, RoutedEventArgs e)
        {
            var rtn = ColorDialog.SelectColorDialog();
            Brush brush = new SolidColorBrush(Color.FromRgb(rtn.Red, rtn.Green, rtn.Blue));
            filter1_color.Background = brush;

            Registry.SaveBeforeExit("F1_r", rtn.Red.ToString());
            Registry.SaveBeforeExit("F1_g", rtn.Green.ToString());
            Registry.SaveBeforeExit("F1_b", rtn.Blue.ToString());
        }
        #endregion

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Registry.SaveBeforeExit("CategoryNameIndex", CategoryList.SelectedIndex.ToString());
            Registry.SaveBeforeExit("delAllFilter", delAllFilter.IsChecked.ToString());
            Registry.SaveBeforeExit("BeamH", BeamH.Text);
            Registry.SaveBeforeExit("BeamB", BeamB.Text);
            Registry.SaveBeforeExit("ColumnB", ColumnB.Text);
            Registry.SaveBeforeExit("ColumnH", ColumnH.Text);
            Registry.SaveBeforeExit("FloorFaceIndex", FloorFace.SelectedIndex.ToString());
            Registry.SaveBeforeExit("OffsetValue", OffsetValue.Text);
            Registry.SaveBeforeExit("Section_Name", SectionName.Text);
            Registry.SaveBeforeExit("Section_Height", SectionHeight.Text);
            Registry.SaveBeforeExit("Section_Offset", SectionOffset.Text);
            this.Close();
        }
    }
}
