using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
	/// <summary>
	/// 活动规则界面  玩法介绍 跟 概率展示
	/// </summary>
    public partial class ActivityRuleAndRatioView : BaseView
    {
        public override bool FullScreen => false;
		public DhButton closeBtn;
		public DhText titleText;
		public DhButton leftBtn;
		public DhImage leftBg;
		public DhText leftText;
		public DhButton rightBtn;
		public DhImage rightBg;
		public DhText rightText;
		public GameObject ruleNode;
		public GameObject ratioNode;
		public DhText ruleText;
		public UICircularScrollView ratioScrollView;
		[AssetPath] public string[] ratioItemPath;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
	        ratioScrollView.GetPrefabPathFunction = GetPrefab;
            await base.Create();
            var bindingSet = this.CreateBindingSet<ActivityRuleAndRatioView, ActivityRuleAndRatioViewModel>();
			bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtnCommand);
			bindingSet.Bind(titleText).For(v => v.text).To(vm => vm.CurData.TitleStr);
			bindingSet.Bind(leftBtn).For(v => v.onClick).To(vm => vm.OnClickLeftBtnCommand);
			bindingSet.Bind(leftBtn).For(v => v.interactable).ToExpression(vm => vm.CurType != EMagicDrawInfoType.Rule);
			bindingSet.Bind(leftBg.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.CurType == EMagicDrawInfoType.Rule);
			bindingSet.Bind(leftText).For(v => v.text).To(vm => vm.CurData.RuleBtnStr);
			bindingSet.Bind(leftText).For(v => v.color).To(vm => vm.LeftTextStrColor);
			bindingSet.Bind(rightBtn).For(v => v.onClick).To(vm => vm.OnClickRightBtnCommand);
			bindingSet.Bind(rightBtn).For(v => v.interactable).ToExpression(vm => vm.CurType != EMagicDrawInfoType.Ratio);
			bindingSet.Bind(rightBg.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.CurType == EMagicDrawInfoType.Ratio);
			bindingSet.Bind(rightText).For(v => v.text).To(vm => vm.CurData.RatioBtnStr);
			bindingSet.Bind(rightText).For(v => v.color).To(vm => vm.RightTextStrColor);
			bindingSet.Bind(ruleNode).For(v => v.activeSelf).ToExpression(vm => vm.CurType == EMagicDrawInfoType.Rule);
			bindingSet.Bind(ruleText).For(v=>v.text).To(vm=>vm.CurData.RuleStr);
			bindingSet.Bind(ratioNode).For(v => v.activeSelf).ToExpression(vm => vm.CurType == EMagicDrawInfoType.Ratio);
			bindingSet.Bind(ratioScrollView).For(v => v.Collection).To(vm => vm.RatioScrollViewList);
            bindingSet.Build();
        }
        private string GetPrefab(int arg1, object model)
        {
	        if (model is LuckDrawRatioCellViewModel) return ratioItemPath[0];
	        return ratioItemPath[1];
        }
    }
}