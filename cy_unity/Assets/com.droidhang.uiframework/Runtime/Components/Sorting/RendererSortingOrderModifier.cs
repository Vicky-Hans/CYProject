using System;
using System.Collections.Generic;
using UnityEngine;

namespace DH.UIFramework
{
    public class RendererSortingOrderModifier : MonoBehaviour,ISortingOrderModifier
    {
        public enum ApplyOrderType
        {
            Relative,
            Absolute
        }

        public int order;
        public ApplyOrderType orderType;
        public bool bCacheRenders = true; //默认缓存Renders，一般用在特效的根结点上
        public bool dynamicItem; // 动态生成的对象
        
        private List<Renderer> renderers = new List<Renderer>();

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

        public void ApplyOrder(int sortingOrder)
        {
            if (!bCacheRenders || renderers.Count == 0)
            {
                renderers.Clear();
                GetComponentsInChildren<Renderer>(true, renderers);
            }
            
            foreach (var item in renderers)
            {
                item.sortingOrder = orderType == ApplyOrderType.Relative ? sortingOrder + order : order;
            }
        }
    }
}