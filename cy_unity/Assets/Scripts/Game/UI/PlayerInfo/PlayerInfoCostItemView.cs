using DH.Game.ViewModels;
using DH.UIFramework;
using TMPro;
using UnityEngine.UI;

namespace DH.Game.UIViews.ItemViews
{
    public partial class PlayerInfoCostItemView : BaseItemView
    {
        public Image icon;
        public TextMeshProUGUI countText;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<PlayerInfoCostItemView, PlayerInfoCostItemViewModel>();
            bindingSet.Bind(icon).For(v=>v.sprite).To(vm => vm.IconPath).WithConversion(this);
            bindingSet.Bind(this.countText).For(v => v.text).To(vm => vm.Count);

            bindingSet.Build();
        }
    }
}