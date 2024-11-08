using System;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine.UI;
using UnityEngine;
namespace DH.Game.UIViews
{
    public partial class TalentCellItemView : BaseItemView
    {
        public override bool FullScreen => false;
        public RectTransform cellRect;
		public Image bg;
		public Image tagBg;
		public GameObject effectNode;
		public Image icon;
		public GameObject bgNode;
		public GameObject noTipNode;
		public GameObject effectRect;
		public DhButton opBtn;
		public DhText chooseCountText;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
			// addOnScrollView.PrefabPath = addOnScrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<TalentCellItemView, TalentCellItemViewModel>();
			bindingSet.Bind(effectNode).For(v => v.activeSelf).To(vm => vm.IsShowEffectNode);
			bindingSet.Bind(icon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);
			bindingSet.Bind(cellRect).For(v => v.sizeDelta).To(vm => vm.CellSize);
			bindingSet.Bind(bgNode).For(v=>v.activeSelf) .To(vm => vm.IsShowBg);
			bindingSet.Bind(noTipNode).For(v=>v.activeSelf) .ToExpression(vm => !vm.IsShowBg);
			bindingSet.Bind(effectRect.transform).For(v => v.localScale).To(vm => vm.EffectScale);
			bindingSet.Bind(opBtn).For(v => v.onClick).To(vm => vm.OnClickOpBtnCommand).CommandParameter(() => new Tuple<Vector3, Vector3>(icon.transform.position, new Vector3(0,20,0)));
			bindingSet.Bind(chooseCountText).For(v=>v.text).To(vm => vm.ChooseCountTextStr);
            bindingSet.Build();
        }
    }
}