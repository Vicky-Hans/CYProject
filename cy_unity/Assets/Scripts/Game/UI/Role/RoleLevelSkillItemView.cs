using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine.UI;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class RoleLevelSkillItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public Image lvBg;
		public DhText lvDes;
		public DhText skillDes;
		public GameObject lockGo;
		public GameObject heroTarget;
		public GameObject effectGo;
		public CanvasGroup canvasGroup;
		private float skillDesAlpha;
		public float SkillDesAlpha
		{
			get => skillDesAlpha;
			set
			{
				skillDesAlpha = value;
				canvasGroup.alpha = skillDesAlpha;
			}

		}

		private bool isShowHeroTarget;
		private RectTransform skillDesSrc;
		public bool IsShowHeroTarget
		{
			get=>isShowHeroTarget;
			set
			{
				isShowHeroTarget = value;
				if (skillDesSrc == null) 
					skillDesSrc = skillDes.GetComponent<RectTransform>();
				skillDesSrc.sizeDelta = new Vector2(isShowHeroTarget?470:750,skillDesSrc.sizeDelta.y);
			}
		}

		public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<RoleLevelSkillItemView, RoleLevelSkillItemViewModel>();
            
			bindingSet.Bind(lvBg).For(v => v.sprite).To(vm => vm.LvBgPath).WithConversion(this);
			bindingSet.Bind(lvDes).For(v => v.text).To(vm => vm.LvDesStr);
			bindingSet.Bind(skillDes).For(v => v.text).To(vm => vm.SkillDesStr);
			bindingSet.Bind(lockGo).For(v => v.activeSelf).To(vm => vm.ShowLockGo);
			bindingSet.Bind(heroTarget).For(v => v.activeSelf).ToExpression(vm => vm.HeroTarget);
			bindingSet.Bind(this).For(v => v.IsShowHeroTarget).ToExpression(vm => vm.HeroTarget);
			bindingSet.Bind(effectGo).For(v => v.activeSelf).ToExpression(vm => vm.EffectGo);
			bindingSet.Bind(this).For(v => v.SkillDesAlpha).ToExpression(vm => vm.SkilDesStAlpha);
            bindingSet.Build();
        }

    }
}