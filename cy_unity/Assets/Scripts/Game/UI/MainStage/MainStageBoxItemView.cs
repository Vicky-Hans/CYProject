using DH.Data;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;
using Spine.Unity;
using TMPro;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class MainStageBoxItemView : BaseItemView,IViewKey
    {
        public bool fullScreen => false;
        public Button boxBtn;
        public TextMeshProUGUI tipsText;
        public RectTransform rectTransform;
        public SkeletonGraphic boxAnim;
        private EBoxState state;
        public DhImage boxImg;
        public GameObject effectNode;
        public int index;

        public object Key => index;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<MainStageBoxItemView, MainStageBoxItemViewModel>();
            bindingSet.Bind(tipsText.gameObject).For(v => v.activeSelf).To(vm =>vm.IsShowTips);
            bindingSet.Bind(tipsText).For(v => v.text).To(vm =>vm.Desc);
            // bindingSet.Bind(tipsText).For(v => v.color).To(vm => vm.DescColor);
            bindingSet.Bind(boxBtn).For(v => v.onClick).To(vm => vm.OnClickBoxBtn);
            bindingSet.Bind(boxImg).For(v => v.sprite).To(vm => vm.BoxImgPath).WithConversion(this);
            bindingSet.Bind(this).For(v => v.rectTransform).To(vm => vm.BoxPosTransform).OneWayToSource();
            bindingSet.Bind(this).For(v => v.State).To(vm => vm.BoxState);
            bindingSet.Build();
        }
        
        public EBoxState State
        {
            get => state;
            set
            {
                state = value;
                
                if (state == EBoxState.Close)
                {
                    boxImg.gameObject.SetActive(true);
                    boxAnim.gameObject.SetActive(false);
                    effectNode.SetActive(false);
                }
                else if (state == EBoxState.Wait)
                {
                    var anim = "kelingqu";
                    boxAnim.AnimationState.SetAnimation(0, anim, true);
                    boxImg.gameObject.SetActive(false);
                    boxAnim.gameObject.SetActive(true);
                    effectNode.SetActive(true);
       
                } else if (state == EBoxState.Open)
                {
                    boxImg.gameObject.SetActive(true);
                    boxAnim.gameObject.SetActive(false);
                    effectNode.SetActive(false);
                }

            }
        }

        
    }
}