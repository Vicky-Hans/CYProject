using System;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class InvitedRewardItemView : BaseItemView,IViewKey
    {
        public override bool FullScreen => false;
        
		public CellItemView cellItemView;
		public DhText progressText;
		public DhButton showIconBtn;
		public int index;
		public object Key => index;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<InvitedRewardItemView, InvitedRewardItemViewModel>();
			bindingSet.Bind(cellItemView.BindingContext).For(v => v.DataContext).To(vm => vm.CellItemViewVm);
			bindingSet.Bind(cellItemView.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowIconBtn);
			bindingSet.Bind(progressText).For(v => v.text).To(vm => vm.ProgressTextStr);
			bindingSet.Bind(showIconBtn).For(v => v.onClick).To(vm => vm.OnClickShowIconBtnCommand).CommandParameter(() => new Tuple<Vector3, Vector3>(showIconBtn.transform.position, new Vector3(0, 20, 0)));
			bindingSet.Bind(showIconBtn.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowIconBtn);
            bindingSet.Build();
        }

        
    }
}