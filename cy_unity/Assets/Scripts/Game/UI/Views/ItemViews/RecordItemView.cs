using DH.Game.ViewModels;
using DH.UIFramework;
using TMPro;
using UnityEngine.UI;

namespace DH.Game.UIViews.ItemViews
{
    public partial class RecordItemView : BaseItemView
    {
        public TextMeshProUGUI recordTxt;
        public Button recordBtn;
        
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<RecordItemView, RecordItemViewModel>();
            bindingSet.Bind(recordTxt).For(v => v.text).To(vm=>vm.RecordStr);
            bindingSet.Bind(recordBtn).For(v => v.onClick).To(vm=>vm.OnClickRecordBtnCommand);
            bindingSet.Build();
        }
    }
}