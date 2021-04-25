using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class AlignColumnBaseWithFloor : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            // 程序是否注册可运行
            if (Run.Running(Strings.key))
            {
                // 创建一个空列表
                List<Element> elementColumns = new List<Element>();

                #region 选择构件
                // 判断当前视口是否已选中构件
                if (sel.GetElementIds().Count != 0)
                {
                    foreach (var item in sel.GetElementIds())
                    {
                        Element el = doc.GetElement(item);
                        if (el.Category.Name == "结构柱")
                        {
                            elementColumns.Add(el);
                        }
                    }
                }

                // 判断是列表是否存在 构件
                if (elementColumns.Count == 0)
                {
                    try
                    {
                        // 选择过滤器
                        var filter_Column = new PickByCategorySelectionFilter
                        {
                            CategoryName = "结构柱"
                        };
                        // 右键完成多选
                        ToFinish toFinish = new ToFinish();
                        toFinish.Subscribe();
                        SelPick.SelRefsList = sel.PickObjects(ObjectType.Element, filter_Column, "<选择需要对其板面的结构柱>").ToList();
                        toFinish.Unsubscribe();
                    }
                    catch { SelPick.SelRefsList = new List<Reference>(); }

                    if (SelPick.SelRefsList.Count == 0)
                    {
                        return Result.Cancelled;
                    }
                    foreach (var elId in SelPick.SelRefsList)
                    {
                        Element el = doc.GetElement(elId);
                        elementColumns.Add(el);
                    }
                }
                #endregion

                try
                {
                    var filter_Floor = new PickByCategorySelectionFilter() { CategoryName = "楼板" };
                    SelPick.SelRef = sel.PickObject(ObjectType.Element, filter_Floor, "<选择梁需平齐的楼板板面>");
                }
                catch { }
                if (SelPick.SelRef == null)
                {
                    return Result.Cancelled;
                }


                var el_B = doc.GetElement(SelPick.SelRef);
                // 模型显示颜色
                ColorWithModel colorWithModel = new ColorWithModel();

                // 设置事务组
                TransactionGroup T = new TransactionGroup(doc);
                T.Start("柱底平板面");

                // 创建一个存储计算错误构件的 列表ids
                List<ElementId> elementError = new List<ElementId>();

                foreach (var el in elementColumns)
                {
                    // 获取结构柱分析模型的定位点 列表
                    var twoPoints = GetAnalyCurve.GetCurveTessellate(doc, el);
                    if (twoPoints.Count == 0)
                    {
                        elementError.Add(el.Id);
                        continue;
                    }
                    Transaction t = new Transaction(doc);
                    t.Start("计算柱距板面");

                    // 关闭警告
                    FailureHandlingOptions fho = t.GetFailureHandlingOptions();
                    fho.SetFailuresPreprocessor(new FailuresPreprocessor());
                    t.SetFailureHandlingOptions(fho);

                    // 柱底平板面
                    XYZ p0 = twoPoints[0];

                    #region 计算垂直距离
                    PointDistanceSolidFace pointDistanceSolidFace = new PointDistanceSolidFace();
                    var dir = new XYZ(0, 0, 1);
                    // 点的投影结果 IntersectionResult
                    var result0 = pointDistanceSolidFace.GetPointDistanceFace(el_B, p0, dir, false);
                    #endregion

                    #region 投影结果
                    if (result0 != null)
                    {
                        // IntersectionResult-Distance 距离值-三维坐标点
                        var distance = result0.Distance;
                        var z0 = result0.IntersectPoint;

                        // 待修改梁参数 底部偏移 SCHEDULE_BASE_LEVEL_OFFSET_PARAM
                        var _Parameter = el.get_Parameter(BuiltInParameter.SCHEDULE_BASE_LEVEL_OFFSET_PARAM);
                        var oldTOP = _Parameter.AsDouble();

                        if (p0.Z < z0.Z)
                        {
                            var _value = oldTOP + distance;
                            _Parameter.Set(_value);
                        }
                        else
                        {
                            var _value = oldTOP - distance;
                            _Parameter.Set(_value);
                        }

                        // 恢复图形当前视图替换
                        var overrideGraphicSettings = new OverrideGraphicSettings();
                        var activeView = uidoc.ActiveView;
                        // 图形替换恢复
                        activeView.SetElementOverrides(el.Id, overrideGraphicSettings);

                        // 完成 提交事务
                        t.Commit();
                    }
                    // 计算错误自动未模型添加颜色
                    else
                    {
                        // 错误 返回事务
                        t.RollBack();
                        // 构件添加颜色
                        GetRegistryBackgroundColor backgroundColor = new GetRegistryBackgroundColor
                        {
                            // No -- 橘色
                            _r = 255,
                            _g = 70,
                            _b = 0
                        };

                        Color color = backgroundColor.BackgroundColor("F1_r", "F1_g", "F1_b");
                        // 设置注释参数 可空
                        // var value = "梁参数计算错误";
                        // 计算失败 标记颜色
                        colorWithModel.ElementSetError(uidoc, doc, el, color);
                        // 正确的id 集合
                        elementError.Add(el.Id);
                    }
                    #endregion
                }

                // 正常运行
                T.Assimilate();
                // UI 选中运行完成的构件
                sel.SetElementIds(elementError);
            }

            return Result.Succeeded;
        }

        private double distance;

        public double Distance
        {
            get { return distance; }
            set { distance = value; }
        }

        public XYZ ofseXYZ { get; set; }
    }
}