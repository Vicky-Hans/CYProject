using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DH.Game;
using DHFramework.Localization;
using TMPro;
using UnityEngine;

namespace Extend
{
    public class DhText:TextMeshProUGUI
    {
        [SerializeField]
        private string languageKey = string.Empty;
        
        protected override void Awake()
        {
            base.Awake();
            Refresh();
            tag = "FontText";

        }

        protected override void Start()
        {
            base.Start();
            if (languageKey != String.Empty)
            {
                Localization.RegisterLocalize(GetInstanceID(),OnLocalize);
            }
        }

        private async UniTask OnLocalize()
        {
            Refresh();
            await UniTask.CompletedTask;
        }
        
        
        public void SetText( string key )
        {
            text = LocalizeHelper.GetGlobal(key);
        }
        
        public void ReplaceSpace( string targetStr )
        {
            if( targetStr.Contains( " " ) )
            {
                targetStr = targetStr.Replace( " ", "\u00A0" );
            }
            text = targetStr;
        }
        
        public void SetGrayActive( bool gray, bool recursive = true, bool purple = false)
        {
            UIHelper.SetGray(gameObject, gray, recursive,purple);
        }
        
        public void Refresh()
        {
            if (languageKey != String.Empty && Application.isPlaying)
                text = LocalizeHelper.GetGlobal(languageKey);
        }
        
        private Sequence mTextAnim;
        //逐字动画
        public void StartTextAnim( string textStr, float time )
        {
            StopTextAnim();
            text = string.Empty;
            mTextAnim = DoTextAni( textStr, time );
            mTextAnim.SetLoops(1);
            mTextAnim.Play();
        }
        
        public void StopTextAnim()
        {
            if( mTextAnim != null )
            {
                mTextAnim.Kill();
                mTextAnim = null;
            }
        }
        
        private Sequence DoTextAni(string targetTextStr, float duration)
        {
            // 使用DOText方法逐个显示文本
            // 设置文本初始状态
            text = "";
        
            // 逐个显示文本
            return DOTween.Sequence()
                .Append(DOTween.To(() => 0, x => UpdateText(targetTextStr,x), targetTextStr.Length, duration))
                .SetEase(Ease.Linear);
            
        }
        
        private void UpdateText(string targetStr,int length)
        {
            text = targetStr.Substring(0, length);
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (languageKey != String.Empty)
            {
                Localization.UnRegisterLocalize(GetInstanceID());
            }
        }

    }
}