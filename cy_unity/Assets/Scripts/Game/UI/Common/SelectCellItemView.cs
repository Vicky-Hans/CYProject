using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class SelectCellItemView : BaseItemView,IViewKey
    {
        public override bool FullScreen => false;
        
		public DhButton selectItem;
        public CellItemBaseView cellItemBaseView;
        public GameObject selectGo;
        public GameObject selectIconGo;
        
        public DhImage butImg;

        public object Key => index;

        public int index;

        private bool butEnabled;
        public bool ButEnabled
        {
            get => butEnabled;
            set
            {
                butEnabled = value;
                butImg.enabled = butEnabled;
            }
        }

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<SelectCellItemView, SelectCellItemViewModel>();
			bindingSet.Bind(selectItem).For(v => v.onClick).To(vm => vm.OnClickSelectItemCommand);
            bindingSet.Bind(cellItemBaseView.BindingContext).For(v => v.DataContext).To(vm => vm.CellItemBaseViewVm);
            bindingSet.Bind(selectGo.transform).For(v => v.localScale).ToExpression(vm => GetSize(vm.CellItemBaseViewVm.SizeBg));
            //bindingSet.Bind(selectIconGo.transform).For(v => v.localScale).ToExpression(vm => GetSize(vm.CellItemBaseViewVm.SizeIcon));
            bindingSet.Bind(selectGo).For(v => v.activeSelf).ToExpression(vm => vm.IsSelect);
            bindingSet.Bind(cellItemBaseView.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsSelect);
            bindingSet.Bind(selectItem.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsSelectType);
            bindingSet.Bind(selectItem.transform).For(v => v.localScale).ToExpression(vm => GetSize(vm.CellItemBaseViewVm.SizeBg));
            bindingSet.Bind(this).For(v => v.ButEnabled).ToExpression(vm => vm.IsButEnabled);
            bindingSet.Build();
        }
        private Vector3 GetSize(Vector2 sizeBg)
        {
            return new Vector3(sizeBg.x / 166, sizeBg.y / 166, 1);
        }

    }
}