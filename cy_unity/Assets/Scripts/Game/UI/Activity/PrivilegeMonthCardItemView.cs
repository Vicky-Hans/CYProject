using System;
using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class PrivilegeMonthCardItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public UICircularScrollView effectDes;
		[AssetPath]public string effectDesCell;
		public DhText timeDes;
		public UICircularScrollView awardScrollview;
		[AssetPath]public string awardScrollviewCell;
		public DhButton buyButton;
		public DhText notInForce;

		public BtnPriceNode priceNode;
		
		public CellItemBaseView atOnceAward;
	    public DhText noAdsDesText;
	    public DhButton GetDayAwardBtn;
	    public DhText titleText;
	    public DhButton noAdsDesBut;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			effectDes.PrefabPath = effectDesCell;
			awardScrollview.PrefabPath = awardScrollviewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<PrivilegeMonthCardItemView, PrivilegeMonthCardItemViewModel>();
            bindingSet.Bind(titleText).For(v => v.text).To(vm => vm.TitleText);
            bindingSet.Bind(effectDes).For(v => v.Collection).To(vm => vm.EffectDesList);
			bindingSet.Bind(timeDes).For(v => v.text).To(vm => vm.TimeDesStr);
			bindingSet.Bind(awardScrollview).For(v => v.Collection).To(vm => vm.AwardScrollviewList);
			bindingSet.Bind(buyButton).For(v => v.onClick).To(vm => vm.OnClickBuyButtonCommand);
			bindingSet.Bind(notInForce.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowNotInForceStr);
			bindingSet.Bind(atOnceAward.BindingContext).For(v => v.DataContext).To(vm => vm.AtOnceAwardCellVm);
			bindingSet.Bind(noAdsDesText).For(v => v.text).To(vm => vm.NoAdsDesText);
			bindingSet.Bind(priceNode.BindingContext).For(v => v.DataContext).To(vm => vm.PriceNodeModel);
			bindingSet.Bind(GetDayAwardBtn).For(v => v.onClick).To(vm => vm.GetDayAwardBtnCommand);
			bindingSet.Bind(GetDayAwardBtn.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowGetAwardBut);
			bindingSet.Bind(noAdsDesBut).For(v => v.onClick).To(vm => vm.OnClickIconBtn)
				.CommandParameter(() => new Tuple<Vector3, Vector3>(noAdsDesBut.transform.position, new Vector3(0,20,0)));
            bindingSet.Build();
        }
    }
}