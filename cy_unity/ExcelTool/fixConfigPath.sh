#!/bin/sh
# it run on macOS
# get current execute script directory
scriptPath=`cd $(dirname $0);pwd`
# get project dir, it is the parent directory of scriptPath
projectPath=`cd $scriptPath/../Assets/Scripts/Config;pwd`
excelPath=`cd $scriptPath/../../cy_excel_dev;pwd`
echo "scriptPath: $scriptPath"
echo "projectPath: $projectPath"
echo "excelPath: $excelPath"
# get machine user name
userName=`whoami`
# if user name is not 'droidhang', exit
if [ $userName != 'droidhang' ]; then
  echo "please run this script as user 'droidhang'"
  exit 1
fi
# copy scriptPath directionary file config_template.json to config.json
cd $scriptPath
cp config_template.json config.json
# replace '##unity_path##' with the variant $projectPath use sed
sed -i "" "s|##unity_path##|${projectPath}|g" config.json
# replace '##excel_path##' with $excelPath
sed -i "" "s|##excel_path##|${excelPath}|g" config.json
