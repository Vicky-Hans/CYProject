using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class MailCellItemView:BaseItemView
    {
        public DhImage giftIcon;
        public DhImage giftIconOpen;
        public GameObject redDot;
        public DhText titleText;
        public DhText timeText;
        public DhButton mailBtn;
        
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<MailCellItemView, MailCellItemViewModel>();
            bindingSet.Bind(giftIcon.gameObject).For(v => v.activeSelf).To(vm => vm.IsHaveGift);
            bindingSet.Bind(giftIconOpen.gameObject).For(v => v.activeSelf).To(vm => vm.IsHaveGiftOpen);
            bindingSet.Bind(redDot).For(v => v.activeSelf).To(vm => vm.IsNew);
            bindingSet.Bind(titleText).For(v=>v.text).To(vm => vm.TitleStr);
            bindingSet.Bind(timeText).For(v=>v.text).To(vm => vm.TimeStr);
            bindingSet.Bind(mailBtn).For(v=>v.onClick).To(vm => vm.OnClickMailBtnCommand);
            bindingSet.Build();
        }
    }
}