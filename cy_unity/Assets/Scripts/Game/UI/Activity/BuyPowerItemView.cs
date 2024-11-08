using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class BuyPowerItemView : BaseItemView
    {
        public DhText titleText;
        public DhText numsText;
        public DhButton buyBut;
        public DhText butText;
        public GameObject buyOutGo;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<BuyPowerItemView, BuyPowerItemViewModel>();
            bindingSet.Bind(titleText).For(v => v.text).To(vm => vm.TitleText);
            bindingSet.Bind(numsText).For(v => v.text).To(vm => vm.NumsText);
            bindingSet.Bind(buyBut).For(v => v.onClick).To(vm => vm.OnClickOpBtn);
            bindingSet.Bind(butText).For(v => v.text).To(vm => vm.ButText);
            bindingSet.Bind(butText).For(v => v.text).To(vm => vm.ButText);
            bindingSet.Bind(buyOutGo).For(v => v.activeSelf).To(vm => vm.BuyOutGo);
            bindingSet.Build();
        }

    }
}