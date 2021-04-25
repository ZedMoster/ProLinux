familyManager = doc.FamilyManager
trans = Transaction(doc, "参数转换")
trans.Start()
for familyParameter in familyManager.Parameters:
    try:
        familyManager.MakeType(familyParameter)
        #familyManager.MakeInstance(familyParameter)
    except:
        pass
trans.Commit()