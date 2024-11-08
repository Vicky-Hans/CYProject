using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class SecretWeaponCellView : BaseItemView
    {
        public override bool FullScreen => false;
        public RectTransform rectTransform;
        public DhImage weaponBg;
        public DhImage weaponIcon;
        public RectTransform iconRectTransform;
		public DhImage timeSlider;
        public DhText countText;
        public GameObject effectNode;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<SecretWeaponCellView, SecretWeaponCellViewModel>();
            bindingSet.Bind(rectTransform).For(v => v.sizeDelta).ToExpression(vm => GetSizeDelta(vm.CellSize));
            bindingSet.Bind(this).For(v=>v.weaponIcon).To(vm=>vm.WeaponIcon).OneWayToSource();
            bindingSet.Bind(this).For(v=>v.weaponBg).To(vm=>vm.WeaponBg).OneWayToSource();
			bindingSet.Bind(timeSlider).For(v => v.fillAmount).To(vm => vm.TimeSliderValue);
            bindingSet.Bind(countText).For(v => v.text).To(vm => vm.EquipModelId);
            bindingSet.Bind(effectNode).For(v => v.activeSelf).To(vm => vm.IsShowEffect);
            bindingSet.Build();
        }

        public Vector2 GetSizeDelta(ECellItemSizeType cellSize)
        {
            var ret = Vector2.zero;
            var iconSize = Vector2.zero;
            var effectScale = 1.0f;
            switch (cellSize)
            {
                case ECellItemSizeType.Size90X80:
                {
                    iconSize = new Vector2(80, 80);
                    ret = new Vector2(90, 90);
                    effectScale = 90 / 166f;
                    break;
                }
                case ECellItemSizeType.Size117X76:
                {
                    iconSize = new Vector2(76, 76);
                    ret = new Vector2(117, 117);
                    effectScale = 117 / 166f;
                    break;
                }
                case ECellItemSizeType.Size120X100:
                {
                    iconSize = new Vector2(100, 100);
                    ret = new Vector2(120, 120);
                    effectScale = 120 / 166f;
                    break;
                }
                case ECellItemSizeType.Size166X150:
                {
                    iconSize = new Vector2(150, 150);
                    ret = new Vector2(166, 166);
                    effectScale = 166 / 166f;
                    break;
                }
                case ECellItemSizeType.Size180X150:
                {
                    iconSize = new Vector2(150, 150);
                    ret = new Vector2(180, 180);
                    effectScale = 180 / 166f;
                    break;
                }
                case ECellItemSizeType.Size200X180:
                {
                    iconSize = new Vector2(180, 180);
                    ret = new Vector2(200, 200);
                    effectScale = 200 / 166f;
                    break;
                }
            }

            iconRectTransform.sizeDelta = iconSize;
            effectNode.transform.localScale = Vector3.one * effectScale;
            return ret;
        }
    }
}