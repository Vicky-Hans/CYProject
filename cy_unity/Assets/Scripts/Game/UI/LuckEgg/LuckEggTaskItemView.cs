using DH.Data;
using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine.UI;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class LuckEggTaskItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhText title;
		public Slider expSlide;
		public DhText expSlideText;
		public UICircularScrollView awardScrollview;
		[AssetPath]public string awardScrollviewCell;
		public GameObject bg;
		public GameObject bgGray;
		public DhButton goBtn;
		public DhButton btnOver;
		public DhButton getBtn;

		public CommonAdvIconView commonAdvView;
		public DhButton advBtn;
		public DhText timeText;
		public DhButton noWayBtn;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			awardScrollview.PrefabPath = awardScrollviewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<LuckEggTaskItemView, LuckEggTaskItemViewModel>();
            
			bindingSet.Bind(title).For(v => v.text).To(vm => vm.TitleStr);
			bindingSet.Bind(expSlide).For(v => v.value).To(vm => vm.ExpSlideValue);
			bindingSet.Bind(expSlideText).For(v => v.text).To(vm => vm.ProgressDesc);
			bindingSet.Bind(awardScrollview).For(v => v.Collection).To(vm => vm.AwardScrollviewList);
			bindingSet.Bind(goBtn).For(v => v.onClick).To(vm => vm.OnClickGoBtnCommand);
			bindingSet.Bind(noWayBtn.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.State == LuckEggTaskItemState.NoWay && !vm.IsAdv);
			bindingSet.Bind(goBtn.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.State == LuckEggTaskItemState.NoFinish && !vm.IsAdv);
			bindingSet.Bind(btnOver.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.State == LuckEggTaskItemState.Finish);
			bindingSet.Bind(getBtn).For(v => v.onClick).To(vm => vm.OnClickGetBtnCommand);
			bindingSet.Bind(getBtn.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.State == LuckEggTaskItemState.NotGetAward);
			
			bindingSet.Bind(bg).For(v => v.activeSelf).ToExpression(vm => vm.State != LuckEggTaskItemState.Finish);
			bindingSet.Bind(bgGray).For(v => v.activeSelf).ToExpression(vm => vm.State == LuckEggTaskItemState.Finish);
			
			bindingSet.Bind(commonAdvView.BindingContext).For(v => v.DataContext).To(vm => vm.CommonAdvIconVm);
			bindingSet.Bind(advBtn.gameObject).For(v => v.activeSelf).
				ToExpression(vm => vm.State != LuckEggTaskItemState.Finish &&  vm.State != LuckEggTaskItemState.NotGetAward && vm.IsAdv);
			bindingSet.Bind(advBtn).For(v => v.onClick).To(vm => vm.OnClickAdvBtnCommand);
	        bindingSet.Bind(timeText).For(v => v.text).To(vm => vm.TimeStr);
	        bindingSet.Bind(timeText.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsAdv);
	        bindingSet.Bind(expSlide.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsAdv);
            bindingSet.Build();
        }
    }
}