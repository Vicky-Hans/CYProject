using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using Game.UI.MainUi;
using Spine.Unity;
using UnityEngine;


namespace DH.Game.ViewModels
{
	public enum ClothesUI
	{
		HeroEquip,
		Hero,
	}

	public partial class ClothesViewModel : ViewModelBase
    {
	    [AutoNotify] private ObservableList<ClothesTopItemViewModel> scrollViewTopList = new();
	    [AutoNotify] private ObservableList<AttrItemViewModel> scrollViewAttrList = new();
		[AutoNotify] private string btnSortPath;
		[AutoNotify] private string sortTitleNameStr;
		[AutoNotify] private bool isShowScrollView;
		[AutoNotify] private ObservableList<ClothesItemViewModel> scrollViewList = new();
		[AutoNotify] private int sortType = 0; //0 按照品质排序  1按照等级排序  2 按照部位排序
		[AutoNotify] private TabBtnGroupViewModel tabBtnViewModel;
		[AutoNotify] private RoleViewModel roleVm;
		[AutoNotify] private bool mergeRed;
		[AutoNotify] private int quality;
		public RectTransform StartNode;
		public GameObject MoveParent;
		[AutoNotify] public string noneTipsStr;
		public ClothesManager Manager => ClothesManager.Instance;
		#region 初始化英雄立绘
		[AutoNotify] private bool isShowChapterEffectNode;
		private readonly string effectPath = "UI/Role/MapEffect/";
		private SkeletonGraphic curSpine;
		private GameObject effectParentNode;
		public GameObject EffectParentNode
		{
			get=> null;
			set
			{
				effectParentNode = value;
				if (effectParentNode != null)
				{
					UpdateChapterMapEffect();
				}
			}
		}
		private async UniTaskVoid UpdateChapterMapEffect()
		{
			if (effectParentNode == null)return;
			
			for (int i = 0; i < effectParentNode.transform.childCount; i++)
			{
				var child = effectParentNode.transform.GetChild(i);
				AssetsManager.ReleaseInstance(child.gameObject);
			}
			curSpine = null;
			var path = $"{effectPath}{DataCenter.roleData.GetNowHero().Model}";
			var effectNode= await AssetsManager.InstantiateWithParentAsync(path, effectParentNode.transform, false);
			curSpine = effectNode.GetComponent<SkeletonGraphic>();
			if (curSpine == null) return;
			IsShowChapterEffectNode = true;
			//curSpine.AnimationState.SetAnimation(0, GameConst.MapAniName.Idle, false);
			var data = DataCenter.clothesData.GetHeroEquipDataByPart(1);
			UIHelper.InitHeroEquipBox(effectNode.transform,data?.Id ?? 0);
			var boxTr = effectNode.transform.Find("Box");
			if (boxTr != null)
			{
				boxTr.transform.localScale = Vector3.one/1.37f;
			}
			//皮皮鬼特殊处理
			var isRole3000 = DataCenter.roleData.GetNowHero().Id == 3000;
			effectParentNode.transform.localPosition = new Vector3(isRole3000 ? 0 : -70, -309, 0);
		}
		#endregion


        [Preserve]
        public ClothesViewModel()
        {
	        RefreshSwitch();
	        InitTopInfo();
	        RefreshList();
	        RefreshItemList();

	        RefreshSortName();
	        RefreshAttr();
	        RefreshMergeRed();
	        RoleVm = new RoleViewModel();
	        DataCenter.itemsData.OnItemUpdate += ItemUpdate;
	        DataCenter.clothesData.Wear.CollectionChanged += ClothesItemUpdate;
	        DataCenter.clothesData.PropertyChanged += ClothesPropertyChanged;
	        DataCenter.roleData.PropertyChanged += RoleDataPropertyChanged;
	        ClothesManager.Instance.PropertyChanged += ClothesManagerPropertyChanged;

        }

