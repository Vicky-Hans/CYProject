//*****************************-》 可收展 循环列表 《-****************************
//初始化:
//       Init()
//刷新列表:
//      ShowList(List<int>)
//回调:
//Func(GameObject = 收展按钮, GameObject = Cell, int = 收展按钮索引 Index, int = 子cell索引) 
//OnClickCell(GameObject = Cell, int = Index)    //点击Cell 

using System;
using System.Collections.Generic;
using DHFramework;
using UnityEngine;
using UnityEngine.UI;

namespace DH.UIFramework
{
    public partial class GroupExpandCircularScrollView : UICircularScrollView
    {
        public bool m_IsExpand = true;
        /// <summary>
        /// 分组按钮仅作为分割线使用，第一个分割线不展示
        /// </summary>
        public bool expandAsSplitLine;

        private float m_ExpandButtonWidth;
        private float m_ExpandButtonHeight;

        //展开信息
        class ExpandInfo : IReference
        {
            public Vector3 buttonPos;
            public GameObject button;
            public bool isExpand; 
            public List<CellInfo> cellInfos;
            public List<int> inRangeIdxList = new List<int>();
            public float size; // 这个组的高度,只算的是cellInfos的size
            public int cellCount;
            
            public void RefreshButtonPos(Vector3 newPos)
            {
                buttonPos = newPos;
                
                if (button != null)
                {
                    button.transform.localPosition = newPos;
                }
            }

            public void Clear()
            {
                size = 0;
                cellCount = 0;
                button = null;
                inRangeIdxList.Clear();

                if (cellInfos != null)
                {
                    foreach (var cellInfo in cellInfos)
                    {
                        ReferencePool.Release(cellInfo);
                    }
                    
                    cellInfos.Clear();
                }
            }
        }
        
        private List<ExpandInfo> m_ExpandInfos = null;

        private Dictionary<GameObject, bool> m_IsAddedListener = new Dictionary<GameObject, bool>(); //用于 判断是否重复添加 点击事件

        public Func<int, int, string> m_FuncGetCellPrefabTemplate;

        internal override void Init()
        {
            base.Init();
            
            m_CanAutoScroll = false;
        }

        private void InitExpandButtonPrefab()
        {
            if (expandButtonPrefab != null)
            {
                RectTransform rectTrans = expandButtonPrefab.transform as RectTransform;

                rectTrans.pivot = new Vector2(0, 1);
                rectTrans.anchorMin = new Vector2(0, 1);
                rectTrans.anchorMax = new Vector2(0, 1);
                
                var viewRectTransRect = m_scrollViewRectTrans.rect;
                var height = viewRectTransRect.height;
                var width = viewRectTransRect.width;
                
                rectTrans.sizeDelta = m_Direction == e_Direction.Vertical
                    ? new Vector2(width, rectTrans.sizeDelta.y)
                    : new Vector2(rectTrans.sizeDelta.x, height);
                
                var rect = rectTrans.rect;
                m_ExpandButtonWidth = rect.width;
                m_ExpandButtonHeight = rect.height;
            }
        }

