using DH.Config;
using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class EquipInfoView : BaseView
    {
        public override bool FullScreen => false;

        public GameObject bgNode;
        public GameObject bgLockNode;
        public DhButton btnClose;
		public DhText name;
		public DhText nameGray;
		public EquipItemView equipItemView;
		public DhButton btnSkin;
		public GameObject lockIcon;
		public ScrollRectExtend scrollViewAttr;
		[AssetPath]public string scrollViewAttrCell;
		public UICircularScrollView scrollView;
		[AssetPath]public string scrollViewCell;
		public DhButton button;
		public DhButton buttonNot;
		public ItemPriceNodeView itemPriceNode;
		public ItemPriceNodeView itemPriceNodeNot;
		public DhText lockTips;
		public GameObject maxTips;
		public EquipUnOwnItemView equipUnOwnItemView;
		public CommonTopView commonTopView;
		public ParticleSystem effectUpLevel;
		public DhImage equipTypeIcon;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollView.PrefabPath = scrollViewCell;
			scrollViewAttr.PrefabPath = scrollViewAttrCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<EquipInfoView, EquipInfoViewModel>();
            bindingSet.Bind(equipTypeIcon).For(v => v.sprite).To(vm => vm.EquipTypePath).WithConversion(this);
            bindingSet.Bind(bgNode).For(v => v.activeSelf).To(vm => vm.IsOwn);
            bindingSet.Bind(bgLockNode).For(v => v.activeSelf).ToExpression(vm => !vm.IsOwn);
            
			bindingSet.Bind(name).For(v => v.text).To(vm => vm.NameStr);
			bindingSet.Bind(nameGray).For(v => v.text).To(vm => vm.NameStr);
			bindingSet.Bind(equipItemView.BindingContext).For(v => v.DataContext).To(vm => vm.EquipItemViewVm);
			bindingSet.Bind(equipUnOwnItemView.BindingContext).For(v => v.DataContext).To(vm => vm.EquipUnOwnItemViewModel);
			bindingSet.Bind(btnSkin).For(v => v.onClick).To(vm => vm.OnClickBtnSkinCommand);
			bindingSet.Bind(btnSkin.gameObject).For(v => v.activeSelf).To(vm => vm.IsOwn);
			bindingSet.Bind(lockIcon).For(v => v.activeSelf).ToExpression(vm => !vm.IsOwn);
			
			bindingSet.Bind(scrollViewAttr).For(v => v.Collection).To(vm => vm.EquipAttrItemViewModelList);
			bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.EquipSkillItemList);
			bindingSet.Bind(button).For(v => v.onClick).To(vm => vm.OnClickButtonCommand);
			bindingSet.Bind(buttonNot).For(v => v.onClick).To(vm => vm.OnClickButtonCommand);
			bindingSet.Bind(itemPriceNode.BindingContext).For(v => v.DataContext).To(vm => vm.ItemPriceNodeVm);
			bindingSet.Bind(itemPriceNodeNot.BindingContext).For(v => v.DataContext).To(vm => vm.ItemPriceNodeVm);
			
			bindingSet.Bind(button.gameObject).For(v => v.activeSelf).ToExpression(vm =>!vm.IsCloseButton && vm.IsEnoughItem && vm.IsOwn && !vm.IsMaxLevel);
			bindingSet.Bind(buttonNot.gameObject).For(v => v.activeSelf).ToExpression(vm =>!vm.IsCloseButton && !vm.IsEnoughItem && vm.IsOwn && !vm.IsMaxLevel);
			
			
			bindingSet.Bind(lockTips.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsOwn);
			bindingSet.Bind(lockTips).For(v => v.text).ToExpression(vm => GetLockTips(vm.Cfg.Unlock));
			bindingSet.Bind(maxTips).For(v => v.activeSelf).ToExpression(vm => vm.IsOwn && vm.IsMaxLevel);
			bindingSet.Bind(btnClose).For(v => v.onClick).To(vm => vm.OnClickCloseCommand);
			bindingSet.Bind(commonTopView.BindingContext).For(v => v.DataContext).To(vm => vm.CommonTopViewModel);
			bindingSet.Bind(this).For(v => v.effectUpLevel).To(vm => vm.UpLevelSucceedEffect).OneWayToSource();
            bindingSet.Build();
        }
        
        private string GetLockTips(int chapterId)
        {
	        return LocalizeHelper.GetGlobal(GlobalLanguageId.Equip_04, chapterId); //$"通过章节{chapterId}解锁";
        } 
    }
}