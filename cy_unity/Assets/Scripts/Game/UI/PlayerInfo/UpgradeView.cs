using DH.Game.ViewModels;
using DH.UIFramework;
using TMPro;
using Extend;
namespace DH.Game.UIViews
{
    public partial class UpgradeView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhText levelNums;
		public TextMeshProUGUI defaultLevel;
		public TextMeshProUGUI curLevel;
		public UICircularScrollView awardScrollview;
		[AssetPath]public string awardScrollviewCell;
		public DhButton closeBut;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			awardScrollview.PrefabPath = awardScrollviewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<UpgradeView, UpgradeViewModel>();
            
			bindingSet.Bind(levelNums).For(v => v.text).To(vm => vm.LevelNumsStr);
			bindingSet.Bind(defaultLevel).For(v => v.text).To(vm => vm.DefaultLevelStr);
			bindingSet.Bind(curLevel).For(v => v.text).To(vm => vm.CurLevelStr);
			bindingSet.Bind(awardScrollview).For(v => v.Collection).To(vm => vm.AwardsList);
			bindingSet.Bind(closeBut).For(v => v.onClick).To(vm => vm.OnClose);
            bindingSet.Build();
            AudioManager.Instance.PlayLevelUp();
        }
    }
}