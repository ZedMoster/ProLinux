using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace Xml.HybhDocument
{
    public static class HybhUIDocument
    {
        /// <summary>
        /// 获取族类型名称族的类型  建议：外部开启事务组 TransactionGroup
        /// </summary>
        /// <param name="uidoc"> </param>
        /// <param name="familyName"> 族文件名称 FamilyName</param>
        /// <param name="name"> 族类型名称 FamilyName</param>
        /// <returns> 类型 FamilySymbol</returns>
        public static FamilySymbol GetFamilySymbol(this UIDocument uidoc, BuiltInCategory builtInCategory,
            string familyName, string name, string familyPath)
        {
            // 定义变量
            FamilySymbol symbol = null;
            Document doc = uidoc.Document;
            // 获取所有的结构框架类型
            var elsType = new FilteredElementCollector(doc).OfCategory(builtInCategory).WhereElementIsElementType().ToList();

            // 判断是否需要载入族文件
            bool loadingFamily = elsType.Any(x => (x as FamilySymbol).FamilyName == familyName);
            if (loadingFamily)
            {
                // 通过族类型及名称获取指定的类型
                symbol = elsType.FirstOrDefault(x => (x as FamilySymbol).Name == name) as FamilySymbol;
                // 创建类型
                if (symbol == null)
                {
                    var elType = elsType.FirstOrDefault(x => (x as FamilySymbol).FamilyName == familyName) as ElementType;
                    Transaction tcopy = new Transaction(doc);
                    tcopy.Start("创建类型");
                    symbol = elType.Duplicate(name) as FamilySymbol;
                    tcopy.Commit();
                }
            }
            else
            {
                var family = doc.LoadRfaPath(familyPath, familyName);
                if (family == null)
                {
                    // rfa 文件载入错误
                    return symbol;
                }
                // 获取载入类型id
                var needId = family.GetFamilySymbolIds().FirstOrDefault(x => (doc.GetElement(x) as FamilySymbol).Name == name);
                if (needId != null)
                {
                    // 获取指定类型
                    symbol = doc.GetElement(needId) as FamilySymbol;
                }
                // 复制新类型
                if (symbol == null)
                {
                    Transaction tcopy = new Transaction(doc);
                    tcopy.Start("创建类型");
                    var elType = doc.GetElement(family.GetFamilySymbolIds().FirstOrDefault()) as FamilySymbol;
                    symbol = elType.Duplicate(name) as FamilySymbol;
                    tcopy.Commit();
                }
            }
            // 获取指定指定族的族类型
            return symbol;
        }

        /// <summary>
        /// 重新载入CAD图纸
        /// </summary>
        /// <param name="uidoc"></param>
        /// <param name="tranName"></param>
        /// <returns></returns>
        public static bool ReloadCAD(this UIDocument uidoc, string tranName)
        {
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;
            // 选择链接图纸信息
            Reference reference = null;

            #region 选择CAD图纸
            try
            {
                SelectionFilterCategoryEndsWith categoryEndsWith = new SelectionFilterCategoryEndsWith(".dwg");
                reference = sel.PickObject(ObjectType.Subelement, categoryEndsWith, "选择需要重新载入的CAD图纸");
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            if (reference == null)
            {
                return false;
            }
            #endregion

            // 获取图纸 CADLinkType
            var dwg = doc.GetElement(reference);
            var link = doc.GetElement(dwg.GetTypeId()) as CADLinkType;
            // 重新链接图纸
            Transaction t = new Transaction(doc, tranName);
            t.Start();
            link.Reload(new CADLinkOptions(true, doc.ActiveView.Id));
            t.Commit();

            return true;
        }

        /// <summary>
        /// 创建文字注释
        /// </summary>
        /// <param name="uidoc"></param>
        /// <param name="xyzs"></param>
        /// <returns></returns>
        public static bool TextNoteListXYZ(this UIDocument uidoc, List<XYZ> xyzs)
        {
            Document doc = uidoc.Document;
            Transaction tran = new Transaction(doc, "注释文字");
            tran.Start();

            #region 更新文字样式
            var noteType = doc.TCollector<TextNoteType>().First();
            noteType.get_Parameter(BuiltInParameter.TEXT_BOX_VISIBILITY).Set(0);
            noteType.get_Parameter(BuiltInParameter.LEADER_OFFSET_SHEET).Set(0);
            noteType.get_Parameter(BuiltInParameter.TEXT_BACKGROUND).Set(0);
            noteType.get_Parameter(BuiltInParameter.TEXT_SIZE).Set(1.0.MillimeterToFeet());
            #endregion

            int count = xyzs.Count;
            if (count == 0)
            {
                tran.RollBack();
                return false;
            }
            for (int i = 0; i < count; i++)
            {
                TextNote.Create(doc, doc.ActiveView.Id, xyzs[i], i.ToString(), noteType.Id);
            }
            tran.Commit();
            return true;
        }

        /// <summary>
        /// 创建屋顶
        /// </summary>
        /// <param name="uidoc"></param>
        /// <param name="footprint"></param>
        /// <param name="level"></param>
        /// <param name="roofType"></param>
        /// <param name="slopAngle"></param>
        public static void CreateRoof(this UIDocument uidoc, CurveArray footprint, Level level, RoofType roofType,
            double slopAngle)
        {
            Document doc = uidoc.Document;
            Transaction tran = new Transaction(doc, "创建屋顶");
            tran.Start();
            FootPrintRoof footprintRoof = doc.Create.NewFootPrintRoof(
                footprint, level, roofType, out ModelCurveArray footPrintToModelCurveMapping);
            ModelCurveArrayIterator iterator = footPrintToModelCurveMapping.ForwardIterator();
            iterator.Reset();
            while (iterator.MoveNext())
            {
                ModelCurve modelCurve = iterator.Current as ModelCurve;
                footprintRoof.set_DefinesSlope(modelCurve, true);
                footprintRoof.set_SlopeAngle(modelCurve, Math.Tan(slopAngle.AngleToRadians()));
            }
            tran.Commit();
        }

        /// <summary>
        /// 幕墙嵌板随机材质
        /// </summary>
        /// <param name="uidoc"></param>
        public static void RandomMaterial(this UIDocument uidoc)
        {
            Document doc = uidoc.Document;
            IEnumerable<Element> materials = new FilteredElementCollector(doc).OfClass(typeof(Material)).Cast<Material>()
                .Where(q => q.MaterialClass == "未指定");

            using (Transaction t = new Transaction(doc, "Random-Materials"))
            {
                t.Start();
                foreach (Element e in new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_CurtainWallPanels))
                {
                    Parameter materialParam = e.LookupParameter("材质");
                    if (materialParam == null)
                        continue;
                    int randomNumber = new Random(Guid.NewGuid().GetHashCode()).Next(0, materials.Count());
                    materialParam.Set(materials.ElementAt(randomNumber).Id);
                }
                t.Commit();
            }
        }
    }
}
