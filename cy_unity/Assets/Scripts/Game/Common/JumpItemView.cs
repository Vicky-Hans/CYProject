using DH.UIFramework;
using Extend;
namespace Game.UI.CommonView
{
    public partial class JumpItemView : BaseItemView
    {
        public DhText titleTxt;
        public DhButton opBtn;
        public DhText opBtnText;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindSet = this.CreateBindingSet<JumpItemView, JumpItemViewModel>();
            bindSet.Bind(opBtn).For(v => v.onClick).To(vm => vm.OnClickOpBtn);
            bindSet.Bind(titleTxt).For(v => v.text).To(vm => vm.TitleName);
            bindSet.Build();
        }
    }
}