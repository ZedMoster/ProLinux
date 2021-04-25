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

namespace hybh
{
    /// <summary>
    /// WPFCreateBeamWithColumns.xaml 的交互逻辑
    /// </summary>
    public partial class WPFCreateBeamWithColumns : Window
    {
        readonly RegistryStorage RegistryStorage = new RegistryStorage();
        public WPFCreateBeamWithColumns(List<SelectElementByName> listPipingSystem)
        {
            InitializeComponent();

            // 选择列表
            tp.ItemsSource = listPipingSystem;

            // 读取梁的参数
            BeamInput_width.Text = RegistryStorage.OpenAfterStart("BeamInput_width") ?? "400";
            BeamInput_heigh.Text = RegistryStorage.OpenAfterStart("BeamInput_heigh") ?? "600";
            // 读取选择的梁类型
            var I = RegistryStorage.OpenAfterStart("BEAM_index") ?? "0";
            int.TryParse(I, out int index);
            tp.SelectedIndex = index;
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            
            RegistryStorage.SaveBeforeExit("BeamInput_width", BeamInput_width.Text);
            RegistryStorage.SaveBeforeExit("BeamInput_heigh", BeamInput_heigh.Text);
            RegistryStorage.SaveBeforeExit("BEAM_index", tp.SelectedIndex.ToString());

            this.Close();
            IsHitTestVisible = true;
        }

        private void 梁尺寸_Closed(object sender, EventArgs e)
        {
            IsHitTestVisible = false;
            this.Close();
        }
    }
}
