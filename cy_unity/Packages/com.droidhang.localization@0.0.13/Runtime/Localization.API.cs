using System;
using Cysharp.Threading.Tasks;
using DHFramework;
using TMPro;

namespace DHFramework.Localization
{
    public static class Localization
    {
        /// <summary>
        /// 初始化函数
        /// </summary>
        public static async UniTask InitSimple(string configPath,Func<string> getSystemLanguage)
        {
            var framework = GameFramework.Instance;
            var localizationComponent = framework.GetModule<LocalizationComponent>();
            
            await localizationComponent.InitWithoutMaterial(configPath, getSystemLanguage);
        }
        
        /// <summary>
        /// 初始化函数
        /// </summary>
        public static async UniTask Init(string configPath,Func<string> getSystemLanguage)
        {
            var framework = GameFramework.Instance;
            var localizationComponent = framework.GetModule<LocalizationComponent>();
            
            await localizationComponent.Init(configPath, getSystemLanguage);
        }
        

        /// <summary>
        /// 释放操作
        /// </summary>
        public static void Release()
        {
            var framework = GameFramework.Instance;
            var localizationComponent = framework.GetModule<LocalizationComponent>();
            localizationComponent.Release();
        }
        
        /// <summary>
        /// 切换语言,需要在Complete回调后处理
        /// </summary>
        /// <param name="languageCode">en, cn等</param>
        public static void ChangeLanguage(string languageCode, Action complete)
        {
            var framework = GameFramework.Instance;
            var localizationComponent = framework.GetModule<LocalizationComponent>();
            localizationComponent.ChangeLanguage(languageCode, complete);
        }
        
        /// <summary>
        /// 切换语言
        /// </summary>
        /// <param name="languageCode">en, cn等</param>
        public static async UniTask ChangeLanguageAsync(string languageCode)
        {
            var framework = GameFramework.Instance;
            var localizationComponent = framework.GetModule<LocalizationComponent>();
            await localizationComponent.ChangeLanguageAsync(languageCode);
        }

        /// <summary>
        /// 设置一个tmp字体的材质球
        /// </summary>
        /// <param name="textComp"></param>
        /// <param name="matName"></param>
        public static void SetTextMaterial(TextMeshProUGUI textComp, string matName)
        {
            SetTextMaterialAsync(textComp, matName).Forget();
        }
        
        private static async UniTaskVoid SetTextMaterialAsync(TextMeshProUGUI textComp, string matName)
        {
            var framework = GameFramework.Instance;
            var localizationComponent = framework.GetModule<LocalizationComponent>();
            localizationComponent.SetTextMaterial(textComp, matName);
        }

        /// <summary>
        /// 获取当前的语言
        /// </summary>
        /// <returns>cn，en等</returns>
        public static string GetCurrentLanguage()
        {
            var framework = GameFramework.Instance;
            var localizationComponent = framework.GetModule<LocalizationComponent>();
            return localizationComponent.GetCurrentLanguage();
        }

        /// <summary>
        /// 获取当前的语言Code
        /// </summary>
        /// <returns>cn，en等对应的数字代号</returns>
        public static int GetCurrentLanguageNumber()
        {
            var framework = GameFramework.Instance;
            var localizationComponent = framework.GetModule<LocalizationComponent>();
            return localizationComponent.GetCurrentLanguageNumber();
        }

        /// <summary>
        /// 注册语言变化时的回调
        /// </summary>
        /// <param name="instanceId">object 的GetInstanceId()</param>
        /// <param name="action">回调函数</param>
        public static void RegisterLocalize(int instanceId, Func<UniTask> action)
        {
            var framework = GameFramework.Instance;
            if (framework == null)
            {
                return;
            }
            var localizationComponent = framework.GetModule<LocalizationComponent>();
            localizationComponent.RegisterLocalize(instanceId, action);
        }

        /// <summary>
        /// 反注册语言变化时的回调
        /// </summary>
        /// <param name="instanceId"></param>
        public static void UnRegisterLocalize(int instanceId)
        {
            var framework = GameFramework.Instance;
            if (framework == null)
            {
                return;
            }
            
            var localizationComponent = framework.GetModule<LocalizationComponent>();
            localizationComponent?.UnRegisterLocalize(instanceId);
        }
    }
}