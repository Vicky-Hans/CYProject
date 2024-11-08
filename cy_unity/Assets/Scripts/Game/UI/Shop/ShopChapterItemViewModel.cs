using System.Collections.Specialized;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;

namespace DH.Game.ViewModels
{
    public partial class ShopChapterItemViewModel : ViewModelBase
    {
	    [AutoNotify] private PackageCfg cfg;
		[AutoNotify] private string nameStr;
		 [AutoNotify] private ObservableList<SelectCellItemViewModel> scrollViewItemList = new();
		[AutoNotify] private BtnPriceNodeModel btnPriceNodeVm;
		[AutoNotify] private BtnPriceNodeModel oldBtnPriceNodeVm;
		[AutoNotify] private string oldPriceStr;
		[AutoNotify] private string discountValueStr;
		[AutoNotify] private bool isLock;
        [Preserve]
        public ShopChapterItemViewModel(PackageCfg cfg)
        {
            Cfg = cfg;
            NameStr = ShopManager.Instance.GetPackageName(Cfg.Id); 
            IsLock = !DataCenter.mainStageData.IsPassChapter(Cfg.Condition);
            DiscountValueStr = ShopManager.Instance.GetPackageDiscountDesc(Cfg.Id);
	        InitRewardList();
	        InitPrice();
	        DataCenter.shopData.PackageRecord.CollectionChanged += PackageChanged;
        }

        protected override void OnDispose()
        {
	        base.OnDispose();
	        DataCenter.shopData.PackageRecord.CollectionChanged -= PackageChanged;
        }
        
        private void PackageChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
	        RefreshCellItem();
        }
        
        protected void RefreshCellItem()
        {
	        if (Cfg?.OptionalReward is { Count: > 0 })
	        {
		        foreach (var item in scrollViewItemList)
		        {
			        if (item.SelectType == SelectItemType.Select)
			        {
				        item.MergeSelect(DataCenter.shopData.GetOptionalSelectIndex(Cfg.Id));
			        }
		        }
	        }
        }

        private void InitRewardList()
        {
	        if (Cfg.Reward is { Count: > 0 })
	        {
		        for (int i = 0; i<Cfg.Reward.Count ; i++)
		        {
			        var reward = Cfg.Reward[i];
			        var selectItemViewModel = SelectCellItemViewModel.Create(reward);
			        scrollViewItemList.Add(selectItemViewModel);
		        }
	        }
	        
	        if (Cfg.OptionalReward is { Count: > 0 })
	        {
		        var selectItemViewModel = new SelectCellItemViewModel(Cfg.OptionalReward,DataCenter.shopData.GetOptionalSelectIndex(Cfg.Id));
		        selectItemViewModel.ClickEvent = OnClickSelectReward;
		        selectItemViewModel.CellItemBaseViewVm.OnClickEvent = (info) =>
		        {
			        OnClickSelectReward();
		        };
		        scrollViewItemList.Add(selectItemViewModel);
	        }
        }
        
        private void OnClickSelectReward()
        {
	        if(Cfg==null || Cfg.OptionalReward==null || Cfg.OptionalReward.Count==0) return;
	        UIManager.Instance.OpenDialog<CommonSelectRewardView>(new CommonSelectRewardViewModel(Cfg.OptionalReward,DataCenter.shopData.GetOptionalSelectIndex(Cfg.Id),(selectIndex)=>
	        {
		        if (selectIndex >= 0)
		        {
			        DataCenter.shopData.SetOptionalSelectIndex(Cfg.Id,selectIndex);
		        }
	        })).Forget();
        }

        private void InitPrice()
        {
	        btnPriceNodeVm = new BtnPriceNodeModel(cfg.Id);
	        //oldPriceStr = cfg.PriceUsStr;
	        //oldBtnPriceNodeVm = new BtnPriceNodeModel(cfg.)
        }

        [Command]
        private void OnClickBtnBuy()
        {
	        // ShopManager.Instance.SendShopPackageBuy(cfg.Id).Forget();
	        if (Cfg.OptionalReward is { Count: > 0 } && !DataCenter.shopData.IsSelectOptionalReward(Cfg.Id))
	        {
		        ToastManager.ShowLanguage(GlobalLanguageId.Trigger08);
		        return;
	        }

	        ShopManager.Instance.SendBuyPackageBuyRecharge(cfg.Id, (packId) =>
	        {
		        var packCfg = ConfigCenter.PackageCfgColl.GetDataById(packId);
		        if (packCfg != null)
		        {
			        DataCenter.shopData.SetChapterGift(packCfg.Id);
		        }
	        },1,DataCenter.shopData.GetOptionalSelectIndex(Cfg.Id));
        }
    }
}