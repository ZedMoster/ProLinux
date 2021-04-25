using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;

namespace FamilyLoadCreate
{
    [Transaction(TransactionMode.Manual)]
    class LoadingFamilyCreate : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            // 族文件的完整路径
            string filePath = @"D:\缓存文件\临时\a .rfa";

            LoadingFamily.Create(uidoc, doc, filePath);

            return Result.Succeeded;
        }
    }

    public class LoadingFamily
    {
        /// <summary>
        /// 在当前doc 载入族文件
        /// -*-注意：外部需要创建事务组TransactionGroup-*-
        /// </summary>
        /// <param name="uidoc"> UIdoc文档</param>
        /// <param name="doc"> 当前使用的document</param>
        /// <param name="rfaFilePath"> 族文件完整的路径</param>
        /// <param name="TypeName"> 指定族文件的类型 (可选)</param>
        /// <returns></returns>
        public static void Create(UIDocument uidoc, Document doc, string rfaFilePath, string TypeName = null)
        {
            FamilySymbol symbol = null;
            // 定义变量判断是否成功
            bool loadSuccess = false;

            // 载入的family
            Family family = null;

            #region 载入族获取family
            Transaction tran = new Transaction(doc, "载入族文件");
            tran.Start();
            try
            {
                // 载入族文件
                loadSuccess = doc.LoadFamily(rfaFilePath, out family);
                tran.Commit();
            }
            catch
            {
                tran.RollBack();
                return;
            }
            #endregion

            #region 载入失败项目中已存在需要加载的族类型
            if (!loadSuccess)
            {
                #region 获取项目中指定类别的族
                // 创建过滤器 获取所有的族类型
                FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(Family));
                // 获取族文件列表
                List<Family> list = collector.OfType<Family>().ToList();
                // 获取到族family的名称
                var familyname = rfaFilePath.Split('\\').LastOrDefault()?.Replace(".rfa", "");
                // 获取当前项目中的family
                foreach (var item in list)
                {
                    if (item.Name == familyname)
                    {
                        family = item;
                        break;
                    }
                }
                #endregion
            }
            #endregion

            // 获取当前Family的类型
            if (family != null)
            {
                symbol = GetFamilySymbol(doc, family, TypeName);
                if (symbol != null)
                {
                    // 激活UI创建窗口
                    uidoc.PostRequestForElementTypePlacement(symbol);
                }
                else
                {
                    TaskDialog.Show("提示", "指定族类型获取失败");
                }
            }
            else
            {
                TaskDialog.Show("提示", "族载入失败");
            }
        }

        /// <summary>
        /// 获取给定Family的族类型
        /// </summary>
        /// <param name="doc"> 文档document</param>
        /// <param name="family"> 族文件Family</param>
        /// <param name="TypeName"> 指定类型名称</param>
        /// <returns></returns>
        private static FamilySymbol GetFamilySymbol(Document doc, Family family, string TypeName)
        {
            #region 获取family 族文件的类型实例 可指定名称
            FamilySymbol symbol = null;

            if (TypeName == null)
            {
                // 未指定类型参数
                symbol = doc.GetElement(family.GetFamilySymbolIds().FirstOrDefault()) as FamilySymbol;
            }
            else
            {
                #region 指定族名称
                foreach (ElementId TypeId in family.GetFamilySymbolIds())
                {
                    symbol = doc.GetElement(TypeId) as FamilySymbol;
                    if (symbol != null)
                    {
                        if (symbol.Name == TypeName)
                        {
                            break;
                        }
                    }
                }
                #endregion
            }
            #endregion
            return symbol;
        }
    }
}
