using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Localize;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

namespace DHFramework.Localization
{
    internal sealed class LocalizationComponent : IGameFrameworkComponent
    {
        //Static, const and readonly fields.
        private const string languageSaveKey = "Locale";
        private string defaultLanguage = "en";
        //Fields and properties.
        private Dictionary<string, int> languageTypeDic = new Dictionary<string, int>()
        {
            {"en", 0}, //英语  English
            {"cn", 1}, //简体中文  Chinese
            {"fr", 2}, //法语  French
            {"it", 3}, //意大利  Italian
            {"de", 4}, //德语  German
            {"es", 5}, //西班牙语  Spanish
            {"nl", 6}, //荷兰语  Dutch
            {"ru", 7}, //俄语  Russian
            {"ko", 8}, //韩语  Korean
            {"ja", 9}, //日语  Japanese
            {"hu", 10}, //匈牙利语  Hungarian
            {"pt", 11}, //葡萄牙语  Portuguese
            {"ar", 12}, //阿拉伯语  Arabic
            {"zh", 13}, //繁体中文  ChineseTW
            {"tr", 14}, //土耳其语  Turkish
            {"th", 15}, //泰语  Thai
            {"ms", 16}, //马来语  Malay
            {"vi", 17}, //越南语  Vietnamese
            {"id", 18}, //印尼语  Indonesian
            {"be", 19}, //白俄罗斯  Belarusian
            {"bg", 20}, //保加利亚  Bulgarian
            {"ca", 21}, //加泰罗尼亚语的  Catalan
            {"cs", 23}, //捷克  Czech
            {"da", 24}, //丹麦  Danish
            {"et", 25}, //爱沙尼亚  Estonian
            {"fo", 26}, //法罗人  Faroese
            {"fi", 27}, //芬兰  Finnish
            {"el", 28}, //希腊  Greek
            {"he", 29}, //希伯来语  Hebrew
            {"is", 30}, //冰岛  Icelandic
            {"eu", 31}, //巴斯克  Basque
            {"lv", 32}, //拉脱维亚  Latvian
            {"lt", 33}, //立陶宛  Lithuanian
            {"nb", 34}, //挪威(Bokm?l)  Norwegian(Bokm?l)
            {"pl", 35}, //波兰的  Polish
            {"ro", 36}, //罗马尼亚  Romanian
            {"sh", 37}, //塞尔维亚-克罗地亚语  SerboCroatian
            {"sk", 38}, //斯洛伐克语  Slovak
            {"sl", 39}, //斯洛维尼亚语  Slovenian
            {"sv", 40}, //瑞典  Swedish
            {"uk", 41}, //乌克兰  Ukrainian
            {"af", 42}, //南非荷兰语  Afrikaans
            {"sa", 43}, //梵文  Sanskrit
            {"se", 44}, //萨米人(北部)  Sami(Northern)
            {"sq", 45}, //阿尔巴尼亚  Albanian
            {"sw", 46}, //斯瓦希里语  Swahili
            {"syr", 47}, //叙利亚的  Syriac
            {"ta", 48}, //泰米尔  Tamil
            {"te", 49}, //泰卢固语  Telugu
            {"tl", 50}, //塔加拉族语  Tagalog
            {"tn", 51}, //茨瓦纳语  Tswana
            {"ts", 52}, //特松加  Tsonga
            {"tt", 53}, //鞑靼人  Tatar
            {"ur", 54}, //乌尔都语  Urdu
            {"uz", 55}, //乌兹别克斯坦(拉丁)  Uzbek(Latin)
            {"qu", 56}, //盖丘亚语  Quechua
            {"gu", 57}, //古吉拉特语  Gujarati
            {"ps", 58}, //普什图语  Pashto
            {"pa", 59}, //旁遮普语  Punjabi
            {"hi", 60}, //北印度语  Hindi
            {"xh", 61}, //科萨人  Xhosa
            {"hy", 62}, //亚美尼亚  Armenian
            {"fa", 63}, //波斯语  Farsi
            {"ka", 64}, //乔治亚语  Georgian
            {"kk", 65}, //哈萨克斯坦  Kazakh
            {"kn", 66}, //埃纳德语  Kannada
            {"kok", 67}, //贡根语  Konkani
            {"az", 68}, //阿塞拜疆(拉丁)  Azeri(Latin)
            {"ky", 69}, //吉尔吉斯语  Kyrgyz
            {"dv", 70}, //迪维西语  Divehi
            {"mi", 71}, //毛利  Maori
            {"mk", 72}, //FYRO马其顿  FYRO Macedonian
            {"mn", 73}, //蒙古  Mongolian
            {"mr", 74}, //马拉地语  Marathi
            {"mt", 75}, //马耳他  Maltese
            {"cy", 76}, //威尔士  Welsh
            {"ns", 77}, //北梭托语  Northern Sotho
            {"eo", 78}, //世界语  Esperanto
            {"hr", 79}, //克罗地亚  Croatian
            {"gl", 80}, //加利西亚语的  Galician
            {"ph", 81}, //菲律宾
        };

