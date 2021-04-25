using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Linq;

namespace AGBIM.Visual.RevitPlugins.AGFamilyLib
{
    /// <summary>
    /// 添加族参数
    /// </summary>
    public class AddParameter
    {
        public Document Fdoc { get; }

        /// <summary>
        /// 传入FamilyDocument
        /// </summary>
        /// <param name="Familydoc"></param>
        public AddParameter(Document Familydoc)
        {
            Fdoc = Familydoc;
        }

        /// <summary>
        /// 创建族参数
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="parameterGroup"></param>
        /// <param name="parameterType"></param>
        /// <param name="isInstance"></param>
        public void Set(string parameterName, BuiltInParameterGroup parameterGroup, ParameterType parameterType, bool isInstance)
        {
            // 族管理
            FamilyManager manager = GetManager();
            var used = ParameterNameIsUsed(manager, parameterName, out FamilyParameter parameter);
            if (!used)
            {
                Transaction tran = new Transaction(Fdoc, "添加参数");
                tran.Start();
                // 创建参数
                manager.AddParameter(parameterName, parameterGroup, parameterType, isInstance);
                tran.Commit();
            }
        }

        /// <summary>
        /// 创建族参数并赋值
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="parameterGroup"></param>
        /// <param name="parameterType"></param>
        /// <param name="isInstance"></param>
        public void Set<T>(string parameterName, BuiltInParameterGroup parameterGroup, ParameterType parameterType, bool isInstance, T parameterValue)
        {
            // 族管理
            FamilyManager manager = GetManager();
            FamilyTypeSet types = manager.Types;
            // 添加族参数
            Transaction tran = new Transaction(Fdoc, "添加参数");
            tran.Start();
            try
            {
                var used = ParameterNameIsUsed(manager, parameterName, out FamilyParameter parameter);
                if (used)
                {
                    manager.RemoveParameter(parameter);
                }
                // 添加族参数
                FamilyParameter mfp = manager.AddParameter(parameterName, parameterGroup, parameterType, isInstance);
                #region 参数赋值
                foreach (FamilyType tp in types)
                {
                    manager.CurrentType = tp;
                    // 创建参数
                    if (parameterValue is double para_double)
                    {
                        manager.Set(mfp, para_double);
                    }
                    else if (parameterValue is int para_int)
                    {
                        manager.Set(mfp, para_int);
                    }
                    else if (parameterValue is string para_string)
                    {
                        manager.Set(mfp, para_string);
                    }
                    else if (parameterValue is ElementId para_ElementId)
                    {
                        manager.Set(mfp, para_ElementId);
                    }
                    else
                    {
                        TaskDialog.Show("错误", "参数类型错误");
                    }
                }
                #endregion
                tran.Commit();
            }
            catch (Exception e)
            {
                tran.RollBack();
                throw new Exception("参数添加失败：" + e.Message);
            }
        }

        /// <summary>
        /// 获取族管理 FamilyManager
        /// </summary>
        /// <returns></returns>
        private FamilyManager GetManager()
        {
            if (Fdoc == null)
            {
                throw new Exception("Error: Family document is null !");
            }

            FamilyManager manager = Fdoc.FamilyManager;
            if (manager.CurrentType == null)
            {
                Transaction tranCurrentType = new Transaction(Fdoc, "新建类型");
                tranCurrentType.Start();
                manager.NewType(Fdoc.Title);
                tranCurrentType.Commit();
            }
            return manager;
        }

        /// <summary>
        /// 判断族参数名称是否已存在
        /// </summary>
        /// <returns></returns>
        private bool ParameterNameIsUsed(FamilyManager manager, string paratemerName, out FamilyParameter familyParameter)
        {
            familyParameter = manager.GetParameters().FirstOrDefault(x => x.Definition.Name == paratemerName);
            return familyParameter != null;
        }
    }
}
