using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using Extend;
namespace DH.Game.UIViews
{
    public partial class CollegeRankView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public RankTopCellView rankTopCell1;
		public RankTopCellView rankTopCell2;
		public RankTopCellView rankTopCell3;
		public DhText rankText;
		public DhText playNameText;
		public DhText maxLevelText;
		public UICircularScrollView scrollViewScore;
		[AssetPath]public string scrollViewScoreCell;
		public UICircularScrollView scrollViewReward;
		[AssetPath]public string scrollViewRewardCell;
		public CommonTopView commonTopItems;
		public CollegeRankItemView collegeRankItemView;
		public CollegeRankRewardItemView collegeRankRewardItemView;
		public TabBtnGroupTitleView tabBtnGroupTitleView;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollViewScore.PrefabPath = scrollViewScoreCell;
			scrollViewReward.PrefabPath = scrollViewRewardCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<CollegeRankView, CollegeRankViewModel>();
            
			bindingSet.Bind(rankTopCell1.BindingContext).For(v => v.DataContext).To(vm => vm.RankTopCell1Vm);
			bindingSet.Bind(rankTopCell2.BindingContext).For(v => v.DataContext).To(vm => vm.RankTopCell2Vm);
			bindingSet.Bind(rankTopCell3.BindingContext).For(v => v.DataContext).To(vm => vm.RankTopCell3Vm);
			bindingSet.Bind(rankText).For(v => v.text).To(vm => vm.RankTextStr);
			bindingSet.Bind(playNameText).For(v => v.text).To(vm => vm.PlayNameTextStr);
			bindingSet.Bind(maxLevelText).For(v => v.text).To(vm => vm.MaxLevelTextStr);
			bindingSet.Bind(scrollViewScore).For(v => v.Collection).To(vm => vm.ScrollViewScoreList);
			bindingSet.Bind(scrollViewReward).For(v => v.Collection).To(vm => vm.ScrollViewRewardList);
			
			bindingSet.Bind(scrollViewScore.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.SelectShopTitle == 0);
			bindingSet.Bind(scrollViewReward.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.SelectShopTitle == 1);
			
			bindingSet.Bind(collegeRankItemView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.SelectShopTitle == 0);
			bindingSet.Bind(collegeRankRewardItemView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.SelectShopTitle == 1);
			
			bindingSet.Bind(commonTopItems.BindingContext).For(v => v.DataContext).To(vm => vm.CommonTopItemsVm);
			bindingSet.Bind(collegeRankItemView.BindingContext).For(v => v.DataContext).To(vm => vm.CollegeRankItemViewVm);
			bindingSet.Bind(collegeRankRewardItemView.BindingContext).For(v => v.DataContext).To(vm => vm.CollegeRankRewardItemViewVm);
			bindingSet.Bind(tabBtnGroupTitleView.BindingContext).For(v => v.DataContext).To(vm => vm.TabBtnGroupTitleViewVm);

            bindingSet.Build();
        }
    }
}