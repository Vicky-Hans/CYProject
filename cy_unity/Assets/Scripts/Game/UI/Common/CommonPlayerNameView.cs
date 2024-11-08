using DG.Tweening;
using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class CommonPlayerNameView : BaseItemView
    {
        public override bool FullScreen => false;
        public Text nameText;
        public Text gNameText;
        public RectTransform lightImg;
        public float moveSpeed = 400f;
        
        private Tweener lightTween;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<CommonPlayerNameView, CommonPlayerNameViewModel>();
            bindingSet.Bind(nameText).For(v => v.text).To(vm => vm.NameTextStr);
            bindingSet.Bind(nameText).For(v => v.color).To(vm => vm.NameTextColor);
            bindingSet.Bind(gNameText).For(v => v.text).To(vm => vm.NameTextStr);
            bindingSet.Bind(nameText.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowGName);
            bindingSet.Bind(gNameText.gameObject).For(v => v.activeSelf).ToExpression(vm => UpdateLightImg(vm.IsShowGName));
            bindingSet.Build();
        }
        
        private bool UpdateLightImg(bool isShow)
        {
            if (!isShow)
            {
                lightTween.Kill();
                lightImg.gameObject.SetActive(false);
                return false;   
            }
            var moveDistance = 1000;
            var time = moveDistance/moveSpeed;
            var delayTime = 3f;
            lightImg.anchoredPosition = Vector3.zero;
            lightImg.gameObject.SetActive(true);
            lightTween = lightImg.DOLocalMoveX(moveDistance, time)
                .SetDelay(delayTime)
                .OnComplete(() =>
                {
                    UpdateLightImg(true);
                });
            
            return true;   
        }
    }
}