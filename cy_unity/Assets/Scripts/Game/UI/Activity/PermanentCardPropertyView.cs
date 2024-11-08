using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class PermanentCardPropertyView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhText text;
        public Text nameText;
        public GameObject headEffectGo;
        public GameObject nameEffectGo;
        public GameObject freeAdEffectGo;
        public UICircularScrollView awardScrollview;
        [AssetPath]public string awardScrollviewCell;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            awardScrollview.PrefabPath = awardScrollviewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<PermanentCardPropertyView, PermanentCardPropertyViewModel>();
			bindingSet.Bind(text).For(v => v.text).To(vm => vm.TextStr);
            bindingSet.Bind(headEffectGo).For(v => v.activeSelf).ToExpression(vm => vm.ShowHeadEffect);
            bindingSet.Bind(nameEffectGo).For(v => v.activeSelf).ToExpression(vm => vm.ShowNameEffect);
            bindingSet.Bind(freeAdEffectGo).For(v => v.activeSelf).ToExpression(vm => vm.ShowFreeAdEffect);
            bindingSet.Bind(awardScrollview).For(v => v.Collection).To(vm => vm.AwardScrollviewList);
            bindingSet.Bind(nameText).For(v => v.text).To(vm => vm.NameStr);
            bindingSet.Build();
        }
    }
}