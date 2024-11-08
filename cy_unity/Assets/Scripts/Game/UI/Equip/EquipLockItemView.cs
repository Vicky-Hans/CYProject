using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class EquipLockItemView : BaseItemView
    {
        public override bool FullScreen => true;

        public DhImage bg;
		public DhImage icon;
		public DhImage typeIcon;
		public DhImage attrIcon;
        public DhButton btnClickOpen;
        public DhText lockTips;
        public override async UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<EquipLockItemView, EquipLockItemViewModel>();
            bindingSet.Bind(bg).For(v => v.sprite).To(vm => vm.BgPath).WithConversion(this);
			bindingSet.Bind(icon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);
			bindingSet.Bind(typeIcon).For(v => v.sprite).To(vm => vm.TypeIconPath).WithConversion(this);
            bindingSet.Bind(btnClickOpen).For(v => v.onClick).To(vm => vm.OnClickOpenCommand);
            bindingSet.Bind(lockTips).For(v => v.text).ToExpression(vm => GetLockTips(vm.UnLockChapterId));
            bindingSet.Bind(attrIcon.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.AttrNum>1);
            bindingSet.Build();
        }

        private string GetLockTips(int chapterId)
        {
            return LocalizeHelper.GetGlobal(GlobalLanguageId.Equip_04, chapterId);//$"通过章节{chapterId}解锁";
        }
    }
}