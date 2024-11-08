using System;
using System.Collections.Generic;
using DH.Data;
using DH.UIFramework;
using DH.UIFramework.ViewModels;
using DHFramework;
using UnityEngine;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public partial class CommonButViewModel : ViewModelBase
    {
        [AutoNotify] private string iconPath;
        [AutoNotify] private bool redNote;
        [AutoNotify] private string nameText;
        private Action<List<object>> mOnClick;
        private Func<bool> mShowRed;
        public MainStageInfoNodeRightButType Type;
        public RectTransform FunctionMenuTransform;
        private long times;
        [AutoNotify] private bool isShowTimeDes;
        [AutoNotify] private string timeText;
        [Preserve]
        public CommonButViewModel(MainStageInfoNodeRightButType mType, string mIconPath,string name,Action<List<object>> onClick,Func<bool> showRed = null,long time = 0)
        {
            Type = mType;
            IconPath = mIconPath;
            NameText = name;
            mOnClick = onClick;
            mShowRed = showRed;
            times = time;
            if (mShowRed!=null)
            {
                RedNote = mShowRed();
            }

            TimeDes();
        }
        
        [Command]
        private void OnClickBut()
        {
            if (Type == MainStageInfoNodeRightButType.FuncEnum)
            {
                mOnClick?.Invoke(new List<object>{FunctionMenuTransform});
            }
            else
            {
                mOnClick?.Invoke(null);
            }
        }

        private void TimeDes()
        {
            IsShowTimeDes = times > ServerTime.Instance.GetNowTime();
            if (IsShowTimeDes)
            {
                TimeText = ServerTime.Instance.SecondsDHAndMS(times - ServerTime.Instance.GetNowTime()); 
            }
        }

        private float time;
        public override void Update()
        {
            base.Update();
            if (!UIHelper.CalculateTime(ref time)) return;
            if (mShowRed!=null)
            {
                RedNote = mShowRed();
            }

            TimeDes();
        }
        
    }
}