        protected override void OnDispose()
        {
	        base.OnDispose();
	        RoleVm.Dispose();
	        DataCenter.itemsData.OnItemUpdate -= ItemUpdate;
	        // DataCenter.clothesData.Items.CollectionChanged -= ClothesItemUpdate;
	        DataCenter.clothesData.Wear.CollectionChanged -= ClothesItemUpdate;
	        DataCenter.clothesData.PropertyChanged -= ClothesPropertyChanged;
	        DataCenter.roleData.PropertyChanged -= RoleDataPropertyChanged;
	        ClothesManager.Instance.IsMergeSelect = false;
	        ClothesManager.Instance.PropertyChanged -= ClothesManagerPropertyChanged;
	        foreach (var item in ScrollViewList)
	        {
		        item.Dispose();
	        }
        }

        private void ClothesManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(ClothesManager.UseClothesPart))
	        {
		        StartFlyPlay(ClothesManager.Instance.UseClothesPart).Forget();
	        }
        }

        private void ClothesPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(ClothesData.Items))
	        {
		        RefreshList();
		        RefreshItemList();
		        RefreshMergeRed();
		        RefreshAttr();
	        }

        }
        private void RoleDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
	        RefreshMergeRed();
	        if (e.PropertyName == nameof( DataCenter.roleData.FmtHero))
	        {
		        UpdateChapterMapEffect().Forget();
		        Quality = DataCenter.roleData.GetNowHero().Qlt;
	        }
	        if (e.PropertyName is nameof( DataCenter.roleData.HeroLevelUpId) or
	            nameof( DataCenter.roleData.HeroStarUpId) or nameof( DataCenter.roleData.FmtHero))
	        {
		        RefreshAttr();
	        }
        }
        
        private void ClothesItemUpdate(object sender, NotifyCollectionChangedEventArgs e)
        {
	        RefreshMergeRed();
	        RefreshList();
	        RefreshItemList();
	        RefreshAttr();
	        UpdateChapterMapEffect().Forget();
        }

        private void ItemUpdate(ResourceData data)
        {
	        RefreshItemList();
	        RefreshLock();
	        RefreshMergeRed();
        }

        private void InitTopInfo()
        {
	        Quality = DataCenter.roleData.GetNowHero().Qlt;
	        ScrollViewTopList.ClearAndDispose();
	        var patCfgList = ConfigCenter.HeroEquipPartCfgColl.DataItems.ToList();
	        UIHelper.SortList(patCfgList,(itemA,itemB)=> itemA.SeqId>itemB.SeqId );
	        foreach (var item in patCfgList)
	        {
		        var model = new ClothesTopItemViewModel(item.Id, (id) =>
		        {
			        ClickItemAction(id);
		        });
		        model.RedRefreshAction = () =>
		        {
			        RefreshTitleRed();
		        };
		        ScrollViewTopList.Add(model);
	        }
        }

        private void RefreshList()
        {
	        for (int i = ScrollViewList.Count-1; i >=0 ; i--)
	        {
		        if (ScrollViewList[i].IsHeroEquip)
		        {
			        ScrollViewList[i].Dispose();
			        ScrollViewList.RemoveAt(i);
		        }
	        }
	        var unUseList = DataCenter.clothesData.GetUnWearList();
	        UIHelper.SortList(unUseList, (itemA, itemB) => SortValue(itemB) > SortValue(itemA));
	        Dictionary<int,List<HeroEquipData>> foldDic = new Dictionary<int,List<HeroEquipData>>();
	        for (int i = 0; i < unUseList.Count; i++)
	        {
		        if (i % 5 == 0)
		        {
			        var list = new List<HeroEquipData> { unUseList[i] };
			        foldDic.Add(i/5,list);
		        }
		        else
		        {
			        foldDic[i/5].Add(unUseList[i]);
		        }
	        }

	        foreach (var item in foldDic)
	        {
		        var model = new ClothesItemViewModel(item.Value, ClickItemAction);
		        model.RedRefreshAction = () =>
		        {
					RefreshTitleRed();
		        };
		        ScrollViewList.Add(model);
	        }
	        RefreshNoneTips();
	        RefreshTitleRed();
        }
        
        private void RefreshItemList()
        {
	        for (int i = ScrollViewList.Count-1; i >=0 ; i--)
	        {
		        if (!ScrollViewList[i].IsHeroEquip)
		        {
			        ScrollViewList[i].Dispose();
			        ScrollViewList.RemoveAt(i);
		        }
	        }
	        
	        var itemList = ItemManager.Instance.GetRewardsByType((int)GameConst.ItemType.HeroEquipUpLevel,true);
	        var itemList1 = ItemManager.Instance.GetRewardsByType((int)GameConst.ItemType.HeroEquipMerge,true);
	        itemList.AddRange(itemList1);
	        UIHelper.SortList(itemList, (itemA, itemB) => SortValueItem(itemB) > SortValueItem(itemA));
	        Dictionary<int,List<Reward>> rewardDic = new Dictionary<int,List<Reward>>();
	        for (int i = 0; i < itemList.Count; i++)
	        {
		        if (i % 5 == 0)
		        {
			        var list = new List<Reward> { itemList[i] };
			        rewardDic.Add(i/5,list);
		        }
		        else
		        {
			        rewardDic[i/5].Add(itemList[i]);
		        }
	        }
	        

	        bool isShow = true;
	        foreach (var item in rewardDic)
	        {
		        ScrollViewList.Add(new ClothesItemViewModel(item.Value,isShow?LocalizeHelper.GetGlobal(GlobalLanguageId.heroEquip_Tips_29):null));
		        isShow = false;
	        }

	        RefreshNoneTips();
        }

        private void RefreshNoneTips()
        {
	        NoneTipsStr = ScrollViewList.Count == 0 ? LocalizeHelper.GetGlobal(GlobalLanguageId.heroEquip_Tips_31) : string.Empty;
        }

        private int SortValueItem(Reward reward)
        {
	        int sortValue = 0;
	        var cfg = ConfigCenter.ItemCfgColl.GetDataById(reward.Id);
	        if (cfg != null)
	        {
		        sortValue += (100 - cfg.Type)* 1000000;
		        sortValue += cfg.Quality * 100000;
		        sortValue += cfg.TypeValue1 * 1000;
		        sortValue += (10 - cfg.TypeValue) * 100;
		        sortValue += cfg.Id;
	        }
	        return sortValue;
        }

        //0 按照品质排序  1按照等级排序  2 按照部位排序
        public int SortValue(HeroEquipData data)
        {
	        int sortValue = 0;
	        if (data == null) return sortValue;
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
	        return sortValue;
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

        private void RefreshSwitch()
        {
	        var btnInfos = new List<TabBtnInfo>();
	        var btn1 = new TabBtnInfo()
	        {
		        Pos = (int)ClothesUI.HeroEquip,
		        Name = MainUiManager.Instance.GetFunctionName(EFunctionOpenType.FunctionClothes),
		        LockDesc = MainUiManager.Instance.GetFunctionUnLockTips(EFunctionOpenType.FunctionClothes)
	        };
	        btnInfos.Add(btn1);
	        var btn2 = new TabBtnInfo()
	        {
		        Pos = (int)ClothesUI.Hero,
		        Name =  MainUiManager.Instance.GetFunctionName(EFunctionOpenType.FunctionRole),
		        LockDesc = MainUiManager.Instance.GetFunctionUnLockTips(EFunctionOpenType.FunctionRole)
	        };
	        btnInfos.Add(btn2);
	        var select = ClothesUI.HeroEquip;
	        if (!MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionClothes))
	        {
		        select = ClothesUI.Hero;
	        }
	        else
	        {
		        if (!HeroEquipRed() && DataCenter.roleData.AllHeroRed())
		        {
			        select = ClothesUI.Hero;
		        }
	        }

	        tabBtnViewModel = new TabBtnGroupViewModel(btnInfos, (int)select,ClickSelect);
	        RefreshLock();
        }

        private bool HeroEquipRed()
        {
	        ClothesManager.Instance.ClothesRed = MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionClothes) && (ClothesManager.Instance.IsCanMerge() || CheckItemListRed() || CheckTopItemUpRed());
	        return ClothesManager.Instance.ClothesRed;
        }

        private void RefreshLock()
        {
	        TabBtnViewModel.RefreshLockState((int)ClothesUI.HeroEquip,!MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionClothes));
	        TabBtnViewModel.RefreshLockState((int)ClothesUI.Hero,!MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionRole));
        }
        
        private void RefreshTitleRed()
        {
	        TabBtnViewModel.RefreshRedDot((int)ClothesUI.HeroEquip,HeroEquipRed());
	        TabBtnViewModel.RefreshRedDot((int)ClothesUI.Hero,DataCenter.roleData.AllHeroRed());
        }

        private void RefreshAttr()
        {
	        Dictionary<string, float> lastDic = new();
	        foreach (var item in ScrollViewAttrList)
	        {
		        lastDic.Add(item.TypeName,item.Value);
	        }
	        
	        ScrollViewAttrList.ClearAndDispose();
	        var list = ClothesManager.Instance.GetClothesAllAttr();
	        foreach (var item in list)
	        {
		        var model = new AttrItemViewModel(item.Key, item.Value);
		        ScrollViewAttrList.Add(model);
		        if (lastDic.TryGetValue(item.Key, out var value))
		        {
			        if (model.Value > value)
			        {
				        PlayEffect(model).Forget();
			        }
		        }
	        }
        }

        private async UniTaskVoid PlayEffect(AttrItemViewModel model)
        {
	        await UniTask.Delay(500);
	        model.PlayEffect();
        }

        private void RefreshMergeRed()
        {
	        MergeRed = ClothesManager.Instance.IsCanMerge();
	        RefreshTitleRed();
        }

        private bool CheckItemListRed()
        {
	        foreach (var item in scrollViewList)
	        {
		        foreach (var info in item.GridDictionary)
		        {
			        if (info.Value.IsRedDot)
			        {
				        return true;
			        }
		        }
	        }

	        return false;
        }
        
        private bool CheckTopItemUpRed()
        {
	        foreach (var item in scrollViewTopList)
	        {
		        if (item.IsUpTips)
		        {
			        return true;
		        }
	        }
	        return false;
        }


        private void ClickSelect(int index)
        {
	        ClothesManager.Instance.CurTabType = (ClothesUI)index;
        }
        
        private void ClickItemAction(int part,RectTransform moveStart=null)
        {
	        var heroEquipData = DataCenter.clothesData.GetHeroEquipDataByPart(part);
	        if(heroEquipData==null || heroEquipData.IsNull() || heroEquipData.Uid==0) return;
	        UIManager.Instance.OpenDialog<ClothesInfoView>(new ClothesInfoViewModel(heroEquipData.Uid,true)).Forget();
        }

        private void ClickItemAction(HeroEquipData data,RectTransform rect=null)
        {
	        if(data.IsNull() || data.Uid==0) return;
	        DataCenter.clothesData.DelNewRed(data.Uid);
	        StartNode = rect;
	        UIManager.Instance.OpenDialog<ClothesInfoView>(new ClothesInfoViewModel(data.Uid,true)).Forget();
        }

        private async UniTaskVoid StartFlyPlay(int part)
        {
	        if(StartNode==null) return;
	        var endModel = ScrollViewTopList.Find(model => model.Part == part);
	        if (endModel == null || endModel.RectNode==null) return;
	        var heroEquipData = DataCenter.clothesData.GetHeroEquipDataByPart(part);
	        if(heroEquipData==null) return;
	        var resource = new Resource()
	        {
		        Id = heroEquipData.Id,
		        Count = 1,
		        Type = (int)RewardType.HeroEquip,
	        };
			
	        var BaseData = new ResourceData(resource);

	        await UniTask.Delay(100);
	        UIHelper.FlyChip(MoveParent,UIHelper.GetRewardsIconPath(BaseData),StartNode.position,endModel.RectNode.position,
		        () =>
		        {
			        
		        }).Forget();
	        
        }

        [Command]
        private void OnClickBtnTips()
        {
	        //点击提示信息
        }

        [Command]
        private void OnClickBtnMerge()
        {
	        UIManager.Instance.OpenDialog<ClothesMergeView>(new ClothesMergeViewModel()).Forget();
        }
        
        [Command]
        private void OnClickBtnSort()
        {
	        if (sortType >= 2)
	        {
		        SortType = 0;
	        }
	        else
	        {
		        SortType += 1;
	        }
	        RefreshList();
	        RefreshItemList();
	        RefreshSortName();
        }
        
    }
}