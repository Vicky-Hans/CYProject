using DH.Data;
using DH.Game.UI;
using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine.UI;
using UnityEngine;
using Extend;
using Game.UI;

namespace DH.Game.UIViews
{
    public partial class SecretGameUiView : BaseView
    {
        public override bool FullScreen => false;
		public Slider expSlider;
		public UICircularScrollView weaponScrollview;
		[AssetPath]public string weaponScrollviewCell;
		public DhText expLevelText;
		public DhButton pauseBtn;
		public GameObject timeNode;
		public DhText timeText;
		public DhText killText;
		public DhText speedText;
		public DhButton speedBtn;
		public GameObject stageInfoNode;
		public GameObject bossInfoNode;
		public Slider bossHpSlider;
		public DhText bossHpInfoText;
		public Slider bossLeftTimeSlider;
		public DhText bossLeftTimeText;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
			weaponScrollview.PrefabPath = weaponScrollviewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<SecretGameUiView, SecretGameUiViewModel>();
			bindingSet.Bind(expSlider).For(v => v.value).To(vm => vm.ExpSliderValue);
			bindingSet.Bind(weaponScrollview).For(v => v.Collection).To(vm => vm.WeaponScrollviewList);
			bindingSet.Bind(weaponScrollview).For(v => v.m_SpacingX).To(vm => vm.ScrollviewSpacingX);
			bindingSet.Bind(weaponScrollview).For(v => v.m_SpacingY).To(vm => vm.ScrollviewSpacingY);
			bindingSet.Bind(weaponScrollview).For(v => v.m_Row).To(vm => vm.ScrollviewRowCount);
			bindingSet.Bind(weaponScrollview).For(v => v.m_PaddingLeft).To(vm => vm.ScrollviewPaddingLeft);
			bindingSet.Bind(weaponScrollview).For(v => v.m_PaddingBottom).To(vm => vm.ScrollviewPaddingTop);
			bindingSet.Bind(this).For(v=>v.weaponScrollview).To(vm=>vm.WeaponScrollview).OneWayToSource();
			bindingSet.Bind(expLevelText).For(v => v.text).To(vm => vm.ExpLevelTextStr);
			bindingSet.Bind(pauseBtn).For(v => v.onClick).To(vm => vm.OnClickPauseBtnCommand);
			bindingSet.Bind(timeNode).For(v => v.activeSelf).To(vm => vm.IsShowTimeNode);
			bindingSet.Bind(timeText).For(v => v.text).To(vm => vm.TimeTextStr);
			bindingSet.Bind(killText).For(v=>v.text).To(vm=>vm.KillTextStr);
			bindingSet.Bind(speedBtn).For(v=>v.onClick).To(vm=>vm.OnClickSpeedBtnCommand);
			bindingSet.Bind(speedText).For(v=>v.text).To(vm=>vm.SpeedTextStr);
			bindingSet.Bind(stageInfoNode).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowBossInfoNode);
			bindingSet.Bind(bossInfoNode).For(v => v.activeSelf).To(vm => vm.IsShowBossInfoNode);
			bindingSet.Bind(bossHpSlider).For(v => v.value).To(vm => vm.BossHpSliderValue);
			bindingSet.Bind(bossHpInfoText).For(v=>v.text).To(vm=>vm.BossHpInfoTextStr);
			bindingSet.Bind(bossLeftTimeSlider).For(v => v.value).To(vm => vm.BossLeftTimeSliderValue);
			bindingSet.Bind(bossLeftTimeText).For(v => v.text).To(vm => vm.BossLeftTimeTextStr);
			bindingSet.Bind(bossLeftTimeText).For(v => v.color).To(vm => vm.BossLeftTimeTextColor);
			bindingSet.Bind(bossLeftTimeText.transform).For(v => v.localScale).To(vm => vm.BossLeftTimeTextScale);
			
			bindingSet.Build();
			var leftTime = ServerTime.Instance.RemainTime(DataCenter.secretData.SeasonTime);
			if (leftTime > GameConst.SecretShowTipsTime)
			{
				GameManager.Instance.CheckUnChooseTalent();
			}
			
            expSlider.interactable = false;
            bossLeftTimeSlider.enabled = false;
            bossHpSlider.interactable = false;
        }

        protected override void OnShow()
        {
	        PopUpManager.Instance.CheckAndPopUpView();
	        base.OnShow();
        }
    }
}