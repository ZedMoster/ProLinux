# -*- coding:utf-8 -*-
# @Time      : 2020-12-12
# @Author    : xml

class FactorCalcRoom():
    def __init__(self, n):
        self.__myList = self.__factorisation(n)

    def __mult(self, myList):
        result = 1
        for x in myList:
            result = result * x
        return result

    def __factorisation(self, n):
        t = n
        result = []
        if n < 1:
            return result
        if t == 1:
            result.append(1)
        else:
            i = 2
            while True:
                if t == i:
                    result.append(i)
                    break
                if t % i == 0:
                    result.append(i)
                    t = t / i
                else:
                    i += 1
        return result

    def main(self):
        result = []
        if len(self.__myList) == 0:
            return [[0, "参数输入错误"]]
        else:
            result.append([1, self.__mult(self.__myList)])
            for i in range(len(self.__myList) - 1):
                x = self.__mult(self.__myList[:i + 1])
                y = self.__mult(self.__myList[i + 1:])
                this = [x, y] if x < y else [y, x]
                if this not in result:
                    result.append(this)

        return result


if __name__ == '__main__':
    import random

    IN = [random.randint(1, 30)]
    print("方案随机数：%s" % IN[0])
    print(FactorCalcRoom(IN[0]).main())
