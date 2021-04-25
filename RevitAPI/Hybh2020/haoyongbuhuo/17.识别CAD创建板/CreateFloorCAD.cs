using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using RvtTxt;
using System.Linq;
using Autodesk.Revit.Attributes;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class CreateFloorCAD : IExternalCommand
    {
        private IList<XYZ> coordinates;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            if (Run.Running(Strings.key))
            {
                if (Run.Is3DViewCanNotWork(doc))
                {
                    return Result.Failed;
                }

                // 获取标高
                var level = doc.ActiveView.GenLevel;
                // 选择CAD图纸
                PickCADFilter pickCADFilter = new PickCADFilter();

                try
                {
                    SelPick.SelRef = sel.PickObject(ObjectType.PointOnElement, pickCADFilter, "请点选连接的CAD轮廓线:");
                }
                catch { }
                if (SelPick.SelRef == null)
                {
                    return Result.Cancelled;
                }

                ImportInstance dwg = doc.GetElement(SelPick.SelRef) as ImportInstance;
                var geoObj = (dwg as Element).GetGeometryObjectFromReference(SelPick.SelRef);
                if (geoObj is PolyLine)
                {
                    var polyLine = geoObj as PolyLine;
                    coordinates = polyLine.GetCoordinates();
                }
                // 获取所有图形的 curveArray 用于创建 矩形柱
                var curveArray = GetCurveArray(coordinates, dwg);
                if (curveArray != null)
                {
                    TransactionGroup transactionGroup = new TransactionGroup(doc);
                    transactionGroup.Start("图纸转楼板");

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
                        // 偏移值
                        double.TryParse(wPF.elva.Text, out double elva);
                        // 厚度
                        double.TryParse(wPF.typeWidth.Text, out double _typeWidth);
                        var isT = wPF.IsStruct.IsChecked.Value;
                        var _newType = wPF.New.IsChecked.Value;
                        var _typeName = wPF.typeName.Text;

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
                        Transaction t = new Transaction(doc);
                        t.Start("创建楼板");

                        // 关闭警告
                        FailureHandlingOptions fho = t.GetFailureHandlingOptions();
                        fho.SetFailuresPreprocessor(new FailuresPreprocessor());
                        t.SetFailureHandlingOptions(fho);
                        try
                        {
                            var floor = doc.Create.NewFloor(curveArray, FloorType, level, isT);
                            floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).Set(UUtools.MillimetersToUnits(elva));
                            t.Commit();
                        }
                        catch
                        {
                            t.RollBack();
                        }
                    }

                    transactionGroup.Assimilate();
                }
                else
                {
                    TaskDialog.Show("提示", "选择的CAD图纸边界出错!");
                    return Result.Failed;
                }
            }

            return Result.Succeeded;
        }
        public FloorType FloorType { get; set; }

        /// <summary>
        /// 创建依据楼板名称厚度创建新的类型
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="floorName"> 楼板类型名称</param>
        /// <param name="width"> 楼板厚度</param>
        /// <returns></returns>
        private FloorType CreateNewFloorType(Document doc, string floorName, double width)
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


        /// <summary>
        /// 获取 CAD图纸 PolyLine
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="dwg"></param>
        /// <returns> GCurveArray</returns>
        private CurveArray GetCurveArray(IList<XYZ> coordinates, ImportInstance dwg)
        {
            try
            {
                CurveArray curveArray = new CurveArray();
                Transform transform = dwg.GetTransform();

                for (int i = 0; i < coordinates.Count - 1; i++)
                {
                    if (i < coordinates.Count - 2)
                    {
                        Line line = Line.CreateBound(transform.OfPoint(coordinates[i]), transform.OfPoint(coordinates[i + 1]));
                        curveArray.Append(line);
                    }
                    else
                    {
                        Line line2 = Line.CreateBound(transform.OfPoint(coordinates[i]), transform.OfPoint(coordinates[0]));
                        curveArray.Append(line2);
                    }
                }
                return curveArray;
            }
            catch (Exception e)
            {
                TaskDialog.Show("提示", e.Message + Strings.error );
                return null;
            }
        }

        /// <summary>
        /// 获得CAD文字
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private string GetCADText(Reference reference, Document doc)
        {
            try
            {
                var selPo = reference.GlobalPoint;
                var CADLinkInstance = doc.GetElement(reference) as Instance;
                var textType = new FilteredElementCollector(doc).OfClass(typeof(TextNoteType)).Cast<TextNoteType>().ToList().FirstOrDefault();
                var styleId = -1;
                try
                {
                    styleId = int.Parse((doc.GetElement(CADLinkInstance.GetGeometryObjectFromReference(reference).GraphicsStyleId) as GraphicsStyle).Id.ToString());
                }
                catch { }

                var geoNode = new CGeoNode();
                var element = doc.GetElement(reference);
                var geo = element.GetGeometryObjectFromReference(new Reference(element));
                var textNodes = new List<CTextNode>();
                geoNode.ParaseGeoText(geo, textNodes);
                var type = textNodes.FirstOrDefault().GetType();
                XYZ po = new XYZ();
                string str = "";
                double num = double.MaxValue;
                foreach (var textNode in textNodes)
                {
                    try
                    {
                        if (textNode.m_idStyle != styleId)
                            continue;

                        var cPoint = textNode.GetType().GetField("m_pt1").GetValue(textNode) as CPointMen;
                        var point = new XYZ(cPoint.m_dx, cPoint.m_dy, cPoint.m_dz);
                        var num1 = point.DistanceTo(selPo);
                        if (num1 < num)
                        {
                            num = num1;
                            str = textNode.m_sValue;
                            po = point;
                        }
                    }
                    catch { continue; }
                }
                return str;
            }
            catch
            {
                return "";
            }
        }
    }
}
