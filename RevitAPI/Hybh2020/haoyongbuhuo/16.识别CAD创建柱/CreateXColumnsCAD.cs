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
    class CreateXColumnsCAD : IExternalCommand
    {
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

                // 定位点
                XYZ loctionPont = null;
                // 获取标高
                var level = doc.ActiveView.GenLevel;

                // 选择CAD图纸
                PickCADFilter pickCADFilter = new PickCADFilter();

                #region 获取异形柱填充区域创建轮廓线
                try
                {
                    SelPick.SelRef = sel.PickObject(ObjectType.PointOnElement, pickCADFilter, "选择异形柱填充区域");
                }
                catch { }

                if (SelPick.SelRef == null)
                {
                    return Result.Cancelled;
                }
                ImportInstance dwg = doc.GetElement(SelPick.SelRef) as ImportInstance;
                var geoObj = dwg.GetGeometryObjectFromReference(SelPick.SelRef);
                Transform transform = dwg.GetTransform();

                // 获取线段集合
                CurveArray curveArray = new CurveArray();
                // 获取结构柱填充
                if (geoObj is PlanarFace)
                {
                    var planarFace = geoObj as PlanarFace;

                    // 获取边界的迭代器
                    var c = planarFace.GetEdgesAsCurveLoops().FirstOrDefault().GetCurveLoopIterator();
                    c.Reset();
                    while (c.MoveNext())
                    {
                        var curve = c.Current;
                        curveArray.Append(curve);
                        loctionPont = curve.GetEndPoint(0);
                    }
                }
                
                CurveArrArray curveArrArray = new CurveArrArray();
                curveArrArray.Append(curveArray);
                #endregion

                #region 获取异形柱的名称
                try
                {
                    SelPick.SelRef_two = sel.PickObject(ObjectType.PointOnElement, pickCADFilter, "再选择异形柱名称");
                }
                catch { }

                if (SelPick.SelRef_two == null)
                {
                    return Result.Cancelled;
                }
                // 获取cad文字
                CAD GetCAD = new CAD();
                var FamilyName = GetCAD.GetCADText(SelPick.SelRef_two, doc);
                #endregion

                // 创建事务组
                TransactionGroup transactionGroup = new TransactionGroup(doc);
                transactionGroup.Start("创建异形柱");
                bool Push = false;

                #region 创建异形柱
                // 创建异形柱名称
                var docTitle = "异形柱_" + level.Name + "_" + FamilyName;
                // 获取定位点 用于创建族文件 -*-
                var FamilyMovePoint = new XYZ(-loctionPont.X, -loctionPont.Y, 0.0);
                // 判断当前项目是否已创建族
                var isHaveElement = GetListInstance(doc, BuiltInCategory.OST_StructuralColumns, docTitle);

                if (isHaveElement.Count == 0)
                {
                    // 创建 结构柱 族加载到项目中
                    var familyCreated = CreateColumn(uiapp, curveArrArray, FamilyMovePoint, docTitle, FamilyName);
                    if (familyCreated)
                    {
                        // 获取族类型
                        var familySymbols = GetListInstance(doc, BuiltInCategory.OST_StructuralColumns, docTitle);
                        var familySymbol = familySymbols[0] as FamilySymbol;
                        // 获取 当前视图底部标高
                        GetLevel getLevel = new GetLevel();
                        var levelBase = getLevel.NearLevel(level, false);
                        // 创建族实例
                        CreateNewInstance(doc, transform.OfPoint(loctionPont), familySymbol, levelBase);
                        Push = true;
                    }
                }
                else
                {
                    TaskDialog.Show("提示", "避免创建过多类型异形柱\n已创建相同的异形柱\n使用镜像命令复制过来即可");
                }
                #endregion

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
        /// 创建 族实例
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="xYZ"> 定位点</param>
        /// <param name="familySymbol"> 族类型</param>
        /// <param name="level"></param>
        private FamilyInstance CreateNewInstance(Document doc, XYZ xYZ, FamilySymbol familySymbol, Level level)
        {

            using (Transaction transaction = new Transaction(doc))
            {
                transaction.Start("创建异形柱实例");

                // 关闭警告
                FailureHandlingOptions fho = transaction.GetFailureHandlingOptions();
                fho.SetFailuresPreprocessor(new FailuresPreprocessor());
                transaction.SetFailureHandlingOptions(fho);

                if (!familySymbol.IsActive)
                {
                    familySymbol.Activate();
                }
                var familyInstance = doc.Create.NewFamilyInstance(xYZ, familySymbol, level, Autodesk.Revit.DB.Structure.StructuralType.Column);
                transaction.Commit();
                return familyInstance;
            }
        }

        /// <summary>
        /// 获取指定族名称（SYMBOL_FAMILY_NAME_PARAM）的族实例
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="builtInCategory"> 类别</param>
        /// <param name="keyword"> 族名称</param>
        /// <returns></returns>
        private List<Element> GetListInstance(Document doc, BuiltInCategory builtInCategory, string keyword)
        {
            var elsType = new FilteredElementCollector(doc).OfCategory(builtInCategory).WhereElementIsElementType();
            var instance = from el in elsType
                           where el.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString() == keyword
                           select el;

            List<Element> ins = instance.ToList<Element>();

            return ins;
        }

        public Family Family { get; set; }
        /// <summary>
        /// 创建异形柱
        /// </summary>
        /// <param name="uiapp"> uiapp</param>
        /// <param name="arry"> CurveArrArray</param>
        /// <param name="xYZLocation"></param>
        /// <param name="typeName"> 族名称</param>
        /// <param name="newType"></param>
        private bool CreateColumn(UIApplication uiapp, CurveArrArray arry, XYZ xYZLocation, string typeName, string newType)
        {
            string rftPath = @"C:\ProgramData\Autodesk\ApplicationPlugins\hybh2020.bundle\Contents\2018\Family\StructureColumn.rft";
            Document doc = uiapp.ActiveUIDocument.Document;
            //创建族文件并载入到项目中 判断是否成功
            Document faDoc = uiapp.Application.NewFamilyDocument(rftPath);
            TransactionGroup transactionGroup = new TransactionGroup(faDoc);
            transactionGroup.Start("创建柱类型");
            bool Push = false;
            // 创建族
            Transaction trans = new Transaction(faDoc, "创建族");//创建事务
            trans.Start();
            try
            {
                // 关闭警告
                FailureHandlingOptions fho = trans.GetFailureHandlingOptions();
                fho.SetFailuresPreprocessor(new FailuresPreprocessor());
                trans.SetFailureHandlingOptions(fho);
                // 创建族文件操作
                SketchPlane skplane = GetSketchPlane(faDoc);
                Extrusion extruction = faDoc.FamilyCreate.NewExtrusion(true, arry, skplane, 4000 / 304.8);
                extruction.Location.Move(xYZLocation);
                faDoc.Regenerate();
                Reference topFaceRef = null;

                Options opt = new Options
                {
                    ComputeReferences = true,
                    DetailLevel = ViewDetailLevel.Fine
                };

                GeometryElement gelm = extruction.get_Geometry(opt);

                foreach (GeometryObject gobj in gelm)
                {
                    if (gobj is Solid)
                    {
                        Solid s = gobj as Solid;
                        foreach (Face face in s.Faces)
                        {
                            if (face.ComputeNormal(new UV()).IsAlmostEqualTo(new XYZ(0, 0, 1)))
                            {
                                topFaceRef = face.Reference;
                            }
                        }
                    }
                }
                View v = GetView(faDoc);
                Reference r = GetTopLevel(faDoc);
                Dimension d = faDoc.FamilyCreate.NewAlignment(v, r, topFaceRef);
                d.IsLocked = true;
                faDoc.Regenerate();

                // 设置用于模型行为的材质 - 混凝土
                faDoc.OwnerFamily.get_Parameter(BuiltInParameter.FAMILY_STRUCT_MATERIAL_TYPE).Set(2);

                #region 创建结构柱并载入项目
                FamilyManager manager = faDoc.FamilyManager;

                //FamilyParameter mfp = manager.AddParameter("材质", BuiltInParameterGroup.PG_MATERIALS, ParameterType.Material, false);//创建材质
                // 获取族材质参数
                var materialFamilyPara = manager.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                var materialID = new FilteredElementCollector(faDoc).OfCategory(BuiltInCategory.OST_Materials).Select(e => e.Id).FirstOrDefault();
                if (materialID == null)
                    return false;
                manager.Set(materialFamilyPara, materialID);
                // 获取元素材质参数
                Parameter p = extruction.get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM);
                // 关联
                manager.AssociateElementParameterToFamilyParameter(p, materialFamilyPara);

                // 族类型管理

                if (manager.CurrentType == null)
                {
                    var famType = manager.NewType(newType);
                }
                else
                {
                    manager.RenameCurrentType(newType);
                }
                trans.Commit();

                Family = faDoc.LoadFamily(doc);
                Push = true;
            }
            catch (Exception e)
            {
                trans.RollBack();
                TaskDialog.Show("提示", e.Message + Strings.error);
            }
            #endregion

            #region 更新事务
            if (Push)
            {
                transactionGroup.Assimilate();
                faDoc.Close(false);
                if (Family != null)
                {
                    trans = new Transaction(doc, "更新类型名称");
                    trans.Start();
                    // 修改类型名称
                    Family.Name = typeName;
                    Family.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).Set(newType);
                    trans.Commit();
                }
            }
            else
            {
                transactionGroup.RollBack();
            }
            #endregion

            return Push;
        }

        private Reference GetTopLevel(Document doc)
        {
            FilteredElementCollector temc = new FilteredElementCollector(doc);
            temc.OfClass(typeof(Level));
            Level lvl = temc.First(m => m.Name == "高于参照标高") as Level;
            return new Reference(lvl);
        }

        private View GetView(Document doc)
        {
            FilteredElementCollector viewFilter = new FilteredElementCollector(doc);
            viewFilter.OfClass(typeof(View));
            View v = viewFilter.First(m => m.Name == "前") as View;
            return v;
        }

        private SketchPlane GetSketchPlane(Document doc)
        {
            FilteredElementCollector temc = new FilteredElementCollector(doc);
            temc.OfClass(typeof(SketchPlane));
            SketchPlane sketchPlane = temc.First(m => m.Name == "低于参照标高") as SketchPlane;
            return sketchPlane;
        }


    }
}
