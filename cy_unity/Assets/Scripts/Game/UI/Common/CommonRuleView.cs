using DH.Game.ViewModels;
using DH.UIFramework;
using TMPro;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class CommonRuleView : BaseView
    {
        public override bool FullScreen => false;
        public Button closeBtn;
        public TextMeshProUGUI titleText;
        public ScrollRectExtend scrollView;
        [AssetPath] public string cellPrefab;
        
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            scrollView.PrefabPath = cellPrefab;
            await base.Create();
            var bindingSet = this.CreateBindingSet<CommonRuleView, CommonRuleViewModel>();
            bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtnCommand);
            bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.RuleInfo);
            bindingSet.Bind(titleText).For(v => v.text).To(vm => vm.TitleStr);
            //bindingSet.Bind(bgRect).For(v => v.sizeDelta).To(vm => vm.BgSize);
            bindingSet.Build();
        }
    }
}