using System;
using DH.Config;
using DH.Game.ViewModels;
using DH.UIFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game.UIViews.ItemViews
{
    public partial class BtnPriceNode : BaseItemView
    {
        public Image icon;
        public Text priceTxt;
        public TextMeshProUGUI priceTmp;
        [NonSerialized]
        public Color startColor;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            startColor = priceTxt.color;
            var bindingSet = this.CreateBindingSet<BtnPriceNode, BtnPriceNodeModel>();
            bindingSet.Bind(this).For(v => v.startColor).To(vm => vm.StartColor).OneWayToSource();
            bindingSet.Bind(icon).For(v => v.sprite).ToExpression(vm => GetIcon(vm.Reward)).WithConversion(this);
            bindingSet.Bind(icon.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowIcon);
            bindingSet.Bind(priceTxt).For(v => v.text).To(vm => vm.PriceStr);
            bindingSet.Bind(priceTxt).For(v => v.color).ToExpression(vm => vm.CostColor());
            bindingSet.Bind(priceTxt.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowTmp);
            bindingSet.Bind(priceTmp).For(v => v.text).To(vm => vm.PriceStr);
            bindingSet.Bind(priceTmp).For(v => v.color).ToExpression(vm => vm.CostColor());
            bindingSet.Bind(priceTmp.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowTmp);
            bindingSet.Build();
        }

        protected string GetIcon(Reward reward)
        {
            if (reward == null) return "";
            return UIHelper.GetRewardsIconPath(reward.Type, reward.Id);
        }
    }
}