using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class SecretTalentChooseItemView : BaseItemView
    {
        public override bool FullScreen => false;

        public DhButton talentBtn;
		public DhImage chooseImg;
		public TalentCellItemView talentCellItemView;
		public CellItemBaseView cellItemBaseView;
		public DhText descText;
		public GameObject tipsNode;
		public RectTransform cellRect;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<SecretTalentChooseItemView, SecretTalentChooseItemViewModel>();
			bindingSet.Bind(chooseImg.gameObject).For(v => v.activeSelf).To(vm => vm.IsChoose);
			bindingSet.Bind(talentBtn).For(v => v.onClick).To(vm => vm.OnClickTalentBtnCommand);
			bindingSet.Bind(talentBtn.image).For(v => v.sprite).To(vm => vm.CellBgImgPath).WithConversion(this);
			bindingSet.Bind(talentCellItemView.BindingContext).For(v => v.DataContext).To(vm => vm.TalentCellItemViewVm);
			bindingSet.Bind(cellItemBaseView.BindingContext).For(v => v.DataContext).To(vm => vm.CellItemBaseViewVm);
			bindingSet.Bind(talentCellItemView.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowTalent);
			bindingSet.Bind(cellItemBaseView.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowTalent);
			bindingSet.Bind(descText).For(v => v.text).To(vm => vm.DescTextStr);
			bindingSet.Bind(tipsNode).For(v => v.activeSelf).To(vm => vm.IsShowTips);
			bindingSet.Bind(cellRect).For(v => v.localScale).To(vm => vm.CellScale);
            bindingSet.Build();
        }
    }
}