using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine.UI;
using Extend;
namespace DH.Game.UIViews
{
    public partial class MagicFundCellView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public ScrollRectExtend freeScrollview;
		[AssetPath]public string freeScrollviewCell;
		public ScrollRectExtend plusScrollview;
		[AssetPath]public string plusScrollviewCell;
		public Slider progress;
		public DhText levelText;
		public DhImage maskImg;
		public DhImage progressImg;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			freeScrollview.PrefabPath = freeScrollviewCell;
			plusScrollview.PrefabPath = plusScrollviewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<MagicFundCellView, MagicFundCellViewModel>();
			bindingSet.Bind(freeScrollview).For(v => v.Collection).To(vm => vm.FreeScrollviewList);
			bindingSet.Bind(plusScrollview).For(v => v.Collection).To(vm => vm.PlusScrollviewList);
			bindingSet.Bind(progress).For(v => v.value).To(vm => vm.ProgressValue);
			bindingSet.Bind(levelText).For(v => v.text).To(vm => vm.LevelTextStr);
			bindingSet.Bind(maskImg.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowMaskImg);
			bindingSet.Bind(progressImg.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowProgress);
			// bindingSet.Bind(progress.transform).For(v => v.localPosition).To(vm => vm.ProgressLocalPos);
            bindingSet.Build();
        }
    }
}