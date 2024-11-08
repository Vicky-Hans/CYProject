using DH.Data;
using DH.Game.ViewModels;
using DH.UIFramework;
namespace DH.Game.UIViews
{
    public partial class PassportsMainView : BaseView
    {
        public override bool FullScreen => false;
        public BottomComponentView bottomComponent;
        public PassportsView passportsView;
        public ChapterFundView chapterFundView;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<PassportsMainView, PassportsMainViewModel>();
            bindingSet.Bind(bottomComponent.BindingContext).For(v => v.DataContext).To(vm => vm.BottomComponentVm);
            bindingSet.Bind(passportsView.BindingContext).For(v => v.DataContext).To(vm => vm.PassportsVm);
            bindingSet.Bind(passportsView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.CurPassPortType != EPassPortType.PassportTypeChapter);
            bindingSet.Bind(chapterFundView.BindingContext).For(v => v.DataContext).To(vm => vm.ChapterFundVm);
            bindingSet.Bind(chapterFundView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.CurPassPortType == EPassPortType.PassportTypeChapter);
            bindingSet.Build();
        }
    }
}