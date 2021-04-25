els = FilteredElementCollector(doc).OfCategory(
        BuiltInCategory.OST_StructuralColumns).WhereElementIsNotElementType().ToElements()

work = False
t = Transaction(doc, "更新柱编号")
t.Start()

error = []
for el in els:
    try:
        el.LookupParameter("柱编号").Set(el.Name)
        work = True
    except:
        error.append(el)
if work:
    t.Commit()
else:
    t.RollBack()
if len(error) == 0:
    print("Done!")
else:
    print("错误个数%s" % len(error))
