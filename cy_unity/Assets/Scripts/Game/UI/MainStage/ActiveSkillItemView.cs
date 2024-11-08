using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class ActiveSkillItemView : BaseItemView
    {
        public override bool FullScreen => false;
        public DhButton skillBtn;
		public DhImage icon;
		public DhImage maskImg;
		public DhImage progress;
		public GameObject effectNode;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<ActiveSkillItemView, ActiveSkillItemViewModel>();
            bindingSet.Bind(skillBtn).For(v => v.onClick).To(vm => vm.OnClickSkillBtnCommand);
            // bindingSet.Bind(skillBtn).For(v => v.interactable).To(vm => vm.Manager.IsCanUseHeroActiveSkill);
			bindingSet.Bind(icon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);
			bindingSet.Bind(maskImg).For(v => v.fillAmount).To(vm => vm.Progress);
			bindingSet.Bind(progress).For(v => v.fillAmount).ToExpression(vm =>  1 - vm.Progress);
			bindingSet.Bind(effectNode).For(v => v.activeSelf).To(vm => vm.Manager.IsCanUseHeroActiveSkill);
            bindingSet.Build();
        }
    }
}