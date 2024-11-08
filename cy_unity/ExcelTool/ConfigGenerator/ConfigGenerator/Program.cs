// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;
using ConfigGenerator;
using Newtonsoft.Json;

Exporter.Export();

public static class Exporter
{
    private static readonly string WorkFolder = Environment.CurrentDirectory;

    /// <summary>
    /// 返回 WorkFolder 中 Assets 文件夹的父路径
    /// </summary>
    /// <returns></returns>
    public static string GetRootDir()
    {
        return WorkFolder.Split("ExcelTool")[0];
    }
    
    public static void Export()
    {
        // var objA = new AttributesCfg(1, 1,"", "", "", 0, 1, "");
        // var obj1 = new DataTable<AttributesCfg>(new List<AttributesCfg> { objA });
        // Console.WriteLine($"test data:{obj1.Data[0].Id}");
        var rootDir = GetRootDir();
        Console.WriteLine($"Exporting...dir:{WorkFolder} root:{rootDir}");
        GenGlobalLanguageId();
        GenConfigCenter();
        GenConfigAttributes();
        GenDefineId();
        GenFunctionJumpId();
        Console.WriteLine("....config generator all done!");
    }
    
        public static void GenGlobalLanguageId()
        {
            string rootDir = GetRootDir();

            var list = new List<string>();
            var cnPath = System.IO.Path.Combine(rootDir, "Assets", "Scripts", "Config", "json","strings", "cn", "GlobalLanguage.json");
            if (!File.Exists(cnPath))
            {
                Console.WriteLine($"path:{cnPath} not existed");
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
            string rootDir = GetRootDir();
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
            
            fileNames.Sort();

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

        public static void GenConfigAttributes()
        {
            string rootDir = GetRootDir();
            var path = System.IO.Path.Combine(rootDir, "Assets", "Scripts", "Config", "json","Attributes.json");
            if (!File.Exists(path))
            {
                Console.WriteLine($"path:{path} not existed");
                return;
            }

            var jsonStr = File.ReadAllText(path);
            // var jsonObj = Newtonsoft.Json.Linq.JObject.Parse(jsonStr);
            var dataTable = JsonConvert.DeserializeObject<DataTable<AttributesCfg>>(jsonStr);

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
            sw.Close();
            sw.Dispose();
        }
        
        public static void GenDefineId()
        {
            string rootDir = GetRootDir();
            var list = new List<string>();
            var path = System.IO.Path.Combine(rootDir, "Assets", "Scripts", "Config", "json","Defines.json");
            if (!File.Exists(path))
            {
                Console.WriteLine($"path:{path} not existed");
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
            reader.Close();
            reader.Dispose();

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
            string rootDir = GetRootDir();
            var list = new List<string>();
            var path = System.IO.Path.Combine(rootDir, "Assets", "Scripts", "Config", "json","FunctionJump.json");
            if (!File.Exists(path))
            {
                Console.WriteLine($"path:{path} not existed");
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
            reader.Close();
            reader.Dispose();

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