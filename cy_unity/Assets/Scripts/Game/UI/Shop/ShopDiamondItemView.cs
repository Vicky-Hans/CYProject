using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class ShopDiamondItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhImage icon;
		public DhText cntValue;
		public GameObject disBg;
		public DhText disValue;
		public DhButton btnBuy;
		public BtnPriceNode btnPriceNode;
		public ItemPriceNodeView itemPriceNode;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<ShopDiamondItemView, ShopDiamondItemViewModel>();
            
			bindingSet.Bind(icon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);
			bindingSet.Bind(cntValue).For(v => v.text).To(vm => vm.CntValueStr);
			bindingSet.Bind(disBg).For(v => v.activeSelf).To(vm => vm.IsShowDiscount);
			bindingSet.Bind(disValue).For(v => v.text).To(vm => vm.LimitCntDesc);
			bindingSet.Bind(btnBuy).For(v => v.onClick).To(vm => vm.OnClickBtnBuyCommand);
			bindingSet.Bind(btnPriceNode.BindingContext).For(v => v.DataContext).To(vm => vm.BtnPriceNodeModel);
			bindingSet.Bind(itemPriceNode.BindingContext).For(v => v.DataContext).To(vm => vm.ItemPriceNodeModel);
            bindingSet.Build();
        }
    }
}