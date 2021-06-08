
# 更新图纸定位到原点
t = Transaction(doc, "定位设置")
t.Start()
el.Location.Point = XYZ.Zero
t.Commit()
