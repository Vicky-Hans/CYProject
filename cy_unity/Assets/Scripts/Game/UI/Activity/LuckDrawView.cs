using DH.Config;
using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using Extend;
using Spine.Unity;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class LuckDrawView : BaseView
    {
        public override bool FullScreen => false;
        
		public CommonTopView commonTopItems;
		public UICircularScrollView rewardScrollView;
		[AssetPath]public string rewardScrollViewCell;
		public DhButton leftOpBtn;
		public DhText leftOpBtnText;
		public ItemPriceNodeView leftPriceView;
		public DhButton rightOpBtn;
		public DhText rightOpBtnText;
		public ItemPriceNodeView rightPriceView;
		public DhButton adBtn;
		// public DhText adTips;
		public DhButton skipBtn;
		public DhImage skipBtnIcon;
		public BottomComponentView bottomComponent;
		public StaticItemsBindComponent randomParentInfoNode;
		public DhText leftCountText;
		public DhButton infoBtn;
		public DhText progressText;
		public DhText timeText;
		public SkeletonGraphic actionSkele;
		public Transform numberEffectNode;
		public CommonAdvIconView commonAdv;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
			rewardScrollView.PrefabPath = rewardScrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<LuckDrawView, LuckDrawViewModel>();
			bindingSet.Bind(commonTopItems.BindingContext).For(v => v.DataContext).To(vm => vm.CommonTopItemsVm);
			bindingSet.Bind(rewardScrollView).For(v => v.Collection).To(vm => vm.RewardScrollViewList);
			bindingSet.Bind(rewardScrollView).For(v => v.DefaultJumpIndex).To(vm => vm.RewardScrollViewJumpIndex);
			bindingSet.Bind(this).For(v => v.ShowIndex).To(vm => vm.RewardScrollViewJumpIndex);
			bindingSet.Bind(leftOpBtn).For(v => v.onClick).To(vm => vm.OnClickLeftOpBtnCommand);
			bindingSet.Bind(leftOpBtn).For(v => v.interactable).To(vm => vm.IsCanOpLeftBtn);
			bindingSet.Bind(leftOpBtn.image).For(v=>v.sprite).To(vm=>vm.LeftOpBtnImgPath).WithConversion(this);
			bindingSet.Bind(leftPriceView.BindingContext).For(v => v.DataContext).To(vm => vm.LeftPriceViewVm);
			bindingSet.Bind(rightOpBtn).For(v => v.onClick).To(vm => vm.OnClickRightOpBtnCommand);
			bindingSet.Bind(rightOpBtn).For(v => v.interactable).To(vm => vm.IsCanOpRightBtn);
			bindingSet.Bind(rightOpBtn.image).For(v=>v.sprite).To(vm=>vm.RightOpBtnImgPath).WithConversion(this);
			bindingSet.Bind(rightPriceView.BindingContext).For(v => v.DataContext).To(vm => vm.RightPriceViewVm);
			bindingSet.Bind(adBtn).For(v => v.onClick).To(vm => vm.OnClickAdBtnCommand);
			bindingSet.Bind(adBtn).For(v => v.interactable).To(vm => vm.IsCanOpAdBtn);
			bindingSet.Bind(adBtn.image).For(v => v.sprite).To(vm => vm.AdBtnImgPath).WithConversion(this);
			// bindingSet.Bind(adTips).For(v => v.text).To(vm => vm.AdTipsStr);
			bindingSet.Bind(bottomComponent.BindingContext).For(v => v.DataContext).To(vm => vm.BottomComponentVm);
			bindingSet.Bind(skipBtn).For(v=>v.onClick).To(vm=>vm.OnClickSkipBtnCommand);
			bindingSet.Bind(skipBtnIcon.gameObject).For(v=>v.activeSelf).To(vm=>vm.IsShowSkipBtnIcon);
			bindingSet.Bind(randomParentInfoNode).For(v=>v.BindDictionaryGetValueFunc).To(vm=>vm.GetInfoCellVmCallBack);
			bindingSet.Bind(randomParentInfoNode).For(v => v.Collection).To(vm => vm.RandomInfoDictionary);
			bindingSet.Bind(leftCountText).For(v=>v.text).To(vm=>vm.LeftCountStr);
			bindingSet.Bind(infoBtn).For(v => v.onClick).To(vm => vm.OnClickInfoBtnCommand);
			bindingSet.Bind(progressText).For(v=>v.text).To(vm=>vm.ProgressStr);
			bindingSet.Bind(timeText).For(v=>v.text).To(vm=>vm.TimeStr);
			bindingSet.Bind(this).For(v => v.actionSkele).To(vm => vm.ActionSkele).OneWayToSource();
			bindingSet.Bind(this).For(v => v.numberEffectNode).To(vm => vm.NumberEffectNode).OneWayToSource();
			bindingSet.Bind(commonAdv.BindingContext).For(v => v.DataContext).To(vm => vm.CommonAdvVm);
            bindingSet.Build();

            var leftStr = LocalizeHelper.GetGlobal(GlobalLanguageId.xingyun_05,"1");
            var rightStr = LocalizeHelper.GetGlobal(GlobalLanguageId.xingyun_05, "5");
            leftOpBtnText.text = leftStr;
            rightOpBtnText.text = rightStr;
        }
        
        private int showIndex;

        public int ShowIndex
        {
	        get => 0;
	        set => RewardListScrollToPos(value);
        }

        private void RewardListScrollToPos(int index)
        {
	        rewardScrollView.Jump2SpecificItem(index);
        }
    }
}