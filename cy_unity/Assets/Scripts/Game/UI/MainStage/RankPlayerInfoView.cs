using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class RankPlayerInfoView : BaseView
    {
        public override bool FullScreen => false;
		public DhButton closeBtn;
		public CommonHeadItemView headView;
		public CommonPlayerNameView commonPlayerNameView;
		public DhText groupDescText;
		public DhText stageName;
		public GameObject playerModelParent;
		public GameObject weaponBgParentNode;
		public GameObject weaponPatent;
		public DhText weaponCountText;
		public UICircularScrollView weaponScrollview;
		[AssetPath]public string weaponScrollviewCell;
		public UICircularScrollView clothesScrollview;
		[AssetPath]public string clothesScrollviewCell;
		public GameObject roleSpineGo;
		public GameObject noWeaponNode;
		public GameObject noClothesNode;
		public GameObject noEquipNode;
		
		
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
	        weaponScrollview.PrefabPath = weaponScrollviewCell;
	        clothesScrollview.PrefabPath = clothesScrollviewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<RankPlayerInfoView, RankPlayerInfoViewModel>();
			bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtnCommand);
			bindingSet.Bind(headView.BindingContext).For(v => v.DataContext).To(vm => vm.HeadVm);
			bindingSet.Bind(commonPlayerNameView.BindingContext).For(v => v.DataContext).To(vm => vm.CommonPlayerNameVm);
			bindingSet.Bind(stageName).For(v => v.text).To(vm => vm.StageNameStr);
			bindingSet.Bind(groupDescText).For(v => v.text).To(vm => vm.GroupDescStr);
			bindingSet.Bind(this).For(v=>v.weaponBgParentNode).To(vm=>vm.WeaponBgParentNode).OneWayToSource();
			bindingSet.Bind(this).For(v => v.weaponPatent).To(vm => vm.WeaponParentNode).OneWayToSource();
			bindingSet.Bind(weaponBgParentNode).For(v => v.activeSelf).To(vm => vm.IsShowWeapon);
			bindingSet.Bind(weaponPatent).For(v => v.activeSelf).To(vm => vm.IsShowWeapon);
			bindingSet.Bind(noWeaponNode).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowWeapon);
			bindingSet.Bind(weaponCountText).For(v => v.text).To(vm => vm.WeaponCountStr);
			bindingSet.Bind(weaponScrollview).For(v => v.Collection).To(vm => vm.WeaponScrollviewList);
			bindingSet.Bind(clothesScrollview).For(v => v.Collection).To(vm => vm.ClothesScrollviewList);
			bindingSet.Bind(this).For(v => v.roleSpineGo).To(vm => vm.EffectParentNode).OneWayToSource();
			bindingSet.Bind(weaponScrollview.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowEquipNode);
			bindingSet.Bind(noEquipNode).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowEquipNode);
			bindingSet.Bind(clothesScrollview.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowClothes);
			bindingSet.Bind(noClothesNode).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowClothes);

			
			
			
            bindingSet.Build();
        }
    }
}