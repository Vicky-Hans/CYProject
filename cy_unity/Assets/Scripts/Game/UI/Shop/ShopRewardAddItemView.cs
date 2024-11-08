using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class ShopRewardAddItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhImage bg;
		public DhText addCnt;
        public CellItemBaseView cellItemBaseView;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<ShopRewardAddItemView, ShopRewardAddItemViewModel>();
			bindingSet.Bind(bg).For(v => v.sprite).To(vm => vm.BgPath).WithConversion(this);
            bindingSet.Bind(gameObject.GetComponent<RectTransform>()).For(v => v.sizeDelta).To(vm => vm.ItemSize);
			bindingSet.Bind(addCnt).For(v => v.text).To(vm => vm.AddCntStr);
            bindingSet.Bind(cellItemBaseView.BindingContext).For(v => v.DataContext).To(vm => vm.CellItemBaseViewModel);
            bindingSet.Build();
        }
    }
}