using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class SelectLanguageView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhButton closeBtn;
        public UICircularScrollView languageList;
        [AssetPath] public string prefabPath;
        
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            languageList.PrefabPath = prefabPath;
            await base.Create();
            var bindingSet = this.CreateBindingSet<SelectLanguageView, SelectLanguageViewModel>();
            bindingSet.Bind(languageList).For(v => v.Collection).To(vm => vm.LanguageItems);
			bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtnCommand);
            bindingSet.Build();
        }
    }
}