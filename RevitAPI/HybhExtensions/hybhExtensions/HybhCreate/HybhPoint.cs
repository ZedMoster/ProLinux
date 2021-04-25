using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace Xml
{
    public static class HybhPointExtensions
    {
        /// <summary>
        /// 创建基于点的模型
        /// </summary>
        /// <param name="doc"> 项目文档</param>
        /// <param name="symbol"> 族类型</param>
        /// <param name="xyzs"> 定位点列表</param>
        /// <param name="count"> 创建完成个数</param>
        /// <param name="structuralType"> 结构类型</param>
        /// <returns> 族实例列表</returns>
        public static List<Element> NewInstancePoints(this Document doc, FamilySymbol symbol, List<XYZ> xyzs, out int count,
            StructuralType structuralType = StructuralType.NonStructural)
        {
            List<Element> elements = new List<Element>();
            count = 0;
            foreach (var xyz in xyzs)
            {
                #region 创建实例并更新参数
                Transaction transaction = new Transaction(doc);
                transaction.Start("创建实例");
                transaction.NoFailure();
                if (!symbol.IsActive)
                {
                    symbol.Activate();
                }
                try
                {
                    FamilyInstance instance = doc.Create.NewFamilyInstance(xyz.Flatten(), symbol, doc.ActiveView.GenLevel, structuralType);
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
        /// 基于点创建模型文字
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="xyzs"></param>
        public static void NewTextNote(this Document doc, List<XYZ> xyzs)
        {
            Transaction t = new Transaction(doc, "创建文字");
            t.Start();
            int count = xyzs.Count;
            if (count == 0)
            {
                t.RollBack();
            }
            else
            {
                #region 更新文字样式
                var noteType = doc.TCollector<TextNoteType>().First();
                noteType.get_Parameter(BuiltInParameter.TEXT_BOX_VISIBILITY).Set(0);
                noteType.get_Parameter(BuiltInParameter.LEADER_OFFSET_SHEET).Set(0);
                noteType.get_Parameter(BuiltInParameter.TEXT_BACKGROUND).Set(0);
                noteType.get_Parameter(BuiltInParameter.TEXT_SIZE).Set(1.0.MillimeterToFeet());
                #endregion

                for (int i = 0; i < count; i++)
                {
                    TextNote.Create(doc, doc.ActiveView.Id, xyzs[i], i.ToString(), noteType.Id);
                }
                t.Commit();
            }
        }
    }
}
