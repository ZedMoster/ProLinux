#!/usr/bin/python3
# -*- coding:utf-8 -*-
# @Time      : 2021-01-01
# @Author    : ZedMoster1@gmail.com

from Autodesk.Revit.UI import *
from Autodesk.Revit.DB import *
import Autodesk
import math
import os
import re
import sys

import clr
from System import Array

clr.AddReference('RevitAPI')
clr.AddReference('RevitAPIUI')
clr.AddReference("System")

doc = __revit__.ActiveUIDocument.Document
uidoc = __revit__.ActiveUIDocument
uiapp = __revit__.Application


def of_category(category, is_type=False):
    """获取 category"""
    col = FilteredElementCollector(doc).OfCategory(category)
    return col.WhereElementIsNotElementType().ToElements() if is_type else \
        col.WhereElementIsElementType().ToElements()


def of_class(typeof):
    """获取 Class"""
    return FilteredElementCollector(doc).OfClass(typeof).ToElements()


# 获取当前选择的构件
selection = [doc.GetElement(elId) for elId in uidoc.Selection.GetElementIds()]
# 获取element
el = selection[0] if len(selection) > 0 else "None pick element"
