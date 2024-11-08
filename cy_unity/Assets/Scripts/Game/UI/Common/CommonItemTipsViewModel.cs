using System;
using Cysharp.Threading.Tasks;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.ViewModels;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public partial class CommonItemTipsViewModel : ViewModelBase
    {
        public Vector3 Position;
        public Vector3 OffSet;
        [AutoNotify] private string desc;
        [AutoNotify] private string title;
        [AutoNotify] private Vector2 bgSize;
        [AutoNotify] private bool isOpenOffset;
        [AutoNotify] private Action closeCallback;
        [AutoNotify] private bool isLeft;//是否左对齐
        private TextMeshProUGUI descText;

        public TextMeshProUGUI DescText
        {
            get => descText;
            set
            {
                descText = value;
                if (descText != null)
                {
                    if (IsLeft)
                    {
                        descText.alignment = TextAlignmentOptions.Left;
                    }
                    else
                    {
                        descText.alignment = TextAlignmentOptions.Center; 
                    }
                    UpdateBgSize();
                }
            }
        }
        private int lineHight = 45;
        private int defaultWidth = 420;
        private int offsetHight = 80;

        [Preserve]
        public CommonItemTipsViewModel(string title, string desc,bool isLeft = true)
        {
            Title = title??"";
            Desc = desc??"";
            OffSet = new Vector3(0, 50, 0);
            IsLeft = isLeft;
            if (DescText != null)
            {
                if (IsLeft)
                {
                    DescText.alignment = TextAlignmentOptions.Left;
                }
                else
                {
                    DescText.alignment = TextAlignmentOptions.Center; 
                }
            }
            UpdateBgSize();
        }
        // fubenpaim01
        public void OnClose()
        {
            UIManager.Instance.CloseDialog<CommonItemTipsView>();
        }

        public void UpdateBgSize()
        {

            var lineCount = 1;
            if (descText != null)
            {
                descText.ForceMeshUpdate();
                lineCount = descText.textInfo.lineCount;
            }
            if (lineCount < 2)
            {
                lineCount = 2;
            }

            var totalHight = lineCount * lineHight + offsetHight;
            
            BgSize = new Vector2(defaultWidth, totalHight);
        }
        public void OnClickCloseCallback()
        {
            CloseCallback?.Invoke();
        }
    }
    
}
