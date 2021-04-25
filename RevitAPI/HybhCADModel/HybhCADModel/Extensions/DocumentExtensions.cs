using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

using HybhCADModel.Lib;

namespace HybhCADModel.Extensions
{
    public static class DocumentExtensions
    {
        /// <summary>
        /// 过滤器
        /// </summary>
        public static IEnumerable<T> TCollector<T>(this Document doc)
        {
            return new FilteredElementCollector(doc).OfClass(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// 过滤器
        /// </summary>
        public static IEnumerable<T> TCollector<T>(this Document doc, BuiltInCategory builtInCategory)
        {
            return new FilteredElementCollector(doc).OfClass(typeof(T)).OfCategory(builtInCategory).Cast<T>();
        }

        /// <summary>
        /// 图层名称是否相同
        /// </summary>
        public static bool IsNotEqualLayerName(this Document doc, GeometryObject geoObj, GeometryObject insObj)
        {
            var graphicsStyle = geoObj.GraphicsStyleId.ToElement(doc) as GraphicsStyle;
            var _graphicsStyle = insObj.GraphicsStyleId.ToElement(doc) as GraphicsStyle;
            return _graphicsStyle.GraphicsStyleCategory.Name != graphicsStyle.GraphicsStyleCategory.Name;
        }

        /// <summary>
        /// 判断导入CAD图纸的单位是否正确
        /// </summary>
        public static bool IsDisplayUnits(this Document doc, Reference reference, string units = "米")
        {
            var dwg = reference.ToElement(doc);
            var linkType = dwg.GetTypeId().ToElement(doc) as CADLinkType;
            return linkType.get_Parameter(BuiltInParameter.IMPORT_DISPLAY_UNITS).AsValueString() == units;
        }

        /// <summary>
        /// 通过名称获取族类型
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elsType"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static FamilySymbol GetFamilySymbolByName(this Document doc, List<FamilySymbol> elsType, string typeName)
        {
            // 定义变量
            FamilySymbol symbol = null;
            if(elsType.Count == 0)
            {
                ShowResult.Print($"请载入族文件");
                return symbol;
            }
            // 通过族类型及名称获取指定的类型
            symbol = elsType.FirstOrDefault(x => x.Name == typeName);
            // 创建类型
            if(symbol == null)
            {
                var elType = elsType.FirstOrDefault();
                Transaction tcopy = new Transaction(doc);
                tcopy.Start("创建类型");
                try
                {
                    symbol = elType.Duplicate(typeName) as FamilySymbol;
                    tcopy.Commit();
                }
                catch(System.Exception e)
                {
                    _ = e.Message;
                    tcopy.RollBack();
                }
            }

            return symbol;
        }


        /// <summary>
        /// 创建基于线的模型
        /// </summary>
        /// <param name="doc"> 项目文档</param>
        /// <param name="symbol"> 族类型</param>
        /// <param name="curves"> 定位线列表</param>
        /// <param name="count"> 创建完成个数</param>
        /// <param name="structuralType"> 结构类型</param>
        /// <returns> 族实例列表</returns>
        public static FamilyInstance NewInstanceByCurve(this Document doc, FamilySymbol symbol, Curve curve, StructuralType structuralType = StructuralType.Beam)
        {
            FamilyInstance instance = null;
            #region 创建实例并更新参数
            Transaction transaction = new Transaction(doc, "创建实例");
            transaction.Start();
            transaction.NoFailure();
            if(!symbol.IsActive)
            {
                symbol.Activate();
            }
            try
            {
                var level = doc.ActiveView.GenLevel;
                instance = doc.Create.NewFamilyInstance(curve, symbol, level, structuralType);
                // instance.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM);
                transaction.Commit();
            }
            catch
            {
                transaction.RollBack();
            }
            #endregion

            return instance;
        }
    }
}
