using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class Text : IExternalCommand
    {
        /// <summary>
        /// 测试是否安装成功
        /// </summary>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;
            Categories Groups = doc.Settings.Categories;
            List<ElementId> selIds = sel.GetElementIds().ToList();
            string CategoryName = Groups.get_Item(BuiltInCategory.OST_Walls).Name;

            //FilteredElementCollector filteredElementCollectors = new FilteredElementCollector(doc);
            //var levels = filteredElementCollectors.OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().ToElements().ToList();
            //GetLevel getLevel = new GetLevel();
            //var L0 = levels[0] as Level;

            //var L = getLevel.NearLevel(L0, true);

            //var L2 = getLevel.NearLevel(L0,false);

            if (selIds.Count == 0)
            {
                TaskDialog.Show("测试成功", "插件安装完成！\n\n1.注册账户即可免费使用\n2.按F1进入帮助模式" + Strings.error);
            }
            else
            {
                // 创建几何选项
                var options = new Options();
                var el = doc.GetElement(selIds.First()); // as FamilyInstance
                var geometry = el.get_Geometry(options);

                foreach (var geometryInstance in geometry)
                {
                    Solid solid = geometryInstance as Solid;
                    if (solid == null || solid.Faces.Size == 0 || solid.Edges.Size == 0)
                    {
                        continue;
                    }
                    FaceArray faceArray = solid.Faces;
                    TaskDialog.Show(CategoryName + "面：", faceArray.Size.ToString());
                }
            }

            #region 备份注释

            // Background="#FFC8FFFF" 统一WPF背景颜色设置

            //MessageBox.Show(string.Format("所选择的结构构件的体积和：\n\n{0:0.000}  m3", volume), "体积");

            //var version = doc.Application.VersionNumber;
            //TaskDialog.Show("测试完成", version);

            // 获取连接项目模型
            //Reference r = uidoc.Selection.PickObject(ObjectType.LinkedElement);

            //if (Run.Running(Strings.key))
            //{
            //    // do something
            //}

            //// 关闭警告  Transaction t = new Transaction(doc);
            //FailureHandlingOptions fho = t.GetFailureHandlingOptions();
            //fho.SetFailuresPreprocessor(new FailuresPreprocessor());
            //t.SetFailureHandlingOptions(fho);
            #endregion

            return Result.Succeeded;
        }
    }
}
