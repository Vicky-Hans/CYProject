using DH.Game.ViewModels;
using DH.UIFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game.UIViews.ItemViews
{
    public partial class LanguageItemView : BaseItemView
    {
        public TextMeshProUGUI languageName;
        public GameObject selectBg;
        public GameObject checkFlag;
        public Button selectBtn;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            
            // var bindingSet = this.CreateBindingSet<LanguageItemView,LanguageItemModel>();
            // bindingSet.Bind(this.languageName).For(v => v.text).To(vm => vm.Name);
            // bindingSet.Bind(this.selectBg).For(v => v.activeSelf).To(vm => vm.Selected);
            // bindingSet.Bind(this.checkFlag).For(v => v.activeSelf).To(vm => vm.Selected);
            // bindingSet.Bind(this.selectBtn).For(v => v.onClick).To(vm => vm.SelectCmd);
            // bindingSet.Build();
        }

    }
}