#!/usr/bin/python3
# -*- coding:utf-8 -*-
# @Time      : 2021-01-01
# @Author    : ZedMoster1@gmail.com


def transactionGroup(func):
    """事务-组"""
    
    def wrapper(*args, **kwargs):
        t = TransactionGroup(doc, "tran-group")
        t.Start()
        try:
            f = func(*args, **kwargs)
            if f:
                t.Assimilate()
            else:
                t.RollBack()
        except Exception as e:
            f = None
            print("Error:%s" % e)
            t.RollBack()
        return f
    
    return wrapper


def transaction(func):
    """事务"""
    
    def wrapper(*args, **kwargs):
        t = Transaction(doc, "tran-")
        t.Start()
        try:
            f = func(*args, **kwargs)
            if f:
                t.Commit()
            else:
                t.RollBack()
        except Exception as e:
            f = None
            print("Error:%s" % e)
            t.RollBack()
        return f
    
    return wrapper


class PickSelectionFilterByCategories(Selection.ISelectionFilter):
    def __init__(self, category_opt):
        self.category_names = category_opt
    
    def AllowElement(self, el):
        return el.Category.Name in self.category_names
    
    def AllowReference(self, ref, point):
        return true


class PickSelectionFilterByCategory(Selection.ISelectionFilter):
    def __init__(self, category_opt):
        self.category_name = category_opt
    
    def AllowElement(self, element):
        return element.Category.Name == self.category_name
    
    def AllowReference(self, ref, point):
        return true


class P_object(object):
    """Object Base"""
    
    @staticmethod
    def to_iter(obj):
        """object对象可迭代处理"""
        return obj if hasattr(obj, "__iter__") else [obj]
    
    @staticmethod
    def double_to_int(double, inter):
        """数值取整"""
        zs = double / inter
        cs = double // inter
        module = 1 if round((zs - cs), 1) >= 0.5 else 0
        return int(cs * inter + inter * module)
    
    @staticmethod
    def to_decimal_feet(value):
        """毫米单位转英尺"""
        return UnitUtils.Convert(value, DisplayUnitType.DUT_MILLIMETERS, DisplayUnitType.DUT_DECIMAL_FEET)
    
    @staticmethod
    def to_millimeter(value):
        """英尺单位转毫米"""
        return UnitUtils.Convert(value, DisplayUnitType.DUT_DECIMAL_FEET, DisplayUnitType.DUT_MILLIMETERS)
    
    @staticmethod
    def get_parameter_by_name(element, name):
        """通过名称获取参数值"""
        value = None
        parameter = element.LookupParameter(name)
        if parameter:
            value = parameter.AsValueString()
            if parameter.StorageType == StorageType.Double:
                value = parameter.AsDouble()
            elif parameter.StorageType == StorageType.ElementId:
                value = parameter.AsElementId()
            elif parameter.StorageType == StorageType.Integer:
                value = parameter.AsInteger()
            elif parameter.StorageType == StorageType.String:
                value = parameter.AsString()
            else:
                print("StorageType.None")
        return value
    
    @staticmethod
    def set_parameter_by_name(element, name, value):
        """设置对象指定参数名称的参数值"""
        element.LookupParameter(name).Set(value)
    
    @staticmethod
    def get_column_grids(column, grids):
        """获取柱定位轴网对象"""
        tag = column.LookupParameter("柱定位标记").AsString()
        c_grids = [g for g in grids if g.Name in tag]
        is_match = bool(re.search("^".format(c_grids[0].Name), tag))
        x_grid = c_grids[0] if is_match else c_grids[1]
        y_grid = c_grids[1] if is_match else c_grids[0]
        return x_grid, y_grid


