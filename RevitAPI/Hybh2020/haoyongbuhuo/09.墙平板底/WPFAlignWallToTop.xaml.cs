using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace hybh
{
    /// <summary>
    /// WPFAlignWallToTop.xaml 的交互逻辑
    /// </summary>
    public partial class WPFAlignWallToTop : Window
    {
        RegistryStorage registryStorage = new RegistryStorage();
        public WPFAlignWallToTop()
        {
            InitializeComponent();

            // 读取记录的数据
            var h = registryStorage.OpenAfterStart("WPFAlignWallToTop_h");
            if (h == null)
            {
                H_name.Text = "h";
            }
            else
            {
                H_name.Text = h;
            }
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            registryStorage.SaveBeforeExit("WPFAlignWallToTop_h", this.H_name.Text);
            this.Close();
            // 运行
            this.IsHitTestVisible = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // 不运行
            this.IsHitTestVisible = false;
            this.Close();
        }
    }
}
