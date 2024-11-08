#!/bin/sh
#
# get current execute script directory
scriptPath=`cd $(dirname $0);pwd`
# it run on macOS
# get machine user name
userName=`whoami`
# if user name is 'droidhang'
if [ $userName != 'droidhang' ]
then
    unityPath="/Applications/2022.3.30f1/Unity.app/Contents/MacOS/Unity"
else
    unityPath="/Applications/Unity/Hub/Editor/2022.3.30f1/Unity.app/Contents/MacOS/Unity"
fi

echo "unityPath:${unityPath}"

# get project dir, it is the parent directory of scriptPath
projectPath=`cd $scriptPath/../;pwd`
echo "scriptPath: $scriptPath"
echo "projectPath: $projectPath"

`${unityPath} -batchmode -projectPath "${projectPath}" -executeMethod DH.Editor.DHMenu.ExprotExcelForDesign -quit`
echo "export done."