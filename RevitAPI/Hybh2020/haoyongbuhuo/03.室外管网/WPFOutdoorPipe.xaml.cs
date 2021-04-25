using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;


namespace hybh
{
    /// <summary>
    /// WPFOutdoorPipe.xaml 的交互逻辑
    /// </summary>
    public partial class WPFOutdoorPipe : Window
    {
        // 注册列表缓存数据
        RegistryStorage registryStorage = new RegistryStorage();
        public WPFOutdoorPipe(IList<Element> pipingSystemType, IList<Element> PipeType) // 主程序与WPF窗口之间的参数传递
        {
            
            InitializeComponent();

            List<SelectElementByName> listPipingSystem = new List<SelectElementByName>();

            // 获取所有的系统类型名称
            foreach (Element el in pipingSystemType)
            {
                var Familytype = el as ElementType;
                listPipingSystem.Add(new SelectElementByName { HybhElement = Familytype, HybhElName = Familytype.Name });
            }
            PipingSystemBox.ItemsSource = listPipingSystem;

            List<SelectElementByName> listPipingType = new List<SelectElementByName>();
            foreach (Element elType in PipeType)
            {
                var Familytype = elType as ElementType;
                listPipingType.Add(new SelectElementByName { HybhElement = Familytype, HybhElName = Familytype.Name });
            }
            PipeTypeBox.ItemsSource = listPipingType;

            // 读取上一次设置的值
            try
            {
                var v1 = registryStorage.OpenAfterStart("WPFOutdoorPipe_01");
                var v2 = registryStorage.OpenAfterStart("WPFOutdoorPipe_02");
                var v3 = registryStorage.OpenAfterStart("WPFOutdoorPipe_03");
                // 设置管道直径
                this.pipeD.Text = v3;

                try
                {
                    // 设置系统类型索引
                    int.TryParse(v1, out int index1);
                    PipingSystemBox.SelectedIndex = index1;
                    // 设置管道类型索引
                    int.TryParse(v2, out int index2);
                    PipeTypeBox.SelectedIndex = index2;
                }
                catch
                {
                    TaskDialog.Show("提示", "索引值超出当前列表长度");
                    // 索引设置失败 默认值
                    PipingSystemBox.SelectedIndex = 0;
                    PipeTypeBox.SelectedIndex = 0;
                }
            }
            catch { }
        }

        // 关闭窗口
        private void Window_Closed(object sender, EventArgs e)
        {
            // 不运行
            this.IsHitTestVisible = false;
            this.Close();
        }
        
        // 确认
        private void select_Click(object sender, RoutedEventArgs e)
        {
            registryStorage.SaveBeforeExit("WPFOutdoorPipe_01", PipingSystemBox.SelectedIndex.ToString());
            registryStorage.SaveBeforeExit("WPFOutdoorPipe_02", PipeTypeBox.SelectedIndex.ToString());
            registryStorage.SaveBeforeExit("WPFOutdoorPipe_03", pipeD.Text);
            this.Close();
            this.IsHitTestVisible = true;
        }

    }
}
