using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class PullDownListView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhText titleName;
		public RectTransform titleDirection;
		public UICircularScrollView scrollView;
		[AssetPath]public string scrollViewCell;
		public DhButton btnClick;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollView.PrefabPath = scrollViewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<PullDownListView, PullDownListViewModel>();
			bindingSet.Bind(titleName).For(v => v.text).To(vm => vm.TitleNameStr);
			bindingSet.Bind(titleDirection).For(v => v.localScale).ToExpression(vm => GetDirection(vm.IsShowPullDown));
			bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);
			bindingSet.Bind(btnClick).For(v => v.onClick).To(vm => vm.OnClickPullDownCommand);
            bindingSet.Build();
        }

        private Vector3 GetDirection(bool isShow)
        {
	        return isShow ? new Vector3(1, -1, 1) : new Vector3(1, 1, 1);
        }
    }
}