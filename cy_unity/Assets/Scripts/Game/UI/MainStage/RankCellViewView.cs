using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using TMPro;
using Extend;
namespace DH.Game.UIViews
{
    public partial class RankCellViewView : BaseItemView
    {
        public override bool FullScreen => false;

        public DhImage bg;
        public DhButton opBtn;
		public DhImage rankBg;
		public TextMeshProUGUI rankText;
		public CommonHeadItemView commonHeadItemView;
		public CommonPlayerNameView commonPlayerNameView;
		public DhText levelText;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<RankCellViewView, RankCellViewViewModel>();
            bindingSet.Bind(bg).For(v=>v.sprite).To(vm=>vm.BgPath).WithConversion(this);
			bindingSet.Bind(rankBg).For(v => v.sprite).To(vm => vm.RankBgPath).WithConversion(this);
			bindingSet.Bind(rankText).For(v => v.text).To(vm => vm.RankTextStr);
			bindingSet.Bind(commonHeadItemView.BindingContext).For(v => v.DataContext).To(vm => vm.CommonHeadItemViewVm);
			bindingSet.Bind(commonPlayerNameView.BindingContext).For(v => v.DataContext).To(vm => vm.CommonPlayerNameVm);
			bindingSet.Bind(levelText).For(v => v.text).To(vm => vm.LevelTextStr);
            bindingSet.Bind(opBtn).For(v => v.onClick).To(vm => vm.OnClickOpBtnCommand);
            bindingSet.Build();
        }
    }
}