        private Dictionary<string, TMP_FontAsset> cacheFontDic = new Dictionary<string, TMP_FontAsset>();
        //<tag, <matName, material>>
        private Dictionary<string, Dictionary<string, Material>> cacheMaterialDic = new Dictionary<string, Dictionary<string, Material>>();
        private Dictionary<string, string> cacheFontNameDic = new Dictionary<string, string>();
        private string currentLanguageCode;
        private string startupLanguageCode;
        //<languageCode, <tag, settingsEntity>>
        private Dictionary<string, Dictionary<string, FontSettingsEntity>> localeFontConfigData = new Dictionary<string, Dictionary<string, FontSettingsEntity>>();
        private LocalizationConfig configData;
        private Dictionary<int, Func<UniTask>> languageChangedEventDic = new Dictionary<int, Func<UniTask>>(); //注册的字体变化的回调事件
        private bool init = false;
        private Func<string> getSystemLanguage;

        /// <summary>
        /// 初始化LanguageCode，仅用于初始化组件获取语言
        /// </summary>
        /// <param name="configPath"></param>
        /// <param name="systemLanguageGetter"></param>
        public async UniTask InitWithoutMaterial(string configPath, Func<string> systemLanguageGetter)
        {
            getSystemLanguage = systemLanguageGetter;
            configData = await AssetsManager.LoadAssetAsync<LocalizationConfig>(configPath);
            
            if (configData == null)
            {
                DHLog.Error("请先配置 LocalizationConfig");
                return;
            }
            
            defaultLanguage = configData.DefaultLanguage;
            HashSet<string> languages = new HashSet<string>();
            foreach (var fontLanguage in configData.FontLanguageList)
            {
                languages.Add(fontLanguage.LanguageCode);
            }

            var languageCode = GetSavedLanguage();

            if (string.IsNullOrEmpty(languageCode))
            {
                languageCode = GetSystemLanguage();
            }

            if (!languages.Contains(languageCode))
            {
                languageCode = defaultLanguage;
            }

            startupLanguageCode = languageCode;
        }

