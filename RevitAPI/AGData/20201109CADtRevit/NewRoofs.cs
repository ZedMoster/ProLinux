using System;
using System.Collections.Generic;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using CADReader.WPF;

namespace CADReader.Command
{
    [Transaction(TransactionMode.Manual)]
    class NewRoofs : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;
            var sel = uidoc.Selection;

            // 选择链接图纸信息
            Pickdwg pickdwg = new Pickdwg();

            #region 选择CAD图纸信息
            // 获取定位线
            pickdwg.Refer(sel, out Reference referenceModel);
            if (referenceModel == null)
            {
                MessageBox.Show("已取消");
                return Result.Cancelled;
            }
            // 获取图形
            CADElement CADModel = new CADElement();
            var models = CADModel.GetLayerCurves(doc, referenceModel);
            if (models.Count == 0)
                return Result.Failed;
            #endregion

            // 屋顶参数设置
            WPFRoof wPFRoof = new WPFRoof(doc);
            wPFRoof.ShowDialog();
            if (wPFRoof.IsHitTestVisible)
            {
                MessageBox.Show("已取消");
                return Result.Cancelled;
            }
            RoofManage roofManage = new RoofManage();
            #region 处理创建参数
            RoofType roofType;
            double.TryParse(wPFRoof.elva.Text, out double elva);
            if (wPFRoof.NewType.IsChecked.Value)
            {
                Material material = wPFRoof.MaterialList.SelectedValue as Material;
                string typeName = wPFRoof.NewTypeName.Text;
                double.TryParse(wPFRoof.NewTypeWidth.Text, out double width);
                roofType = roofManage.CreateNewType(doc, typeName, width, material);
            }
            else
                roofType = wPFRoof.ElementList.SelectedValue as RoofType;
            var level = doc.ActiveView.GenLevel?.NearLevel(true);
            if (level == null)
                throw new Exception("No level.");
            #endregion

            #region 创建屋顶
            List<bool> Push = new List<bool>();
            TransactionGroup T = new TransactionGroup(doc, "创建地形");
            T.Start();
            foreach (var item in models)
            {
                CurveArray curveArray = roofManage.ListCurveToArray(item.Curves);
                // 创建屋顶
                var push = roofManage.CreateRoof(doc, curveArray, level, roofType, elva);
                if (push)
                    Push.Add(push);
            }
            // 处理事务组
            _ = Push.Count == 0 ? T.RollBack() : T.Assimilate();
            #endregion
            MessageBox.Show($"总计屋顶组数：{Push.Count}");

            return Result.Succeeded;
        }



        ///// <summary>
        ///// 创建屋顶
        ///// </summary>
        ///// <param name="doc"></param>
        ///// <param name="footprint"></param>
        ///// <param name="level"></param>
        ///// <param name="roofType"></param>
        ///// <param name="slopAngle"></param>
        //private bool CreateRoof(Document doc, CurveArray footprint, Level level, RoofType roofType, double slopAngle)
        //{
        //    var push = true;
        //    Transaction tran = new Transaction(doc, "创建屋顶");
        //    tran.Start();
        //    try
        //    {
        //        ModelCurveArray footPrintToModelCurveMapping = new ModelCurveArray();
        //        FootPrintRoof footprintRoof = doc.Create.NewFootPrintRoof(footprint, level, roofType, out footPrintToModelCurveMapping);
        //        ModelCurveArrayIterator iterator = footPrintToModelCurveMapping.ForwardIterator();
        //        iterator.Reset();
        //        while (iterator.MoveNext())
        //        {
        //            ModelCurve modelCurve = iterator.Current as ModelCurve;
        //            footprintRoof.set_DefinesSlope(modelCurve, true);
        //            footprintRoof.set_SlopeAngle(modelCurve, Math.Tan(slopAngle.AngleToRadians()));
        //        }
        //        tran.Commit();
        //    }
        //    catch (Exception)
        //    {
        //        tran.RollBack();
        //        push = false;
        //    }

        //    return push;
        //}

        ///// <summary>
        ///// 创建依据楼板名称厚度创建新的类型  存在相同的更新厚度参数
        ///// </summary>
        ///// <param name="doc"></param>
        ///// <param name="floorName"> 楼板类型名称</param>
        ///// <param name="width"> 楼板厚度</param>
        ///// <returns></returns>
        //public RoofType CreateNewType(Document doc, string typeName, double width, Material material)
        //{
        //    Transaction trans = new Transaction(doc, "新建类型");
        //    trans.Start();

        //    // 创建滤器
        //    var Types = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Roofs).WhereElementIsElementType().ToElements();
        //    // 判断是否已经存在该类型
        //    var oldFloorType = Types.Where(x => x.Name == typeName).ToList();

        //    if (oldFloorType.Count() == 0)
        //    {
        //        var newFloorType = Types[0] as RoofType;
        //        var newtype = newFloorType.Duplicate(typeName) as RoofType;
        //        var createSingleLayerCompoundStructure = CompoundStructure.CreateSingleLayerCompoundStructure(
        //            MaterialFunctionAssignment.Structure, width.MillimeterToFeet(), material.Id);
        //        createSingleLayerCompoundStructure.EndCap = EndCapCondition.NoEndCap;
        //        // 设置复合结构
        //        newtype.SetCompoundStructure(createSingleLayerCompoundStructure);
        //        trans.Commit();

        //        return newtype;
        //    }
        //    else
        //    {
        //        // 已存在类型 更新厚度参数
        //        var newType = oldFloorType.First() as RoofType;
        //        var createSingleLayerCompoundStructure = CompoundStructure.CreateSingleLayerCompoundStructure(
        //            MaterialFunctionAssignment.Structure, width, material.Id);
        //        createSingleLayerCompoundStructure.EndCap = EndCapCondition.NoEndCap;
        //        newType.SetCompoundStructure(createSingleLayerCompoundStructure);
        //        trans.Commit();
        //        return newType;
        //    }
        //}
    }
}