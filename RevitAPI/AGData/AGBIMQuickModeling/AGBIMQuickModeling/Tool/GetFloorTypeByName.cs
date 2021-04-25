using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace CADReader.Model
{
    class GetFloorTypeByName
    {
        /// <summary>
        /// 获取类型 FloorType
        /// </summary>
        public FloorType Get(Document doc, SelectElement selectElement, double width = 100)
        {
            var floorTypes = doc.TCollector<FloorType>(true);
            var floortype = floorTypes.FirstOrDefault(i => i.Name == selectElement.ElementName);
            if(floortype == null)
            {
                floortype = NewType(doc, floorTypes, selectElement);
            }
            UpdateColor(doc, floortype, selectElement, width);
            return floortype;
        }

        /// <summary>
        /// 复制类型
        /// </summary>
        private FloorType NewType(Document doc, List<FloorType> floorTypes, SelectElement selectElement)
        {
            Transaction trans = new Transaction(doc, "newType");
            trans.Start();
            FloorType newtype = floorTypes.FirstOrDefault().Duplicate(selectElement.ElementName) as FloorType;
            trans.Commit();
            return newtype;
        }

        /// <summary>
        /// 更新类型厚度及材质参数
        /// </summary>
        private void UpdateColor(Document doc, FloorType floorType, SelectElement selectElement, double width)
        {
            var materialId = NewMaterialId(doc, selectElement);
            Transaction tranColor = new Transaction(doc, "updataMatrial");
            tranColor.Start();
            var layer = CompoundStructure.CreateSingleLayerCompoundStructure(
                MaterialFunctionAssignment.Structure, width.MillimeterToFeet(), materialId);
            layer.EndCap = EndCapCondition.NoEndCap;
            floorType.SetCompoundStructure(layer);
            tranColor.Commit();
        }

        /// <summary>
        /// 创建材质 Material
        /// </summary>
        private ElementId NewMaterialId(Document doc, SelectElement selectElement)
        {
            List<Material> materials = doc.TCollector<Material>();
            Material material = materials.FirstOrDefault(i => i.Name == selectElement.ElementName);
            if(material == null)
            {
                var t = new Transaction(doc, "newMatrial");
                t.Start();
                material = Material.Create(doc, selectElement.ElementName).ToElement(doc) as Material;
                t.Commit();
            }
            // 处理 Material 颜色
            var t_color = new Transaction(doc, "updateColor");
            t_color.Start();
            material.Color = selectElement.Color.ToColor();
            t_color.Commit();
            return material.Id;
        }
    }
}
