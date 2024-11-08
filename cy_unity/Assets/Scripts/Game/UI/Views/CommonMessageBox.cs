using DH.Game.ViewModels;
using DH.UIFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public  partial class CommonMessageBox : BaseView
    {
        public RectTransform bgRect;
        public TextMeshProUGUI Title;
        public TextMeshProUGUI texMessage;
        public Button BlackBkg;
        public Button btnCancel;
        public GameObject btnCancelObj;
        public Button btnConfirm;
        public GameObject btnConfirmObj;
        public TextMeshProUGUI texCancel;
        public TextMeshProUGUI texConfirm;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            
            var bindingSet = this.CreateBindingSet<CommonMessageBox,CommonMessageBoxViewModel>();
            bindingSet.Bind(bgRect).For(v => v.sizeDelta).To(vm => vm.BgSize);
            bindingSet.Bind(Title).For(v => v.text).To(vm => vm.MsgTitle);
            bindingSet.Bind(texMessage).For(v => v.text).ToExpression(vm => vm.MsgContent);
            bindingSet.Bind(texCancel).For(v => v.text).ToExpression(vm => vm.CancelTxt);
            bindingSet.Bind(texConfirm).For(v => v.text).ToExpression(vm => vm.ConfirmTxt);
            bindingSet.Bind(BlackBkg).For(v => v.onClick).To(vm => vm.HandleClose);
            bindingSet.Bind(btnCancel).For(v => v.onClick).To(vm => vm.HandleCancel);
            bindingSet.Bind(btnCancelObj).For(v => v.activeSelf).To(vm => vm.HasCancelBtn);
            bindingSet.Bind(btnConfirm).For(v => v.onClick).To(vm => vm.HandleConfirm);
            bindingSet.Bind(btnConfirmObj).For(v => v.activeSelf).To(vm => vm.HasConfirmBtn);
            
            bindingSet.Build();
        }
        public override bool OnPhysicExit()
        {
            return true;
        }
    }
}