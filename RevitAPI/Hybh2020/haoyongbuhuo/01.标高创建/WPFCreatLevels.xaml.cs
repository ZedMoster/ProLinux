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
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace hybh
{
    /// <summary>
    /// WPFCreatLevels.xaml 的交互逻辑
    /// </summary>
    public partial class WPFCreatLevels : Window
    {
        // 注册列表缓存数据
        readonly RegistryStorage registryStorage = new RegistryStorage();
        public bool Stop { get; set; } // 判断是否直接关闭窗口停止程序运行
        public WPFCreatLevels()
        {
            InitializeComponent();

            List<SelectElementByName> ListAS = new List<SelectElementByName>();
            ListAS.Add(new SelectElementByName { HybhKey = "建筑", HybhValue = "A" });
            ListAS.Add(new SelectElementByName { HybhKey = "结构", HybhValue = "S" });

            AS.ItemsSource = ListAS;

            var v0 = registryStorage.OpenAfterStart("Level_0");
            var v1 = registryStorage.OpenAfterStart("Level_1");
            var v2 = registryStorage.OpenAfterStart("Level_2");
            var v3 = registryStorage.OpenAfterStart("Level_3");
            var v4 = registryStorage.OpenAfterStart("Level_4");

            if (v0 != null || v1 != null || v2 != null || v3 != null)
            {
                try
                {
                    // 设置上一次数据
                    name0.Text = v0;
                    name1.Text = v1;
                    name2.Text = v2;
                    name3.Text = v3;

                    int.TryParse(v4, out int index);
                    AS.SelectedIndex = index;
                }
                catch
                {
                    // 设置上一次数据
                    name0.Text = "F";
                    name1.Text = "0";
                    name2.Text = "0.000";
                    name3.Text = "3600,3300,3000*3";
                    AS.SelectedIndex = 0;

                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 保存本次数据
            registryStorage.SaveBeforeExit("Level_0", name0.Text); // 标高名称:F B
            registryStorage.SaveBeforeExit("Level_1", name1.Text); // 初始编号：0
            registryStorage.SaveBeforeExit("Level_2", name2.Text); // 初始高程：0.000
            registryStorage.SaveBeforeExit("Level_3", name3.Text); // 层高表：3600，3300，3000*3
            registryStorage.SaveBeforeExit("Level_4", AS.SelectedIndex.ToString()); // 建筑结构：A S

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
