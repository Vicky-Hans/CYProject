using System;
using Cysharp.Threading.Tasks;
using DH.Game;
using DHFramework.Localization;
using TMPro;
using UnityEngine;

namespace Game.UI.Control
{
    public class LocalizeKeyAttribute : PropertyAttribute
    {
        
    }
    
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizationText : MonoBehaviour
    {
        [LocalizeKey]
        public string key;

        public TextChangeListener[] textSizes;
        private TMP_Text textComponent;

        private void Start()
        {
            textComponent = GetComponent<TMP_Text>();
            textComponent.text = LocalizeHelper.GetGlobal(key);
            Localization.RegisterLocalize(GetInstanceID(),OnLocalize);
            foreach (var item in textSizes)
            {
                item.OnTextChanged();
            }
        }
        
        private async UniTask OnLocalize()
        {
            textComponent.text = LocalizeHelper.GetGlobal(key);
            foreach (var item in textSizes)
            {
                item.OnTextChanged();
            }
            await UniTask.CompletedTask;
        }

        private void OnDestroy()
        {
            Localization.UnRegisterLocalize(GetInstanceID());
        }
    }
}