using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class Hybh2020 : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded; // 不需要处理
        }
        public Result OnStartup(UIControlledApplication application)
        {

            #region //////////////////////////////////////////////  -*-标签面板-*-  //////////////////////////////////////////////////////
            // 创建标签 tab
            String tabName = "hybh2020";
            application.CreateRibbonTab(tabName);

            // 创建面板 panel
            RibbonPanel PanelSetting = application.CreateRibbonPanel(tabName, "全局属性");
            RibbonPanel PanelCreate = application.CreateRibbonPanel(tabName, "创建功能");
            RibbonPanel PanelCAD = application.CreateRibbonPanel(tabName, "图纸转换");
            RibbonPanel PanelManage = application.CreateRibbonPanel(tabName, "综合功能");
            RibbonPanel PanelAlign = application.CreateRibbonPanel(tabName, "对齐功能");
            RibbonPanel PanelModel = application.CreateRibbonPanel(tabName, "审查功能");
            RibbonPanel PanelPick = application.CreateRibbonPanel(tabName, "高级选择");
            RibbonPanel PanelAbout = application.CreateRibbonPanel(tabName, "关于插件");
            //RibbonPanel PanelText = application.CreateRibbonPanel(tabName, "测试功能");


            /////   -*-属性设置-*-    ////    HybhSettings
            var ButtonAbout = new ButtonParamter
            {
                InName = "关于",
                ButtonName = "关于",
                NameSpace = "AboutMe",
                Tooltip = "希望能听到您更多的反馈信息",
                ImageName = "A-000.png",
                StackedImageName = "A-000-16.png"
            };
            PushButtonData AboutMeData = GetButtonData(ButtonAbout);
            PanelAbout.AddItem(AboutMeData);
            #endregion

            #region //////////////////////////////////////////////  -*-插件全局-*-  //////////////////////////////////////////////////////

            /////   -*-属性设置-*-    ////    HybhSettings
            var ButtonhybhSettings = new ButtonParamter
            {
                InName = "属性设置",
                ButtonName = "属性设置",
                NameSpace = "HybhSettings",
                Tooltip = "插件部分功能的参数支持自定义设置",
                ImageName = "L-002.png",
                StackedImageName = "L-002-16.png"
            };
            PushButtonData hybhSettingsData = GetButtonData(ButtonhybhSettings);

            /////   -*-用户登录-*-    ////    LoginInUser
            var ButtonLoginInUser = new ButtonParamter
            {
                InName = "用户登录",
                ButtonName = "用户登录",
                NameSpace = "LoginInUser",
                Tooltip = "注册插件及登录\n后台回复\n<注册插件 用户名>\n 获取验证码后\n即可成功注册\n\n本插件由指定开发公众号开发注册后功能免费使用",
                ImageName = "L-000.png",
                StackedImageName = "L-000-16.png"
            };
            PushButtonData LoginInUserData = GetButtonData(ButtonLoginInUser);

            /////   -*-用户注销-*-    ////    LogoutUser
            var ButtonLogoutUser = new ButtonParamter
            {
                InName = "用户注销",
                ButtonName = "用户注销",
                NameSpace = "LogoutUser",
                Tooltip = "注册插件及注销\n每个用户支持两台电脑同时登录使用\n\n本插件由指定开发公众号开发注册后功能免费使用",
                ImageName = "L-001.png",
                StackedImageName = "L-001-16.png"
            };
            PushButtonData LogoutUserData = GetButtonData(ButtonLogoutUser);

            PanelSetting.AddStackedItems(LoginInUserData, LogoutUserData, hybhSettingsData);
            #endregion

            #region //////////////////////////////////////////////  -*-功能增强-*-  //////////////////////////////////////////////////////
            /////   -*-替换背景颜色-*-    ////    BackgroundColor 
            var ButtonBackgroundColor = new ButtonParamter
            {
                InName = "转换背景色",
                ButtonName = "转换\n背景色",
                NameSpace = "BackgroundColor",
                Tooltip = "一键替换视图背景颜色\n\n颜色可在全局属性-属性设置\n设置相应的转换颜色",
                ImageName = "A-001.png",
                StackedImageName = "A-001-16.png"
            };
            PushButtonData sBackgroundColorData = GetButtonData(ButtonBackgroundColor);
            PanelManage.AddItem(sBackgroundColorData);

            /////   -*-重命名标高-*-    ////    ReNameLevel
            var ButtonReNameLevel = new ButtonParamter
            {
                InName = "标高重命名",
                ButtonName = "标高\n重命名",
                NameSpace = "ReNameLevel",
                Tooltip = "自动将标高按设定规则进行重命名\n规则:\nF{1}{(A)}_{0.000}\nB{1}{(S)}_{0.000}",
                ImageName = "A-012.png",
                StackedImageName = "A-012-16.png"
            };
            PushButtonData ReNameLevelData = GetButtonData(ButtonReNameLevel);
            PanelManage.AddItem(ReNameLevelData);

            /////   -*-隔离指定注释参数模型-*-    ////    ReloadDwg
            var ButtonReloadDwg = new ButtonParamter
            {
                InName = "重载图纸",
                ButtonName = "重载\n图纸",
                NameSpace = "ReloadDwg",
                Tooltip = "选择图纸后自动重新加载更新图纸内容",
                ImageName = "A-018.png",
                StackedImageName = "A-018-16.png"
            };
            PushButtonData ReloadDwgData = GetButtonData(ButtonReloadDwg);
            PanelManage.AddItem(ReloadDwgData);

            /////   -*-隔离指定注释参数模型-*-    ////    SelectKeyWordElements
            var ButtonSelectKeyWordElements = new ButtonParamter
            {
                InName = "隔离注释",
                ButtonName = "隔离\n注释",
                NameSpace = "SelectKeyWordElements",
                Tooltip = "隔离指定<注释>参数值等于选定的值\n\n包含构件的类别：\n梁、板、墙、柱、常规模型、门、窗、风管、管道、电缆桥架",
                ImageName = "A-007.png",
                StackedImageName = "A-007-16.png"
            };
            PushButtonData SelectKeyWordElementsData = GetButtonData(ButtonSelectKeyWordElements);
            PanelManage.AddItem(SelectKeyWordElementsData);

            /////   -*-批量扣减模型-*-    ////    InstanceVoidCutUtils
            var ButtonInstanceVoidCutUtils = new ButtonParamter
            {
                InName = "批量扣减",
                ButtonName = "批量\n扣减",
                NameSpace = "InstanceVoidCutUtils",
                Tooltip = "批量自动扣减模型\n--可以设置指定楼层构件\n--提前在窗口中旋转相应的构件设定范围\n\n连接-->对应实体模型自动连接\n剪切-->对应空心洞口自动剪切\n默认A类别扣减B类别",
                ImageName = "A-006.png",
                StackedImageName = "A-006-16.png"
            };
            PushButtonData InstanceVoidCutUtilsData = GetButtonData(ButtonInstanceVoidCutUtils);
            PanelManage.AddItem(InstanceVoidCutUtilsData);

            /////   -*-求和体积-*-    ////    CalculateVolume
            var ButtonCalculateVolume = new ButtonParamter
            {
                InName = "求和体积",
                ButtonName = "求和\n体积",
                NameSpace = "CalculateVolume",
                Tooltip = "自动计算选择的结构模型体积和",
                ImageName = "A-009.png",
                StackedImageName = "A-009-16.png"
            };
            PushButtonData CalculateVolumeData = GetButtonData(ButtonCalculateVolume);
            PanelManage.AddItem(CalculateVolumeData);

            /////   -*-EXCEL文件导入 + 导出-*-    ////    ExcelExporterImporter.Command
            var ButtonExcelExporterImporter = new ButtonParamter
            {
                InName = "处理明细表",
                ButtonName = "处理\n明细表",
                AssemblyName = @"C:\ProgramData\Autodesk\ApplicationPlugins\hybh2020.bundle\Contents\2018\ExcelExporterImporter\ExcelExporterImporter.dll",
                NameSpace = "ExcelExporterImporter.Command",
                Tooltip = "明细表数据的导出和导入功能",
                ImageName = "A-013.png",
                StackedImageName = "A-013-16.png"
            };
            PushButtonData ExcelExporterImporterData = GetButtonData(ButtonExcelExporterImporter);
            PanelManage.AddItem(ExcelExporterImporterData);

            ////////   -*-视图可见性设置-*-    ////    SetCategoryHidden
            var ButtonSetCategoryHidden = new ButtonParamter
            {
                InName = "管理可见性",
                ButtonName = "管理\n可见性",
                NameSpace = "SetCategoryHidden",
                Tooltip = "方便快捷管理当前视图图元可见性\n按专业进行分类\n土建、机电、注释三大类",
                ImageName = "A-008.png",
                StackedImageName = "A-008-16.png"
            };
            PushButtonData SetCategoryHiddenData = GetButtonData(ButtonSetCategoryHidden);
            PanelManage.AddItem(SetCategoryHiddenData);

            #endregion

            #region //////////////////////////////////////////////  -*-高级选择-*-  //////////////////////////////////////////////////////
            ////////   -*-选择过滤器-*-    ////    PickFilter
            var ButtonPickFilter = new ButtonParamter
            {
                InName = "高级选择",
                ButtonName = "高级选择",
                NameSpace = "PickFilter",
                Tooltip = "在弹出的窗口中指定类别的模型\n框选选择指定类别的图元\n空格完成选择",
                ImageName = "A-014.png",
                StackedImageName = "A-014-16.png"
            };
            PushButtonData PickFilterData = GetButtonData(ButtonPickFilter);

            ////////   -*-选择模型-*-    ////    PickModel
            var ButtonPickModel = new ButtonParamter
            {
                InName = "选择模型",
                ButtonName = "选择模型",
                NameSpace = "PickModel",
                Tooltip = "框选选择模型类别的图元\n空格完成选择",
                ImageName = "A-011.png",
                StackedImageName = "A-011-16.png"
            };
            PushButtonData PickModelData = GetButtonData(ButtonPickModel);

            ////////   -*-选择标记-*-    ////    PickDetail
            var ButtonPickDetail = new ButtonParamter
            {
                InName = "选择标记",
                ButtonName = "选择标记",
                NameSpace = "PickDetail",
                Tooltip = "框选注释标记类别的图元\n空格完成选择",
                ImageName = "A-010.png",
                StackedImageName = "A-010-16.png"
            };
            PushButtonData PickDetailData = GetButtonData(ButtonPickDetail);

            ////////   -*-新增选择-*-    ////    SelWrite
            var ButtonSelWrite = new ButtonParamter
            {
                InName = "新增选择",
                ButtonName = "新增选择",
                NameSpace = "SelWrite",
                Tooltip = "保存当前窗口已选中的构件\n保存后通过加载选择自动选中构件集合\n\n公式（+=）",
                ImageName = "A-015.png",
                StackedImageName = "A-015-16.png"
            };
            PushButtonData SelWriteData = GetButtonData(ButtonSelWrite);

            ////////   -*-删除选择-*-    ////    SelRemove
            var ButtonSelRemove = new ButtonParamter
            {
                InName = "删除选择",
                ButtonName = "删除选择",
                NameSpace = "SelRemove",
                Tooltip = "在当前已保存选中的构件中\n删除包含本次选中的构件\n\n公式（-=）",
                ImageName = "A-016.png",
                StackedImageName = "A-016-16.png"
            };
            PushButtonData SelRemoveData = GetButtonData(ButtonSelRemove);

            ////////   -*-载入选择-*-    ////    SelRead
            var ButtonSelRead = new ButtonParamter
            {
                InName = "加载选择",
                ButtonName = "加载选择",
                NameSpace = "SelRead",
                Tooltip = "载入已缓存的选择构件\n自动在窗口中选中构件",
                ImageName = "A-017.png",
                StackedImageName = "A-017-16.png"
            };
            PushButtonData SelReadData = GetButtonData(ButtonSelRead);

            PanelPick.AddStackedItems(PickFilterData, PickModelData, PickDetailData);
            PanelPick.AddSeparator();
            PanelPick.AddStackedItems(SelWriteData, SelRemoveData, SelReadData);
            #endregion

            #region //////////////////////////////////////////////  -*-对齐楼板-*-  //////////////////////////////////////////////////////

            #region 基于点的构件对齐楼板
            var ButtonAlignElementInLocationPointToFloor = new ButtonParamter
            {
                InName = "点构件参照楼板",
                ButtonName = "点构件\n参照楼板",
                NameSpace = "AlignElementInLocationPointToFloor",
                Tooltip = "基于点定位的构件参照楼板面自动调整偏移值\n可在全局属性中进行距离板面偏移的参数设置\n默认为：0",
                StackedImageName = "S-013-16.png",
                ImageName = "S-013.png"
            };
            var AlignElementInLocationPointToFloorData = GetButtonData(ButtonAlignElementInLocationPointToFloor);
            //PanelAlign.AddItem(AlignElementInLocationPointToFloorData);
            #endregion

            #region 基于线的构件对齐楼板
            var ButtonAlignElementInLocationCurveToFloor = new ButtonParamter
            {
                InName = "线构件参照楼板",
                ButtonName = "线构件\n参照楼板",
                NameSpace = "AlignElementInLocationCurveToFloor",
                Tooltip = "基于线定位的构件参照楼板面自动调整偏移值\n可在全局属性中进行距离板面偏移的参数设置\n默认为：0",
                StackedImageName = "S-014-16.png",
                ImageName = "S-014.png"
            };
            var AlignElementInLocationCurveToFloorData = GetButtonData(ButtonAlignElementInLocationCurveToFloor);
            //PanelAlign.AddItem(AlignElementInLocationCurveToFloorData);
            #endregion

            #region 基于点的构件对齐楼板
            var ButtonAlignElementInLocationPointToLinkFloor = new ButtonParamter
            {
                InName = "点构件参照链接楼板",
                ButtonName = "点构件\n链接楼板",
                NameSpace = "AlignElementInLocationPointToLinkFloor",
                Tooltip = "基于点定位的构件参照楼板面自动调整偏移值\n可在全局属性中进行距离链接文件中板面偏移的参数设置\n默认为：0",
                StackedImageName = "S-015-16.png",
                ImageName = "S-015.png"
            };
            var AlignElementInLocationPointToLinkFloorData = GetButtonData(ButtonAlignElementInLocationPointToLinkFloor);
            //PanelAlign.AddItem(AlignElementInLocationPointToLinkFloorData);
            #endregion

            #region 基于线的构件对齐链接楼板
            var ButtonAlignElementInLocationCurveToLinkFloor = new ButtonParamter
            {
                InName = "线构件参照链接楼板",
                ButtonName = "线构件\n链接楼板",
                NameSpace = "AlignElementInLocationCurveToLinkFloor",
                Tooltip = "基于线定位的构件参照楼板面自动调整偏移值\n可在全局属性中进行距离链接文件中板面偏移的参数设置\n默认为：0",
                StackedImageName = "S-016-16.png",
                ImageName = "S-016.png"
            };
            var AlignElementInLocationCurveToLinkFloorData = GetButtonData(ButtonAlignElementInLocationCurveToLinkFloor);
            //PanelAlign.AddItem(AlignElementInLocationCurveToLinkFloorData);
            #endregion

            SplitButtonData splitButtonDataPoint = new SplitButtonData("点构件偏移", "点构件偏移");
            SplitButton splitButtonPoint = PanelAlign.AddItem(splitButtonDataPoint) as SplitButton;
            splitButtonPoint.AddPushButton(AlignElementInLocationPointToFloorData);
            splitButtonPoint.AddPushButton(AlignElementInLocationPointToLinkFloorData);

            SplitButtonData splitButtonDataCurve = new SplitButtonData("线构件偏移", "线构件偏移");
            SplitButton splitButtonCurve = PanelAlign.AddItem(splitButtonDataCurve) as SplitButton;
            splitButtonCurve.AddPushButton(AlignElementInLocationCurveToFloorData);
            splitButtonCurve.AddPushButton(AlignElementInLocationCurveToLinkFloorData);

            #region 创建一个可切换的按钮
            //RadioButtonGroupData radioDataPoint = new RadioButtonGroupData("点构件参照楼板偏移");
            //RadioButtonGroup radioButtonGroupPoint = PanelAlign.AddItem(radioDataPoint) as RadioButtonGroup;
            //radioButtonGroupPoint.AddItem(AlignElementInLocationPointToFloorData);
            //radioButtonGroupPoint.AddItem(AlignElementInLocationPointToLinkFloorData);

            //RadioButtonGroupData radioDataCurve= new RadioButtonGroupData("线构件参照楼板偏移");
            //RadioButtonGroup radioButtonGroupCurve = PanelAlign.AddItem(radioDataCurve) as RadioButtonGroup;
            //radioButtonGroupCurve.AddItem(AlignElementInLocationCurveToFloorData);
            //radioButtonGroupCurve.AddItem(AlignElementInLocationCurveToLinkFloorData);
            #endregion

            #endregion

            #region //////////////////////////////////////////////  -*-梁齐楼板-*-  //////////////////////////////////////////////////////
            /////   -*-梁齐楼板-*-    ////    AlignBeamBottomtoFloorBottom  AlignBeamToptoFloorTop AlignBeamBtoColumnF
            var ButtonAlignBeamBtoFloorB = new ButtonParamter
            {
                InName = "梁齐板底",
                ButtonName = "梁齐板底",
                NameSpace = "AlignBeamBottomtoFloorBottom",
                Tooltip = "选中待调整的梁\n在选中参考的楼板\n自动更新参数使得梁底面自动平齐楼板板底",
                StackedImageName = "S-001-16.png",
                ImageName = "S-001.png"
            };
            PushButtonData AlignBeamBtoFloorBData = GetButtonData(ButtonAlignBeamBtoFloorB);

            var ButtonAlignBeamTtofloorF = new ButtonParamter
            {
                InName = "梁齐板面",
                ButtonName = "梁齐板面",
                NameSpace = "AlignBeamToptoFloorTop",
                Tooltip = "选中待调整的梁\n在选中参考的楼板梁\n自动更新参数使得梁面自动平齐楼板板面",
                StackedImageName = "S-002-16.png",
                ImageName = "S-002.png"
            };
            PushButtonData AlignBeamTtofloorFData = GetButtonData(ButtonAlignBeamTtofloorF);

            var ButtonAlignBeamtoColumnLocation = new ButtonParamter
            {
                InName = "梁齐柱心",
                ButtonName = "梁齐柱心",
                NameSpace = "AlignBeamtoColumnLocation",
                Tooltip = "框选梁和柱子\n梁端点自动对齐到柱子中心定位点\n\n柱个数应为1",
                StackedImageName = "S-003-16.png",
                ImageName = "S-003.png"
            };
            PushButtonData AlignBeamtoColumnLocationData = GetButtonData(ButtonAlignBeamtoColumnLocation);

            /////   -*-柱齐楼板-*-    ////    AlignColumnTopWithFace  AlignColumnBaseWithFace  AlignBeamBtoColumnF

            var ButtonAlignColumnTopWithFace = new ButtonParamter
            {
                InName = "柱顶齐板",
                ButtonName = "柱顶齐板",
                NameSpace = "AlignColumnTopWithFloor",
                Tooltip = "选中待调整的柱\n在选中柱顶参考的楼板\n调整结构柱顶部偏移参数自动平齐楼板板面",
                StackedImageName = "S-010-16.png",
                ImageName = "S-010.png"
            };
            PushButtonData AlignColumnTopWithFaceData = GetButtonData(ButtonAlignColumnTopWithFace);

            var ButtonAlignColumnBaseWithFace = new ButtonParamter
            {
                InName = "柱底齐板",
                ButtonName = "柱底齐板",
                NameSpace = "AlignColumnBaseWithFloor",
                Tooltip = "选中待调整的柱\n在选中柱底参考的楼板\n调整结构柱底部偏移参数自动平齐平楼板板面",
                StackedImageName = "S-011-16.png",
                ImageName = "S-011.png"
            };
            PushButtonData AlignColumnBaseWithFaceData = GetButtonData(ButtonAlignColumnBaseWithFace);

            var ButtonAlignBeamBtoColumnF = new ButtonParamter
            {
                InName = "墙梁齐柱",
                ButtonName = "墙梁齐柱",
                NameSpace = "AlignBeamBtoColumn",
                Tooltip = "选中墙和梁后\n在选中需要对齐的柱子\n自动将墙梁构件对齐到结构柱表面",
                StackedImageName = "S-012-16.png",
                ImageName = "S-012.png"
            };
            PushButtonData AlignBeamBtoColumnFData = GetButtonData(ButtonAlignBeamBtoColumnF);

            PanelAlign.AddSeparator();
            PanelAlign.AddStackedItems(AlignColumnTopWithFaceData, AlignColumnBaseWithFaceData, AlignBeamBtoColumnFData);
            PanelAlign.AddSeparator();
            PanelAlign.AddStackedItems(AlignBeamTtofloorFData, AlignBeamBtoFloorBData, AlignBeamtoColumnLocationData);

            #region 下拉记忆按钮
            //PanelManage.AddSeparator();
            //SplitButtonData AlignBeamDocData = new SplitButtonData("梁参数调整", "梁参数调整");
            //SplitButton SplitAlignBeam = PanelManage.AddItem(AlignBeamDocData) as SplitButton;
            //// 添加下拉选择功能按键
            //SplitAlignBeam.AddPushButton(AlignBeamTtofloorFData);
            //SplitAlignBeam.AddPushButton(AlignBeamBtoFloorBData);
            //SplitAlignBeam.AddPushButton(AlignBeamtoColumnLocationData);

            //SplitAlignBeam.AddSeparator();
            //SplitAlignBeam.AddPushButton(AlignColumnTopWithFaceData);
            //SplitAlignBeam.AddPushButton(AlignColumnBaseWithFaceData);
            #endregion

            #endregion

            #region //////////////////////////////////////////////  -*-墙齐楼板-*-  //////////////////////////////////////////////////////
            var ButtonAlignWallToTop = new ButtonParamter
            {
                InName = "墙顶齐梁板",
                ButtonName = "墙顶齐梁板",
                NameSpace = "AlignWallToTop",
                Tooltip = "选择楼板或梁范围内需要对齐的墙（空格完成）\n再选择平楼板或梁\n自动修改墙顶部偏移参数使墙顶平平楼板底",
                StackedImageName = "S-004-16.png",
                ImageName = "S-004.png"
            };
            PushButtonData AlignWallToTopData = GetButtonData(ButtonAlignWallToTop);

            var ButtonAlignWallToBottom = new ButtonParamter
            {
                InName = "墙底齐梁板",
                ButtonName = "墙底齐梁板",
                NameSpace = "AlignWallToBottom",
                Tooltip = "选择楼板或梁范围内需要对齐的墙（空格完成）\n再选择平楼板或梁\n自动修改墙底部偏移参数使墙顶平平楼板面",
                StackedImageName = "S-005-16.png",
                ImageName = "S-005.png"
            };
            PushButtonData AlignWallToBottomData = GetButtonData(ButtonAlignWallToBottom);

            var ButtonAlignWallsToFloor = new ButtonParamter
            {
                InName = "板底墙对齐",
                ButtonName = "板底墙对齐",
                NameSpace = "AlignWallsToFloor",
                Tooltip = "自动获取到与楼板板相交的墙体(一定存在相交)\n自动修改墙顶部偏移参数使墙顶平平楼板底",
                StackedImageName = "S-006-16.png",
                ImageName = "S-006.png"
            };
            PushButtonData AlignWallsToFloorData = GetButtonData(ButtonAlignWallsToFloor);


            /////   -*-墙齐链接文件-*-    ////
            var ButtonAlignWallToTopLink = new ButtonParamter
            {
                InName = "墙顶齐链接梁板",
                ButtonName = "墙顶齐链接梁板",
                NameSpace = "AlignWallToTopLink",
                Tooltip = "选择楼板或梁范围内需要对齐的墙（空格完成）\n再选择平楼板或梁\n自动修改墙顶部偏移参数使墙顶平平楼板底\n\n-*-连接结构文档-*-",
                StackedImageName = "S-007-16.png",
                ImageName = "S-007.png"
            };
            PushButtonData AlignWallToTopLinkData = GetButtonData(ButtonAlignWallToTopLink);

            var ButtonAlignWallToBottomLink = new ButtonParamter
            {
                InName = "墙底齐链接梁板",
                ButtonName = "墙底齐链接梁板",
                NameSpace = "AlignWallToBottomLink",
                Tooltip = "选择楼板或梁范围内需要对齐的墙（空格完成）\n再选择平楼板或梁\n自动修改墙底部偏移参数使墙顶平平楼板面\n\n-*-连接结构文档-*-",
                StackedImageName = "S-008-16.png",
                ImageName = "S-008.png"
            };
            PushButtonData AlignWallToBottomLinkData = GetButtonData(ButtonAlignWallToBottomLink);

            var ButtonAlignWallsToFloorLink = new ButtonParamter
            {
                InName = "链接板底墙对齐",
                ButtonName = "链接板底墙对齐",
                NameSpace = "AlignWallsToFloorLink",
                Tooltip = "自动获取到与楼板板相交的墙体(一定存在相交)\n自动修改墙顶部偏移参数使墙顶平平楼板底\n\n-*-连接结构文档-*-",
                StackedImageName = "S-009-16.png",
                ImageName = "S-009.png"
            };
            PushButtonData AlignWallsToFloorLinkData = GetButtonData(ButtonAlignWallsToFloorLink);

            PanelAlign.AddSeparator();
            PanelAlign.AddStackedItems(AlignWallToTopData, AlignWallToBottomData, AlignWallsToFloorData);
            PanelAlign.AddStackedItems(AlignWallToTopLinkData, AlignWallToBottomLinkData, AlignWallsToFloorLinkData);
            #endregion

            #region //////////////////////////////////////////////  -*-创建模型-*-  //////////////////////////////////////////////////////
            /////   -*-批量创建标高-*-    ////    CreateGrids
            var ButtonCreatLevels = new ButtonParamter
            {
                InName = "创建标高",
                ButtonName = "创建\n标高",
                NameSpace = "CreateLevels",
                Tooltip = "依据给定的层高等参数自动创建创建标高\n并添加相应的视图到楼层平面",
                ImageName = "C-001.png",
                StackedImageName = "C-001-16.png"
            };
            PushButtonData CreatLevelsData = GetButtonData(ButtonCreatLevels);
            PanelCreate.AddItem(CreatLevelsData);

            /////   -*-批量创建轴网-*-    ////    CreateGrids
            var ButtonCreateGrids = new ButtonParamter
            {
                InName = "创建轴网",
                ButtonName = "创建\n轴网",
                NameSpace = "CreateGrids",
                Tooltip = "依据开间及进深及轴网编号自动创建轴网\n同时进行两道尺寸标注\n\n支持批量输入 间距*个数 \n建议中间使用 , 分隔",
                ImageName = "C-009.png",
                StackedImageName = "C-009-16.png"
            };
            PushButtonData CreateGridsData = GetButtonData(ButtonCreateGrids);
            PanelCreate.AddItem(CreateGridsData);

            /////   -*-快速剖面-*-    ////    CreateSection
            var ButtonCreateSection = new ButtonParamter
            {
                InName = "快速剖面",
                ButtonName = "快速\n剖面",
                NameSpace = "CreateSectionQuickLy",
                Tooltip = "点击构件自动基于构件定位线快速创建剖面",
                ImageName = "C-019.png",
                StackedImageName = "C-019-16.png"
            };
            PushButtonData CreateSectionData = GetButtonData(ButtonCreateSection);
            PanelCreate.AddItem(CreateSectionData);

            /////   -*-根据所选柱子顶部标高创建结构梁-*-    ////    CreateBeamWithColumns
            var ButtonCreateBeamWithColumns = new ButtonParamter
            {
                InName = "柱顶成梁",
                ButtonName = "柱顶\n成梁",
                NameSpace = "CreateBeamWithColumns",
                Tooltip = "根据所选结构柱顶部标高自动创建结构梁\n\n注意：结构柱顶部需要标高存在",
                ImageName = "C-006.png",
                StackedImageName = "C-006-16.png"
            };
            PushButtonData CreateBeamWithColumnsData = GetButtonData(ButtonCreateBeamWithColumns);
            PanelCreate.AddItem(CreateBeamWithColumnsData);

            /////   -*-结构墙梁柱区域自动创建楼板-*-    ////    AutoCreateFloor

            var ButtonCreateFloorWithPointSlide = new ButtonParamter
            {
                InName = "点击楼板",
                ButtonName = "点击\n楼板",
                NameSpace = "CreateFloorWithPointSlide",
                Tooltip = "设置楼板参数后\n单击创建楼板的位置\n即可自动创建楼板\n\n注意：功能仅支持在楼层平面使用",
                ImageName = "C-018.png",
                StackedImageName = "C-018-16.png"
            };
            PushButtonData CreateFloorWithPointSlideData = GetButtonData(ButtonCreateFloorWithPointSlide);
            PanelCreate.AddItem(CreateFloorWithPointSlideData);

            ////////////////////////////////  -*-机电功能-*-  /////////////////////////////////
            PanelCreate.AddSeparator();

            /////   -*-MEP标记-*-    ////    CreateMarkerMEP
            var ButtonCreateMarkerMEP = new ButtonParamter
            {
                InName = "MEP标记",
                ButtonName = "MEP\n标记",
                NameSpace = "CreateMarkerMEP",
                Tooltip = "选中需要标记的管线\n设置标记的参数\n点击标记的位置\n自动标记按类型族标记MEP完成出图\n\n可以修改族文件调整需要标记的参数",
                ImageName = "C-005.png",
                StackedImageName = "C-005-16.png"
            };
            PushButtonData CreateMarkerMEPData = GetButtonData(ButtonCreateMarkerMEP);
            PanelCreate.AddItem(CreateMarkerMEPData);

            /////   -*-水井管段-*-    ////    OutdoorPipe
            var ButtonOutdoorPipe = new ButtonParamter
            {
                InName = "水井管段",
                ButtonName = "水井\n管段",
                NameSpace = "OutdoorPipe",
                Tooltip = "依次选择两个水井族实例自动创建管段\n\n注意：水井族需要使用指定的族文件创建的实例",
                ImageName = "C-003.png",
                StackedImageName = "C-003-16.png"
            };
            PushButtonData OutdoorPipeData = GetButtonData(ButtonOutdoorPipe);
            PanelCreate.AddItem(OutdoorPipeData);

            /////   -*-布置套管-*-    ////    CreatPipeAccessory
            var ButtonCreatPipeAccessory = new ButtonParamter
            {
                InName = "布置套管",
                ButtonName = "布置\n套管",
                NameSpace = "CreatePipeAccessory",
                Tooltip = "在当前土建模型当中选择链接的MEP模型\n设置是否按楼层布置\n\n自动布置套管并提示布置情况",
                ImageName = "C-002.png",
                StackedImageName = "C-002-16.png"
            };
            PushButtonData CreatPipeAccessoryData = GetButtonData(ButtonCreatPipeAccessory);
            PanelCreate.AddItem(CreatPipeAccessoryData);

            /////   -*-风管洞口-*-    ////    CreatDuctFitting
            var ButtonCreatDuctFitting = new ButtonParamter
            {
                InName = "风管洞口",
                ButtonName = "风管\n洞口",
                NameSpace = "CreateDuctFitting",
                Tooltip = "在当前土建模型当中选择链接的MEP模型\n设置是否按楼层布置\n自动布置洞口并提示布置情况",
                ImageName = "C-004.png",
                StackedImageName = "C-004-16.png"
            };
            PushButtonData CreatDuctFittingData = GetButtonData(ButtonCreatDuctFitting);
            PanelCreate.AddItem(CreatDuctFittingData);

            /////   -*-一键剖面-*-    ////    CreateElementSection
            var ButtonCreateElementSection = new ButtonParamter
            {
                InName = "构件剖面",
                ButtonName = "构件\n剖面",
                NameSpace = "CreateElementSection",
                Tooltip = "首先点击基于线的模型\n然后在点击剖面的朝向\n\n自动创建剖面视图并自动跳转到创建的剖面\n可以在全局设置中设置剖面的名称、偏移值及深度等默认参数\n默认高度参数为层高",
                ImageName = "C-017.png",
                StackedImageName = "C-017-16.png"
            };
            PushButtonData CreateElementSectionData = GetButtonData(ButtonCreateElementSection);
            PanelCreate.AddItem(CreateElementSectionData);
            #endregion

            #region //////////////////////////////////////////////  -*-CAD 转换-*-  //////////////////////////////////////////////////////
            /////   -*-依据CAD图纸创建矩形结构柱-*-    ////    CreateColumnBH
            var ButtonCreateColumnBH = new ButtonParamter
            {
                InName = "矩形结构柱",
                ButtonName = "矩形\n结构柱",
                NameSpace = "CreateColumnBH",
                Tooltip = "依次选择CAD图纸矩形柱轮廓及柱编号文字创建矩形结构柱\n\n柱子轮廓不要成组否则不能准确定位",
                ImageName = "C-013.png",
                StackedImageName = "C-013-16.png"
            };
            PushButtonData CreateColumnBHData = GetButtonData(ButtonCreateColumnBH);
            PanelCAD.AddItem(CreateColumnBHData);

            /////   -*-依据CAD图纸创建异形结构柱-*-    ////    CreateXColumnsCAD
            var ButtonCreateXColumnsCAD = new ButtonParamter
            {
                InName = "异形结构柱",
                ButtonName = "异形\n结构柱",
                NameSpace = "CreateXColumnsCAD",
                Tooltip = "依次选择CAD图纸轮廓及柱编号文字创建异形结构柱\n柱类型命名规则按文件名+标高+名称\n\n柱子轮廓不要成组否则不能准确定位",
                ImageName = "C-007.png",
                StackedImageName = "C-007-16.png"
            };
            PushButtonData CreateXColumnsCADData = GetButtonData(ButtonCreateXColumnsCAD);
            PanelCAD.AddItem(CreateXColumnsCADData);

            /////   -*-依据CAD图纸创建结构梁-*-    ////    CreateBeamCAD CreateBeamsCAD CreateBeamsOffsetCAD CreateBeamsReadParaCAD
            var ButtonCreateBeamCAD = new ButtonParamter
            {
                InName = "定位线标注转梁",
                ButtonName = "定位线标注转梁",
                NameSpace = "CreateBeamCAD",
                Tooltip = "首先选中CAD图纸梁定位中线\n再次选择结构梁对应的图纸标注信息\n原位标注的类型默认是上一次<集中标注定位线转梁>的类型\n\n样式：\n----KL1(3) 300X600",
                ImageName = "C-012.png",
                StackedImageName = "C-012-16.png"
            };
            PushButtonData CreateBeamCADData = GetButtonData(ButtonCreateBeamCAD);

            // 编号 样式：KL1(3)\n 300X600
            var ButtonCreateBeamsCAD = new ButtonParamter
            {
                NameSpace = "CreateBeamsCAD",
                InName = "集中标注定位线转梁",
                ButtonName = "集中标注定位线转梁",
                Tooltip = "依据先选中CAD图纸梁集中标注名称及尺寸\n在选择梁定位边线\n默认对齐方式为左对齐\n\n样式：\n----KL1(3)\n----300X600",
                ImageName = "C-012.png",
                StackedImageName = "C-012-16.png"
            };
            PushButtonData CreateBeamsCADData = GetButtonData(ButtonCreateBeamsCAD);

            // 编号 样式：KL1(3)\n 300X600\n (Hg+0.100)
            var ButtonCreateBeamsOffsetCAD = new ButtonParamter
            {
                NameSpace = "CreateBeamsOffsetCAD",
                InName = "标注定位线偏移值转梁",
                ButtonName = "标注定位线偏移值转梁",
                Tooltip = "依据先选中CAD图纸梁集中标注名称及尺寸\n在选择梁定位边线\n在选择梁偏移值(需要含有正负号)\n默认对齐方式为左对齐\n\n样式：\n----KL1(3)\n----300X600\n----Hg+0.100",
                ImageName = "C-012.png",
                StackedImageName = "C-012-16.png"
            };
            PushButtonData CreateBeamsOffsetCADData = GetButtonData(ButtonCreateBeamsOffsetCAD);

            var ButtonCreateBeamsReadParaCAD = new ButtonParamter
            {
                NameSpace = "CreateBeamsReadParaCAD",
                InName = "定位线转梁",
                ButtonName = "定位线转梁",
                Tooltip = "选择梁定位边线\n默认对齐方式为左对齐\n\n-*- 依据上一次<集中标注定位线转梁>的参数数据 -*-",
                ImageName = "C-012.png",
                StackedImageName = "C-012-16.png"
            };
            PushButtonData CreateBeamsReadParaCADData = GetButtonData(ButtonCreateBeamsReadParaCAD);

            // 创建下拉按钮
            var CreateBeamCAD = new ButtonParamter
            {
                InName = "矩形结构梁",
                ButtonName = "矩形\n结构梁",
                Tooltip = "依据CAD图纸标注创建梁",
                ImageName = "C-012.png",
                StackedImageName = "C-012-16.png"
            };

            var pullCreateBeamCAD = GetPulldownButtonData(CreateBeamCAD);
            var SplitButtonCreateBeamCAD = PanelCAD.AddItem(pullCreateBeamCAD) as PulldownButton;
            SplitButtonCreateBeamCAD.AddPushButton(CreateBeamCADData);
            SplitButtonCreateBeamCAD.AddPushButton(CreateBeamsCADData);
            SplitButtonCreateBeamCAD.AddPushButton(CreateBeamsOffsetCADData);
            SplitButtonCreateBeamCAD.AddPushButton(CreateBeamsReadParaCADData);

            ///////   -*-依据CAD图纸创建楼板-*-    ////    CreateFloorCAD
            //var ButtonCreateFloorCAD = new ButtonParamter
            //{
            //    InName = "区域楼板",
            //    ButtonName = "区域\n楼板",
            //    NameSpace = "CreateFloorCAD",
            //    Tooltip = "依据CAD图纸填充的轮廓创建楼板",
            //    ImageName = "C-008.png",
            //    StackedImageName = "C-008-16.png"
            //};
            //PushButtonData CreateFloorCADData = GetButtonData(ButtonCreateFloorCAD);
            //PanelCAD.AddItem(CreateFloorCADData);

            ///////   -*-依据CAD图纸创建矩形基础-*-    // CreateFoundation
            //var ButtonCreateFoundation = new ButtonParamter
            //{
            //    InName = "矩形基础",
            //    ButtonName = "矩形\n基础",
            //    NameSpace = "CreateFoundation",
            //    Tooltip = "依据CAD图纸信息自动创建矩形基础\n  选择矩形基础定位边线\n  选择矩形基础编号名称\n  厚度参数默认：1000（data22.json）",
            //    ImageName = "C-014.png",
            //    StackedImageName = "C-014-16.png"
            //};
            //PushButtonData CreateFoundationData = GetButtonData(ButtonCreateFoundation);
            //PanelCAD.AddItem(CreateFoundationData);

            ///////   -*-依据CAD图纸创建矩形柱帽-*-    // CreateConcreteCap
            //var ButtonCreateConcreteCap = new ButtonParamter
            //{
            //    InName = "矩形柱帽",
            //    ButtonName = "矩形柱帽",
            //    NameSpace = "CreateConcreteCap",
            //    Tooltip = "依据CAD图纸信息柱帽名称自动创建矩形柱帽\n  选择结构柱\n  选择矩形柱帽编号名称",
            //    ImageName = "C-015.png",
            //    StackedImageName = "C-015-16.png"
            //};
            //PushButtonData CreateConcreteCapData = GetButtonData(ButtonCreateConcreteCap);

            ///////   -*-柱顶自动创建柱帽类型同识别图纸时创建的类型-*-    //CreateConcreteCaps
            //var ButtonCreateConcreteCaps = new ButtonParamter
            //{
            //    InName = "柱顶柱帽",
            //    ButtonName = "柱顶柱帽",
            //    NameSpace = "CreateConcreteCaps",
            //    Tooltip = "依据上一次创建的柱帽类型\n选择结构柱自动创建柱帽\n\n注意：仅支持在平面视图中创建",
            //    ImageName = "C-015.png",
            //    StackedImageName = "C-015-16.png"
            //};
            //PushButtonData CreateConcreteCapsData = GetButtonData(ButtonCreateConcreteCaps);

            ////PanelCAD.AddItem(CreateConcreteCapsData);
            //var CreateConcreteCaps = new ButtonParamter
            //{
            //    InName = "结构柱帽",
            //    ButtonName = "结构\n柱帽",
            //    Tooltip = "依据CAD图纸图纸类型读取本地的json数据类型自动创建柱帽",
            //    ImageName = "C-015.png",
            //    StackedImageName = "C-015-16.png"
            //};
            //var pullCreateConcreteCaps = GetPulldownButtonData(CreateConcreteCaps);
            //var PulldownButtonConcreteCaps = PanelCAD.AddItem(pullCreateConcreteCaps) as PulldownButton;
            //PulldownButtonConcreteCaps.AddPushButton(CreateConcreteCapData);
            //PulldownButtonConcreteCaps.AddPushButton(CreateConcreteCapsData);

            ///////   -*-柱顶自动创建柱帽类型同识别图纸时创建的类型-*-   /////  UpdateSymbolName
            //PanelCAD.AddSeparator();
            //var ButtonUpdateSymbolName = new ButtonParamter
            //{
            //    InName = "更新类型",
            //    ButtonName = "更新\n类型",
            //    NameSpace = "UpdateSymbolName",
            //    Tooltip = "依据CAD图纸更新类型名称\n若类型不存在自动创建新的类型替换",
            //    ImageName = "C-016.png",
            //    StackedImageName = "C-016-16.png"
            //};
            //PushButtonData UpdateSymbolNameData = GetButtonData(ButtonUpdateSymbolName);
            //PanelCAD.AddItem(UpdateSymbolNameData);
            #endregion

            #region //////////////////////////////////////////////  -*-模型审查-*-  //////////////////////////////////////////////////////
            /////   -*-模型审查-OK-*-    ////    ColorSplasherOk
            var ButtonColorSplasherOk = new ButtonParamter
            {
                InName = "审查模型",
                ButtonName = "审查模型",
                NameSpace = "ColorSplasherOk",
                Tooltip = "设置当前已选择的图元在当前视口的颜色为\n注释：Yes",
                ImageName = "D-001.png",
                StackedImageName = "D-001-16.png"
            };
            PushButtonData ColorSplasherOkData = GetButtonData(ButtonColorSplasherOk);

            /////   -*-模型审查-NO-*-    ////    ColorSplasherNo
            var ButtonColorSplasherNo = new ButtonParamter
            {
                InName = "问题模型",
                ButtonName = "问题模型",
                NameSpace = "ColorSplasherNo",
                Tooltip = "设置当前已选择的图元在当前视口的颜色\n注释：No",
                ImageName = "D-002.png",
                StackedImageName = "D-002-16.png"
            };
            PushButtonData ColorSplasherNoData = GetButtonData(ButtonColorSplasherNo);

            /////   -*-审查视图-3D-*-    ////    Create3DFilterRules
            var ButtonCreate3DFilterRules = new ButtonParamter
            {
                InName = "审查视图",
                ButtonName = "审查视图",
                NameSpace = "Create3DFilterRules",
                Tooltip = "创建3D审查视图\n查看已经审查过后的模型且添加过滤器进行展示",
                ImageName = "D-003.png",
                StackedImageName = "D-003-16.png"
            };
            PushButtonData Create3DFilterRulesData = GetButtonData(ButtonCreate3DFilterRules);

            /////   -*-当前视图创建审查过滤器 -*-    ////    ColorFilterThisView
            var ButtonColorFilterThisView = new ButtonParamter
            {
                InName = "审查过滤",
                ButtonName = "审查过滤",
                NameSpace = "ColorFilterThisView",
                Tooltip = "一键向当前激活的视图添加的审查过滤器\n<模型审查-OK><模型过滤-NO>",
                ImageName = "D-004.png",
                StackedImageName = "D-004-16.png"
            };
            PushButtonData ColorFilterThisViewData = GetButtonData(ButtonColorFilterThisView);

            /////   -*-当前视图删除审查过滤器-*-    ////    ColorRemoveFilter
            var ButtonColorRemoveFilter = new ButtonParamter
            {
                InName = "删除过滤",
                ButtonName = "删除过滤",
                NameSpace = "ColorRemoveFilter",
                Tooltip = "一键删除当前视图已添加的审查过滤器\n<模型审查-OK><模型过滤-NO>",
                ImageName = "D-005.png",
                StackedImageName = "D-005-16.png"
            };
            PushButtonData ColorRemoveFilterData = GetButtonData(ButtonColorRemoveFilter);

            /////   -*-删除当前视图图元替换-*-    ////    DelOverrideInType
            var ButtonDelOverrideInType = new ButtonParamter
            {
                InName = "替换恢复",
                ButtonName = "替换恢复",
                NameSpace = "DelOverrideInType",
                Tooltip = "恢复当前图元的替换颜色为默认显示\n删除审查的颜色",
                ImageName = "A-004.png",
                StackedImageName = "A-004-16.png"
            };
            PushButtonData DelOverrideInTypeData = GetButtonData(ButtonDelOverrideInType);

            PanelModel.AddStackedItems(ColorSplasherOkData, ColorSplasherNoData, Create3DFilterRulesData);
            PanelModel.AddStackedItems(ColorFilterThisViewData, ColorRemoveFilterData, DelOverrideInTypeData);

            #endregion

            #region //////////////////////////////////////////////  -*-按键位置-*-  //////////////////////////////////////////////////////
            // 移动面板到前方
            Autodesk.Windows.RibbonControl ribbon = Autodesk.Windows.ComponentManager.Ribbon;
            Autodesk.Windows.RibbonTab myTab = null;
            var alltabs = ribbon.Tabs;

            foreach (Autodesk.Windows.RibbonTab tab in alltabs)
            {
                if (tab.Name == tabName)
                {
                    myTab = tab;
                    ribbon.Tabs.Remove(myTab);
                    break;
                }
            }
            // 附加模块 前面
            ribbon.Tabs.Insert(12, myTab);
            #endregion

            return Result.Succeeded;
        }

        /// <summary>
        /// 创建按键模板
        /// </summary>
        /// <param name="ButtonParamter"></param>
        /// <returns></returns>
        public PushButtonData GetButtonData(ButtonParamter ButtonParamter)
        #region 创建按键Data-PushButtonData
        {
            ButtonParamter paramter = new ButtonParamter();
            // 按键集合 - 系统名称，面板名称，dll文件位置，命名空间引用
            PushButtonData ButtonData = new PushButtonData(ButtonParamter.InName, ButtonParamter.ButtonName, ButtonParamter.AssemblyName, ButtonParamter.NameSpace)
            {
                // 提示信息
                ToolTip = ButtonParamter.Tooltip
            };
            // F1 帮助地址
            ContextualHelp contextHelp = new ContextualHelp(ContextualHelpType.Url, "https://mp.weixin.qq.com/s/SIpuJlNNtofjR6OrtTxNeQ");
            ButtonData.SetContextualHelp(contextHelp);
            // 长按提示
            ButtonData.LongDescription = "使用问题添加下方QQ群进行反馈！";  //Long description of the command tooltip
            try
            {
                // 设置图标 32*32
                string imageName = string.Format("\\Resources\\{0}", ButtonParamter.ImageName);
                string imagePath = Path.GetDirectoryName(ButtonParamter.AssemblyName) + imageName;
                Uri uri = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                ButtonData.LargeImage = new BitmapImage(uri); // 大图
            }
            catch (Exception e)
            {
                TaskDialog.Show("error-LargeImage", "未找到图片ImageName\n" + e.Message.Split('\\').Last());
                string imageName = string.Format("\\Resources\\{0}", paramter.ImageName);
                string imagePath = Path.GetDirectoryName(ButtonParamter.AssemblyName) + imageName;
                Uri uri = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                ButtonData.LargeImage = new BitmapImage(uri); // 大图
            }
            try
            {
                // 设置图标 16*16
                string imageName = string.Format("\\Resources\\{0}", ButtonParamter.StackedImageName);
                string imagePath = Path.GetDirectoryName(ButtonParamter.AssemblyName) + imageName;
                Uri uri = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                ButtonData.Image = new BitmapImage(uri); // 小图
            }
            catch (Exception e)
            {
                TaskDialog.Show("error-LargeImage", "未找到图片StackedImageName\n" + e.Message.Split('\\').Last());
                string imageName = string.Format("\\Resources\\{0}", paramter.StackedImageName);
                string imagePath = Path.GetDirectoryName(ButtonParamter.AssemblyName) + imageName;
                Uri uri = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                ButtonData.LargeImage = new BitmapImage(uri); // 大图
            }

            #region 提示图片显示
            //try
            //{
            //    // 设置图标 32*32
            //    string imageName = string.Format("\\Resources\\{0}", ButtonParamter.TooltipImage);
            //    string imagePath = Path.GetDirectoryName(ButtonParamter.AssemblyName) + imageName;
            //    Uri uri = new Uri(imagePath, UriKind.RelativeOrAbsolute);
            //    ButtonData.ToolTipImage = new BitmapImage(uri); // 动图
            //}
            //catch (Exception e)
            //{
            //    TaskDialog.Show("error-LargeImage", "未找到图片TooltipImage\n" + e.Message.Split('\\').Last());
            //}
            #endregion

            return ButtonData;
        }
        #endregion

        /// <summary>
        /// 创建下拉按钮
        /// </summary>
        /// <param name="ButtonParamter"></param>
        /// <returns></returns>
        public PulldownButtonData GetPulldownButtonData(ButtonParamter ButtonParamter)
        #region 创建按键Data-PulldownButtonData
        {
            ButtonParamter paramter = new ButtonParamter();
            #region 按键基本帮助提示信息
            // 按键集合 - 系统名称，面板名称
            PulldownButtonData ButtonData = new PulldownButtonData(ButtonParamter.InName, ButtonParamter.ButtonName)
            {
                // 提示信息
                ToolTip = ButtonParamter.Tooltip
            };
            // F1 帮助地址
            ContextualHelp contextHelp = new ContextualHelp(ContextualHelpType.Url, "https://mp.weixin.qq.com/s/SIpuJlNNtofjR6OrtTxNeQ");
            ButtonData.SetContextualHelp(contextHelp);
            // 长按提示  Long description of the command tooltip
            ButtonData.LongDescription = "注册：关注公众号：好用不火\n回复：注册插件 用户名 \n\n使用过程中的问题添加下方QQ群进行反馈！";
            #endregion

            try
            {
                // 设置图标 32*32
                string imageName = string.Format("\\Resources\\{0}", ButtonParamter.ImageName);
                string imagePath = Path.GetDirectoryName(ButtonParamter.AssemblyName) + imageName;
                Uri uri = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                ButtonData.LargeImage = new BitmapImage(uri); // 大图
            }
            catch (Exception e)
            {
                TaskDialog.Show("error-LargeImage", "未找到图片 ImageName\n" + e.Message.Split('\\').Last());
                string imageName = string.Format("\\Resources\\{0}", paramter.ImageName);
                string imagePath = Path.GetDirectoryName(ButtonParamter.AssemblyName) + imageName;
                Uri uri = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                ButtonData.LargeImage = new BitmapImage(uri);
            }
            try
            {
                // 设置图标 16*16
                string imageName = string.Format("\\Resources\\{0}", ButtonParamter.StackedImageName);
                string imagePath = Path.GetDirectoryName(ButtonParamter.AssemblyName) + imageName;
                Uri uri = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                ButtonData.Image = new BitmapImage(uri); // 小图
            }
            catch (Exception e)
            {
                TaskDialog.Show("error-Image", "未找到图片\n" + e.Message);
                TaskDialog.Show("error-LargeImage", "未找到图片 StackedImageName\n" + e.Message.Split('\\').Last());
                string imageName = string.Format("\\Resources\\{0}", paramter.StackedImageName);
                string imagePath = Path.GetDirectoryName(ButtonParamter.AssemblyName) + imageName;
                Uri uri = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                ButtonData.LargeImage = new BitmapImage(uri);
            }
            try
            {
                // 设置图标 32*32
                string imageName = string.Format("\\Resources\\{0}", ButtonParamter.TooltipImage);
                string imagePath = Path.GetDirectoryName(ButtonParamter.AssemblyName) + imageName;
                Uri uri = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                ButtonData.ToolTipImage = new BitmapImage(uri); // 动图
            }
            catch (Exception e)
            {
                TaskDialog.Show("error-ToolTipImage", "未找到图片 TooltipImage\n" + e.Message);
            }

            return ButtonData;
        }
        #endregion

        /// <summary>
        /// 创建按键模板
        /// </summary>
        /// <param name="ButtonParamter"></param>
        /// <returns></returns>
        public ToggleButtonData GetToggleButtonData(ButtonParamter ButtonParamter)
        #region 创建按键Data-ToggleButtonData
        {
            ButtonParamter paramter = new ButtonParamter();
            // 按键集合 - 系统名称，面板名称，dll文件位置，命名空间引用
            ToggleButtonData ButtonData = new ToggleButtonData(ButtonParamter.InName, ButtonParamter.ButtonName, ButtonParamter.AssemblyName, ButtonParamter.NameSpace)
            {
                // 提示信息
                ToolTip = ButtonParamter.Tooltip
            };
            // F1 帮助地址
            ContextualHelp contextHelp = new ContextualHelp(ContextualHelpType.Url, "https://mp.weixin.qq.com/s/SIpuJlNNtofjR6OrtTxNeQ");
            ButtonData.SetContextualHelp(contextHelp);
            // 长按提示
            ButtonData.LongDescription = "使用问题添加下方QQ群进行反馈！";  //Long description of the command tooltip
            try
            {
                // 设置图标 32*32
                string imageName = string.Format("\\Resources\\{0}", ButtonParamter.ImageName);
                string imagePath = Path.GetDirectoryName(ButtonParamter.AssemblyName) + imageName;
                Uri uri = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                ButtonData.LargeImage = new BitmapImage(uri); // 大图
            }
            catch (Exception e)
            {
                TaskDialog.Show("error-LargeImage", "未找到图片 ImageName\n" + e.Message.Split('\\').Last());
                string imageName = string.Format("\\Resources\\{0}", paramter.ImageName);
                string imagePath = Path.GetDirectoryName(ButtonParamter.AssemblyName) + imageName;
                Uri uri = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                ButtonData.LargeImage = new BitmapImage(uri);
            }
            try
            {
                // 设置图标 16*16
                string imageName = string.Format("\\Resources\\{0}", ButtonParamter.StackedImageName);
                string imagePath = Path.GetDirectoryName(ButtonParamter.AssemblyName) + imageName;
                Uri uri = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                ButtonData.Image = new BitmapImage(uri); // 小图
            }
            catch (Exception e)
            {
                TaskDialog.Show("error-LargeImage", "未找到图片 StackedImageName\n" + e.Message.Split('\\').Last());
                string imageName = string.Format("\\Resources\\{0}", paramter.StackedImageName);
                string imagePath = Path.GetDirectoryName(ButtonParamter.AssemblyName) + imageName;
                Uri uri = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                ButtonData.LargeImage = new BitmapImage(uri);
            }
            try
            {
                // 设置图标 32*32
                string imageName = string.Format("\\Resources\\{0}", ButtonParamter.TooltipImage);
                string imagePath = Path.GetDirectoryName(ButtonParamter.AssemblyName) + imageName;
                Uri uri = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                ButtonData.ToolTipImage = new BitmapImage(uri); // 动图
            }
            catch (Exception e)
            {
                TaskDialog.Show("error-ToolTipImage", "未找到图片 TooltipImage\n" + e.Message);
            }

            return ButtonData;
        }
        #endregion
    }

    /// <summary>
    /// 定义按键参数 
    /// ---------------------------------------------
    /// string inName           系统名称      内部名称
    /// string buttonName       按键名称      \n换行
    /// string assemblyName     dll文件地址   完成地址
    /// string nameSpace        命名空间引用  .方法
    /// string Tooltip          提示文字      方法解释
    /// string TooltipImage     提示图片      .png
    /// string imageName        按钮图片      32*32
    /// string StackedImageName 按钮小图      16*16
    /// ---------------------------------------------
    /// </summary>
    public class ButtonParamter
    {
        public string InName
        {
            get { return inName; }
            set { inName = value; }
        }
        private string inName = "测试安装";

        public string ButtonName
        {
            get { return buttonName; }
            set { buttonName = value; }
        }
        private string buttonName = "测试\n安装";
        public string NameSpace
        {
            get { return nameSpace; }
            set
            {
                if (value.Contains("."))
                {
                    nameSpace = value;
                }
                else
                {
                    nameSpace = Strings.hybhSpace + value;
                }
            }
        }
        private string nameSpace = Strings.hybhSpace + "Text";

        public string AssemblyName
        {
            get { return assemblyName; }
            set { assemblyName = value; }
        }
        // 设置引用文件的路径
        private string assemblyName = Assembly.GetExecutingAssembly().Location;

        public string TooltipImage
        {
            get { return tooltipImage; }
            set { tooltipImage = value; }
        }
        private string tooltipImage = "QQ.png";


        public string Tooltip
        {
            get { return tooltip; }
            set { tooltip = value; }
        }
        private string tooltip = "--测试功能 \n--软件安装成功 \n--弹出提示窗口";

        public string ImageName
        {
            get { return imageName; }
            set { imageName = value; }
        }
        private string imageName = "debug.png";

        public string StackedImageName
        {
            get { return stackedImageName; }
            set { stackedImageName = value; }
        }
        private string stackedImageName = "debug-16.png";
    }
}
