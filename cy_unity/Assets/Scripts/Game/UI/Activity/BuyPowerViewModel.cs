using System.Collections.Generic;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class BuyPowerViewModel : ViewModelBase
    {
	    public BuyPowerItemViewModel BuyPowerItem1;
	    public BuyPowerItemViewModel BuyPowerItem2;
	    public BuyPowerItemViewModel BuyPowerItem3;
	    public CommonTopViewModel TopViewModel;
	    [AutoNotify] private RectTransform bgRect;
	    public CommonAdvIconViewModel CommonAdvVm;
        [Preserve]
        public BuyPowerViewModel()
        {
	        BuyPowerItem1 = new BuyPowerItemViewModel(5,OnClickItemOPBtn);
	        BuyPowerItem2 = new BuyPowerItemViewModel(15,OnClickItemOPBtn);
	        BuyPowerItem3 = new BuyPowerItemViewModel(20,OnClickItemOPBtn);
	        
	        List<int> list = new List<int>() {(int)GameConst.ItemIdCode.EnergyDrink, (int)GameConst.ItemIdCode.Money, (int)GameConst.ItemIdCode.Stone};
	        TopViewModel = new(list);
	        CommonAdvVm = new CommonAdvIconViewModel();
        }
        private void OnClickItemOPBtn()
        {
	        // TopViewModel.PlayEnergyFly();
	        if(bgRect == null) return;
	        UIEffectManager.Instance.PlayItemClaimedActionInShowTop((int)GameConst.ItemIdCode.EnergyDrink, bgRect, 10).Forget();
        }

        [Command]
        private void OnClickCloseBtn()
        {
	        UIManager.Instance.CloseDialog<BuyPowerView>();
        }

        protected override void OnDispose()
        {
	        base.OnDispose();
	        BuyPowerItem1.Dispose();
	        BuyPowerItem2.Dispose();
	        BuyPowerItem3.Dispose();
	        CommonAdvVm?.Dispose();
        }
    }
}