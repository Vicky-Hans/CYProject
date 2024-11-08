using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Config;
using DHFramework;

namespace DH.Game
{
    public static class LocalizeHelper
    {
        private const string UnknownString = "UnknownString";
        
        private static readonly Dictionary<int, Func<UniTask>> LanguageChangedEventDic = new Dictionary<int, Func<UniTask>>(); //注册的字体变化的回调事件

        /// <summary>
        /// 注册语言变化时的回调
        /// </summary>
        /// <param name="instanceId">object 的GetInstanceId()</param>
        /// <param name="action">回调函数</param>
        public static void RegisterLocalize(int instanceId, Func<UniTask> action)
        {
            if (LanguageChangedEventDic.ContainsKey(instanceId))
            {
                return;
            }
            
            LanguageChangedEventDic.Add(instanceId, action);
        }

        /// <summary>
        /// 反注册语言变化时的回调
        /// </summary>
        /// <param name="instanceId"></param>
        public static void UnRegisterLocalize(int instanceId)
        {
            LanguageChangedEventDic.Remove(instanceId);
        }
        
        public static async UniTask NotifyOnLocalize()
        {
            foreach (var action in LanguageChangedEventDic)
            {
                if (action.Value == null)
                {
                    continue;
                }
                
                await action.Value.Invoke();
            }
        }

        public static void Clear()
        {
            LanguageChangedEventDic.Clear();
        }

        public static string GetGlobal(string key, params object[] args)
        {
            var data = ConfigCenter.GlobalLanguageCfgColl.GetDataById(key);
            if (data == null)
            {
                DHLog.Error($"没有这个配置 请检查配置跟对应的key 是否存在 key == {key}");
                return key;
            }
            
            var str = data.Name;
            if (str == null)
            {
                DHLog.Error($"没有配置 请检查配置 GlobalLanguageCfg  跟对应的key 是否存在 key == {key} 的多语言是有否配");
                return "";
            }

            if (args != null && args.Length > 0)
            {
                str = string.Format(str, args);
            }

            return str;
        }
        public static string GetFunctionOpenGlobal(int key)
        {
            var data = ConfigCenter.FunctionOpenLanguageCfgColl.GetDataById(key);
            if (data == null)
            {
                DHLog.Error($"没有这个配置 请检查配置跟对应的key 是否存在 key == {key}");
                return "";
            }
            return  data.Name;
        }
    }
}