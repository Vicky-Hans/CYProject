using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DH.Localize
{
    public class TMPLocalizationItem
    {
        public string Tag { get; set; }
        public TMP_FontAsset CurrentFont => currentFont;
        
        private Dictionary<int, Action> languageChangedEventDic = new Dictionary<int, Action>(); //注册的字体变化的回调事件
        private Dictionary<string, Material> usedMaterial = new Dictionary<string, Material>(); //字体名字使用的材质球，材质球由lua层加载
        private TMP_FontAsset currentFont;
        private Func<string, string, Material> getMaterialFunc; //lua注册的获取材质球的接口

        #region 语言改变的事件回调

        public void InvokeLanguageChangedEvent()
        {
            foreach (var action in languageChangedEventDic)
            {
                action.Value?.Invoke();
            }
        }

        public void AddLanguageChangedEvent(int instanceId, Action action)
        {
            if (languageChangedEventDic.ContainsKey(instanceId))
            {
                return;
            }
            
            languageChangedEventDic.Add(instanceId, action);
        }

        public void RemoveLanguageChangedEvent(int instanceId)
        {
            languageChangedEventDic.Remove(instanceId);
        }

        #endregion

        public void SetGetMaterialFunc(Func<string, string, Material> getFunc)
        {
            getMaterialFunc = getFunc;
        }

        public void OnLocalize(TMP_FontAsset fontAsset)
        {
            ReleaseCache(); //按理lua层应该先调用过
            currentFont = fontAsset;
            InvokeLanguageChangedEvent();
        }
        
        public Material GetMaterial(Material original)
        {
            if (!currentFont)
            {
                return null;
            }

            Material material = null;

            if (original)
            {
                string matName = original.name;

                if (!usedMaterial.TryGetValue(matName, out material))
                {
                    //没找到就像lua层获取
                    if (getMaterialFunc != null)
                    {
                        material = getMaterialFunc(Tag, matName);

                        if (material)
                        {
                            usedMaterial.Add(matName, material);
                        }
                    }
                }
            }
            
            return material ? material : currentFont.material;
        }

        /// <summary>
        /// 释放lua环境时需要调用
        /// </summary>
        public void Release()
        {
            ReleaseCache();
            getMaterialFunc = null;
            languageChangedEventDic.Clear();
        }

        /// <summary>
        /// 清除C#层对lua层加载的对象的引用
        /// </summary>
        public void ReleaseCache()
        {
            currentFont = null;
            usedMaterial.Clear();
        }
    }
    
    public static class TMP_Localization
    {
        private static Dictionary<string, TMPLocalizationItem> localizationDic = new Dictionary<string, TMPLocalizationItem>();
        
        #region 语言改变的事件回调

        public static void AddLanguageChangedEvent(int instanceId, string tag, Action action)
        {
            if (localizationDic.TryGetValue(tag, out var item))
            {
                item.AddLanguageChangedEvent(instanceId, action);
            }
        }

        public static void RemoveLanguageChangedEvent(int instanceId, string tag)
        {
            if (localizationDic.TryGetValue(tag, out var item))
            {
                item.RemoveLanguageChangedEvent(instanceId);
            }
        }

        #endregion

        #region 初始化函数

        /// <summary>
        /// 根据配置表里支持的tag初始化
        /// </summary>
        /// <param name="tag"></param>
        public static void InitTag(string tag)
        {
            if (!localizationDic.TryGetValue(tag, out var item))
            {
                localizationDic.Add(tag, new TMPLocalizationItem
                {
                    Tag = tag
                });
            }
        }
        
        /// <summary>
        /// 设置在lua层获取材质球的方法
        /// </summary>
        /// <param name="getFunc"></param>
        public static void SetGetMaterialFunc(Func<string, string, Material> getFunc)
        {
            foreach (var localizationItem in localizationDic)
            {
                localizationItem.Value.SetGetMaterialFunc(getFunc);
            }
        }

        #endregion"
        
        /// <summary>
        /// 缓存当前tag使用的字体
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="fontAsset"></param>
        public static void OnLocalize(string tag, TMP_FontAsset fontAsset)
        {
            if (localizationDic.TryGetValue(tag, out var item))
            {
                item.OnLocalize(fontAsset);
            };
        }

        /// <summary>
        /// 获取此tag在当前语言下使用的字体
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static TMP_FontAsset GetFont(string tag)
        {
            if (localizationDic.TryGetValue(tag, out var item))
            {
                return item.CurrentFont;
            }
            
            return null;
        }
        
        /// <summary>
        /// 获取当前使用的对应语言的材质球，没有使用自定义的材质球就返回字体的材质球
        /// </summary>
        /// <param name="original"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static Material GetMaterial(Material original, string tag)
        {
            if (!Application.isPlaying)
            {
                return null;
            }
            
            if (localizationDic.TryGetValue(tag, out var item))
            {
                return item.GetMaterial(original);
            }
            
            return null;
        }
        
        /// <summary>
        /// 清除C#层对lua层加载的对象的引用
        /// </summary>
        public static void ReleaseCache()
        {
            foreach (var localizationItem in localizationDic)
            {
                localizationItem.Value.ReleaseCache();
            }
        }
        
        /// <summary>
        /// 释放lua环境时需要调用
        /// </summary>
        public static void Release()
        {
            foreach (var localizationItem in localizationDic)
            {
                localizationItem.Value.Release();
            }
            
            localizationDic.Clear();
        }
    }
}