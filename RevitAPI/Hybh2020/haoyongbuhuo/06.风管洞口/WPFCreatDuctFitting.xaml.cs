using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
    /// WPFCreatDuctFitting.xaml 的交互逻辑
    /// </summary>
    public partial class WPFCreatDuctFitting : Window
    {
        // 注册列表缓存数据
        RegistryStorage registryStorage = new RegistryStorage();
        public WPFCreatDuctFitting(List<Element> linkInstances, List<Element> pipeFamilys, List<Element> levels)
        {
            InitializeComponent();

            List<SelectElementByName> listlinkInstances = new List<SelectElementByName>();

            // 获取所有链接模型
            foreach (Element elLink in linkInstances)
            {

                var name = elLink.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsValueString();
                listlinkInstances.Add(new SelectElementByName { HybhElement = elLink, HybhElName = name });
            }
            LinkInstances.ItemsSource = listlinkInstances;

            List<SelectElementByName> DuctFittings = new List<SelectElementByName>();
            // 类型名称的列表
            List<string> familyNams = new List<string>();
            // 获取所有的套管类型
            foreach (Element elPipe in pipeFamilys)
            {
                var Familytype = elPipe as FamilySymbol;
                var name = Familytype.Name;
                // 族类型获取类型名称 去重复
                if (!familyNams.Contains(name))
                {
                    DuctFittings.Add(new SelectElementByName { HybhElement = elPipe, HybhElName = Familytype.FamilyName + ":" + name });
                    familyNams.Add(name);
                }
            }
            DuctFittingType.ItemsSource = DuctFittings;

            List<SelectElementByName> LevelSelectByName = new List<SelectElementByName>();
            // 获取所有的标高
            foreach (Element elLevel in levels)
            {
                var level = elLevel as Level;
                var name = level.Name;

                LevelSelectByName.Add(new SelectElementByName { HybhElement = level, HybhElName = name });
            }
            SelectLevel.ItemsSource = LevelSelectByName;

            #region 读取上一次设置的值
            var v1 = registryStorage.OpenAfterStart("WPF_LinkInstances_index");
            var v2 = registryStorage.OpenAfterStart("WPF_DuctFittingType_index");
            var v3 = registryStorage.OpenAfterStart("WPF_SelectLevelDuct_index");
            var v4 = registryStorage.OpenAfterStart("WPF_6_throughtLevel");
            try
            {
                int.TryParse(v1, out int index1);
                LinkInstances.SelectedIndex = index1;

                int.TryParse(v2, out int index2);
                DuctFittingType.SelectedIndex = index2;

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
                LinkInstances.SelectedIndex = 0;
                DuctFittingType.SelectedIndex = 0;
                SelectLevel.SelectedIndex = 0;
                InLevel.IsChecked = true;
            }
            #endregion
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (InLevel.IsChecked == true && SelectLevel.SelectedIndex == -1)
            {
                TaskDialog.Show("提示", "设置冲突：\n需要选择楼层标高列中的名称\n或取消指定标高勾选");
            }
            else
            {
                registryStorage.SaveBeforeExit("WPF_LinkInstances_index", LinkInstances.SelectedIndex.ToString());
                registryStorage.SaveBeforeExit("WPF_DuctFittingType_index", DuctFittingType.SelectedIndex.ToString());
                registryStorage.SaveBeforeExit("WPF_SelectLevelDuct_index", SelectLevel.SelectedIndex.ToString());
                registryStorage.SaveBeforeExit("WPF_6_throughtLevel", InLevel.IsChecked.Value.ToString());
                // InLevel
                this.Close();
                this.IsHitTestVisible = true;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // 关闭 不允许
            this.IsHitTestVisible = false;
            this.Close();
        }
    }
}
