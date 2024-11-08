using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class MonthCardPropertyView : BaseItemView
    {
        public override bool FullScreen => false;

		public DhText text;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<MonthCardPropertyView, MonthCardPropertyViewModel>();
            
			bindingSet.Bind(text).For(v => v.text).To(vm => vm.TextStr);

            bindingSet.Build();
        }
    }
}