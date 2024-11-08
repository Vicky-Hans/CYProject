using System;
using DH.Config;
using DH.Data;
using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class AdFreePlusPrivilegeMonthCardItemView : BaseItemView
    {

        public override bool FullScreen => false;
        
        public UICircularScrollView awardScrollview;
        [AssetPath]public string awardScrollviewCell;
        public DhButton buyButton;
        public CellItemBaseView atOnceAward;
        public BtnPriceNode priceNode;
        public DhText title;
        public GameObject grayButton;
        public DhText adFreeEffectDes;
        public DhButton otherEffectDes;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            awardScrollview.PrefabPath = awardScrollviewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<AdFreePlusPrivilegeMonthCardItemView, AdFreePlusPrivilegeMonthCardItemViewModel>();
            bindingSet.Bind(awardScrollview).For(v => v.Collection).To(vm => vm.AwardScrollviewList);
            bindingSet.Bind(buyButton).For(v => v.onClick).To(vm => vm.OnClickBuyButtonCommand);
            bindingSet.Bind(atOnceAward.BindingContext).For(v => v.DataContext).To(vm => vm.AtOnceAwardCellVm);
            bindingSet.Bind(priceNode.BindingContext).For(v => v.DataContext).To(vm => vm.BtnPriceNodeVm);
            bindingSet.Bind(title).For(v => v.text).To(vm => vm.TitleStr);
            bindingSet.Bind(grayButton.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsShowGrayButton);
            bindingSet.Bind(buyButton.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowGrayButton);
            bindingSet.Bind(otherEffectDes).For(v => v.onClick).To(vm => vm.OnClickIconBtn)
                .CommandParameter(() => new Tuple<Vector3, Vector3>(otherEffectDes.transform.position, new Vector3(0,20,0)));
            bindingSet.Build();
            InitEffectDes();
        }

        private void InitEffectDes()
        {
            adFreeEffectDes.text = LocalizeHelper.GetGlobal(GlobalLanguageId.Shop_tips20);
        }
    }
}