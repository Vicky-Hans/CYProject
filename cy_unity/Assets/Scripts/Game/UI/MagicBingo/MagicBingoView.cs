using DH.Game.ViewModels;
using DH.UIFramework;

namespace DH.Game.UIViews
{
    public partial class MagicBingoView : BaseView
    {
        public override bool FullScreen => false;
        public BottomComponentView bottomComponent;
        public MagicBingoGiftView magicBingoGiftView;
        public MagicBingoExchangeView magicBingoExchangeView;
        public MagicBingoTaskView magicBingoTaskView;
        public MagicBingoBGView magicBingoView;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<MagicBingoView, MagicBingoViewModel>();
            bindingSet.Bind(bottomComponent.BindingContext).For(v => v.DataContext).To(vm => vm.BottomComponentVm);
            
            bindingSet.Bind(magicBingoGiftView.BindingContext).For(v => v.DataContext).To(vm => vm.MagicBingoGiftVm);
            bindingSet.Bind(magicBingoGiftView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.Manager.BagicBingo == MagicBingoShowType.Gift);

            bindingSet.Bind(magicBingoExchangeView.BindingContext).For(v => v.DataContext).To(vm => vm.MagicBingoExchangeVm);
            bindingSet.Bind(magicBingoExchangeView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.Manager.BagicBingo  == MagicBingoShowType.Exchange);
            
            bindingSet.Bind(magicBingoTaskView.BindingContext).For(v => v.DataContext).To(vm => vm.MagicBingoTaskVm);
            bindingSet.Bind(magicBingoTaskView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.Manager.BagicBingo  == MagicBingoShowType.Task);
            
            bindingSet.Bind(magicBingoView.BindingContext).For(v => v.DataContext).To(vm => vm.MagicBingoBgVm);
            bindingSet.Bind(magicBingoView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.Manager.BagicBingo  == MagicBingoShowType.Main);
            
            bindingSet.Build();
        }
    }
}