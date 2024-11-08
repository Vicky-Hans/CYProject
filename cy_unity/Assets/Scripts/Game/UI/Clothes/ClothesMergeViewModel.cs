using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class ClothesMergeViewModel : ViewModelBase, IComparer
    {
	    public override bool AutoDispose => true;
	    private HeroEquipData selectMergeData;

	    public HeroEquipData SelectMergeData
	    {
		    get => selectMergeData;
		    set
		    {
			    Set(ref selectMergeData, value);
			    // RefreshListState();
			    //HeroEquipCollectionView?.Refresh();
		    }
	    }
	    [AutoNotify] private CellItemBaseViewModel cellItemBaseViewVm;
		[AutoNotify] private ObservableList<ClothesMergeTopItemViewModel> scrollViewTopList = new();
		[AutoNotify] private string btnSortPath;
		[AutoNotify] private string sortTitleNameStr;
		[AutoNotify] private ObservableList<ClothesMergeItemViewModel> scrollViewList = new();
		[AutoNotify] private BottomComponentViewModel bottomComponentVm;
		[AutoNotify] private int sortType = 0;
		[AutoNotify] private bool isCanOneKey;
		[AutoNotify] private bool isCanMerge;
		[AutoNotify] private int quality;
		private ICollectionView heroEquipCollectionView;

        
		public ICollectionView HeroEquipCollectionView
		{
			get => heroEquipCollectionView;
			set
			{
				heroEquipCollectionView = value;
				if (heroEquipCollectionView == null) return;
				heroEquipCollectionView.Filter = FilterChip;
				heroEquipCollectionView.Comparer = this;
				heroEquipCollectionView?.Refresh();
			}
		}
		public UICircularScrollView UiScrollViewList;
		[AutoNotify] private GameObject moveParent;

		[AutoNotify] private string mergeTipsDesc;
        [Preserve]
        public ClothesMergeViewModel()
        {
	        InitList();
	        InitMergeTopItem();
	        RefreshSortName();
	        RefreshOneKey();
	        RefreshMergeBtnShow();
	        BottomComponentVm = new BottomComponentViewModel(() =>
	        {
				UIManager.Instance.CloseDialog<ClothesMergeView>();
	        });
	        Quality = DataCenter.roleData.GetNowHero().Qlt;
	        DataCenter.roleData.PropertyChanged += RoleDataPropertyChanged;
        }

        protected override void OnDispose()
        {
	        base.OnDispose();
	        DisposeCollection();
	        DataCenter.roleData.PropertyChanged -= RoleDataPropertyChanged;
        }

  

        private void InitList()
        {
	        BindCollection(DataCenter.clothesData.Items, scrollViewList, CreateHeroEquipItem, RemoveHeroEquipItem, ClearAllHeroEquipItem);
	        InitCollection();
	        // RefreshClothesList();
	        HeroEquipCollectionView?.Refresh();
        }

        private void RemoveHeroEquipItem(long arg1, HeroEquipData data)
        {
	        DHLog.Debug("Cjl111: RemoveHeroEquipItem:");

	        foreach (var heroEquipModel in ScrollViewList)
	        {
		        if (heroEquipModel.CellItemBaseViewVm.BaseData.HeroEquip?.Uid == data.Uid)
		        {
			        ScrollViewList.Remove(heroEquipModel);
			        break;
		        }
	        }
	        // HeroEquipCollectionView?.Refresh();
        }

        private void CreateHeroEquipItem(long arg1, HeroEquipData data)
        {
	        DHLog.Debug("Cjl111: CreateHeroEquipItem:");
	        ScrollViewList.Add(new ClothesMergeItemViewModel(data.Uid,ClickItem));
	        // HeroEquipCollectionView?.Refresh();
        }


        private void ClearAllHeroEquipItem()
        {
	        for (int i = ScrollViewList.Count-1; i > 0; i--)
	        {
		        var heroEquipModel = ScrollViewList[i];
		        if (heroEquipModel.CellItemBaseViewVm.BaseData.HeroEquip!=null)
		        {
			        ScrollViewList.Remove(heroEquipModel);
			        break;
		        }
	        }
	        // HeroEquipCollectionView?.Refresh();
        }

  


        private void RefreshClothesList()
        {
	        for (int i = ScrollViewList.Count-1; i >=0; i--)
	        {
		        if (ScrollViewList[i].State == ClothesMergeItemState.Reward)
		        {
			        scrollViewList[i].Dispose();
			        scrollViewList.RemoveAt(i);
		        }
	        }

	        RaisePropertyChanged(nameof(ScrollViewList));
	        if (SelectMergeData != null)
	        {
		        var mergeItemList = ClothesManager.Instance.GetMergeAllItem();
		        foreach (var item in mergeItemList)
		        {
			        if (CheckIsCanMerge(ClothesManager.Instance.GetRewardUid(item.Id, 999), ClothesMergeItemState.Reward))
			        {
				        var ownNum = DataCenter.itemsData.GetItemCountById(item.Id);
				        ownNum = Math.Min(ownNum, 20);
				        for (int i = 0; i < ownNum; i++)
				        {
					        scrollViewList.Add(new ClothesMergeItemViewModel(item,ClickItem,i));
				        }
				        break;
			        }
		        }
	        }
        }

        private void ClickItem(ClothesMergeItemViewModel model)
        {
	        
	        if (model.SelectState)
	        {
		        foreach (var topModel in ScrollViewTopList)
		        {
			        if (topModel.Uid == model.Uid && (topModel.ShowState == ClothesMergeItemState.Reward || topModel.ShowState == ClothesMergeItemState.HeroEquip))
			        {
				        topModel.ClickBack();
				        RefreshMergeBtnShow();
			        }
		        }
	        }
	        else
	        {
		        if (SelectMergeData==null)
		        {
			        if (model.CellItemBaseViewVm.BaseData.HeroEquip.IsNull())
			        {
				        //首先选择的不能是道具
				        return;
			        }
		        
			        if (ClothesManager.Instance.IsMergeMaxQua(model.CellItemBaseViewVm.BaseData.HeroEquip.QuaId))
			        {
				        ToastManager.ShowLanguage(GlobalLanguageId.heroEquip_Tips_10);
				        return;
			        }
		        }
		        

		        if (SelectMergeData == null)
		        {
			        
			        model.ClickPosition = model.rect.position;
			        SelectMergeTopItem(model,model.CellItemBaseViewVm.HeroEquipData);
		        }
		        else
		        {
			        if (model.CellItemBaseViewVm.HeroEquipData != null && DataCenter.clothesData.IsUseIng(model.CellItemBaseViewVm.HeroEquipData.Uid))
			        {
				        // ToastManager.Show("穿戴中的服饰不可用于合成");
				        return;
			        }

			        SelectMergeItem(model,model.CellItemBaseViewVm.BaseData,model.Uid);
		        }
	        }
	        RefreshMergeBtnShow();
        }

        private void InitMergeTopItem()
        {
	        ScrollViewTopList.ClearAndDispose();
	        ScrollViewTopList.Add(new ClothesMergeTopItemViewModel(null,true, (uid,state) =>
	        {
		        SelectMergeData = null;
		        RefreshClothesList();
		        ClothesManager.Instance.IsMergeSelect = true;
		        RefreshListState();
		        SelectMergeTopItem();
		        RefreshMergeBtnShow();
		        HeroEquipCollectionView?.Refresh();
	        },true));
	        CellItemBaseViewVm = null;
	        ClothesManager.Instance.IsMergeSelect = false;
        }

        private void SelectMergeTopItem(ClothesMergeItemViewModel itemModel=null,HeroEquipData tempData=null)
        {
	        SelectMergeData = tempData;

	        ClothesManager.Instance.IsMergeSelect = SelectMergeData!=null;
	        for (int i = ScrollViewTopList.Count-1; i > 0; i--)
	        {
		        ScrollViewTopList[i].Dispose();
		        ScrollViewTopList.RemoveAt(i);
	        }
	        
	        if (SelectMergeData ==null || ClothesManager.Instance.IsMergeMaxQua(selectMergeData.QuaId))
	        {
		        var topModel = ScrollViewTopList[0];
		        topModel.Merge();
		        topModel.IsExit = false;
		        topModel.IsShowExit = false;
		        CellItemBaseViewVm = null;
		        SelectMergeData = null;
		        RefreshClothesList();
	        }
	        else
	        {
		        var heroData = new HeroEquipData
		        {
			        Uid = 0,
			        Id = selectMergeData.Id,
			        QuaId = ClothesManager.Instance.GetNextQuaId(selectMergeData.QuaId),
			        Lv = selectMergeData.Lv,
		        };
		        CellItemBaseViewVm = CellItemBaseViewModel.Create(heroData,ECellItemSizeType.Size166X150);
		        CellItemBaseViewVm.OnClickEvent = (info) =>
		        {
					//拦截点击事件
		        };
		        var topModel = ScrollViewTopList[0];
		        topModel.Merge(SelectMergeData);
		        topModel.IsExit = true;
		        topModel.IsShowExit = false;
		        
		        PlayFly(topModel,itemModel,true, () =>
		        {
			        CreateItem();
			        RefreshClothesList();
		        });
	        }
        }

        private void CreateItem()
        {
	        var needNum = ClothesManager.Instance.GetMergeNeedNum(selectMergeData);
	        for (int i = 0; i < needNum; i++)
	        {
		        var data = new HeroEquipData()
		        {
			        Uid = 0,
			        Id = SelectMergeData.Id,
			        QuaId = SelectMergeData.QuaId,
			        Lv = 0,
		        };

		        var quacfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(selectMergeData.QuaId);
		        ScrollViewTopList.Add(new ClothesMergeTopItemViewModel(data,i!=needNum-1, (uid, state) =>
		        {
			        RefreshListState();
			        HeroEquipCollectionView?.Refresh();
			        RefreshMergeBtnShow();
		        },false,quacfg?.Type ?? 2));
	        }
        }

        private void SelectMergeItem(ClothesMergeItemViewModel itemModel,ResourceData data,long rewardUid = 0)
        {
	        if (!CheckNonePos())
	        {
		        //不存在多余位置提示
		        return;
	        }

	        if (data.HeroEquip!=null && !data.HeroEquip.IsNull() && CheckIsAdd(data.HeroEquip.Uid))
	        {
		        return;
	        }

	        for (int i = 1; i < ScrollViewTopList.Count; i++)
	        {
		        var topModel = ScrollViewTopList[i];
		        if(topModel.ShowState!=ClothesMergeItemState.Sketch && topModel.IsExit) continue;
		        if (data.HeroEquip!=null && !data.HeroEquip.IsNull())
		        {
			        topModel.Merge(data.HeroEquip);
			        topModel.IsExit = true;
			        topModel.IsShowExit = false;
		        }
		        else
		        {
			        var reward = new Reward((RewardType)data.Type, data.Id, 1);
			        topModel.Merge(reward,rewardUid);
			        topModel.IsExit = true;
			        topModel.IsShowExit = false;
		        }
		        PlayFly(topModel,itemModel);
				return;
	        }
        }

        private void PlayFly(ClothesMergeTopItemViewModel topModel, ClothesMergeItemViewModel itemModel,bool isFirst = false,Action flyEnd=null)
        {
	        HeroEquipCollectionView?.Refresh();
	        UiScrollViewList.ScrollToPos(Vector2.zero,0.2f);
	        if(isFirst)
				topModel.IsShowAdd = false;
	        UIHelper.FlyChip(moveParent,UIHelper.GetRewardsIconPath(itemModel.CellItemBaseViewVm.BaseData),
		        isFirst?itemModel.ClickPosition:itemModel.GetRectTransform().position, topModel.NoneTransform.position,
		        () =>
		        {
			        flyEnd?.Invoke();
			        topModel.IsShowExit = true;
			        if(isFirst)
						topModel.IsShowAdd = true;
			        RefreshListState();
		        }).Forget();
        }

        private bool CheckNonePos()
        {
	        if (ScrollViewTopList.Count <= 1 ) return true;
	        for (int i = 1; i < ScrollViewTopList.Count; i++)
	        {
		        var topModel = ScrollViewTopList[i];
		        if (topModel.ShowState != ClothesMergeItemState.HeroEquip && topModel.ShowState != ClothesMergeItemState.Reward && !topModel.IsExit)
		        {
			        return true;
		        }
	        }

	        return false;

        }

        private bool CheckIsAdd(long uid)
        {
	        for (int i = 1; i < ScrollViewTopList.Count; i++)
	        {
		        var topModel = ScrollViewTopList[i];
		        if (topModel.ShowState == ClothesMergeItemState.HeroEquip && topModel.IsExit)
		        {
			        if (topModel.Uid == uid)
				        return true;
		        }
	        }

	        return false;
        }

        private void RefreshListState()
        {
	        foreach (var model in ScrollViewList)
	        {
		        if (SelectMergeData != null)
		        {
			        model.SelectState = CheckTopItemIsUse(model.Uid);
			        model.LockState = !CheckIsCanMerge(model.Uid,model.State);
		        }
		        else
		        {
			        model.SelectState = false;
			        model.LockState = false;
		        }
	        }
        }

        private bool CheckTopItemIsUse(long uid)
        {
	        foreach (var item in ScrollViewTopList)
	        {
		        if (item.Uid == uid) return true;
	        }
	        return false;
        }

        public bool CheckIsCanMerge(long uid,ClothesMergeItemState state)
        {
	        if (SelectMergeData == null) return false;
	        if (state == ClothesMergeItemState.HeroEquip)
	        {
		        var quaCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(SelectMergeData.QuaId);
		        if (quaCfg != null)
		        {
			        var partId = ClothesManager.Instance.GetHeroEquipPart(selectMergeData.Id);
			        var data = DataCenter.clothesData.GetHeroEquipDataByUid(uid);
			        if (data != null)
			        {
				        // if (ClothesManager.Instance.IsMergeMaxLV(data.QuaId))
				        // {
					       //  return false;
				        // }

				        if (DataCenter.clothesData.IsUseIng(data.Uid))
				        {
					        return false;
				        }
				        
				        var selectCfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(selectMergeData.Id);
				        var cfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(data.Id);
				        if (selectCfg.RareType==1 && cfg?.RareType!=1)
				        {
					        return false;
				        }
				        
				        var partId1 = ClothesManager.Instance.GetHeroEquipPart(data.Id);
				        var selectQua = ClothesManager.Instance.GetQuaSmallByQuaId(SelectMergeData.QuaId);
				        var dataQua = ClothesManager.Instance.GetQuaSmallByQuaId(data.QuaId);
						

				        if (quaCfg.Type == 1)
				        {
					        
					        if (!ClothesManager.Instance.CheckStateStart(data.QuaId))
					        {
						        return false;
					        }
					        return selectQua!=0 && selectQua == dataQua && partId == partId1 && SelectMergeData.Id == data.Id;
				        }else if (quaCfg.Type == 2)
				        {
					        if (!ClothesManager.Instance.CheckStateStart(data.QuaId))
					        {
						        return false;
					        }
					        return selectQua == dataQua && partId == partId1 ;
				        }else if (quaCfg.Type == 3)
				        {
					        return SelectMergeData.QuaId == data.QuaId && partId == partId1 && SelectMergeData.Id == data.Id;
				        } 
			        }
		        }
	        }
	        else
	        {
		        var quaCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(SelectMergeData.QuaId);
		        if (quaCfg.Type == 2)
		        {
			        var needId = ClothesManager.Instance.GetNeedItemId(SelectMergeData.Id,true,SelectMergeData.QuaId);
			        var itemId = ClothesManager.Instance.ResolveRewardUid(uid);
			        return itemId == needId;
		        }
	        }
	        return false;
        }

        private bool FilterChip(object obj)
        {
	        if (obj is not ClothesMergeItemViewModel heroEquipModel) return false;
	        if (heroEquipModel.State == ClothesMergeItemState.HeroEquip && SelectMergeData != null)
	        {
		        return heroEquipModel.CellItemBaseViewVm.BaseData.HeroEquip.Uid != SelectMergeData.Uid;
	        }

	        if (heroEquipModel.State == ClothesMergeItemState.HeroEquip)
	        {
		        return !ClothesManager.Instance.IsMergeMaxQua(heroEquipModel.CellItemBaseViewVm.BaseData.HeroEquip.QuaId);
	        }

	        if (heroEquipModel.State == ClothesMergeItemState.Reward)
	        {
		        return SelectMergeData != null;
	        }

	        return true;
        }
        
        public int Compare(object x, object y)
        {
	        if (!(x is ClothesMergeItemViewModel item1) || !(y is ClothesMergeItemViewModel item2)) return 0;

	        return SortValue(item2) - SortValue(item1);
        }

        private int SortValue(ClothesMergeItemViewModel item)
        {
	        int sortValue=0;
	        if (SelectMergeData != null)
	        {
		        if (CheckIsCanMerge(item.Uid, item.State))
		        {
			        sortValue += 200000000;
		        }
		        
		        if (item.State == ClothesMergeItemState.HeroEquip)
		        {
			        sortValue += 20000000;
			        var data = item.CellItemBaseViewVm.BaseData.HeroEquip;
			        var cfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(data.Id);
			        if (cfg == null) return sortValue;
			        if (SortType == 0)
			        {
				        sortValue += data.QuaId * 1000000;
				        sortValue += data.Lv * 10000;
				        sortValue += cfg.RareType * 1000;
				        sortValue += (6-cfg.PartId) * 100;
				        sortValue += data.Id;
			        }else  if (SortType == 1)
			        {
				        sortValue += data.Lv * 1000000;
				        sortValue += data.QuaId * 10000;
				        sortValue += cfg.RareType * 1000;
				        sortValue += (6-cfg.PartId) * 100;
				        sortValue += data.Id;
			        }else  if (SortType == 2)
			        {
				        sortValue += (6-cfg.PartId) * 1000000;
				        sortValue += data.Lv * 10000;
				        sortValue += data.QuaId * 1000;
				        sortValue += cfg.RareType * 100;
				        sortValue += data.Id;
			        }
		        }
		        else
		        {
			        sortValue += 50000000;
			        var itemCfg = ConfigCenter.ItemCfgColl.GetDataById(item.CellItemBaseViewVm.BaseData.Id);
			        if (itemCfg != null)
			        {
				        sortValue += itemCfg.Quality * 100000;
			        }
		        }
	        }
	        else
	        {
		        if (item.State == ClothesMergeItemState.HeroEquip)
		        {
			        sortValue += 200000000;
			        var data = item.CellItemBaseViewVm.BaseData.HeroEquip;
			        var cfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(data.Id);
			        if (cfg == null) return sortValue;
			        if (SortType == 0)
			        {
				        sortValue += data.QuaId * 1000000;
				        sortValue += data.Lv * 10000;
				        sortValue += cfg.RareType * 1000;
				        sortValue += (6-cfg.PartId) * 100;
				        sortValue += data.Id;
			        }else  if (SortType == 1)
			        {
				        sortValue += data.Lv * 1000000;
				        sortValue += data.QuaId * 10000;
				        sortValue += cfg.RareType * 1000;
				        sortValue += (6-cfg.PartId) * 100;
				        sortValue += data.Id;
			        }else  if (SortType == 2)
			        {
				        sortValue += (6-cfg.PartId) * 10000000;
				        sortValue += data.Lv * 100000;
				        sortValue += data.QuaId * 1000;
				        sortValue += cfg.RareType * 100;
				        sortValue += data.Id;
			        }
		        }
		        else
		        {
			        sortValue += 10000000;
			        var itemCfg = ConfigCenter.ItemCfgColl.GetDataById(item.CellItemBaseViewVm.BaseData.Id);
			        if (itemCfg != null)
			        {
				        sortValue += itemCfg.Quality * 100000;
			        }
		        }
		        
	        }
	        return sortValue;
        }

        private List<long> GetTopSelectList()
        {
	        var list = new List<long>();
	        if (selectMergeData == null) return list;
	        foreach (var topModel in ScrollViewTopList)
	        {
		        if(topModel.IsFirst) continue;
		        if (topModel.ShowState == ClothesMergeItemState.HeroEquip)
		        {
			        list.Add(topModel.Uid);
		        }
	        }

	        return list;
        }
        
        private Reward GetTopSelectReward()
        {
	        int num = 0;
	        if (selectMergeData == null) return null;
	        var id = ClothesManager.Instance.GetNeedItemId(selectMergeData.Id,true,SelectMergeData.QuaId);
	        if (id == 0) return null;
	        foreach (var topModel in ScrollViewTopList)
	        {
		        if(topModel.ShowState == ClothesMergeItemState.HeroEquip && topModel.Uid == SelectMergeData.Uid) continue;
		        if (topModel.ShowState == ClothesMergeItemState.Reward)
		        {
			        num++;
		        }
	        }

	        return new Reward(RewardType.Item,id,num);
        }

        private void RefreshOneKey()
        {
	        IsCanOneKey = ClothesManager.Instance.IsCanOneKeyMerge();
        }

        private void RefreshSortName()
        {
	        if (SortType == 0)
	        {
		        SortTitleNameStr = LocalizeHelper.GetGlobal(GlobalLanguageId.heroEquip_Tips_02);
	        }else if (SortType == 1)
	        {
		        SortTitleNameStr = LocalizeHelper.GetGlobal(GlobalLanguageId.heroEquip_Tips_04);
	        }else if (SortType == 2)
	        {
		        SortTitleNameStr = LocalizeHelper.GetGlobal(GlobalLanguageId.heroEquip_Tips_03);
	        }
        }
        
    


        private void RefreshMergeBtnShow()
        {
	        IsCanMerge = selectMergeData != null && !CheckNonePos();
	        if (SelectMergeData == null)
	        {
		        MergeTipsDesc = LocalizeHelper.GetGlobal(GlobalLanguageId.heroEquip_Tips_06);
	        }
	        else
	        {
		        var quaCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(SelectMergeData.QuaId);
		        if (quaCfg != null)
		        {
			        if (quaCfg.Type == 2)
			        {
				        MergeTipsDesc = LocalizeHelper.GetGlobal(GlobalLanguageId.heroEquip_Tips_09,quaCfg.Num,ClothesManager.Instance.GetQuaName(SelectMergeData.Id,SelectMergeData,isBase:false),ClothesManager.Instance.GetPartNameById(SelectMergeData.Id));
			        }else if (quaCfg.Type == 1 || quaCfg.Type == 3)
			        {
				        MergeTipsDesc = LocalizeHelper.GetGlobal(GlobalLanguageId.heroEquip_Tips_09,quaCfg.Num,ClothesManager.Instance.GetQuaName(SelectMergeData.Id,SelectMergeData,isBase:false),ClothesManager.Instance.GetHeroEquipNameById(SelectMergeData.Id));
			        }
		        }
	        }
        }


        [Command]
        private void OnClickBtnMergeOneKey()
        {
	        if (!IsCanOneKey)
	        {
		        ToastManager.ShowLanguage(GlobalLanguageId.heroEquip_Tips_24);
		        return;
	        }

	        ClothesManager.Instance.SendOneKeyMergeHeroEquip((rewards) =>
	        {
		        if (rewards.Count > 0)
		        {
			        List<Resource> dataList = new();
			        foreach (var item in rewards)
			        {
				        for (int i = 0; i < 2; i++)
				        {
					        if(dataList.Count>=5) continue;
					        dataList.Add(new Resource()
					        {
						        Id = item.Id,
						        Type = (int)RewardType.HeroEquip,
						        HeroEquip = new HeroEquip()
						        {
							        Id = item.Id,
							        QuaId = item.QuaId-1,
							        Lv = 1,
						        }
					        });
				        }
			        }

			        if (dataList.Count > 0)
			        {
				        while (dataList.Count<5)
				        {
					        dataList.Add(dataList[0]);
				        }
				        var succeedModel = new ClothesMergeSucceedViewModel(0,dataList,rewards);
				        UIManager.Instance.OpenDialog<ClothesMergeSucceedView>(succeedModel).Forget();
			        }
		        }
		        SelectMergeData = null;
		        ClothesManager.Instance.IsMergeSelect = SelectMergeData!=null;
		        RefreshListState();
		        RefreshOneKey();
		        RefreshMergeBtnShow();
		        RefreshClothesList();
	        }).Forget();
        }

        [Command]
        private void OnClickBtnMerge()
        {
	        if (selectMergeData == null)
	        {
		        ToastManager.ShowLanguage(GlobalLanguageId.heroEquip_Tips_24);
		        return;
	        }

	        if (CheckNonePos())
	        {
		        ToastManager.ShowLanguage(GlobalLanguageId.heroEquip_Tips_24);
		        return;
	        }

	         var str = "";
	         var data =DataCenter.clothesData.GetHeroEquipDataByUid(selectMergeData.Uid);

	        var cfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(data.Id);
	         str = $"OnClickBtnMerge Mian: uid:{data.Uid}    id:{data.Id}   lv:{data.Lv}  quaId:{data.QuaId}  part:{cfg.PartId}\n";
	         DHLog.Debug(str);
		     var list = GetTopSelectList();
		     foreach (var item in list)
		     {   
			     data =DataCenter.clothesData.GetHeroEquipDataByUid(selectMergeData.Uid);
			     cfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(data.Id);
			     str = $"OnClickBtnMerge HeroEquip: uid:{data.Uid}    id:{data.Id}   lv:{data.Lv}  quaId:{data.QuaId}  part:{cfg.PartId}\n";
			     DHLog.Debug(str);
		     }
	     
		     var itemList = GetTopSelectReward();
		     if (itemList != null)
		     {
			     var cfgItem = ConfigCenter.ItemCfgColl.GetDataById(itemList.Id);
			     str = $"OnClickBtnMerge Item: {cfgItem.Id}    Name:{UIHelper.GetRewardName(itemList)}   Count:{itemList.Count}  ";
			     DHLog.Debug(str);
		     }
		    var selectHeroEquipList = GetTopSelectList();
		    var selectItemReward = GetTopSelectReward();
		    
		    List<Resource> dataList = new List<Resource>();
		    var dataBase = UIHelper.HeroEquipDataToHeroEquip(selectMergeData);
		    dataBase.Uid = 0;
		    var res1 = new Resource()
		    {
			    Type = (int)RewardType.HeroEquip,
			    Id = selectMergeData.Id,
			    Count = 1,
			    HeroEquip = dataBase,
		    };
		    dataList.Add(res1);
		    
		    foreach (var item in selectHeroEquipList)
		    {
			    var heroData = DataCenter.clothesData.GetHeroEquipDataByUid(item);
			    var res = new Resource()
			    {
				    Type = (int)RewardType.HeroEquip,
				    Id = heroData.Id,
				    Count = 1,
				    HeroEquip = UIHelper.HeroEquipDataToHeroEquip(heroData),
			    };
			    dataList.Add(res);
		    }

		    if (selectItemReward != null && selectItemReward.Count > 0)
		    {
			    dataList.Add(UIHelper.RewardToResource(selectItemReward));
		    }


		    ClothesManager.Instance.SendMergeHeroEquip(selectMergeData.Uid,selectHeroEquipList,selectItemReward,() =>
	        {
		 
		        var succeedModel = new ClothesMergeSucceedViewModel(selectMergeData.Uid,dataList);
		        UIManager.Instance.OpenDialog<ClothesMergeSucceedView>(succeedModel).Forget();
		        SelectMergeData = null;
		        ClothesManager.Instance.IsMergeSelect = SelectMergeData!=null;
		        RefreshListState();
		        SelectMergeTopItem();
		        RefreshOneKey();
		        RefreshMergeBtnShow();
		        RefreshClothesList();
		        Refresh().Forget();
		
	        }).Forget();
        }

        private async UniTaskVoid Refresh()
        { 
	        await UniTask.Delay(300);
	        HeroEquipCollectionView?.Refresh();
        }

        [Command]
        private void OnClickBtnSort()
        {
	        if(SelectMergeData!=null) return;
	        if (sortType >= 2)
	        {
		        SortType = 0;
	        }
	        else
	        {
		        SortType += 1;
	        }
	        RefreshSortName();
	        HeroEquipCollectionView?.Refresh();
        }


        [Command]
        private void OnClickBtnMergeUnOneKey()
        {
	        ToastManager.ShowLanguage(GlobalLanguageId.heroEquip_Tips_24);
        }
        
        
        private void RoleDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof( DataCenter.roleData.FmtHero))
	        {
		        Quality = DataCenter.roleData.GetNowHero().Qlt;
	        }
        }
    }
}