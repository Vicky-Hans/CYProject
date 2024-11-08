# coding=utf-8
import os
import sys
import shutil
import xlrd
import openpyxl
import json

HEADER_ROW = 2

def getDirXlsPath(filename):
    # 读取文件名为filename的json文件，获取key为dir.xls的值
    with open(filename, 'r') as file:
        data = json.load(file)
        dir_xls_path = data['dir']['xls']
    dir_xls_path = dir_xls_path.replace('_copy', '')
    return dir_xls_path

def getExcutePath():
    # 获取当前执行路径
    return os.getcwd()

def deleteFile(filename):
    # 删除文件filename
    os.remove(filename)

def deleteDir(path):
    # 删除path目录
    shutil.rmtree(path)

def mkdir(path):
    # 创建path路径，如果path存在则什么都不做
    if not os.path.exists(path):
        os.makedirs(path)

def cpDir(src_path, dst_path):
    # 递归拷贝src_path下所有文件到dst_path下
    shutil.copytree(src_path, dst_path)

def isContainJoint(h):
    # Check if h contains the '-' symbol
    if '_' in h:
        return True
    else:
        return False

def splitHeader(h):
    # 使用'-'分割字符串h
    return h.split('_')

def getColData(sheet, col_name):
    # 获取表单中，名字为col_name的整列数据
    col_data = [sheet.cell_value(row, col) for row in range(sheet.nrows) for col in range(sheet.ncols) if sheet.cell_value(HEADER_ROW, col) == col_name]
    return col_data

def writeCols(filename, dict_array):
    # 创建一个新的工作簿
    workbook = openpyxl.Workbook()
    # 选择默认的活动表单
    sheet = workbook.active

    # 将dict_array的元素写入excel表单的不同列
    for i, row in enumerate(dict_array):
        for j, value in enumerate(row):
            sheet.cell(j+1, i+1, value)

    # 保存excel文件
    workbook.save(filename)

def readExcel(path, filename):
    # 使用xlrd读取文件名为filename的excel
    excel_data = xlrd.open_workbook(os.path.join(path, filename))
    
    # 将excel数据转换为字典数组
    dict_array = []
    sheet = excel_data.sheet_by_index(0)
    headers = [sheet.cell_value(HEADER_ROW, col) for col in range(sheet.ncols)]
    lggs = set()
    key = headers[0]
    col_names = set()
    for header in headers:
        if not isContainJoint(header):
            continue
        name, lgg = splitHeader(header)
        col_names.add(name)
        lggs.add(lgg)
    # print("key:%s"%(key))
    # print(col_names)
    # print(lggs)
    sub_headers = [key]
    for col_name in col_names:
        sub_headers.append(col_name)
    sheets = {}
    for lgg in lggs:
        sheets[lgg] = []
        col_data = getColData(sheet, sub_headers[0])
        sheets[lgg].append(col_data)
        for col_name in col_names:
            old_col_name = "%s_%s"%(col_name, lgg)
            col_data = getColData(sheet, old_col_name)
            col_data[HEADER_ROW] = col_name
            sheets[lgg].append(col_data)
    for lgg in lggs:
        filepath = "%s/%s"%(path,lgg)
        mkdir(filepath)
        writeCols("%s/%s"%(filepath, filename), sheets[lgg])


def walkdir(path):
    # 遍历path目录下每个.xlsx文件
    for root, dirs, files in os.walk(path, False):
        for file in files:
            if file.endswith(".xlsx"):
                file_path = os.path.join(root, file)
                readExcel(path, file)
                deleteFile(os.path.join(path, file))
    

# if  __name__ == "__main__":
def main():
    curr_path = getExcutePath()
    json_file_path = "%s/config.json"%(curr_path)
    if not os.path.exists(json_file_path):
        print("config path [%s] error...."%{json_file_path})
        exit(-1)
    xls_path = getDirXlsPath(json_file_path)
    print("xls_path:", xls_path)
    if not os.path.exists(xls_path):
        print("config path error....")
        exit(-1)
    config_path, sub_path = os.path.split(xls_path)
    work_copy = "%s_copy"%(config_path)
    strings_path = os.path.join(work_copy, sub_path, "strings")
    if os.path.exists(work_copy):
        deleteDir(work_copy)
    #mkdir(work_copy)
    cpDir(config_path, work_copy)
    print(strings_path)
    if not os.path.exists(strings_path):
        print("strings path not exist")
        exit(-2)
    walkdir(strings_path)

main()
