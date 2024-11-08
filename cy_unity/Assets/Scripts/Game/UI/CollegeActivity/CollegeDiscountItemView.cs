using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;
namespace DH.Game.UIViews.ItemViews
{
    public partial class CollegeDiscountItemView : BaseItemView
    {
        public DhText levelTxt;
        public CellItemView itemFree;
        public StaticItemsBindComponent staticItemsBindComponent;
        
        public GameObject itemFreeOffBg;
        public GameObject itemLv1OffBg;
        public GameObject levelOffBg;
        public GameObject progressBar;
        public GameObject line;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindSet = this.CreateBindingSet<CollegeDiscountItemView, CollegeDiscountItemModel>();
            bindSet.Bind(levelTxt).For(v => v.text).To(vm => vm.Cfg.Factor);
            bindSet.Bind(itemFree.BindingContext).For(v => v.DataContext).To(vm => vm.ItemFreeData);
            bindSet.Bind(staticItemsBindComponent).For(v=>v.BindDictionaryGetValueFunc).To(vm=>vm.GetOutSideRewardByIndex);
            bindSet.Bind(staticItemsBindComponent).For(v => v.Collection).To(vm => vm.Items);
            bindSet.Bind(itemFreeOffBg.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ItemFreeData.State == ECellItemState.None);
            bindSet.Bind(itemLv1OffBg.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ItemFreeData.State == ECellItemState.None);
            bindSet.Bind(levelOffBg.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ItemFreeData.State == ECellItemState.None);
            bindSet.Bind(progressBar).For(v => v.activeSelf).ToExpression(vm => vm.ItemFreeData.State != ECellItemState.None);
            bindSet.Bind(line).For(v => v.activeSelf).To(vm => vm.ShowLine);
            bindSet.Build();
        }
    }
}