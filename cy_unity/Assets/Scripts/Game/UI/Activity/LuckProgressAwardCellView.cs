using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine.UI;
using Extend;
namespace DH.Game.UIViews
{
    public partial class LuckProgressAwardCellView : BaseItemView
    {
        public override bool FullScreen => false;
		public SelectCellItemEffectView cellItemView;
		public DhText progressText;
		public Slider progress;
		public Image progressBg;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<LuckProgressAwardCellView, LuckProgressAwardCellViewModel>();
			bindingSet.Bind(cellItemView.BindingContext).For(v => v.DataContext).To(vm => vm.CellItemViewVm);
			bindingSet.Bind(progressText).For(v => v.text).To(vm => vm.ProgressTextStr);
			bindingSet.Bind(progress).For(v => v.value).To(vm => vm.ProgressValue);
			bindingSet.Bind(progressBg).For(v => v.sprite).To(vm => vm.ProgressImgPath).WithConversion(this);
            bindingSet.Build();
        }
    }
}