using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class ShopDailyItemView : BaseItemView
    {
        public override bool FullScreen => false;

        public DhImage bg;
        public CellItemBaseView cellItemView;
		public DhText name;
		public DhText limitValue;
		public DhButton btnAd;
		public DhButton btnFree;
		public DhButton btnUseItem;
		public ItemPriceNodeView itemPriceNodeView;
		public GameObject discountNode;
		public DhText discountNum;
		public GameObject finishNode;
		public CommonAdvIconView commonAdv;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<ShopDailyItemView, ShopDailyItemViewModel>();
            bindingSet.Bind(bg).For(v => v.sprite).To(vm => vm.BaPath).WithConversion(this);
            bindingSet.Bind(cellItemView.BindingContext).For(v => v.DataContext).To(vm => vm.CellItemBaseViewModel);
            bindingSet.Bind(name).For(v => v.text).To(vm => vm.NameStr);
            bindingSet.Bind(name).For(v => v.color).To(vm => vm.NameColor);
            bindingSet.Bind(limitValue).For(v => v.text).To(vm => vm.LimitValueStr);
            bindingSet.Bind(discountNode).For(v => v.activeSelf).To(vm => vm.IsShowDis);
            bindingSet.Bind(discountNum).For(v => v.text).To(vm => vm.DiscountStr);
			bindingSet.Bind(btnAd).For(v => v.onClick).To(vm => vm.OnClickBtnAdCommand);
			bindingSet.Bind(btnFree).For(v => v.onClick).To(vm => vm.OnClickBtnFreeCommand);
			bindingSet.Bind(btnUseItem).For(v => v.onClick).To(vm => vm.OnClickBtnUseItemCommand);
			
			bindingSet.Bind(btnAd.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsFinish && vm.ShopBuyState==ShopBuyState.Adv);
			bindingSet.Bind(btnFree.gameObject).For(v => v.activeSelf).ToExpression(vm =>!vm.IsFinish && vm.ShopBuyState==ShopBuyState.Free);
			bindingSet.Bind(btnUseItem.gameObject).For(v => v.activeSelf).ToExpression(vm =>!vm.IsFinish && vm.ShopBuyState==ShopBuyState.Item);
			bindingSet.Bind(itemPriceNodeView.BindingContext).For(v => v.DataContext).To(vm => vm.ItemPriceViewModel);
			bindingSet.Bind(finishNode).For(v => v.activeSelf).To(vm => vm.IsFinish);
			bindingSet.Bind(commonAdv.BindingContext).For(v => v.DataContext).To(vm => vm.CommonAdvVm);
            bindingSet.Build();
        }
    }
}