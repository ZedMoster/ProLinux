import os
import re


class file(object):
    @staticmethod
    def read_file(filepath):
        if not os.path.exists(filepath):
            return None
        with open(filepath, "r", encoding="utf-8") as f:
            file_data = f.read()
        print("读取成功：%s" % os.path.basename(filepath))
        return file_data
    
    @staticmethod
    def save_file(filepath, data):
        with open(filepath, "w+", encoding="utf-8") as f:
            f.write(data)
        print("保存成功：%s" % os.path.basename(filepath))


class ProductVersion(file):
    def __init__(self, filepath=None, start=2016, end=2020):
        self.filepath = filepath if filepath else input("输入文件地址：\n")
        # 获取文件版本
        self.versions = [i for i in range(start, end + 1)]
        
        # 文件地址参数：文件地址所在文件夹
        self._dir_name = os.path.dirname(self.filepath)
        # 文件地址参数：文件名称及类型
        self._full_name = os.path.basename(self.filepath)
        # 文件地址参数：获取文件版本
        self._version = re.findall(r'(\d{4})', self.filepath.split("\\")[-1])[0]
        
        # 显示信息
        self._show_dialog()
        
        # 读取文件内容
        self._file_data = self.read_file(self.filepath)
    
    def _show_dialog(self):
        print("%s" % "-" * 45)
        print("** 文件位置：%s **" % self._dir_name)
        print("** 文件名称：%s **" % self._full_name)
        print("** 当前版本：%s **" % self._version)
        print("** 待建版本：%s **" % self.versions)
        print("%s" % "-" * 45)
    
    def _new_file_path(self, version):
        fullname = self._full_name.replace(self._version, str(version))
        return os.path.join(self._dir_name, fullname)
    
    def _new_file_data(self, version):
        return re.sub(self._version, str(version), self._file_data)
    
    def to_save_other_version(self):
        if not self._file_data:
            print("文件不存：%s" % self.filepath)
        # 读取文件内容
        for _version in self.versions:
            if self._version == str(_version):
                continue
            # 保存文件
            new_data = self._new_file_data(_version)
            new_path = self._new_file_path(_version)
            self.save_file(new_path, new_data)


if __name__ == '__main__':
    # 配置输出缓存
    # <IntermediateOutputPath>obj\Debug\2020\</IntermediateOutputPath>
    # <IntermediateOutputPath>obj\Release\2020\</IntermediateOutputPath>
    
    path = r"C:\Users\xml\Desktop\DtreeUtils2020.csproj"
    p = ProductVersion()
    p.to_save_other_version()
