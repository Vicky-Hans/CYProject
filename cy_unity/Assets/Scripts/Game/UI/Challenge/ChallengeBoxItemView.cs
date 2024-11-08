using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using Cysharp.Threading.Tasks;
using DH.Data;
using UnityEngine;
namespace DH.Game.UIViews
{
    public partial class ChallengeBoxItemView  : BaseItemView
    {
        public override bool FullScreen => false;
        public DhImage normalIcon;
        public DhImage claimedIcon;
        public GameObject awardEffect;
        public DhButton btnBox;
        public DhText progressNum;
        public RectTransform boxRectTransform;
        
        public int index;
        public object Key => index;
        public override async UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<ChallengeBoxItemView, ChallengeBoxItemViewModel>();
            bindingSet.Bind(progressNum).For(v => v.text).To(vm => vm.ProgressNumStr);
            bindingSet.Bind(btnBox).For(v => v.onClick).To(vm => vm.OnClickBoxCommand);
            bindingSet.Bind(awardEffect).For(v => v.activeSelf).ToExpression(vm => vm.BoxState == EBoxState.Wait);
            bindingSet.Bind(normalIcon.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.BoxState != EBoxState.Open);
            bindingSet.Bind(claimedIcon.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.BoxState ==  EBoxState.Open);
            bindingSet.Bind(normalIcon).For(v => v.sprite).To(vm => vm.NormalIconStr).WithConversion(this);
            bindingSet.Bind(claimedIcon).For(v => v.sprite).To(vm => vm.ClaimedIconStr).WithConversion(this);
            bindingSet.Bind(this).For(v => v.boxRectTransform).To(vm => vm.BoxRectTransform).OneWayToSource();
            bindingSet.Build();
        }
    }
}