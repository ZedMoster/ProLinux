using System.Collections.Generic;
using System.Windows;

namespace hybh
{
    /// <summary>
    /// WPFCreateFloorCAD.xaml 的交互逻辑
    /// </summary>
    public partial class WPFCreateFloorCAD : Window
    {
        readonly RegistryStorage registryStorage = new RegistryStorage();
        public WPFCreateFloorCAD(List<SelectElementByName> listData)
        {
            InitializeComponent();

            // 选择列表
            tp.ItemsSource = listData;
            try
            {
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

            }
            catch { }
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            registryStorage.SaveBeforeExit("WPFCreateFloorCAD_elva", this.elva.Text);
            registryStorage.SaveBeforeExit("WPFCreateFloorCAD_struct", this.IsStruct.IsChecked.Value.ToString());
            registryStorage.SaveBeforeExit("WPFCreateFloorCAD_index", tp.SelectedIndex.ToString());

            registryStorage.SaveBeforeExit("WPFCreateFloorCAD_typeName", typeName.Text);
            registryStorage.SaveBeforeExit("WPFCreateFloorCAD_typeWidth", typeWidth.Text);
            registryStorage.SaveBeforeExit("WPFCreateFloorCAD_Newtype", New.IsChecked.Value.ToString());
            this.Close();
            IsHitTestVisible = true;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            IsHitTestVisible = false;
            this.Close();
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            IsHitTestVisible = false;
            this.Close();
        }
    }
}
