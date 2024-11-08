using System;
using DH.Data;
using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using Extend;
using UnityEngine;
using UnityEngine.UI;
namespace DH.Game.UIViews
{
    public partial class RankView : BaseView
    {
        public override bool FullScreen => false;
		public Button ruleBtn;
		public UICircularScrollView scrollview;
		[AssetPath]public string scrollviewCell;
		public RankCellViewView selfRankCellView;
		public Button closeBtn;
		public ScrollRectExtend subTitleView;
		[AssetPath]public string subTitleViewCell;
		public StaticItemsBindComponent topPlayerInfo;
		public CommonTopView commonTopView;
		public BottomComponentView bottomComponentView;
		public DhText maxLevelTitle;
		public GameObject mainRankBanner;
		public GameObject endlessRankBanner;
		public GameObject newcomerBannerImg;
		public GameObject secretBannerImg;
		public DhButton newcomerInfoBut;
		public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollview.PrefabPath = scrollviewCell;
			subTitleView.PrefabPath = subTitleViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<RankView, RankViewModel>();
            bindingSet.Bind(ruleBtn).For(v => v.onClick).To(vm => vm.OnClickRuleBtnCommand);
            bindingSet.Bind(ruleBtn.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowRuleBtn);
			bindingSet.Bind(scrollview).For(v => v.Collection).To(vm => vm.ScrollviewList);
			bindingSet.Bind(selfRankCellView.BindingContext).For(v => v.DataContext).To(vm => vm.SelfRankCellViewVm);
			bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtnCommand);
			bindingSet.Bind(subTitleView).For(v => v.Collection).To(vm => vm.SubTitleViewList);
			bindingSet.Bind(topPlayerInfo).For(v=>v.BindDictionaryGetValueFunc).To(vm=>vm.GetTopRankCellVmCallback);
			bindingSet.Bind(topPlayerInfo).For(v => v.Collection).To(vm => vm.TopRankDic);
			bindingSet.Bind(maxLevelTitle).For(v => v.text).To(vm => vm.MaxLevelTitleStr);
			bindingSet.Bind(mainRankBanner).For(v => v.activeSelf).ToExpression(vm => vm.CurRankType == ERankType.RankItemMainStage &&  !vm.IsNewcomer);
			bindingSet.Bind(endlessRankBanner).For(v => v.activeSelf).ToExpression(vm => vm.CurRankType == ERankType.RankItemEndless);
			bindingSet.Bind(newcomerBannerImg).For(v => v.activeSelf).ToExpression(vm => vm.CurRankType == ERankType.RankItemMainStage &&  vm.IsNewcomer);
			bindingSet.Bind(secretBannerImg).For(v => v.activeSelf).ToExpression(vm => vm.CurRankType == ERankType.RankItemSecret);
			bindingSet.Bind(commonTopView.BindingContext).For(v => v.DataContext).To(vm => vm.CommonTopVm);
			bindingSet.Bind(bottomComponentView.BindingContext).For(v => v.DataContext).To(vm => vm.BottomComponentVm);
			bindingSet.Bind(newcomerInfoBut.gameObject).For(v => v.activeSelf).ToExpression(vm =>vm.IsNewcomer);
			bindingSet.Bind(newcomerInfoBut).For(v => v.onClick).To(vm => vm.OnClickIconBtn)
				.CommandParameter(() => new Tuple<Vector3, Vector3>(newcomerInfoBut.transform.position, new Vector3(-100,0,0)));
            bindingSet.Build();
        }
    }
}