        protected void ShowList(List<int> numArray)
        {
            m_lastNormalizedPos = -1f;
            ClearCell(); //清除所有Cell
            
            int buttonCount = numArray.Count;

            bool isReset;
            if(m_IsListInited && m_ExpandInfos.Count == buttonCount)
            {
                isReset = false;
            }
            else
            {
                if (m_ExpandInfos == null)
                {
                    m_ExpandInfos = new List<ExpandInfo>();
                }
                else
                {
                    //过多的放回缓存池
                    if (m_ExpandInfos.Count > buttonCount)
                    {
                        for (int i = m_ExpandInfos.Count - 1; i >= buttonCount; --i)
                        {
                            var info = m_ExpandInfos[i];
                            
                            ReferencePool.Release(info);
                            m_ExpandInfos.RemoveAt(i);
                        }
                    }
                }
                
                isReset = true;
            }

            for (int k = 0; k < buttonCount; k++)
            {
                if (k >= m_ExpandInfos.Count)
                {
                    m_ExpandInfos.Add(ReferencePool.Acquire<ExpandInfo>());
                }
                
                //-> Button 物体处理
                GameObject button = GetPoolsButtonObj();
                button.name = k.ToString();
                Button buttonComponent = button.GetComponent<Button>();
                if (!m_IsAddedListener.ContainsKey(button) && buttonComponent != null)
                {
                    m_IsAddedListener[button] = true;
                    buttonComponent.onClick.AddListener(delegate () { OnClickExpand(button); });
                }
                button.transform.SetSiblingIndex(k);

                int count = numArray[k];

                //-> 存储数据
                ExpandInfo expandInfo = m_ExpandInfos[k];
                expandInfo.button = button;
                expandInfo.cellCount = count;
                ReSizeExpandInfoCellInfoList(expandInfo);
                expandInfo.isExpand = isReset ? m_IsExpand : expandInfo.isExpand;
                
                float cellSize = m_Direction == e_Direction.Vertical ? m_CellObjectHeight : m_CellObjectWidth;
                expandInfo.size = count == 0 && expandAsSplitLine ? 0 : (cellSize + m_SpacingY) * Mathf.CeilToInt((float)count / m_Row);
                OnRefreshGroupCellItem(button, null);
                
                expandInfo.isExpand = !expandInfo.isExpand;
                OnButtonExpand(k, true);
            }

            UpdateContentSize();
            m_IsListInited = true;
        }
        
        public bool IsGroupExpand(int groupNum)
        {
            if (groupNum < 0 || groupNum >= m_ExpandInfos.Count)
            {
                return false;
            }
            
            ExpandInfo expandInfo = m_ExpandInfos[groupNum];

            return expandInfo.isExpand;
        }

        public void Jump2SpecificItem(int groupNum, int index)
        {
            if (groupNum < 0 || groupNum >= m_ExpandInfos.Count)
            {
                Jump2Start();
                return;
            }
            
            if (m_ScrollRect == null)
            {
                return;
            }
            
            m_ScrollRect.velocity = Vector2.zero;
            
            ExpandInfo expandInfo = m_ExpandInfos[groupNum];
            Vector3 targetPos = Vector3.zero;
            
            if(!expandInfo.isExpand)
            {
                targetPos = expandInfo.buttonPos;
            }
            else
            {
                if (index < 0 || index >= expandInfo.cellCount)
                {
                    index = 0;
                }
                
                CellInfo cellInfo = expandInfo.cellInfos[index];
                targetPos = cellInfo.Pos;
            }
            
            if(m_Direction == e_Direction.Vertical)
            {
                m_ContentRectTrans.anchoredPosition = new Vector2(m_ContentRectTrans.anchoredPosition.x, -targetPos.y);
            }
            else
            {
                m_ContentRectTrans.anchoredPosition = new Vector2(-targetPos.x, m_ContentRectTrans.anchoredPosition.y);
            }
            
            ClampScrollView();
        }

        public void ExpandGroup(int groupIdx)
        {
            ExpandOrShrinkGroup(groupIdx, false);
        }

        public void ShrinkGroup(int groupIdx)
        {
            if (groupIdx < 0 || groupIdx >= m_ExpandInfos.Count)
            {
                return;
            }

            var info = m_ExpandInfos[groupIdx];

            if (!info.isExpand) //已经收回了，就不处理了
            {
                return;
            }
            
            ExpandOrShrinkGroup(groupIdx, true);
            
            ClampScrollView();
        }

        public override void Clear()
        {
            ClearCell();
        }

        protected override void LateUpdate()
        {
            if (m_ContentRectTrans == null)
            {
                return;
            }
            
            if (m_Dirty)
            {
                m_Dirty = !CheckItemIsOutRange();
            }
        }

