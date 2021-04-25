using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FamilyLoadCreate
{
    [Transaction(TransactionMode.Manual)]
    class SavingFamily : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var filePath = @"D:\缓存文件\临时\导出";
            bool overWrite = false;
            var res = ExportFamilies.Instance(doc, filePath, overWrite);
            //var res = ExportFamilies.Documnet(doc, filePath, overWrite);
            var result = string.Format("导出位置：{0}\n是否覆盖:{1}\n导出个数：{2}", filePath, overWrite, res);
            TaskDialog.Show("结果", result);

            return Result.Succeeded;
        }
    }

    public class ExportFamilies
    {
        /// <summary>
        /// 导出当前项目正在使用的族实例文件
        /// </summary>
        /// <param name="targetFolder"> 导出的路径</param>
        /// <param name="overwrite"> 是否覆盖文件 (可选) 默认 true(覆盖)</param>
        /// <returns> 成功导出族文件的个数</returns>
        public static int Instance(Document doc, string targetFolder, bool overwrite = true)
        {
            #region 获取当前文档使用的所有族Family
            // 过滤所有的族实例
            FilteredElementCollector collectorInstance = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType();
            // 导出族family列表
            List<Family> list = new List<Family>();
            // 过滤重复Family类型
            List<string> _FamilyName = new List<string>();
            foreach (var item in collectorInstance)
            {
                var el = item as FamilyInstance;
                var family = el.Symbol.Family;
                if (!_FamilyName.Contains(family.Name))
                {
                    // 添加正在使用的family到列表
                    list.Add(family);
                    // 添加名字到判断列表
                    _FamilyName.Add(family.Name);
                }
            }
            #endregion

            // 导出Family到本地文件
            int exported = FamilySave(doc, targetFolder, list, overwrite);
            return exported;
        }

        /// <summary>
        /// 导出当前项目所有的族
        /// </summary>
        /// <param name="targetFolder"> 导出的路径</param>
        /// <param name="overwrite"> 是否覆盖文件</param>
        /// <returns> 成功导出族文件的个数</returns>
        public static int Documnet(Document doc, string targetFolder, bool overwrite = true)
        {
            #region 获取当前文档所有族Family
            // 创建过滤器 获取所有的族类型
            FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(Family));
            // 获取族文件列表
            List<Family> list = collector.OfType<Family>().ToList();
            #endregion

            // 导出Family到本地文件
            int exported = FamilySave(doc, targetFolder, list, overwrite);
            return exported;
        }

        /// <summary>
        /// 导出family到本地文件
        /// </summary>
        /// <param name="doc"> document项目文档</param>
        /// <param name="targetFolder"> 保存文件的路径地址</param>
        /// <param name="list"> 需要导出的family列表</param>
        /// <param name="Overwrite"> 是否覆盖文件</param>
        /// <returns> 成功导出族文件的个数</returns>
        private static int FamilySave(Document doc, string targetFolder, List<Family> list, bool Overwrite)
        {
            int exportedNum = 0;
            // 判断给定的列表是不是空
            if (list.Count == 0)
            {
                return exportedNum;
            }
            // 遍历family列表
            foreach (Family item in list)
            {
                #region 打开族文档保持文件
                // 判断family文件是不是可编辑的
                if (item.IsEditable /*&& item.IsUserCreated*/)
                {
                    // 判断文件夹是否以创建
                    if (!Directory.Exists(targetFolder))
                    {
                        Directory.CreateDirectory(targetFolder);
                    }
                    // 构建文件名称 string
                    string fileName = Path.Combine(targetFolder, item.Name + ".rfa");
                    // 判断文件是否存在
                    var fileExist = File.Exists(fileName);
                    // 判断文件是否存在
                    if (fileExist && !Overwrite)
                    {
                        continue;
                    }
                    // 获取族文件文档 Document
                    Document familyDoc = doc.EditFamily(item);
                    if (familyDoc != null)
                    {
                        // 确认它是否未族文件 IsFamilyDocument
                        if (familyDoc.IsFamilyDocument)
                        {
                            // 另存参数设置
                            var options = new SaveAsOptions()
                            {
                                OverwriteExistingFile = true,
                                MaximumBackups = 1,
                                Compact = true
                            };
                            familyDoc.SaveAs(fileName, options);
                        }
                        // 关闭族文件
                        familyDoc.Close(false);
                        // 统计个数
                        ++exportedNum;
                    }
                }
                #endregion
            }
            return exportedNum;
        }
    }
}
