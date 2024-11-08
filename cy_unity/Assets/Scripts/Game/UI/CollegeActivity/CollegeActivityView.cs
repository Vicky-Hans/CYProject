using DH.Game.UI;
using Dh.Game.UIViews.ItemViews;
using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using Game.UI.UIViews;

namespace DH.Game.UIViews
{
    public partial class CollegeActivityView : BaseView
    {
        public CollegeTaskView collegeTaskView;
        
        public CollegeRankView collegeRankView;
        public CollegeDiscountsView collegeDiscountsView;
        public CollegeShopView collegeShopView;
        public TabBtnGroupView bottomComponentView;
        public DhButton btnClose;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindSet = this.CreateBindingSet<CollegeActivityView, CollegeActivityModel>();
            bindSet.Bind(btnClose).For(v => v.onClick).To(vm => vm.OnCloseCommand);
            bindSet.Bind(bottomComponentView.BindingContext()).For(v => v.DataContext).To(vm => vm.CollegeTabBtnModel);
            bindSet.Bind(collegeTaskView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.CurSelect == CollegeActivity.CollegeTask);
            bindSet.Bind(collegeRankView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.CurSelect == CollegeActivity.CollegeRank);
            bindSet.Bind(collegeDiscountsView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.CurSelect == CollegeActivity.CollegeFund);
            bindSet.Bind(collegeShopView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.CurSelect == CollegeActivity.CollegeShop);
            
            bindSet.Bind(collegeRankView.BindingContext).For(v => v.DataContext).To(vm => vm.CollegeRankView);
            bindSet.Bind(collegeTaskView.BindingContext).For(v => v.DataContext).To(vm => vm.CollegeTaskModel);
            bindSet.Bind(collegeDiscountsView.BindingContext).For(v => v.DataContext).To(vm => vm.CollegeDiscountsModel);
            bindSet.Bind(collegeShopView.BindingContext).For(v => v.DataContext).To(vm => vm.CollegeShopModel);
            
            bindSet.Build();
        }
    }
}