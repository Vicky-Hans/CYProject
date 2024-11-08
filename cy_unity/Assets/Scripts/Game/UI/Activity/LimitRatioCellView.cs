using DH.Game.ViewModels;
using DH.UIFramework;
namespace DH.Game.UIViews
{
    public partial class LimitRatioCellView : BaseItemView
    {
        public override bool FullScreen => false;
		public ScrollRectExtend limitLayout;
		[AssetPath]public string limitLayoutCell;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
			limitLayout.PrefabPath = limitLayoutCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<LimitRatioCellView, LimitRatioCellViewModel>();
			bindingSet.Bind(limitLayout).For(v => v.Collection).To(vm => vm.LimitLayoutList);
            bindingSet.Build();
        }
    }
}