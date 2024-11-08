using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using TMPro;
using Extend;
namespace DH.Game.UIViews
{
    public partial class CollegeRankItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public TextMeshProUGUI rankText;
		public DhImage rankIcon;
		public CommonHeadItemView commonHeadItemView;
		public CommonPlayerNameView commonPlayerName;
		public DhText levelText;
		public DhButton btnClick;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<CollegeRankItemView, CollegeRankItemViewModel>();
            
			bindingSet.Bind(rankText).For(v => v.text).To(vm => vm.RankTextStr);
			bindingSet.Bind(rankIcon).For(v => v.sprite).To(vm => vm.RankIconPath).WithConversion(this);
			bindingSet.Bind(commonHeadItemView.BindingContext).For(v => v.DataContext).To(vm => vm.CommonHeadItemViewVm);
			bindingSet.Bind(commonPlayerName.BindingContext).For(v => v.DataContext).To(vm => vm.CommonPlayerNameVm);
			bindingSet.Bind(levelText).For(v => v.text).To(vm => vm.LevelTextStr);
			bindingSet.Bind(btnClick).For(v => v.onClick).To(vm => vm.OnClickCommand);
            bindingSet.Build();
        }
    }
}