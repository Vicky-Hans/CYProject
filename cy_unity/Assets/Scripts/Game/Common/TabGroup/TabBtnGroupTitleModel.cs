using System;
using System.Collections.Generic;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
namespace DH.Game.ViewModels
{
    public partial class TabBtnGroupTitleModel : ViewModelBase
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
        public ObservableList<TabBtnTitleModel> BtnModels=new ();
        public Action<int> ClickAction;
        public bool IsPause;
        
        [Preserve]
        public TabBtnGroupTitleModel(List<TabBtnInfo> tabInfos,int initPos=-1,Action<int> clickAction=null)
        {
            for (int i = 0; i < tabInfos.Count; i++)
            {
                var tabBtnModel = new TabBtnTitleModel(tabInfos[i]);
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

        private void ClickBtn(int pos)
        {
            if (IsPause) return;
            CurPos = pos;
            ClickAction?.Invoke(pos);
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
    }
}