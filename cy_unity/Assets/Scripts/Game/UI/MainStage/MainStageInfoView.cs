using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;

namespace DH.Game.UIViews
{
    public partial class MainStageInfoView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public GameObject leftTopNode;
		public GameObject rightTopNode;
		public GameObject chapterNode;
		public DhButton leftBtn;
		public DhButton rightBtn;
		public DhButton battleBtn;
		public DhText battleText;
		public ItemPriceNodeView battleBtnCostView;
		public StaticItemsBindComponent allBoxComp;
		public ChapterCellView chapterCell;
		public UICircularScrollView topLeftScroll;
		public UICircularScrollView leftScroll;
		public UICircularScrollView rightScroll;
		[AssetPath] public string itemPrefab;
		public DhButton functionMenuBut;
		public GameObject functionMenuButRed;
		public RectTransform functionMenuButRec;
		public DhButton  patrolRewardBut;
		public GameObject  patrolRewardButPr;
		public GameObject  patrolRewardButRed;
		public DhButton secretBattleBtn;
		public GameObject secretBtnLockNode;
		public GameObject secretBtnRedDot;
		
		public GameObject autumnGo;
		public CommonButView autumnBut;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
	        rightScroll.PrefabPath = itemPrefab;
	        leftScroll.PrefabPath = itemPrefab;
	        topLeftScroll.PrefabPath = itemPrefab;
            await base.Create();
            var bindingSet = this.CreateBindingSet<MainStageInfoView, MainStageInfoViewModel>();
            bindingSet.Bind(rightScroll).For(v => v.Collection).To(vm => vm.Manager.RightButItems);
            bindingSet.Bind(leftScroll).For(v => v.Collection).To(vm => vm.Manager.LeftButItems);
            bindingSet.Bind(topLeftScroll).For(v => v.Collection).To(vm => vm.Manager.LeftTopButItems);
			bindingSet.Bind(leftTopNode).For(v => v.activeSelf).To(vm => vm.IsShowLeftTopNode);
			bindingSet.Bind(rightTopNode).For(v => v.activeSelf).To(vm => vm.IsShowRightTopNode);
			// bindingSet.Bind(chapterNode).For(v => v.activeSelf).To(vm => vm.IsShowChapterNode);
			bindingSet.Bind(leftBtn).For(v => v.onClick).To(vm => vm.OnClickLeftBtnCommand);
			bindingSet.Bind(leftBtn.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowLeftBtn);
			bindingSet.Bind(rightBtn).For(v => v.onClick).To(vm => vm.OnClickRightBtnCommand);
			bindingSet.Bind(rightBtn.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowRightBtn);
			bindingSet.Bind(battleBtn).For(v => v.onClick).To(vm => vm.OnClickBattleBtnCommand);
			bindingSet.Bind(battleBtnCostView.BindingContext).For(v => v.DataContext).To(vm => vm.BattleBtnCostVm);
			bindingSet.Bind(battleBtnCostView.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowCostView);
			// bindingSet.Bind(battleText).For(v => v.text).To(vm => vm.BattleTextStr);
			bindingSet.Bind(allBoxComp).For(v=>v.BindDictionaryGetValueFunc).To(vm=>vm.GetBoxVmCallBack);
			bindingSet.Bind(allBoxComp).For(v => v.Collection).To(vm => vm.BoxDictionary);
			bindingSet.Bind(chapterCell.BindingContext).For(v => v.DataContext).To(vm => vm.ChapterCellVm);
			bindingSet.Bind(functionMenuBut).For(v => v.onClick).To(vm => vm.OnClickSettingBtnCommand);
			bindingSet.Bind(functionMenuButRed).For(v => v.activeSelf).To(vm => vm.FunctionMenuRed);
			bindingSet.Bind(this).For(v => v.functionMenuButRec).To(vm => vm.FunctionMenuTransform).OneWayToSource();
			bindingSet.Bind(patrolRewardBut).For(v => v.onClick).To(vm => vm.OnClickPatrolRewardBtnCommand);
			bindingSet.Bind(patrolRewardButPr).For(v => v.activeSelf).To(vm => vm.IsShowPatrolBtn);
			bindingSet.Bind(patrolRewardButRed).For(v => v.activeSelf).To(vm => vm.PatrolRewardButRed);
			bindingSet.Bind(secretBattleBtn).For(v=>v.onClick).To(vm=>vm.OnClickSecretBattleBtnCommand);
			bindingSet.Bind(secretBattleBtn.gameObject).For(v=>v.activeSelf).To(vm=>vm.IsShowSecretBattleBtn);
			bindingSet.Bind(secretBtnLockNode).For(v=>v.activeSelf).To(vm=>vm.IsShowSecretBtnLockNode);
			bindingSet.Bind(secretBtnRedDot).For(v=>v.activeSelf).ToExpression(vm=> vm.IsShowSecretRedDot && !vm.IsShowSecretBtnLockNode);
			bindingSet.Bind(autumnGo).For(v=>v.activeSelf).ToExpression(vm=> vm.IsShowAutumnGo);
			bindingSet.Bind(autumnBut.BindingContext).For(v => v.DataContext).To(vm => vm.AutumnBut);
            bindingSet.Build();
        }
    }
}