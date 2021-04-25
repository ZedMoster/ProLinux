using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class ReNameLevel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // 程序是否注册可运行
            if (Run.Running(Strings.key))
            {
                // 获取所有标高
                List<Element> levels = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels)
                    .WhereElementIsNotElementType().ToElements().ToList();
                if (levels.Count == 0)
                {
                    TaskDialog.Show("提示", "当前项目还未创建标高");
                    return Result.Failed;
                }
                else
                {
                    // 获取建筑标高 结构标高
                    List<Element> els_A = new List<Element>();
                    List<Element> els_S = new List<Element>();
                    foreach (var item in levels)
                    {
                        if (item.get_Parameter(BuiltInParameter.LEVEL_IS_BUILDING_STORY).AsInteger() == 1)
                        {
                            els_A.Add(item);
                        }
                        else
                        {
                            els_S.Add(item);
                        }
                    }
                    TransactionGroup T = new TransactionGroup(doc, "更新标高名称");
                    T.Start();
                    bool Push = false;
                    if (els_A.Count != 0)
                    {
                        Transaction t = new Transaction(doc);
                        t.Start("建筑楼层命名");
                        try
                        {
                            RenameLevel renameLevel = new RenameLevel();
                            renameLevel.Rename(els_A);
                            t.Commit();
                            Push = true;
                        }
                        catch (Exception e)
                        {
                            TaskDialog.Show("提示", e.Message + Strings.error);
                            t.RollBack();
                        }
                    }

                    if (els_S.Count != 0)
                    {
                        Transaction t = new Transaction(doc);
                        t.Start("结构楼层命名");
                        try
                        {
                            RenameLevel renameLevel = new RenameLevel();
                            renameLevel.Rename(els_S);
                            t.Commit();
                            Push = true;
                        }
                        catch (Exception e)
                        {
                            TaskDialog.Show("提示", e.Message + Strings.error);
                            t.RollBack();
                        }
                    }
                    if (Push)
                    {
                        T.Assimilate();
                    }
                    else
                    {
                        T.RollBack();
                    }
                }
            }

            return Result.Succeeded;
        }

    }

    class RenameLevel
    {
        public void Rename(List<Element> levels)
        {
            Dictionary<double, Level> keyValues_TOP = new Dictionary<double, Level>();
            Dictionary<double, Level> keyValues_BOT = new Dictionary<double, Level>();
            // 标高按高程进行分组
            foreach (var item in levels)
            {
                var level = item as Level;
                var value = level.ProjectElevation;
                if (value >= 0)
                {
                    keyValues_TOP.Add(value, level);
                }
                else
                {
                    keyValues_BOT.Add(value, level);
                }
            }

            // 按高程进行排序
            var _TOP = keyValues_TOP.Keys.ToList();
            var _Bot = keyValues_BOT.Keys.ToList();

            // 排序
            _TOP.Sort();
            // 先排序 再反转 实现倒序排序
            _Bot.Sort();
            _Bot.Reverse();

            // 地上部分
            if (_TOP.Count != 0)
            {
                for (int i = 0; i < _TOP.Count; i++)
                {
                    var _value = _TOP[i];
                    var level = keyValues_TOP[_value];
                    var _AS = GetLevelStory(level);
                    // 格式化高程信息
                    var strElevation = String.Format(CultureInfo.InvariantCulture, "{0:0.000}", _value.FeetToMillimeter() / 1000);
                    if (_AS == null)
                    {
                        // 格式化名称
                        string LevelName = string.Format("F{0}_{1}", i + 1, strElevation);
                        // 更新标高名称
                        level.get_Parameter(BuiltInParameter.DATUM_TEXT).Set(LevelName);
                    }
                    else
                    {
                        // 格式化名称
                        string LevelName = string.Format("F{0}({1})_{2}", i + 1, _AS, strElevation);
                        // 更新标高名称
                        level.get_Parameter(BuiltInParameter.DATUM_TEXT).Set(LevelName);
                    }
                }

            }
            // 地下室部分
            if (_Bot.Count != 0)
            {
                for (int i = 0; i < _Bot.Count; i++)
                {
                    var _value = _Bot[i];
                    var level = keyValues_BOT[_value];
                    var _AS = GetLevelStory(level);
                    // 格式化高程信息
                    var strElevation = String.Format(CultureInfo.InvariantCulture, "{0:0.000}", _value.FeetToMillimeter() / 1000);
                    if (_AS == null)
                    {
                        // 格式化名称
                        string LevelName = string.Format("F{0}_{1}", i + 1, strElevation);
                        // 更新标高名称
                        level.get_Parameter(BuiltInParameter.DATUM_TEXT).Set(LevelName);
                    }
                    else
                    {
                        // 格式化名称
                        string LevelName = string.Format("B{0}({1})_{2}", i + 1, _AS, strElevation);
                        // 更新标高名称
                        level.get_Parameter(BuiltInParameter.DATUM_TEXT).Set(LevelName);
                        //TaskDialog.Show("1", LevelName);
                    }
                }
            }
        }

        /// <summary>
        /// 依据 标高 表示数据 重命名
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public string GetLevelStory(Level level)
        {
            if (level.get_Parameter(BuiltInParameter.LEVEL_IS_BUILDING_STORY).AsInteger() == 1 && level.get_Parameter(BuiltInParameter.LEVEL_IS_STRUCTURAL).AsInteger() == 0)
            {
                return "A";
            }
            else if (level.get_Parameter(BuiltInParameter.LEVEL_IS_STRUCTURAL).AsInteger() == 1 && level.get_Parameter(BuiltInParameter.LEVEL_IS_BUILDING_STORY).AsInteger() == 0)
            {
                return "S";
            }
            else if (level.get_Parameter(BuiltInParameter.LEVEL_IS_STRUCTURAL).AsInteger() == 1 && level.get_Parameter(BuiltInParameter.LEVEL_IS_BUILDING_STORY).AsInteger() == 1)
            {
                return "AS";
            }
            else
            {
                return null;
            }
        }
    }
}
