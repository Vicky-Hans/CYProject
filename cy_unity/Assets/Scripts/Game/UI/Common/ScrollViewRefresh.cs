using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


/// <summary>
///  scrollview 滑动到底部的一个回调
/// </summary>
public class ScrollViewRefresh : MonoBehaviour, IEndDragHandler
{
    public ScrollRect scrollRect;
    public RectTransform content;
    public float refreshThreshold = 200f;

    private bool isRefreshing;

    public Action<Action> RefreshCallback;
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isRefreshing && scrollRect.normalizedPosition.y <= 0f)
        {
            float distanceToBottom = content.sizeDelta.y - scrollRect.viewport.rect.height - scrollRect.content.anchoredPosition.y;
            if (distanceToBottom < refreshThreshold)
            {
                
                isRefreshing = true;
                // 执行刷新操作
                RefreshCallback?.Invoke(RefreshContent);    
            }
        }
    }

    private async void RefreshContent()
    {
        // 在此处执行刷新操作
        // 可以根据具体需求更新内容、重新加载数据等
        // 刷新完成后，重置标志和位置
        isRefreshing = false;
        // 设置滚动底部
        scrollRect.normalizedPosition = new Vector2(0f, 0f);
    }
}