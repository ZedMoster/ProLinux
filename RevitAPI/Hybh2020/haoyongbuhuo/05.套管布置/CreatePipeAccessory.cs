using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class CreatePipeAccessory : IExternalCommand
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
                // 统计个数
                int num_Q = 0;
                int num_L = 0;
                TransactionGroup transactionGroup = new TransactionGroup(doc, "套管布置");
                transactionGroup.Start();
                bool Push = false;

                // 载入所需的族文件
                DoLoadFamily doLoadFamily = new DoLoadFamily();
                try
                {
                    var Rfa = "PipeAccessory-A";
                    var have = doLoadFamily.IsHaveFamily(doc, BuiltInCategory.OST_PipeFitting, "hybh套管管件-A");
                    if (!have)
                    {
                        doLoadFamily.LoadFamily(doc, Rfa + ".rfa", "hybh套管管件-A");
                    }
                    var Rfa1 = "PipeAccessory";
                    var have1 = doLoadFamily.IsHaveFamily(doc, BuiltInCategory.OST_PipeFitting, "hybh套管管件");
                    if (!have1)
                    {
                        doLoadFamily.LoadFamily(doc, Rfa1 + ".rfa", "hybh套管管件");
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

                // 获取套管族文件
                var els_pipe_family = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_PipeFitting).WhereElementIsElementType().ToElements().ToList();
                // 获取所有的标高
                var levels = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().ToElements().ToList();

                // 结构墙与管线相交
                List<ElementId> ErrorWallIds = new List<ElementId>();

                var window = new WPFCreatPipeAccessory(linkInstances, els_pipe_family, levels);
                window.ShowDialog();
                // 判断程序结构是否运行
                if (window.IsHitTestVisible)
                {
                    // 获取链接文件的doc
                    var linkInstance = window.LinkInstances.SelectedValue as RevitLinkInstance;
                    var linkdoc = linkInstance.GetLinkDocument();

                    var familySymbol = window.PipeAccessoryType.SelectedValue as FamilySymbol;
                    var Sellevel = window.SelectLevel.SelectedValue as Level;

                    // 穿墙？ 穿梁？
                    var throughtWall = window.HaveWall.IsChecked.Value;
                    var throughtBeam = window.HaveBeam.IsChecked.Value;
                    // 标高?
                    var throughtLevel = window.InLevel.IsChecked.Value;

                    // 获取element指定相交类别的过滤器
                    GetInstersectsElements GetInstersectsElements = new GetInstersectsElements();

                    using (Transaction transaction = new Transaction(doc))
                    {
                        if (transaction.Start("布置套管") == TransactionStatus.Started)
                        {
                            #region  穿墙套管
                            if (throughtWall)
                            {
                                List<Element> els_wall = new List<Element>();
                                // 获取项目中所有的墙体
                                var Wallels = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType().ToElements().ToList();
                                if (throughtLevel)
                                {
                                    var select = from el in Wallels
                                                 where el.LevelId.IntegerValue == Sellevel.Id.IntegerValue
                                                 select el;
                                    els_wall = select.ToList();
                                }
                                else
                                {
                                    els_wall = Wallels;
                                }

                                // 项目中不含墙体
                                if (els_wall.Count != 0)
                                {
                                    foreach (var elWall in els_wall)
                                    {
                                        // 获取墙体相交的管线模型
                                        var instersectsElements = GetInstersectsElements.ElementByType(linkdoc, elWall, BuiltInCategory.OST_PipeCurves);
                                        if (instersectsElements.Count != 0)
                                        {
                                            foreach (var elPipe in instersectsElements)
                                            {
                                                var work = CreateWallAccessory(doc, elPipe, elWall, familySymbol);
                                                num_Q += 1;
                                                Push = true;
                                                // 判断墙体是建筑墙还是结构墙
                                                if (elWall.get_Parameter(BuiltInParameter.WALL_STRUCTURAL_SIGNIFICANT).AsInteger() == 1)
                                                {
                                                    ErrorWallIds.Add(elWall.Id);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region  穿梁套管
                            if (throughtBeam)
                            {
                                List<Element> els_Beam = new List<Element>();
                                var Beamels = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming).WhereElementIsNotElementType().ToElements();
                                if (throughtLevel)
                                {
                                    var select = from el in Beamels
                                                 where (el as FamilyInstance).Host.Id.IntegerValue == Sellevel.Id.IntegerValue
                                                 select el;
                                    els_Beam = select.ToList();
                                }
                                else
                                {
                                    els_Beam = Beamels.ToList();
                                }

                                // 项目中不含结构梁
                                if (els_Beam.Count != 0)
                                {
                                    foreach (var elBeam in els_Beam)
                                    {
                                        // 获取墙体相交的管线模型
                                        var instersectsElements = GetInstersectsElements.ElementByType(linkdoc, elBeam, BuiltInCategory.OST_PipeCurves);
                                        if (instersectsElements.Count != 0)
                                        {
                                            foreach (var elPipe in instersectsElements)
                                            {
                                                var work = CreateBeamAccessory(doc, elPipe, elBeam, familySymbol);
                                                num_L += 1;
                                                Push = true;
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            transaction.Commit();
                        }
                        #region 输出布置的结果信息
                        var resultMessage = "总计布置\n----------";
                        if (num_Q != 0 || num_L != 0)
                        {
                            if (num_Q > 0)
                            {
                                resultMessage += string.Format("\n穿墙套管：{0}", num_Q);
                            }
                            if (num_L > 0)
                            {
                                resultMessage += string.Format("\n穿梁套管：{0}", num_L);
                            }
                            TaskDialog.Show("统计", resultMessage);
                        }
                        else
                        {
                            TaskDialog.Show("统计", "未创建套管或洞口");
                        }
                        #endregion
                    }
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

                #region ui选中结构属性墙体
                if (ErrorWallIds.Count > 0)
                {
                    TaskDialog.Show("提示", "项目中包含结构属性墙体与管线碰撞\n当前窗口已自动选中该属性的墙体");
                    sel.SetElementIds(ErrorWallIds);
                }
                #endregion
            }
            return Result.Succeeded;
        }

        // 获取XYZ(0,1,0)与element向量的旋转弧度
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

        // 创建穿墙套管
        public bool CreateWallAccessory(Document doc, Element elPipe, Element elementLink, FamilySymbol familySymbol)
        {
            bool goodjob = false;
            // 标高 高程点
            var mepCurve = elPipe as MEPCurve;
            var level = mepCurve.ReferenceLevel;
            var levelElevation = level.LookupParameter("立面").AsDouble();
            var levelPoint = new XYZ(0, 0, levelElevation);
            // 管道直径 RBS_PIPE_DIAMETER_PARAM
            var diameterParam = elPipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble();
            
            // 获取定位
            var location = elPipe.Location;
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
            instance.LookupParameter("穿管长度").Set(Wight);
            instance.LookupParameter("穿管管径").Set(diameterParam);
            instance.LookupParameter("中心标高CL").Set(nowElevation);
            // 选择角度
            var axis = Line.CreateBound(origin, new XYZ(origin.X, origin.Y, origin.Z + 1));
            var rotated = instance.Location.Rotate(axis, angle);

            // 剪切洞口
            CutTwoElement(doc, elementLink, instance);

            if (rotated)
            {
                goodjob = true;
            }
            return goodjob;
        }

        /// <summary>
        /// 梁几何参数
        /// </summary>
        public double HightValue { get; set; }

        // 创建穿梁套管
        public bool CreateBeamAccessory(Document doc, Element elPipe, Element elementLink, FamilySymbol familySymbol)
        {
            bool goodjob = false;
            // 标高 高程点
            var mepCurve = elPipe as MEPCurve;
            var level = mepCurve.ReferenceLevel;
            var levelElevation = level.LookupParameter("立面").AsDouble();
            var levelPoint = new XYZ(0, 0, levelElevation);
            // 管道直径 RBS_PIPE_DIAMETER_PARAM
            var diameterParam = elPipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble();

            // 获取定位
            var location = elPipe.Location;
            var curve_local = (location as LocationCurve).Curve;

            // 获取中心点
            var midPoint = GetInsertSolidPoint.GetMidPointsWithCurve(elementLink, curve_local);
            // 获取选择的角度值 90° = PI/4
            var angle = ToRotaElementAngle(elementLink);
            // 获取套管定位点
            var origin = midPoint - levelPoint;

            RegistryStorage Registry = new RegistryStorage();
            // 读取梁参数名称
            var B_string = Registry.OpenAfterStart("BeamB") ?? "b";
            try
            {
                // 获取梁类型参数 b
                var elType = (elementLink as FamilyInstance).Symbol;
                HightValue = elType.LookupParameter(B_string).AsDouble();
            }
            catch
            {
                // 获取梁实例参数 宽度
                HightValue = elementLink.LookupParameter(B_string).AsDouble();
            }

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
            instance.LookupParameter("穿管长度").Set(HightValue);
            instance.LookupParameter("穿管管径").Set(diameterParam);
            instance.LookupParameter("中心标高CL").Set(nowElevation);
            // 选择角度
            var axis = Line.CreateBound(origin, new XYZ(origin.X, origin.Y, origin.Z + 1));
            var rotated = instance.Location.Rotate(axis, angle);

            // 剪切洞口
            CutTwoElement(doc, elementLink, instance);

            if (rotated)
            {
                goodjob = true;
            }
            return goodjob;
        }

    }

    public class GetPipeFamilySymbol
    {
        private Element pipeElement;

        public Element PipeElement
        {
            get { return pipeElement; }
            set { pipeElement = value; }
        }

        // 通过管道直径获取套管族类型
        public Element IsHavePipeAccessory(IList<Element> els_pipe_family)
        {
            var haveBZ = false;
            foreach (var pipe_family in els_pipe_family)
            {
                if (pipe_family.Name == "标准")
                {
                    PipeElement = pipe_family;
                    haveBZ = true;
                }
            }
            if (!haveBZ)
            {
                PipeElement = els_pipe_family[0];
            }
            return PipeElement;

        }
    }
}
