using System;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class CanClaimItemView:BaseItemView
    {
        public bool fullScreen => false;
        public Image bgImg;
        public Image icon;
        public Image partIcon;
        public GameObject stateDot;
        public TextMeshProUGUI countText;
        public Button opBtn;
        public GameObject effectNode;
        public Transform iconEffectParent;
        public Transform seasonParent;
        public SpriteAtlas atlas;
        public DhImage tipsIcon;
        public GameObject tipsNode;
        
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<CanClaimItemView, CanClaimItemViewModel>();
            bindingSet.Bind(bgImg).For(v => v.sprite).To(vm => vm.BgImgPath).WithConversion(this);
            bindingSet.Bind(icon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);
            bindingSet.Bind(partIcon.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowPartIcon);
            bindingSet.Bind(partIcon).For(v => v.sprite).To(vm => vm.PartIconPath).WithConversion(this);
            bindingSet.Bind(countText).For(v => v.text).To(vm => vm.CountStr);
            bindingSet.Bind(countText.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowCount);
            bindingSet.Bind(stateDot).For(v => v.activeSelf).To(vm => vm.IsClaimed);
            bindingSet.Bind(opBtn).For(v => v.onClick).To(vm => vm.OnClickIconBtn).CommandParameter(() => new Tuple<Vector3, Vector3>(icon.transform.position, new Vector3(0,20,0)));
            bindingSet.Bind(bgImg.transform).For(v => v.localScale).To(vm => vm.LocalScale);
            bindingSet.Bind(stateDot.transform).For(v => v.localScale).To(vm => vm.LocalScale);
            bindingSet.Bind(effectNode).For(v=>v.activeSelf).To(vm=>vm.IsShowEffect);
            bindingSet.Bind(this).For(v => v.iconEffectParent).To(vm => vm.IconEffectParent).OneWayToSource();
            bindingSet.Bind(this).For(v => v.atlas).To(vm => vm.SeasonAtlas).OneWayToSource();
            bindingSet.Bind(this).For(v => v.seasonParent).To(vm => vm.SeasonParent).OneWayToSource();
            bindingSet.Bind(tipsIcon).For(v => v.sprite).To(vm => vm.TipsIconPath).WithConversion(this);
            bindingSet.Bind(tipsNode).For(v => v.activeSelf).To(vm => vm.IsShowTips);
            bindingSet.Build();
        }
    }
}