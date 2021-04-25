using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using AGBIMMunicipalPipeline.Properties;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;

using Logger.Help;

namespace AGBIMMunicipalPipeline
{
    public static class PipeExtensions
    {
        /// <summary>
        /// 通过两点创建管线
        /// </summary>
        /// <param name="doc"> 当前文档</param>
        /// <param name="p0"> 管段起点</param>
        /// <param name="p1"> 管段终点</param>
        /// <param name="pipeTypeName"> 管道类型名称</param>
        /// <param name="systemTypeName"> 管道系统名称</param>
        /// <returns></returns>
        public static Pipe CreatePipe(this Document doc, XYZ p0, XYZ p1, string pipeTypeName = "默认", string systemTypeName = "其他")
        {
            // 获取管道系统
            FilteredElementCollector collectorsys = new FilteredElementCollector(doc).OfClass(typeof(PipingSystemType));
            var systemTypeId = collectorsys.ToElements().FirstOrDefault(i => i.Name == systemTypeName)?.Id;
            // 获取管道类型
            FilteredElementCollector collectortypes = new FilteredElementCollector(doc).OfClass(typeof(PipeType));
            var _pipeTypeId = collectortypes.ToElements().FirstOrDefault(i => i.Name == pipeTypeName)?.Id;
            // 设置管道
            Pipe pipe = null;
            if(null != systemTypeId)
            {
                Transaction t = new Transaction(doc, "CreatePipe");
                t.Start();
                try
                {
                    var pipeTypeId = _pipeTypeId == null
                        ? collectortypes.Cast<PipeType>().FirstOrDefault()?.Duplicate(pipeTypeName)?.Id : _pipeTypeId;
                    pipe = Pipe.Create(doc, systemTypeId, pipeTypeId, doc.ActiveView.GenLevel.Id, p0, p1);
                    t.Commit();
                }
                catch(System.Exception e)
                {
                    $"CreatePipe: {e.Message}".ToLog();
                    t.RollBack();
                }
            }
            return pipe;
        }

        /// <summary>
        /// 更新管道参数
        /// </summary>
        /// <param name="pipe"> 管道实例</param>
        /// <param name="doc"> 当前文档</param>
        /// <returns></returns>
        public static bool UpdateParameter(this Pipe pipe, Document doc, Dictionary<string, string> dict, string attname)
        {
            int count = 0;

            Transaction t = new Transaction(doc, "updatePipe");
            t.Start();
            try
            {
                dict.TryGetValue("D_DIA", out string value);
                if(string.IsNullOrWhiteSpace(value))
                {
                    $"UpdateParameter: pipe D key is error".ToLog();
                }
                pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(value.ToFloat(true));

            }
            catch(System.Exception e)
            {
                $"UpdateParameter: {e.Message}".ToLog();
            }
            foreach(var _key in dict.Keys)
            {
                try
                {
                    var key = ProduceXml.Xml.Read(attname, _key, attname);
                    var _get = dict.TryGetValue(_key, out string value);
                    if(!_get)
                    {
                        continue;
                    }
                    var para = pipe.LookupParameter(key);
                    if(para == null)
                    {
                        continue;
                    }

                    if(para.StorageType == StorageType.String)
                    {
                        para.Set(value);
                        $"Set: {key}{value}".ToLog();
                    }
                    else if(para.StorageType == StorageType.Double)
                    {
                        para.Set(value.ToFloat(true));
                    }
                    else if(para.StorageType == StorageType.Integer)
                    {
                        if(value == "FALSE")
                        {
                            para.Set(0);
                        }
                        else
                        {
                            para.Set(1);
                        }
                    }
                    count++;
                }
                catch(System.Exception e)
                {
                    $"UpdateParameter: {e.Message}".ToLog();
                }
            }
            _ = count > 0 ? t.Commit() : t.RollBack();

            return count > 0;

        }

        /// <summary>
        /// 基于点创建管井
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="p0"></param>
        /// <returns></returns>
        public static FamilyInstance CreateWell(this Document doc, XYZ p0)
        {
            FamilyInstance instance = null;
            // 获取 OST_GenericModel
            List<Element> elsType = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_GenericModel).ToList();
            // 族类型
            if(!(elsType.FirstOrDefault(i => i.Name == "检修井") is FamilySymbol symbol))
            {
                //var rfa_file_path = @"D:\Git\AgBIM\BIM2ReviewRevit\AGBIMMunicipalPipeline\data\Family\检修井.rfa";
                var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Family");
                var rfa_file_path = Path.Combine(path, "检修井.rfa");

                Family family = null;
                Transaction t_load = new Transaction(doc, "loadFamily");
                t_load.Start();
                try
                {
                    // 载入族文件
                    doc.LoadFamily(rfa_file_path, out family);
                    t_load.Commit();
                }
                catch(System.Exception e)
                {
                    $"CreateWell: {e.Message}".ToLog();
                    t_load.RollBack();
                }
                symbol = doc.GetElement(family.GetFamilySymbolIds().FirstOrDefault()) as FamilySymbol;
            }

            Transaction t = new Transaction(doc, "newWell");
            t.Start();
            try
            {
                if(!symbol.IsActive)
                    symbol.Activate();
                // 创建管井
                instance = doc.Create.NewFamilyInstance(p0, symbol, doc.ActiveView.GenLevel, StructuralType.NonStructural);
                t.Commit();
            }
            catch(System.Exception e)
            {
                $"CreateWell: {e.Message}".ToLog();
                t.RollBack();
            }

            return instance;
        }

        /// <summary>
        /// 更新族参数
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="doc"></param>
        /// <param name="dict"></param>
        /// <param name="attname"></param>
        /// <returns></returns>
        public static bool UpdateParameter(this FamilyInstance instance, Document doc, Dictionary<string, string> dict, string attname)
        {
            int count = 0;

            Transaction t = new Transaction(doc, "updateElement");
            t.Start();

            foreach(var _key in dict.Keys)
            {
                try
                {
                    var key = ProduceXml.Xml.Read(attname, _key, attname);
                    var _get = dict.TryGetValue(_key, out string value);
                    if(!_get)
                    {
                        continue;
                    }
                    var para = instance.LookupParameter(key);
                    if(para == null)
                    {
                        continue;
                    }

                    if(para.StorageType == StorageType.String)
                    {
                        para.Set(value);
                    }
                    else if(para.StorageType == StorageType.Double)
                    {
                        para.Set(value.ToFloat(true));
                    }
                    else if(para.StorageType == StorageType.Integer)
                    {
                        if(value == "FALSE")
                        {
                            para.Set(0);
                        }
                        else
                        {
                            para.Set(1);
                        }
                    }
                    count++;
                }
                catch(System.Exception e)
                {
                    $"UpdateParameter: {e.Message}".ToLog();
                }
            }
            _ = count > 0 ? t.Commit() : t.RollBack();

            return count > 0;
        }
    }
}
