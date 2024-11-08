using System;
using System.Collections.Generic;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
namespace DH.Game.ViewModels
{
    public struct TabBtnInfo
    {
        public int Pos;
        public string Name;
        public string OnPath;
        public string OffPath;
        public bool IsLock;
        public string LockDesc;
    }
    
    public partial class TabBtnGroupViewModel : ViewModelBase
    {
        private int curPos;

        public int CurPos
        {
            get => curPos;
            set
            {
                Set(ref curPos, value);
                RefreshSelect();
            }
        }
        public ObservableList<TabBtnViewModel> BtnModels=new ();
        public Action<int> ClickAction;
        public bool IsPause;
        
        [Preserve]
        public TabBtnGroupViewModel(List<TabBtnInfo> tabInfos,int initPos=-1,Action<int> clickAction=null)
        {
            for (int i = 0; i < tabInfos.Count; i++)
            {
                var tabBtnModel = new TabBtnViewModel(tabInfos[i]);
                tabBtnModel.ClickEvent = ClickBtn;
                BtnModels.Add(tabBtnModel);
            }

            if (initPos == -1 && BtnModels.Count>0)
            {
                CurPos = BtnModels[0].Pos;
            }
            else
            {
                CurPos = initPos;
            }

            ClickAction = clickAction;
            ClickAction?.Invoke(initPos);
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            UIHelper.ViewModelBaseOnDisposes(BtnModels);
            ClickAction = null;
        }

        private void ClickBtn(int pos)
        {
            if (IsPause) return;
            CurPos = pos;
            ClickAction?.Invoke(pos);
        }

        public void RefreshSetSelect(int curPos)
        {
            CurPos = curPos;
        }

        private void RefreshSelect()
        {
            for (int i = 0; i < BtnModels.Count; i++)
            {
                BtnModels[i].SelectPos = CurPos;
            }
        }
        
        public void RefreshRedDot(int pos,bool redState)
        {
            for (int i = 0; i < BtnModels.Count; i++)
            {
                if (BtnModels[i].Pos == pos)
                {
                    BtnModels[i].ShowRedDot = redState;
                }
            }
        }
        
        public void RefreshLockState(int pos,bool lockState)
        {
            for (int i = 0; i < BtnModels.Count; i++)
            {
                if (BtnModels[i].Pos == pos)
                {
                    BtnModels[i].IsLock = lockState;
                }
            }
        }
    }
}