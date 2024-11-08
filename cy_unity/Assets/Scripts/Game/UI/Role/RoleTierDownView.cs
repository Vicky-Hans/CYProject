using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class RoleTierDownView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhText des;
		public DhButton closeBut;
		public DhImage quilityBg;
		public DhImage heroHead;
		public UICircularScrollView awardScrollview;
		[AssetPath]public string awardScrollviewCell;
		public DhButton sureButton;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			awardScrollview.PrefabPath = awardScrollviewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<RoleTierDownView, RoleTierDownViewModel>();
            
			bindingSet.Bind(des).For(v => v.text).To(vm => vm.DesStr);
			bindingSet.Bind(closeBut).For(v => v.onClick).To(vm => vm.OnClickCloseButCommand);
			bindingSet.Bind(quilityBg).For(v => v.sprite).To(vm => vm.QuilityBgPath).WithConversion(this);
			bindingSet.Bind(heroHead).For(v => v.sprite).To(vm => vm.HeroHeadPath).WithConversion(this);
			bindingSet.Bind(awardScrollview).For(v => v.Collection).To(vm => vm.AwardScrollviewList);
			bindingSet.Bind(sureButton).For(v => v.onClick).To(vm => vm.OnClickSureButtonCommand);

            bindingSet.Build();
        }
    }
}