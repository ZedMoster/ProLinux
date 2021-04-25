using System;
using System.Windows;
using System.Collections.Generic;

using AGBIM.Visual.RevitPlugins.AGFamilyLib.FamiliRepo.Model;

using Autodesk.Revit.DB;

namespace AGBIM.Visual.RevitPlugins.AGFamilyLib
{
    public class GetParameters
    {
        public Document Fdoc { get; }

        /// <summary>
        /// 传入FamilyDocument
        /// </summary>
        /// <param name="Familydoc"></param>
        public GetParameters(Document Familydoc)
        {
            Fdoc = Familydoc;
        }

        /// <summary>
        /// 参数数据
        /// </summary>
        /// <returns></returns>
        public FamilyParameterModel GetData()
        {
            FamilyParameterModel familyParameterModel = new FamilyParameterModel();
            // 管理
            FamilyManager manage = Fdoc.FamilyManager;
            // 参数排序
            Dictionary<string, FamilyParameter> fps = new Dictionary<string, FamilyParameter>(manage.Parameters.Size);
            foreach (FamilyParameter fp in manage.Parameters)
            {
                string name = fp.Definition.Name;
                fps.Add(name, fp);
            }
            List<string> keys = new List<string>(fps.Keys);
            keys.Sort();
            // 类型总计
            familyParameterModel.Types = manage.Types.Size;
            // 类型列表
            List<CurrentType> currentTypes = new List<CurrentType>();
            foreach (FamilyType t in manage.Types)
            {
                // 1.类型
                CurrentType currentType = new CurrentType
                {
                    Name = t.Name
                };
                // 参数列表
                List<Parameters> getParameters = new List<Parameters>();
                // 2.参数
                foreach (string key in keys)
                {
                    FamilyParameter fp = fps[key];
                    #region 绑定参数
                    // 跳过异常
                    string displayUnitType = null;
                    try
                    {
                        displayUnitType = fp.DisplayUnitType.ToString();
                    }
                    catch { }
                    Parameters parameters = new Parameters
                    {
                        Id = fp.Id.IntegerValue,
                        Name = fp.Definition.Name,
                        StorageType = fp.StorageType.ToString(),
                        DisplayUnitType = displayUnitType,
                        IsInstance = fp.IsInstance,
                        IsShared = fp.IsShared,
                        IsReporting = fp.IsReporting,
                        CanAssignFormula = fp.CanAssignFormula,
                        Formula = fp.Formula,
                        ParameterGroup = fp.Definition.ParameterGroup.ToString(),
                        ParameterType = fp.Definition.ParameterType.ToString(),
                        UnitType = fp.Definition.UnitType.ToString(),
                        BuiltInParameter = (fp.Definition as InternalDefinition).BuiltInParameter.ToString(),
                    };
                    // 3.数值
                    if (t.HasValue(fp))
                    {
                        parameters.AsValueString = FamilyParamValueString(t, fp);
                    }
                    #endregion
                    getParameters.Add(parameters);
                    // --参数
                }
                currentType.GetParameters = getParameters;
                // --类型及参数
                currentTypes.Add(currentType);
            }
            familyParameterModel.CurrentTypes = currentTypes;

            return familyParameterModel;
        }

        /// <summary>
        /// 获取参数值 ValueString
        /// </summary>
        /// <param name="t"></param>
        /// <param name="fp"></param>
        /// <returns></returns>
        private static string FamilyParamValueString(FamilyType t, FamilyParameter fp)
        {
            string value = t.AsValueString(fp);
            switch (fp.StorageType)
            {
                case StorageType.Double:
                    break;
                case StorageType.ElementId:
                    ElementId id = t.AsElementId(fp);
                    value = id.IntegerValue.ToString();
                    break;
                case StorageType.Integer:
                    value = t.AsInteger(fp).ToString();
                    break;
                case StorageType.String:
                    value = t.AsString(fp);
                    break;
            }
            return value;
        }
    }
}
