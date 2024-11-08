using Cysharp.Threading.Tasks;
using DH.Game.ViewModels;
using DH.UIFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game.UIViews.ItemViews
{
    public partial class TabBtnTitleView : BaseItemView
    {
        public Button btn;
        public TextMeshProUGUI nameOffTxt;
        public TextMeshProUGUI nameOnTxt;
        public GameObject onObj;
        public GameObject offObj;
        public GameObject redDot;
        public override async UniTask Create()
        {
            await base.Create();
            var bindSet = this.CreateBindingSet<TabBtnTitleView, TabBtnTitleModel>();
            bindSet.Bind(nameOffTxt).For(v => v.text).To(vm => vm.Name);
            bindSet.Bind(nameOnTxt).For(v => v.text).To(vm => vm.Name);
            bindSet.Bind(btn).For(v => v.onClick).To(vm => vm.OnClickCommand);
            bindSet.Bind(onObj.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.SelectPos == vm.Pos);
            bindSet.Bind(offObj.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.SelectPos != vm.Pos);
            bindSet.Bind(redDot).For(v => v.activeSelf).To(vm => vm.ShowRedDot);
            bindSet.Build();
            
        }
      
    }
}