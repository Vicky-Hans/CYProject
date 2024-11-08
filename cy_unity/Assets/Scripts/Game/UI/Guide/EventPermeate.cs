using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DHFramework;
using Game.UI.Guide;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

/// <summary>
///     控制穿透
/// </summary>
public class EventPermeate : MonoBehaviour,IPointerClickHandler,IBeginDragHandler,IDragHandler,IEndDragHandler
{
    public enum EHandEventType
    {
        BeginDrag,
        Drag,
        EndDrag,
        Click
    }

    public bool isTouchStart;
    private Vector2 startPos;
    private bool isPass;
    private Vector2 endPassPos;
    /// <summary>
    /// 是否穿透事件
    /// </summary>
    public bool IsPass { get=>isPass;
        set
        {
            isPass = value;
            // DHLog.Debug("muzili log enter set :" + value);
        }
    }

    public Action DragEndCheckIsShowAction;

    /// <summary>
    /// 触发事件
    /// </summary>
    public Action TriggerEvent;
    public int GuideId { get; set; }
    /// <summary>
    /// 是否拦截事件 强引导/弱引导
    /// </summary>
    public bool IsEventInterceptor { get; set; }

    private readonly List<int> exportGuideIds = new (){ 106,112 };
    private readonly List<int> areaGuideIds = new (){ 108, 109 };
    // private readonly List<int>  = new (){ 107 };
    // 事件穿透对象
    [HideInInspector] public GameObject target;
    public void OnBeginDrag(PointerEventData eventData)
    {
     
        isTouchStart = true;
        startPos = eventData.position;
        IsPass = true;
        // Debug.Log("muzili log drag Begin Drag");
        PassEvent(eventData, ExecuteEvents.beginDragHandler, EHandEventType.BeginDrag);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        // Debug.Log("muzili log drag Drag");
        if(!isTouchStart) return;
        PassEvent(eventData, ExecuteEvents.dragHandler, EHandEventType.Drag);
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if(!isTouchStart) return;
        // Debug.Log("muzili log drag EndDrag");
        PassEvent(eventData, ExecuteEvents.endDragHandler, EHandEventType.EndDrag);
        isTouchStart = false;
        
        DragEndCheckIsShowAction?.Invoke();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        var endPos = eventData.position;
        if(isTouchStart && Vector3.Distance(startPos, endPos) > 20f)
        { 
            return;
        }
        
        IsPass = true;
        // Debug.Log("muzili log drag click");
        PassClickEvent(eventData, ExecuteEvents.pointerClickHandler, EHandEventType.Click);
    }
    

    public void PassClickEvent<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function, EHandEventType type) where T : IEventSystemHandler
    {
        if (exportGuideIds.Contains(GuideId))
        {
            IsEventInterceptor = false;
        }

        if (IsEventInterceptor)
        {
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, results);
            
            // 如果是区域引导
            if (areaGuideIds.Contains(GuideId))
            {
                var allTalent = target.transform.parent;
                for (var j = 0; j < allTalent.childCount; j++)
                {
                    bool isBreak = false;
                    for (var i = 0; i < results.Count; i++)
                    {
                        var tempNode = allTalent.GetChild(j).GetChild(0);
                        if (tempNode.gameObject == results[i].gameObject)
                        {
                            // 如果是目标物体，则把事件透传下去，然后break
                            ExecuteEvents.Execute(tempNode.gameObject, data, function);
                            // if (GuideId != 112)
                            // {
                                TriggerEvent?.Invoke();
                            // }
                            isBreak = true;
                            break;
                        }
                    } 
                    if(isBreak)
                    {
                        break;
                    }
                }
            } 
            // 如果指定引导
            //获取射线所碰撞的所有物体

            for (var i = 0; i < results.Count; i++)
            {
                if (target == results[i].gameObject)
                {
                    // 如果是目标物体，则把事件透传下去，然后break
                    ExecuteEvents.Execute(target, data, function);
                    // if (GuideId != 112)
                    // {
                        TriggerEvent?.Invoke();
                    // }
                    break;
                }
            } 
        }
        else
        {
            TriggerEvent?.Invoke();
            // if (GuideId == 116)
            // {
            //     var results = new List<RaycastResult>();
            //     EventSystem.current.RaycastAll(data, results);
            //     for (var i = 0; i < results.Count; i++)
            //     {
            //         if (target == results[i].gameObject)
            //         {
            //             // 如果是目标物体，则把事件透传下去，然后break
            //             ExecuteEvents.Execute(target, data, function);
            //             break;
            //         }
            //     } 
            // }
        }
    }

    // 把事件透下去
    public void PassEvent<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function, EHandEventType type) where T : IEventSystemHandler
    {
     
        if (IsEventInterceptor)
        {
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, results); //获取射线所碰撞的所有物体
            // 如果点击区域在目标区域内，则把事件透传下去，然后break
            // 确定区域
            var areaRect = target.GetComponent<RectTransform>();
            var tempRect = areaRect.rect;
            // 确定点击位置是否在区域内
            RectTransformUtility.ScreenPointToLocalPointInRectangle(areaRect, data.position, AppGlobal.Instance.UICamera, out var localPoint);
            // 计算区域
            Rect rect = new Rect(-tempRect.width / 2f, -tempRect.height / 2f, tempRect.width, tempRect.height);
            
            if (rect.Contains(localPoint))
            {
                // DHLog.Debug($"muzili log enter {IsPass}");
                if (!IsPass)
                {
                    data.position = startPos;
                    // 如果是目标物体，则把事件透传下去，然后break
                    ExecuteEvents.Execute(target, data, function);
                }
                else
                {
                    // 如果是目标物体，则把事件透传下去，然后break
                    ExecuteEvents.Execute(target, data, function); 
                }
            }
            else
            {
                if(type == EHandEventType.EndDrag)
                {
                    data.position =endPassPos;
                    // 结束透传
                    ExecuteEvents.Execute(target, data, function); 
                }
            }

        }
        else
        {
            // if (GuideId != 112)
            // {
                TriggerEvent?.Invoke();
            // }
        }
    }
}