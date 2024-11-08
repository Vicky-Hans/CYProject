using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class EquipViewModel : ViewModelBase
    {
	    public override bool AutoDispose => true;
	    private int ShowNum = 4;
	    [AutoNotify] private string battleNumStr;
		[AutoNotify] private ObservableList<EquipBattleItemViewModel> battleGridList = new();
		[AutoNotify] private ObservableList<EquipItemViewModel> scrollViewList=new();
		[AutoNotify] private ObservableList<EquipLockItemViewModel> scrollViewLockList=new();

		 [AutoNotify] private int battleNum;
		 [AutoNotify] private int allBattleNum;

		 [AutoNotify] private bool isReplace;
		 [AutoNotify] private EquipItemViewModel equipItemViewModel;

		 [AutoNotify] private bool isOpenState;

		 [AutoNotify] private TabBtnGroupTitleModel tabBtnGroupTitle;
        [Preserve]
        public EquipViewModel()
        {
	        isReplace = false;
	        InitSwitchGroup();
	        InitEquipSlots();
	        InitEquipList();
	        InitEquipLockList();
	        RefreshBattleNum();
	        DataCenter.equipData.PropertyChanged += ChangeEquipData;
	        EquipManager.Instance.PropertyChanged += ChangeEquipManager;
	        DataCenter.equipData.Formations.CollectionChanged += FormationsCollectionChanged;
        }

        protected override void OnDispose()
        {
	        base.OnDispose();
	        DataCenter.equipData.PropertyChanged -= ChangeEquipData;
	        EquipManager.Instance.PropertyChanged -= ChangeEquipManager;
	        DataCenter.equipData.Formations.CollectionChanged -= FormationsCollectionChanged;
	        // UIHelper.ViewModelBaseOnDisposes(battleGridList);
	        // UIHelper.ViewModelBaseOnDisposes(scrollViewList);
	        // UIHelper.ViewModelBaseOnDisposes(scrollViewLockList);
	        // equipItemViewModel?.Dispose();
        }

        private void FormationsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
	        InitEquipList();
	        RefreshBattleNum();
        }

        private void ChangeEquipManager(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(EquipManager.Instance.ReplaceEquipId))
	        {
		        IsReplace = EquipManager.Instance.ReplaceEquipId != 0;
		        if (EquipManager.Instance.ReplaceEquipId != 0)
		        {
			        EquipItemViewModel = new EquipItemViewModel(EquipManager.Instance.ReplaceEquipId,true,true);
			        EquipItemViewModel.ClickItemAction = () =>
			        {
				        //点击替换事件
			        };
		        }
	        }else if (e.PropertyName == nameof(EquipManager.Instance.CurSelectEquipId))
	        {
		        IsOpenState =EquipManager.Instance.CurSelectEquipId!=0 && !DataCenter.equipData.IsUseIng(EquipManager.Instance.CurSelectEquipId) && IsEquipListLast();
	        }
        }

        private void ChangeEquipData(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(DataCenter.equipData.Items))
	        {
		        InitEquipList();
		        RefreshBattleNum();
	        }else if (e.PropertyName == nameof(DataCenter.equipData.CurrWearFormation))
	        {
		        InitEquipList();
		        RefreshBattleNum();
	        }
        }

        private void InitSwitchGroup()
        {  
	        List<TabBtnInfo> btnInfos = new List<TabBtnInfo>();
	        var btnInfo1 = new TabBtnInfo()
	        {
		        Pos = 1,
		        Name = "1",
	        };
	        var btnInfo2 = new TabBtnInfo()
	        {
		        Pos = 2,
		        Name = "2",
	        };
	        var btnInfo3 = new TabBtnInfo()
	        {
		        Pos = 3,
		        Name = "3",
	        };
 
	        btnInfos.Add(btnInfo1);
	        btnInfos.Add(btnInfo2);
	        btnInfos.Add(btnInfo3);

	        TabBtnGroupTitle = new TabBtnGroupTitleModel(btnInfos,DataCenter.equipData.CurrWearFormation, (pos) =>
	        {
		        if(pos != DataCenter.equipData.CurrWearFormation)
					EquipManager.Instance.SendEquipSwitch(pos).Forget();
	        });
        }

        protected void RefreshBattleNum()
        {
	        BattleNum = DataCenter.equipData.GetBattleEquipCount();
	        AllBattleNum = battleGridList.Count;
        }

        private void InitEquipSlots()
        {
	        battleGridList.ClearAndDispose();
	        var slotsList = EquipManager.Instance.GetEquipSlotsList();
	        for (int i = slotsList.Count-1; i >= 0; i--)
	        {
		        battleGridList.Add(new EquipBattleItemViewModel(slotsList[i],i%4));
	        }

        }
        
        private void InitEquipLockList()
        {
	        scrollViewLockList.ClearAndDispose();
	        var slotsList = EquipManager.Instance.GetEquipLockList();
	        UIHelper.SortList(slotsList, (itemA, itemB) => itemA.Unlock < itemB.Unlock);
	        for (int i = slotsList.Count-1; i >= 0; i--)
	        {
		        scrollViewLockList.Add(new EquipLockItemViewModel(slotsList[i].Id));
	        }
        }
        
        private void InitEquipList()
        {
	        scrollViewList.ClearAndDispose();
	        var ownList = DataCenter.equipData.GetOwnUnUseEquipList();
	        
	        for (int i = 0; i < ownList.Count; i++)
	        {
		        if(ownList[i]!= EquipManager.Instance.GoldEquipId)
					scrollViewList.Add(new EquipItemViewModel(ownList[i]));
	        }

	        var surplusNum = scrollViewList.Count % ShowNum;
	        if (surplusNum > 0)
	        {
		        var needNum = ShowNum - surplusNum;
		        for (int i = 0; i < needNum; i++)
		        {
			        scrollViewList.Add(new EquipItemViewModel(EquipManager.Instance.BaseEquipId,false));
		        }
	        }
	        SortEquipList();
        }

        private bool IsEquipListLast()
        {
	        for (int i = 0; i < scrollViewList.Count; i++)
	        {
		        if (i < 4 && EquipManager.Instance.CurSelectEquipId == scrollViewList[i].Id)
		        {
			        return true;
		        }
	        }

	        return false;
        }

        private void SortEquipList()
        {
	        UIHelper.SortList(scrollViewList, (itemA,itemB) => SortValue(itemB)>SortValue(itemA));
        }

        private int SortValue(EquipItemViewModel model)
        {
	        int sortValue = 0;
	        if (!model.IsShow)
	        {
		        sortValue += 10000000;
	        }
	        else
	        {
		        if (model.Cfg != null)
		        {
			        sortValue += (10 - model.Cfg.Quality) * 1000000;
			        sortValue += (1000 - model.Level) * 100;
			        sortValue += 100 - model.Cfg.Id;
		        }
	        }
	        return sortValue;
        }

        [Command]
        private void OnClickReplace(int pos)
        {
	        if (IsReplace)
	        {
		        var battleModel = battleGridList[^pos];
		        if (battleModel!=null)
		        {
			        if (!DataCenter.mainStageData.IsPassChapter(battleModel.Cfg.Unlock))
			        {
				        ToastManager.ShowLanguage(GlobalLanguageId.Equip_04,battleModel.Cfg.Unlock);
				        return;
			        }
		        }
		        
		        var equipData =EquipManager.Instance.GetEquipDataByPos(pos);

		        if (EquipManager.Instance.CheckEquipIsCanUse(EquipManager.Instance.ReplaceEquipId, equipData.Id))
		        {
			        EquipManager.Instance.SendEquipBattle(EquipManager.Instance.ReplaceEquipId, equipData?.Id ?? 0).Forget();
			        EquipManager.Instance.ReplaceEquipId = 0;
		        }
		        else
		        {
			        EquipManager.Instance.ReplaceEquipId = 0;
			        ToastManager.ShowLanguage(GlobalLanguageId.Equip_20);
		        }


	        }
        }

        [Command]
        private void OnClickCloseReplace()
        {
	        EquipManager.Instance.ReplaceEquipId = 0;
        }
    }
}