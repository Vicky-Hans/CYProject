using DH.Data;
using TMPro;
using UnityEngine;

public class AutoScrollText : MonoBehaviour
{
    public TextMeshProUGUI text;
    public RectTransform rectTs;
    public float scrollSpeed = 50f;
    private bool isRolling;
    public float delayTime = 5.0f;

    private long nextTime;
    private Vector2 startAnchored;
    void Start()
    {
        text.enableAutoSizing = false;
        startAnchored = rectTs.anchoredPosition;
    }

    void Update()
    {
        
        if(nextTime > Lodash.GetUnixTime()) return; 
        if (CheckIsAutoScrolling())
        {
            // 每帧向左滚动
            var pos = rectTs.anchoredPosition;
            var offset = Vector2.left * scrollSpeed;
            rectTs.anchoredPosition = pos + offset * Time.deltaTime;

            // 如果滚动到文本末尾，将其位置重置到起点
            if (rectTs.anchoredPosition.x <= -rectTs.rect.width)
            {
                rectTs.anchoredPosition = startAnchored;
                updateNextTime();
            }
            text.alignment = TextAlignmentOptions.Left;
        }
        else
        {
            rectTs.anchoredPosition = startAnchored;
            text.alignment = TextAlignmentOptions.Center;
        }
    }

    /// <summary>
    /// ·重置锚点位置和时间
    /// </summary>
    public void ResetAnchoredPositionAndTime()
    {
        
        rectTs.anchoredPosition = startAnchored;
        updateNextTime();
    }

    /// <summary>
    /// 检查是否需要滚动
    /// </summary>
    /// <returns></returns>
    private bool CheckIsAutoScrolling()
    {
        var textWidth = rectTs.rect.width;
        var contentWidth = text.preferredWidth;
        return contentWidth > textWidth;
    }

    private void updateNextTime()
    {
        nextTime = Lodash.GetUnixTime() + (long)delayTime;
    }
}