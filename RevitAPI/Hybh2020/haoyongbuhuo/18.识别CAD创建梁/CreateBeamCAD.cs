using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class CreateBeamCAD : IExternalCommand
    {
        // 注册列表缓存数据
        readonly RegistryStorage RegistryStorage = new RegistryStorage();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIApplication uiapp = commandData.Application;
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
                // 选择链接CAD图纸
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
                    #region 获取多段线中点击的线段
                    var polyLine = geoObj as PolyLine;
                    var coordinates = polyLine.GetCoordinates().ToList();

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
                    SelPick.SelRef_two = sel.PickObject(ObjectType.PointOnElement, CADFilter, "<请选择梁图层对应的标注名称及尺寸>");
                }
                catch { }

                var cadText = cad.GetCADText(SelPick.SelRef_two, doc);
                if (cadText == "--")
                {
                    TaskDialog.Show("提示", "CAD图纸文字识别错误");
                    return Result.Failed;
                }
                #endregion

                TransactionGroup T = new TransactionGroup(doc);
                T.Start("CAD转梁");
                // 事务参数
                bool Push = false;
                // 读取梁参数名称
                var b_string = RegistryStorage.OpenAfterStart("BeamB") ?? "b";
                var h_string = RegistryStorage.OpenAfterStart("BeamH") ?? "h";

                #region 集中标注
                // 集中标注 关键字-L
                if (cadText.Contains('L'))
                {
                    // 分隔字符串
                    var text = cadText.Split(' ', '(', 'x', 'X');

                    #region KL1(3) 200X500
                    if (text.Length == 4)
                    {
                        var familyName = text[0];
                        // 保存本次识别的 集中标注的信息
                        RegistryStorage.SaveBeforeExit("BEAM_NAME_18", familyName);
                        RegistryStorage.SaveBeforeExit("BEAM_NAME_18_B", text[2]);
                        RegistryStorage.SaveBeforeExit("BEAM_NAME_18_H", text[3]);
                        // 文字参数转double类型
                        double.TryParse(text[2], out double width);
                        double.TryParse(text[3], out double heigh);
                        // 获取族类型
                        var symbol = twoPoint.GetFamilySymbol(doc, familyName);
                        // 创建族实例
                        var instance = twoPoint.CreateBeamWithTwoPoints(doc, symbol, level, p1, p2);
                        if (instance != null)
                        {
                            // 更新实例或类型参数
                            Push = twoPoint.UpDateInstanceOrSymbolParater(doc, instance, b_string, h_string, width, heigh);
                        }
                    }
                    #endregion

                    #region KL1 200X500
                    else if (text.Length == 3)
                    {
                        var familyName = text[0];
                        // 保存本次识别的 集中标注的信息
                        RegistryStorage.SaveBeforeExit("BEAM_NAME_18", familyName);
                        RegistryStorage.SaveBeforeExit("BEAM_NAME_18_B", text[1]);
                        RegistryStorage.SaveBeforeExit("BEAM_NAME_18_H", text[2]);
                        // 文字参数转double类型
                        double.TryParse(text[1], out double width);
                        double.TryParse(text[2], out double heigh);
                        // 获取族类型
                        var symbol = twoPoint.GetFamilySymbol(doc, familyName);
                        // 创建族实例
                        var instance = twoPoint.CreateBeamWithTwoPoints(doc, symbol, level, p1, p2);
                        if (instance != null)
                        {
                            // 更新实例或类型参数
                            Push = twoPoint.UpDateInstanceOrSymbolParater(doc, instance, b_string, h_string, width, heigh);
                        }
                    }
                    #endregion

                    #region KL1(3)
                    else if (text.Length == 2 || text.Length == 1)
                    {
                        // 保存本次识别的 集中标注的信息
                        string familyName = RegistryStorage.OpenAfterStart("BEAM_NAME_18");
                        string B = RegistryStorage.OpenAfterStart("BEAM_NAME_18_B");
                        string H = RegistryStorage.OpenAfterStart("BEAM_NAME_18_H");
                        if (text[0] == familyName)
                        {
                            // 文字参数转double类型
                            double.TryParse(B, out double width);
                            double.TryParse(H, out double heigh);
                            // 获取族类型
                            var symbol = twoPoint.GetFamilySymbol(doc, familyName);
                            // 创建族实例
                            var instance = twoPoint.CreateBeamWithTwoPoints(doc, symbol, level, p1, p2);
                            if (instance != null)
                            {
                                // 更新实例或类型参数
                                Push = twoPoint.UpDateInstanceOrSymbolParater(doc, instance, b_string, h_string, width, heigh);
                            }
                        }
                        else
                        {
                            TaskDialog.Show("提示", "上一次创建的梁编号与本次识别的梁编号不同");
                        }
                    }
                    #endregion

                    else
                    {
                        TaskDialog.Show("集中标注", "梁编号规则不在程序设定范围");
                    }
                }
                #endregion

                #region 原位标注
                else
                {
                    var text = cadText.Split('x', 'X');
                    if (text.Length == 2)
                    {
                        // 查看注册列表是否存在名称
                        string familyName = RegistryStorage.OpenAfterStart("BEAM_NAME_18");
                        if (familyName == null)
                        {
                            familyName = cadText.Replace('X', 'x');
                        }
                        if (familyName != null)
                        {
                            double.TryParse(text[0], out double width);
                            double.TryParse(text[1], out double heigh);
                            var symbol = twoPoint.GetFamilySymbol(doc, familyName);
                            var instance = twoPoint.CreateBeamWithTwoPoints(doc, symbol, level, p1, p2);
                            if (instance != null)
                            {
                                // 更新实例或类型参数
                                Push = twoPoint.UpDateInstanceOrSymbolParater(doc, instance, b_string, h_string, width, heigh);
                            }
                        }
                    }
                    else
                    {
                        TaskDialog.Show("原位标注", "梁编号规则不在程序设定范围");
                    }
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