        public async UniTask Init(string configPath,Func<string> systemLanguageGetter)
        {
            if (init || !Application.isPlaying)
            {
                return;
            }

            getSystemLanguage = systemLanguageGetter;
            configData = await AssetsManager.LoadAssetAsync<LocalizationConfig>(configPath);
            
            if (configData == null)
            {
                DHLog.Error("请先配置 LocalizationConfig");
                return;
            }

            foreach (var tag in configData.Tags)
            {
                TMP_Localization.InitTag(tag);
            }

            defaultLanguage = configData.DefaultLanguage;

            FontLanguageSettings defaultSetting = null;
            foreach (var fontLanguage in configData.FontLanguageList)
            {
                if (fontLanguage.LanguageCode == defaultLanguage)
                {
                    defaultSetting = fontLanguage;
                }

                if (localeFontConfigData.ContainsKey(fontLanguage.LanguageCode))
                {
                    DHLog.Error("配置 LocalizationConfig 的语言重复");
                    return;
                }

                var fontDic = new Dictionary<string, FontSettingsEntity>();
                localeFontConfigData.Add(fontLanguage.LanguageCode, fontDic);
                string languageCode = fontLanguage.LanguageCode;
                List<FontSettingsEntity> fontList = fontLanguage.FontList;
                if (fontList.Count == 0 && defaultSetting != null)
                {
                    fontList = defaultSetting.FontList;
                    languageCode = defaultLanguage;
                    DHLog.Debug($"{fontLanguage.LanguageCode}未找到多语言字体配置信息,将使用默认语言配置：{languageCode}");
                }

                foreach (var fontSettings in fontList)
                {
                    var tmpSettings = fontSettings.Clone();
                    if (fontDic.ContainsKey(tmpSettings.Tag))
                    {
                        DHLog.Error("配置 LocalizationConfig 的tag重复");
                        return;
                    }

                    fontDic.Add(tmpSettings.Tag, tmpSettings);

                    var relativePath = tmpSettings.FontPath;
                    tmpSettings.FontPath = DHUtility.Path.GetRegularPath(Path.Combine(configData.RootPath, languageCode,
                        configData.FontsFolderName, relativePath));
                }
            }

            init = true;
            TMP_Localization.SetGetMaterialFunc(GetMaterialFunc);

            await PrepareCurrentLanguage();
        }

        /// <summary>
        /// 释放操作
        /// </summary>
        public void Release()
        {
            if (!init)
            {
                return;
            }

            //释放字体
            foreach (var fontEntry in cacheFontDic)
            {
                AssetsManager.Release(fontEntry.Value);
            }
            
            cacheFontDic.Clear();

            //释放材质球
            foreach (var materialDicEntry in cacheMaterialDic)
            {
                if (materialDicEntry.Value != null)
                {
                    foreach (var materialEntry in materialDicEntry.Value)
                    {
                        AssetsManager.Release(materialEntry.Value);
                    }
                }
            }
            
            cacheMaterialDic.Clear();
        }

        /// <summary>
        /// 切换语言, 注意此方法为异步执行
        /// </summary>
        /// <param name="languageCode">en, cn等</param>
        public void ChangeLanguage(string code, Action ChangeComplete = null)
        {
            ChangeLanguageWrap(code, ChangeComplete).Forget();
        }

        private async UniTaskVoid ChangeLanguageWrap(string code, Action ChangeComplete)
        {   
            await ChangeLanguageAsync(code); 
            
            ChangeComplete?.Invoke();
        }

        public async UniTask ChangeLanguageAsync(string code)
        {
            if (!init)
            {
                return;
            }

            DHLog.Debug($"[localization]切换语言：{code}");
            if (!languageTypeDic.ContainsKey(code))
            {
                DHLog.Error($"[localization]不支持的多语言Code:{code}");
                return;
            }

            if (!localeFontConfigData.TryGetValue(code, out var fontSettingsDic))
            {
                DHLog.Error($"[localization]所选的语言没有配置 语言Code:{code}");
                return;
            }

            if (currentLanguageCode == code)
            {
                await NotifyOnLocalize();
                return;
            }

            TMP_Localization.ReleaseCache();
            currentLanguageCode = code;
            
            var tmpCacheFontDic = cacheFontDic;
            var tmpCacheMaterialDic = cacheMaterialDic;

            cacheFontDic = new Dictionary<string, TMP_FontAsset>();
            cacheMaterialDic = new Dictionary<string, Dictionary<string, Material>>();
            cacheFontNameDic.Clear();

            foreach (var configDataTag in configData.Tags)
            {
                if (fontSettingsDic.TryGetValue(configDataTag, out var fontSettings))
                {
                    var font = await AssetsManager.LoadAssetAsync<TMP_FontAsset>(fontSettings.FontPath);

                    if (!font)
                    {
                        DHLog.Error($"[localization]没有加载到对应的字体,fontPath:{fontSettings.FontPath}");
                        return;
                    }

                    cacheFontDic.Add(configDataTag, font);
                    cacheFontNameDic.Add(configDataTag, font.name);
                    await PreloadFontAssetMaterials(font.name, fontSettings);
                    TMP_Localization.OnLocalize(configDataTag, font);
                }
                else
                {
                    DHLog.Error($"[localization]没有找到对应的字体设置,语言Code:{code},Tag:{configDataTag}");
                    return;
                }
            }

            SaveCurrentLanguage(code);
            await NotifyOnLocalize();

            // 释放材质球
            foreach (var materialDicEntry in tmpCacheMaterialDic)
            {
                if (materialDicEntry.Value != null)
                {
                    foreach (var materialEntry in materialDicEntry.Value)
                    {
                        AssetsManager.Release(materialEntry.Value);
                    }
                }
            }

            // 释放字体
            foreach (var fontEntry in tmpCacheFontDic)
            {
                AssetsManager.Release(fontEntry.Value);
            }
        }

