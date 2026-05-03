//*****************************-》 滑动翻页 循环列表 《-****************************
//刷新列表:
//      ShowList(int = 数量)
//回调:
// 1: Func(cell, index)  //列表刷新回调

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DH.UIFramework
{
    public enum NavAlignment
    {
        Left = 0,
        Middle = 1,
        Right = 2
    }
    
    public class FlipPageCircularScrollView : UICircularScrollView
    {
        [Serializable]
        public class OnChangeEvent : UnityEvent<int> { }
        
        /// <summary>
        /// Event delegates triggered when the input field changes its data.
        /// </summary>
        [SerializeField]
        private OnChangeEvent m_OnValueChanged = new OnChangeEvent();
        
        public OnChangeEvent onValueChanged { 
            get => m_OnValueChanged;
            set => m_OnValueChanged = value;
        }
        
        public bool m_Teleport
        {
            get => teleport;
            set => teleport = value;
        }
        
        public int m_OnePageCount = 1;
        public float m_SlideSpeed = 5;
        public bool m_IsMaxRange = false;
        public bool m_IsOpenNavIcon = false;
        public GameObject m_NavNormalPrefab;
        public GameObject m_NavSelectedPrefab;
        public float m_NavSpacing = 5.0f;
        public Transform m_ObjNavigationParent;
        public NavAlignment m_CurNavAlignment = NavAlignment.Middle;

        public int CurPageIndex
        {
            get => m_NowIndex;
            set
            {
                if (m_NowIndex == value)
                {
                    return;
                }

                m_NowIndex = value;
                SetToPageIndex(value, m_Teleport);
            }
        }

        private float m_NavNormalSizeX = 20;
        private float m_NavSelectSizeX = 40;
        private int m_AllPageCount;          //总页数
        private int m_NowIndex = 0;          //当前位置索引
        private int m_LastIndex = -1;        //上一次的索引
        private bool m_Lerping = false;
        private float m_LastAutoFlipTime = 0f;
        private bool teleport = false;
        
        private List<Transform> m_NavNormalTransList = new List<Transform>();
        private Transform m_NavSelectedTrans;
        private int defaultIndex = -1;

        public new Action<int> m_FuncPageMoveBeginCallBack; //传回的index从0开始
        public new Action<int> m_FuncPageMoveEndCallBack; //传回的index从0开始
        
        internal override void Init()
        {
            base.Init();
            m_ScrollRect.enabled = m_CanDragScrollView;
            
            if (Application.isPlaying)
            {
                if (!m_NavNormalPrefab || !m_NavSelectedPrefab)
                {
                    m_IsOpenNavIcon = false;
                }
                else
                {
                    var navNormalPrefabRt = m_NavNormalPrefab.transform as RectTransform;
                    if (!(navNormalPrefabRt is null)) m_NavNormalSizeX = navNormalPrefabRt.sizeDelta.x;

                    var navSelectedPrefabRt = m_NavSelectedPrefab.transform as RectTransform;
                    if (!(navSelectedPrefabRt is null)) m_NavSelectSizeX = navSelectedPrefabRt.sizeDelta.x;
                }
            }
        }

        protected override void ShowList(int num, bool bJumpStart = false)
        {
            if (num == 0)
            {
                m_TargetPos = Vector2.zero;
                SetNowIndex(0);
            }
            
            m_AllPageCount = num - m_OnePageCount + 1;
            base.ShowList(num, false);

            if (num > 0 && m_NowIndex >= num)
            {
                SetNowIndex(num - 1);
            }
            
            var targetIdx = defaultIndex >= 0 ? defaultIndex : m_NowIndex;
            
            ListPageValueInit();
            SetToPageIndex(targetIdx, true);
            defaultIndex = -1;
            m_LastAutoFlipTime = Time.time;
        }

        public override void AddItems(int totalCount)
        {
            base.AddItems(totalCount);
            m_AllPageCount += totalCount;
            ListPageValueInit();
        }

        public override void InsertItems(int posIdx, int insertCount)
        {
            base.InsertItems(posIdx, insertCount);
            m_AllPageCount += insertCount;
            ListPageValueInit();
        }

        //翻页到某一页
        private void SetToPageIndex(int index , bool teleport = false)
        {
            if (m_AllPageCount == 0)
            {
                defaultIndex = index;
                return;
            }
            
            m_IsDrag = false;
            
            var idx = index;
            idx %= m_AllPageCount;
            SetNowIndex(idx);
            
            m_Teleport = teleport ;
            var targetPos = GetSpecificCellPos(m_NowIndex);
            RefreshNavState(m_NowIndex);
            SetScroll2TargetPos(targetPos);

            if (m_Teleport)
            {
                m_ContentRectTrans.anchoredPosition = targetPos;
            }

            m_Dirty = true;
        }
        
        //滑动事件
        protected override void RefreshDragInView(bool bContentInView)
        {
            if (m_CanDragScrollView)
            {
                m_ScrollRect.enabled = m_CanDragInView || m_AllPageCount > 1;
            }
            else
            {
                m_ScrollRect.enabled = false;
            }
        }

        //每页比例  
        void ListPageValueInit()
        {
            m_AllPageCount = m_MaxCount - m_OnePageCount + 1;

            if (m_IsOpenNavIcon && m_AllPageCount > 1)
            {
                if (!m_ObjNavigationParent)
                {
                    return;
                }

                if (!m_NavSelectedTrans)
                {
                    var normalObj = GameObject.Instantiate(m_NavSelectedPrefab, Vector3.zero, Quaternion.identity, m_ObjNavigationParent);
                    normalObj.SetActive(true);
                    m_NavSelectedTrans = normalObj.transform;
                    m_NavSelectedTrans.localScale = Vector3.one;
                }

                // 生成normal状态的列表
                for (int i = 0; i < m_AllPageCount; ++i)
                {
                    if (i < m_NavNormalTransList.Count)
                    {
                        m_NavNormalTransList[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        var normalObj = GameObject.Instantiate(m_NavNormalPrefab, Vector3.zero, Quaternion.identity, m_ObjNavigationParent);
                        var tmpTrans = normalObj.transform;
                        tmpTrans.localScale = Vector3.one;
                        m_NavNormalTransList.Add(tmpTrans);
                        
                        normalObj.name = i.ToString();
                        normalObj.SetActive(true);
                        Button buttonComponent = normalObj.GetComponent<Button>();
                        if (buttonComponent != null)
                        {
                            buttonComponent.onClick.AddListener(delegate () { OnNavButtonClick(i); });
                        }
                    }
                }

                RefreshNavState(m_NowIndex);
            }

            int idx = m_AllPageCount > 0 ? m_AllPageCount : 0;
            for (int i = idx; i < m_NavNormalTransList.Count; ++i)
            {
                m_NavNormalTransList[i].gameObject.SetActive(false);
            }
        }

        protected override void Update()
        {
            if (!m_IsDrag)
            {
                if (m_ContentRectTrans == null) return;
                
                if (m_AllPageCount <= 1 || m_ScrollRect == null)
                {
                    return;
                }
                
                if (m_CanAutoScroll && IsLargeEnough() && Time.time - m_LastAutoFlipTime > m_AutoScrollSpeed)
                {
                    m_LastAutoFlipTime = Time.time;
                    
                    float spaing = 0f;
                    if(m_Direction == e_Direction.Vertical)
                    {
                        spaing = m_SpacingY;
                    }
                    else
                    {
                        spaing = m_SpacingX;
                    }
                    
                    MoveContent(spaing + 20f);
                    CheckItemIsOutRangeForAutoScroll(null);
                    
                    SetNowIndex(GetNextIndex(1));
                    RefreshNavState(m_NowIndex);
                }
                else
                {
                    if(m_Teleport)
                    {
                        m_Teleport = false;
                        m_ContentRectTrans.anchoredPosition = m_TargetPos;
                        m_ScrollLeftTime = 0f;
                        m_Dirty = true;
                    }
                    else
                    {
                        CheckScrollView2TargetPos();
                    }
                }
            }
            
            CheckDataDirty();
        }

        protected override bool NeedResetContentRectTransPos(int lastPage)
        {
            if (!m_CanAutoScroll && m_NowIndex >= 0 && m_NowIndex < m_CellInfos.Count)
            {
                CellInfo cellInfo = m_CellInfos[m_NowIndex];
                float pos = m_Direction == e_Direction.Vertical ? cellInfo.Pos.y : cellInfo.Pos.x;
                Vector3 curContentPos = m_ContentRectTrans.anchoredPosition;
                
                //需要补偿
                if(m_Direction == e_Direction.Vertical && curContentPos.y > pos ||
                   m_Direction == e_Direction.Horizontal && curContentPos.x < -pos
                   )
                {
                        return true;
                }
            }
            
            //-> 计算 Content 尺寸
            if (m_Direction == e_Direction.Vertical)
            {
                if (m_TargetPos.y < m_StartPos.y) //向下拖动时要变化
                {
                    return true;
                }
            }
            else
            {
                if (m_TargetPos.x > m_StartPos.x) //向右拖动时要变化
                {
                    return true;
                }
            }

            return false;
        }

        protected override void OnContentRectSizeChanged()
        {
            m_StartPos = m_ContentRectTrans.anchoredPosition;
        }

        protected override void OnCheckItemOutRangeEnd(Vector2 offset)
        {
            m_TargetPos -= offset;
        }

        protected override bool IsLargeEnough()
        {
            return m_AllPageCount > 1;
        }

        private int indexOnDrag = 0;
        /// 拖动开始   
        public override void OnBeginDrag(PointerEventData eventData)
        {
            beginCallback = false;
            base.OnBeginDrag(eventData);
            indexOnDrag = m_NowIndex;
        }

        /// 拖拽结束   
        public override void OnEndDrag(PointerEventData eventData)
        {
            beginCallback = false;
            base.OnEndDrag(eventData);

            if (m_ScrollRect)
            {
                m_ScrollRect.velocity = Vector2.zero;
                m_StartPos = m_ContentRectTrans.anchoredPosition;
            }
            
            CheckTargetItemIndex();
        }

        //翻页时的回调
        private void Func(Action<int> Func, int index)
        {
            if (Func == null)
                return;

            Func(index);
        }

        public void ResetNavPosForEditor()
        {
            if (m_ObjNavigationParent)
            {
                m_AllPageCount = m_ObjNavigationParent.childCount;
                
                if (m_AllPageCount > 0)
                {
                    m_NavNormalTransList.Clear();
                    for (int i = 0; i < m_AllPageCount; ++i)
                    {
                        RectTransform childRect = m_ObjNavigationParent.GetChild(i) as RectTransform;

                        if (i == 0)
                        {
                            m_NavNormalSizeX = m_NavSelectSizeX = childRect.sizeDelta.x;
                        }
                        m_NavNormalTransList.Add(childRect);
                    }

                    RefreshNavState(-1);
                }
            }
        }

        private bool beginCallback = false;
        private int lastNoticeEndIdx = -1;

        protected override void OnMoveBegin()
        {
            if(!beginCallback)
            {
                lastNoticeEndIdx = -1;
                beginCallback = true;
                
                if (m_FuncPageMoveBeginCallBack != null)
                {
                    Func(m_FuncPageMoveBeginCallBack, m_NowIndex);
                }
            }
        }

        protected override void OnMoveEnd()
        {
            if(lastNoticeEndIdx != m_NowIndex)
            {
                lastNoticeEndIdx = m_NowIndex;
                if (m_FuncPageMoveEndCallBack != null)
                {
                    Func(m_FuncPageMoveEndCallBack, m_NowIndex);
                }
            }
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (m_IsDrag)
            {
                CheckTargetItemIndex();
            }
        }

        private void CheckTargetItemIndex()
        {
            if (m_AllPageCount <= 1 || m_ScrollRect == null)
            {
                return;
            }

            if (_curOffset == 0)
            {
                return;
            }
            
            var selectIdx = GetNextIndex(_curOffset);
            if(m_LastIndex != selectIdx)
            {
                SetNowIndex(selectIdx);
                RefreshNavState(selectIdx);
                m_LastIndex = m_NowIndex;
            }
        }

        private void RefreshNavState(int selectedIndex)
        {
            if (m_IsOpenNavIcon)
            {
                float totalLength = m_NavNormalSizeX * (m_AllPageCount - 1) + m_NavSelectSizeX + m_NavSpacing * (m_AllPageCount - 1);
                float lastPos = -totalLength / 2;

                switch (m_CurNavAlignment)
                {
                    case NavAlignment.Left:
                        lastPos = 0f;
                        break;
                    case NavAlignment.Middle:
                        lastPos = -totalLength / 2;
                        break;
                    case NavAlignment.Right:
                        lastPos = -totalLength;
                        break;
                }
                
                float lastOffset = 0f;
                
                for (int i = 0; i < m_AllPageCount; ++i)
                {
                    UIUtils.SetActive(m_NavNormalTransList[i], i != selectedIndex);
                    
                    var curTrans = m_NavSelectedTrans;
                    var curOffset = m_NavSelectSizeX;
                    
                    if (i != selectedIndex)
                    {   
                        curOffset = m_NavNormalSizeX;
                        curTrans = m_NavNormalTransList[i];
                    }

                    lastPos = lastPos + lastOffset + curOffset * 0.5f;
                    curTrans.localPosition = new Vector3(lastPos, 0, 0);
                    lastOffset = curOffset * 0.5f + m_NavSpacing;
                }
            }
        }
        
        /// <summary>
        /// 按滑动方向获得下一个item
        /// </summary>
        /// <param name="offset">1:向上滑动或向左滑动; -1:向下滑动或向右滑动</param>
        /// <returns></returns>
        private int GetNextIndex(int offset)
        {
            if (m_AllPageCount <= 1 || offset == 0)
            {
                return m_NowIndex;
            }
            
            int nextIndex = m_NowIndex;
            int checkIndex = m_NowIndex + offset;
            if (checkIndex == -1)
            {
                checkIndex = m_CanAutoScroll ? m_MaxCount - 1 : 0;
            }else if (checkIndex == m_MaxCount)
            {
                checkIndex = m_CanAutoScroll ? 0 : m_MaxCount - 1;
            }
            
            int checkItemIndex = offset > 0 ? checkIndex + m_OnePageCount - 1 : checkIndex; //下一个要出来的ItemIdx
            checkItemIndex %= m_CellInfos.Count;
            
            checkIndex %= m_MaxCount;
            int checkPageIdx = -1;

            while (checkIndex >= 0 && checkIndex < m_MaxCount)
            {
                if (checkItemIndex < m_CellInfos.Count)
                {
                    Vector3 pos = m_CellInfos[checkItemIndex].Pos;
                    int pageIdx = m_CellInfos[checkItemIndex].PageIdx;
                    if (checkPageIdx >= 0 && (offset > 0 && pageIdx < checkPageIdx || offset < 0 && pageIdx > checkPageIdx))
                    {
                        break;
                    }

                    checkPageIdx = pageIdx;
                    
                    float rangePos = m_Direction == e_Direction.Vertical ? pos.y - m_ContentHeight * pageIdx : pos.x + m_ContentWidth * pageIdx;
                    
                    if (!IsOutRange(rangePos, m_CellInfos[checkItemIndex].ItemSize))
                    {
                        nextIndex = checkIndex;
                        int finalItemIndex = checkItemIndex;

                        if (offset > 0)
                        {
                            finalItemIndex -= (m_OnePageCount - 1);

                            if (finalItemIndex < 0)
                            {
                                finalItemIndex += m_CellInfos.Count;
                            }
                        }

                        if (!m_CanAutoScroll)
                        {
                            if ((offset > 0 && finalItemIndex == m_CellInfos.Count - 1 ||
                                offset < 0 && finalItemIndex == 0) && IsScrollViewOutView())
                            {
                                m_ScrollLeftTime = indexOnDrag == m_NowIndex ? 0f : m_ScrollLeftTime;
                                OnMoveEnd();
                                break;
                            }
                        }
                        
                        SetScroll2TargetPos(GetSpecificCellPos(finalItemIndex));
                        nextIndex = finalItemIndex;
                        break;
                    }
                }
                
                checkItemIndex += offset;
                checkIndex += offset;
            }

            return nextIndex;
        }

        private void OnNavButtonClick(int pageIdx)
        {
            SetToPageIndex(pageIdx);
        }

        private void SetNowIndex(int index)
        {
            m_NowIndex = index;
            onValueChanged?.Invoke(index);
        }
    }
}

