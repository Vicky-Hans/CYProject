using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class AutumnSpecialOfferItemView : BaseItemView
    {
		public DhText title;
		public DhText limitBuy;
		public UICircularScrollView awardScrollview;
		[AssetPath]public string awardScrollviewCell;
		public DhButton buyBtn;
		public DhButton advBtn;
		public CommonAdvIconView commonAdvIconView;
		public BtnPriceNode btnPriceNode;
		public DhButton buyOverBtn;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			awardScrollview.PrefabPath = awardScrollviewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<AutumnSpecialOfferItemView, AutumnSpecialOfferItemViewModel>();
            
			bindingSet.Bind(title).For(v => v.text).To(vm => vm.TitleStr);
			bindingSet.Bind(limitBuy).For(v => v.text).To(vm => vm.LimitBuyStr);
			bindingSet.Bind(awardScrollview).For(v => v.Collection).To(vm => vm.AwardScrollviewList);
			bindingSet.Bind(buyBtn).For(v => v.onClick).To(vm => vm.OnClickBuyBtnCommand);
			bindingSet.Bind(buyBtn.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsBuyOver);
			bindingSet.Bind(buyOverBtn.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsBuyOver);
			bindingSet.Bind(advBtn).For(v => v.onClick).To(vm => vm.OnClickAdvBtnCommand);
			bindingSet.Bind(advBtn.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsShowAdBtn);
			bindingSet.Bind(commonAdvIconView.BindingContext).For(v => v.DataContext).To(vm => vm.CommonAdvIconVm);
			bindingSet.Bind(btnPriceNode.BindingContext).For(v => v.DataContext).To(vm => vm.BtnPriceNodeModel);
            bindingSet.Build();
        }
    }
}