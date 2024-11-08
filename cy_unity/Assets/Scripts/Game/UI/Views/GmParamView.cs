using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using TMPro;
namespace DH.Game.UIViews
{
    public partial class GmParamView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public TMP_InputField inputParam;
		public TextMeshProUGUI placeholder;
        public DhButton btnSelect;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<GmParamView, GmParamViewModel>();
            bindingSet.Bind(this).For(v => v.inputParam).To(vm => vm.InputParamField).OneWayToSource();
			bindingSet.Bind(placeholder).For(v => v.text).To(vm => vm.PlaceholderStr);
            bindingSet.Bind(btnSelect.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ParamType != GmParamType.Base );
            bindingSet.Bind(btnSelect).For(v => v.onClick).To(vm => vm.OnClockSelectCommand);
            bindingSet.Build();
        }
    }
}