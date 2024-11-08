using DH.Game.ViewModels;
using DH.UIFramework;

namespace DH.Game.UIViews
{
    public partial class HolyView : BaseView
    {
        public override bool FullScreen => false;
        

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<HolyView, HolyViewModel>();
            

            bindingSet.Build();
        }
    }
}