using System;
using System.Collections.Generic;
using DH.Config;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using UnityEngine;
using UnityEngine.Scripting;


namespace DH.Game.ViewModels
{
    public partial class CommonSelectRewardViewModel : ViewModelBase
    {
        
        private int selectIndex;

        public int SelectIndex
        {
            get => selectIndex;
            set
            {
                Set(ref selectIndex, value);
                RefreshCurCellSelectItem();
                RefreshSelect();
            }
        }
        

        [AutoNotify] private CellItemBaseViewModel itemBaseVm;
        public ObservableList<CellItemViewModel> itemList=new();
        
        [AutoNotify] private bool isSelect;
        private Action<int> SelectRewardAction;
        private List<Reward> rewardList;
        [AutoNotify] private string tipsTextStr;
        
        private UICircularScrollView scrollView;
        public UICircularScrollView ScrollView
        {
            get => scrollView;
            set
            {
                scrollView = value;
                if (rewardList !=null && rewardList.Count < 5 && ScrollView!= null)
                {
                    ScrollView.childAlignment = TextAnchor.MiddleCenter;
                }
            }
        }
        
        [Preserve]
        public CommonSelectRewardViewModel(List<Reward> selectList,int initPos,Action<int> callBack)
        {
            rewardList = selectList;
            InitList(selectList);
            SelectIndex = initPos; //DataCenter.discountShopData.GetSelectPacket(packetId);
            SelectRewardAction = callBack;
        }
        private void InitList(List<Reward> selectList)
        {
            itemList.Clear();
            for (int i = 0; i < selectList.Count; i++)
            {
                int index = i;
                CellItemViewModel model = CellItemViewModel.Create(selectList[i]);
                model.SetClickAction((info) =>
                {
                     SelectIndex = index;
                });
                itemList.Add(model);
            }
            if (itemList.Count < 5 && ScrollView!= null)
            {
                ScrollView.childAlignment = TextAnchor.MiddleCenter;
            }
        }

        private void RefreshCurCellSelectItem()
        {
            if (SelectIndex != -1)
            {
                ItemBaseVm =CellItemBaseViewModel.Create(rewardList[SelectIndex],ECellItemSizeType.Size180X150);
            }

            IsSelect = SelectIndex != -1;
            TipsTextStr = !IsSelect
                ? LocalizeHelper.GetGlobal(GlobalLanguageId.Trigger08)
                : $"{LocalizeHelper.GetGlobal(GlobalLanguageId.mofaxy17)}{UIHelper.GetRewardName(ItemBaseVm.BaseData)}";

        }
        private void RefreshSelect()
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                itemList[i].State = i == SelectIndex? ECellItemState.Select :ECellItemState.None;
            }
        }
        
        public void OnClickClose()
        {
            UIManager.Instance.CloseDialog<CommonSelectRewardView>();
        }
        [Command]
        private void OnClickBtnOK()
        {
            OnClickClose();
            if (SelectIndex == -1)return;
            SelectRewardAction?.Invoke(SelectIndex);
        }
       
    }
}