class P_Filter(P_object):
    """过滤器"""
    
    @staticmethod
    def of_class(typeof):
        """通过【class类型】过滤对象"""
        return FilteredElementCollector(doc).OfClass(typeof).ToElements()
    
    @staticmethod
    def of_category(typeof, category):
        """通过【class类型及类别】过滤对象"""
        return FilteredElementCollector(doc).OfClass(typeof).OfCategory(category).ToElements()
    
    @staticmethod
    def get_element_type_name(element_type):
        """通过名称获取指定类型的名称"""
        return Element.Name.GetValue(element_type)
    
    def get_element_type_by_name(self, element_types, name):
        """通过名称获取Element"""
        el = [i for i in element_types if self.getElementName(i) == name]
        return el[0] if el else None


class P_Geometry(P_object):
    """模型实体"""
    
    @staticmethod
    def flatten_xyz(xyz, z=0):
        """点拍平到面.默认Z=0"""
        return XYZ(xyz.X, xyz.Y, z)
    
    def flatten_line(self, line, z=0):
        """线段拍平到面.默认Z=0"""
        return Autodesk.Revit.DB.Line.CreateBound(self.flatten_xyz(line.GetEndPoint(0), z),
                                                  self.flatten_xyz(line.GetEndPoint(1), z)) \
            if type(line) == Autodesk.Revit.DB.Line else line
    
    @staticmethod
    def is_almost_equal_zero(value, tolerance=0.001):
        """约等于0"""
        return abs(value) < tolerance
    
    @staticmethod
    def is_parallel(dir0, dir1):
        """平行"""
        return dir0.IsAlmostEqualTo(dir1) or dir0.IsAlmostEqualTo(dir1.Negate())
    
    @staticmethod
    def is_vertical(dir1, dir2, tolerance=0.001):
        """垂直"""
        return abs(dir1.DotProduct(dir2)) < tolerance
    
    @staticmethod
    def is_Joined(face, line):
        """相交"""
        return face.Intersect(line) != SetComparisonResult.Disjoint
    
    @staticmethod
    def get_line_offset_XYZ(line, vector):
        """给定线段偏移向量"""
        return Autodesk.Revit.DB.Line.CreateBound(
                line.GetEndPoint(0) + vector,
                line.GetEndPoint(1) + vector) \
            if type(line) == Autodesk.Revit.DB.Line else line
    
    @staticmethod
    def rotation_direction_90(direction):
        """向量平面环境逆时针旋转90°"""
        return XYZ(-direction.Y, direction.X, direction.Z)
    
    @staticmethod
    def get_element_geometry(element):
        """获取模型的实体包含的全部Solid"""
        solids = []
        try:
            opt = Options()
            opt.ComputeReferences = True
            geo = element.get_Geometry(opt)
            for g in geo:
                if g.GetType() == GeometryInstance:
                    # 判断实体是否已经自动进行修改
                    gg = g.GetSymbolGeometry() if element.HasModifiedGeometry() else g.GetInstanceGeometry()
                    for g_ins in gg:
                        if g_ins.GetType() == Solid:
                            solids.append(g_ins)
                else:
                    if g.GetType() == Solid:
                        solids.append(g)
        except Exception as e:
            print("异常：%s" % e)
        return solids
    
    @staticmethod
    def face_to_solid(face):
        """通过面创建实体solid"""
        curve_loops = face.GetEdgesAsCurveLoops()
        solid = GeometryCreationUtilities.CreateExtrusionGeometry(curve_loops, face.FaceNormal, 0.01)
        return solid
    
    def get_parallel_solid_faces(self, direction, solid, isParallel=True):
        """获取solid与给定方向平行的面或者方向相同的面"""
        faces = []
        fs = solid.Faces.GetEnumerator()
        while fs.MoveNext():
            face = fs.Current
            if isParallel:
                if self.is_parallel(face.FaceNormal, direction):
                    faces.append(face)
            else:
                if face.FaceNormal.IsAlmostEqualTo(direction):
                    faces.append(face)
        return faces
    
    def is_intersect_solid_edges(self, curve, solid, flatten=False):
        """判断给定的Curve是否与实体Edge是否相交"""
        edges = solid.Edges.GetEnumerator()
        while edges.MoveNext():
            edge = edges.Current
            line = edge.AsCurve()
            if line.Length < 0.001:
                # Curve length is too small for Revit’s tolerance
                continue
            result_array = clr.Reference[IntersectionResultArray]()
            _curve = self.flatten_line(edge.AsCurve()) if flatten else edge.AsCurve()
            result = curve.Intersect(_curve, result_array)
            if result == SetComparisonResult.Overlap:
                return True
        return False
    
    @staticmethod
    def curves_to_solid(curves):
        """拉伸curves创建实体"""
        solid = None
        if curves:
            profile = CurveLoop()
            for c in curves:
                profile.Append(c)
            try:
                solid = GeometryCreationUtilities.CreateExtrusionGeometry([profile], XYZ.BasisZ, 1)
            except Exception as e:
                print("创建实体失败：%s" % e)
        return solid
    
    @staticmethod
    def is_solid_intersect(solid_1, solid_2):
        """判断两个实体是否相交"""
        intersection = BooleanOperationsUtils.ExecuteBooleanOperation(solid_1, solid_2, BooleanOperationsType.Intersect)
        return intersection.Volume > 0
    
    @staticmethod
    def get_element_box_center_point(el):
        """获取element对象包围盒中心点"""
        box_XYZ = el.get_BoundingBox(doc.ActiveView)
        return (box_XYZ.Min + box_XYZ.Max) * 0.5
    
    @staticmethod
    def get_face_box_center_point(face):
        """获取face面对象包围盒中心点"""
        box_uv = face.GetBoundingBox()
        center_uv = (box_uv.Max + box_uv.Min) * 0.5
        return face.Evaluate(center_uv)


