using System;
using DH.Game.UI;
using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using Game.UI.Guide;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DH.Data;

namespace DH.Game.UIViews
{
    public partial class MainStageGameUiView : BaseView
    {
        public override bool FullScreen => true;
		public DhButton pauseBtn;
		public DhButton speedBtn;
		public DhText speedText;
		public DhText waveText;
		public DhButton adBtn;
		public DhButton refreshBtn;
		public DhText refreshBtnText;
		public ItemPriceNodeView refreshCostView;
		public DhButton battleBtn;
		public Transform wishUINode;
		public CommonTopView commonTopView;
		public DhText hpValue;
		public Slider hpSlider;
		public DhText defValue;
		public Slider defSlider;
		public Slider talentSlider;
		public DhText talentCount;
		public MainMergeView mainMergeNode;
		public GameObject bottomBtnNode;
		public GameObject confirmBtnNode;
		public DhButton confirmBtn;
		public GameObject waveEffectBode;
		public DhText waveEffectText;
		public CanvasGroup waveEffectCanvasGroup;
		public RectTransform bgRect;
		public RectTransform mergeAreaRect;
		public ScrollRectExtend activeScrollView;
		[AssetPath] public string activeSkillItem;
		public CommonAdvIconView commonAdv;
		public RectTransform contentRect;
		public ResItemView adResItemView;
		public RectTransform playerInfoNode;
		public RectTransform hpNode;
		public GameObject bottomNode;

		private void Awake()
		{
			bottomNode.SetActive(false);
		}

		public override async UniTask Create()
        {
	        hpSlider.interactable = false;
	        defSlider.interactable = false;
	        activeScrollView.PrefabPath = activeSkillItem;
            await base.Create();
            var bindingSet = this.CreateBindingSet<MainStageGameUiView, MainStageGameUiViewModel>();
			bindingSet.Bind(pauseBtn).For(v => v.onClick).To(vm => vm.OnClickPauseBtnCommand);
			bindingSet.Bind(pauseBtn.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowPauseBtn);
			bindingSet.Bind(speedBtn).For(v => v.onClick).To(vm => vm.OnClickSpeedBtnCommand);
			bindingSet.Bind(mainMergeNode.BindingContext).For(v => v.DataContext).To(vm => vm.MainMergeVm);
			bindingSet.Bind(speedText).For(v => v.text).To(vm => vm.SpeedTextStr);
			bindingSet.Bind(waveText).For(v => v.text).To(vm => vm.WaveTextStr);
			bindingSet.Bind(adBtn).For(v => v.onClick).To(vm => vm.OnClickAdBtnCommand);
			bindingSet.Bind(adBtn.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowAdBtn);
			bindingSet.Bind(refreshBtn).For(v => v.onClick).To(vm => vm.OnClickRefreshBtnCommand);
			bindingSet.Bind(refreshBtn.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowRefreshBtn);
			bindingSet.Bind(refreshBtnText).For(v=>v.text).To(vm=>vm.RefreshBtnTextStr);
			bindingSet.Bind(refreshCostView.gameObject).For(v=>v.activeSelf).To(vm=>vm.IsShowRefreshCost);
			bindingSet.Bind(refreshCostView.BindingContext).For(v => v.DataContext).To(vm => vm.RefreshCostViewVm);
			bindingSet.Bind(battleBtn).For(v => v.onClick).To(vm => vm.OnClickBattleBtnCommand);
			bindingSet.Bind(battleBtn.gameObject).For(v => v.activeSelf).To(vm => vm.DataManager.WaveEnd);
			bindingSet.Bind(this).For(v => v.wishUINode).To(vm => vm.WishUINode).OneWayToSource();
			bindingSet.Bind(commonTopView.BindingContext).For(v=>v.DataContext).To(vm=>vm.CommonTopViewVm);
			bindingSet.Bind(hpValue).For(v=>v.text).ToExpression(vm=>GetHpValueStr(vm.HpValueStr));
			bindingSet.Bind(defValue).For(v=>v.text).To(vm=>vm.DefValueStr);
			bindingSet.Bind(talentCount).For(v=>v.text).To(vm=>vm.TalentCountStr);
			bindingSet.Bind(talentCount.gameObject).For(v=>v.activeSelf).To(vm=>vm.IsShowTalentCount);
			bindingSet.Bind(talentSlider).For(v => v.value).To(vm => vm.TalentProgressValue);
			bindingSet.Bind(hpSlider).For(v => v.value).To(vm => vm.HpSliderProgress);
			bindingSet.Bind(defSlider).For(v => v.value).To(vm => vm.DefSliderProgress);
			bindingSet.Bind(bottomBtnNode).For(v => v.activeSelf).To(vm => vm.IsShowBottomBtn);
			bindingSet.Bind(confirmBtnNode).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowBottomBtn);
			bindingSet.Bind(confirmBtn).For(v => v.onClick).To(vm => vm.OnClickConfirmBtnCommand);
			bindingSet.Bind(waveEffectBode).For(v=>v.activeSelf).To(vm=>vm.IsShowWaveEffect);
			bindingSet.Bind(waveEffectText).For(v=>v.text).To(vm=>vm.WaveEffectTextStr);
			bindingSet.Bind(waveEffectCanvasGroup).For(v=>v.alpha).To(vm=>vm.WaveEffectAlpha);
			bindingSet.Bind(bgRect).For(v=>v.anchoredPosition).To(vm=>vm.BgRectPos);
			bindingSet.Bind(mergeAreaRect).For(v=>v.localScale).To(vm=>vm.MergeAreaScale);
			bindingSet.Bind(activeScrollView).For(v=>v.Collection).To(vm=>vm.ActiveSkillList);
			bindingSet.Bind(commonAdv.BindingContext).For(v => v.DataContext).To(vm => vm.CommonAdvVm);
			bindingSet.Bind(contentRect).For(v => v.sizeDelta).To(vm => vm.ContentSize);
			bindingSet.Bind(adResItemView.BindingContext).For(v => v.DataContext).To(vm => vm.AdResIteVm);
            bindingSet.Build();
            adResItemView.gameObject.SetActive(!DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.ADFreeForever) );
            DelayShowBottomNode();
        }
        private string GetHpValueStr(string hpStr)
        {
	        hpValue.text = hpStr;
	        LayoutRebuilder.ForceRebuildLayoutImmediate(hpNode);
	        LayoutRebuilder.ForceRebuildLayoutImmediate(playerInfoNode);
	        return hpStr;
        }

        private async void DelayShowBottomNode()
        {
	        bottomNode.SetActive(false);
	        await UniTask.Delay(500);
	        bottomNode.SetActive(true);
        }

        public override bool OnPhysicExit()
        {
	        if (GuideManager.Instance.IsTriggerLevelGuide) return true;
	        GameManager.Instance.OnClickExitBtn();
	        return true;
        }
    }
}