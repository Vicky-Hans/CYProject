import os
import tagWhiteList

def getFileName(filePath):
    return os.path.basename(filePath)

def getCurrentPath():
    currentFile = os.path.abspath(__file__)
    return os.path.dirname(currentFile)

def CheckTag(filePath):
    noTagCount=0
    with open(filePath, 'r') as file:
        lines = file.readlines()
        for i in range(len(lines)):
            if 'Text (TMP)' in lines[i] and i+1 < len(lines):
                if 'FontText' in lines[i+1] or 'FontNum' in lines[i+1]:
                    pass
                else:
                    noTagCount = noTagCount + 1
        return noTagCount

def generate_for_prefabs(path):
    wlist = tagWhiteList.wlist
    for root, dirs, files in os.walk(path):
        for file in files:
            if file.endswith(".prefab"):
                filePath = os.path.join(root, file)
                count = CheckTag(filePath)
                if count > 0:
                    filename = getFileName(filePath)
                    if filename in wlist and wlist[filename] >= count:
                        continue
                    print "warning file: %s, no tag count:%d"%(filename, count)

if __name__ == '__main__':
    currentPath = getCurrentPath()
    path = os.path.join(currentPath, "../", "Assets/GameAssets/Prefabs/")
    generate_for_prefabs(path)
