using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace DH.Game.UIViews.ItemViews
{
    public partial class CommonHeadItemView : BaseItemView
    {
        public GameObject headNode;
        public Image headImg;
        public Image headFrameImg;
        public Transform frameEffectTransform;
        public Transform seasonTransform;
        public SpriteAtlas atlas;
        public Button headBtn;
        public RectMask2D effectMask;
        public GameObject newHeadRed;
        public Transform dynamicParentNode;
        
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<CommonHeadItemView, CommonHeadItemViewModel>();
            bindingSet.Bind(this).For(v => v.atlas).To(vm => vm.SeasonAtlas).OneWayToSource();
            bindingSet.Bind(this).For(v => v.headNode).To(vm => vm.HeadNode).OneWayToSource();
            bindingSet.Bind(headImg).For(v => v.sprite).To(vm => vm.HeadImgPath).WithConversion(this);
            bindingSet.Bind(headFrameImg).For(v => v.sprite).To(vm => vm.HeadFrameImgPath).WithConversion(this);
            bindingSet.Bind(this).For(v => v.frameEffectTransform).To(vm => vm.FrameEffectNode).OneWayToSource();
            bindingSet.Bind(this).For(v => v.seasonTransform).To(vm => vm.SeasonNode).OneWayToSource();
            bindingSet.Bind(effectMask).For(v => v.enabled).To(vm => vm.IsShowEffectMask);
            bindingSet.Bind(headBtn).For(v => v.onClick).To(vm => vm.OnClickHeadBtnCommand);
            bindingSet.Bind(newHeadRed).For(v => v.activeSelf).To(vm => vm.NewHeadRed);
            bindingSet.Bind(this).For(v=>v.dynamicParentNode).To(vm=>vm.DynamicHeadParent).OneWayToSource();
            bindingSet.Build();
        }
    }
}