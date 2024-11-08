using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class ReviveView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhText timeText;
		public DhButton opBtn;
		public ItemPriceNodeView costItem;
		public ClickToClose clickToClose;
		public Button closeBtn;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<ReviveView, ReviveViewModel>();
			bindingSet.Bind(timeText).For(v => v.text).To(vm => vm.TimeTextStr);
			bindingSet.Bind(opBtn).For(v => v.onClick).To(vm => vm.OnClickOpBtnCommand);
			bindingSet.Bind(costItem.BindingContext).For(v => v.DataContext).To(vm => vm.CostVm);
			bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtn);
			bindingSet.Bind(clickToClose).For(v => v.CloseCallback).To(vm => vm.OnClickCloseBtn);
            bindingSet.Build();
        }
    }
}