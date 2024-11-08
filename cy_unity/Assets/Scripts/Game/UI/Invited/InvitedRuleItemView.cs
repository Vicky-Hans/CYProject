using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class InvitedRuleItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhImage icon;
		public DhText descText;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<InvitedRuleItemView, InvitedRuleItemViewModel>();
			bindingSet.Bind(icon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);
			bindingSet.Bind(descText).For(v => v.text).To(vm => vm.DescTextStr);
            bindingSet.Build();
        }
    }
}