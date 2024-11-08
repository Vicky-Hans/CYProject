using DH.Game.ViewModels;
using DH.UIFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class CostConfirmView : BaseView
    {
        public Button closeBtn;
        public Button leftBtn;
        public Image leftBtnIcon;
        public TextMeshProUGUI leftBtnText;
        public Button rightBtn;
        public Image rightBtnIcon;
        public TextMeshProUGUI rightBtnText;
        public Image itemIcon;
        public TextMeshProUGUI costText;
        public GameObject descWithIconNode;
        public GameObject descNoIconNode;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<CostConfirmView, CostConfirmViewModel>();
            bindingSet.Bind(leftBtn).For(v => v.onClick).To(vm => vm.OnClickLeftBtn);
            bindingSet.Bind(leftBtnIcon).For(v => v.sprite).To(vm => vm.LeftBtnIconPath).WithConversion(this);
            bindingSet.Bind(leftBtnIcon.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowLeftBtnIcon);
            bindingSet.Bind(leftBtnText).For(v => v.text).To(vm => vm.LeftBtnStr);
            bindingSet.Bind(leftBtnText).For(v => v.color).To(vm => vm.LeftBtnStrColor);
            bindingSet.Bind(rightBtn).For(v => v.onClick).To(vm => vm.OnClickRightBtn);
            bindingSet.Bind(rightBtnIcon).For(v => v.sprite).To(vm => vm.RightBtnIconPath).WithConversion(this);
            bindingSet.Bind(rightBtnIcon.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowRightBtnIcon);
            bindingSet.Bind(rightBtnText).For(v => v.text).To(vm => vm.RightBtnStr);
            bindingSet.Bind(rightBtnText).For(v => v.color).To(vm => vm.RightBtnStrColor);
            bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnCloseBtn);
            
            bindingSet.Bind(itemIcon).For(v => v.sprite).To(vm => vm.CostIconImgPath).WithConversion(this);
            bindingSet.Bind(costText).For(v => v.text).To(vm => vm.CostValueStr);
            bindingSet.Bind(descWithIconNode).For(v => v.activeSelf).To(vm => vm.IsShowDescWithIcon);
            bindingSet.Bind(descNoIconNode).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowDescWithIcon);
            bindingSet.Build();
        }
    }
}