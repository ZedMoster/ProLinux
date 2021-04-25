using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
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
    /// WPFInstanceCutUtils.xaml 的交互逻辑
    /// </summary>
    public partial class WPFInstanceCutUtils : Window
    {
        // 注册列表缓存数据
        readonly RegistryStorage registryStorage = new RegistryStorage();
        public WPFInstanceCutUtils(Categories groups, List<Element> levels)
        {
            InitializeComponent();

            List<SelectElementByName> listCategory = new List<SelectElementByName>();
            // 类别类型列表
            List<BuiltInCategory> CateList = new List<BuiltInCategory> {
                BuiltInCategory.OST_Walls,
                BuiltInCategory.OST_StructuralFraming,
                BuiltInCategory.OST_StructuralColumns,
                BuiltInCategory.OST_Floors,
                BuiltInCategory.OST_DuctFitting,
                BuiltInCategory.OST_PipeAccessory,
            };

            foreach (var cate in CateList)
            {
                var categoryItem = groups.get_Item(cate);
                listCategory.Add(new SelectElementByName { HybhBuiltInCategory = cate, HybhElName = categoryItem.Name});
            }
            // 设置下拉列表的键值对
            elementA.ItemsSource = listCategory;
            elementB.ItemsSource = listCategory;

            List<SelectElementByName> LevelSelectByName = new List<SelectElementByName>();

            // 获取所有的标高
            foreach (Element elLevel in levels)
            {
                var level = elLevel as Level;
                var name = level.Name;

                LevelSelectByName.Add(new SelectElementByName { HybhElement = level, HybhElName = name });
            }
            // 设置标高的下拉键值对
            SelectLevel.ItemsSource = LevelSelectByName;

            #region 读取上一次设置的值
            var v1 = registryStorage.OpenAfterStart("WPF_elementB_index");
            var v2 = registryStorage.OpenAfterStart("WPF_elementA_index");
            var v3 = registryStorage.OpenAfterStart("WPF_SelectLevel_index");
            var v4 = registryStorage.OpenAfterStart("WPF_7_throughtLevel");
            try
            {
                int.TryParse(v1, out int index1);
                elementA.SelectedIndex = index1;

                int.TryParse(v2, out int index2);
                elementB.SelectedIndex = index2;

                int.TryParse(v3, out int index3);
                SelectLevel.SelectedIndex = index3;

                if (v4 == "True")
                {
                    InLevel.IsChecked = true;
                }
                else
                {
                    SelectLevel.SelectedIndex = -1;
                    InLevel.IsChecked = false;
                }
            }
            catch
            {
                // 索引设置失败 默认值
                elementA.SelectedIndex = 0;
                elementB.SelectedIndex = 0;
                SelectLevel.SelectedIndex = 0;
                InLevel.IsChecked = true;
            }
            #endregion
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // 关闭窗口不运行
            this.IsHitTestVisible = false;
            this.Close();
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (InLevel.IsChecked == true && SelectLevel.SelectedIndex == -1)
            {
                TaskDialog.Show("提示", "设置冲突：\n需要选择楼层标高列中的名称\n或取消指定标高勾选");
            }
            else
            {
                registryStorage.SaveBeforeExit("WPF_elementB_index", elementA.SelectedIndex.ToString());
                registryStorage.SaveBeforeExit("WPF_elementA_index", elementB.SelectedIndex.ToString());
                registryStorage.SaveBeforeExit("WPF_SelectLevel_index", SelectLevel.SelectedIndex.ToString());
                registryStorage.SaveBeforeExit("WPF_7_throughtLevel", InLevel.IsChecked.Value.ToString());
                // var throughtLevel = window.InLevel.IsChecked.Value;
                this.Close();
                // 运行程序
                this.IsHitTestVisible = true;
            }

        }
    }
}
