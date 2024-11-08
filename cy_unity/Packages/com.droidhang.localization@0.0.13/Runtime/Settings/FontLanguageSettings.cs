using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHFramework.Localization
{
    [Serializable]
    public class FontLanguageSettings
    {
        [Header("语言code(en,cn...)")]
        public string LanguageCode;
        [Header("对应的字体列表")]
        public List<FontSettingsEntity> FontList;
    }
}
