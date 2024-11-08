using DH.Config;
using DH.Data;
using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class AdFreePlusPermanentCardItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
        public UICircularScrollView awardScrollview;
        [AssetPath]public string awardScrollviewCell;
        public DhButton buyButton;
        public BtnPriceNode priceNode;
        public GameObject grayButton;
        public DhText adFreeEffectDes;
        public CommonPlayerNameView commonPlayerNameView;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
            awardScrollview.PrefabPath = awardScrollviewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<AdFreePlusPermanentCardItemView, AdFreePlusPermanentCardItemViewModel>();
            bindingSet.Bind(awardScrollview).For(v => v.Collection).To(vm => vm.AwardScrollviewList);
            bindingSet.Bind(buyButton).For(v => v.onClick).To(vm => vm.OnClickBuyButtonCommand);
            bindingSet.Bind(priceNode.BindingContext).For(v => v.DataContext).To(vm => vm.BtnPriceNodeVm);
            bindingSet.Bind(commonPlayerNameView.BindingContext).For(v => v.DataContext).To(vm => vm.CommonPlayerNameVm);
            bindingSet.Bind(grayButton.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsShowGrayButton);
            bindingSet.Bind(buyButton.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowGrayButton);
            bindingSet.Build();
            InitEffectDes();
        }

        private void InitEffectDes()
        {
            var cfg = ConfigCenter.MonthlyVipEffectLanguageCfgColl.GetDataById(
                (int)MonthCardEffectType.ADFreeForever);
            adFreeEffectDes.text = cfg.Dec;
        }
    }
}