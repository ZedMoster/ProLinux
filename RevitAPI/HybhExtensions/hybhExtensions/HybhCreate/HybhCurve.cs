using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace Xml
{
    public static class HybhCurveExtensions
    {
        /// <summary>
        /// 创建基于线的模型
        /// </summary>
        /// <param name="doc"> 项目文档</param>
        /// <param name="symbol"> 族类型</param>
        /// <param name="curves"> 定位线列表</param>
        /// <param name="count"> 创建完成个数</param>
        /// <param name="structuralType"> 结构类型</param>
        /// <returns> 族实例列表</returns>
        public static List<Element> NewInstanceCurves(this Document doc, FamilySymbol symbol, List<Curve> curves, out int count,
            StructuralType structuralType = StructuralType.Beam)
        {
            List<Element> elements = new List<Element>();
            count = 0;
            foreach (var curve in curves)
            {
                #region 创建实例并更新参数
                Transaction transaction = new Transaction(doc, "创建实例");
                transaction.Start();
                transaction.NoFailure();
                if (!symbol.IsActive)
                {
                    symbol.Activate();
                }
                try
                {
                    var level = doc.ActiveView.GenLevel;
                    FamilyInstance instance = doc.Create.NewFamilyInstance(curve, symbol, level, structuralType);
                    // instance.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM);
                    transaction.Commit();
                    elements.Add(instance);
                    count++;
                }
                catch
                {
                    transaction.RollBack();
                }
                #endregion
            }
            return elements;
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
        public static FamilyInstance NewInstanceByCurve(this Document doc, FamilySymbol symbol, Curve curve,
            StructuralType structuralType = StructuralType.Beam)
        {
            FamilyInstance instance = null;
            #region 创建实例并更新参数
            Transaction transaction = new Transaction(doc, "创建实例");
            transaction.Start();
            if (!symbol.IsActive)
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
