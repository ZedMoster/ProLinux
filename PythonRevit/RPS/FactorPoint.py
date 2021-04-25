# -*- coding:utf-8 -*-
# @Time      : 2020-09-14
# @Author    : xml

class PointFactory:
    def __init__(self,x, y, xl, yl):
        self.list = self.__factorList(x, y, xl, yl)

    def __factorList(self, x, y, xl, yl):
        XYZ = []
        x_eq = xl/(2*(x))
        y_eq = yl/(2*(y))
        for i in range (0, x):
            for j in range(0, y):
                a = x_eq + i*x_eq*2
                b = y_eq + j*y_eq*2
                XYZ.append([a, b])
        return  XYZ

    def factorPoint(self, p_loc, x_dir, y_dir):
        res = []
        for i in range(len(self.list)):
            res.append([(p_loc[0] + self.list[i][0])*x_dir, (p_loc[1] + self.list[i][1])*y_dir])
        return  res
