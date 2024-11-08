using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class ShopBoxItemView : BaseItemView
    {
        public override bool FullScreen => false;

        public DhImage bg;
        public DhImage bg1;
		public DhImage icon;
		public DhText name;
		public DhButton btnRuleTips;
		public DhText tipsDesc;
		public DhButton btnAd;
		public DhButton btnFree;
		public DhButton btnUseItem;
		public DhButton btnClickIcon;
		public DhButton btnAdNot;
		public DhButton btnFreeNot;
		public ItemPriceNodeView itemPriceNodeView;
		public DhImage freeIcon;
		public CommonAdvIconView commonAdv;

		public GameObject tipsNode;
		public GameObject tipsNode1;
		public DhText tipsNode1Desc1;
		public DhButton btnItemOne;
		public DhButton btnItemTen;
		
		public DhText btnItemOneCnt;
		public DhText btnItemTenCnt;
		public ItemPriceNodeView itemPriceNodeViewOne;
		public ItemPriceNodeView itemPriceNodeViewTen;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<ShopBoxItemView, ShopBoxItemViewModel>();
            //bindingSet.Bind(bg).For(v => v.sprite).To(vm => vm.BgPath).WithConversion(this);
            //bindingSet.Bind(bg1).For(v => v.sprite).To(vm => vm.BgPath1).WithConversion(this);
			bindingSet.Bind(icon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);
			bindingSet.Bind(name).For(v => v.text).To(vm => vm.NameStr);
			bindingSet.Bind(btnRuleTips).For(v => v.onClick).To(vm => vm.OnClickBtnRuleTipsCommand);
			bindingSet.Bind(btnClickIcon).For(v => v.onClick).To(vm => vm.OnClickBtnRuleTipsCommand);
			bindingSet.Bind(tipsDesc).For(v => v.text).To(vm => vm.TipsDescStr);
			bindingSet.Bind(btnAd).For(v => v.onClick).To(vm => vm.OnClickBtnAdCommand);
			bindingSet.Bind(btnFree).For(v => v.onClick).To(vm => vm.OnClickBtnFreeCommand);
			bindingSet.Bind(btnUseItem).For(v => v.onClick).To(vm => vm.OnClickBtnUseItemCommand).CommandParameter(1);
			bindingSet.Bind(btnItemOne).For(v => v.onClick).To(vm => vm.OnClickBtnUseItemCommand).CommandParameter(1);
			bindingSet.Bind(btnItemTen).For(v => v.onClick).To(vm => vm.OnClickBtnUseItemCommand).CommandParameter(10);
			
			bindingSet.Bind(btnAd.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ShopBuyState==ShopBuyState.Adv && vm.ShowDrawType == DrawType.Base);
			bindingSet.Bind(btnFree.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ShopBuyState==ShopBuyState.Free && vm.ShowDrawType == DrawType.Base);
			bindingSet.Bind(btnAdNot.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ShopBuyState==ShopBuyState.NoneAdv && vm.ShowDrawType == DrawType.Base);
			bindingSet.Bind(btnFreeNot.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ShopBuyState==ShopBuyState.NoneFree && vm.ShowDrawType == DrawType.Base);
			
			bindingSet.Bind(btnUseItem.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ShowDrawType == DrawType.Base);
			bindingSet.Bind(btnItemOne.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ShowDrawType == DrawType.Clothes);
			bindingSet.Bind(btnItemTen.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ShowDrawType == DrawType.Clothes);
			
			bindingSet.Bind(btnItemOneCnt).For(v => v.text).To(vm => vm.BtnDrawOneCnt);
			bindingSet.Bind(btnItemTenCnt).For(v => v.text).To(vm => vm.BtnDrawTenCnt);
			
			bindingSet.Bind(itemPriceNodeView.BindingContext).For(v => v.DataContext).To(vm => vm.ItemPriceNodeModel);
			bindingSet.Bind(itemPriceNodeViewOne.BindingContext).For(v => v.DataContext).To(vm => vm.ItemPriceNodeModelOne);
			bindingSet.Bind(itemPriceNodeViewTen.BindingContext).For(v => v.DataContext).To(vm => vm.ItemPriceNodeModelTen);
			
			bindingSet.Bind(tipsNode1Desc1).For(v => v.text).To(vm => vm.TipsDescStr1);
			
			bindingSet.Bind(commonAdv.BindingContext).For(v => v.DataContext).To(vm => vm.CommonAdvVm);
			
            bindingSet.Build();

            freeIcon.Grag = true;
        }
    }
}