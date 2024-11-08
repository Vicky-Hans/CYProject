using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class MonthCardAwardShowView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhButton clickToClose;
		public DhImage card;
		public UICircularScrollView effectDes;
		[AssetPath]public string effectDesCell;
		public UICircularScrollView awardScrollview;
		[AssetPath]public string awardScrollviewCell;

		public GameObject cardEffect;
		public GameObject cardEffectPlus;
		public GameObject cardEffectPlusPermanent;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			effectDes.PrefabPath = effectDesCell;
			awardScrollview.PrefabPath = awardScrollviewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<MonthCardAwardShowView, MonthCardAwardShowViewModel>();
            
			bindingSet.Bind(clickToClose).For(v => v.onClick).To(vm => vm.OnClickClickToCloseCommand);
			bindingSet.Bind(card).For(v => v.sprite).To(vm => vm.CardPath).WithConversion(this);
			bindingSet.Bind(effectDes).For(v => v.Collection).To(vm => vm.EffectDesList);
			bindingSet.Bind(awardScrollview).For(v => v.Collection).To(vm => vm.AwardScrollviewList);
			bindingSet.Bind(cardEffect).For(v => v.activeSelf).To(vm => vm.CardEffect);
			bindingSet.Bind(cardEffectPlus).For(v => v.activeSelf).To(vm => vm.CardEffectPlus);
			bindingSet.Bind(cardEffectPlusPermanent).For(v => v.activeSelf).To(vm => vm.CardEffectPermanent);
            bindingSet.Build();
            AudioManager.Instance.PlayRewardTitle();
        }

    }
}