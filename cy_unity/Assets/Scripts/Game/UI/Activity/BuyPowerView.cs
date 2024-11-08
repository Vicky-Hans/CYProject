using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class BuyPowerView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhButton closeBtn;
        public BuyPowerItemView itemView1;
        public BuyPowerItemView itemView2;
        public BuyPowerItemView itemView3;
        public CommonTopView topView;
        public RectTransform bgRect;
        public CommonAdvIconView commonAdv;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<BuyPowerView, BuyPowerViewModel>();
			bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtnCommand);
            bindingSet.Bind(itemView1.BindingContext).For(v => v.DataContext).To(vm => vm.BuyPowerItem1);
            bindingSet.Bind(itemView2.BindingContext).For(v => v.DataContext).To(vm => vm.BuyPowerItem2);
            bindingSet.Bind(itemView3.BindingContext).For(v => v.DataContext).To(vm => vm.BuyPowerItem3);
            bindingSet.Bind(topView.BindingContext).For(v => v.DataContext).To(vm => vm.TopViewModel);
            bindingSet.Bind(this).For(v => v.bgRect).To(vm => vm.BgRect).OneWayToSource();
            bindingSet.Bind(commonAdv.BindingContext).For(v => v.DataContext).To(vm => vm.CommonAdvVm);
            bindingSet.Build();
        }
    }
}