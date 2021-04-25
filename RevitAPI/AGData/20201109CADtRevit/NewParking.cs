using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace CADReader.Command
{
    [Transaction(TransactionMode.Manual)]
    class NewParking : IExternalCommand
    {
        const string FamilyName = "停车位.rfa";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

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
            // 文字信息
            pickdwg.Refer(sel, out Reference ReferenceText);
            if (ReferenceText == null)
            {
                MessageBox.Show("已取消");
                return Result.Cancelled;
            }
            #endregion

            // 获取图形
            CADElement CADModel = new CADElement();
            // 获取编号
            CADText CADtext = new CADText();
            var models = CADModel.GetLayerGetCoordinates(doc, referenceModel);
            var texts = CADtext.GetLayer(doc, ReferenceText);
            // 定义文字列表
            List<int> noSlide = new List<int>();
            // 矩形多段线
            if (models.Count > 0 && texts.Count > 0)
            {
                TransactionGroup T = new TransactionGroup(doc, "矩形停车位识别");
                T.Start();
                List<bool> Push = new List<bool>();
                ParkingManage parking = new ParkingManage(FamilyName);
                bool errorElement = false;
                try
                {
                    foreach (var item in models)
                    {
                        if (item.XYZs.Count == 5)
                        {
                            var xyzs = item.XYZs;
                            // 获取尺寸参数
                            var p0 = xyzs[0];
                            var p1 = xyzs[1];
                            var p2 = xyzs[2];
                            var locationPoint = (p0 + p2) / 2;
                            // 尺寸参数
                            double b = p1.DistanceTo(p2);
                            double h = p1.DistanceTo(p0);

                            #region 获取内部停车位编号名称
                            string cateNum = "停车位";
                            string number = "";
                            for (int i = 0; i < texts.Count; i++)
                            {
                                if (noSlide.Contains(i))
                                    continue;
                                if (parking.IsInside(texts[i].Location, xyzs))
                                {
                                    number = texts[i].Text;
                                    noSlide.Add(i);
                                    break;
                                }
                            }
                            var type = parking.GetFamilySymbol(doc, BuiltInCategory.OST_Parking, FamilyName.Split('.')[0], cateNum);
                            if (type == null)
                                break;
                            #endregion

                            #region 创建族文件更新参数
                            Transaction tranC = new Transaction(doc, "创建模型");
                            tranC.Start();
                            // 创建实例
                            if (!type.IsActive)
                                type.Activate();
                            var instance = doc.Create.NewFamilyInstance(locationPoint, type, doc.ActiveView.GenLevel, StructuralType.NonStructural);
                            // 更新参数
                            var push = parking.UpdateParatemer(instance, locationPoint, xyzs, b, h, number);
                            tranC.Commit();
                            Push.Add(push);
                            #endregion
                        }
                        else if (item.XYZs.Count == 13)
                        {
                            #region 点位分区
                            // 1#
                            List<XYZ> xyzs1 = new List<XYZ>
                            {
                                item.XYZs.ElementAt(0),
                                item.XYZs.ElementAt(1),
                                item.XYZs.ElementAt(2),
                                item.XYZs.ElementAt(11),
                                item.XYZs.ElementAt(0)
                            };
                            // 2#
                            List<XYZ> xyzs2 = new List<XYZ>
                            {
                                item.XYZs.ElementAt(1),
                                item.XYZs.ElementAt(4),
                                item.XYZs.ElementAt(3),
                                item.XYZs.ElementAt(2),
                                item.XYZs.ElementAt(1)
                            };
                            // 3#
                            List<XYZ> xyzs3 = new List<XYZ>
                            {
                                item.XYZs.ElementAt(4),
                                item.XYZs.ElementAt(5),
                                item.XYZs.ElementAt(6),
                                item.XYZs.ElementAt(3),
                                item.XYZs.ElementAt(4)
                            };
                            #endregion

                            var push1 = parking.CreateRectangleParking(doc, xyzs1, texts);
                            var push2 = parking.CreateRectangleParking(doc, xyzs2, texts);
                            var push3 = parking.CreateRectangleParking(doc, xyzs3, texts);
                            Push.Add(push1);
                            Push.Add(push2);
                            Push.Add(push3);
                        }
                        else if (item.XYZs.Count == 9)
                        {
                            #region 点位分区
                            // 1#
                            List<XYZ> xyzs1 = new List<XYZ>
                            {
                                item.XYZs.ElementAt(0),
                                item.XYZs.ElementAt(1),
                                item.XYZs.ElementAt(2),
                                item.XYZs.ElementAt(3),
                                item.XYZs.ElementAt(0)
                            };
                            // 2#
                            List<XYZ> xyzs2 = new List<XYZ>
                            {
                                item.XYZs.ElementAt(3),
                                item.XYZs.ElementAt(4),
                                item.XYZs.ElementAt(5),
                                item.XYZs.ElementAt(6),
                                item.XYZs.ElementAt(3)
                            };
                            #endregion
                            var push1 = parking.CreateRectangleParking(doc, xyzs1, texts);
                            var push2 = parking.CreateRectangleParking(doc, xyzs2, texts);
                            Push.Add(push1);
                            Push.Add(push2);
                        }
                        else
                            errorElement = true;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    if (errorElement)
                        MessageBox.Show("存在未知图形！");
                }

                // 处理事务组
                _ = Push.Count == 0 ? T.RollBack() : T.Assimilate();
            }

            return Result.Succeeded;
        }
    }
}
