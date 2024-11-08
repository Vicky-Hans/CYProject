using Cysharp.Threading.Tasks;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class MagicBingoItemView : BaseItemView
    {
        public override bool FullScreen => false;
        public CellItemView cellItem;
        public DhButton opBut;
        
        public GameObject effectOpen;
        public GameObject bingoEffect;

        private bool showOpenEffect;
        public bool ShowOpenEffect
        {
            get => showOpenEffect;
            set
            {
                showOpenEffect = value;
                if (showOpenEffect)
                {
                    effectOpen.SetActive(false);
                    effectOpen.SetActive(true);
                    EffectOver();
                }
            }
        }

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<MagicBingoItemView, MagicBingoItemViewModel>();
            bindingSet.Bind(cellItem.BindingContext).For(v => v.DataContext).To(vm => vm.CellItemVm);
            bindingSet.Bind(opBut).For(v => v.onClick).To(vm => vm.OnClickOpBtnCommand);
            bindingSet.Bind(this).For(v => v.ShowOpenEffect).To(vm => vm.ShowOpenEffect);
            bindingSet.Bind(bingoEffect).For(v => v.activeSelf).To(vm => vm.BingoEffect);
            bindingSet.Bind(opBut.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsGetAward);
            bindingSet.Build();
        }

        private async void EffectOver()
        {
            await UniTask.Delay(350);
            effectOpen.SetActive(false);
        }
    }
}