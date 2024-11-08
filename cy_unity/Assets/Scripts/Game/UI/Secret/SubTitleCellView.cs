using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class SubTitleCellView : BaseItemView
    {
        public override bool FullScreen => false;
		public DhButton subTitleBtn;
		public DhImage lockImg;
		public DhText subTitleText;
		public DhImage chooseImg;
		public DhImage unChooseImg;
		

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<SubTitleCellView, SubTitleCellViewModel>();
			bindingSet.Bind(subTitleBtn).For(v => v.onClick).To(vm => vm.OnClickSubTitleBtnCommand);
			bindingSet.Bind(subTitleBtn.image).For(v=>v.sprite).To(vm=>vm.SubTitleBtnImgPath).WithConversion(this);
			bindingSet.Bind(lockImg.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowLockImg);
			bindingSet.Bind(subTitleText).For(v => v.text).To(vm => vm.SubTitleTextStr);
			bindingSet.Bind(subTitleText).For(v=>v.color).To(vm=>vm.SubTitleTextColor);
			bindingSet.Bind(chooseImg.gameObject).For(v => v.activeSelf).To(vm => vm.IsChoose);
			bindingSet.Bind(unChooseImg.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsChoose);
            bindingSet.Build();
        }
    }
}