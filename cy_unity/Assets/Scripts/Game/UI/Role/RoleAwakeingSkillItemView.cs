using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class RoleAwakeingSkillItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhImage lvBg;
		public DhText skilDes;

        public GameObject heroTarget;
        public GameObject lockGo;
        public GameObject lockStar;
        public GameObject unLockStar;
        public GameObject effectGo;
        public CanvasGroup canvasGroup;
        private int star;
        public int Star
        {
            get => star;
            set
            {
                star = value;
                var childs = lockStar.transform.childCount;
                for (int i = 0; i < childs; i++)
                {
                    lockStar.transform.GetChild(i).gameObject.SetActive(star-1>=i);
                    unLockStar.transform.GetChild(i).gameObject.SetActive(star-1>=i);
                }
            }
        }
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
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<RoleAwakeingSkillItemView, RoleAwakeingSkillItemViewModel>();
            
			bindingSet.Bind(lvBg).For(v => v.sprite).To(vm => vm.LvBgPath).WithConversion(this);
			bindingSet.Bind(skilDes).For(v => v.text).To(vm => vm.SkilDesStr);
            bindingSet.Bind(lockGo).For(v => v.activeSelf).ToExpression(vm => vm.IsLock);
            bindingSet.Bind(lockStar).For(v => v.activeSelf).ToExpression(vm => vm.IsLock);
            bindingSet.Bind(unLockStar).For(v => v.activeSelf).ToExpression(vm => !vm.IsLock);
            bindingSet.Bind(this).For(v => v.Star).ToExpression(vm => vm.Star);
            bindingSet.Bind(heroTarget).For(v => v.activeSelf).ToExpression(vm => vm.HeroTarget);
            bindingSet.Bind(effectGo).For(v => v.activeSelf).ToExpression(vm => vm.EffectGo);
            bindingSet.Bind(this).For(v => v.SkillDesAlpha).ToExpression(vm => vm.SkilDesStAlpha);
            bindingSet.Build();
        }
    }
}