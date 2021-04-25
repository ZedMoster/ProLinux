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
            string tabName = "规划建模";
            // 创建面板
            application.CreateRibbonTab(tabName);
            // 添加面板到菜单栏
            RibbonPanel RibbonPanel = application.CreateRibbonPanel(tabName, tabName);

            /// -------------------------------------功能---------------------------------------
            /// 

            #region 墙顶部生成屋顶  AppRoofs
            var buttonData_AppRoofs = new PushButtonParamter
            {
                InName = "墙生屋顶",
                ButtonName = "墙生\n屋顶",
                NameSpace = "AppRoofs",
                ImageName = "icon.png",
                StackedImageName = "icon_16.png",
                Tooltip =
                "闭合墙顶部创建屋顶：" +
                "\n1.选择墙体且墙体可围成闭合区域" +
                "\n2.设置屋顶类型参数及坡度" +
                "\n    可选参数：若类型不存在可简单设置名称、厚度及材质新建类型" +
                "\n3.则该墙体轮廓区域范围将自动创建屋顶",
            };
            #endregion

            #region 线性布置车位
            var buttonData_ParkingWPF = new PushButtonParamter
            {
                InName = "线性车位",
                ButtonName = "线性\n车位",
                NameSpace = "AppParking",
                ImageName = "icon.png",
                StackedImageName = "icon_16.png",
                Tooltip =
                "线性绘制布置车位：" +
                "\n1.首先选择停车位族文件" +
                "\n2.在指定本次创建的停车位族类型" +
                "\n3.设置停车位族尺寸信息" +
                "\n    可选参数：是否成组布置设置间距及成组个数" +
                "\n4.点击线性布置后点击车位布置的起点及终点" +
                "\n    即完成车位模型创建",
            };
            #endregion

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

            #region 批量建筑外墙
            var buttonData_NewWallsExterior = new PushButtonParamter
            {
                InName = "识别外墙",
                ButtonName = "识别\n外墙",
                NameSpace = "NewWallsExterior",
                ImageName = "icon.png",
                StackedImageName = "icon_16.png",
                Tooltip =
                "识别图纸创建外墙:" +
                "\n1.先选择CAD图纸外墙轮廓线(应闭合)" +
                "\n2.再选择CAD图纸外墙注释文字信息(注释参数)" +
                "\n3.设置创建的外墙类型及楼层标高" +
                "\n    可选参数：女儿墙高度(大于0则创建)及外墙结构类型" +
                "\n4.确认后可在选择的楼层创建外墙",
            };
            #endregion

            #region 批量建筑内墙
            var buttonData_Wall = new PushButtonParamter
            {
                InName = "识别内墙",
                ButtonName = "识别\n内墙",
                NameSpace = "NewWalls",
                ImageName = "icon.png",
                StackedImageName = "icon_16.png",
                Tooltip =
                "识别图纸建筑内墙:" +
                "\n1.选择CAD图纸内墙轮廓线图层的任一图元" +
                "\n2.自动依据墙厚选择相应的类型创建墙体" +
                "\n    建议:使用完整的项目样板调用此功能" +
                "\n3.自动创建识别信息的内墙",
            };
            #endregion

            #region 批量建筑门
            var buttonData_Door = new PushButtonParamter
            {
                InName = "识别门",
                ButtonName = "识别\n门",
                NameSpace = "NewDoors",
                ImageName = "icon.png",
                StackedImageName = "icon_16.png",
                Tooltip =
                "识别图纸创建门：" +
                "\n1.选择CAD图纸门注释信息图层的任一图元" +
                "\n2.更具参数信息指定创建的族名称" +
                "\n    可选参数：设置宽度高度及低高度" +
                "\n3.确认后可在当前标高创建门",
            };
            #endregion

            #region 批量建筑窗
            var buttonData_Windows = new PushButtonParamter
            {
                InName = "识别窗",
                ButtonName = "识别\n窗",
                NameSpace = "NewWindows",
                ImageName = "icon.png",
                StackedImageName = "icon_16.png",
                Tooltip =
                "识别图纸创建窗：" +
                "\n1.选择CAD图纸窗注释信息图层的任一图元" +
                "\n2.更具参数信息指定创建的族名称" +
                "\n    可选参数：设置宽度高度及低高度" +
                "\n3.确认后可在当前标高创建窗",
            };
            #endregion

            #region 批量屋顶
            var buttonData_Roofs = new PushButtonParamter
            {
                InName = "识别屋顶",
                ButtonName = "识别\n屋顶",
                NameSpace = "NewRoofs",
                ImageName = "icon.png",
                StackedImageName = "icon_16.png",
                Tooltip =
                "选择图纸创建屋顶:" +
                "\n1.先选择CAD图纸屋顶轮廓线(应闭合)" +
                "\n2.设置创建的屋顶类型及屋顶坡度(单位：度)" +
                "\n    可选参数：若类型不存在可简单设置名称、厚度及材质新建类型" +
                "\n3.确认后可在当前标高创建屋顶",
            };
            #endregion

            #region 线性布置车位
            var buttonData_Parking = new PushButtonParamter
            {
                InName = "识别车位",
                ButtonName = "识别\n车位",
                NameSpace = "NewParking",
                ImageName = "icon.png",
                StackedImageName = "icon_16.png",
                Tooltip =
                "选择图纸识别停车位:" +
                "\n1.先选择CAD图纸矩形停车位边线(图元矩形)" +
                "\n2.先选择CAD图纸停车位编号(注释参数)" +
                "\n    注意：可替换插件目录停车位族文件" +
                "\n    族信息应保持一直：中心点位、参数名称及类型" +
                "\n3.自动在当前标高创建停车位模型",
            };
            #endregion

            #region 创建道路
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
                "\n2.点选CAD图纸建筑明细表中文字图层"+
                "\n3.点选CAD图纸建筑外轮廓线"+
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



            #region 功能添加到面板
            RibbonPanel.AddItem(buttonData_box.PushData());
            RibbonPanel.AddItem(buttonData_roads.PushData());
            RibbonPanel.AddItem(buttonData_Floor.PushData());
            //RibbonPanel.AddSeparator();
            //RibbonPanel.AddItem(buttonData_Floor.PushData());
            //RibbonPanel.AddItem(buttonData_NewWallsExterior.PushData());
            //RibbonPanel.AddItem(buttonData_Wall.PushData());
            //RibbonPanel.AddItem(buttonData_Door.PushData());
            //RibbonPanel.AddItem(buttonData_Windows.PushData());
            //RibbonPanel.AddItem(buttonData_Roofs.PushData());
            //RibbonPanel.AddItem(buttonData_Parking.PushData());
            //RibbonPanel.AddSeparator();
            //RibbonPanel.AddItem(buttonData_ParkingWPF.PushData());
            //RibbonPanel.AddItem(buttonData_AppRoofs.PushData());
            #endregion

            return Result.Succeeded;
        }
    }
}
