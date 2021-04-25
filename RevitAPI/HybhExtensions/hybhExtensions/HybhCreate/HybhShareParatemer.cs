using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Xml
{
    /// <summary>
    /// 共享参数
    /// </summary>
    public class ShareParatemer
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 参数分组
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 共享参数构造函数
        /// </summary>
        public ShareParatemer(string sharedParametersFilename = @"C:\temp\tmp.txt", string groupName = "tmp")
        {
            if (!System.IO.File.Exists(sharedParametersFilename))
            {
                System.IO.StreamWriter sw = System.IO.File.CreateText(sharedParametersFilename);
                sw.Close();
            }
            FilePath = sharedParametersFilename;
            GroupName = groupName;
        }

        /// <summary>
        /// 添加共享参数
        /// </summary>
        /// <param name="uiapp"></param>
        /// <param name="definitionName"></param>
        /// <param name="parameterType"></param>
        /// <param name="instanceParameter"></param>
        /// <returns> 是否添加成功</returns>
        public bool Create(UIApplication uiapp,
            string definitionName = "paratemerName", ParameterType parameterType = ParameterType.Text, bool isinstance = false)
        {
            #region 参数
            if (FilePath == null || GroupName == null)
            {
                return false;
            }
            var app = uiapp.Application;
            var doc = uiapp.ActiveUIDocument.Document;
            CategorySet categorySet = new CategorySet();
            Category wallCategory = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Walls);
            Category floorCategory = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Floors);
            categorySet.Insert(wallCategory);
            categorySet.Insert(floorCategory);
            BuiltInParameterGroup parameterGroup = BuiltInParameterGroup.PG_DATA;
            app.SharedParametersFilename = FilePath;
            DefinitionFile definitionFile = app.OpenSharedParameterFile();
            DefinitionGroups groups = definitionFile.Groups;
            DefinitionGroup group = groups.get_Item(GroupName);
            #endregion
            // 创建参数
            if (group == null)
            {
                group = groups.Create(GroupName);
            }
            Definition definition = group.Definitions.get_Item(definitionName);
            if (definition == null)
            {
                var definitionOptions = new ExternalDefinitionCreationOptions(definitionName, parameterType);
                definition = group.Definitions.Create(definitionOptions);
            }
            ElementBinding binding;
            if (isinstance)
            {
                binding = app.Create.NewInstanceBinding(categorySet);
            }
            else
            {
                binding = app.Create.NewTypeBinding(categorySet);
            }
            // 绑定参数
            bool isSucces;
            var bindingMap = doc.ParameterBindings;
            if (bindingMap.Contains(definition))
            {
                isSucces = doc.ParameterBindings.ReInsert(definition, binding, parameterGroup);
            }
            else
            {
                isSucces = doc.ParameterBindings.Insert(definition, binding, parameterGroup);
            }

            return isSucces;
        }
    }
}
