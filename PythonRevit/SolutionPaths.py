class Solution:
    def __init__(self, a, b):
        """初始化网格节点"""
        self.a = a
        self.b = b
        self.Mat = [[-1 for _ in range(b)] for _ in range(a)]
        self.paths(a - 1, b - 1)
        # 网格
        self.__show__()

    def paths(self, row, col):
        """创建路径"""
        if row <= 0 or col <= 0:
            self.Mat[row][col] = 1
            return 1
        self.Mat[row][col - 1] = self.paths(row, col - 1)
        self.Mat[row - 1][col] = self.paths(row - 1, col)
        self.Mat[row][col] = self.Mat[row][col - 1] + self.Mat[row - 1][col]
        return self.Mat[row][col]

    def unique_path_count(self):
        """获取总计路径个数"""
        return self.Mat[self.a - 1][self.b - 1]

    def __show__(self):
        print("行:{0} 列:{1}".format(self.a, self.b))
        print("-" * 20)
        for i in self.Mat:
            print(i)
        print("-" * 20)


sol = Solution(3, 4)
print("总计路径个数:%s" % sol.unique_path_count())
