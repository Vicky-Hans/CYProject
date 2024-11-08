using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 解决2个层级的ScrollView嵌套但是方向不同的情况
/// </summary>
namespace Game.UI.Control
{
    public class ScrollViewLayers : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        /// <summary>
        /// 上层的ScrollRect
        /// </summary>
        public ScrollRect parentScrollRect = null;

        private void Start()
        {
            if (parentScrollRect == null)
            {
                parentScrollRect = transform.parent.GetComponentInParent<ScrollRect>();
            }
        }

        //事件传递
        public void OnBeginDrag(PointerEventData eventData)
        {
            // ReSharper disable once Unity.NoNullPropagation
            parentScrollRect?.OnBeginDrag(eventData);
        }

        //事件传递
        public void OnDrag(PointerEventData eventData)
        {
            // ReSharper disable once Unity.NoNullPropagation
            parentScrollRect?.OnDrag(eventData);
        }

        //事件传递
        public void OnEndDrag(PointerEventData eventData)
        {
            // ReSharper disable once Unity.NoNullPropagation
            parentScrollRect?.OnEndDrag(eventData);
        }
    }
}
