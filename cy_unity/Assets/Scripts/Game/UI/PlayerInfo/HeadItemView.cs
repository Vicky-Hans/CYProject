using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;


namespace DH.Game.UIViews.ItemViews
{
    public partial class HeadItemView : BaseItemView
    {
        public CommonHeadItemView CommonHead;
        public DhImage outline;
        public DhButton selectBtn;
        public GameObject lockNode;
        public GameObject useNode;
        public GameObject newTag;
        public DhText name;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<HeadItemView, HeadItemViewModel>();
            bindingSet.Bind(CommonHead.BindingContext).For(v => v.DataContext).To(vm => vm.CommonHeadVm);
            bindingSet.Bind(outline).For(v => v.enabled).To(vm => vm.Selected);
            bindingSet.Bind(selectBtn).For(v => v.onClick).To(vm => vm.SelectCmd);
            bindingSet.Bind(name).For(v => v.text).To(vm => vm.NameText);
            bindingSet.Bind(lockNode).For(v => v.activeSelf).To(vm => vm.IsLock);
            bindingSet.Bind(useNode).For(v => v.activeSelf).To(vm => vm.Use);
            bindingSet.Bind(newTag).For(v => v.activeSelf).To(vm => vm.IsNew);
           // bindingSet.Bind(this).For(v => v.effectTransform).To(vm => vm.EffectTransform).OneWayToSource();
            bindingSet.Bind(this.newTag).For(v => v.activeSelf).To(vm => vm.IsNew);
            bindingSet.Build();
        }
    }
}