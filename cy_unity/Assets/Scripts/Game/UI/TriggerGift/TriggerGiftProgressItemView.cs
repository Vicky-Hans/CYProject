using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine.UI;
using Extend;
namespace DH.Game.UIViews
{
    public partial class TriggerGiftProgressItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhText progressText;
		public Slider progress;

        public SelectCellItemEffectView selectCellItemEffectView;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<TriggerGiftProgressItemView, TriggerGiftProgressItemViewModel>();
            
			bindingSet.Bind(progressText).For(v => v.text).To(vm => vm.ProgressTextStr);
			bindingSet.Bind(progress).For(v => v.value).To(vm => vm.ProgressValue);
            bindingSet.Bind(selectCellItemEffectView.BindingContext).For(v => v.DataContext).To(vm => vm.SelectCellItemEffectVm);
            bindingSet.Build();
        }
    }
}