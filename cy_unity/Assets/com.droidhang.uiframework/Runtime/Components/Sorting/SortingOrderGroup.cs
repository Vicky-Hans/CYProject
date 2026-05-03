using System;
using System.Collections.Generic;
using UnityEngine;

namespace DH.UIFramework
{
    public class SortingOrderGroup : MonoBehaviour
    {
        private List<ISortingOrderModifier> sortingOrderModifiers = new List<ISortingOrderModifier>();
        internal int currentOrder;

        public void ApplySortingOrderModifier(int order)
        {
            currentOrder = order;
            sortingOrderModifiers.Clear();
            GetComponentsInChildren<ISortingOrderModifier>(true,sortingOrderModifiers);
            
            foreach (var modifier in sortingOrderModifiers)
            {
                modifier.ApplyOrder(order);
            }
        }

        [Obsolete("Use ApplySortingOrderModifier (typo fix).")]
        public void ApplySoringModifier(int order)
        {
            ApplySortingOrderModifier(order);
        }
    }
}