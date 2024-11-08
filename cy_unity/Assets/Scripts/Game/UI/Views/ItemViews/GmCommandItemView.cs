using DH.Game.ViewModels;
using DH.UIFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game.UIViews.ItemViews
{
    public partial class GmCommandItemView: BaseItemView
    {
        public TMP_Text desc;
        public Button selectBtn;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            
            var bindingSet = this.CreateBindingSet<GmCommandItemView,GmViewModel.GmCommandItemModel>();
            bindingSet.Bind(this.desc).For(v => v.text).To(vm => vm.Desc);
            bindingSet.Bind(this.selectBtn).For(v => v.onClick).To(vm => vm.SelectCmd);
            bindingSet.Build();
        }

    }
}