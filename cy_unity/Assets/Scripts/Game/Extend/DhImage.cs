
using Cysharp.Threading.Tasks;
using DHFramework.Localization;
using Game.UI.Control;
using UnityEngine;
using UnityEngine.UI;

namespace Extend
{
    public class DhImage:Image
    {
        [SerializeField] private LanguageSprite[] spriteLanguage;
        
        new public Sprite sprite
        {
            get => base.sprite;
            set => base.sprite = value;
        }

        public bool gray;
        public bool Grag
        {
            get => gray;
            set
            {
                gray = value;
                SetGrayActive(gray);
            }
        }

        protected override void Start()
        {
            base.Start();
            if (spriteLanguage != null && spriteLanguage.Length > 0)
            {
                sprite = GetTargetSprite();
            }
            Localization.RegisterLocalize(GetInstanceID(),OnLocalize);
        }
        private async UniTask OnLocalize()
        {
            if (spriteLanguage != null && spriteLanguage.Length > 0)
            {
                sprite = GetTargetSprite();
                SetNativeSize();
            }
            await UniTask.CompletedTask;
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Localization.UnRegisterLocalize(GetInstanceID());
        }

        public void SetGrayActive( bool gray, bool recursive = true, bool purple = false)
        {
            UIHelper.SetGray(gameObject, gray, recursive,purple);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
        }
#endif
    }
    
    
}