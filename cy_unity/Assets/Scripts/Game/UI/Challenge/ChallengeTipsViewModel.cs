using System;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.ViewModels;

namespace DH.Game.ViewModels
{
    public partial class ChallengeTipsViewModel : ViewModelBase
    {
        [AutoNotify] private string refreshTimeStr;
        private readonly Action callback;
        private bool isAutoClose;
        private long closeTime;
        private string defaultStr;
        /// <summary>
        /// 需要一个回调函数，用于关闭弹窗
        /// </summary>
        /// <param name="languageKey"> 自动关闭的时间</param>
        /// <param name="autoCloseTime"> 自动关闭的时间</param>
        /// <param name="callback">关闭的回调</param>
        /// <param name="args">替换的参数 时间位置用{0} 带提</param>
        public ChallengeTipsViewModel(string languageKey,int autoCloseTime, Action callback,params object[] args)
        { 
            defaultStr = args.Length > 0 ? LocalizeHelper.GetGlobal(languageKey, args):LocalizeHelper.GetGlobal(languageKey);
            this.callback = callback;
            closeTime = Lodash.GetUnixTime() + autoCloseTime;
            isAutoClose = false;
        }

        [Command]
        private void OnClickOpBtn()
        {
            callback.Invoke();
            UIManager.Instance.CloseDialog<ChallengeTipsView>();
        }
        public void OnClickCloseBtn()
        {
            callback.Invoke();
            UIManager.Instance.CloseDialog<ChallengeTipsView>();
        }
        public override void Update()
        {
            base.Update();
            UpdateRefreshTime();
        }

        private void UpdateRefreshTime()
        {
            var nowTime = Lodash.GetUnixTime();
            if (nowTime <= closeTime)
            {
                var tempStr = $"</color><color=#00fff7>{closeTime - nowTime}</color><color=#ad88df>";
                var tipsStr = String.Format(defaultStr, tempStr);
                RefreshTimeStr = $"<color=#ad88df>{tipsStr}</color>";
            }
            else
            {
                if(isAutoClose)return;
                isAutoClose = true;
                OnClickOpBtn();
            }
        }
    }
}