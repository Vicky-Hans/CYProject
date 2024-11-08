using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class ClothesMergeTopItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public GameObject add;
		public GameObject noneNode;
		public GameObject baseNode;
		public GameObject baseItemNode;
		public GameObject infoNode;
		public CellItemBaseView cellItemBaseView;
		public CellItemBaseView cellItemShowView;

		public DhImage baseBg;
		public DhImage baseIcon;
		public DhImage baseQuality;
		public DhImage basePart;
		public RectTransform movePos;
		
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<ClothesMergeTopItemView, ClothesMergeTopItemViewModel>();

            bindingSet.Bind(add).For(v => v.activeSelf).ToExpression(vm => vm.IsShowAdd && vm.ShowState != ClothesMergeItemState.None);
			bindingSet.Bind(noneNode).For(v => v.activeSelf).ToExpression(vm => vm.ShowState == ClothesMergeItemState.None);
			bindingSet.Bind(baseNode).For(v => v.activeSelf).ToExpression(vm => GetSketchShow(vm.ShowState,vm.IsShowExit,vm.ShowSketchType));
			bindingSet.Bind(baseItemNode).For(v => v.activeSelf).ToExpression(vm => GetSketchItemShow(vm.ShowState,vm.IsShowExit,vm.ShowSketchType));
			
			bindingSet.Bind(infoNode).For(v => v.activeSelf).ToExpression(vm => (vm.ShowState == ClothesMergeItemState.Reward || vm.ShowState == ClothesMergeItemState.HeroEquip) && vm.IsShowExit );
			bindingSet.Bind(cellItemBaseView.BindingContext).For(v => v.DataContext).To(vm => vm.CellItemBaseViewVm);
			bindingSet.Bind(cellItemShowView.BindingContext).For(v => v.DataContext).To(vm => vm.CellItemViewShowVm);
			bindingSet.Bind(baseBg).For(v => v.sprite).To(vm => vm.BaseBgPath).WithConversion(this);
			bindingSet.Bind(baseIcon).For(v => v.sprite).To(vm => vm.BaseIconPath).WithConversion(this);
			bindingSet.Bind(baseQuality).For(v => v.sprite).To(vm => vm.QualityPath).WithConversion(this);
			bindingSet.Bind(basePart).For(v => v.sprite).To(vm => vm.PartPath).WithConversion(this);
			bindingSet.Bind(this).For(v => v.movePos).To(vm => vm.NoneTransform).OneWayToSource();
			
            bindingSet.Build();
        }

        private bool GetSketchShow(ClothesMergeItemState state,bool isShow,int type)
        {
	        return type==2 && (state == ClothesMergeItemState.Sketch || ((state == ClothesMergeItemState.Reward || state == ClothesMergeItemState.HeroEquip) && !isShow));
        }
        
        private bool GetSketchItemShow(ClothesMergeItemState state,bool isShow,int type)
        {
	        return type!=2 && (state == ClothesMergeItemState.Sketch || ((state == ClothesMergeItemState.Reward || state == ClothesMergeItemState.HeroEquip) && !isShow));
        }
    }
}