        /// <summary>
        /// 设置一个tmp字体的材质球
        /// </summary>
        /// <param name="textComp"></param>
        /// <param name="matName"></param>
        public async UniTask<bool> SetTextMaterial(TextMeshProUGUI textComp, string matName)
        {
            var useDefault = false;
            var tag = textComp.gameObject.tag;
    
            if (!IsLocalizationTag(tag))
            {
                DHLog.Error("[localization]SetTextMaterial invalid tag ：{tag}");
                return false;
            }
            
            if (!string.IsNullOrEmpty(matName))
            {
                var material = GetMaterialFunc(tag, matName);
                if (material)
                {
                    textComp.fontSharedMaterial = material;
                }
                else
                {
                    useDefault = true;
                }
            }
            else
            {
                useDefault = true;
            }
            
            if (useDefault && cacheFontDic.TryGetValue(tag, out var font) && font)
            {
                textComp.fontSharedMaterial = font.material;
            }
            
            return true;
        }

        /// <summary>
        /// 获取当前的语言
        /// </summary>
        /// <returns>cn，en等</returns>
        public string GetCurrentLanguage()
        {
            // 增加InitWithoutMaterial，不再需要检查Init标志位，仅限于获取Language
            if (!init)
            {
                return startupLanguageCode;
            }
            
            return currentLanguageCode;
        }

        //// <summary>
        /// 获取当前的语言Code
        /// </summary>
        /// <returns>cn，en等对应的数字代号</returns>
        public int GetCurrentLanguageNumber()
        {
            var language = GetCurrentLanguage();

            languageTypeDic.TryGetValue(language, out var number);
            return number;
        }

        /// <summary>
        /// 注册语言变化时的回调
        /// </summary>
        /// <param name="instanceId">object 的GetInstanceId()</param>
        /// <param name="action">回调函数</param>
        public void RegisterLocalize(int instanceId, Func<UniTask> action)
        {
            if (languageChangedEventDic.ContainsKey(instanceId))
            {
                return;
            }
            
            languageChangedEventDic.Add(instanceId, action);
        }

        /// <summary>
        /// 反注册语言变化时的回调
        /// </summary>
        /// <param name="instanceId"></param>
        public void UnRegisterLocalize(int instanceId)
        {
            languageChangedEventDic.Remove(instanceId);
        }

        /// <summary>
        /// 返回上次保存的语言，cn，en等
        /// </summary>
        /// <returns></returns>
        private string GetSavedLanguage()
        {
            var code = DHUnityUtil.PlayerPrefs.GetString(languageSaveKey, string.Empty);

            if (!languageTypeDic.ContainsKey(code))
            {
                code = "";
            }

            return code;
        }
        
        /// <summary>
        /// 保存当前的语言code
        /// </summary>
        /// <param name="language">cn，en等</param>
        private void SaveCurrentLanguage(string language)
        {
            DHUnityUtil.PlayerPrefs.SetString(languageSaveKey, language);
            DHUnityUtil.PlayerPrefs.Save();
        }

