using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class ShopDrawClothesInfoItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
        public RectTransform bgRect;
		public RectTransform showDIs;
		public DhText titleName;
		public DhText titleValue;
		public RectTransform itemBg;
		public GameObject titleNode;
		public StaticItemsBindComponent grid;
		// public CellItemBaseView[] itemList;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<ShopDrawClothesInfoItemView, ShopDrawClothesInfoItemViewModel>();
            bindingSet.Bind(bgRect).For(v => v.sizeDelta).To(vm => vm.ShowBgSize);
            bindingSet.Bind(showDIs).For(v => v.sizeDelta).To(vm => vm.ShowBgSize);
			bindingSet.Bind(titleName).For(v => v.text).To(vm => vm.TitleNameStr);
			bindingSet.Bind(titleValue).For(v => v.text).To(vm => vm.TitleValueStr);
			bindingSet.Bind(itemBg).For(v => v.sizeDelta).To(vm => vm.ItemSize);
			bindingSet.Bind(titleNode.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.TitleNameStr!=string.Empty);
			bindingSet.Bind(grid).For(v => v.BindDictionaryGetValueFunc).To(vm => vm.GetGridCellCallback);
			bindingSet.Bind(grid).For(v => v.Collection).To(vm => vm.GridDictionary);
			// for (int i = 0; i < itemList.Length; i++)
			// {
			// 	int pos = i;
			// 	bindingSet.Bind(itemList[i].BindingContext).For(v => v.DataContext).ToExpression(vm => vm.GridDictionary[pos]);
			// 	bindingSet.Bind(itemList[i].gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ShowItemNum>pos);
			// }

            bindingSet.Build();
        }

    }
}