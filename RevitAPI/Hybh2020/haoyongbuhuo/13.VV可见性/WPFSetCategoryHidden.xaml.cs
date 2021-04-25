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
using MongoDB.Driver.Core.Operations;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    /// <summary>
    /// WPFvvPlus.xaml 的交互逻辑
    /// </summary>
    public partial class WPFSetCategoryHidden : Window
    {
        // 建筑-list
        List<BuiltInCategory> BuiltInCategories_AS = new List<BuiltInCategory> {
            BuiltInCategory.OST_Walls,
            BuiltInCategory.OST_Floors,
            BuiltInCategory.OST_StructuralColumns,
            BuiltInCategory.OST_StructuralFraming,
            BuiltInCategory.OST_StructuralFoundation,
            BuiltInCategory.OST_Rebar,
            BuiltInCategory.OST_GenericModel,
            BuiltInCategory.OST_Doors,
            BuiltInCategory.OST_Windows,
            BuiltInCategory.OST_StairsRailing,
            BuiltInCategory.OST_Stairs,
            BuiltInCategory.OST_Roofs,
            BuiltInCategory.OST_Ramps,
            BuiltInCategory.OST_Mass,
            BuiltInCategory.OST_Topography,
            BuiltInCategory.OST_Planting,
            BuiltInCategory.OST_CurtainWallPanels,
            BuiltInCategory.OST_CurtainWallMullions,
            BuiltInCategory.OST_CurtaSystem,
            };
        // 机电-list
        List<BuiltInCategory> BuiltInCategories_MEP = new List<BuiltInCategory> {
            BuiltInCategory.OST_PipeCurves,
            BuiltInCategory.OST_DuctCurves,
            BuiltInCategory.OST_CableTray,
            BuiltInCategory.OST_Conduit,
            BuiltInCategory.OST_PipeFitting,
            BuiltInCategory.OST_DuctFitting,
            BuiltInCategory.OST_CableTrayFitting,
            BuiltInCategory.OST_MechanicalEquipment,
            BuiltInCategory.OST_PlumbingFixtures,
            BuiltInCategory.OST_Sprinklers,
            BuiltInCategory.OST_ElectricalEquipment,
            BuiltInCategory.OST_LightingFixtures,
            BuiltInCategory.OST_PipeAccessory,
            BuiltInCategory.OST_DuctAccessory,
            BuiltInCategory.OST_ConduitFitting,
            BuiltInCategory.OST_FlexPipeCurves,
            BuiltInCategory.OST_FlexDuctCurves,
            BuiltInCategory.OST_DuctTerminal,
            BuiltInCategory.OST_Wire,
            };
        // 注释-list
        List<BuiltInCategory> BuiltInCategories_TAG = new List<BuiltInCategory> {
            BuiltInCategory.OST_Levels,
            BuiltInCategory.OST_Grids,
            BuiltInCategory.OST_Sections,
            BuiltInCategory.OST_CLines,
            BuiltInCategory.OST_TextNotes,
            BuiltInCategory.OST_SpotSlopes,
            BuiltInCategory.OST_SpotElevations,
            BuiltInCategory.OST_Dimensions,
            BuiltInCategory.OST_RevisionClouds,
            BuiltInCategory.OST_DoorTags,
            BuiltInCategory.OST_WindowTags,
            BuiltInCategory.OST_FloorTags,
            BuiltInCategory.OST_CurtainWallPanelTags,
            BuiltInCategory.OST_PipeTags,
            BuiltInCategory.OST_DuctTags,
            BuiltInCategory.OST_CableTrayTags,
            BuiltInCategory.OST_ConduitTags,
            BuiltInCategory.OST_MaterialTags,
            BuiltInCategory.OST_AreaTags,
            };

        // 建筑
        ExternalEvent hander_OST_Wall = null;
        ExternalEvent hander_OST_Floor = null;
        ExternalEvent hander_OST_StructuralColumns = null;
        ExternalEvent hander_OST_StructuralFraming = null;
        ExternalEvent hander_OST_StructuralFoundation = null;
        ExternalEvent hander_OST_Rebar = null;
        ExternalEvent hander_OST_GenericModel = null;
        ExternalEvent hander_OST_Doors = null;
        ExternalEvent hander_OST_Windows = null;
        ExternalEvent hander_OST_StairsRailing = null;
        ExternalEvent hander_OST_Stairs = null;
        ExternalEvent hander_OST_Roofs = null;
        ExternalEvent hander_OST_Ramps = null;
        ExternalEvent hander_OST_Mass = null;
        ExternalEvent hander_OST_Topography = null;
        ExternalEvent hander_OST_Planting = null;
        ExternalEvent hander_OST_CurtainWallPanels = null;
        ExternalEvent hander_OST_CurtainWallMullions = null;
        ExternalEvent hander_OST_CurtaSystem = null;

        // 机电
        ExternalEvent hander_OST_PipeCurves = null;
        ExternalEvent hander_OST_DuctCurves = null;
        ExternalEvent hander_OST_CableTray = null;
        ExternalEvent hander_OST_Conduit = null;
        ExternalEvent hander_OST_PipeFitting = null;
        ExternalEvent hander_OST_DuctFitting = null;
        ExternalEvent hander_OST_CableTrayFitting = null;
        ExternalEvent hander_OST_MechanicalEquipment = null;
        ExternalEvent hander_OST_PlumbingFixtures = null;
        ExternalEvent hander_OST_Sprinklers = null;
        ExternalEvent hander_OST_ElectricalEquipment = null;
        ExternalEvent hander_OST_LightingFixtures = null;
        ExternalEvent hander_OST_PipeAccessory = null;
        ExternalEvent hander_OST_DuctAccessory = null;
        ExternalEvent hander_OST_ConduitFitting = null;
        ExternalEvent hander_OST_FlexPipeCurves = null;
        ExternalEvent hander_OST_FlexDuctCurves = null;
        ExternalEvent hander_OST_DuctTerminal = null;
        ExternalEvent hander_OST_Wire = null;


        // 注释
        ExternalEvent hander_OST_Levels = null;
        ExternalEvent hander_OST_Grids = null;
        ExternalEvent hander_OST_Sections = null;
        ExternalEvent hander_OST_CLines = null;
        ExternalEvent hander_OST_TextNotes = null;
        ExternalEvent hander_OST_SpotSlopes = null;
        ExternalEvent hander_OST_SpotElevations = null;
        ExternalEvent hander_OST_Dimensions = null;
        ExternalEvent hander_OST_RevisionClouds = null;
        ExternalEvent hander_OST_DoorTags = null;
        ExternalEvent hander_OST_WindowTags = null;
        ExternalEvent hander_OST_FloorTags = null;
        ExternalEvent hander_OST_CurtainWallPanelTags = null;
        ExternalEvent hander_OST_PipeTags = null;
        ExternalEvent hander_OST_DuctTags = null;
        ExternalEvent hander_OST_CableTrayTags = null;
        ExternalEvent hander_OST_ConduitTags = null;
        ExternalEvent hander_OST_MaterialTags = null;
        ExternalEvent hander_OST_AreaTags = null;

        // all
        ExternalEvent hander_ShowAll_AS = null;
        ExternalEvent hander_ShowAll_MEP = null;
        ExternalEvent hander_ShowAll_TAG = null;
        ExternalEvent hander_ShowNothing_AS = null;
        ExternalEvent hander_ShowNothing_MEP = null;
        ExternalEvent hander_ShowNothing_TAG = null;


        public WPFSetCategoryHidden(ExternalCommandData commandData)
        {
            InitializeComponent();
            // 设置全部类别可见性- 建筑
            CategoryHiddenAll categoryHiddenAll_AS = new CategoryHiddenAll
            {
                ShowAll = false,
                builtInCategories = BuiltInCategories_AS
            };
            hander_ShowAll_AS = ExternalEvent.Create(categoryHiddenAll_AS);

            CategoryHiddenAll categoryHiddenAll_MEP = new CategoryHiddenAll();
            categoryHiddenAll_MEP.ShowAll = false;
            categoryHiddenAll_MEP.builtInCategories = BuiltInCategories_MEP;
            hander_ShowAll_MEP = ExternalEvent.Create(categoryHiddenAll_MEP);

            CategoryHiddenAll categoryHiddenAll_TAG = new CategoryHiddenAll();
            categoryHiddenAll_TAG.ShowAll = false;
            categoryHiddenAll_TAG.builtInCategories = BuiltInCategories_TAG;
            hander_ShowAll_TAG = ExternalEvent.Create(categoryHiddenAll_TAG);

            // 设置全部类别可见性- 建筑
            CategoryHiddenAll categoryHiddenNothing_AS = new CategoryHiddenAll();
            categoryHiddenNothing_AS.ShowAll = true;
            categoryHiddenNothing_AS.builtInCategories = BuiltInCategories_AS;
            hander_ShowNothing_AS = ExternalEvent.Create(categoryHiddenNothing_AS);
            // 设置全部类别可见性- 机电
            CategoryHiddenAll categoryHiddenNothing_MEP = new CategoryHiddenAll();
            categoryHiddenNothing_MEP.ShowAll = true;
            categoryHiddenNothing_MEP.builtInCategories = BuiltInCategories_MEP;
            hander_ShowNothing_MEP = ExternalEvent.Create(categoryHiddenNothing_MEP);

            // 设置全部类别可见性- 注释
            CategoryHiddenAll categoryHiddenNothing_TAG = new CategoryHiddenAll();
            categoryHiddenNothing_TAG.ShowAll = true;
            categoryHiddenNothing_TAG.builtInCategories = BuiltInCategories_TAG;
            hander_ShowNothing_TAG = ExternalEvent.Create(categoryHiddenNothing_TAG);


            // 设置 墙 可见性
            CategoryHidden Wall = new CategoryHidden();
            Wall.Category = BuiltInCategory.OST_Walls;
            hander_OST_Wall = ExternalEvent.Create(Wall);

            CategoryHidden Floor = new CategoryHidden();
            Floor.Category = BuiltInCategory.OST_Floors;
            hander_OST_Floor = ExternalEvent.Create(Floor);

            CategoryHidden StructuralColumns = new CategoryHidden();
            StructuralColumns.Category = BuiltInCategory.OST_StructuralColumns;
            hander_OST_StructuralColumns = ExternalEvent.Create(StructuralColumns);

            CategoryHidden StructuralFraming = new CategoryHidden();
            StructuralFraming.Category = BuiltInCategory.OST_StructuralFraming;
            hander_OST_StructuralFraming = ExternalEvent.Create(StructuralFraming);

            CategoryHidden StructuralFoundation = new CategoryHidden();
            StructuralFoundation.Category = BuiltInCategory.OST_StructuralFoundation;
            hander_OST_StructuralFoundation = ExternalEvent.Create(StructuralFoundation);

            CategoryHidden Rebar = new CategoryHidden();
            Rebar.Category = BuiltInCategory.OST_Rebar;
            hander_OST_Rebar = ExternalEvent.Create(Rebar);

            CategoryHidden GenericModel = new CategoryHidden();
            GenericModel.Category = BuiltInCategory.OST_GenericModel;
            hander_OST_GenericModel = ExternalEvent.Create(GenericModel);

            CategoryHidden Doors = new CategoryHidden();
            Doors.Category = BuiltInCategory.OST_Doors;
            hander_OST_Doors = ExternalEvent.Create(Doors);

            CategoryHidden Windows = new CategoryHidden();
            Windows.Category = BuiltInCategory.OST_Windows;
            hander_OST_Windows = ExternalEvent.Create(Windows);


            CategoryHidden StairsRailing = new CategoryHidden();
            StairsRailing.Category = BuiltInCategory.OST_StairsRailing;
            hander_OST_StairsRailing = ExternalEvent.Create(StairsRailing);

            CategoryHidden Stairs = new CategoryHidden();
            Stairs.Category = BuiltInCategory.OST_Stairs;
            hander_OST_Stairs = ExternalEvent.Create(Stairs);

            CategoryHidden Roofs = new CategoryHidden();
            Roofs.Category = BuiltInCategory.OST_Roofs;
            hander_OST_Roofs = ExternalEvent.Create(Roofs);

            CategoryHidden Ramps = new CategoryHidden();
            Ramps.Category = BuiltInCategory.OST_Ramps;
            hander_OST_Ramps = ExternalEvent.Create(Ramps);

            CategoryHidden Mass = new CategoryHidden();
            Mass.Category = BuiltInCategory.OST_Mass;
            hander_OST_Mass = ExternalEvent.Create(Mass);

            CategoryHidden Topography = new CategoryHidden();
            Topography.Category = BuiltInCategory.OST_Topography;
            hander_OST_Topography = ExternalEvent.Create(Topography);

            CategoryHidden Planting = new CategoryHidden();
            Planting.Category = BuiltInCategory.OST_Planting;
            hander_OST_Planting = ExternalEvent.Create(Planting);

            CategoryHidden CurtainWallPanels = new CategoryHidden();
            CurtainWallPanels.Category = BuiltInCategory.OST_CurtainWallPanels;
            hander_OST_CurtainWallPanels = ExternalEvent.Create(CurtainWallPanels);

            CategoryHidden CurtainWallMullions = new CategoryHidden();
            CurtainWallMullions.Category = BuiltInCategory.OST_CurtainWallMullions;
            hander_OST_CurtainWallMullions = ExternalEvent.Create(CurtainWallMullions);

            CategoryHidden CurtaSystem = new CategoryHidden();
            CurtaSystem.Category = BuiltInCategory.OST_CurtaSystem;
            hander_OST_CurtaSystem = ExternalEvent.Create(CurtaSystem);

            // 机电
            CategoryHidden PipeCurves = new CategoryHidden();
            PipeCurves.Category = BuiltInCategory.OST_PipeCurves;
            hander_OST_PipeCurves = ExternalEvent.Create(PipeCurves);

            CategoryHidden DuctCurves = new CategoryHidden();
            DuctCurves.Category = BuiltInCategory.OST_DuctCurves;
            hander_OST_DuctCurves = ExternalEvent.Create(DuctCurves);

            CategoryHidden CableTray = new CategoryHidden();
            CableTray.Category = BuiltInCategory.OST_CableTray;
            hander_OST_CableTray = ExternalEvent.Create(CableTray);

            CategoryHidden Conduit = new CategoryHidden();
            Conduit.Category = BuiltInCategory.OST_Conduit;
            hander_OST_Conduit = ExternalEvent.Create(Conduit);

            CategoryHidden PipeFitting = new CategoryHidden();
            PipeFitting.Category = BuiltInCategory.OST_PipeFitting;
            hander_OST_PipeFitting = ExternalEvent.Create(PipeFitting);

            CategoryHidden DuctFitting = new CategoryHidden();
            DuctFitting.Category = BuiltInCategory.OST_DuctFitting;
            hander_OST_DuctFitting = ExternalEvent.Create(DuctFitting);

            CategoryHidden CableTrayFitting = new CategoryHidden();
            CableTrayFitting.Category = BuiltInCategory.OST_CableTrayFitting;
            hander_OST_CableTrayFitting = ExternalEvent.Create(CableTrayFitting);

            CategoryHidden MechanicalEquipment = new CategoryHidden();
            MechanicalEquipment.Category = BuiltInCategory.OST_MechanicalEquipment;
            hander_OST_MechanicalEquipment = ExternalEvent.Create(MechanicalEquipment);

            CategoryHidden PlumbingFixtures = new CategoryHidden();
            PlumbingFixtures.Category = BuiltInCategory.OST_PlumbingFixtures;
            hander_OST_PlumbingFixtures = ExternalEvent.Create(PlumbingFixtures);

            CategoryHidden Sprinklers = new CategoryHidden();
            Sprinklers.Category = BuiltInCategory.OST_Sprinklers;
            hander_OST_Sprinklers = ExternalEvent.Create(Sprinklers);

            CategoryHidden ElectricalEquipment = new CategoryHidden();
            ElectricalEquipment.Category = BuiltInCategory.OST_ElectricalEquipment;
            hander_OST_ElectricalEquipment = ExternalEvent.Create(ElectricalEquipment);

            CategoryHidden LightingFixtures = new CategoryHidden();
            LightingFixtures.Category = BuiltInCategory.OST_LightingFixtures;
            hander_OST_LightingFixtures = ExternalEvent.Create(LightingFixtures);

            CategoryHidden PipeAccessory = new CategoryHidden();
            PipeAccessory.Category = BuiltInCategory.OST_PipeAccessory;
            hander_OST_PipeAccessory = ExternalEvent.Create(PipeAccessory);

            CategoryHidden DuctAccessory = new CategoryHidden();
            DuctAccessory.Category = BuiltInCategory.OST_DuctAccessory;
            hander_OST_DuctAccessory = ExternalEvent.Create(DuctAccessory);

            CategoryHidden ConduitFitting = new CategoryHidden();
            ConduitFitting.Category = BuiltInCategory.OST_ConduitFitting;
            hander_OST_ConduitFitting = ExternalEvent.Create(ConduitFitting);

            CategoryHidden FlexPipeCurves = new CategoryHidden();
            FlexPipeCurves.Category = BuiltInCategory.OST_FlexPipeCurves;
            hander_OST_FlexPipeCurves = ExternalEvent.Create(FlexPipeCurves);

            CategoryHidden FlexDuctCurves = new CategoryHidden();
            FlexDuctCurves.Category = BuiltInCategory.OST_FlexDuctCurves;
            hander_OST_FlexDuctCurves = ExternalEvent.Create(FlexDuctCurves);

            CategoryHidden DuctTerminal = new CategoryHidden();
            DuctTerminal.Category = BuiltInCategory.OST_DuctTerminal;
            hander_OST_DuctTerminal = ExternalEvent.Create(DuctTerminal);

            CategoryHidden Wire = new CategoryHidden();
            Wire.Category = BuiltInCategory.OST_Wire;
            hander_OST_Wire = ExternalEvent.Create(Wire);

            // 注释
            CategoryHidden Levels = new CategoryHidden();
            Levels.Category = BuiltInCategory.OST_Levels;
            hander_OST_Levels = ExternalEvent.Create(Levels);

            CategoryHidden Grids = new CategoryHidden();
            Grids.Category = BuiltInCategory.OST_Grids;
            hander_OST_Grids = ExternalEvent.Create(Grids);

            CategoryHidden Sections = new CategoryHidden();
            Sections.Category = BuiltInCategory.OST_Sections;
            hander_OST_Sections = ExternalEvent.Create(Sections);

            CategoryHidden CLines = new CategoryHidden();
            CLines.Category = BuiltInCategory.OST_CLines;
            hander_OST_CLines = ExternalEvent.Create(CLines);

            CategoryHidden TextNotes = new CategoryHidden();
            TextNotes.Category = BuiltInCategory.OST_TextNotes;
            hander_OST_TextNotes = ExternalEvent.Create(TextNotes);

            CategoryHidden SpotSlopes = new CategoryHidden();
            SpotSlopes.Category = BuiltInCategory.OST_SpotSlopes;
            hander_OST_SpotSlopes = ExternalEvent.Create(SpotSlopes);

            CategoryHidden SpotElevations = new CategoryHidden();
            SpotElevations.Category = BuiltInCategory.OST_SpotElevations;
            hander_OST_SpotElevations = ExternalEvent.Create(SpotElevations);

            CategoryHidden Dimensions = new CategoryHidden();
            Dimensions.Category = BuiltInCategory.OST_Dimensions;
            hander_OST_Dimensions = ExternalEvent.Create(Dimensions);

            CategoryHidden RevisionClouds = new CategoryHidden();
            RevisionClouds.Category = BuiltInCategory.OST_RevisionClouds;
            hander_OST_RevisionClouds = ExternalEvent.Create(RevisionClouds);

            CategoryHidden DoorTags = new CategoryHidden();
            DoorTags.Category = BuiltInCategory.OST_DoorTags;
            hander_OST_DoorTags = ExternalEvent.Create(DoorTags);

            CategoryHidden WindowTags = new CategoryHidden();
            WindowTags.Category = BuiltInCategory.OST_WindowTags;
            hander_OST_WindowTags = ExternalEvent.Create(WindowTags);

            CategoryHidden FloorTags = new CategoryHidden();
            FloorTags.Category = BuiltInCategory.OST_FloorTags;
            hander_OST_FloorTags = ExternalEvent.Create(FloorTags);

            CategoryHidden CurtainWallPanelTags = new CategoryHidden();
            CurtainWallPanelTags.Category = BuiltInCategory.OST_CurtainWallPanelTags;
            hander_OST_CurtainWallPanelTags = ExternalEvent.Create(CurtainWallPanelTags);

            CategoryHidden PipeTags = new CategoryHidden();
            PipeTags.Category = BuiltInCategory.OST_PipeTags;
            hander_OST_PipeTags = ExternalEvent.Create(PipeTags);

            CategoryHidden DuctTags = new CategoryHidden();
            DuctTags.Category = BuiltInCategory.OST_DuctTags;
            hander_OST_DuctTags = ExternalEvent.Create(DuctTags);

            CategoryHidden CableTrayTags = new CategoryHidden();
            CableTrayTags.Category = BuiltInCategory.OST_CableTrayTags;
            hander_OST_CableTrayTags = ExternalEvent.Create(CableTrayTags);

            CategoryHidden ConduitTags = new CategoryHidden();
            ConduitTags.Category = BuiltInCategory.OST_ConduitTags;
            hander_OST_ConduitTags = ExternalEvent.Create(ConduitTags);

            CategoryHidden MaterialTags = new CategoryHidden();
            MaterialTags.Category = BuiltInCategory.OST_MaterialTags;
            hander_OST_MaterialTags = ExternalEvent.Create(MaterialTags);

            CategoryHidden AreaTags = new CategoryHidden();
            AreaTags.Category = BuiltInCategory.OST_AreaTags;
            hander_OST_AreaTags = ExternalEvent.Create(AreaTags);

        }



        private void Window_Closed(object sender, EventArgs e)
        {
            // 窗口关闭
            this.Close();
        }

        private void Button_ShowNothing_AS_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackgroundAll(Button_ShowNothing_AS, Button_ShowAll_AS);
            hander_ShowNothing_AS.Raise();
        }

        private void Button_ShowNothing_MEP_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackgroundAll(Button_ShowNothing_MEP, Button_ShowAll_MEP);
            hander_ShowNothing_MEP.Raise();
        }

        private void Button_ShowNothing_TAG_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackgroundAll(Button_ShowNothing_TAG, Button_ShowAll_TAG);
            hander_ShowNothing_TAG.Raise();
        }

        private void Button_ShowAll_AS_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackgroundAll(Button_ShowAll_AS, Button_ShowNothing_AS);
            hander_ShowAll_AS.Raise();
        }

        private void Button_ShowAll_MEP_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackgroundAll(Button_ShowAll_MEP, Button_ShowNothing_MEP);
            hander_ShowAll_MEP.Raise();
        }

        private void Button_ShowAll_TAG_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackgroundAll(Button_ShowAll_TAG, Button_ShowNothing_TAG);
            hander_ShowAll_TAG.Raise();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="button"> 点击</param>
        /// <param name="button2"> 自动更换</param>
        public void ButtonBackgroundAll(Button button, Button button2)
        {
            if (button.Background == Brushes.Orange)
            {
                button.Background = Brushes.White;
                button2.Background = Brushes.Orange;
            }
            else
            {
                button.Background = Brushes.Orange;
                button2.Background = Brushes.White;
            }
        }

        // 按钮颜色设置
        public void ButtonBackground(Button button)
        {
            if (button.Background == Brushes.GreenYellow)
            {
                button.Background = Brushes.White;
            }
            else
            {
                button.Background = Brushes.GreenYellow;
            }
        }


        // 墙可见性
        private void Button_Wall_Click(object sender, RoutedEventArgs e)
        {
            // 按钮颜色设置
            ButtonBackground(button_Wall);
            // 墙 可见性设置
            hander_OST_Wall.Raise();
        }

        private void Button_Floor_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_Floor);
            hander_OST_Floor.Raise();
        }

        private void button_StructuralFraming_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_StructuralFraming);
            hander_OST_StructuralFraming.Raise();
        }

        private void button_StructuralColumns_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_StructuralColumns);
            hander_OST_StructuralColumns.Raise();
        }

        private void button_StructuralFoundation_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_StructuralFoundation);
            hander_OST_StructuralFoundation.Raise();
        }

        private void button_Rebar_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_Rebar);
            hander_OST_Rebar.Raise();
        }

        private void button_GenericModel_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_GenericModel);
            hander_OST_GenericModel.Raise();
        }

        private void button_Doors_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_Doors);
            hander_OST_Doors.Raise();
        }

        private void button_Windows_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_Windows);
            hander_OST_Windows.Raise();
        }

        private void button_StairsRailing_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_StairsRailing);
            hander_OST_StairsRailing.Raise();
        }

        private void button_Stairs_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_Stairs);
            hander_OST_Stairs.Raise();
        }

        private void button_Roofs_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_Roofs);
            hander_OST_Roofs.Raise();
        }

        private void button_Ramps_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_Ramps);
            hander_OST_Ramps.Raise();
        }

        private void button_Mass_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_Mass);
            hander_OST_Mass.Raise();
        }

        private void button_Topography_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_Topography);
            hander_OST_Topography.Raise();
        }

        private void button_Planting_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_Planting);
            hander_OST_Planting.Raise();
        }

        private void button_CurtainWallPanels_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_CurtainWallPanels);
            hander_OST_CurtainWallPanels.Raise();
        }

        private void button_CurtainWallMullions_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_CurtainWallMullions);
            hander_OST_CurtainWallMullions.Raise();
        }

        private void button_CurtaSystem_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_CurtaSystem);
            hander_OST_CurtaSystem.Raise();
        }

        // 机电
        private void button_OST_PipeCurves_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_PipeCurves);
            hander_OST_PipeCurves.Raise();
        }

        private void button_OST_DuctCurves_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_DuctCurves);
            hander_OST_DuctCurves.Raise();
        }

        private void button_OST_CableTray_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_CableTray);
            hander_OST_CableTray.Raise();
        }

        private void button_OST_Conduit_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_Conduit);
            hander_OST_Conduit.Raise();
        }

        private void button_OST_PipeFitting_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_PipeFitting);
            hander_OST_PipeFitting.Raise();
        }

        private void button_OST_DuctFitting_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_DuctFitting);
            hander_OST_DuctFitting.Raise();
        }

        private void button_OST_CableTrayFitting_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_CableTrayFitting);
            hander_OST_CableTrayFitting.Raise();
        }

        private void button_OST_MechanicalEquipment_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_MechanicalEquipment);
            hander_OST_MechanicalEquipment.Raise();
        }

        private void button_OST_PlumbingFixtures_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_PlumbingFixtures);
            hander_OST_PlumbingFixtures.Raise();
        }

        private void button_OST_Sprinklers_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_Sprinklers);
            hander_OST_Sprinklers.Raise();
        }

        private void button_OST_ElectricalEquipment_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_ElectricalEquipment);
            hander_OST_ElectricalEquipment.Raise();
        }

        private void button_OST_LightingFixtures_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_LightingFixtures);
            hander_OST_LightingFixtures.Raise();
        }

        private void button_OST_PipeAccessory_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_PipeAccessory);
            hander_OST_PipeAccessory.Raise();
        }

        private void button_OST_DuctAccessory_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_DuctAccessory);
            hander_OST_DuctAccessory.Raise();
        }

        private void button_OST_ConduitFitting_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_ConduitFitting);
            hander_OST_ConduitFitting.Raise();
        }

        private void button_OST_FlexPipeCurves_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_FlexPipeCurves);
            hander_OST_FlexPipeCurves.Raise();
        }

        private void button_OST_FlexDuctCurves_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_FlexDuctCurves);
            hander_OST_FlexDuctCurves.Raise();
        }

        private void button_OST_DuctTerminal_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_DuctTerminal);
            hander_OST_DuctTerminal.Raise();
        }

        private void button_OST_Wire_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_Wire);
            hander_OST_Wire.Raise();
        }


        // 注释
        private void button_OST_Levels_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_Levels);
            hander_OST_Levels.Raise();
        }

        private void button_OST_Grids_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_Grids);
            hander_OST_Grids.Raise();
        }

        private void button_OST_Sections_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_Sections);
            hander_OST_Sections.Raise();
        }

        private void button_OST_CLines_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_CLines);
            hander_OST_CLines.Raise();
        }

        private void button_OST_TextNotes_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_TextNotes);
            hander_OST_TextNotes.Raise();
        }

        private void button_OST_SpotSlopes_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_SpotSlopes);
            hander_OST_SpotSlopes.Raise();
        }

        private void button_OST_SpotElevations_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_SpotElevations);
            hander_OST_SpotElevations.Raise();
        }

        private void button_OST_Dimensions_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_Dimensions);
            hander_OST_Dimensions.Raise();
        }

        private void button_OST_RevisionClouds_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_RevisionClouds);
            hander_OST_RevisionClouds.Raise();
        }

        private void button_OST_DoorTags_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_DoorTags);
            hander_OST_DoorTags.Raise();
        }

        private void button_OST_WindowTags_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_WindowTags);
            hander_OST_WindowTags.Raise();
        }

        private void button_OST_FloorTags_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_FloorTags);
            hander_OST_FloorTags.Raise();
        }

        private void button_OST_CurtainWallPanelTags_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_CurtainWallPanelTags);
            hander_OST_CurtainWallPanelTags.Raise();
        }

        private void button_OST_PipeTags_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_PipeTags);
            hander_OST_PipeTags.Raise();
        }

        private void button_OST_DuctTags_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_DuctTags);
            hander_OST_DuctTags.Raise();
        }

        private void button_OST_CableTrayTags_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_CableTrayTags);
            hander_OST_CableTrayTags.Raise();
        }

        private void button_OST_ConduitTags_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_ConduitTags);
            hander_OST_ConduitTags.Raise();
        }

        private void button_OST_MaterialTags_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_MaterialTags);
            hander_OST_MaterialTags.Raise();
        }

        private void button_OST_AreaTags_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackground(button_OST_AreaTags);
            hander_OST_AreaTags.Raise();
        }

    }

    /// <summary>
    /// 设置当前视图类别图元可见性
    /// </summary>
    public class CategoryHidden : IExternalEventHandler
    {
        // 类别名称
        public BuiltInCategory Category { get; set; }
        // 设置外部事务事件名称
        public string StartName = "图元可见性";

        /// <summary>
        /// 固定接口设置可见性
        /// </summary>
        /// <param name="app"></param>
        public void Execute(UIApplication app)
        {
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;

            Transaction t = new Transaction(doc);
            t.Start(StartName);

            try
            {
                SetCategoryVV(doc);
                t.Commit();
            }
            catch (Exception e)
            {
                t.RollBack();
                TaskDialog.Show("提示", e.Message + Strings.error);
            }
        }
        /// <summary>
        /// 固定接口获取事务名称
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return StartName;
        }

        /// <summary>
        /// 设置类别可见性
        /// </summary>
        /// <param name="doc"></param>
        public void SetCategoryVV(Document doc)
        {
            Categories cates = doc.Settings.Categories;
            var cateId = cates.get_Item(Category).Id;
            var isShow = doc.ActiveView.GetCategoryHidden(cateId);
            // 隐藏/显示 True to make elements of this category invisible, false to make them visible.
            if (isShow)
            {
                isShow = false;
                doc.ActiveView.SetCategoryHidden(cateId, isShow);
            }
            else
            {
                isShow = true;
                doc.ActiveView.SetCategoryHidden(cateId, isShow);
            }
        }
    }

    /// <summary>
    /// 设置全部类别的可见性
    /// </summary>
    public class CategoryHiddenAll : IExternalEventHandler
    {
        public bool ShowAll { get; set; }
        public List<BuiltInCategory> builtInCategories { get; set; }
        // 设置外部事务事件名称
        public string StartName = "所有可见性";

        public void Execute(UIApplication app)
        {
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;

            Transaction t = new Transaction(doc);
            t.Start(StartName);

            try
            {
                SetCategory(doc, builtInCategories);
                t.Commit();
            }
            catch (Exception e)
            {
                t.RollBack();
                TaskDialog.Show("提示", e.Message + Strings.error);
            }
        }

        public string GetName()
        {
            return StartName;
        }

        public void SetCategory(Document doc, List<BuiltInCategory> builtInCategories)
        {
            foreach (var category in builtInCategories)
            {
                Categories categories = doc.Settings.Categories;
                var cateId = categories.get_Item(category).Id;
                var isShow = doc.ActiveView.GetCategoryHidden(cateId);
                if (isShow != ShowAll)
                {
                    doc.ActiveView.SetCategoryHidden(cateId, ShowAll);
                }
            }

        }


    }

}
