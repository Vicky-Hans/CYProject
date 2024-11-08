using Cysharp.Threading.Tasks;
using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;

namespace DH.Game.UIViews
{
    public partial class WeaponItemView : BaseItemView
    {
        public DhImage weaponIcon;
        public DhImage cdIcon;
        public CanvasGroup canvasGroupObj;
        public RectTransform cdRect;
        public RectTransform rootNode;
        public RectTransform imgRect;
        public RectTransform gainTipsEffect;
        /// <summary>
        /// 装备模型id
        /// </summary>
        public int EquipModelId;
        public override async UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<WeaponItemView, WeaponItemViewModel>();
            bindingSet.Bind(canvasGroupObj).For(v => v.alpha).To(vm => vm.Alpha);
            bindingSet.Bind(rootNode).For(v => v.localPosition).ToExpression(vm =>vm.CalculationLocalPos(vm.PosIndex,vm.WeaponData));
            bindingSet.Bind(cdIcon).For(v => v.fillAmount).To(vm => vm.CdTime);
            bindingSet.Bind(rootNode).For(v => v.sizeDelta).To(vm => vm.WeaponSize);
            bindingSet.Bind(cdRect).For(v => v.sizeDelta).To(vm => vm.IconSize);
            bindingSet.Bind(imgRect).For(v => v.sizeDelta).To(vm => vm.IconSize);
            bindingSet.Bind(gainTipsEffect.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowGainTips);
            bindingSet.Bind(gainTipsEffect).For(v => v.anchoredPosition).To(vm => vm.TipsEffectPos);
            bindingSet.Bind(this).For(v => v.imgRect).To(vm => vm.WeaponIconRect).OneWayToSource();
            bindingSet.Bind(this).For(v => v.rootNode).To(vm => vm.WeaponNodeRect).OneWayToSource();
            bindingSet.Bind(cdIcon).For(v => v.sprite).To(vm => vm.IconPathStr).WithConversion(this);
            bindingSet.Bind(weaponIcon).For(v => v.sprite).To(vm => vm.IconPathStr).WithConversion(this);
            bindingSet.Bind(this).For(v => v.EquipModelId).To(vm => vm.WeaponData.WeaponId);
            bindingSet.Bind(this).For(v => v.weaponIcon).To(vm => vm.WeaponIcon).OneWayToSource();
            bindingSet.Bind(this).For(v => v.weaponIcon.sprite).To(vm => vm.WeaponSprite).OneWayToSource();
            bindingSet.Build();
        }
    }
}