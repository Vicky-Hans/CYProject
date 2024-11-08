using Extend;
using DH.Data;
using DH.Config;
using UnityEngine;
using DH.UIFramework;
using DH.Game.ViewModels;
using Cysharp.Threading.Tasks;
using DH.Game.UIViews.ItemViews;
namespace DH.Game.UIViews
{
    public partial class EndlessActivityView : BaseView
    {
        public override bool FullScreen => false;
        public DhButton ruleBtn;
        public DhButton startBtn;
        public DhText timeText;
        public DhText killText;
        public DhText coinNumText;
        public DhText remainingNumText;
        public ItemPriceNodeView battleBtnCostView;
        public CommonButView rankBtn;
        public GameObject redNode;
        public DhButton closeBtn;
        public CommonTopView commonTopView;
        public DhButton sweepingBtn;
        public DhText sweepingTipsText;
        public DhImage sweepingBtnImg;
        public ItemPriceNodeView sweepingCostView;
        public ItemPriceNodeView coinCostView;
        public override async UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<EndlessActivityView, EndlessActivityViewModel>();
            bindingSet.Bind(rankBtn.BindingContext).For(v => v.DataContext).To(vm => vm.RankBtnItemVm);
            bindingSet.Bind(battleBtnCostView.BindingContext).For(v => v.DataContext).To(vm => vm.BattleBtnCostVm);
            bindingSet.Bind(ruleBtn).For(v => v.onClick).To(vm => vm.OnClickRuleBtnCommand);
            bindingSet.Bind(startBtn).For(v => v.onClick).To(vm => vm.OnClickBattleBtnCommand);
            bindingSet.Bind(timeText).For(v => v.text).ToExpression(vm => GetTimeStr(vm.TimeCd));
            bindingSet.Bind(killText).For(v => v.text).To(vm => vm.MaxKillNumStr);
            bindingSet.Bind(coinNumText).For(v => v.text).To(vm => vm.MaxCoinNumStr);
            bindingSet.Bind(remainingNumText).For(v => v.text).To(vm => vm.RemainingNumStr);
            bindingSet.Bind(redNode).For(v => v.activeSelf).To(vm =>vm.IsShowCostView);
            bindingSet.Bind(battleBtnCostView.gameObject).For(v => v.activeSelf).To(vm =>vm.IsShowCostView);
            bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseCommand);
            bindingSet.Bind(commonTopView.BindingContext).For(v => v.DataContext).To(vm => vm.CommonTopViewModel);
            bindingSet.Bind(sweepingBtnImg).For(v => v.sprite).To(vm => vm.SweepingBtnImgStr).WithConversion(this);
            bindingSet.Bind(sweepingBtn).For(v => v.enabled).To(vm => vm.IsEnough);
            bindingSet.Bind(sweepingCostView.BindingContext).For(v => v.DataContext).To(vm => vm.BattleBtnCostVm);
            bindingSet.Bind(coinCostView.BindingContext).For(v => v.DataContext).To(vm => vm.SweepingCostVm);
            bindingSet.Bind(sweepingBtn.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowSweeping);
            bindingSet.Bind(sweepingBtn).For(v => v.onClick).To(vm => vm.OnClickSweepingBtnCommand);
            bindingSet.Bind(sweepingTipsText.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsShowSweepingTips);
            bindingSet.Build();
        }
        private string GetTimeStr(long cd)
        {
            var str = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.endless_02).Name;
            return $"{str}{UIHelper.ConvertTimeSecondToString(cd, ETimeFormatType.TimeFormatChampion)}";
        }
    }
}