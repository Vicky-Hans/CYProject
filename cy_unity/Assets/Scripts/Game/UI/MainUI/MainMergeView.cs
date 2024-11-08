using DH.Game.UI;
using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DH.Data;

namespace DH.Game.UIViews
{
    public partial class MainMergeView : BaseItemView
    {
        public RectTransform bgImageRect;
        public ScrollRectExtend gridScroll;
        public ScrollRectExtend weaponListScroll;
        public ScrollRectExtend randomScroll;
        public SubViewBinder dragBatchItem;
        public SubViewBinder dragWeaponItem;
        public SubViewBinder dragGridAddItem;
        public RectTransform dragAreaRect;
        public RectTransform girdRect;
        public RectTransform batchItemRect;
        public RectTransform weaponItemRect;
        public RectTransform gridAdditemRect;
        public CanvasGroup dragBatchCanvasGroup;
        public CanvasGroup dragWeaponCanvasGroup;
        public CanvasGroup dragGridAddCanvasGroup;
        public RectTransform flyEndRect;
        [AssetPath] public string[] itemPrefab;
        public WishItemView wishView;
        private DragEventTriggerListener dragAreaCmp;
        private RectTransform weaponListRect;
        
        public override async UniTask Create()
        {
            gridScroll.PrefabPath = itemPrefab[0];
            dragBatchItem.PrefabPath = itemPrefab[3];
            dragWeaponItem.PrefabPath = itemPrefab[2];
            dragGridAddItem.PrefabPath = itemPrefab[1];
            weaponListScroll.PrefabPath = itemPrefab[2];
            randomScroll.PrefabPath = itemPrefab[4];
            weaponListRect = weaponListScroll.GetComponent<RectTransform>();
            dragAreaCmp = dragAreaRect.GetComponent<DragEventTriggerListener>();
            await base.Create();
            var bindingSet = this.CreateBindingSet<MainMergeView, MainMergeViewModel>();
            bindingSet.Bind(gridScroll).For(v => v.Collection).To(vm => vm.GridList);
            bindingSet.Bind(weaponListScroll).For(v => v.Collection).To(vm => vm.WeaponList);
            bindingSet.Bind(randomScroll).For(v => v.Collection).To(vm => vm.RandomBagList);
            bindingSet.Bind(dragBatchItem).For(v => v.Source).To(vm => vm.DragBatchItemVm);
            bindingSet.Bind(dragWeaponItem).For(v => v.Source).To(vm => vm.DragWeaponItemVm);
            bindingSet.Bind(dragGridAddItem).For(v => v.Source).To(vm => vm.DragGridAddItemVm);
            bindingSet.Bind(girdRect).For(v => v.sizeDelta).ToExpression(vm => CalculationGridSize(vm.GridSize));
            bindingSet.Bind(bgImageRect).For(v => v.sizeDelta).ToExpression(vm => CalculationBgSize(vm.GridSize));
            bindingSet.Bind(weaponListRect).For(v => v.sizeDelta).ToExpression(vm => CalculationGridSize(vm.GridSize));
            bindingSet.Bind(dragBatchCanvasGroup).For(v => v.alpha).ToExpression(vm => GetBatchCanvasGroup(vm.Manager.BlockState));
            bindingSet.Bind(dragWeaponCanvasGroup).For(v => v.alpha).ToExpression(vm => GetWeaponCanvasGroup(vm.DragState));
            bindingSet.Bind(dragGridAddCanvasGroup).For(v => v.alpha).ToExpression(vm => GetBatchCanvasGroup(vm.Manager.BlockState));
            bindingSet.Bind(dragAreaCmp).For(v => v.OnDragHandle).To(vm => vm.DragFunc);
            bindingSet.Bind(dragAreaCmp).For(v => v.EndDragHandle).To(vm => vm.EndDragFunc);
            bindingSet.Bind(dragAreaCmp).For(v => v.BeginDragHandle).To(vm => vm.BeginDragFunc);
            bindingSet.Bind(dragAreaCmp).For(v => v.PointerClickHandle).To(vm => vm.PointerClickFunc);
            bindingSet.Bind(dragAreaCmp).For(v => v.PointerDownHandler).To(vm => vm.PointerClickDownFunc);
            bindingSet.Bind(dragAreaCmp).For(v => v.PointerUpHandler).To(vm => vm.PointerClickUpFunc);
            bindingSet.Bind(this).For(v => v.dragAreaRect).To(vm => vm.DragAreaRect).OneWayToSource();
            bindingSet.Bind(this).For(v => v.girdRect).To(vm => vm.GridListRect).OneWayToSource();
            bindingSet.Bind(this).For(v => v.batchItemRect).To(vm => vm.BatchItemRect).OneWayToSource();
            bindingSet.Bind(this).For(v => v.weaponItemRect).To(vm => vm.WeaponItemRect).OneWayToSource();
            bindingSet.Bind(this).For(v => v.gridAdditemRect).To(vm => vm.GridAddItemRect).OneWayToSource();
            bindingSet.Bind(this).For(v => v.flyEndRect).To(vm => vm.FlyEndRect).OneWayToSource();
            bindingSet.Bind(this).For(v=>v.wishView).To(vm=>vm.WishView).OneWayToSource();
            bindingSet.Bind(wishView.BindingContext).For(v=>v.DataContext).To(vm=>vm.WishVm);
            bindingSet.Build();
        }
        /// <summary>
        /// 获取批量拖动的CanvasGroup值
        /// </summary>
        /// <returns></returns>
        private static float GetBatchCanvasGroup(EBlockState curState)
        {
            float curValue = 0;
            if (curState == EBlockState.AddCell) curValue = 1;
            return curValue;
        }
        /// <summary>
        /// 获取武器拖动的CanvasGroup值
        /// </summary>
        /// <returns></returns>
        private static float GetWeaponCanvasGroup(EDragState curState)
        {
            float curValue = 0;
            if (curState == EDragState.Weapon) curValue = 1;
            return curValue;
        }
        private static Vector2 CalculationGridSize(Vector2Int gridSize)
        {
            var newWidth = gridSize.y * GameManager.Instance.CellSize;
            var newHeight = gridSize.x * GameManager.Instance.CellSize;
            return new Vector2(newWidth,newHeight);
        }
        private static Vector2 CalculationBgSize(Vector2Int gridSize)
        {
            var newWidth = gridSize.y * GameManager.Instance.CellSize+60;
            var newHeight = gridSize.x * GameManager.Instance.CellSize+60;
            return new Vector2(newWidth,newHeight);
        }
    }
}