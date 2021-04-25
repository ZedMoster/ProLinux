﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class PickModel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            if (Run.Running(Strings.key))
            {
                #region 选择模型类别的构件
                try
                {
                    PickModelSelectionFilter selectionFilter = new PickModelSelectionFilter();
                    // 空格完成多选
                    ToFinish toFinish = new ToFinish();
                    toFinish.Subscribe();
                    SelPick.SelRefsList = sel.PickObjects(ObjectType.Element, selectionFilter, "<选择模型类别的构件>").ToList();
                    toFinish.Unsubscribe();
                }
                catch { SelPick.SelRefsList = new List<Reference>(); }
                if (SelPick.SelRefsList.Count == 0)
                {
                    return Result.Cancelled;
                }
                List<ElementId> elementIds = new List<ElementId>();
                foreach (var item in SelPick.SelRefsList)
                {
                    elementIds.Add(item.ElementId);
                }
                #endregion

                sel.SetElementIds(elementIds);
                uidoc.RefreshActiveView();
            }

            return Result.Succeeded;
        }
    }
}