        /// <summary>
        /// 获取当前已选择的语言，若还未选择语言则获取系统语言
        /// </summary>
        private async UniTask PrepareCurrentLanguage()
        {
            var languageCode = GetSavedLanguage();

            if (string.IsNullOrEmpty(languageCode))
            {
                languageCode = GetSystemLanguage();
            }

            if (!localeFontConfigData.ContainsKey(languageCode))
            {
                languageCode = defaultLanguage;
            }

            await ChangeLanguageAsync(languageCode);
        }

        /// <summary>
        /// 获取系统语言
        /// </summary>
        /// <returns></returns>
        private string GetSystemLanguage()
        {
            var countryCode = getSystemLanguage?.Invoke();
            var languageCode = defaultLanguage;
            var splitCode = countryCode.Split('_');
            
            //检查返回的语言格式是否正确，若不正确将使用默认的语言
            if (splitCode.Length != 2)
            {
                return languageCode;
            }

            //针对目前的多语言规则，特殊处理TW和其他简体中文的情况
            if (splitCode[1] == "CN")
            {
                languageCode = "cn";
            }
            else
            {
                languageCode = splitCode[0];
            }

            return languageCode;
        }

        /// <summary>
        /// 获取材质球
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="materialName"></param>
        /// <returns></returns>
        private Material GetMaterialFunc(string tag, string materialName)
        {
            Material material = null;
            
            if (cacheMaterialDic.TryGetValue(tag, out var materialDic))
            {
                if (materialDic.TryGetValue(materialName, out material))
                {
                    return material;
                }
            }

            if (cacheFontNameDic.TryGetValue(tag, out var fontName))
            {
                var materialPath = DHUtility.Path.GetRegularPath(Path.Combine(configData.RootPath, currentLanguageCode, configData.MaterialFolderName, fontName, materialName));
                
                if (AssetsManager.CheckAssetsValid(materialPath))
                {
                    material = AssetsManager.LoadAssetSync<Material>(materialPath);

                    if (!cacheMaterialDic.TryGetValue(tag, out materialDic))
                    {
                        materialDic = new Dictionary<string, Material>();
                        cacheMaterialDic.Add(tag, materialDic);
                    }
                    
                    materialDic.Add(materialName, material);
                }
            }

            return material;
        }

        /// <summary>
        /// 预加载fontAsset的所有材质，其中材质和tag一一对应
        /// </summary>
        /// <param name="fontName">FontAsset的名字</param>
        /// <param name="allMaterials">所有材质的数组</param>
        /// <param name="allTags">对应材质的tag数组</param>
        private async UniTask PreloadFontAssetMaterials(string fontName, FontSettingsEntity fontSettingsEntity)
        {
            if (fontSettingsEntity == null || fontSettingsEntity.FontMaterialPath == null)
            {
                return;
            }

            var tag = fontSettingsEntity.Tag;

            for (int i = 0; i < fontSettingsEntity.FontMaterialPath.Count; i++)
            {
                var materialName = fontSettingsEntity.FontMaterialPath[i];
                var materialPath = DHUtility.Path.GetRegularPath(Path.Combine(currentLanguageCode, configData.MaterialFolderName, fontName, materialName));
                if (AssetsManager.CheckAssetsValid(materialPath))
                {
                    var material = await AssetsManager.LoadAssetAsync<Material>(materialPath);

                    if (!cacheMaterialDic.TryGetValue(tag, out var materialDic))
                    {
                        materialDic = new Dictionary<string, Material>();
                        cacheMaterialDic.Add(tag, materialDic);
                    }

                    materialDic.Add(materialName, material);
                }
            }
        }

        /// <summary>
        /// 判断是否是多语言支持的tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        private bool IsLocalizationTag(string tag)
        {
            return configData != null && configData.Tags.Contains(tag);
        }

        private async UniTask NotifyOnLocalize()
        {
            foreach (var action in languageChangedEventDic)
            {
                if (action.Value == null)
                {
                    continue;
                }
                
                await action.Value.Invoke();
            }
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public void Shutdown()
        {
            Release();
        }
    }
}