class P_UI(P_object):
    """UI窗口"""
    
    @staticmethod
    def to_pick_point():
        """选择定位点"""
        return uidoc.Selection.PickPoint("确定标注方向")
    
    @staticmethod
    def print(string):
        """弹窗显示提示信息"""
        TaskDialog.Show("提示", "[%s]" % string)


class P_Create(P_object):
    """创建对象"""
    
    @staticmethod
    @transaction
    def new_instance_by_point(point, symbol, structural=Structure.StructuralType.NonStructural):
        try:
            instance = doc.Create.NewFamilyInstance(point, symbol, doc.ActiveView.GenLevel, structural)
            return instance
        except Exception as e:
            print("创建异常：%s" % e)
            return None
    
    @transaction
    @staticmethod
    def new_instance_by_curve(curve, symbol, structural=Structure.StructuralType.Beam):
        try:
            instance = doc.Create.NewFamilyInstance(curve, symbol, doc.ActiveView.GenLevel, structural)
            return instance
        except Exception as e:
            print("创建异常：%s" % e)
            return None
    
    @transaction
    @staticmethod
    def new_dimension(active_view, line, references, dim_type):
        try:
            dim = doc.Create.NewDimension(active_view, line, references)
            # 删除0标注
            to_del = []
            se = list(dim.Segments)
            rf = list(dim.References)
            for i in range(len(se)):
                if se[i].ValueString == "0":
                    to_del.append(i)
            # 清理 0 标注
            if to_del:
                _reference = ReferenceArray()
                for j in range(len(rf)):
                    if j in to_del:
                        continue
                    _reference.Append(rf[j])
                doc.Delete(dim.Id)
                dim = doc.Create.NewDimension(self._active_view, line, _reference)
            if dim_type:
                dim.ChangeTypeId(dim_type.Id)
            return dim
        except Exception as e:
            print("标注错误：%s" % e)
            return None


class P_Revit(P_Filter, P_Geometry, P_Create, P_UI):
    """Revit API 项目类"""
    pass


if __name__ == '__main__':
    r = P_Revit()
    print(r.__doc__)
