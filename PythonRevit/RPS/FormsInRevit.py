# -*- coding:utf-8 -*-
# @Time      : 2020-12-14
# @Author    : xml

class Forms:
    def __init__(self, elementList):
        '''
        移动构件确认窗口

        Args:
            elementList: 包含列表构件 [element,XYZ(vector)] 的列表数据
        '''
        self.__count = len(elementList) if elementList else 0
        self.els = elementList
        self.__start = 0

    def __XYZToM__(self, xyz):
        return (round(xyz.X * 304.8), round(xyz.Y * 304.8), round(xyz.Z * 304.8))

    def __ShowDialog(self, i):
        td = TaskDialog("设置")
        td.MainContent = "调整构件定位"
        td.FooterText = "否：跳过构件 || 重试：上一个构件 || 关闭：终止程序";
        td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1,
                          "功能：xx",
                          "类型：%s\n距离：%s" % (i[0], i[1]))
        td.CommonButtons = TaskDialogCommonButtons.No | TaskDialogCommonButtons.Retry | TaskDialogCommonButtons.Close
        return td.Show()

    def ShowDialog(self):
        while self.__start < self.__count:
            #t = Transaction(doc, "预处理")
            #t.Start()
            #uidoc.ShowElements(self.els[self.__start][0])
            #ElementTransformUtils.MoveElement(doc, self.els[self.__start][0].Id, self.els[self.__start][1])
            tResult = self.__ShowDialog(self.els[self.__start])
            # 处理选择的情况
            if tResult == TaskDialogResult.CommandLink1:
                #t.Commit()
                print("处理构件：%s...." % self.__start)
                self.__start += 1
            elif tResult == TaskDialogResult.No:
                #t.RollBack()
                print("跳过构件：%s...." % self.__start)
                self.__start += 1
            elif tResult == TaskDialogResult.Retry:
                self.__start -= 0 if self.__start == 0 else 1
                print("上一构件：%s...." % self.__start)
                #t.RollBack()
            elif tResult == TaskDialogResult.Close:
                #t.Commit()
                print("剩余构件：%s...." % (self.__count - self.__start))
                break
            else:
                #t.RollBack()
                print("手动关闭：%s...." % self.__start)
                break
        else:
            print("结束程序....")


if __name__ == '__main__':
    froms = Forms([[i, i + 100] for i in range(10)])
    froms.ShowDialog()