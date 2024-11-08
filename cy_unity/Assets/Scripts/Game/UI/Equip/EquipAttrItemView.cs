using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class EquipAttrItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhImage bg;
		public DhImage targetIcon;
		public DhText targetValue;
		public DhText targetName;
		
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<EquipAttrItemView, EquipAttrItemViewModel>();
            
			bindingSet.Bind(bg).For(v => v.sprite).To(vm => vm.BgPath).WithConversion(this);
			bindingSet.Bind(targetIcon).For(v => v.sprite).To(vm => vm.TargetIconPath).WithConversion(this);
			bindingSet.Bind(targetValue).For(v => v.text).To(vm => vm.TargetValueStr);
			bindingSet.Bind(targetName).For(v => v.text).To(vm => vm.TargetNameStr);
			bindingSet.Bind(targetValue).For(v => v.color).To(vm => vm.FontColor);
			bindingSet.Bind(targetName).For(v => v.color).To(vm => vm.TitleColor);
            bindingSet.Build();
        }
    }
}