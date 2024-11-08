using Cysharp.Threading.Tasks;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game.UIViews.ItemViews
{
    public partial class TabBtnView : BaseItemView
    {
        public Button btn;
        public TextMeshProUGUI nameOffTxt;
        public TextMeshProUGUI nameOnTxt;
        // public Image onIcon;
        // public Image offIcon;
        public GameObject redDot;
        public GameObject lockNode;

        public GameObject onNode;
        public GameObject offNode;
        
        public DhImage onNodeImage;
        public DhImage offNodeImage;
        public override async UniTask Create()
        {
            await base.Create();
            var bindSet = this.CreateBindingSet<TabBtnView, TabBtnViewModel>();
            bindSet.Bind(nameOffTxt).For(v => v.text).To(vm => vm.Name);
            bindSet.Bind(nameOnTxt).For(v => v.text).To(vm => vm.Name);
            bindSet.Bind(btn).For(v => v.onClick).To(vm => vm.OnClickCommand);
            bindSet.Bind(onNode.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.SelectPos == vm.Pos);
            bindSet.Bind(offNode.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.SelectPos != vm.Pos);
            bindSet.Bind(onNodeImage).For(v => v.sprite).To(vm => vm.OnIconPath).WithConversion(this);
            bindSet.Bind(offNodeImage).For(v => v.sprite).To(vm => vm.OffIconPath).WithConversion(this);
            bindSet.Bind(btn).For(v => v.onClick).To(vm => vm.OnClickCommand);
            bindSet.Bind(redDot).For(v => v.activeSelf).To(vm => vm.ShowRedDot);
            bindSet.Bind(lockNode).For(v => v.activeSelf).To(vm => vm.IsLock);
            bindSet.Build();
            
        }
      
    }
}