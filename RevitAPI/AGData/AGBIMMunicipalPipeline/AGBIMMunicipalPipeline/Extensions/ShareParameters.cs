using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Logger.Help;

namespace AGBIMMunicipalPipeline
{
    class ShareParameters
    {
        /// <summary>
        /// 共享参数
        /// </summary>
        /// <param name="sharedParametersFilename"> 共享参数文件存储位置</param>
        /// <param name="groupName"> 参数分组</param>
        public ShareParameters(string sharedParametersFilename = "AGKeys.txt", string groupName = "AGKeys", string type = ".txt")
        {
            var path = System.Environment.GetEnvironmentVariable("TEMP");
            if(path == null)
            {
                "Class ShareParatemer-参数系统缓存txt文件创建失败".ToLog();
                System.Windows.MessageBox.Show("缓存保存失败");
                return;
            }

            var _sharedParametersFilename = sharedParametersFilename.Contains(type) ? sharedParametersFilename : $"{sharedParametersFilename}{type}";
            var filePath = System.IO.Path.Combine(path, _sharedParametersFilename);
            System.IO.StreamWriter sw = System.IO.File.CreateText(filePath);
            sw.Close();

            FilePath = filePath;
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
            string definitionName = "paratemerName", ParameterType parameterType = ParameterType.Text, bool isinstance = true)
        {
            #region 参数
            if(FilePath == null || GroupName == null)
            {
                return false;
            }
            var app = uiapp.Application;
            var doc = uiapp.ActiveUIDocument.Document;
            CategorySet categorySet = new CategorySet();
            Category pipeCategory = doc.Settings.Categories.get_Item(BuiltInCategory.OST_PipeCurves);
            categorySet.Insert(pipeCategory);

            BuiltInParameterGroup parameterGroup = BuiltInParameterGroup.PG_DATA;
            app.SharedParametersFilename = FilePath;
            DefinitionFile definitionFile = app.OpenSharedParameterFile();
            DefinitionGroups groups = definitionFile.Groups;
            DefinitionGroup group = groups.get_Item(GroupName);
            #endregion
            // 创建参数
            if(group == null)
            {
                group = groups.Create(GroupName);
            }
            Definition definition = group.Definitions.get_Item(definitionName);
            if(definition == null)
            {
                var definitionOptions = new ExternalDefinitionCreationOptions(definitionName, parameterType);
                definition = group.Definitions.Create(definitionOptions);
            }
            ElementBinding binding;
            if(isinstance)
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
            if(bindingMap.Contains(definition))
            {
                isSucces = doc.ParameterBindings.ReInsert(definition, binding, parameterGroup);
            }
            else
            {
                isSucces = doc.ParameterBindings.Insert(definition, binding, parameterGroup);
            }

            return isSucces;

        }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 参数分组
        /// </summary>
        public string GroupName { get; set; }
    }
}
