using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using RvtTxt;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Structure;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class CreateBeamsOffsetCAD : IExternalCommand
    {
        readonly RegistryStorage RegistryStorage = new RegistryStorage();
        public string Name { get; set; }
        public string B { get; set; }
        public string H { get; set; }
        public string Z { get; set; }
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

                // 获取标高
                var level = doc.ActiveView.GenLevel;
                PickCADFilter CADFilter = new PickCADFilter();

                // 梁定位点
                XYZ p1 = null;
                XYZ p2 = null;

                #region 选择梁定位线
                try
                {
                    SelPick.SelRef = sel.PickObject(ObjectType.PointOnElement, CADFilter, "请点击选梁定位线");
                }
                catch { }

                if (SelPick.SelRef == null)
                {
                    return Result.Cancelled;
                }
                #endregion

                // 方法集合
                NewFamilyInstanceTwoPoint twoPoint = new NewFamilyInstanceTwoPoint();

                #region 获取梁定位点
                // 定位点
                var selPoint = SelPick.SelRef.GlobalPoint;
                ImportInstance dwg = doc.GetElement(SelPick.SelRef) as ImportInstance;
                var geoObj = dwg.GetGeometryObjectFromReference(SelPick.SelRef);
                Transform transform = dwg.GetTransform();

                var dir = new XYZ(0, 0, level.ProjectElevation);
                if (geoObj is PolyLine)
                {
                    // 选择的定位线为多段线的时候
                    var polyLine = geoObj as PolyLine;
                    var coordinates = polyLine.GetCoordinates().ToList();

                    #region 遍历多段线的点创建线段判断全局pick点在不在线段上
                    for (int i = 0; i < coordinates.Count - 1; i++)
                    {
                        if (i < coordinates.Count - 2)
                        {
                            XYZ start = transform.OfPoint(coordinates[i]);
                            XYZ end = transform.OfPoint(coordinates[i + 1]);
                            // 判断点再线段上
                            if (twoPoint.IsPointOnLine(start, end, selPoint))
                            {
                                p1 = start + dir;
                                p2 = end + dir;
                            }
                        }
                        else
                        {
                            // 特殊情况线段只有三个点算法不满足条件
                            if (coordinates.Count == 3)
                            {
                                XYZ _p0 = transform.OfPoint(coordinates[0]);
                                XYZ _p1 = transform.OfPoint(coordinates[1]);
                                XYZ _p2 = transform.OfPoint(coordinates[2]);
                                if (twoPoint.IsPointOnLine(_p0, _p1, selPoint))
                                {
                                    p1 = _p0 + dir;
                                    p2 = _p1 + dir;
                                }
                                else
                                {
                                    p1 = _p1 + dir;
                                    p2 = _p2 + dir;
                                }
                            }
                            else
                            {
                                XYZ start = transform.OfPoint(coordinates[i]);
                                XYZ end = transform.OfPoint(coordinates[0]);
                                // 判断点再线段上
                                if (twoPoint.IsPointOnLine(start, end, selPoint))
                                {
                                    p1 = start + dir;
                                    p2 = end + dir;
                                }
                            }

                        }

                    }
                    #endregion

                }
                else if (geoObj is Line)
                {
                    var line = geoObj as Line;
                    var coordinates = line.Tessellate().ToList();
                    p1 = transform.OfPoint(coordinates[0]) + dir; // 起点
                    p2 = transform.OfPoint(coordinates[1]) + dir; // 终点
                }
                if (p1 == null || p2 == null)
                {
                    TaskDialog.Show("提示", "梁获取定位线错误！");
                    return Result.Failed;
                }
                #endregion

                #region 获取梁的编号信息
                CAD cad = new CAD();
                // 获取选中 梁 标注的信息
                try
                {
                    SelPick.SelRef = sel.PickObject(ObjectType.PointOnElement, CADFilter, "<请选择 梁名称/尺寸>");
                    SelPick.SelRef_two = sel.PickObject(ObjectType.PointOnElement, CADFilter, "<请选择 梁尺寸/名称>");
                    SelPick.SelRef_three = sel.PickObject(ObjectType.PointOnElement, CADFilter, "<请选择 梁尺寸/名称>");
                }
                catch { }
                var cadName = cad.GetCADText(SelPick.SelRef, doc);
                var cadPara = cad.GetCADText(SelPick.SelRef_two, doc);
                var cadHz = cad.GetCADText(SelPick.SelRef_three, doc);
                if (cadName == "--" || cadPara == "--" || cadHz == "--")
                {
                    TaskDialog.Show("提示", "CAD图纸梁标注规则：\n--KL1(1)\n--300x600\n--(Hg+0.100)");
                    return Result.Failed;
                }
                #endregion

                #region --KL3 and --300X600
                if (cadName.Contains('L'))
                {
                    Name = cadName.Split('(')[0];
                    B = cadPara.Split('x', 'X')[0];
                    H = cadPara.Split('x', 'X')[1];
                }
                else
                {
                    Name = cadPara.Split('(')[0];
                    B = cadName.Split('x', 'X')[0];
                    H = cadName.Split('x', 'X')[1];
                }
                #endregion

                if (cadHz.Contains('+'))
                {
                    Z = cadHz.Split('+').Last().Replace("]", "").Replace(")", "");
                }
                else
                {
                    Z = "-" + cadHz.Split('-').Last().Replace("]", "").Replace(")", "");
                }

                // 保存本次识别的 集中标注的信息
                RegistryStorage.SaveBeforeExit("BEAM_NAME_18", Name);
                RegistryStorage.SaveBeforeExit("BEAM_NAME_18_B", B);
                RegistryStorage.SaveBeforeExit("BEAM_NAME_18_H", H);
                // 文字参数转double类型
                double.TryParse(B, out double width);
                double.TryParse(H, out double heigh);
                double.TryParse(Z, out double zh);

                TransactionGroup T = new TransactionGroup(doc);
                T.Start("CAD梁");
                bool Push = false;

                #region 创建梁实例
                // 读取梁参数名称
                var b_string = RegistryStorage.OpenAfterStart("BeamB") ?? "b";
                var h_string = RegistryStorage.OpenAfterStart("BeamH") ?? "h";
                try
                {
                    // 获取族类型
                    var symbol = twoPoint.GetFamilySymbol(doc, Name);
                    // 创建族实例
                    var instance = twoPoint.CreateBeamWithTwoPoints(doc, symbol, level, p1, p2);
                    if (instance != null)
                    {
                        // 更新实例或类型参数
                        Push = twoPoint.UpDateInstanceOrSymbolParater(doc, instance, b_string, h_string, width, heigh, zh);
                    }
                }
                catch (Exception e)
                {
                    TaskDialog.Show("提示", e.Message + Strings.error);
                }
                #endregion

                #region 是否提交事务组
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

            return Result.Succeeded;
        }
    }
}
