using System.IO;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DH.Config;
using DHEditor.Toolset;
using DHFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace DH.Editor
{
    [InitializeOnLoad]
    public static class ExportExcelTools
    {
        private static readonly string WorkFolder = Environment.CurrentDirectory;

        private static readonly string
            ExcelFolder = $"{Environment.CurrentDirectory}_config/config";

        private static readonly string ExcelFolderCopy =
            $"{Environment.CurrentDirectory}_config/config";

        private static readonly string WinFilePath = Path.Combine($"{WorkFolder}", "ExcelTool", "exporter_windows.exe");
        private static readonly string MacFilePath = Path.Combine($"{WorkFolder}", "ExcelTool", "exporter");

        private static readonly string WinJson2BsonPath =
            $"{WorkFolder}/ExcelTool/json2bson_win.exe";

        private static readonly string MacJson2BsonPath = Path.Combine($"{WorkFolder}", "ExcelTool", "json2bson");
        private static readonly string ConfigOutputFolder = Path.Combine($"{WorkFolder}", "Assets", "Scripts", "Config");
        private static readonly string JsonPath = Path.Combine($"{ConfigOutputFolder}", "json");
        private static readonly string BsonPath = Path.Combine($"{ConfigOutputFolder}", "bson");
        private static readonly string ToolPath = Path.Combine($"{WorkFolder}", "ExcelTool");
        private static readonly string PythonPath = Path.Combine($"{WorkFolder}", "ExcelTool", "excel_i18n.py");
        
        private static readonly string MacConfigGeneratorPath =
            Path.Combine($"{WorkFolder}", "ExcelTool", "ConfigGenerator_arm");
        private static readonly string WinConfigGeneratorPath = Path.Combine($"{WorkFolder}", "ExcelTool", "ConfigGenerator_win.exe");

        public static void Execute()
        {
            // if (!Directory.Exists(ExcelFolder))
            // {
            //     throw new Exception($"[dherror]不存在配置文件目录{ExcelFolder}");
            // }
            
#if UNITY_EDITOR_OSX
            // var configJsonFile = $"{WorkFolder}/ExcelTool/config.json";
            // var configJson = File.ReadAllText(configJsonFile);
            // var configData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(configJson);
            // configData.dir.export_client = DHUtility.Path.GetRegularPath(ConfigOutputFolder);
            // configData.dir.xls = DHUtility.Path.GetRegularPath(ExcelFolderCopy);
            // File.WriteAllText(configJsonFile,
            //     Newtonsoft.Json.JsonConvert.SerializeObject(configData, Formatting.Indented));
#else
            // var configJsonFile = $"{WorkFolder}\\ExcelTool\\config.json";
            // var configJson = File.ReadAllText(configJsonFile);
            // var configData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(configJson);
            // configData.dir.export_client = DHUtility.Path.GetRegularPath($"{WorkFolder}\\Assets\\Scripts\\Config\\");
            // configData.dir.xls = DHUtility.Path.GetRegularPath($"{Environment.CurrentDirectory}_config\\config");
            // File.WriteAllText(configJsonFile,
            //     Newtonsoft.Json.JsonConvert.SerializeObject(configData, Formatting.Indented));
#endif
            // PrepareConfigs();

#if UNITY_EDITOR_OSX
            var result = CommandUtil.RunNormalCommand(MacFilePath, null, ToolPath);
            if (result.exitCode != 0)
            {
                Debug.LogError($"Export excel error {result.stderr}");
                // if (Application.isBatchMode)
                // {
                //     ExecutionEnvironment.EditorApplicationExit(-1);
                //     return;
                // }
            }

            result = CommandUtil.RunNormalCommand(MacJson2BsonPath, $"-d {JsonPath} -o {BsonPath}",
                ToolPath);
            if (result.exitCode != 0)
            {
                Debug.LogError($"Export excel error {result.stderr}");
            }
#else
            var result = CommandUtil.RunNormalCommand(WinFilePath, null, ToolPath,false);
            if (result.exitCode != 0)
            {
                Debug.LogError($"Export excel error {result.stderr}");
            }

            result = CommandUtil.RunNormalCommand(WinJson2BsonPath, $"-d {JsonPath} -o {BsonPath}", ToolPath,false);
            if (result.exitCode != 0)
            {
                Debug.LogError($"Export excel error {result.stderr}");
            }
#endif
            // GenGlobalLanguageId();
            // GenConfigCenter();
            // GenConfigAttributes();
            // GenDefineId();
            // GenFunctionJumpId();
#if UNITY_EDITOR_OSX
            result = CommandUtil.RunNormalCommand(MacConfigGeneratorPath, null, ToolPath);
            if (result.exitCode != 0)
            {
                Debug.LogError($"config generator excel error {result.stderr}");             
            }
#else
            result = CommandUtil.RunNormalCommand(WinConfigGeneratorPath, null, ToolPath, false);
            if (result.exitCode != 0)
            {
                Debug.LogError($"config generator excel error {result.stderr}");
            }
#endif
        }

        public static void PrepareConfigs()
        {
            string python_file = Application.dataPath + "/../ExcelTool/excel_i18n.py";
            CommandUtil.RunNormalCommand("python", python_file, ToolPath);
        }

        public static void GenGlobalLanguageId()
        {
            string rootDir = Environment.CurrentDirectory;

            var list = new List<string>();
            var cnPath = System.IO.Path.Combine(rootDir, "Assets", "Scripts", "Config", "json","strings", "cn", "GlobalLanguage.json");
            if (!File.Exists(cnPath))
            {
                return;
            }

            using (var fileStream = new FileStream(cnPath, FileMode.Open))
            {
                using (var reader = new StreamReader(fileStream))
                {
                    string lineStr;
                    while ((lineStr = reader.ReadLine()) != null)
                    {
                        string noEmptyLine = Regex.Replace(lineStr, @"\s", "");
                        if (noEmptyLine.StartsWith("\"id\":\""))
                        {
                            list.Add(noEmptyLine.Replace("\"id\":\"", "").Replace("\",", ""));
                        }
                    }
                }
            }

            string csPath = System.IO.Path.Combine(rootDir, "Assets", "Scripts", "Config","GlobalLanguageId.cs");
            if (File.Exists(csPath))
            {
                File.Delete(csPath);
            }


            using (var sw = new StreamWriter(csPath, false))
            {
                sw.WriteLine("/// <summary>");
                sw.WriteLine("/// 通过Global表自动生成");
                sw.WriteLine("/// 不要手动修改");
                sw.WriteLine("/// </summary>");
                sw.WriteLine("");
                sw.WriteLine("namespace DH.Config");
                sw.WriteLine("{");
                sw.WriteLine("    public class GlobalLanguageId");
                sw.WriteLine("    {");

                foreach (var id in list)
                {
                    if ('0' <= id[0] && id[0] <= '9')
                    {
                        continue;
                    }

                    sw.WriteLine($"       public const string {id} = \"{id}\";");
                }

                sw.WriteLine("    }");
                sw.WriteLine("}");
            }
        }

        public static void GenConfigCenter()
        {
            string rootDir = Environment.CurrentDirectory;
            var fileNames = new List<string>();

            string cfgDir = Path.Combine(rootDir, "Assets", "Scripts", "Config", "json/");
            var dirInfo = new DirectoryInfo(cfgDir);
            foreach (var fileInfo in dirInfo.GetFiles("*.json"))
            {
                string name = Path.GetFileNameWithoutExtension(fileInfo.Name);
                fileNames.Add(name);
            }

            string lanCfgDir = Path.Combine(rootDir, "Assets", "Scripts", "Config", "json", "strings", "en");
            var lanDirInfo = new DirectoryInfo(lanCfgDir);
            foreach (var fileInfo in lanDirInfo.GetFiles("*.json"))
            {
                string name = Path.GetFileNameWithoutExtension(fileInfo.Name);
                fileNames.Add(name);
            }

            string cfgPath = Path.Combine(rootDir, "Assets", "Scripts", "Config", "ConfigCenter.cs");
            if (File.Exists(cfgPath))
            {
                File.Delete(cfgPath);
            }

            var sw = new StreamWriter(cfgPath, false);
            sw.WriteLine("/// <summary>");
            sw.WriteLine("/// 通过配置表自动生成");
            sw.WriteLine("/// 不要手动修改");
            sw.WriteLine("/// </summary>");
            sw.WriteLine("");
            sw.WriteLine("using System.Collections.Generic;");
            sw.WriteLine("using DH.Config;");
            sw.WriteLine("");

            sw.WriteLine("namespace DH.Config");
            sw.WriteLine("{");
            sw.WriteLine("    public static class ConfigCenter");
            sw.WriteLine("    {");
            sw.WriteLine($"        public static List<ICfgCollectionBase> Configs = new List<ICfgCollectionBase>();");
            sw.WriteLine("");
            foreach (var fileName in fileNames)
            {
                sw.WriteLine($"        public static {fileName}CfgCollection {fileName}CfgColl;");
            }

            sw.WriteLine("");

            sw.WriteLine("        public static void InitConfigs()");
            sw.WriteLine("        {");

            foreach (var fileName in fileNames)
            {
                sw.WriteLine($"            {fileName}CfgColl = new {fileName}CfgCollection();");
                sw.WriteLine($"            Configs.Add({fileName}CfgColl);");
                sw.WriteLine("");
            }

            sw.WriteLine("        }");
            sw.WriteLine("");

            sw.WriteLine("        public static void Clear()");
            sw.WriteLine("        {");

            foreach (var fileName in fileNames)
            {
                sw.WriteLine($"            {fileName}CfgColl = null;");
            }

            sw.WriteLine("");
            sw.WriteLine("            Configs.Clear();");
            sw.WriteLine("        }");
            sw.WriteLine("");

            sw.WriteLine("        public static string GetGlobalString(string id, params object[] par)");
            sw.WriteLine("        {");
            sw.WriteLine("            if (GlobalLanguageCfgColl == null)");
            sw.WriteLine("                return id;");
            sw.WriteLine("            var cfg = GlobalLanguageCfgColl.GetDataById(id);");
            sw.WriteLine("            if (cfg == null)");
            sw.WriteLine("                return id;");
            sw.WriteLine("            string output = cfg.Name;");
            sw.WriteLine("            if (par == null || par.Length == 0)");
            sw.WriteLine("            {");
            sw.WriteLine("                return output;");
            sw.WriteLine("            }");
            sw.WriteLine("            else");
            sw.WriteLine("            {");
            sw.WriteLine("                return string.Format(output, par);");
            sw.WriteLine("            }");
            sw.WriteLine("        }");

            sw.WriteLine("    }");
            sw.WriteLine("}");
            sw.Close();
            sw.Dispose();
        }

        [MenuItem("DH Tools/生成属性类型")]
        public static void GenConfigAttributes()
        {
            string rootDir = Environment.CurrentDirectory;
            var path = System.IO.Path.Combine(rootDir, "Assets", "Scripts", "Config", "json","Attributes.json");
            if (!File.Exists(path))
            {
                return;
            }

            var jsonStr = File.ReadAllText(path);
            DH.Config.DataTable<AttributesCfg> dataTable =
                Newtonsoft.Json.JsonConvert.DeserializeObject<DataTable<AttributesCfg>>(jsonStr);

            string csPath =
                System.IO.Path.Combine(rootDir, "Assets", "Scripts", "Data", "Attribute", "AttributeType.cs");
            if (File.Exists(csPath))
            {
                File.Delete(csPath);
            }

            using var sw = new StreamWriter(csPath, false);
            sw.WriteLine("/// <summary>");
            sw.WriteLine("/// 通过Attributes表自动生成");
            sw.WriteLine("/// 不要手动修改");
            sw.WriteLine("/// </summary>");
            sw.WriteLine("");
            sw.WriteLine("using System.Collections.Generic;");
            sw.WriteLine("namespace DH.Data");
            sw.WriteLine("{");
            sw.WriteLine("\tpublic enum AttributeType");
            sw.WriteLine("\t{");
            foreach (var item in dataTable.Data)
            {
                var attrName = Regex.Replace(item.Name, "^[a-z]", m => m.Value.ToUpper());
                sw.WriteLine($"\t\t{attrName} = {item.Id},");
            }
            sw.WriteLine("\t}");
            
            sw.WriteLine();
            sw.WriteLine("\tpublic static class AttributeName");
            sw.WriteLine("\t{");
            foreach (var item in dataTable.Data)
            {
                var attrName = Regex.Replace(item.Name, "^[a-z]", m => m.Value.ToUpper());
                sw.WriteLine($"\t\tpublic const string {attrName} = \"{item.Name}\";");
            }
            
            sw.WriteLine();
            sw.WriteLine("\t\tpublic static readonly Dictionary<long,string> AttrIdToName = new ()");
            sw.WriteLine("\t\t{");
            foreach (var item in dataTable.Data)
            {
                var attrName = Regex.Replace(item.Name, "^[a-z]", m => m.Value.ToUpper());
                sw.WriteLine($"\t\t\t{{{item.Id},{attrName}}},");
            }
            sw.WriteLine("\t\t};");
            
            sw.WriteLine();
            sw.WriteLine("\t\tpublic static readonly Dictionary<string,long> AttrNameToId = new ()");
            sw.WriteLine("\t\t{");
            foreach (var item in dataTable.Data)
            {
                var attrName = Regex.Replace(item.Name, "^[a-z]", m => m.Value.ToUpper());
                sw.WriteLine($"\t\t\t{{{attrName},{item.Id}}},");
            }
            sw.WriteLine("\t\t};");
            sw.WriteLine("\t}");
            
            sw.WriteLine("}");
        }
        
        public static void GenDefineId()
        {
            string rootDir = Environment.CurrentDirectory;
            var list = new List<string>();
            var path = System.IO.Path.Combine(rootDir, "Assets", "Scripts", "Config", "json","Defines.json");
            if (!File.Exists(path))
            {
                return;
            }

            var reader = new StreamReader(new FileStream(path, FileMode.Open));
            string lineStr;
            string idStr = string.Empty;
            while ((lineStr = reader.ReadLine()) != null)
            {
                string noEmptyLine = Regex.Replace(lineStr, @"\s", "");
                if (noEmptyLine.StartsWith(@"""id"":"))
                {
                    idStr = noEmptyLine.Substring(5, noEmptyLine.Length - 6);
                    continue;
                }
                else if (!noEmptyLine.StartsWith(@"""attrib"":"))
                {
                    continue;
                }

                if (idStr != string.Empty)
                {
                    int idx = noEmptyLine.IndexOf(":") + 2;
                    list.Add(String.Format("\t\t{0} = {1},",
                        noEmptyLine.Substring(idx, noEmptyLine.Length - idx - 2), idStr));
                    idStr = string.Empty;
                }
            }

            string csPath =
                System.IO.Path.Combine(rootDir, "Assets", "Scripts", "Config", "DefineId.cs");
            if (File.Exists(csPath))
            {
                File.Delete(csPath);
            }

            var sw = new StreamWriter(csPath, false);
            sw.WriteLine("/// <summary>");
            sw.WriteLine("/// 通过Define表自动生成");
            sw.WriteLine("/// 不要手动修改");
            sw.WriteLine("/// </summary>");
            sw.WriteLine("");
            sw.WriteLine("namespace DH.Config");
            sw.WriteLine("{");
            sw.WriteLine("    public enum DefineCfgId");
            sw.WriteLine("    {");
            foreach (var item in list)
            {
                sw.WriteLine(item);
            }

            sw.WriteLine("    }");
            sw.WriteLine("}");
            sw.Close();
            sw.Dispose();
        }
        
          public static void GenFunctionJumpId()
        {
            string rootDir = Environment.CurrentDirectory;
            var list = new List<string>();
            var path = System.IO.Path.Combine(rootDir, "Assets", "Scripts", "Config", "json","FunctionJump.json");
            if (!File.Exists(path))
            {
                return;
            }

            var reader = new StreamReader(new FileStream(path, FileMode.Open));
            string lineStr;
            string idStr = string.Empty;
            while ((lineStr = reader.ReadLine()) != null)
            {
                string noEmptyLine = Regex.Replace(lineStr, @"\s", "");
                if (noEmptyLine.StartsWith(@"""id"":"))
                {
                    idStr = noEmptyLine.Substring(5, noEmptyLine.Length - 6);
                    continue;
                }
                else if (!noEmptyLine.StartsWith(@"""key"":"))
                {
                    continue;
                }

                if (idStr != string.Empty)
                {
                    int idx = noEmptyLine.IndexOf(":") + 2;
                    list.Add(String.Format("\t\t{0} = {1},", noEmptyLine.Substring(idx, noEmptyLine.Length - idx - 2), idStr));
                    idStr = string.Empty;
                }
            }

            string csPath =
                System.IO.Path.Combine(rootDir, "Assets", "Scripts", "Config", "FunctionJump.cs");
            if (File.Exists(csPath))
            {
                File.Delete(csPath);
            }

            var sw = new StreamWriter(csPath, false);
            sw.WriteLine("/// <summary>");
            sw.WriteLine("/// 通过FunctionJump表自动生成");
            sw.WriteLine("/// 不要手动修改");
            sw.WriteLine("/// </summary>");
            sw.WriteLine("");
            sw.WriteLine("namespace DH.Config");
            sw.WriteLine("{");
            sw.WriteLine("    public enum FunctionJumpCfgId");
            sw.WriteLine("    {");
            foreach (var item in list)
            {
                sw.WriteLine(item);
            }

            sw.WriteLine("    }");
            sw.WriteLine("}");
            sw.Close();
            sw.Dispose();
        }
    }
}