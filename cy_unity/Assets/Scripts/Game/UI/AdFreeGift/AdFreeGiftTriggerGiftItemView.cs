using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class AdFreeGiftTriggerGiftItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhText tipsDesc;
		public DhImage titleIcon;
		public ScrollRectExtend awardScrollview;
		[AssetPath]public string awardScrollviewCell;
		public DhButton btnBuy;
		public DhText discountValue;
		public BtnPriceNode priceNode;
		public GameObject discountBg;
		public DhText title;

		private bool showSelf;

		public bool ShowSelf
		{
			get => showSelf;
			set
			{
				showSelf = value;
				gameObject.SetActive(showSelf);
			}
		}

		public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
	        awardScrollview.PrefabPath = awardScrollviewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<AdFreeGiftTriggerGiftItemView, AdFreeGiftTriggerGiftItemViewModel>();
            
			bindingSet.Bind(tipsDesc).For(v => v.text).To(vm => vm.TipsDescStr);
			bindingSet.Bind(titleIcon).For(v => v.sprite).To(vm => vm.TitleIconPath).WithConversion(this);
			bindingSet.Bind(awardScrollview).For(v => v.Collection).To(vm => vm.ScrollViewItemList);
			bindingSet.Bind(btnBuy).For(v => v.onClick).To(vm => vm.OnClickBtnBuyCommand);
			bindingSet.Bind(discountValue).For(v => v.text).ToExpression(vm => vm.DiscountValueStr);
			bindingSet.Bind(discountBg).For(v => v.activeSelf).ToExpression(vm => !string.IsNullOrEmpty(vm.DiscountValueStr));
			bindingSet.Bind(priceNode.BindingContext).For(v => v.DataContext).To(vm => vm.BtnPriceNodeVm);
			bindingSet.Bind(title).For(v => v.text).To(vm => vm.TitleStr);
			bindingSet.Bind(this).For(v => v.ShowSelf).To(vm => vm.IsShowSelf);
            bindingSet.Build();
        }
    }
}