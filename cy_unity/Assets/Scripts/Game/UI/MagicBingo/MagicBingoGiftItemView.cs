using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class MagicBingoGiftItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
        public DhText limitNum;
        public StaticItemsBindComponent itemGrid;
        public DhText titleName;
        public DhButton btnAd;
        public DhButton btnRecharge;
        public BtnPriceNode btnPriceNode;
        public GameObject buyEnd;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<MagicBingoGiftItemView, MagicBingoGiftItemViewModel>();
            
            bindingSet.Bind(limitNum).For(v => v.text).To(vm => vm.LimitNumStr);
            bindingSet.Bind(itemGrid).For(v => v.BindDictionaryGetValueFunc).To(vm => vm.GetItemGridCellCallback);
            bindingSet.Bind(itemGrid).For(v => v.Collection).To(vm => vm.ItemGridDictionary);
            bindingSet.Bind(titleName).For(v => v.text).To(vm => vm.TitleNameStr);
            bindingSet.Bind(btnAd).For(v => v.onClick).To(vm => vm.OnClickBtnRechargeCommand);
            bindingSet.Bind(btnRecharge).For(v => v.onClick).To(vm => vm.OnClickBtnRechargeCommand);
            bindingSet.Bind(btnPriceNode.BindingContext).For(v => v.DataContext).To(vm => vm.BtnPriceNodeVm);
			
            bindingSet.Bind(btnAd.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.State == ShopBuyState.Adv);
            bindingSet.Bind(btnRecharge.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.State == ShopBuyState.Money);
            bindingSet.Bind(buyEnd).For(v => v.activeSelf).ToExpression(vm => vm.State == ShopBuyState.Finish);

            bindingSet.Build();
        }
    }
}