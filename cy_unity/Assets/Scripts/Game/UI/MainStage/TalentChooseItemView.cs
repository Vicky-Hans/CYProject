using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class TalentChooseItemView : BaseItemView
    {
        public override bool FullScreen => false;

        public Transform cellRect;
		public DhButton cellBg;
		public TalentCellItemView talentCellItemView;
		public DhText descText;
        public GameObject tipsNode;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<TalentChooseItemView, TalentChooseItemViewModel>();
            bindingSet.Bind(cellBg).For(v => v.onClick).To(vm => vm.OnClickCellBgBtnCommand);
            bindingSet.Bind(cellBg).For(v => v.interactable).To(vm => vm.IsCanOp);
            bindingSet.Bind(cellBg.image).For(v => v.sprite).To(vm => vm.CellBgImgPath).WithConversion(this);
			bindingSet.Bind(talentCellItemView.BindingContext).For(v => v.DataContext).To(vm => vm.TalentCellItemViewVm);
			bindingSet.Bind(descText).For(v => v.text).To(vm => vm.DescTextStr);
            bindingSet.Bind(cellRect).For(v => v.localScale).To(vm => vm.CellScale);
            bindingSet.Bind(tipsNode).For(v=>v.activeSelf).To(vm=>vm.IsShowTips);
            bindingSet.Build();
        }
    }
}