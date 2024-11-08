@echo off
set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\2022.3.30f1\Editor\Unity.exe"
set PROJECT_PATH="C:\Work_CY\cy_unity"

%UNITY_PATH% -batchmode -projectPath "%PROJECT_PATH%" -executeMethod DH.Editor.DHMenu.ExprotExcelForDesign -quit
echo "end"
