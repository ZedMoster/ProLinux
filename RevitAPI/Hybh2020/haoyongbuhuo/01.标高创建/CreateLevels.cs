using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    public class CreateLevels : IExternalCommand
    {
        public ElementId FloorplanId { get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // 程序是否注册可运行
            if (Run.Running(Strings.key))
            {
                // 显示用户窗口
                var window = new WPFCreatLevels();
                window.ShowDialog();
                if (window.IsHitTestVisible)
                {
                    var firstName = window.name0.Text.ToUpper();  // 名称
                    var AorS = window.AS.SelectedValue.ToString();  // 建筑还是结构
                    var result1 = int.TryParse(window.name1.Text, out int number);  // 编号
                    if (!result1)
                        number = 0;
                    var result2 = float.TryParse(window.name2.Text, out float inputValue);  // 高程
                    float elevation = inputValue * 1000;
                    if (!result2)
                        elevation = 0;
                    var height = window.name3.Text;
                    string[] strArray = height.Split(',', '，', ' ');

                    var collection_levels = new FilteredElementCollector(doc).OfClass(typeof(Level)).ToElements().ToList();

                    List<float> oldLevelElevations = new List<float>();  // 项目存在的标高高程值
                    foreach (Element levelEl in collection_levels)
                    {
                        var oldEleva = levelEl.get_Parameter(BuiltInParameter.LEVEL_ELEV).AsValueString();
                        bool boolold = float.TryParse(oldEleva, out float oldElevation);
                        if (boolold)
                        {
                            oldLevelElevations.Add(oldElevation);
                        }
                    }

                    List<float> elevaList = ElevationList(elevation, strArray, firstName); // 高程值 列表
                    List<string> newNamesList = NewLevelNames(firstName, number, elevaList, AorS); // 标高 名称列表

                    // to guarantee that a transaction does not out-live its scope.
                    using (Transaction transaction = new Transaction(doc))
                    #region 创建标高及视图平面
                    {
                        if (transaction.Start("创建标高") == TransactionStatus.Started)
                        {
                            try
                            {
                                // 开启事务修改模型
                                int count = elevaList.Count();

                                // 获取楼层平面类型id
                                var floorPlanIds = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).WhereElementIsElementType().ToElements();
                                foreach (var item in floorPlanIds)
                                {
                                    // 获取创建楼层标高的类别 建筑还是结构
                                    var el = item as ElementType;
                                    if (AorS == "A")
                                    {
                                        if (el.Name == "楼层平面")
                                        {
                                            FloorplanId = el.Id;
                                        }
                                    }
                                    else
                                    {
                                        if (el.Name == "结构平面")
                                        {
                                            FloorplanId = el.Id;
                                        }
                                    }
                                }

                                for (int a = 0; a < count; a++)
                                {
                                    float Levelelevation = elevaList[a];
                                    string LevelName = newNamesList[a];
                                    if (!oldLevelElevations.Contains(Levelelevation))
                                    {
                                        var value = UnitUtils.Convert(Levelelevation, DisplayUnitType.DUT_MILLIMETERS, DisplayUnitType.DUT_DECIMAL_FEET);
                                        // 创建标高
                                        var level_F = Level.Create(doc, value);
                                        // 创建楼层平面
                                        ViewPlan floorView = ViewPlan.Create(doc, FloorplanId, level_F.Id);
                                        level_F.Name = LevelName;
                                        // 建筑 结构
                                        if (AorS.Contains('S'))
                                        {
                                            level_F.get_Parameter(BuiltInParameter.LEVEL_IS_BUILDING_STORY).Set(0);
                                            level_F.get_Parameter(BuiltInParameter.LEVEL_IS_STRUCTURAL).Set(1);
                                        }
                                        else
                                        {
                                            level_F.get_Parameter(BuiltInParameter.LEVEL_IS_BUILDING_STORY).Set(1);
                                            level_F.get_Parameter(BuiltInParameter.LEVEL_IS_STRUCTURAL).Set(0);
                                        }
                                    }

                                }
                                // 汇总结果展示
                                if (TransactionStatus.Committed == transaction.Commit())
                                    // 输出完成数据  messageT 
                                    TaskDialog.Show("统计", "标高及平面视图已创建完成总计：\n" + count.ToString());
                            }
                            catch (Exception EX)
                            {
                                transaction.RollBack();
                                TaskDialog.Show("提示", EX.ToString() + Strings.error);
                                return Result.Failed;
                            }
                        }
                    }
                    #endregion
                }
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// 构建 高程值 列表
        /// </summary>
        /// <param name="elevation"></param>
        /// <param name="strArray"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public List<float> ElevationList(float elevation, string[] strArray, string code)
        {
            List<float> elevaList = new List<float>();
            elevaList.Add(elevation); // 创建第一个标高

            foreach (string strheight in strArray)  // 遍历输入

                if (strheight.Contains("*"))
                {
                    string[] arry = strheight.Split('*');
                    bool resBool = float.TryParse(arry[0], out float res);  // 层高
                    bool numBool = int.TryParse(arry[1], out int num);  // 个数
                    for (int i = 0; i < num; i++)
                    {
                        if (numBool && resBool)
                        {
                            if (code == "B")
                                elevation -= res;
                            else
                                elevation += res;
                            elevaList.Add(elevation);
                        }
                    }
                }

                else
                {
                    bool resBool = float.TryParse(strheight, out float res);
                    if (resBool)
                    {
                        if (code == "B")
                            elevation -= res;
                        else
                            elevation += res;
                        elevaList.Add(elevation);
                    }
                }

            return elevaList;
        }

        /// <summary>
        /// 构建 标高 名称 列表
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="number"></param>
        /// <param name="elevation_list"></param>
        /// <param name="AorS"></param>
        /// <returns></returns>
        public List<string> NewLevelNames(string firstName, int number, List<float> elevation_list, string AorS)
        {
            // 获取标高名称列表
            var newNames = new List<string>();
            //List<string> newNames = new List<string>();
            int count = elevation_list.Count();
            for (int a = 0; a < count; a++)
            {
                var N = String.Format(CultureInfo.InvariantCulture, "{0:0}", number + a);
                var strElevation = String.Format(CultureInfo.InvariantCulture, "{0:0.000}", elevation_list[a] / 1000);
                var name = String.Format("{0}{1}({2})_{3}", firstName, N, AorS, strElevation);
                newNames.Add(name);
            }
            return newNames;
        }
    }
}
