using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;

namespace DH.Game.UIViews
{
    public partial class SubscribeGiftView : BaseView
    {
        public override bool FullScreen => false;
        
		public UICircularScrollView awardScrollview;
		[AssetPath]public string awardScrollviewCell;
		public DhText keyText;
		public DhButton gOTOBtn;
		public DhButton close;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			awardScrollview.PrefabPath = awardScrollviewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<SubscribeGiftView, SubscribeGiftViewModel>();
            
			bindingSet.Bind(awardScrollview).For(v => v.Collection).To(vm => vm.AwardScrollviewList);
			bindingSet.Bind(keyText).For(v => v.text).To(vm => vm.KeyTextStr);
			bindingSet.Bind(gOTOBtn).For(v => v.onClick).To(vm => vm.OnClickGOTOBtnCommand);
			bindingSet.Bind(close).For(v => v.onClick).To(vm => vm.Close);
            bindingSet.Build();
        }
    }
}