using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class MagicBingoProgressItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
        public DhText progressText;

        public SelectCellItemEffectView selectCellItemEffectView;

        public GameObject getEffectUp;
        public GameObject getEffectDown;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<MagicBingoProgressItemView, MagicBingoProgressItemViewModel>();
            
            bindingSet.Bind(progressText).For(v => v.text).To(vm => vm.ProgressTextStr);
            bindingSet.Bind(selectCellItemEffectView.BindingContext).For(v => v.DataContext).To(vm => vm.SelectCellItemEffectVm);
            bindingSet.Bind(getEffectUp).For(v => v.activeSelf).ToExpression(vm => vm.ShowEffect);
            bindingSet.Bind(getEffectDown).For(v => v.activeSelf).ToExpression(vm => vm.ShowEffect);
            bindingSet.Build();
        }
    }
}