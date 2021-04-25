using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class CreateFloorHanderPoint : IExternalCommand
    {
        /// <summary>
        /// 点击创建楼板未完成！
        /// </summary>
        public Level LocalLevel { get; set; }
        public XYZ SelPoint { get; set; }
        public FloorType FloorType { get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // 程序是否注册可运行
            if (Run.Running(Strings.key))
            {
                // 获取 类型
                var ElementsType = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Floors).WhereElementIsElementType().ToElements();
                List<SelectElementByName> listData = new List<SelectElementByName>();
                // 获取所有的类型名称 
                foreach (Element el in ElementsType)
                {
                    var Familytype = el as FloorType;
                    listData.Add(new SelectElementByName { HybhElement = Familytype, HybhElName = Familytype.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString() + ":" + Familytype.Name });
                }

                WPFCreateFloorHanderPoint wPFAutoCreateFloorPoint = new WPFCreateFloorHanderPoint(listData);
                _ = new System.Windows.Interop.WindowInteropHelper(wPFAutoCreateFloorPoint)
                {
                    Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle
                };
                wPFAutoCreateFloorPoint.Show();
            }

            return Result.Succeeded;
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
