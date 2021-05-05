#!/usr/bin/python3
# -*- coding:utf-8 -*-
# @Time      : 2021-01-01
# @Author    : ZedMoster1@gmail.com

import os
import re
import sys
import clr
import math

clr.AddReference('ProtoGeometry')
from Autodesk.DesignScript.Geometry import *

clr.AddReference('RevitAPIUI')
clr.AddReference('RevitAPI')
import Autodesk
from Autodesk.Revit.DB import *
from Autodesk.Revit.UI import *

clr.AddReference('RevitNodes')
import Revit

clr.ImportExtensions(Revit.Elements)
clr.ImportExtensions(Revit.GeometryConversion)

clr.AddReference('RevitServices')
import RevitServices
from RevitServices.Persistence import DocumentManager
from RevitServices.Transactions import TransactionManager

# 默认参数
uiapp = DocumentManager.Instance.CurrentUIApplication
app = uiapp.Application
uidoc = uiapp.ActiveUIDocument
doc = DocumentManager.Instance.CurrentDBDocument

# 手动事务
TransactionManager.Instance.ForceCloseTransaction()
# 默认事务
# TransactionManager.Instance.EnsureInTransaction(doc)
# TransactionManager.Instance.TransactionTaskDone()

# 输出
OUT = UnwrapElement(IN[0])
