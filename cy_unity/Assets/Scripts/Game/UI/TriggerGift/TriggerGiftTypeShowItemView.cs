using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class TriggerGiftTypeShowItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhText name;
        public DhButton but;
        public DhImage Icon;
        public GameObject IsSelect;
        public GameObject red;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<TriggerGiftTypeShowItemView, TriggerGiftTypeShowItemViewModel>();
            
			bindingSet.Bind(name).For(v => v.text).To(vm => vm.NameStr);
            bindingSet.Bind(but).For(v => v.onClick).To(vm => vm.OnClickButCommand);
            bindingSet.Bind(Icon).For(v => v.sprite).To(vm => vm.Icon).WithConversion(this);
            bindingSet.Bind(IsSelect).For(v => v.activeSelf).To(vm => vm.Select);
            bindingSet.Bind(red).For(v => v.activeSelf).ToExpression(vm => vm.Red);
            bindingSet.Build();
        }
    }
}