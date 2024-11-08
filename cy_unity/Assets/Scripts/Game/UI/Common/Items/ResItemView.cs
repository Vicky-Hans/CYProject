using DH.Game.ViewModels;
using DH.UIFramework;
using TMPro;
using UnityEngine.UI;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class ResItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhButton bg;
		public DhImage icon;
		public Image addIcon;
		public TextMeshProUGUI countText;
		public DhText leftTime;
		public RectTransform bgRect;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<ResItemView, ResItemViewModel>();
			bindingSet.Bind(bg).For(v => v.onClick).To(vm => vm.OnClickBgCommand);
			bindingSet.Bind(icon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);
			bindingSet.Bind(countText).For(v => v.text).To(vm => vm.CountTextStr);
			bindingSet.Bind(addIcon.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowAddIcon);
			bindingSet.Bind(leftTime.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowLeftTime);
			bindingSet.Bind(leftTime).For(v => v.text).To(vm => vm.LeftTimeStr);
			bindingSet.Bind(this).For(v => v.bgRect).To(vm => vm.BgRect).OneWayToSource();
            bindingSet.Build();
        }
    }
}