using Autodesk.Revit.UI;
using System.Windows;

namespace hybh
{
    /// <summary>
    /// WPFPickFilter.xaml 的交互逻辑
    /// </summary>
    public partial class WPFPickFilter : Window
    {
        public WPFPickFilter()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 定义属性
        /// </summary>
        public string MyTag { get; set; }

        #region 结构类型
        private void Wall_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "墙";
            this.Close();
        }

        private void Floor_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "楼板";
            this.Close();
        }

        private void Beam_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "结构框架";
            this.Close();
        }

        private void SColumn_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "结构柱";
            this.Close();
        }

        private void Founddation_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "结构基础";
            this.Close();
        }

        private void GenericModel_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "常规模型";
            this.Close();
        }
        #endregion

        #region 建筑类型
        private void Furniture_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "家具";
            this.Close();
        }

        private void Grid_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "轴网";
            this.Close();
        }

        private void Dimensions_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "尺寸标注";
            this.Close();
        }

        private void Rooms_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "房间";
            this.Close();
        }

        private void SpotElevations_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "高程点";
            this.Close();
        }

        private void Areas_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "面积";
            this.Close();
        }

        private void AreaTags_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "面积标记";
            this.Close();
        }

        private void Columns_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "柱";
            this.Close();
        }

        private void Ceiling_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "天花板";
            this.Close();
        }

        private void Doors_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "门";
            this.Close();
        }

        private void Windows_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "窗";
            this.Close();
        }


        private void StairsRailing_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "楼梯";
            this.Close();
        }
        #endregion

        #region 机电类型

        private void Pipe_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "管道";
            this.Close();
        }

        private void PipeFitting_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "管件";
            this.Close();
        }

        private void DuctCurves_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "风管";
            this.Close();
        }

        private void DuctFitting_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "风管管件";
            this.Close();
        }

        private void CableTray_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "电缆桥架";
            this.Close();
        }

        private void CableTrayFitting_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "电缆桥架配件";
            this.Close();
        }

        private void MechanicalEquipment_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "机械设备";
            this.Close();
        }

        private void Sprinklers_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "喷头";
            this.Close();
        }

        private void PipeAccessory_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "管道附件";
            this.Close();
        }

        private void DuctAccessory_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "风管附件";
            this.Close();
        }

        private void ElectricalEquipment_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "电气设备";
            this.Close();
        }

        private void LightingFixtures_Click(object sender, RoutedEventArgs e)
        {
            MyTag = "照明设备";
            this.Close();
        }

        #endregion
    }
}
