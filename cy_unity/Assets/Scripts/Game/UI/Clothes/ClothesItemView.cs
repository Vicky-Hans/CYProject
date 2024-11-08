using System.Collections.Generic;
using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class ClothesItemView : BaseItemView
    {
        public override bool FullScreen => false;
        public RectTransform bgSize;
        public GameObject titleNode;
		public DhText titleName;
		public StaticItemsBindComponent grid;
        public List<RectTransform> rectList;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<ClothesItemView, ClothesItemViewModel>();
            bindingSet.Bind(bgSize).For(v => v.sizeDelta).To(vm => vm.BgSize);
			bindingSet.Bind(titleName).For(v => v.text).To(vm => vm.TitleNameStr);
            bindingSet.Bind(titleNode).For(v => v.activeSelf).ToExpression(vm => vm.TitleNameStr!=string.Empty);
			bindingSet.Bind(grid).For(v => v.BindDictionaryGetValueFunc).To(vm => vm.GetGridCellCallback);
			bindingSet.Bind(grid).For(v => v.Collection).To(vm => vm.GridDictionary);
            bindingSet.Bind(this).For(v => v.rectList).To(vm => vm.RectList).OneWayToSource();
            bindingSet.Build();
        }
    }
}