using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class CreateDuctFitting : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            // 程序是否注册可运行
            if (Run.Running(Strings.key))
            {
                // 统计个数num_Q
                int num_Q = 0;
                // 创建事务组
                TransactionGroup transactionGroup = new TransactionGroup(doc, "洞口布置");
                transactionGroup.Start();
                bool Push = false;

                // 载入所需的族文件
                DoLoadFamily doLoadFamily = new DoLoadFamily();
                try
                {
                    var Rfa = "DuctFitting";
                    var have1 = doLoadFamily.IsHaveFamily(doc, BuiltInCategory.OST_DuctFitting, "hybh风管洞口");
                    if (!have1)
                    {
                        doLoadFamily.LoadFamily(doc, Rfa + ".rfa", "hybh风管洞口");
                    }
                }
                catch
                {
                    TaskDialog.Show("提示", "需要使用指定族文件才能使用\n");
                    transactionGroup.RollBack();
                    return Result.Failed;
                }

                // 过滤当前项目中的链接文件
                var linkInstances = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType().ToElements().ToList();
                if (linkInstances.Count == 0)
                {
                    TaskDialog.Show("提示", "没有找到链接的机电模型文档\n需要将机电文档链接到土建模型（当前项目）中");
                    transactionGroup.RollBack();
                    return Result.Failed;
                }
                // // 获取 风管管件 族
                var elsDuctFittings = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctFitting).WhereElementIsElementType().ToElements().ToList();
                // 获取所有的标高
                var levels = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().ToElements().ToList();

                var window = new WPFCreatDuctFitting(linkInstances, elsDuctFittings, levels);
                window.ShowDialog();
                // 是否继续运行
                if (window.IsHitTestVisible)
                {
                    // 获取链接文件的doc
                    var linkInstance = window.LinkInstances.SelectedValue as RevitLinkInstance;
                    var linkdoc = linkInstance.GetLinkDocument();

                    var familySymbol = window.DuctFittingType.SelectedValue as FamilySymbol;
                    var Sellevel = window.SelectLevel.SelectedValue as Level;

                    // 获取element指定相交类别的过滤器
                    GetInstersectsElements GetInstersectsElements = new GetInstersectsElements();

                    // 标高?
                    var throughtLevel = window.InLevel.IsChecked.Value;

                    using (Transaction transaction = new Transaction(doc))
                    {
                        if (transaction.Start("布置套管") == TransactionStatus.Started)
                        {
                            List<Element> els_wall = new List<Element>();
                            // 获取项目中所有的墙体
                            var Wallels = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType().ToElements();
                            if (throughtLevel)
                            {
                                var select = from el in Wallels
                                             where el.LevelId.IntegerValue == Sellevel.Id.IntegerValue
                                             select el;
                                els_wall = select.ToList();
                            }
                            else
                            {
                                els_wall = Wallels.ToList();
                            }

                            if (els_wall.Count != 0)
                            {
                                foreach (var elWall in els_wall)
                                {
                                    // 获取墙体相交的 OST_DuctCurves 模型
                                    var instersectsElements = GetInstersectsElements.ElementByType(linkdoc, elWall, BuiltInCategory.OST_DuctCurves);

                                    if (instersectsElements.Count != 0)
                                    {
                                        foreach (var elDuct in instersectsElements)
                                        {
                                            var work = CreateWallDuctFitting(doc, elDuct, elWall, familySymbol);
                                            num_Q += 1;
                                            Push = true;
                                        }
                                    }
                                }
                            }
                            transaction.Commit();
                        }

                    }

                    #region 输出布置的结果信息
                    var resultMessage = "总计布置\n----------";
                    if (num_Q != 0 )
                    {
                        if (num_Q > 0)
                        {
                            resultMessage += string.Format("\n穿墙套管：{0}", num_Q);
                        }

                        TaskDialog.Show("统计", resultMessage);
                    }
                    else
                    {
                        TaskDialog.Show("统计", "未创建套管或洞口");
                    }
                    #endregion
                }

                #region 是否提交事务组
                if (Push)
                {
                    transactionGroup.Assimilate();
                }
                else
                {
                    transactionGroup.RollBack();
                }
                #endregion

            }

            return Result.Succeeded;
        }

        /// <summary>
        /// 获取XYZ(0,1,0)与element向量的旋转弧度
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public double ToRotaElementAngle(Element el)
        {
            var elCurve = el.Location as LocationCurve;
            var elLine = elCurve.Curve as Line;
            var elDirection = elLine.Direction;
            // 套管初始方向 Y--(1,0,0) X--(0,1,0)
            var familyDirection = new XYZ(1, 0, 0);
            var angle = familyDirection.AngleTo(elDirection);
            return angle;
        }

        /// <summary>
        /// 用eleB 剪切 eleA
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="eleA"></param>
        /// <param name="eleB"></param>
        /// <returns></returns>
        public bool CutTwoElement(Document doc, Element eleA, Element eleB)

        {
            bool IsTrue = Autodesk.Revit.DB.InstanceVoidCutUtils.CanBeCutWithVoid(eleA);
            if (IsTrue)
            {
                Autodesk.Revit.DB.InstanceVoidCutUtils.AddInstanceVoidCut(doc, eleA, eleB);
            }

            return IsTrue;
        }

        /// <summary>
        /// 创建风管 穿墙洞口
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elDuct"></param>
        /// <param name="elementLink"></param>
        /// <param name="familySymbol"></param>
        /// <returns></returns>
        public bool CreateWallDuctFitting(Document doc, Element elDuct, Element elementLink, FamilySymbol familySymbol)
        {
            bool goodjob = false;
            // 标高 高程点
            var mepCurve = elDuct as MEPCurve;
            var level = mepCurve.ReferenceLevel;
            var levelElevation = level.LookupParameter("立面").AsDouble();
            var levelPoint = new XYZ(0, 0, levelElevation);
            // 风管宽度 RBS_CURVE_WIDTH_PARAM
            var paramWidth = elDuct.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsDouble();
            // RBS_CURVE_HEIGHT_PARAM
            var paramHeight = elDuct.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsDouble();

            // 获取定位
            var location = elDuct.Location;
            var curve_local = (location as LocationCurve).Curve;
            // 获取中心点
            var midPoint = GetInsertSolidPoint.GetMidPointsWithCurve(elementLink, curve_local);
            // 获取选择的角度值 90° = PI/4
            var angle = ToRotaElementAngle(elementLink);
            // 获取套管定位点
            var origin = midPoint - levelPoint;

            // 墙体厚度
            var Wight = (elementLink as Wall).Width;

            // 类型设置
            var structuralType = Autodesk.Revit.DB.Structure.StructuralType.NonStructural;
            // 激活 确保可访问
            familySymbol.Activate();

            // 获取标高
            var levelId = elementLink.LevelId;
            var levelIn = doc.GetElement(levelId);

            // 创建套管 CreateNewAccessory
            var instance = doc.Create.NewFamilyInstance(origin, familySymbol, levelIn, structuralType);
            // 2018 api 标高中的高程
            var nowElevation = instance.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM).AsDouble();
            // 修改族参数
            instance.LookupParameter("穿管宽度").Set(paramWidth);
            instance.LookupParameter("穿管高度").Set(paramHeight);
            instance.LookupParameter("中心标高CL").Set(nowElevation);
            instance.LookupParameter("穿管长度").Set(Wight);
            // 选择角度
            var axis = Line.CreateBound(origin, new XYZ(origin.X, origin.Y, origin.Z + 1));
            var rotated = instance.Location.Rotate(axis, angle);

            // 剪切墙体与洞口
            CutTwoElement(doc, elementLink, instance);

            if (rotated)
            {
                goodjob = true;
            }
            return goodjob;
        }
    }
}
