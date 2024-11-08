using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using UnityEngine;
using Extend;
using TMPro;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class MagicDrawView : BaseView
    {
        public override bool FullScreen => false;
		public UICircularScrollView rewardScrollView;
		[AssetPath]public string rewardScrollViewCell;
		public DhText progress;
		public Transform numEffectParent;
		public CommonTopView commonTopItems;
		public BottomComponentView bottomComponent;
		public Button skipBtn;
		public DhImage skipIcon;
		public DhButton adBtn;
		public CommonAdvIconView advIcon;
		public DhButton opBtn;
		public DhText opBtnText;
		public ItemPriceNodeView opBtnItem;
		public DhButton leftBtn;
		public DhButton rightBtn;
		public  TextMeshProUGUI countPointer;
		public DhText leftText;
		public DhText timeText;
		public StaticItemsBindComponent outSideRewardNode;
		public StaticItemsBindComponent inSideRewardNode;
		public GameObject turnPointer0;
		public GameObject turnPointer1;
		public GameObject turnPointer2;
		public DhButton infoBtn;
		public DhButton fundBtn;
		public DhImage fundRedDot;
		public GameObject turnNode;
		
		
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
			rewardScrollView.PrefabPath = rewardScrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<MagicDrawView, MagicDrawViewModel>();
			bindingSet.Bind(rewardScrollView).For(v => v.Collection).To(vm => vm.RewardScrollViewList);
			bindingSet.Bind(rewardScrollView).For(v => v.DefaultJumpIndex).To(vm => vm.RewardScrollViewJumpIndex);
			bindingSet.Bind(this).For(v => v.ShowIndex).To(vm => vm.RewardScrollViewJumpIndex);
			bindingSet.Bind(progress).For(v => v.text).To(vm => vm.ProgressStr);
			bindingSet.Bind(numEffectParent.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowNumEffectParent);
			bindingSet.Bind(commonTopItems.BindingContext).For(v => v.DataContext).To(vm => vm.CommonTopItemsVm);
			bindingSet.Bind(bottomComponent.BindingContext).For(v => v.DataContext).To(vm => vm.BottomComponentVm);
			bindingSet.Bind(skipBtn).For(v => v.onClick).To(vm => vm.OnClickSkipBtnCommand);
			bindingSet.Bind(skipIcon.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowSkipIcon);
			bindingSet.Bind(this).For(v =>v.numEffectParent ).To(vm => vm.NumberEffectNode).OneWayToSource();
			bindingSet.Bind(adBtn).For(v => v.onClick).To(vm => vm.OnClickAdBtnCommand);
			bindingSet.Bind(adBtn).For(v => v.interactable).To(vm => vm.IsCanOpAdBtn);
			bindingSet.Bind(adBtn.image).For(v => v.sprite).To(vm => vm.AdBtnImgPath).WithConversion(this);
			bindingSet.Bind(advIcon.BindingContext).For(v => v.DataContext).To(vm => vm.AdvIconVm);
			bindingSet.Bind(outSideRewardNode).For(v=>v.BindDictionaryGetValueFunc).To(vm=>vm.GetOutSideRewardByIndex);
			bindingSet.Bind(outSideRewardNode).For(v => v.Collection).To(vm => vm.OutSideRewardDictionary);
			bindingSet.Bind(inSideRewardNode).For(v=>v.BindDictionaryGetValueFunc).To(vm=>vm.GetInSideRewardByIndex);
			bindingSet.Bind(inSideRewardNode).For(v => v.Collection).To(vm => vm.InSideRewardDictionary);
			bindingSet.Bind(countPointer).For(v=>v.text).To(vm=>vm.CountPointerStr);
			bindingSet.Bind(leftBtn).For(v => v.onClick).To(vm => vm.OnClickLeftBtnCommand);
			bindingSet.Bind(rightBtn).For(v => v.onClick).To(vm => vm.OnClickRightBtnCommand);
			bindingSet.Bind(leftText).For(v=>v.text).To(vm=>vm.LeftTextStr);
			bindingSet.Bind(timeText).For(v => v.text).To(vm => vm.TimeTextStr);
			bindingSet.Bind(opBtn).For(v=>v.onClick).To(vm=>vm.OnClickOpBtnCommand);
			bindingSet.Bind(opBtn).For(v => v.interactable).To(vm => vm.IsCanOpBtn);
			bindingSet.Bind(opBtn.image).For(v=>v.sprite).To(vm=>vm.OpBtnImgPath).WithConversion(this);
			bindingSet.Bind(opBtnItem.BindingContext).For(v => v.DataContext).To(vm => vm.ItemPriceNodeVm);
			bindingSet.Bind(opBtnText).For(v=>v.text).To(vm => vm.OpBtnTextStr);
			bindingSet.Bind(this).For(v=>v.turnPointer0).To(vm=>vm.TurnPointer0).OneWayToSource();
			bindingSet.Bind(this).For(v=>v.turnPointer1).To(vm=>vm.TurnPointer1).OneWayToSource();
			bindingSet.Bind(this).For(v=>v.turnPointer2).To(vm=>vm.TurnPointer2).OneWayToSource();
			bindingSet.Bind(this).For(v=>v.turnNode).To(vm=>vm.TurnNode).OneWayToSource();
			bindingSet.Bind(infoBtn).For(v=>v.onClick).To(vm=>vm.OnClickInfoBtnCommand);
			bindingSet.Bind(fundBtn).For(v=>v.onClick).To(vm=>vm.OnClickFundBtnCommand);
			bindingSet.Bind(fundRedDot.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowFundRedDot);
			
            bindingSet.Build();
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