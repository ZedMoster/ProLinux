using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class AutoCreateFloor : IExternalCommand
    {
        public Level LocalLevel { get; set; }
        public XYZ SelPoint { get; set; }
        public FloorType FloorType { get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            // 程序是否注册可运行
            if (Run.Running(Strings.key))
            {
                if (Run.Is3DViewCanNotWork(doc))
                {
                    return Result.Failed;
                }

                List<Element> allElements = new List<Element>();
                #region 选择构件
                if (sel.GetElementIds().Count != 0)
                {
                    foreach (var item in sel.GetElementIds())
                    {
                        List<string> allowCategory = new List<string>() { "结构柱", "结构框架", "墙" };
                        Element el = doc.GetElement(item);
                        if (allowCategory.Contains(el.Category.Name))
                        {
                            allElements.Add(el);
                        }
                    }
                }
                if (allElements.Count == 0)
                {
                    #region 获取所有的模型实体
                    // 过滤器-按类别
                    List<ElementFilter> filters = new List<ElementFilter>() {
                    new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming),
                    new ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns),
                    new ElementCategoryFilter(BuiltInCategory.OST_Walls), };
                    // 获取当前视图中可见且指定类别构件的列表
                    allElements = new FilteredElementCollector(doc, uidoc.ActiveView.Id).WherePasses(
                        new LogicalOrFilter(filters)).WhereElementIsNotElementType().ToElements().ToList();
                    #endregion
                }

                // 获取当前项目中的梁
                if (allElements.Count == 0)
                {
                    TaskDialog.Show("提示", "当前视图未找到梁柱是否可见");
                    return Result.Failed;
                }

                // 合并所有的结构实体
                SolidByUnion solidByUnion = new SolidByUnion();
                #endregion

                // 讲所有的模型构件的实体合并成一个solid
                var solidEls = solidByUnion.ByUnion(allElements);

                #region 创建平面与实体相交获取 surface
                var Z = doc.ActiveView.GenLevel.ProjectElevation;
                // 创建标高平面 方向为垂直向下
                Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ.Negate(), new XYZ(0, 0, Z));
                // 获取标高与平面相切的实体
                Solid cast = BooleanOperationsUtils.CutWithHalfSpace(solidEls, plane);

                PlanarFace CutFace = null;
                // 获取剪切后实体的上表面 ：XYZ.BasisZ
                foreach (Face face in cast.Faces)
                {
                    PlanarFace planFace = face as PlanarFace;
                    if (planFace == null) continue;
                    if (planFace.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ) && planFace.Origin.Z == Z)
                    {
                        CutFace = planFace;
                    }
                }
                if (CutFace == null)
                {
                    TaskDialog.Show("提示", "未知错误不能创建楼板");
                    return Result.Failed;
                }
                // 获取所有的边
                var curveLoops = CutFace.GetEdgesAsCurveLoops().ToList();

                List<CurveLoop> orderedcurveloops = curveLoops.OrderBy(l => l.GetExactLength()).ToList();
                orderedcurveloops.RemoveAt(orderedcurveloops.Count - 1);
                #endregion

                if (orderedcurveloops.Count == 0)
                {
                    TaskDialog.Show("提示", "梁柱所围成的区域不是空心");
                    return Result.Failed;
                }
                //TaskDialog.Show("0", orderedcurveloops.Count.ToString());

                // 获取 类型
                var ElementsType = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Floors).WhereElementIsElementType().ToElements();
                List<SelectElementByName> listData = new List<SelectElementByName>();
                // 获取所有的类型名称 
                foreach (Element el in ElementsType)
                {
                    var Familytype = el as FloorType;
                    listData.Add(new SelectElementByName { HybhElement = Familytype, HybhElName = Familytype.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString() + ":" + Familytype.Name });
                }

                // 获取用户输入参数
                WPFCreateFloorCAD wPF = new WPFCreateFloorCAD(listData);
                wPF.ShowDialog();
                if (wPF.IsHitTestVisible)
                {
                    TransactionGroup T = new TransactionGroup(doc, "一键楼板");
                    T.Start();
                    bool Push = false;

                    foreach (CurveLoop curveloop in orderedcurveloops)
                    {
                        #region 创建楼板需要使用CurveArray参数
                        var result = new CurveArray();
                        foreach (Curve curve in curveloop)
                        {
                            result.Append(curve);
                        }
                        #endregion

                        #region 设置楼板参数
                        // 偏移值
                        double.TryParse(wPF.elva.Text, out double elva);
                        // 厚度
                        double.TryParse(wPF.typeWidth.Text, out double _typeWidth);
                        bool isStruct = wPF.IsStruct.IsChecked.Value;
                        bool _newType = wPF.New.IsChecked.Value;
                        string _typeName = wPF.typeName.Text;
                        // 新建楼板类型
                        if (_newType)
                        {
                            // 新建类型
                            FloorType = CreateNewFloorType(doc, _typeName, UUtools.MillimetersToUnits(_typeWidth));
                        }
                        else
                        {
                            // 选择类型
                            FloorType = wPF.tp.SelectedValue as FloorType;
                        }
                        #endregion

                        #region 创建楼板
                        Transaction t = new Transaction(doc);
                        t.Start("新建楼板");
                        try
                        {
                            // 关闭警告
                            FailureHandlingOptions fho = t.GetFailureHandlingOptions();
                            fho.SetFailuresPreprocessor(new FailuresPreprocessor());
                            t.SetFailureHandlingOptions(fho);

                            var floor = doc.Create.NewFloor(result, FloorType, doc.ActiveView.GenLevel, isStruct);
                            floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).Set(UUtools.MillimetersToUnits(elva));

                            t.Commit();
                            Push = true;
                        }
                        catch
                        {
                            t.RollBack();
                        }
                        #endregion
                    }
                    #region 判断是否创建成功提交事务组
                    if (Push)
                    {
                        T.Assimilate();
                    }
                    else
                    {
                        T.RollBack();
                    }

                    #endregion
                }
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// 判断点是否在一个闭合轮廓内部
        /// </summary>
        /// <returns></returns>
        public bool IsInside(XYZ TargetPoint, CurveLoop curveloop)
        {
            int insertCount = 0;

            #region  判断点是否在一个闭合轮廓内:通过判断点的射线与边界相交点的个数来判断
            var TarPoint = new XYZ(TargetPoint.X, TargetPoint.Y, 0);
            Line rayLine = Line.CreateBound(TarPoint, TarPoint.Add(XYZ.BasisX * 10000));
            var i = curveloop.GetCurveLoopIterator();
            i.Reset();
            while (i.MoveNext())
            {
                var curve = i.Current;
                var line = Line.CreateBound(new XYZ(curve.GetEndPoint(0).X, curve.GetEndPoint(0).Y, 0),
                    new XYZ(curve.GetEndPoint(1).X, curve.GetEndPoint(1).Y, 0));
                var interResult = line.Intersect(rayLine);
                if (interResult == SetComparisonResult.Overlap)
                {
                    insertCount += 1;
                }

            }
            #endregion

            #region 如果次数为偶数就在外面，次数为奇数就在里面
            if (insertCount % 2 == 0)//偶数
            {
                return false;
            }
            else
            {
                return true;
            }
            #endregion
        }

        /// <summary>
        /// 创建依据楼板名称厚度创建新的类型
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="floorName"> 楼板类型名称</param>
        /// <param name="width"> 楼板厚度</param>
        /// <returns></returns>
        public FloorType CreateNewFloorType(Document doc, string floorName, double width)
        {
            Transaction trans = new Transaction(doc, "新建类型");
            trans.Start();

            // 创建滤器
            var floorTypes = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Floors).WhereElementIsElementType().ToList();
            // 判断是否已经存在该类型
            var floorIn = from tp in floorTypes
                          where tp.Name == floorName
                          select tp;

            if (floorIn.Count() == 0)
            {
                var newFloorType = floorTypes[0] as FloorType;
                // 创建新类型，使用Duplicate方法
                var newtype = newFloorType.Duplicate(floorName) as FloorType;
                // 获取材质id
                var materialId = newtype.GetCompoundStructure().GetMaterialId(0);
                // 创建复合结构
                var createSingleLayerCompoundStructure = CompoundStructure.CreateSingleLayerCompoundStructure(MaterialFunctionAssignment.Structure, width, materialId);
                createSingleLayerCompoundStructure.EndCap = EndCapCondition.NoEndCap;
                // 设置复合结构
                newtype.SetCompoundStructure(createSingleLayerCompoundStructure);
                trans.Commit();

                return newtype;
            }
            else
            {
                try
                {
                    // 已存在类型 更新厚度参数
                    var newFloorType = floorIn.ToList()[0] as FloorType;
                    var materialId = newFloorType.GetCompoundStructure().GetMaterialId(0);
                    var createSingleLayerCompoundStructure = CompoundStructure.CreateSingleLayerCompoundStructure(MaterialFunctionAssignment.Structure, width, materialId);
                    createSingleLayerCompoundStructure.EndCap = EndCapCondition.NoEndCap;
                    newFloorType.SetCompoundStructure(createSingleLayerCompoundStructure);
                    trans.Commit();
                    return newFloorType;
                }
                catch (Exception)
                {
                    trans.RollBack();
                    TaskDialog.Show("提示", "创建楼板类型出错！");
                    return null;
                }
            }
        }
    }
}
