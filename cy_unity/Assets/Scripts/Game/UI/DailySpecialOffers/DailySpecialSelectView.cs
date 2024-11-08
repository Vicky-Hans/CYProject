using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class DailySpecialSelectView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhText titile;
		public DhImage equipIcon;
		public DhButton close;
		public DhButton infoBut;
		public UICircularScrollView equipScrollview;
		[AssetPath]public string equipScrollviewCell;
		public DhButton button;
		public GameObject selectGo;
		public GameObject lockGo;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			equipScrollview.PrefabPath = equipScrollviewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<DailySpecialSelectView, DailySpecialSelectViewModel>();
            
			bindingSet.Bind(titile).For(v => v.text).To(vm => vm.TitileStr);
			bindingSet.Bind(equipIcon).For(v => v.sprite).To(vm => vm.EquipIconPath).WithConversion(this);
			bindingSet.Bind(close).For(v => v.onClick).To(vm => vm.OnClickCloseCommand);
			bindingSet.Bind(infoBut).For(v => v.onClick).To(vm => vm.OnClickInfoButCommand);
			bindingSet.Bind(equipScrollview).For(v => v.Collection).To(vm => vm.EquipScrollviewList);
			bindingSet.Bind(button).For(v => v.onClick).To(vm => vm.OnClickButtonCommand);
			bindingSet.Bind(selectGo).For(v => v.activeSelf).ToExpression(vm => vm.IsShowSelect);
			bindingSet.Bind(button.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowSelect && !vm.LockGo);
			bindingSet.Bind(lockGo).For(v => v.activeSelf).ToExpression(vm => vm.LockGo);
            bindingSet.Build();
        }
    }
}