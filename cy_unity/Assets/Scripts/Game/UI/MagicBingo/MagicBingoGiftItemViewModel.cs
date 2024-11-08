using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class MagicBingoGiftItemViewModel : ViewModelBase
    {
		public override bool AutoDispose => true;
	    [AutoNotify] private int packageId;
	    [AutoNotify] private PackageCfg packageCfg;
		[AutoNotify] private string limitNumStr;
		public Func<object, object> GetItemGridCellCallback => GetItemGridCellCallbackByIndex;
		[AutoNotify] private ObservableDictionary<int,SelectCellItemViewModel> itemGridDictionary = new();
		[AutoNotify] private string titleNameStr;
		[AutoNotify] private BtnPriceNodeModel btnPriceNodeVm;
		[AutoNotify] private ShopBuyState state;
		private MagicBingoData Data => DataCenter.mgicBingoData;
        [Preserve]
        public MagicBingoGiftItemViewModel(int id)
        {
	        PackageId = id;
	        PackageCfg = ConfigCenter.PackageCfgColl.GetDataById(packageId); 
	        TitleNameStr = ShopManager.Instance.GetPackageName(id);
	        BtnPriceNodeVm = new BtnPriceNodeModel(PackageId);
	        InitReward();
	        RefreshBuyState();
	        DataCenter.mgicBingoData.OptionalRecord.CollectionChanged += PackageSelectIndexDicChanged;
	        DataCenter.mgicBingoData.PropertyChanged += BuyGiftNumsChange;
        }

        protected override void OnDispose()
        {
	        base.OnDispose();
	        DataCenter.mgicBingoData.OptionalRecord.CollectionChanged -= PackageSelectIndexDicChanged;
	        DataCenter.mgicBingoData.PropertyChanged -= BuyGiftNumsChange;
        }
        
        private void PackageSelectIndexDicChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
	        RefreshSelectReward();
        }
        
        private void BuyGiftNumsChange(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(DataCenter.mgicBingoData.PackageRecord))
	        {
		        RefreshBuyState();
	        }
        }

        private void InitReward()
        {
	        int pos = 0;
	        if (packageCfg != null && packageCfg.Reward != null)
	        {
		        for (int i = 0; i < PackageCfg.Reward.Count; i++)
		        {
			        var model = SelectCellItemViewModel.Create(packageCfg.Reward[i], ECellItemSizeType.Size120X100);
			        model.CellItemBaseViewVm.IsOpenMask = true;
			        itemGridDictionary.Add(i,model);
			        pos++;
		        }
	        }
	        
	        if (packageCfg.OptionalReward is { Count: > 0 })
	        {
		        var selectItemViewModel = new SelectCellItemViewModel(packageCfg.OptionalReward,DataCenter.mgicBingoData.GetOptionalSelectIndex(packageCfg.Id,2),ECellItemSizeType.Size120X100);
		        selectItemViewModel.ClickEvent = OnClickSelectReward;
		        selectItemViewModel.CellItemBaseViewVm.OnClickEvent = (info) =>
		        {
			        OnClickSelectReward();
		        };
		        itemGridDictionary.Add(pos,selectItemViewModel);
	        }
        }

        private void RefreshSelectReward()
        {
	        foreach (var item in itemGridDictionary)
	        {
		        if (item.Value.IsSelectType)
		        {
			        item.Value.MergeSelect(DataCenter.mgicBingoData.GetOptionalSelectIndex(packageCfg.Id,2));
		        }
	        }
        }
        
        private void RefreshSelectRewardState()
        {
	        foreach (var item in itemGridDictionary)
	        {
		        if (item.Value.IsSelectType)
		        {
			        item.Value.CellItemBaseViewVm.OnClickEvent = state == ShopBuyState.Finish ? null : (info) => { OnClickSelectReward(); };
		        }
	        }
        }

        private void OnClickSelectReward()
        {
	        if(State==ShopBuyState.Finish) return;
	        if(packageCfg==null || packageCfg.OptionalReward==null || packageCfg.OptionalReward.Count==0) return;
	        UIManager.Instance.OpenDialog<CommonSelectRewardView>(new CommonSelectRewardViewModel(packageCfg.OptionalReward,DataCenter.mgicBingoData.GetOptionalSelectIndex(packageCfg.Id,2),(selectIndex)=>
	        {
		        if (selectIndex >= 0)
		        {
			        DataCenter.mgicBingoData.SetOptionalSelectIndex(packageCfg.Id,selectIndex,2);
		        }
	        })).Forget();
        }

        private object GetItemGridCellCallbackByIndex(object index)
		{
			if (itemGridDictionary.TryGetValue((int)index, out SelectCellItemViewModel ret))
			{
				return ret;
			}

			return null;
		}

		private void RefreshBuyState()
		{
			var buyNum = DataCenter.mgicBingoData.GetGiftNum(packageCfg.Id);
			var isLimit = packageCfg.BuyLimit == 0;
			LimitNumStr = isLimit ? "" : LocalizeHelper.GetGlobal(GlobalLanguageId.Shop_tips18, Math.Max(0, packageCfg.BuyLimit - DataCenter.mgicBingoData.GetGiftNum(packageCfg.Id)),packageCfg.BuyLimit);
			
			if (isLimit || buyNum < packageCfg.BuyLimit)
			{
				State = ShopManager.Instance.GetPackageState(packageCfg.Id, buyNum);
			}
			else
			{
				State = ShopBuyState.Finish;
			}

			RefreshSelectRewardState();
		}

		[Command]
		private void OnClickBtnRecharge()
		{
			if (Data.IsExchangeTime)
			{
				ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.niudan17));
				return;
			}
			
			if (packageCfg.OptionalReward is { Count: > 0 })
			{
				if (!DataCenter.mgicBingoData.IsSelectOptionalReward(packageCfg.Id,2))
				{
					ToastManager.ShowLanguage(GlobalLanguageId.Trigger08);
					return;
				}
			}
			
			ShopManager.Instance.SendBuyPackageBuyRecharge(packageCfg.Id, (mPackageId) =>
			{
				DataCenter.mgicBingoData.BuyGift(mPackageId);
			},DataCenter.mgicBingoData.GetGiftNum(packageCfg.Id),DataCenter.mgicBingoData.GetOptionalSelectIndex(packageCfg.Id,2));
		}
    }
}