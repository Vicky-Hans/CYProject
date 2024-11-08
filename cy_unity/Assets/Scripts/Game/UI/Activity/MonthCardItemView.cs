using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class MonthCardItemView : BaseItemView
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
		public DhText titleText;
		public DhButton GetDayAwardBtn;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			effectDes.PrefabPath = effectDesCell;
			awardScrollview.PrefabPath = awardScrollviewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<MonthCardItemView, MonthCardItemViewModel>();
            bindingSet.Bind(titleText).For(v => v.text).To(vm => vm.TitleText);
			bindingSet.Bind(effectDes).For(v => v.Collection).To(vm => vm.EffectDesList);
			bindingSet.Bind(timeDes).For(v => v.text).To(vm => vm.TimeDesStr);
			bindingSet.Bind(awardScrollview).For(v => v.Collection).To(vm => vm.AwardScrollviewList);
			bindingSet.Bind(buyButton).For(v => v.onClick).To(vm => vm.OnClickBuyButtonCommand);
			bindingSet.Bind(GetDayAwardBtn).For(v => v.onClick).To(vm => vm.GetDayAwardBtnCommand);
			bindingSet.Bind(notInForce.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowNotInForceStr);
			bindingSet.Bind(atOnceAward.BindingContext).For(v=>v.DataContext).To(vm => vm.AtOnceAwardCellVm);
			bindingSet.Bind(priceNode.BindingContext).For(v => v.DataContext).To(vm => vm.PriceNodeModel);
			bindingSet.Bind(GetDayAwardBtn.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowGetAwardBut);
            bindingSet.Build();
        }
    }
}