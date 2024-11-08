using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class SelectCellItemEffectView : BaseItemView
    {
	    
	    public override bool FullScreen => false;
	    public DhButton selectItem;
	    public CellItemView cellItemView;
	    public GameObject selectGo;
	    public GameObject selectTips;
	    public override async Cysharp.Threading.Tasks.UniTask Create()
	    {
		    await base.Create();
		    var bindingSet = this.CreateBindingSet<SelectCellItemEffectView, SelectCellItemEffectViewModel>();
		    bindingSet.Bind(selectItem).For(v => v.onClick).To(vm => vm.OnClickSelectItemCommand);
		    bindingSet.Bind(cellItemView.BindingContext).For(v => v.DataContext).To(vm => vm.CellItemBaseViewVm);
		    bindingSet.Bind(selectGo.transform).For(v => v.localScale).ToExpression(vm => vm.CellItemBaseViewVm.CellItemBaseViewVm.ItemScale);
		    bindingSet.Bind(selectGo).For(v => v.activeSelf).To(vm => vm.IsSelect);
		    bindingSet.Bind(cellItemView.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsSelect);
		    bindingSet.Bind(selectTips.transform).For(v => v.localScale).ToExpression(vm => vm.CellItemBaseViewVm.CellItemBaseViewVm.ItemScale);
		    bindingSet.Bind(selectTips).For(v => v.activeSelf).To(vm => vm.SelectType == SelectItemType.Select);
		    bindingSet.Build();
	    }
    }
}