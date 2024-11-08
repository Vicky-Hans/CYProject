using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using Extend;
namespace DH.Game.UIViews
{
    public partial class SecretView : BaseView
    {
        public override bool FullScreen => false;
        
		public CommonTopView commonTopItems;
		public UICircularScrollView levelScrollview;
		[AssetPath]public string levelScrollviewCell;
		public DhButton opBtn;
		public DhText opBtnText;
		public ItemPriceNodeView costItemView;
		public DhText leftCountText;
		public UICircularScrollView subTitleScrollView;
		[AssetPath]public string subTitleScrollViewCell;
		public BottomComponentView bottomComponent;
		public DhButton infoBtn;
		public DhText timeText;
		public DhButton rankBtn;
		public DhButton seasonBtn;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			levelScrollview.PrefabPath = levelScrollviewCell;
			subTitleScrollView.PrefabPath = subTitleScrollViewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<SecretView, SecretViewModel>();
			bindingSet.Bind(commonTopItems.BindingContext).For(v => v.DataContext).To(vm => vm.CommonTopItemsVm);
			bindingSet.Bind(levelScrollview).For(v => v.Collection).To(vm => vm.LevelScrollviewList);
			bindingSet.Bind(levelScrollview).For(v => v.DefaultJumpIndex).To(vm => vm.DefaultIndex);
			bindingSet.Bind(this).For(v => v.JumpIndex).To(vm => vm.DefaultIndex);
			bindingSet.Bind(opBtn).For(v => v.onClick).To(vm => vm.OnClickOpBtnCommand);
			bindingSet.Bind(opBtnText).For(v => v.text).To(vm => vm.OpBtnTextStr);
			bindingSet.Bind(costItemView.BindingContext).For(v => v.DataContext).To(vm => vm.CostItemViewVm);
			bindingSet.Bind(costItemView.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowCostItem);
			bindingSet.Bind(leftCountText).For(v => v.text).To(vm => vm.LeftCountTextStr);
			bindingSet.Bind(subTitleScrollView).For(v => v.Collection).To(vm => vm.SubTitleScrollViewList);
			bindingSet.Bind(bottomComponent.BindingContext).For(v => v.DataContext).To(vm => vm.BottomComponentVm);
			bindingSet.Bind(infoBtn).For(v => v.onClick).To(vm => vm.OnClickInfoBtnCommand);
			bindingSet.Bind(timeText).For(v => v.text).To(vm => vm.TimeTextStr);
			bindingSet.Bind(rankBtn).For(v => v.onClick).To(vm => vm.OnClickRankBtnCommand);
			bindingSet.Bind(seasonBtn).For(v => v.onClick).To(vm => vm.OnClickSeasonBtnCommand);
            bindingSet.Build();
        }

        private int jumpIndex;

        public int JumpIndex
        {
	        get=>jumpIndex;
	        set=> UpdateJumpIndex(value);
        }

        private void UpdateJumpIndex(int value)
        {
	        if(levelScrollview ==null || value == -1)return;
	        levelScrollview.Jump2SpecificItem(value);
        }
    }
}