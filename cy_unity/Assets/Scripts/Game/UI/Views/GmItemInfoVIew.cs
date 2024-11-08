using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class GmItemInfoVIew : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhText text;
        public DhButton btnClick;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
            await base.Create();
            var bindingSet = this.CreateBindingSet<GmItemInfoVIew, GmItemInfoVIewModel>();
			bindingSet.Bind(text).For(v => v.text).To(vm => vm.TextStr);
            bindingSet.Bind(btnClick).For(v => v.onClick).To(vm => vm.OnClickCommand);
            bindingSet.Build();
        }
    }
}