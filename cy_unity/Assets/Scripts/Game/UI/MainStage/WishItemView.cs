using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Game.UI;

namespace DH.Game.UIViews
{
    public partial class WishItemView : BaseItemView
    {
        public override bool FullScreen => false;
		public GameObject wishNode;
		public GameObject wishTipIcon;
		public DhImage wishItemImg;
        public RectTransform wishNodeRect;
        public GameObject lockNode;
        public GameObject effectNode;
        public RectTransform iconEffectRect;
        public DhButton adFreePlusBtn;
        
        public GameObject adTools;
        private bool isShowTools;

        public bool IsShowTools
        {
            get => isShowTools;
            set
            {
                isShowTools = value;
                if (isShowTools)
                {
                    PlayerAni();
                }
            }
        }

        public override async UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<WishItemView, WishItemViewModel>();
            bindingSet.Bind(wishNode).For(v=>v.activeSelf).ToExpression(vm=>vm.IsShowWishNode && !vm.IsShowAdFreePlus);
            bindingSet.Bind(wishNode.transform).For(v=>v.localScale).To(vm=>vm.WishNodeScale);
            // bindingSet.Bind(wishItemImg).For(v=>v.sprite).To(vm=>vm.WishItemImgPath).WithConversion(this);
            bindingSet.Bind(wishItemImg.gameObject).For(v=>v.activeSelf).ToExpression(vm=>SetWishItemIsShow(vm.IsShowWishItem, vm.WishItemImgPath));
            bindingSet.Bind(wishTipIcon).For(v => v.activeSelf).ToExpression(vm =>!vm.IsShowWishItem);
            bindingSet.Bind(this).For(v => v.wishNodeRect).To(vm =>vm.WishNodeRect).OneWayToSource();
            bindingSet.Bind(lockNode).For(v=>v.activeSelf).To(vm=>vm.IsShowLockNode);
            bindingSet.Bind(this).For(v=>v.iconEffectRect).To(vm=>vm.IconEffectRect).OneWayToSource();
            bindingSet.Bind(this).For(v => v.wishItemImg).To(vm => vm.WishIcon).OneWayToSource();
            bindingSet.Bind(this).For(v => v.wishItemImg.sprite).To(vm => vm.WishSprite).OneWayToSource();
            bindingSet.Bind(adFreePlusBtn.gameObject).For(v=>v.activeSelf).ToExpression(vm=>vm.IsShowAdFreePlus);
            bindingSet.Bind(adFreePlusBtn).For(v=>v.onClick).To(vm=>vm.OnClickAdFreePlusBtnButtonCommand);
            bindingSet.Bind(this).For(v=>v.IsShowTools).To(vm=>vm.Manager.ShowAdFreePlusAni);
            bindingSet.Build();
        }
        
        private bool SetWishItemIsShow(bool isShow, string path)
        {
            if (isShow)
            {
                effectNode.SetActive(false);
                ConvertDirectly(path,wishItemImg);
                wishItemImg.sprite = AssetsManager.LoadSpriteSync(path);
                wishItemImg.SetNativeSize();
                effectNode.SetActive(true);
            }
            else
            {
                effectNode.SetActive(false);
            }

            return isShow;
        }

        private async UniTaskVoid  PlayerAni()
        {
            adTools.gameObject.SetActive(true);
            await UniTask.Delay(2500);
            GameManager.Instance.ShowAdFreePlusAni = false;
            adTools.gameObject.SetActive(false);
        }
    }
}