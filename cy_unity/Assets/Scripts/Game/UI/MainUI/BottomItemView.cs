using System;
using DH.Data;
using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
namespace DH.Game.UIViews
{
    public partial class BottomItemView : BaseItemView
    {
        public override bool FullScreen => true;
        
		public RectTransform effectImgRect;
		public ScrollRectExtend opBtnScrollview;
		[AssetPath]public string opBtnScrollviewCell;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
	        opBtnScrollview.PrefabPath = opBtnScrollviewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<BottomItemView, BottomItemViewModel>();
            bindingSet.Bind(this).For(v => v.effectImgRect).To(vm => vm.EffectImgRect).OneWayToSource();
			// bindingSet.Bind(effectImgRect).For(v => v.anchoredPosition).ToExpression(vm => GetEffectPos(vm.EffectImgRectPos));
			bindingSet.Bind(opBtnScrollview).For(v => v.Collection).To(vm => vm.OpBtnScrollviewList);
            bindingSet.Build();

            SetEffectImgSize();
        }

        private void SetEffectImgSize()
        {
	        var totalWidth = opBtnScrollview.gameObject.GetComponent<RectTransform>().rect.width;
	        var count = Enum.GetValues(typeof(ETabType)).Length;
            var effectWidth = totalWidth / count + 12;
            effectImgRect.sizeDelta = new Vector2(effectWidth, effectImgRect.rect.height);

        }

    }
}