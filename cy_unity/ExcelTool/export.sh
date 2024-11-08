#!/bin/sh
bash ./fixConfigPath.sh
#bash ./exportExcelForDesign.sh
./exporter
./json2bson -d ../Assets/Scripts/Config/json -o ../Assets/Scripts/Config/bson
./ConfigGenerator_arm

