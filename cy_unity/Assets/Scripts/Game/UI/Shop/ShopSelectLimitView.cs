using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using Extend;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class ShopSelectLimitView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhButton btnClose;
		public DhText name;
		public CellItemBaseView cellItemView;
		public DhButton btnDel;
		public DhButton btnAdd;
		public DhText limitValue;
		public DhText limitDescValue;
		public DhButton btnConfirm;
		public ItemPriceNodeView itemPriceNodeView;

		public Slider slider;
		public DhText buyDesc;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
            await base.Create();
            var bindingSet = this.CreateBindingSet<ShopSelectLimitView, ShopSelectLimitViewModel>();
			bindingSet.Bind(btnClose).For(v => v.onClick).To(vm => vm.OnClickBtnCloseCommand);
			bindingSet.Bind(name).For(v => v.text).To(vm => vm.NameStr);
			bindingSet.Bind(cellItemView.BindingContext).For(v => v.DataContext).To(vm => vm.CellItemViewVm);
			bindingSet.Bind(btnDel).For(v => v.onClick).To(vm => vm.OnClickBtnDelCommand);
			bindingSet.Bind(btnAdd).For(v => v.onClick).To(vm => vm.OnClickBtnAddCommand);
			bindingSet.Bind(limitValue).For(v => v.text).To(vm => vm.SelectNum);
			bindingSet.Bind(limitDescValue).For(v => v.text).To(vm => vm.LimitDescValueStr);
			bindingSet.Bind(btnConfirm).For(v => v.onClick).To(vm => vm.OnClickBtnConfirmCommand);
			bindingSet.Bind(itemPriceNodeView.BindingContext).For(v => v.DataContext).To(vm => vm.ItemPriceNodeModel);
			bindingSet.Bind(btnDel.GetComponent<DhImage>()).For(v => v.sprite).ToExpression(vm => GetBtnImagePathDel(!vm.IsMinPos)).WithConversion(this);
			bindingSet.Bind(btnAdd.GetComponent<DhImage>()).For(v => v.sprite).ToExpression(vm => GetBtnImagePathAdd(!vm.IsMaxPos)).WithConversion(this);
			
			bindingSet.Bind(this).For(v => v.slider).To(vm => vm.Slider).OneWayToSource();
			bindingSet.Bind(buyDesc).For(v => v.text).To(vm => vm.BuyDesc);
            bindingSet.Build();
        }
        
        private string GetBtnImagePathDel(bool isOpen)
        {
	        return isOpen ? "gacha[gacha_btn_2]" : "gacha[gacha_btn_5]";
        }
        
        private string GetBtnImagePathAdd(bool isOpen)
        {
	        return isOpen ? "gacha[gacha_btn_1]" : "gacha[gacha_btn_4]";
        }
    }
}