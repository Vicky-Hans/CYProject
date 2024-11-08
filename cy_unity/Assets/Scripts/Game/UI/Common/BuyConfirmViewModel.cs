using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using System.Collections.Generic;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;


namespace DH.Game.ViewModels
{
    public partial class BuyConfirmViewModel : ViewModelBase
    {
	    public override bool AutoDispose => true;
	    [AutoNotify] private string descStr;
		[AutoNotify] private ItemPriceNodeModel itemPriceNodeVm;
		[AutoNotify] private BtnPriceNodeModel btnPriceNodeVm;
		[AutoNotify] private CommonTopViewModel commonTopItemsVm;
		
		public Action ItemBuyAction;
		public Action MoneyBuyAction;
        [Preserve]
        public BuyConfirmViewModel(int packageId,Action itemAction,Action moneyAction)
        {
	        List<int> list = new List<int>() {(int)GameConst.ItemIdCode.Yuan};
	        CommonTopItemsVm = new CommonTopViewModel(list);
	        
	        ItemBuyAction = itemAction;
	        MoneyBuyAction = moneyAction;
	        var cfg = ConfigCenter.PackageCfgColl.GetDataById(packageId);
	        if (cfg != null)
	        {
		        BtnPriceNodeVm = new BtnPriceNodeModel(packageId);
		        if (cfg.Cost is { Count: >= 1 })
		        {
			        ItemPriceNodeVm = new ItemPriceNodeModel(cfg.Cost[0]);
		        }
	        }
        }


        [Command]
        private void OnClickBtnClose()
        {
	        UIManager.Instance.CloseDialog<BuyConfirmView>();
        }

        [Command]
        private void OnClickBtnItem()
        {
	        UIManager.Instance.CloseDialog<BuyConfirmView>();
	        ItemBuyAction?.Invoke();
        }

        [Command]
        private void OnClickBtnMoney()
        {
	        UIManager.Instance.CloseDialog<BuyConfirmView>();
	        MoneyBuyAction?.Invoke();
        }

        
    }
}