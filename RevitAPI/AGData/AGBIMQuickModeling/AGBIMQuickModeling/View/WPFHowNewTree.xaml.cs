﻿using System.Windows;
using System.Windows.Media;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using CADReader.Handler;

namespace CADReader.View
{
    /// <summary>
    /// WPFHowNewTree.xaml 的交互逻辑
    /// </summary>
    public partial class WPFHowNewTree : Window
    {
        private readonly EventHandlerTree Command = null;
        private readonly ExternalEvent Hander = null;
        private Document Doc { get; set; }
        private Selection Sel { get; set; }
        private Pickdwg Pickdwg { get; set; }
        public ToDo ToDo { get; set; }
        public WPFHowNewTree(UIDocument uidoc)
        {
            InitializeComponent();

            Command = new EventHandlerTree();
            Hander = ExternalEvent.Create(Command);

            Doc = uidoc.Document;
            Sel = uidoc.Selection;
            Pickdwg = new Pickdwg();
            ToDo = new ToDo();
        }

        /// <summary>
        /// 选择图层
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PickLayer_Click(object sender, RoutedEventArgs e)
        {
            Pickdwg.Refer(Sel, out Reference reference, "请点选规划图纸中植物(植物图库应包含中心定位点)图层");
            if(reference == null)
            {
                MessageBox.Show("未选择图层", "警告");
                return;
            }

            #region 导入图纸比例：米    IMPORT_DISPLAY_UNITS 
            if(!Doc.IsDisplayUnits(reference))
            {
                MessageBox.Show("链接图纸比例设置出错\n请删除已导入图纸并重新链接时设置单位：米", "警告");
                Close();
                return;
            }
            #endregion

            // 获取选择
            BoxLayer.Text = Doc.GetLayerName(reference);
            BoxLayer.Background = new SolidColorBrush(Colors.LightGreen);
            ToDo.ReferenceCurves = reference;
        }

        /// <summary>
        /// 选择族类型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PickFamily_Click(object sender, RoutedEventArgs e)
        {
            // 通用建模工具基于点
            WPFSymbolPlaceType familySymbol = new WPFSymbolPlaceType(Doc);
            //WPFFamilySymbol familySymbol = new WPFFamilySymbol(Doc, BuiltInCategory.OST_Planting);
            familySymbol.ShowDialog();
            if(familySymbol.IsHitTestVisible)
            {
                MessageBox.Show("已取消类型选择", "提示");
                return;
            }
            // 获取选择
            if(!(familySymbol.ElementList.SelectedItem is SelectElement selectElement))
            {
                MessageBox.Show("未指定类型名称", "警告");
                return;
            }
            BoxFamilyName.Text = "类型名称：" + selectElement.ElementName;
            BoxFamilyName.Background = new SolidColorBrush(Colors.LightGreen);
            ToDo.SelectElement = selectElement;
        }

        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if(ToDo.ReferenceCurves == null || ToDo.SelectElement == null)
            {
                MessageBox.Show("请按步骤依次选择相应内容", "提示");
                return;
            }
            // 创建模型
            Command.ToDoData = ToDo;
            Hander.Raise();
            Close();
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NO_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
