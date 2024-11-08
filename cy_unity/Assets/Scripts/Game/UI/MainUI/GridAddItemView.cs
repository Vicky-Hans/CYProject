using Cysharp.Threading.Tasks;
using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class GridAddItemView : BaseItemView
    {
        public DhImage adIcon;
        public DhImage gridAddIcon;
        public RectTransform iconRect;
        public RectTransform gridRect;
        public RectTransform adIconRect;
        public CanvasGroup canvasGroupObj;
        public override async UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<GridAddItemView, GridAddItemViewModel>();
            bindingSet.Bind(canvasGroupObj).For(v => v.alpha).ToExpression(vm => vm.Alpha);
            bindingSet.Bind(gridRect).For(v => v.sizeDelta).To(vm => vm.GridSize);
            bindingSet.Bind(iconRect).For(v => v.sizeDelta).To(vm => vm.IconSize);
            bindingSet.Bind(adIconRect).For(v => v.sizeDelta).To(vm => vm.AdIconSize);
            bindingSet.Bind(adIcon.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.AdState == 1);
            bindingSet.Bind(adIcon).For(v => v.sprite).To(vm => vm.AdIconPathStr).WithConversion(this);
            bindingSet.Bind(gridAddIcon).For(v => v.sprite).To(vm => vm.IconPathStr).WithConversion(this);
            bindingSet.Bind(this).For(v => v.gridRect).To(vm => vm.GridNodeRect).OneWayToSource();
            bindingSet.Build();
        }
    }
}