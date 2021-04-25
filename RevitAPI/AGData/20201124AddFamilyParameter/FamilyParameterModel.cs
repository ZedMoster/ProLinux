using System.Collections.Generic;

//namespace AGBIM.Visual.RevitPlugins.AGFamilyLib.FamiliRepo.Model
namespace AddFamilyParameter
{
    /// <summary>
    /// 族
    /// </summary>
    public class FamilyParameterModel
    {

        public int Types { get; set; }
        public List<CurrentType> CurrentTypes { get; set; }
    }

    /// <summary>
    /// 类型
    /// </summary>
    public class CurrentType
    {
        public string Name { get; set; }
        public List<Parameters> GetParameters { get; set; }
    }

    /// <summary>
    /// 参数
    /// </summary>
    public class Parameters
    {
        /// <summary>
        /// FamilyParameter
        /// </summary>
        public int Id { get; set; }
        public string Name { get; set; }
        public string StorageType { get; set; }
        public string DisplayUnitType { get; set; }
        public bool IsInstance { get; set; }
        public bool IsShared { get; set; }
        public bool IsReporting { get; set; }
        public bool CanAssignFormula { get; set; }
        public string Formula { get; set; }

        /// <summary>
        /// Definition
        /// </summary>
        public string ParameterGroup { get; set; }
        public string ParameterType { get; set; }
        public string UnitType { get; set; }
        public string BuiltInParameter { get; set; }
        public string AsValueString { get; set; }
    }
}
