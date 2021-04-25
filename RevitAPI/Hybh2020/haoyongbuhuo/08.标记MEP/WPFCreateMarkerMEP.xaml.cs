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
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace hybh
{
    /// <summary>
    /// WPFCreateMarkerMEP.xaml 的交互逻辑
    /// </summary>
    public partial class WPFCreateMarkerMEP : Window
    {
        // 注册列表缓存数据
        readonly RegistryStorage registryStorage = new RegistryStorage();
        //elementArrow, pipeTags, ductTags, cableTrayTags
        public WPFCreateMarkerMEP(IList<Element> elementArrow, IList<Element> pipeTags, IList<Element> ductTags, IList<Element> cableTrayTags)
        {

            InitializeComponent();

            // 箭头
            List<SelectElementByName> listelementArrow = getListElementSelect(elementArrow);

            Arrow.ItemsSource = listelementArrow;

            // 标记管道
            List<SelectElementByName> listpipeTags = getListFamilySelect(pipeTags);
            PipeType.ItemsSource = listpipeTags;


            // 标记风管
            List<SelectElementByName> listductTags = getListFamilySelect(ductTags);
            DuctType.ItemsSource = listductTags;

            // 标记桥架
            List<SelectElementByName> listcableTrayTags = getListFamilySelect(cableTrayTags);
            CableTrayType.ItemsSource = listcableTrayTags;

            // 读取上一次设置的值
            try
            {
                var v1 = registryStorage.OpenAfterStart("WPF_Arrow_index");
                var v2 = registryStorage.OpenAfterStart("WPF_PipeType_index");
                var v3 = registryStorage.OpenAfterStart("WPF_DuctType_index");
                var v4 = registryStorage.OpenAfterStart("WPF_CableTrayType_index");

                try
                {
                    int.TryParse(v1, out int index1);
                    Arrow.SelectedIndex = index1;
                    int.TryParse(v2, out int index2);
                    PipeType.SelectedIndex = index2;
                    int.TryParse(v3, out int index3);
                    DuctType.SelectedIndex = index3;
                    int.TryParse(v4, out int index4);
                    CableTrayType.SelectedIndex = index4;
                }
                catch
                {
                    // 索引设置失败 默认值
                    Arrow.SelectedIndex = 0;
                    PipeType.SelectedIndex = 0;
                    DuctType.SelectedIndex = 0;
                    CableTrayType.SelectedIndex = 0;
                }
            }
            catch (Exception e) { TaskDialog.Show("提示", e.Message + Strings.error); }

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.IsHitTestVisible = false;
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            registryStorage.SaveBeforeExit("WPF_Arrow_index", Arrow.SelectedIndex.ToString());
            registryStorage.SaveBeforeExit("WPF_PipeType_index", PipeType.SelectedIndex.ToString());
            registryStorage.SaveBeforeExit("WPF_DuctType_index", DuctType.SelectedIndex.ToString());
            registryStorage.SaveBeforeExit("WPF_CableTrayType_index", CableTrayType.SelectedIndex.ToString());
            this.Close();
            this.IsHitTestVisible = true;
        }

        public List<SelectElementByName> getListElementSelect(IList<Element> elements)
        {
            List<SelectElementByName> listelement = new List<SelectElementByName>();

            // 获取所有的系统类型名称
            foreach (Element el in elements)
            {
                listelement.Add(new SelectElementByName { HybhElement = el, HybhElName = el.Name });
            }
            return listelement;
        }

        public List<SelectElementByName> getListFamilySelect(IList<Element> elements)
        {
            List<SelectElementByName> listelement = new List<SelectElementByName>();

            // 获取所有的系统类型名称
            foreach (Element el in elements)
            {
                var elF = el as ElementType;
                var elname = elF.FamilyName + ":" + elF.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString();
                listelement.Add(new SelectElementByName { HybhElement = el, HybhElName = elname });
            }
            return listelement;
        }

        public List<SelectElementByName> getListStringSelect(IList<Element> elements)
        {
            List<SelectElementByName> listelement = new List<SelectElementByName>();

            // 获取所有的系统类型名称
            foreach (Element el in elements)
            {
                listelement.Add(new SelectElementByName { HybhElement = el, HybhElName = el.Name });
            }
            return listelement;
        }
    }
}
