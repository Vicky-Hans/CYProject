using System;
using Cysharp.Threading.Tasks;
using DHFramework.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Control
{
    public enum LanguageType
    {
        en,
        cn,
        zh,
        de,
        fr,
        es,
        pt
    }
    
    [Serializable]
    public class LanguageSprite
    {
        public LanguageType languageCode;
        public Sprite targetSprite;
    }
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public class LocalizationSprite : MonoBehaviour
    {
      
        public LanguageSprite[] spriteLanguage;
        
        private Image imageComponent;

        private void Start()
        {
            imageComponent = GetComponent<Image>();
            
            imageComponent.sprite = GetTargetSprite();
            imageComponent.SetNativeSize();
            Localization.RegisterLocalize(GetInstanceID(),OnLocalize);
        }
        
        private async UniTask OnLocalize()
        {
            imageComponent.sprite = GetTargetSprite();
            imageComponent.SetNativeSize();
            await UniTask.CompletedTask;
        }

        private void OnDestroy()
        {
            Localization.UnRegisterLocalize(GetInstanceID());
        }

        private Sprite GetTargetSprite()
        {
            string languageCode = Localization.GetCurrentLanguage();

            LanguageSprite enSprite = null;
            foreach (var item in spriteLanguage)
            {
                if (item.languageCode == LanguageType.en)
                {
                    enSprite = item;
                }
                if (item.languageCode.ToString() == languageCode)
                {
                    return item.targetSprite;
                }
            }
            if(enSprite != null)
            {
                return enSprite.targetSprite;
            }
            return spriteLanguage[0].targetSprite;
        }
    }
}