
using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using Extend;
namespace DH.Game.UIViews
{
    public partial class ClothesResetView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhButton btnClose;
		public ScrollRectExtend scrollView;
		[AssetPath]public string scrollViewCell;
		public CellItemBaseView cellItemBaseView;
		public TabBtnGroupView tabGroup;
		public DhButton btnReset;
		public DhText resetDesc;
		public DhText noneTipsDesc;
		public DhText resetTipsDesc;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollView.PrefabPath = scrollViewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<ClothesResetView, ClothesResetViewModel>();
            
			bindingSet.Bind(btnClose).For(v => v.onClick).To(vm => vm.OnClickBtnCloseCommand);
			bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);
			bindingSet.Bind(cellItemBaseView.BindingContext).For(v => v.DataContext).To(vm => vm.CellItemBaseViewVm);
			bindingSet.Bind(tabGroup.BindingContext).For(v => v.DataContext).To(vm => vm.TabGroupVm);
			bindingSet.Bind(btnReset).For(v => v.onClick).To(vm => vm.OnClickBtnResetCommand);
			bindingSet.Bind(resetDesc).For(v => v.text).To(vm => vm.ResetDescStr);
			bindingSet.Bind(resetTipsDesc).For(v => v.text).To(vm => vm.ResetTipsDescStr);
			bindingSet.Bind(noneTipsDesc).For(v => v.text).To(vm => vm.NoneTipsStr);
			bindingSet.Bind(noneTipsDesc.gameObject).For(v => v.activeSelf).To(vm => vm.IsNoneState);

            bindingSet.Build();
        }
    }
}