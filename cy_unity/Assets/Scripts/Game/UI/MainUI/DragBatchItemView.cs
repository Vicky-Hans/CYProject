using Cysharp.Threading.Tasks;
using DH.Game.UI;
using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class DragBatchItemView: BaseItemView
    {
        public CanvasGroup canvasGroupObj;
        public RectTransform girdRect;
        public RectTransform batchRect;
        public RectTransform weaponRect;
        public ScrollRectExtend gridScroll;
        public ScrollRectExtend weaponListScroll;
        [AssetPath] public string[] itemPrefab;
        public override async UniTask Create()
        {
            gridScroll.PrefabPath = itemPrefab[0];
            weaponListScroll.PrefabPath = itemPrefab[1];
            await base.Create();
            var bindingSet = this.CreateBindingSet<DragBatchItemView, DragBatchItemViewModel>();
            bindingSet.Bind(canvasGroupObj).For(v => v.alpha).ToExpression(vm => vm.Alpha);
            bindingSet.Bind(gridScroll).For(v => v.Collection).To(vm => vm.GridList);
            bindingSet.Bind(weaponListScroll).For(v => v.Collection).To(vm => vm.WeaponList);
            bindingSet.Bind(girdRect).For(v => v.sizeDelta).ToExpression(vm => CalculationBatchSize(vm.BatchSize));
            bindingSet.Bind(batchRect).For(v => v.sizeDelta).ToExpression(vm => CalculationBatchSize(vm.BatchSize));
            bindingSet.Bind(weaponRect).For(v => v.sizeDelta).ToExpression(vm => CalculationBatchSize(vm.BatchSize));
            bindingSet.Bind(this).For(v => v.batchRect).To(vm => vm.BatchRect).OneWayToSource();
            bindingSet.Bind(this).For(v => v.girdRect).To(vm => vm.GridListRect).OneWayToSource();
            bindingSet.Bind(this).For(v => v.weaponRect).To(vm => vm.WeaponListRect).OneWayToSource();
            bindingSet.Build();
        }
        private static Vector2 CalculationBatchSize(Vector2Int gridSize)
        {
            var newWidth = gridSize.y * GameManager.Instance.CellSize;
            var newHeight = gridSize.x * GameManager.Instance.CellSize;
            return new Vector2(newWidth,newHeight);
        }
    }
}