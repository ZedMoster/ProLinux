using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AGBIM.Visual.RevitPlugins.AGFamilyLib
{
    public class FamilyLoad
    {
        /// <summary>
        /// 在当前doc 载入族文件  -*- 注意：外部需要创建事务 Transaction -*-
        /// </summary>
        /// <param name="uidoc"> UIdoc文档</param>
        /// <param name="rfaFilePath"> 族文件完整的路径</param>
        /// <param name="TypeName"> 指定族文件的类型 (可选)</param>
        /// <returns></returns>
        public static bool Create(UIDocument uidoc, string rfaFilePath, string TypeName = null)
        {
            // 族类型
            FamilySymbol symbol = null;
            // 载入的 Family
            Family family = null;
            // 判断是否载入成功
            bool loadSuccess = false;

            #region 载入族获取family
            Document doc = uidoc.Document;
            Transaction t = new Transaction(doc, "载入族");
            t.Start();
            try
            {
                // 载入族文件
                loadSuccess = doc.LoadFamily(rfaFilePath, out family);
                t.Commit();
            }
            catch(Exception ex)
            {
                t.RollBack();
                throw new NotSupportedException("Need start new Transaction ." );
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
                var familyname = rfaFilePath.Split('\\').LastOrDefault().Replace(".rfa", "");
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
            return loadSuccess;
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

            return symbol;
        }
    }
}
