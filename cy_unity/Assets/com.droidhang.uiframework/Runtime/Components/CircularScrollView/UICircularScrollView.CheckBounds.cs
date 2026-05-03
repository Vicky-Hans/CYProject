using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DH.UIFramework
{
    public partial class UICircularScrollView
    {
        public int StartShowIdx { get; set; }
        public int EndShowIdx { get; set; }
        public Action ShowIdxChangedCallback;
        
        protected virtual bool CheckItemIsOutRange()
        {
            return CheckItemList();
        }

        private bool CheckItemList(int firstPage = 0, int lastPage = 0)
        {
            bool bAllCal = false;

            do
            {
                bAllCal = bAllCalSize(firstPage, lastPage, out var targetAnchorPos);

                if (!bAllCal)
                {
                    ReCalCellPos(0);
                    
                    if (targetAnchorPos != Vector2.zero)
                    {
                        m_ContentRectTrans.anchoredPosition = targetAnchorPos;
                        ClampScrollView();
                        m_ScrollRect.Rebuild(CanvasUpdate.PostLayout);
                        OnContentRectSizeChanged();

                        if (m_IsDrag)
                        {
                            var lastButton = _curEventData.button;
                            
                            _curEventData.button = PointerEventData.InputButton.Left;
                            m_ScrollRect.OnEndDrag(_curEventData);
                            
                            _curEventData.button = lastButton;
                        }
                    }
                }
                else
                {
                    if (_jumpIndex != -2)
                    {
                        Jump2SpecificItem(_jumpIndex);
                        m_ScrollRect.Rebuild(CanvasUpdate.PostLayout);
                        _jumpIndex = -2;
                        bAllCal = false;
                    }
                }
            } while (false);
            
            RecycleOutRangeItems();
            return CheckInRangeItems(firstPage, lastPage) && bAllCal;
        }

        protected bool bAllCalSize(int firstPage, int lastPage, out Vector2 targetAnchorPos)
        {
            var lastStart = StartShowIdx;
            _inRangeIdxList.Clear();
                
            for (int i = firstPage; i <= lastPage; ++i)
            {
                CheckInRangeItemIdxList(i);
            }
            
            var bAllCal = true;
            var changePos = false;

            float deltaSize = 0f;
            for (int i = 0, length = m_CellInfos.Count; i < length; i++)
            {
                int idx = i;
                if (!_inRangeIdxList.Contains(idx)) continue;
                
                CellInfo cellInfo = m_CellInfos[idx];
                if (!cellInfo.InitSize)
                {
                    float size = GetItemAdaptingSize(cellInfo.Go, i);

                    if (size > 0)
                    {
                        if (!changePos)
                        {
                            changePos = i <= lastStart || i == m_CellInfos.Count - 1 && _curOffset == 0;
                        }
                        
                        var deltaChange = size - cellInfo.ItemSize;
                        deltaSize += deltaChange;
                        
                        if(m_Direction == e_Direction.Vertical)
                        {
                            m_ContentHeight += deltaSize;
                        }
                        else
                        {
                            m_ContentWidth += deltaSize;
                        }
                        
                        cellInfo.SetItemSize(size);
                        bAllCal = false;
                    }
                    
                    cellInfo.InitSize = CellItemShowPredicate != null || cellInfo.Go;

                    if (!cellInfo.InitSize)
                    {
                        bAllCal = false;
                    }
                }
            }

            targetAnchorPos = Vector2.zero;
            if (Mathf.Abs(deltaSize) > 0 && (changePos || NeedResetContentRectTransPos(lastPage)))
            {
                //-> 计算 Content 尺寸
                if (m_Direction == e_Direction.Vertical)
                {
                    targetAnchorPos = new Vector2(m_ContentRectTrans.anchoredPosition.x, m_ContentRectTrans.anchoredPosition.y + deltaSize);
                }
                else
                {
                    targetAnchorPos = new Vector2(m_ContentRectTrans.anchoredPosition.x - deltaSize, m_ContentRectTrans.anchoredPosition.y);
                }
            }
            
            return bAllCal;
        }

        protected virtual bool NeedResetContentRectTransPos(int lastPage)
        {
            //-> 计算 Content 尺寸
            if (m_Direction == e_Direction.Vertical)
            {
                if ((m_IsDrag || m_ScrollRect.velocity.SqrMagnitude() > 0.0001f) && _curOffset == -1 || m_autoScrolling && lastPage > 0) //向下拖动时要变化
                {
                    return true;
                }
            }
            else
            {
                if ((m_IsDrag || m_ScrollRect.velocity.SqrMagnitude() > 0.0001f) && _curOffset == -1 || m_autoScrolling && lastPage > 0) //向右拖动时要变化
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual void OnContentRectSizeChanged()
        {
        }

        //判断是否超出显示范围
        protected bool IsOutRange(float pos, float cellSize = -1)
        {
            Vector3 curContentPos = m_ContentRectTrans.anchoredPosition;
            if(m_Direction == e_Direction.Vertical)
            {
                cellSize = cellSize < 0 ? m_CellObjectHeight : cellSize;
                // cellSize += m_SpacingY * 2;
                cellSize = cellSize < 0 ? 0 : cellSize;
                
                if (pos + curContentPos.y > cellSize || pos + curContentPos.y < -m_scrollViewRectTransRect.height)
                {
                    return true;
                }
            }
            else
            {
                cellSize = cellSize < 0 ? m_CellObjectWidth : cellSize;
                // cellSize += m_SpacingX * 2;
                
                if (pos + curContentPos.x < -cellSize || pos + curContentPos.x > m_scrollViewRectTransRect.width)
                {
                    return true;
                }
            }
            
            return false;
        }
        private List<int> _inRangeIdxList = new List<int>();

        protected void CheckInRangeItemIdxList(int pageIdx = 0)
        {
            //先检查超出范围
            for (int i = 0, length = m_CellInfos.Count; i < length; i++)
            {
                int idx = i;
                CellInfo cellInfo = m_CellInfos[idx];
                Vector3 pos = cellInfo.Pos;

                float rangePos = m_Direction == e_Direction.Vertical ? pos.y - m_ContentHeight * pageIdx : pos.x + m_ContentWidth * pageIdx;
                //判断是否超出显示范围
                if (!IsOutRange(rangePos, cellInfo.ItemSize))
                {
                    cellInfo.PageIdx = pageIdx;
                    _inRangeIdxList.Add(idx);
                }
            }
            
            //检查显示的范围
            if (_inRangeIdxList.Count > 0 && _jumpIndex == -2)
            {
                var lastStart = StartShowIdx;
                var lastEnd = EndShowIdx;
                
                StartShowIdx = _inRangeIdxList[0];
                EndShowIdx = _inRangeIdxList[^1];
                
                if (lastStart != StartShowIdx || lastEnd != EndShowIdx)
                {
                    ShowIdxChangedCallback?.Invoke();
                }
            } 
        }

        private void RecycleOutRangeItems()
        {
            //先检查超出范围
            for (int i = 0, length = m_CellInfos.Count; i < length; i++)
            {
                int idx = i;
                if (_inRangeIdxList.Contains(idx)) continue;
                
                CellInfo cellInfo = m_CellInfos[idx];
                RecycleCellInfo(cellInfo);
            }
        }

        private bool CheckInRangeItems(int firstPage, int lastPage)
        {
            bool bAllChecked = true;

            for (int i = 0; i < _inRangeIdxList.Count; ++i)
            {
                int idx = _inRangeIdxList[i];
                
                CellInfo cellInfo = m_CellInfos[idx];
                GameObject obj = cellInfo.Go;

                bool bAlloc = !obj;
                if (bAlloc)
                {
                    //优先从 poolsObj中 取出 （poolsObj为空则返回 实例化的cell）
                    GameObject cell = GetPoolsObj(idx);
                    if (!cell)
                    {
                        if (NeedShowCellItem(idx))
                        {
                            return false;
                        }
                        
                        continue;
                    }
                        
                    cell.gameObject.name = idx.ToString();
                    cellInfo.SetGameObject(cell);
                    Func(m_RefreshItemCallBack, cell);
                }
                
                if (!firstAnimation)
                {
                    var scrollItemAnimation = cellInfo.Go.GetComponent<IScrollItemAnimation>();
                    scrollItemAnimation?.PlaySetupAnimation(idx);
                }
                
                Vector2 pos = cellInfo.Pos;
                Vector2 offset = Vector2.zero;
                
                //刷新位置
                if(m_Direction == e_Direction.Vertical)
                {
                    offset.y = -m_ContentHeight * cellInfo.PageIdx;
                }
                else
                {
                    offset.x = m_ContentWidth * cellInfo.PageIdx;
                }
                    
                cellInfo.SetAnchorPos(pos + offset + GetCellShowOffset());

                if (bAlloc)
                {
                    if (!HasItemsInPoolByInstance(cellInfo.Go))
                    {
                        bAllChecked = i == (_inRangeIdxList.Count - 1);
                        
                        if (idx % m_Row == m_Row - 1)
                        {
                            break; //一帧实例化一行对象
                        }
                    }
                }
            }

            if (bAllChecked)
            {
                firstAnimation = true;
            }
            return bAllChecked;
        }

        protected Vector2 CheckItemIsOutRangeForAutoScroll(PointerEventData eventData = null)
        {
            Vector2 offset = Vector2.zero;
            
            Vector2 curContentPos = m_ContentRectTrans.anchoredPosition;
            int changeSizeDir = 0;
            
            if(m_Direction == e_Direction.Vertical && curContentPos.y < -1f || m_Direction == e_Direction.Horizontal && curContentPos.x > 1f)
            {
                //需要把最后一个移到前面
                changeSizeDir = -1;
            }
            else if(m_Direction == e_Direction.Vertical && curContentPos.y + m_scrollViewRectTransRect.height > m_ContentRectTrans.sizeDelta.y + 1f
                || m_Direction == e_Direction.Horizontal && -curContentPos.x + m_scrollViewRectTransRect.width > m_ContentRectTrans.sizeDelta.x + 1f)
            {
                //需要把第一个移到后面
                changeSizeDir = 1;
            }

            if (changeSizeDir != 0)
            {
                bool needAddSize = true;
                if(m_Direction == e_Direction.Vertical)
                {
                    needAddSize = Mathf.RoundToInt(m_ContentRectTrans.sizeDelta.y / m_ContentHeight) < 2;
                }
                else
                {
                    needAddSize = Mathf.RoundToInt(m_ContentRectTrans.sizeDelta.x / m_ContentWidth) < 2;
                }
                
                var sizeDelta = m_ContentRectTrans.sizeDelta;
                
                if(m_Direction == e_Direction.Vertical)
                {
                    if (needAddSize)
                    {
                        sizeDelta = new Vector2(sizeDelta.x, sizeDelta.y + m_ContentHeight);
                        m_ContentRectTrans.sizeDelta = sizeDelta;
                    }

                    if (changeSizeDir == -1 || !needAddSize)
                    {
                        offset.y = m_ContentHeight * changeSizeDir;
                        m_ContentRectTrans.anchoredPosition = new Vector2(curContentPos.x, curContentPos.y - m_ContentHeight * changeSizeDir);
                    }
                }
                else
                {
                    if (needAddSize)
                    {
                        sizeDelta = new Vector2(m_ContentRectTrans.sizeDelta.x + m_ContentWidth, sizeDelta.y);
                        m_ContentRectTrans.sizeDelta = sizeDelta;
                    }
                    
                    if (changeSizeDir == -1 || !needAddSize)
                    {
                        offset.x = m_ContentWidth * -changeSizeDir;
                        m_ContentRectTrans.anchoredPosition = new Vector2(curContentPos.x + m_ContentWidth * changeSizeDir, curContentPos.y);
                    }
                }
                
                if (changeSizeDir == -1 || !needAddSize)
                {
                    Vector3 offsetV3 = offset;
                    for (int i = 0, length = m_CellInfos.Count; i < length; i++)
                    {
                        m_CellInfos[i].ApplyObjOffsetPos(offsetV3);
                    }
                }
                
                m_ScrollRect.Rebuild(CanvasUpdate.PostLayout);
                
                if (eventData != null)
                {
                    m_ScrollRect.OnEndDrag(eventData);
                    m_ScrollRect.OnBeginDrag(eventData);
                }
            }
            
            if (m_Dirty)
            {
                curContentPos = m_ContentRectTrans.anchoredPosition;
                int firstPage, lastPage = 0;
                
                if(m_Direction == e_Direction.Vertical)
                {
                    firstPage = Mathf.FloorToInt(curContentPos.y / m_ContentHeight);
                    lastPage = Mathf.FloorToInt((curContentPos.y + m_scrollViewRectTransRect.height) / m_ContentHeight);
                }
                else
                {
                    firstPage = Mathf.FloorToInt(-curContentPos.x / m_ContentWidth);
                    lastPage = Mathf.FloorToInt((-curContentPos.x + m_scrollViewRectTransRect.width) / m_ContentWidth);
                }

                m_Dirty = !CheckItemList(firstPage, lastPage);
            }

            return offset;
        }

        protected virtual void OnCheckItemOutRangeEnd(Vector2 offset)
        {
        }
    }
}
