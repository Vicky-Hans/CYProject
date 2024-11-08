using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHFramework.Localization
{
    [Serializable]
    public class FontSettingsEntity
    {
        [Header("对应的tag")] public string Tag;
        [Header("字体的路径（相对于Fonts文件夹的）")] public string FontPath;
        [Header("对应的材质")] public List<string> FontMaterialPath;

        public FontSettingsEntity Clone()
        {
            return new FontSettingsEntity
            {
                Tag = this.Tag,
                FontPath = this.FontPath,
                FontMaterialPath = this.FontMaterialPath
            };
        }
    }
}