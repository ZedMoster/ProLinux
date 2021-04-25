using Autodesk.Revit.UI;

namespace CADReader
{
    class App : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            // 面板名称
            string tabName = "规划转换";
            // 创建面板
            application.CreateRibbonTab(tabName);
            // 添加面板到菜单栏
            RibbonPanel RibbonPanel = application.CreateRibbonPanel(tabName, tabName);

            // -------------------------------------功能---------------------------------------

            #region 创建楼板
            var buttonData_Floor = new PushButtonParamter
            {
                InName = "识别楼板",
                ButtonName = "识别\n楼板",
                NameSpace = "NewFloor",
                ImageName = "icon.png",
                StackedImageName = "icon_16.png",
                Tooltip =
                "识别图纸创建楼板:" +
                "\n1.先选择CAD图纸楼板轮廓线(应闭合)" +
                "\n2.再选择CAD图纸楼板注释文字信息(注释参数)" +
                "\n3.设置创建的楼板类型及楼层标高" +
                "\n    可选参数：楼板偏移值及楼板结构类型" +
                "\n4.确认后可在选择的楼层创建楼板",
            };
            #endregion

            #region 创建建筑
            var buttonData_box = new PushButtonParamter
            {
                InName = "识别建筑",
                ButtonName = "识别\n建筑",
                NameSpace = "NewBoxExterior",
                ImageName = "icon.png",
                StackedImageName = "icon_16.png",
                Tooltip =
                "识别图纸创建建筑实体:" +
                "\n1.框选CAD图纸建筑明细表" +
                "\n2.点选CAD图纸建筑明细表中文字图层" +
                "\n3.点选CAD图纸建筑外轮廓线" +
                "\n4.点选CAD图纸建筑编号",
            };
            #endregion

            #region 创建道路
            var buttonData_roads = new PushButtonParamter
            {
                InName = "识别道路",
                ButtonName = "识别\n道路",
                NameSpace = "NewRoads",
                ImageName = "icon.png",
                StackedImageName = "icon_16.png",
                Tooltip =
                "识别图纸创建道路:" +
                "\n1.选择CAD图纸道路中心线图层" +
                "\n2.自动创建道路",
            };
            #endregion

            #region 创建车位
            var buttonData_parking = new PushButtonParamter
            {
                InName = "识别车位",
                ButtonName = "识别\n车位",
                NameSpace = "NewParking",
                ImageName = "icon.png",
                StackedImageName = "icon_16.png",
                Tooltip =
                "识别图纸创建车位:" +
                "\n1.选择CAD图纸车位边线图层" +
                "\n    自动创建车位模型",
            };
            #endregion

            #region 创建灌木
            var buttonData_trees = new PushButtonParamter
            {
                InName = "识别植物",
                ButtonName = "识别\n植物",
                NameSpace = "NewTrees",
                ImageName = "icon.png",
                StackedImageName = "icon_16.png",
                Tooltip =
                "识别图纸创建植物:" +
                "\n1.选择CAD图纸植物圆心图层" +
                "\n    自动创建植物模型",
            };
            #endregion

            // -------------------------------------面板---------------------------------------

            #region 功能添加到面板
            RibbonPanel.AddItem(buttonData_box.PushData());
            RibbonPanel.AddItem(buttonData_roads.PushData());
            RibbonPanel.AddItem(buttonData_parking.PushData());
            RibbonPanel.AddItem(buttonData_trees.PushData());
            RibbonPanel.AddItem(buttonData_Floor.PushData());
            #endregion

            return Result.Succeeded;
        }
    }
}
