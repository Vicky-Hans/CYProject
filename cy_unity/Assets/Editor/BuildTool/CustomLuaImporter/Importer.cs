using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomLuaImporter
{
    public static class Importer
    {
        public delegate void CompileLuaScript(string srcDir, string destDir, ref Dictionary<string, bool> srcFileMap,
            List<FileInfo> fileList, string luacPath);
        
        public static void CompileCustomLuaBytes(CompileLuaScript compileFunc,ref Dictionary<string,bool> srcFileMap,string destDir,string luacPath)
        {
            var type = typeof(ICustomLuaImporter);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p));
            
            foreach (var item in types)
            {
                if (item == type)
                {
                    continue;
                }
                
                var instance = Activator.CreateInstance(item) as ICustomLuaImporter;
                var dir = instance?.GetImportLuaDir();
                var fileInfos = GetAllFileWithExtension(dir,"*.lua");
                compileFunc.Invoke(dir,destDir,ref srcFileMap,fileInfos,luacPath);
            }
        }

        public static List<FileInfo> GetAllFileWithExtension(string basePath, string extension)
        {
            Dictionary<string, bool> srcFileMap = new Dictionary<string, bool>();

            //获取目录信息
            DirectoryInfo srcDirInfo = new DirectoryInfo(basePath);

            //获取所有的Lua文件信息
            FileInfo[] srcFiles = srcDirInfo.GetFiles(extension, SearchOption.AllDirectories);

            //待编译lua文件列表
            List<FileInfo> fileList = new List<FileInfo>(srcFiles);

            return fileList;
        }
    }
}