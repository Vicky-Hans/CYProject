using DH.Game.UI;
using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DH.Data;

namespace DH.Game.UIViews
{
    public partial class EndlessGameUIView : BaseView
    {
        public override bool FullScreen => true;
        public DhButton pauseBtn;
        public DhButton speedBtn;
        public DhText speedText;
        public DhText killText;
        public DhButton adBtn;
        public GameObject adIcon;
        public CommonAdvIconView commonAdv;
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
        public MainMergeView mainMergeNode;
        public GameObject bottomBtnNode;
        public GameObject confirmBtnNode;
        public DhButton confirmBtn;
        public DhText timeText;
        public DhText coinNumText;
        public RectTransform bgRect;
        public RectTransform mergeAreaRect;
        public ScrollRectExtend activeScrollView;
        [AssetPath] public string activeSkillItem;
        public RectTransform contentRect;
        public ResItemView adResItemView;
        public override async UniTask Create()
        {
            hpSlider.interactable = false;
	        defSlider.interactable = false;
	        activeScrollView.PrefabPath = activeSkillItem;
            await base.Create();
            var bindingSet = this.CreateBindingSet<EndlessGameUIView, EndlessGameUIViewModel>();
			bindingSet.Bind(pauseBtn).For(v => v.onClick).To(vm => vm.OnClickPauseBtnCommand);
			bindingSet.Bind(pauseBtn.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowPauseBtn);
			bindingSet.Bind(speedBtn).For(v => v.onClick).To(vm => vm.OnClickSpeedBtnCommand);
			bindingSet.Bind(mainMergeNode.BindingContext).For(v => v.DataContext).To(vm => vm.MainMergeVm);
			bindingSet.Bind(speedText).For(v => v.text).To(vm => vm.SpeedTextStr);
			bindingSet.Bind(timeText).For(v => v.text).To(vm => vm.TimeStr);
			bindingSet.Bind(killText).For(v => v.text).To(vm => vm.KillTextStr);
			bindingSet.Bind(coinNumText).For(v => v.text).To(vm => vm.CoinNumStr);
			bindingSet.Bind(adBtn).For(v => v.onClick).To(vm => vm.OnClickAdBtnCommand);
			bindingSet.Bind(adBtn.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowAdBtn);
			// bindingSet.Bind(adIcon.transform.parent.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowAdIcon);
			bindingSet.Bind(commonAdv.BindingContext).For(v => v.DataContext).To(vm => vm.CommonAdvVm);
			bindingSet.Bind(refreshBtn).For(v => v.onClick).To(vm => vm.OnClickRefreshBtnCommand);
			bindingSet.Bind(refreshBtn.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowRefreshBtn);
			bindingSet.Bind(refreshBtnText).For(v=>v.text).To(vm=>vm.RefreshBtnTextStr);
			bindingSet.Bind(refreshCostView.gameObject).For(v=>v.activeSelf).To(vm=>vm.IsShowRefreshCost);
			bindingSet.Bind(refreshCostView.BindingContext).For(v => v.DataContext).To(vm => vm.RefreshCostViewVm);
			bindingSet.Bind(battleBtn).For(v => v.onClick).To(vm => vm.OnClickBattleBtnCommand);
			bindingSet.Bind(battleBtn.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowBattle);
			bindingSet.Bind(this).For(v => v.wishUINode).To(vm => vm.WishUINode).OneWayToSource();
			bindingSet.Bind(commonTopView.BindingContext).For(v=>v.DataContext).To(vm=>vm.CommonTopViewVm);
			bindingSet.Bind(hpValue).For(v=>v.text).To(vm=>vm.HpValueStr);
			bindingSet.Bind(hpValue).For(v=>v.text).To(vm=>vm.HpValueStr);
			bindingSet.Bind(defValue).For(v=>v.text).To(vm=>vm.DefValueStr);
			bindingSet.Bind(hpSlider).For(v => v.value).To(vm => vm.HpSliderProgress);
			bindingSet.Bind(defSlider).For(v => v.value).To(vm => vm.DefSliderProgress);
			bindingSet.Bind(bottomBtnNode).For(v => v.activeSelf).To(vm => vm.IsShowBottomBtn);
			bindingSet.Bind(confirmBtnNode).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowBottomBtn);
			bindingSet.Bind(confirmBtn).For(v => v.onClick).To(vm => vm.OnClickConfirmBtnCommand);
			bindingSet.Bind(bgRect).For(v=>v.anchoredPosition).To(vm=>vm.BgRectPos);
			bindingSet.Bind(mergeAreaRect).For(v=>v.localScale).To(vm=>vm.MergeAreaScale);
			bindingSet.Bind(activeScrollView).For(v=>v.Collection).To(vm=>vm.ActiveSkillList);
			bindingSet.Bind(contentRect).For(v => v.sizeDelta).To(vm => vm.ContentSize);
			bindingSet.Bind(adResItemView.BindingContext).For(v => v.DataContext).To(vm => vm.AdResIteVm);
			
            bindingSet.Build();
            
            adResItemView.gameObject.SetActive(!DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.ADFreeForever));
        }
        public override bool OnPhysicExit()
        {
            GameManager.Instance.OnClickExitBtn();
            return true;
        }
    }
}