t = Transaction(doc, "定位设置")
t.Start()
box = el.get_BoundingBox(None)
center = (box.Min + box.Max)*0.5

translation = XYZ.Zero - center
ElementTransformUtils.MoveElement(doc, el.Id, translation)
t.Commit()
