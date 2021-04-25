using Autodesk.Revit.DB;

namespace Xml.HybhElement
{
    public static class HybhFamilySymbol
    {
        /// <summary>
        /// 获取族类型创建的方式
        /// </summary>
        /// <param name="familySymbol"> 族类型</param>
        /// <returns> 获取族类型的创建方式</returns>
        public static FamilyPlacementType GetSymbolPlaceType(this FamilySymbol familySymbol)
        {
            return familySymbol.Family.FamilyPlacementType;
        }
    }
}
