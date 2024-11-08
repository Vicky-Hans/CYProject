using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class AdFreeGiftPrivilegeMonthCardItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public ScrollRectExtend scrollViewLock;
		[AssetPath]public string scrollViewLockCell;
		public UICircularScrollView awardScrollview;
		[AssetPath]public string awardScrollviewCell;
		public DhButton buyButton;
		public CellItemBaseView atOnceAward;
		public BtnPriceNode priceNode;
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
			scrollViewLock.PrefabPath = scrollViewLockCell;
			awardScrollview.PrefabPath = awardScrollviewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<AdFreeGiftPrivilegeMonthCardItemView, AdFreeGiftPrivilegeMonthCardItemViewModel>();
            
			bindingSet.Bind(scrollViewLock).For(v => v.Collection).To(vm => vm.ScrollViewLockList);
			bindingSet.Bind(awardScrollview).For(v => v.Collection).To(vm => vm.AwardScrollviewList);
			bindingSet.Bind(buyButton).For(v => v.onClick).To(vm => vm.OnClickBuyButtonCommand);
			bindingSet.Bind(atOnceAward.BindingContext).For(v => v.DataContext).To(vm => vm.AtOnceAwardCellVm);
			bindingSet.Bind(priceNode.BindingContext).For(v => v.DataContext).To(vm => vm.BtnPriceNodeVm);
			bindingSet.Bind(title).For(v => v.text).To(vm => vm.TitleStr);
			bindingSet.Bind(this).For(v => v.ShowSelf).To(vm => vm.IsShowSelf);
            bindingSet.Build();
        }
    }
}