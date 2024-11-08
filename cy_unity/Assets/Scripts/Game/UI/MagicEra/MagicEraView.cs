using Cysharp.Threading.Tasks;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class MagicEraView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhText timesDes;
		public UICircularScrollView scrollView;
		[AssetPath]public string scrollViewCell;
		public DhButton close;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollView.PrefabPath = scrollViewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<MagicEraView, MagicEraViewModel>();
            
			bindingSet.Bind(timesDes).For(v => v.text).To(vm => vm.TimesDesStr);
			bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);
			bindingSet.Bind(close).For(v => v.onClick).To(vm => vm.OnClickCloseCommand);
			bindingSet.Bind(this).For(v => v.ScrollPos).ToExpression(vm => vm.ScrollViewPos);
			bindingSet.Bind(scrollView).For(v => v.DefaultJumpIndex).To(vm => vm.ScrollViewPos);
            bindingSet.Build();
        }
        
        #region 跳转
        public int ScrollPos
        {
	        get => 0;
	        set => DelayScrollToPos(value).Forget();
        }
        private  async UniTaskVoid DelayScrollToPos(int pos)
        {
	        if(scrollView ==null) return;
	        scrollView.Jump2SpecificItem(pos);
        }
        

        #endregion
        
    }
}