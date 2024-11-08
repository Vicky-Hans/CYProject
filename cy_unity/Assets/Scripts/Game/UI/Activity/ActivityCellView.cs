using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class ActivityCellView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhImage cellBg;
		public DhImage titleBg;
		public DhImage lockedTitleBg;
		public DhText nameText;
		public DhText lockNameText;
		public DhText leftTimeText;
		public UICircularScrollView challengeAwardList;
		[AssetPath]public string challengeAwardListCell;
		public DhButton opBtn;
		public GameObject redDotNode;
		public GameObject leftCountNode;
		public DhText leftCountText;
		public GameObject lockNode;
		public DhText lockDescText;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
			challengeAwardList.PrefabPath = challengeAwardListCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<ActivityCellView, ActivityCellViewModel>();
            bindingSet.Bind(cellBg).For(v => v.sprite).To(vm => vm.CellBgPath).WithConversion(this);
            bindingSet.Bind(titleBg).For(v => v.sprite).To(vm => vm.TitleBgPath).WithConversion(this);
            bindingSet.Bind(titleBg.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowLockTitle);
            bindingSet.Bind(lockedTitleBg).For(v => v.sprite).To(vm => vm.TitleBgPath).WithConversion(this);
			bindingSet.Bind(lockedTitleBg.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowLockTitle);
			bindingSet.Bind(nameText).For(v => v.text).To(vm => vm.NameTextStr);
			bindingSet.Bind(lockNameText).For(v => v.text).To(vm => vm.NameTextStr);
			bindingSet.Bind(leftCountNode).For(v=>v.activeSelf).ToExpression(vm=>vm.IsShowLeftCountNode && !vm.IsShowLockTitle);
			bindingSet.Bind(leftTimeText).For(v => v.text).To(vm => vm.LeftTimeTextStr);
			bindingSet.Bind(leftTimeText.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowLeftTime);
			bindingSet.Bind(challengeAwardList).For(v => v.Collection).To(vm => vm.AwardScrollviewList);
			bindingSet.Bind(challengeAwardList.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowLockTitle);
			bindingSet.Bind(opBtn).For(v => v.onClick).To(vm => vm.OnClickOpBtnCommand);
			bindingSet.Bind(opBtn.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowLockNode);
			bindingSet.Bind(redDotNode).For(v => v.activeSelf).To(vm => vm.IsShowRedDotNode);
			bindingSet.Bind(leftCountText).For(v => v.text).To(vm => vm.LeftCountTextStr);
			bindingSet.Bind(lockNode).For(v => v.activeSelf).To(vm => vm.IsShowLockNode);
			bindingSet.Bind(lockDescText).For(v => v.text).To(vm => vm.LockDescTextStr);
            bindingSet.Build();
            challengeAwardList.m_CanDragScrollView = false;
        }
    }
}