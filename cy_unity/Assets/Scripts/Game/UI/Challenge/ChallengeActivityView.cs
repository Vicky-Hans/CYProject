using Extend;
using DH.Data;
using DH.Config;
using UnityEngine;
using DH.UIFramework;
using DH.Game.ViewModels;
using Cysharp.Threading.Tasks;
using DH.Game.UIViews.ItemViews;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class ChallengeActivityView : BaseView
    {
        public override bool FullScreen => false;
        public Slider progressSlider;
        public GameObject redNode;
        public DhButton ruleBtn;
        public DhButton startBtn;
        public DhImage startBtnImg;
        public DhImage sweepingBtnImg;
        public DhText timeText;
        public DhText weekTimeText;
        public DhText weekProgressNumText;
        public DhText sweepingTipsText;
        public DhText remainingNumText;
        public DhText killNumText;
        public ItemPriceNodeView battleBtnCostView;
        public DhButton sweepingBtn;
        public ItemPriceNodeView sweepingCostView;
        public DhButton closeBtn;
        public CommonTopView commonTopView;
        public ScrollRectExtend buffScrollView;
        public StaticItemsBindComponent dailyAwardBoxComp;
        public StaticItemsBindComponent weekAwardBoxComp;
        [AssetPath] public string ItemPrefab;
        public override async UniTask Create()
        {
            buffScrollView.PrefabPath = ItemPrefab;
            await base.Create();
            var bindingSet = this.CreateBindingSet<ChallengeActivityView, ChallengeActivityViewModel>();
            bindingSet.Bind(startBtn).For(v => v.enabled).To(vm => vm.IsEnough);
            bindingSet.Bind(startBtnImg).For(v => v.sprite).To(vm => vm.StartBtnImgStr).WithConversion(this);
            bindingSet.Bind(battleBtnCostView.BindingContext).For(v => v.DataContext).To(vm => vm.BattleBtnCostVm);
            bindingSet.Bind(ruleBtn).For(v => v.onClick).To(vm => vm.OnClickRuleBtnCommand);
            bindingSet.Bind(startBtn).For(v => v.onClick).To(vm => vm.OnClickBattleBtnCommand);
            bindingSet.Bind(timeText).For(v => v.text).ToExpression(vm => GetTimeStr(vm.DailyTimeCd));
            bindingSet.Bind(weekTimeText).For(v => v.text).ToExpression(vm => UIHelper.ConvertTimeSecondToString(vm.WeekTimeCd, ETimeFormatType.TimeFormatChampion));
            bindingSet.Bind(remainingNumText).For(v => v.text).To(vm => vm.RemainingNumStr);
            bindingSet.Bind(redNode).For(v => v.activeSelf).To(vm => vm.IsShowRed);
            bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseCommand);
            bindingSet.Bind(dailyAwardBoxComp).For(v=>v.BindDictionaryGetValueFunc).To(vm=>vm.GetDailyBoxVmCallBack);
            bindingSet.Bind(dailyAwardBoxComp).For(v => v.Collection).To(vm => vm.DailyAwardList);
            bindingSet.Bind(weekAwardBoxComp).For(v=>v.BindDictionaryGetValueFunc).To(vm=>vm.GetWeekBoxVmCallBack);
            bindingSet.Bind(weekAwardBoxComp).For(v => v.Collection).To(vm => vm.WeekAwardList);
            bindingSet.Bind(commonTopView.BindingContext).For(v => v.DataContext).To(vm => vm.CommonTopViewModel);
            bindingSet.Bind(weekProgressNumText).For(v => v.text).To(vm => vm.WeekProgressNumStr);
            bindingSet.Bind(buffScrollView).For(v=>v.Collection).To(vm=>vm.BuffItemList);
            bindingSet.Bind(progressSlider).For(v=>v.value).To(vm=>vm.WeekProgressValue);
            bindingSet.Bind(killNumText).For(v=>v.text).To(vm=>vm.KillNumStr);
            bindingSet.Bind(sweepingBtnImg).For(v => v.sprite).To(vm => vm.SweepingBtnImgStr).WithConversion(this);
            bindingSet.Bind(sweepingBtn).For(v => v.enabled).To(vm => vm.IsEnough);
            bindingSet.Bind(sweepingCostView.BindingContext).For(v => v.DataContext).To(vm => vm.BattleBtnCostVm);
            bindingSet.Bind(sweepingTipsText.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowSweeping);
            bindingSet.Bind(sweepingBtn.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowSweeping);
            bindingSet.Bind(sweepingBtn).For(v => v.onClick).To(vm => vm.OnClickSweepingBtnCommand);
            bindingSet.Build();
        }
        private string GetTimeStr(long cd)
        {
            var str = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.endless_02).Name;
            return $"{str}{UIHelper.ConvertTimeSecondToString(cd, ETimeFormatType.TimeFormatChampion)}";
        }
    }
}