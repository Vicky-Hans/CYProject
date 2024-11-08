using DH.Game.ViewModels;
using DH.UIFramework;
namespace DH.Game.UIViews
{
    public partial class EquipAdvanceView : BaseView
    {
        public override bool FullScreen => false;
        
		public UICircularScrollView equipScrollView;
		[AssetPath]public string equipScrollViewCell;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			equipScrollView.PrefabPath = equipScrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<EquipAdvanceView, EquipAdvanceViewModel>();
			bindingSet.Bind(equipScrollView).For(v => v.Collection).To(vm => vm.EquipScrollViewList);
            bindingSet.Build();
            equipScrollView.m_CanDragScrollView = false;
            
            AudioManager.Instance.Play(AudioType.OpenUi);
        }

        public override bool OnPhysicExit()
        {
            return true;
        }
    }
}