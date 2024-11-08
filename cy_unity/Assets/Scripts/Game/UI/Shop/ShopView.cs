using System;
using System.Collections.Generic;
using DH.Data;
using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class ShopView : BaseItemView
    {
        public override bool FullScreen => true;

        public TabBtnGroupView tabBtnGroupView;
        public ShopDrawView shopDrawView;
        public ShopChapterView shopChapterView;
        public ShopDailyView shopDailyView;
        public ShopDiamondView shopDiamondView;
        public ShopGoldView shopGoldView;
        public ShopDrawClothesView shopDrawClothes;
        public ScrollRect scrollView;
        
        [NonSerialized]
        public int ShowPos;

        [NonSerialized] public int index;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindSet = this.CreateBindingSet<ShopView, ShopViewModel>();
            bindSet.Bind(tabBtnGroupView.BindingContext).For(v => v.DataContext).To(vm => vm.TabBtnViewModel);
            bindSet.Bind(shopDrawView.BindingContext).For(v => v.DataContext).To(vm => vm.ShopDrawViewModel);
            bindSet.Bind(shopDrawView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.SelectShopTitle==vm.ShopDraw);
            bindSet.Bind(shopChapterView.BindingContext).For(v => v.DataContext).To(vm => vm.ShopChapterViewModel);
            bindSet.Bind(shopChapterView.gameObject).For(v => v.activeSelf).ToExpression(vm => !GameConst.IsIosAuditState && vm.SelectShopTitle==vm.ShopChapter);
            bindSet.Bind(shopDailyView.BindingContext).For(v => v.DataContext).To(vm => vm.ShopDailyViewModel);
            bindSet.Bind(shopDailyView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.SelectShopTitle==vm.ShopDaily);
            bindSet.Bind(shopDiamondView.BindingContext).For(v => v.DataContext).To(vm => vm.ShopDiamondViewModel);
            bindSet.Bind(shopDiamondView.gameObject).For(v => v.activeSelf).ToExpression(vm => !GameConst.IsIosAuditState && vm.SelectShopTitle==vm.ShopDiamond);
            bindSet.Bind(shopGoldView.BindingContext).For(v => v.DataContext).To(vm => vm.ShopGoldViewModel);
            bindSet.Bind(shopGoldView.gameObject).For(v => v.activeSelf).ToExpression(vm => !GameConst.IsIosAuditState && vm.SelectShopTitle==vm.ShopGold);
            bindSet.Bind(shopDrawClothes.BindingContext).For(v => v.DataContext).To(vm => vm.ShopDrawClothesViewModel);
            bindSet.Bind(shopDrawClothes.gameObject).For(v => v.activeSelf).ToExpression(vm => !GameConst.IsIosAuditState && vm.SelectShopTitle==vm.ShopClothes);
            bindSet.Bind(this).For(v => v.index).ToExpression(vm => GetPosIndex(vm.SelectShopTitle));
            bindSet.Bind(this).For(v => v.ShowPos).ToExpression(vm => SetPosIndex(vm.ShowPosDic));
            bindSet.Build();
        }
        
        private int SetPosIndex(Dictionary<int,int> posArray)
        {
            shopDrawView.transform.SetSiblingIndex(posArray[3]);
            shopChapterView.transform.SetSiblingIndex(posArray[1]);
            shopDailyView.transform.SetSiblingIndex(posArray[2]);
            shopDiamondView.transform.SetSiblingIndex(posArray[4]);
            shopGoldView.transform.SetSiblingIndex(posArray[5]);
            shopDrawClothes.transform.SetSiblingIndex(posArray[7]);
            return 0;
        }

        private int GetPosIndex(ShopTitle vmSelectShopTitle)
        {
            if (index != (int)vmSelectShopTitle)
            {
                scrollView.normalizedPosition = new Vector2(0, 1);
            }
            
            return (int)vmSelectShopTitle;
        }
    }
}