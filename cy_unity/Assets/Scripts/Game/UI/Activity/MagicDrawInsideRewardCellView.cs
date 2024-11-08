using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class MagicDrawInsideRewardCellView : BaseItemView,IViewKey
    {
        public override bool FullScreen => false;
		public DhImage bg1;
		public DhImage bg2;
		public DhText countText;
		public MagicDrawRewardCellView magicDrawRewardCell;
		public DhImage chooseImg;
		public int index;
		public DhImage outMask;
		public object Key => index;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<MagicDrawInsideRewardCellView, MagicDrawInsideRewardCellViewModel>();
			bindingSet.Bind(bg1).For(v => v.sprite).To(vm => vm.BgPath).WithConversion(this);
			bindingSet.Bind(bg2).For(v => v.sprite).To(vm => vm.BgPath).WithConversion(this);
			bindingSet.Bind(countText).For(v => v.text).To(vm => vm.CountTextStr);
			bindingSet.Bind(magicDrawRewardCell.BindingContext).For(v => v.DataContext).To(vm => vm.MagicDrawRewardCellVm);
			// bindingSet.Bind(magicDrawRewardCell.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowRewardCell);
			bindingSet.Bind(chooseImg.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowChooseImg);
			bindingSet.Bind(outMask.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowOutMask);
            bindingSet.Build();

            ResetMagicDrawRewardCellViewAngle();
        }

        private void ResetMagicDrawRewardCellViewAngle()
        {
	        var curAngle = gameObject.transform.localEulerAngles;
	        curAngle.z = -curAngle.z;
	        magicDrawRewardCell.gameObject.transform.localEulerAngles = curAngle;
        }
    }
}