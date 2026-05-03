using DHFramework;
using UnityEngine;

namespace DH.UIFramework
{
    [RequireComponent(typeof(Canvas))]
    public class CanvasSortingOrderModifier : MonoBehaviour,ISortingOrderModifier
    {
        public int relativeOrder;
        private Canvas cachedCanvas;
        public bool dynamicItem; // 动态生成的对象
        
        
        private void Start()
        {
            if (!dynamicItem)
            {
                return;
            }

            var group = GetComponentInParent<SortingOrderGroup>();
            if (!group)
            {
                return;
            }
            
            ApplyOrder(group.currentOrder);
        }
        
        public void ApplyOrder(int order)
        {
            if (!cachedCanvas)
            {
                cachedCanvas = GetComponent<Canvas>();
                if (!cachedCanvas)
                {
                    throw new GameFrameworkException($"{nameof(CanvasSortingOrderModifier)} require Canvas component");
                }
            }


            cachedCanvas.sortingOrder = order + relativeOrder;
        }
    }
}