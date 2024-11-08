using System;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using DHFramework;
using TMPro;
using UnityEngine;
using Extend;
using Spine.Unity;

namespace DH.Game.UIViews
{
    public partial class LuckTravelView : BaseView
    {
        public override bool FullScreen => false;
        
		public StaticItemsBindComponent rewardBg;
		public DhButton btnTips;
		
		public FlipPageCircularScrollView scrollViewInfo;
		[AssetPath]public string scrollViewInfoCell;
		
		public StaticItemsBindComponent drawItemGrid;
		public DhButton skipBtn;
		public GameObject skipSelect;
		public DhButton btnDel;
		public DhButton btnAdd;
		public DhText endTimeText;
		public TextMeshProUGUI countText;
		public DhButton btnDrawFree;
		public DhButton btnDrawFreeUnUse;
		public DhImage greyIcon;
		public DhButton btnDrawBatch;
		public ItemPriceNodeView itemPriceNodeView;
		
		public DhButton btnOpenReward;
		public DhButton btnOpenRewardClose;
		public GameObject progressReward;
		public DhText rewardAllProgress;
		public UICircularScrollView rewardScrollView;
		[AssetPath]public string rewardScrollViewCell;
		public GameObject drawIngNode;

		public BottomComponentView bottomComponentView;
		public DhText btnDrawBatchCnt;
		public DhText btnDrawBatchUnUseCnt;
		public SkeletonGraphic skeletonGraphic;

		public CommonAdvIconView commonAdvIconView;
		public CommonAdvIconView commonAdvIconViewUnUse;
		public GameObject tipsRewardNode;
		public DhText limitDrawCnt;
		public CommonTopView commonTopView;

		public GameObject freeRed;
		public GameObject openRewardRed;

