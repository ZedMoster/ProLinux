using System.Collections.Generic;
using System.Linq;

using AGBIMMunicipalPipeline.Extensions;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

using ExcelHelper;

using Logger.Help;

namespace AGBIMMunicipalPipeline
{
    [Transaction(TransactionMode.Manual)]
    public class NewPipeCommand : IExternalCommand
    {
        readonly string SheetName1 = "管点调查表";
        readonly string SheetName2 = "管线调查表";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Main main = new Main();
            main.ShowDialog();

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            if(main.IsHitTestVisible)
            {
                TaskDialog.Show("警告", "已取消运行");
                return Result.Cancelled;
            }
            if(doc.ActiveView is View3D)
            {
                TaskDialog.Show("警告", "禁止在三维视图创建模型");
                return Result.Cancelled;
            }

            var filePath = main.OriginPoint.FilePath;
            ReadExcel readExcel = new ReadExcel(filePath);
            var table = readExcel.GetDataTableToDict();
            var table_points = table[SheetName1];
            var table_curves = table[SheetName2];

            TransactionGroup T = new TransactionGroup(doc, "地下管网");
            T.Start();
            // 添加参数
            var add_pipe = AddParatemerByName(uiapp, table_curves, SheetName2);
            $"参数是否添加：{add_pipe}".ToLog();
            // 创建管道
            var pipes = NewPipeByPoints(doc, table_points, table_curves, main.OriginPoint);
            $"创建管道个数：{pipes.Count}".ToLog();
            // 创建管井
            var wells = NewWellByPoint(doc, table_points, main.OriginPoint);
            $"创建管井个数：{pipes.Count}".ToLog();

            // 更新事务组
            _ = pipes.Count > 0 || wells.Count > 0 ? T.Assimilate() : T.RollBack();
            // 显示创建结果
            TaskDialog.Show("提示", $" 管段个数: {pipes.Count}\n 管井个数: {wells.Count}");

            return Result.Succeeded;
        }

        /// <summary>
        /// 创建管井
        /// </summary>
        /// <returns></returns>
        private List<Element> NewWellByPoint(Document doc, List<Dictionary<string, string>> table_points, OriginPoint originPoint)
        {
            List<Element> els = new List<Element>();
            foreach(var item in table_points)
            {
                try
                {
                    // 编号
                    var SUBSID = item.GetValue("SUBSID");
                    if(SUBSID != "检修井")
                    {
                        continue;
                    }
                    // 定位点
                    var p0 = item.GetPoint(originPoint.X.ToFloat(), originPoint.Y.ToFloat(), originPoint.Z.ToFloat());
                    var instance = doc.CreateWell(p0);
                    if(instance == null)
                    {
                        continue;
                    }

                    // 更新参数
                    var push = instance.UpdateParameter(doc, item, SheetName1);

                    if(push)
                    {
                        els.Add(instance);
                    }
                }
                catch(System.Exception e)
                {
                    $"NewWellByPoint:{e.Message}".ToLog();
                }
            }
            return els;
        }

        /// <summary>
        /// 创建管线
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="table_points"></param>
        /// <param name="table_curves"></param>
        /// <returns></returns>
        private List<Pipe> NewPipeByPoints(Document doc, List<Dictionary<string, string>> table_points, List<Dictionary<string, string>> table_curves, OriginPoint originPoint)
        {
            List<Pipe> pipes = new List<Pipe>();
            var elevation = doc.ActiveView.GenLevel.Elevation;
            foreach(var item in table_curves)
            {
                try
                {
                    // 编号
                    var S_POINT = item["S_POINT"];
                    var E_POINT = item["E_POINT"];
                    // 定位点
                    var _p0 = table_points.GetPointByEXP(S_POINT, originPoint.X.ToFloat(), originPoint.Y.ToFloat(), originPoint.Z.ToFloat());
                    var _p1 = table_points.GetPointByEXP(E_POINT, originPoint.X.ToFloat(), originPoint.Y.ToFloat(), originPoint.Z.ToFloat());
                    var p0 = _p0.GetXYZ(elevation);
                    var p1 = _p1.GetXYZ(elevation);
                    if(p0.DistanceTo(p1) < 0.001)
                    {
                        continue;
                    }
                    // 创建管道
                    var pipe = doc.CreatePipe(p0, p1, item["CODE"]);
                    if(pipe == null)
                    {
                        continue;
                    }
                    // 更新参数
                    var push = pipe.UpdateParameter(doc, item, SheetName2);

                    if(push)
                    {
                        pipes.Add(pipe);
                    }
                }
                catch(System.Exception e)
                {
                    $"NewPipeByPoints:{e.Message}".ToLog();
                }
            }

            return pipes;
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="uiapp"></param>
        /// <param name="table"></param>
        /// <param name="attname"></param>
        /// <returns></returns>
        private bool AddParatemerByName(UIApplication uiapp, List<Dictionary<string, string>> table, string attname)
        {
            // 设置参数文件
            ShareParameters shareParatemer = new ShareParameters();
            // 添加参数
            List<bool> Push = new List<bool>();
            Transaction t = new Transaction(uiapp.ActiveUIDocument.Document, "_addParameter");
            t.Start();

            var data = table.FirstOrDefault();
            if(data == null)
            {
                return false;
            }
            foreach(var _key in data.Keys)
            {
                var key = ProduceXml.Xml.Read(attname, _key, attname);
                var isSuccess = shareParatemer.Create(uiapp, key, ParameterType.Text, true);
                if(isSuccess)
                {
                    Push.Add(isSuccess);
                }
            }
            // 处理事务
            _ = Push.Count > 0 ? t.Commit() : t.RollBack();
            return Push.Count > 0;
        }
    }
}
