using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class NewbieView : BaseView
    {
        public override bool FullScreen => false;
        
        public UICircularScrollView scrollView;
        [AssetPath]public string scrollViewCell;

        public DhText heroNameText;
        public DhButton buyButton;
        public BtnPriceNode priceNode;
        public GameObject getAwardTextGo;
        public GameObject getAwardGo;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            scrollView.PrefabPath = scrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<NewbieView, NewbieViewModel>();
            bindingSet.Bind(buyButton).For(v => v.onClick).To(vm => vm.OnClickBuyButtonCommand);
            bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.RewardsList);
            bindingSet.Bind(priceNode.BindingContext).For(v => v.DataContext).To(vm => vm.PriceNodeModel);
            bindingSet.Bind(priceNode.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.Data.IsBuy);
            bindingSet.Bind(getAwardGo).For(v => v.activeSelf).ToExpression(vm => vm.GetAwardGo);
            bindingSet.Bind(getAwardTextGo).For(v => v.activeSelf).ToExpression(vm => vm.Data.IsBuy);
            bindingSet.Bind(heroNameText).For(v => v.text).To(vm => vm.HeroNameStr);    
            bindingSet.Build();
        }
    }
}