using System;
using System.Collections.Generic;
using System.Windows;

using CADReader.Model;

namespace CADReader.View
{
    /// <summary>
    /// wpfFloor.xaml 的交互逻辑
    /// </summary>
    public partial class WPFFloorName : Window
    {
        public WPFFloorName()
        {
            InitializeComponent();

            List<SelectElement> listSource = new List<SelectElement>();
            foreach (var item in GlobaData.GlobaDataDic.ColorDic)
            {
                listSource.Add(new SelectElement
                {
                    ElementName = item.Key,
                });
            }
            System.Windows.Data.Binding listBinding = new System.Windows.Data.Binding
            {
                Source = listSource,
                Mode = System.Windows.Data.BindingMode.OneWay
            };
            ElementList.SetBinding(System.Windows.Controls.ListBox.ItemsSourceProperty, listBinding);
        }

        /// <summary>
        /// 创建
        /// </summary>
        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            #region 未指定楼板类型
            if (ElementList.SelectedIndex == -1)
            {
                MessageBox.Show("需要指定类型名称", "警告");
                return;
            }
            #endregion

            Close();
            IsHitTestVisible = false;
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        private void Window_Closed(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 取消创建
        /// </summary>
        private void No_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