		public DhButton btnDrawBatchUnUse;
		public ItemPriceNodeView priceUnUse;
		[NonSerialized] public int IndexPos;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
	        scrollViewInfo.PrefabPath = scrollViewInfoCell;
	        scrollViewInfo.onValueChanged.AddListener((index) =>
	        {
		        DataCenter.luckyTravelData.CurTipsInfoSelectIndex = index;
		        DHLog.Debug($"CurTipsInfoSelectIndex: {index}");
	        });
			rewardScrollView.PrefabPath = rewardScrollViewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<LuckTravelView, LuckTravelViewModel>();
            bindingSet.Bind(limitDrawCnt).For(v => v.text).To(vm => vm.LimitDrawCntStr);
            bindingSet.Bind(commonTopView.BindingContext).For(v => v.DataContext).To(vm => vm.TopViewModel);
            bindingSet.Bind(tipsRewardNode.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ScrollViewInfoList.Count == 0);
            bindingSet.Bind(scrollViewInfo).For(v => v.Collection).To(vm => vm.ScrollViewInfoList);
            bindingSet.Bind(this).For(v => v.scrollViewInfo).To(vm => vm.ScrollViewInfo).OneWayToSource();
            bindingSet.Bind(endTimeText).For(v => v.text).To(vm => vm.EndTimeValueStr);
            bindingSet.Bind(rewardBg).For(v => v.BindDictionaryGetValueFunc).To(vm => vm.GetSuperRewardCellCallback);
            bindingSet.Bind(rewardBg).For(v => v.Collection).To(vm => vm.SuperRewardList);
			bindingSet.Bind(btnTips).For(v => v.onClick).To(vm => vm.OnClickBtnTipsCommand);
			bindingSet.Bind(drawItemGrid).For(v => v.BindDictionaryGetValueFunc).To(vm => vm.GetRewardCellCallback);
			bindingSet.Bind(drawItemGrid).For(v => v.Collection).To(vm => vm.ScrollViewRewardList);
			bindingSet.Bind(skipBtn).For(v => v.onClick).To(vm => vm.OnClickSkipBtnCommand);
			bindingSet.Bind(skipSelect).For(v => v.activeSelf).To(vm => vm.IsSkipAnimation);
			bindingSet.Bind(btnDel).For(v => v.onClick).To(vm => vm.OnClickBtnDelCommand);
			bindingSet.Bind(btnAdd).For(v => v.onClick).To(vm => vm.OnClickBtnAddCommand);
			bindingSet.Bind(countText).For(v => v.text).To(vm => vm.CountTextCnt);
			bindingSet.Bind(btnDrawFree).For(v => v.onClick).To(vm => vm.OnClickBtnDrawFreeCommand);
			bindingSet.Bind(btnDrawFree.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsCanFreeDraw && vm.BatchCanDraw);
			bindingSet.Bind(btnDrawFreeUnUse.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsCanFreeDraw || !vm.BatchCanDraw);
			bindingSet.Bind(itemPriceNodeView.BindingContext).For(v => v.DataContext).To(vm => vm.ItemPriceNodeModel);
			bindingSet.Bind(btnDrawBatch).For(v => v.onClick).To(vm => vm.OnClickBtnDrawBatchCommand);
			bindingSet.Bind(btnDrawBatch.gameObject).For(v => v.activeSelf).To(vm => vm.BatchCanDraw);
			bindingSet.Bind(btnOpenReward).For(v => v.onClick).To(vm => vm.OnClickBtnProgressCommand);
			bindingSet.Bind(btnOpenRewardClose).For(v => v.onClick).To(vm => vm.OnClickBtnProgressCommand);
			bindingSet.Bind(btnOpenRewardClose.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowProgree);
			bindingSet.Bind(progressReward).For(v => v.activeSelf).To(vm => vm.IsShowProgree);
			bindingSet.Bind(rewardAllProgress).For(v => v.text).To(vm => vm.RewardAllProgressStr);
			bindingSet.Bind(rewardScrollView).For(v => v.Collection).To(vm => vm.RewardScrollViewList);
			bindingSet.Bind(this).For(v => v.rewardScrollView).To(vm => vm.RewardScrollView).OneWayToSource();
			bindingSet.Bind(bottomComponentView.BindingContext).For(v => v.DataContext).To(vm => vm.BottomComponentViewModel);
			bindingSet.Bind(btnDrawBatchCnt).For(v => v.text).ToExpression(vm => GetDrawDesc(vm.CountTextCnt));
			bindingSet.Bind(btnDrawBatchUnUseCnt).For(v => v.text).ToExpression(vm => GetDrawDesc(vm.CountTextCnt));
			bindingSet.Bind(this).For(v => v.skeletonGraphic).To(vm => vm.SkeletonGraphic).OneWayToSource();
			bindingSet.Bind(commonAdvIconView.BindingContext).For(v => v.DataContext).To(vm => vm.CommonAdvIconViewModel);
			bindingSet.Bind(commonAdvIconViewUnUse.BindingContext).For(v => v.DataContext).To(vm => vm.CommonAdvIconViewModel);
			bindingSet.Bind(this).For(v => v.IndexPos).ToExpression(vm => RefreshIndex(vm.TopIndex));
			bindingSet.Bind(drawIngNode).For(v => v.activeSelf).To(vm => vm.DrawIng);
			bindingSet.Bind(freeRed).For(v => v.activeSelf).To(vm => vm.FreeDrawRed);
			bindingSet.Bind(openRewardRed).For(v => v.activeSelf).To(vm => vm.ProgressRed);
			
			bindingSet.Bind(priceUnUse.BindingContext).For(v => v.DataContext).To(vm => vm.ItemPriceNodeModel);
			bindingSet.Bind(btnDrawBatchUnUse.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.BatchCanDraw);
			
            bindingSet.Build();
            greyIcon.Grag = true;
            
        }
        
        
        private int RefreshIndex(int vmTopIndex)
        {
	        rewardScrollView.Jump2SpecificItem(vmTopIndex);
	        return vmTopIndex;
        }
        
        private async UniTaskVoid DelayScrollToPos()
        {
	        await UniTask.Delay(300);
	        rewardScrollView.Jump2SpecificItem(IndexPos);
        }

        private string GetDrawDesc(int vmCountTextCnt)
        {
	        return LocalizeHelper.GetGlobal(GlobalLanguageId.xingyun_05,vmCountTextCnt);
        }
    }
}