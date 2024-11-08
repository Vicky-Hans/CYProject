using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using System.Collections.Generic;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class PullDownListViewModel : ViewModelBase
    {
        public override bool AutoDispose => true;
        [AutoNotify] private List<PullDownItemInfo> infoList=new();
        [AutoNotify] private int selectPos;
		[AutoNotify] private string titleNameStr;
        [AutoNotify] private bool isShowPullDown;
		[AutoNotify] private ObservableList<PullDownItemViewModel> scrollViewList = new();
        public Action<int> SelectAction;

        [Preserve]
        public PullDownListViewModel(List<PullDownItemInfo> showList,int initPos=0,Action<int> selectAction = null)
        {
            IsShowPullDown = false;
            InfoList.Clear();
            ScrollViewList.ClearAndDispose();
            if (showList != null)
            {
                InfoList.AddRange(showList);
                for (int i = 0; i < InfoList.Count; i++)
                {
                    scrollViewList.Add(new PullDownItemViewModel(i,InfoList[i],ClickSelect));
                }
            }
            SelectAction = selectAction;
            if (InfoList!=null && infoList.Count > initPos)
            {
                ClickSelect(initPos);
            }

            RefreshInfo();
        }

        private void ClickSelect(int pos)
        {
            IsShowPullDown = false;
            SelectPos = pos;
            foreach (var item in scrollViewList)
            {
                item.RefreshSelect(SelectPos);
            }

            RefreshInfo();
            SelectAction?.Invoke(SelectPos);
        }

        private void RefreshInfo()
        {
            if (InfoList!=null && infoList.Count > SelectPos)
            {
                var info = InfoList[SelectPos];
                TitleNameStr = info.Name;
            }
        }

        [Command]
        private void OnClickPullDown()
        {
            IsShowPullDown = true;
        }
    }
}