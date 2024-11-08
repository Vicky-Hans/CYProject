using DH.Game.ViewModels;
using DH.UIFramework;
using TMPro;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class CommonRuleCellView : BaseItemView
    {
        public override bool FullScreen => false;

        public Image bgImg;
		public TextMeshProUGUI title;
		public TextMeshProUGUI content;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<CommonRuleCellView, CommonRuleCellViewModel>();
            bindingSet.Bind(bgImg).For(v => v.sprite).To(vm => vm.BgImgPath).WithConversion(this);
            bindingSet.Bind(bgImg.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowBg);
			bindingSet.Bind(title).For(v => v.text).To(vm => vm.TitleStr);
			bindingSet.Bind(content).For(v => v.text).To(vm => vm.ContentStr);
            bindingSet.Build();
        }
    }
}