        protected override void InitAllCellPos()
        {
            if (!m_IsListInited)
            {
                return;
            }
            
            var buttonCount = m_ExpandInfos.Count;
            
            for (int i = 0; i < buttonCount; ++i)
            {
                //-> 存储数据
                ExpandInfo expandInfo = m_ExpandInfos[i];
                
                float cellSize = m_Direction == e_Direction.Vertical ? m_CellObjectHeight : m_CellObjectWidth;
                expandInfo.size = (cellSize + m_SpacingY) * Mathf.CeilToInt((float)expandInfo.cellCount / m_Row);
                expandInfo.isExpand = !expandInfo.isExpand;
                
                OnButtonExpand(i, true);
            }
            
            UpdateContentSize();
        }

        protected override bool CheckItemIsOutRange()
        {
            for(int k = 0, length = m_ExpandInfos.Count; k < length; k++)
            {
                //收集显示范围内的index
                int kIdx = k;
     
                ExpandInfo expandInfo = m_ExpandInfos[kIdx];
                if(!expandInfo.isExpand)
                {
                    continue;
                }

                expandInfo.inRangeIdxList.Clear();
                int count = expandInfo.cellCount;
                
                for(int i = 0; i < count; i++)
                {
                    CellInfo cellInfo = expandInfo.cellInfos[i];
                    float rangePos = m_Direction == e_Direction.Vertical ? cellInfo.Pos.y : cellInfo.Pos.x;
                    if(!IsOutRange(rangePos))
                    {
                        expandInfo.inRangeIdxList.Add(i);
                    }
                    else
                    {
                        RecycleCellInfo(cellInfo);
                    }
                }
            }
            
            for(int k = 0, length = m_ExpandInfos.Count; k < length; k++)
            {
                //收集显示范围内的index
                int kIdx = k;
     
                ExpandInfo expandInfo = m_ExpandInfos[kIdx];
                if(!expandInfo.isExpand)
                {
                    continue;
                }
                
                var cellList = expandInfo.cellInfos;
                bool bAllChecked = true;

                for (int i = 0; i < expandInfo.inRangeIdxList.Count; ++i)
                {
                    int idx = expandInfo.inRangeIdxList[i];
                
                    CellInfo cellInfo = cellList[idx];
                    GameObject obj = cellInfo.Go;
                    Vector3 pos = cellInfo.Pos;

                    if (obj == null)
                    {
                        //优先从 poolsObj中 取出 （poolsObj为空则返回 实例化的cell）
                        GameObject cell = GetCellObj(kIdx, idx);

                        if (cell == null)
                        {
                            return false;
                        }
                        
                        cellInfo.SetGameObject(cell);
                        cellInfo.SetAnchorPos(pos);
                        cell.name = kIdx + "-" + idx;
                        
                        OnRefreshGroupCellItem(expandInfo.button, cell);

                        if (!HasItemsInPoolByInstance(cell))
                        {
                            bAllChecked = i == (expandInfo.inRangeIdxList.Count - 1);
                            if (idx == m_Row - 1)
                            {
                                break; //一帧实例化一行对象
                            }
                        }
                    }

                    //刷新位置
                    cellInfo.SetAnchorPos(pos);
                }

                if (!bAllChecked)
                {
                    return false;
                }
            }

            return true;
        }

        //清除所有Cell (扔到对象池)
        private void ClearCell()
        {
            if (!m_IsListInited || m_ExpandInfos == null) return;

            for (int i = 0, length = m_ExpandInfos.Count; i < length; i++)
            {
                ClearOneExpandInfo(m_ExpandInfos[i]);
            }
        }

