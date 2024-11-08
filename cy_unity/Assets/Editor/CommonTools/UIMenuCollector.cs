using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DH.UIFramework;
using DHFramework;
using Game.UI.CommonView;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace DH.Editor
{
    public static class UIMenuCollector
    {
        private static readonly string RootPath = "Assets/GameAssets/Prefabs/UI";
        private static readonly string ScriptFilePath = "Assets/Scripts/Game/UI/UIConfig.Code.cs";
        private static AddressableAssetSettings settings;

        public static AddressableAssetSettings Settings
        {
            get
            {
                if (!settings)
                {
                    settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
                }

                return settings;
            }
        }

        [MenuItem("DH Tools/刷新UI配置")]
        public static void Refresh()
        {
            var prefabs = Directory.GetFiles(RootPath, "*.prefab", SearchOption.AllDirectories);
            Dictionary<string, Tuple<Type, string, bool>> container =
                new Dictionary<string, Tuple<Type, string, bool>>();
            var names = Enum.GetNames(typeof(UILayersConfig));

            foreach (var path in prefabs)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var baseView = prefab.GetComponent<BaseView>();
                if (!baseView || baseView is BaseItemView)
                {
                    continue;
                }

                var guid = AssetDatabase.AssetPathToGUID(path);
                var type = baseView.GetType();
                var address = Settings.FindAssetEntry(guid)?.address;
                if (string.IsNullOrEmpty(address))
                {
                    continue;
                }

                // bool fullScreen = !(path.Contains("MsgBox", StringComparison.Ordinal)
                //                     || path.Contains("MessageBox", StringComparison.Ordinal));
                bool fullScreen = baseView.FullScreen;

                if (container.TryGetValue(type.Name, out var info ))
                {
                    DHLog.Error( $"有 多预制体 挂有相同的 脚本 {type.Name} 1 {info.Item2}  2 {address}");
                }

                container.Add(type.Name, new Tuple<Type, string, bool>(type, address, fullScreen));
            }

            GenerateCode(container);
        }

        private class CodeScope : IDisposable
        {
            private static int intentCount;
            private StringBuilder stringBuilder;
            private string end;

            public CodeScope(StringBuilder stringBuilder,string endStr = "")
            {
                this.stringBuilder = stringBuilder;
                this.end = endStr;
                intentCount++;
                Output("{");
            }

            public void Output(string str,int offset = 0)
            {
                for (int index = 1; index < intentCount + offset; index++)
                {
                    stringBuilder.Append("\t");
                }

                stringBuilder.Append($"{str}\r\n");
            }

            public void Dispose()
            {
                Output($"}}{end}");
                intentCount--;
            }
        }

        private static void GenerateCode( Dictionary<string, Tuple<Type, string, bool>> container)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("// Auto generated");
            stringBuilder.AppendLine("using System.Collections.Generic;");
            stringBuilder.AppendLine("public static partial class UIConfig");
            using (var scope = new CodeScope(stringBuilder))
            {
                scope.Output("private static Dictionary<System.Type, UIConfigItem> configItems = new Dictionary<System.Type, UIConfigItem>",1);
                using (var currentScope = new CodeScope(stringBuilder,";"))
                {
                    foreach (var item in container)
                    {
                        var fullScreen = item.Value.Item3.ToString().ToLowerInvariant();
                        Type LoadedType = typeof(BaseLoadingView);
                        var layer = "UILayersConfig.UI";
                        if(item.Value.Item1.IsSubclassOf(LoadedType))
                        {
                            layer = "UILayersConfig.Toast";
                        }
                        currentScope.Output($" {{typeof({item.Value.Item1.FullName}), new UIConfigItem(\"{item.Value.Item2}\", {layer}, {fullScreen},false)}},",1);
                    }
                }
            }
            
            File.WriteAllText(ScriptFilePath,stringBuilder.ToString());
            DHLog.Debug("刷新UI配置完成");
        }
    }
}