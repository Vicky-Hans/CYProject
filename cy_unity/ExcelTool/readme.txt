# 关于导表工具excel_i18n.py的说明

-   ** **
    1. 由于多语言表整合在一起了，目录结构不适用exporter导表工具，所以创建tf_config_copy副本目录
    2. 程序会将 tf_config复制到同级目录tf_config_copy
    3. 程序会将tf_config_copy目录中的strings文件所有xlsx文件拆分成符号exporter需求的目录结构
    4. exporter使用tf_config_copy进行导表


# 软件及环境要求

-   ** **
    1. python2.7
    2. 安装python模块shutil、xlrd、openpyxl、json模块

# 配置变化
    ExcelTool/config.json 中data.xls tf_config需要改成tf_config_copy
