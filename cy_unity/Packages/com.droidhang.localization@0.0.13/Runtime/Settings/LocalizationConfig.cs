using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHFramework.Localization
{
    [CreateAssetMenu(fileName = "LocalizationConfig", menuName = "Localization/LocalizationConfig")]
    public class LocalizationConfig : ScriptableObject
    {
        [Header("使用的tags")] public List<string> Tags;

        [Header("字体根目录（资源的address path里语言code之前的部分）")]
        public string RootPath = "";
        [Header("字体目录名称")]
        public string FontsFolderName;
        [Header("材质球的目录名称")]
        public string MaterialFolderName;

        [Header("默认的语言")] public string DefaultLanguage = "en";
        [Header("语言对应的字体设置")] public List<FontLanguageSettings> FontLanguageList;
    }
}