        private void UpdateContentSize()
        {
            float contentSize = 0f;
            var previous = m_ExpandInfos.Count > 0 ? m_ExpandInfos[^1] : null;
            var totalOffset = GetGroupOffset(m_ExpandInfos.Count,expandAsSplitLine,previous);
            
            if(m_Direction == e_Direction.Vertical)
            {
                contentSize = -totalOffset.y + m_PaddingBottom + m_SpacingY;
            }
            else
            {
                contentSize = totalOffset.x + m_PaddingRight + m_SpacingX;
            }
            
            bool bContentInView = false;
            if(m_Direction == e_Direction.Vertical)
            {
                bContentInView = contentSize < m_scrollViewRectTransRect.height;
                contentSize = bContentInView ? m_scrollViewRectTransRect.height : contentSize;
                m_ContentRectTrans.sizeDelta = new Vector2(m_ContentRectTrans.sizeDelta.x, contentSize);
            }
            else
            {
                bContentInView = contentSize < m_scrollViewRectTransRect.width;
                contentSize = bContentInView ? m_scrollViewRectTransRect.width : contentSize;
                m_ContentRectTrans.sizeDelta = new Vector2(contentSize, m_ContentRectTrans.sizeDelta.y);
            }

            if (!m_CanDragInView)
            {
                m_ScrollRect.enabled = !bContentInView;
            }
            
            m_Dirty = true;
        }

        //清除所有Cell (扔到对象池)
        private void ClearOneExpandInfo(ExpandInfo info)
        {
            if (info == null)
            {
                return;
            }
            
            if (info.button != null)
            {
                SetPoolsButtonObj(info.button);
                info.button = null;
            }

            if (info.cellInfos != null)
            {
                for (int j = 0, count = info.cellInfos.Count; j < count; j++)
                {
                    RecycleCellInfo(info.cellInfos[j]);
                }
            }

            info.Clear();
        }

        //收展按钮 点击事件
        private void OnClickExpand(GameObject button)
        {
            int index = int.Parse(button.name);
            ExpandOrShrinkGroup(index);
        }
        
        private void ExpandOrShrinkGroup(int index, bool canShrink = true)
        {
            if (index < 0 || index >= m_ExpandInfos.Count)
            {
                return;
            }

            var info = m_ExpandInfos[index];

            if (!canShrink && info.isExpand) //不允许收回但已经展开了，就不处理了
            {
                return;
            }
            
            OnButtonExpand(index);
            UpdateContentSize();
            
            if (index > 0 && index == m_ExpandInfos.Count - 1 && info.isExpand && info.cellCount > 0) //点击的是最后一个并且要展开
            {
                var cellInfo = info.cellInfos[0];
                float rangePos = m_Direction == e_Direction.Vertical ? cellInfo.Pos.y : cellInfo.Pos.x;
                float spacing = m_Direction == e_Direction.Vertical ? m_SpacingY : m_SpacingX;

                if (IsOutRange(rangePos))
                {
                    MoveContent(cellInfo.ItemSize + spacing);
                }
            }
        }
        
