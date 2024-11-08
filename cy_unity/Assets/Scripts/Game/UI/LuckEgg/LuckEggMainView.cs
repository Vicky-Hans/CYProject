using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class LuckEggMainView : BaseView
    {
        public override bool FullScreen => false;
        public BottomComponentView bottomComponent;

        public LuckEggDrawView luckEggDrawView;
        public LuckEggExchangeView luckEggExchangeView;
        public LuckEggFundView luckEggFundView;
        public LuckEggTaskView luckEggTaskView;
        public GameObject drawIngGameObject;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<LuckEggMainView, LuckEggMainViewModel>();
            bindingSet.Bind(bottomComponent.BindingContext).For(v => v.DataContext).To(vm => vm.BottomComponentVm);
            
            bindingSet.Bind(luckEggFundView.BindingContext).For(v => v.DataContext).To(vm => vm.LuckEggFundVm);
            bindingSet.Bind(luckEggFundView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.Manager.EggTabType == LuckEggShowView.Fund);
            
            bindingSet.Bind(luckEggExchangeView.BindingContext).For(v => v.DataContext).To(vm => vm.LuckEggExchangeVm);
            bindingSet.Bind(luckEggExchangeView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.Manager.EggTabType == LuckEggShowView.Exchange);
            
            bindingSet.Bind(luckEggDrawView.BindingContext).For(v => v.DataContext).To(vm => vm.LuckEggDrawVm);
            bindingSet.Bind(luckEggDrawView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.Manager.EggTabType == LuckEggShowView.Main);
            
            bindingSet.Bind(luckEggTaskView.BindingContext).For(v => v.DataContext).To(vm => vm.LuckEggTaskVm);
            bindingSet.Bind(luckEggTaskView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.Manager.EggTabType == LuckEggShowView.Task);
            
            bindingSet.Bind(drawIngGameObject).For(v => v.activeSelf).To(vm => vm.Manager.LuckEggDrawIng);
            
            bindingSet.Build();
        }
    }
}