using DH.UIFramework;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class DebugCellView: BaseItemView
    {

        public Text desc;
        public Button cellButton;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<DebugCellView, DebugCellViewModel>();
            bindingSet.Bind(desc).For(v => v.text).To(vm => vm.Desc);
            bindingSet.Bind(cellButton).For(v => v.onClick).To(vm => vm.OnClickBtn);
            bindingSet.Build();
        }
    }
}