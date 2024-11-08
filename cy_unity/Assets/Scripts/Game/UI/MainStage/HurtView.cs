using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;
namespace DH.Game.UIViews
{
    public partial class HurtView : BaseView
    {
        public override bool FullScreen => false;
        
		public RectTransform bgRect;
		public DhButton closeBtn;
		public UICircularScrollView hurtScrollview;
		[AssetPath]public string hurtScrollviewCell;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
			hurtScrollview.PrefabPath = hurtScrollviewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<HurtView, HurtViewModel>();
			bindingSet.Bind(bgRect).For(v => v.sizeDelta).To(vm => vm.BgSize);
			bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtnCommand);
			bindingSet.Bind(hurtScrollview).For(v => v.Collection).To(vm => vm.HurtScrollviewList);
            bindingSet.Build();
        }
    }
}