        public void OnButtonExpand(int index, bool bForInit = false)
        {
            m_Dirty = true;
            bool expand = !m_ExpandInfos[index].isExpand;
            m_ExpandInfos[index].isExpand = expand;
            var previous = index > 0 ? m_ExpandInfos[index - 1] : null;

            //-> 重新计算坐标 并 显示处理
            for (int k = 0, length = m_ExpandInfos.Count; k < length; k++)
            {
                ExpandInfo expandInfo = m_ExpandInfos[k];
                int count = expandInfo.cellCount;

                if (k >= index)
                {
                    //-> 计算 按钮位置
                    GameObject button = expandInfo.button;
                    if (button == null)
                    {
                        continue;
                    }

                    bool zeroSize = index == 0 || previous?.cellCount == 0 || expandInfo.cellCount == 0;
                    var buttonPos = GetGroupOffset(k,expandAsSplitLine,previous);
                    expandInfo.RefreshButtonPos(buttonPos);
                    if (expandAsSplitLine && zeroSize)
                    {
                        button.SetActive(false);
                    }
                    else
                    {
                        button.SetActive(true);
                    }
                    
                    //-> 按钮 收 状态时
                    if (!expandInfo.isExpand)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            RecycleCellInfo(expandInfo.cellInfos[i]);
                        }
                    }
                    else
                    {
                        Vector2 totalOffset = buttonPos;
                        
                        if (m_Direction == e_Direction.Vertical)
                        {
                            totalOffset.y -= zeroSize ? 0 : m_ExpandButtonHeight;
                        }
                        else
                        {
                            totalOffset.x += zeroSize ? 0 :m_ExpandButtonWidth;
                        }
                        
                        for (int i = 0; i < count; i++)
                        {
                            CellInfo cellInfo = expandInfo.cellInfos[i];

                            if (cellInfo == null)
                            {
                                continue;
                            }
                            cellInfo.SetItemSize(m_Direction == e_Direction.Vertical
                                ? m_CellObjectHeight
                                : m_CellObjectWidth);
                            
                            var newPos = GetCellOriPos(i, expandInfo.cellInfos);
                            cellInfo.SetOriPos(newPos + totalOffset);
                        }
                    }

                    if (bForInit)
                    {
                        break;
                    }
                }
            }
        }

        protected override void ScrollRectListener(Vector2 value)
        {
            if (m_ExpandInfos == null) return;

            float normalizedPos = m_Direction == e_Direction.Vertical ? value.y : value.x;

            if (Mathf.Abs(m_lastNormalizedPos - normalizedPos) < 0.0001f)
            {
                return;
            }

            if (normalizedPos > 0 && normalizedPos < 1f) //只记录有效的位置
            {
                m_lastNormalizedPos = normalizedPos;
            }

            m_Dirty = true;
        }

        //取出 cell
        protected GameObject GetCellObj(int groupNum, int idx)
        {
            if (m_FuncGetCellPrefabTemplate != null)
            {
                var prefabTemplate = m_FuncGetCellPrefabTemplate(groupNum, idx);

                if (!string.IsNullOrEmpty(prefabTemplate))
                {
                    return DynamicGetObj(prefabTemplate);
                }
            }

            if (m_CellGameObject)
            {
                return GetPoolsObj(m_CellGameObject, m_Content.transform);
            }

            return null;
        }

        //取出 button
        private GameObject GetPoolsButtonObj()
        {
            return GetPoolsObj(expandButtonPrefab, m_Content.transform);
        }
        
        //存入 button
        private void SetPoolsButtonObj(GameObject button)
        {
            base.SetPoolsObj(button);
        }

        string[] defaultObjName = { "1", "-2" };
        
        private void OnRefreshGroupCellItem(GameObject button, GameObject selectObject)
        {
            var objName = defaultObjName;
            if (selectObject != null)
            {
                objName = selectObject.name.Split('-');
            }
            
            var buttonNum = int.Parse(button.name);
            var num = int.Parse(objName[1]);

            OnBindGroupBtn(button, buttonNum);
            OnBindGroupCell(selectObject, buttonNum, num);
        }
        
        private void ReSizeExpandInfoCellInfoList(ExpandInfo info)
        {
            if (info.cellInfos == null)
            {
                info.cellInfos = new List<CellInfo>();
            }
                
            if (info.cellInfos.Count > info.cellCount)
            {
                for (int i = info.cellInfos.Count - 1; i >= info.cellCount; --i)
                {
                    CellInfo tmpCell = info.cellInfos[i];
                    
                    RecycleCellInfo(tmpCell);
                    ReferencePool.Release(tmpCell);
                    info.cellInfos.RemoveAt(i);
                }
            }

            for (int i = 0; i < info.cellCount; ++i)
            {
                if (i >= info.cellInfos.Count)
                {
                    info.cellInfos.Add(ReferencePool.Acquire<CellInfo>());
                }
            }
        }
    }
}

