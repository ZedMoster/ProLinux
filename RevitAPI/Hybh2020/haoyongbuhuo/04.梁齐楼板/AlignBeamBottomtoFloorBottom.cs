using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace hybh
{
    /// <summary>
    /// 梁底平板底
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    class AlignBeamBottomtoFloorBottom : IExternalCommand
    {
        public double HightValue { get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            // 程序是否注册可运行
            if (Run.Running(Strings.key))
            {
                // 创建一个空列表
                List<Element> elementsBeams = new List<Element>();

                #region 选择板范围内结构梁
                // 提前选中
                if (sel.GetElementIds().Count != 0)
                {
                    // 已经选择了结构梁
                    foreach (var item in sel.GetElementIds())
                    {
                        Element el = doc.GetElement(item);
                        if (el.Category.Name == "结构框架")
                        {
                            elementsBeams.Add(el);
                        }
                    }
                }
                // 手动选择
                if (elementsBeams.Count == 0)
                {
                    try
                    {
                        var newfilter_beam = new PickByCategorySelectionFilter() { CategoryName = "结构框架" };
                        ToFinish toFinish = new ToFinish();
                        toFinish.Subscribe();
                        SelPick.SelRefsList = sel.PickObjects(ObjectType.Element, newfilter_beam, "<选择需要对其板底的梁>").ToList();
                        toFinish.Unsubscribe();
                    }
                    catch { SelPick.SelRefsList = new List<Reference>(); }
                    // 是否选择了结构梁
                    if (SelPick.SelRefsList.Count == 0)
                    {
                        return Result.Cancelled;
                    }
                    // 获取sel 结构梁列表
                    foreach (var elId in SelPick.SelRefsList)
                    {
                        Element el = doc.GetElement(elId);
                        elementsBeams.Add(el);
                    }
                }
                #endregion

                #region 选择需要对齐到的楼板
                try
                {
                    var newfilter_floor = new PickByCategorySelectionFilter() { CategoryName = "楼板" };
                    SelPick.SelRef = sel.PickObject(ObjectType.Element, newfilter_floor, "<选择梁需平齐的楼板>");
                }
                catch { }

                if (SelPick.SelRef == null)
                {
                    return Result.Cancelled;
                }
                #endregion

                // 获取楼板
                var el_B = doc.GetElement(SelPick.SelRef) as Floor;

                // 获取梁的定位点
                GetElementLocationCurvePoints getElCurvePoints = new GetElementLocationCurvePoints();
                // 模型显示颜色
                ColorWithModel colorWithModel = new ColorWithModel();

                // 构件添加颜色
                GetRegistryBackgroundColor backgroundColor = new GetRegistryBackgroundColor
                {
                    // 问题 -- 橘色
                    _r = 255,
                    _g = 70,
                    _b = 0
                };
                Color color = backgroundColor.BackgroundColor("F1_r", "F1_g", "F1_b");
                // 是否存在调整成功的情况
                List<bool> GoodJob = new List<bool>();
                // 设置事务组
                TransactionGroup T = new TransactionGroup(doc);
                T.Start("梁顶平板面");

                RegistryStorage Registry = new RegistryStorage();
                // 读取梁参数名称
                var h_string = Registry.OpenAfterStart("BeamH")?? "h";
                foreach (var el in elementsBeams)
                {
                    Transaction t = new Transaction(doc);
                    t.Start("计算梁具板面距离");
                    try
                    {
                        #region 关闭警告
                        // 关闭警告
                        FailureHandlingOptions fho = t.GetFailureHandlingOptions();
                        fho.SetFailuresPreprocessor(new FailuresPreprocessor());
                        t.SetFailureHandlingOptions(fho);
                        #endregion

                        #region 计算距离参数
                        // 梁的偏移值 Z_OFFSET_VALUE 初始化计算结果才准确
                        var old_z = el.get_Parameter(BuiltInParameter.Z_OFFSET_VALUE).Set(0.0);

                        // 获取 结构框架 定位线的两个点
                        var twoPoints = getElCurvePoints.CurvePoints(el);
                        var p0 = twoPoints[0];
                        var p1 = twoPoints[1];

                        PointDistanceSolidFace pointDistanceSolidFace = new PointDistanceSolidFace();
                        var dir = new XYZ(0, 0, 1);
                        // 点的投影结果 IntersectionResult
                        var result0 = pointDistanceSolidFace.GetPointDistanceFace(el_B, p0, dir, true);
                        var result1 = pointDistanceSolidFace.GetPointDistanceFace(el_B, p1, dir, true);

                        var old0 = el.get_Parameter(BuiltInParameter.STRUCTURAL_BEAM_END0_ELEVATION).AsDouble();
                        var old1 = el.get_Parameter(BuiltInParameter.STRUCTURAL_BEAM_END1_ELEVATION).AsDouble();


                        // 获取梁需要修改的两个参数 parameter
                        var ParameterOne = el.get_Parameter(BuiltInParameter.STRUCTURAL_BEAM_END0_ELEVATION);
                        var ParameterTwo = el.get_Parameter(BuiltInParameter.STRUCTURAL_BEAM_END1_ELEVATION);
                        #endregion

                        #region 端点0 到面的距离结果不是null
                        if (result0 != null)
                        {
                            // IntersectionResult-Distance 距离值-三维坐标点
                            var value0 = result0.Distance;
                            var z0 = result0.IntersectPoint;

                            // 设置梁参数
                            if (z0.Z < p0.Z)
                            {
                                ParameterOne.Set(old0 - value0);
                            }
                            else
                            {
                                ParameterOne.Set(value0 + old0);
                            }

                        }
                        #endregion

                        #region 端点1 到面的距离结果不是null
                        if (result1 != null)
                        {
                            var value1 = result1.Distance;
                            var z1 = result1.IntersectPoint;

                            if (z1.Z < p1.Z)
                            {
                                ParameterTwo.Set(old1 - value1);
                            }
                            else
                            {
                                ParameterTwo.Set(value1 + old1);
                            }
                        }
                        #endregion

                        #region 设置Z 轴偏移值 为梁的高度
                        try
                        {
                            // 获取梁类型参数 H
                            var elType = (el as FamilyInstance).Symbol;
                            HightValue = elType.LookupParameter(h_string).AsDouble();
                        }
                        catch
                        {
                            // 获取梁实例参数 高度
                            HightValue = el.LookupParameter(h_string).AsDouble();
                        }
                        el.get_Parameter(BuiltInParameter.Z_OFFSET_VALUE).Set(HightValue);
                        #endregion

                        #region 恢复图形当前视图替换
                        // 恢复图形当前视图替换
                        var overrideGraphicSettings = new OverrideGraphicSettings();
                        var activeView = uidoc.ActiveView;
                        // 图形替换恢复
                        activeView.SetElementOverrides(el.Id, overrideGraphicSettings);
                        // 完成 提交事务
                        #endregion

                        t.Commit();

                        ////创建事务--拓展储存
                        //storeDataCreate.StoreDataWriteDouble(schema, entity, el, key, el_B_height);
                    }
                    // 计算错误自动未模型添加颜色
                    catch
                    {
                        // 错误 返回事务
                        t.RollBack();
                        // 设置注释参数 可空
                        // var value = "梁参数计算错误";
                        // 计算失败 标记颜色
                        colorWithModel.ElementSetError(uidoc, doc, el, color);
                        GoodJob.Add(false);
                    }
                }
                // 正常运行
                T.Assimilate();

                if (GoodJob.Count != 0)
                {
                    TaskDialog.Show("提示", "需要全局参数设置梁高度参数的名称\n且梁端部定位点需要在板面的投影范围内");
                }
            }

            return Result.Succeeded;
        }

    }
}
