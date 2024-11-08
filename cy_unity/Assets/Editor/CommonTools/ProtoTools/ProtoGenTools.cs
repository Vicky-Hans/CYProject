using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace DH.Editor
{
    public static class ProtoGenTools
    {
        private const string pbDir = "pb";

        private static List<List<PbInfo>> allInfos;

        public class PbInfo
        {
            public int reqId;
            public int rspId;
            public string req;
            public string rsp;
        }

        public static void GenPB()
        {
            GenMsgIdProto();
            if (!Proto2CS())
            {
                return;
            }
            GenMsgTypeDict();

            AssetDatabase.Refresh();
        }

        private static void GenMsgIdProto()
        {
            string rootDir = Environment.CurrentDirectory;
            string msgIdPath = Path.Combine(rootDir, "Proto", pbDir, "msg_id.proto");
            var fileNames = GetAllPbFileNames();
            if (fileNames.Contains("msg_id.proto"))
            {
                fileNames.Remove("msg_id.proto");
                File.Delete(msgIdPath);
            }

            allInfos = new List<List<PbInfo>>();
            foreach (var fileName in fileNames)
            {
                var infos = new List<PbInfo>();
                allInfos.Add(infos);
                var path = Path.Combine(rootDir, "Proto", pbDir, fileName);
                var reader = new StreamReader(new FileStream(path, FileMode.Open));
                string lineStr;
                int index = -1;
                while ((lineStr = reader.ReadLine()) != null)
                {
                    string noEmptyLine = Regex.Replace(lineStr, @"\s", "");

                    if (lineStr == "")
                    {
                        continue;
                    }

                    if (noEmptyLine.Contains("grp") && noEmptyLine.Contains("cmd"))
                    {
                        int grp = GetGrp(noEmptyLine);
                        int cmd = GetCmd(noEmptyLine);
                        var info = new PbInfo();
                        info.reqId = grp * 100000 + cmd * 10 + 0;
                        info.rspId = grp * 100000 + cmd * 10 + 1;
                        infos.Add(info);
                        index++;
                        continue;
                    }

                    if (index < 0)
                    {
                        continue;
                    }

                    if (noEmptyLine.Contains("messageReq"))
                    {
                        var req = noEmptyLine.Remove(0, 7).Replace("{", "");
                        infos[index].req = req;
                        continue;
                    }

                    if (noEmptyLine.Contains("messageRsp"))
                    {
                        var rsp = noEmptyLine.Remove(0, 7).Replace("{", "");
                        infos[index].rsp = rsp;
                    }
                    else if (noEmptyLine.Contains("messagePush"))
                    {
                        var rsp = noEmptyLine.Remove(0, 7).Replace("{", "");
                        infos[index].rsp = rsp;
                    }
                }
            }

            var sw = new StreamWriter(msgIdPath, false);
            sw.WriteLine("syntax = \"proto3\";");
            sw.WriteLine("package DH.Proto;");
            sw.WriteLine("");
            sw.WriteLine("enum ReqGameMsg");
            sw.WriteLine("{");
            sw.WriteLine("    NoneReq = 0;");
            foreach (var infos in allInfos)
            {
                foreach (var info in infos)
                {
                    var reqId = info.reqId;
                    var req = info.req;
                    if (string.IsNullOrEmpty(req))
                    {
                        continue;
                    }
                    sw.WriteLine("    " + req + " = " + reqId + ";");
                }
            }
            sw.WriteLine("}");

            sw.WriteLine("");
            sw.WriteLine("enum RspGameMsg");
            sw.WriteLine("{");
            sw.WriteLine("    NoneRsp = 0;");
            foreach (var infos in allInfos)
            {
                foreach (var info in infos)
                {
                    var rspId = info.rspId;
                    var rsp = info.rsp;
                    if (string.IsNullOrEmpty(rsp))
                    {
                        continue;
                    }
                    sw.WriteLine("    " + rsp + " = " + rspId + ";");
                }
            }
            sw.WriteLine("}");
            sw.Close();
            sw.Dispose();
        }

        private static bool Proto2CS()
        {
            string rootDir = Environment.CurrentDirectory;
            string protoDir = Path.Combine(rootDir, "Proto", $"{pbDir}/");
#if UNITY_EDITOR_WIN
            string protoc = Path.Combine(rootDir, "Proto", "protoc.exe");
#else
            string protoc = Path.Combine(rootDir, "Proto", "protoc");
#endif
            string csPath = Path.Combine(rootDir, "Assets", "Scripts", "Proto/");
            var fileNames = GetAllPbFileNames();
            foreach (var fName in fileNames)
            {
#if UNITY_EDITOR_WIN
                string argument = $"--csharp_out=\"{csPath}\" --proto_path=\"{protoDir}\" \"{protoDir}{fName}\"";
                var result = CommandUtil.RunNormalCommand(protoc, argument, Environment.CurrentDirectory,false);
#else
                string argument = $"--csharp_out=\"{csPath}\" --proto_path=\"{protoDir}\" \"{fName}\"";
                var result = CommandUtil.RunNormalCommand(protoc, argument, Environment.CurrentDirectory);
#endif
               if (result.exitCode != 0)
               {
                   Debug.LogError(result.message);
                   return false;
               }
            }

            return true;
        }

        private static List<string> GetAllPbFileNames()
        {
            string rootDir = Environment.CurrentDirectory;
            string protoDir = Path.Combine(rootDir, "Proto", $"{pbDir}/");
            var dirInfo = new DirectoryInfo(protoDir);
            var fileNames = new List<string>();
            foreach (var fileInfo in dirInfo.GetFiles("*.proto"))
            {
                fileNames.Add(fileInfo.Name);
            }
            return fileNames;
        }

        private static int GetGrp(string str)
        {
            var index = str.IndexOf("grp");
            string result = "";
            bool appendFlag = false;
            for (int i = index; i < str.Length; i++)
            {
                if (char.IsNumber(str[i]))
                {
                    result += str[i];
                    appendFlag = true;
                }
                else
                {
                    if (appendFlag)
                    {
                        break;
                    }
                }
            }
            return int.Parse(result);
        }

        private static int GetCmd(string str)
        {
            var index = str.IndexOf("cmd");
            string result = "";
            bool appendFlag = false;
            for (int i = index; i < str.Length; i++)
            {
                if (char.IsNumber(str[i]))
                {
                    result += str[i];
                    appendFlag = true;
                }
                else
                {
                    if (appendFlag)
                    {
                        break;
                    }
                }
            }
            return int.Parse(result);
        }

        private static void GenMsgTypeDict()
        {
            string rootDir = Environment.CurrentDirectory;
            string csPath = Path.Combine(rootDir, "Assets", "Scripts", "Proto", "MsgType.cs");
            if (File.Exists(csPath))
            {
                File.Delete(csPath);
            }

            using (var sw = new StreamWriter(csPath, false))
            {
                sw.WriteLine("using System;");
                sw.WriteLine("using System.Collections.Generic;");
                sw.WriteLine("using Google.Protobuf;");
                sw.WriteLine("");
                sw.WriteLine("namespace DH.Proto");
                sw.WriteLine("{");
                sw.WriteLine("\tpublic static class MsgType");
                sw.WriteLine("\t{");
                sw.WriteLine("\t\tpublic static readonly Dictionary<ReqGameMsg, Func<IMessage>> ReqGameMsgDict = new Dictionary<ReqGameMsg, Func<IMessage>>");
                sw.WriteLine("\t\t{");


                foreach (var infos in allInfos)
                {
                    foreach (var info in infos)
                    {
                        var reqId = info.reqId;
                        var req = info.req;
                        if (string.IsNullOrEmpty(req))
                        {
                            continue;
                        }
                        sw.WriteLine($"\t\t\t{{ReqGameMsg.{req}, ()=>new {req}()}},");
                    }
                }

                sw.WriteLine("\t\t};");
                sw.WriteLine("");

                sw.WriteLine("\t\tpublic static readonly Dictionary<RspGameMsg, Func<IMessage>> RspGameMsgDict = new Dictionary<RspGameMsg, Func<IMessage>>");
                sw.WriteLine("\t\t{");

                foreach (var infos in allInfos)
                {
                    foreach (var info in infos)
                    {
                        var rspId = info.rspId;
                        var rsp = info.rsp;
                        if (string.IsNullOrEmpty(rsp))
                        {
                            continue;
                        }
                        sw.WriteLine($"\t\t\t{{RspGameMsg.{rsp}, ()=>new {rsp}()}},");
                    }
                }
                sw.WriteLine("\t\t};");

                sw.WriteLine("\t\tpublic static readonly Dictionary<Type, RspGameMsg> RspType = new Dictionary<Type, RspGameMsg>");
                sw.WriteLine("\t\t{");

                foreach (var infos in allInfos)
                {
                    foreach (var info in infos)
                    {
                        var rspId = info.rspId;
                        var rsp = info.rsp;
                        if (string.IsNullOrEmpty(rsp))
                        {
                            continue;
                        }
                        sw.WriteLine($"\t\t\t{{typeof({rsp}), RspGameMsg.{rsp}}},");
                    }
                }

                sw.WriteLine("\t\t};");
                
                sw.WriteLine("\t\tpublic static readonly Dictionary<Type, ReqGameMsg> ReqType = new Dictionary<Type, ReqGameMsg>");
                sw.WriteLine("\t\t{");

                foreach (var infos in allInfos)
                {
                    foreach (var info in infos)
                    {
                        var req = info.req;
                        if (string.IsNullOrEmpty(req))
                        {
                            continue;
                        }
                        sw.WriteLine($"\t\t\t{{typeof({req}), ReqGameMsg.{req}}},");
                    }
                }

                sw.WriteLine("\t\t};");
                
                sw.WriteLine("\t}");
                
                sw.WriteLine("}");
            }
        }
    }
}