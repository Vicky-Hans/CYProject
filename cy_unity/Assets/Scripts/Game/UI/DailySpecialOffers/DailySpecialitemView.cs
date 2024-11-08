using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class DailySpecialitemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public UICircularScrollView awardScrollview;
		[AssetPath]public string awardScrollviewCell;
		public DhImage dimIcon;
		public DhText nums;
		public DhButton buyBut;
		public DhText discountText;
		public BtnPriceNode buyPriceNode;
		public CellItemBaseView cellItemBaseView;
		public GameObject buyOverDes;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			awardScrollview.PrefabPath = awardScrollviewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<DailySpecialitemView, DailySpecialitemViewModel>();
			bindingSet.Bind(awardScrollview).For(v => v.Collection).To(vm => vm.AwardScrollviewList);
			bindingSet.Bind(dimIcon).For(v => v.sprite).To(vm => vm.DimIconPath).WithConversion(this);
			bindingSet.Bind(nums).For(v => v.text).To(vm => vm.NumsStr);
			bindingSet.Bind(buyBut).For(v => v.onClick).To(vm => vm.OnClickBuyButCommand);
			bindingSet.Bind(discountText).For(v => v.text).To(vm => vm.DiscountTextStr);
			bindingSet.Bind(buyPriceNode.BindingContext).For(v => v.DataContext).To(vm => vm.BtnPriceNode);
			bindingSet.Bind(cellItemBaseView.BindingContext).For(v => v.DataContext).To(vm => vm.EquipItemVm);
			bindingSet.Bind(buyOverDes).For(v => v.activeSelf).ToExpression(vm => vm.IsBuyOver);
			bindingSet.Bind(buyBut.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsBuyOver);
            bindingSet.Build();
        }
    }
}