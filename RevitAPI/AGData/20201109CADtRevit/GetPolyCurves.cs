using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace CADReader
{
    [Transaction(TransactionMode.Manual)]
    class GetPolyCurves : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            // 选择链接图纸信息
            Pickdwg pickdwg = new Pickdwg();

            #region 选择CAD图纸信息
            // 获取定位线
            pickdwg.Refer(sel, out Reference referenceModel);
            if (referenceModel == null)
            {
                MessageBox.Show("已取消");
                return Result.Cancelled;
            }
            // 获取图形
            CADElement CADModel = new CADElement();
            // 获取选择图形的定位点
            var models = CADModel.GetLayerCurves(doc, referenceModel);
            #endregion

            List<Curve> curves = new List<Curve>();
            foreach (var item in models)
            {
                foreach (var curve in item.Curves)
                {
                    curves.Add(curve);
                }
            }

            var group = GroupCurves(curves);

            string msg = "";
            foreach (var item in group)
            {
                msg += item.Curves.Count.ToString() + "\n";
            }
            MessageBox.Show(msg);


            return Result.Succeeded;
        }



        /// <summary>
        /// 获取闭合多段线
        /// </summary>
        /// <param name="curves"></param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        private List<GroupCurve> GroupCurves(List<Curve> curves, double ignore = 0.001)
        {
            List<GroupCurve> curvesModels = new List<GroupCurve>();
            var queue = new Queue<Curve>();
            while (curves.Count > 0)
            {
                queue.Enqueue(curves.LastOrDefault());
                List<Curve> shape = new List<Curve>();
                while (queue.Count > 0)
                {
                    var current_curve = queue.Dequeue();
                    shape.Add(current_curve);
                    var current_points = current_curve.Tessellate();
                    foreach (var mathch in curves)
                    {
                        var P1 = mathch.Tessellate()[0];
                        foreach (var P2 in current_points)
                        {
                            var distance = P1.DistanceTo(P2);
                            if (distance <= ignore && !shape.Contains(mathch))
                            {
                                queue.Enqueue(mathch);
                                break;
                            }
                        }
                    }
                    var _curves = new List<Curve>();
                    foreach (var item in curves)
                    {
                        if (!shape.Contains(item))
                        {
                            _curves.Add(item);
                        }
                    }
                    curves = _curves;
                }
                if (shape.Count > 1)
                {
                    curvesModels.Add(new GroupCurve() { Curves = shape });
                }
            }
            return curvesModels;
        }
    }

    class GroupCurve
    {
        public List<Curve> Curves { get; set; }
    }
}
