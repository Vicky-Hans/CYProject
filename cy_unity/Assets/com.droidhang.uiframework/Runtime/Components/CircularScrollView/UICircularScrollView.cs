//*****************************-》 基类 循环列表 《-****************************
//ScrollView和item的pivot必须用左上角的方式
//初始化:
//      Init()
//刷新整个列表（首次调用和数量变化时调用）:
//      ShowList(int = 数量, bJumpStart = 是否跳到开头)
//刷新单个项:
//      UpdateCell(int = 索引)
//刷新列表数据(无数量变化时调用):
//      UpdateList()
//回调:
//Func(GameObject = Cell, int = Index)  //刷新列表

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Asset;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DHFramework;
using UnityEngine.Serialization;

namespace DH.UIFramework
{
    public enum CellItemSizeType
    {
        CellOriginSize, //prefab 原大小
        DirectionAdaptingSize, //在滑动的方向适应大小（如竖直滑动时，每个item的高度可能不一样）
        ViewSize, //cell和view保持一致
    }
    
    public enum e_Direction
    {
        Horizontal,
        Vertical
    }

    [RequireComponent(typeof(ScrollRect))]
    public partial class UICircularScrollView : PoolRecyclable, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [SerializeField, Range(float.Epsilon, 1f)]
        float cellInterval;
        [SerializeField, Range(0f, 1f)]
        float cellOffset = 0f;
        [SerializeField]
        bool inertia = false; //是否吸附在cellOffset上位置上
        
        public e_Direction m_Direction = e_Direction.Vertical;
        public bool m_CanAutoScroll = false;
        public float m_AutoScrollSpeed = 50f;
        public int m_PreloadItemCount = 0;
        public int m_Row = 1; //单排显示的个数，对于单排显示多个的时候是不应该要自适应size变化的
        [FormerlySerializedAs("m_Spacing")] public float m_SpacingY = 0f; //纵向间距
        public float m_SpacingX = 0f; //横向间距
        public bool m_DefaultBottom = false;
        public float m_ShowTipOffset = 200;
        public bool m_loadAsync = true;
        public float m_PaddingLeft = 0;
        public float m_PaddingRight = 0;
        public float m_PaddingTop = 0;
        public float m_PaddingBottom = 0;
        public bool m_CanDragScrollView = true;
        public bool m_CanDragInView = true; //内容一页可全部显示时是否可拖拽
        public CellItemSizeType m_CellSizeType = CellItemSizeType.CellOriginSize;

        public Action<GameObject> m_RemoveItemCallBack; //删除该item时的回调
        public Action<GameObject, int> m_RefreshItemCallBack; //需要刷新该item
        public Func<int, object, string> GetPrefabPathFunction;
        public Action<bool> m_ShowTopTipCallBack;
        public Action<bool> m_ShowBottomTipCallBack;
        public Action<int, bool, GameObject> m_FuncOnButtonClickCallBack;
        public UnityEvent OnReleaseTopCallback = new();
        public UnityEvent OnReleaseDownCallback = new();
        [NonSerialized]
        public GameObject m_CellGameObject; //指定的cell

        public int DefaultJumpIndex
        {
            get => defaultJumpIndex;
            set => defaultJumpIndex = value;
        }

        public Vector2 CellItemSize
        {
            get => cellItemSize;
            set
            {
                cellItemSize = value;
                ResetCellSize(cellItemSize.x, cellItemSize.y);
            }
        }

        public bool Inited => m_isInited;

        protected RectTransform m_scrollViewRectTrans;
        protected Rect m_scrollViewRectTransRect;

        protected float m_ContentWidth;
        protected float m_ContentHeight;

        /// <summary>
        /// 这两个变量在自适应高度的时候不起作用
        /// </summary>
        protected float m_CellObjectWidth;

        protected float m_CellObjectHeight;

        protected GameObject m_Content;
        protected RectTransform m_ContentRectTrans;
        protected bool m_IsDrag = false; //是否拖拽中

        private bool m_isInited = false;
        protected bool m_Dirty = false;
        private bool m_autoScrolling = false;

        protected List<CellInfo> m_CellInfos = new();

        protected bool m_IsListInited = false;

        protected ScrollRect m_ScrollRect;

        protected int m_MaxCount = -1; //列表数量
        protected Dictionary<string, GameObject> m_DynamicPrefabDic = new();
        protected bool dataDirty;

        #region 自动滑动部分
        
        protected Vector2 m_StartPos;       //滑动的开始位置  
        protected Vector2 m_TargetPos;       //滑动的目标位置  
        protected float m_ScrollTime; //滑动的时间
        protected float m_ScrollLeftTime = 0f; //滑动剩余的时间

        #endregion
        
        #region 吸附某个位置，远离靠近时动效相关
        #endregion

        private int defaultJumpIndex = -1;
        private Vector2 cellItemSize = Vector2.one * -1;
        private bool firstAnimation;

        public void ResetInit(GameObject prefab)
        {
            m_isInited = false;
            m_CellObjectWidth = 0;
            m_CellObjectHeight = 0;
            
            Init();
            InitPrefab(prefab);
        }

        internal virtual void Init()
        {
            if (m_isInited)
                return;

            m_DynamicPrefabDic.Clear();
            m_ScrollRect = GetComponent<ScrollRect>();
            m_Content = m_ScrollRect.content.gameObject;

            if (m_Direction == e_Direction.Horizontal)
            {
                m_ScrollRect.horizontal = true;
                m_ScrollRect.vertical = false;
            }
            else
            {
                m_ScrollRect.horizontal = false;
                m_ScrollRect.vertical = true;
            }

            //记录 Plane 信息
            m_scrollViewRectTrans = m_ScrollRect.viewport;
            if (m_scrollViewRectTrans == null) m_scrollViewRectTrans = GetComponent<RectTransform>();

            m_scrollViewRectTransRect = m_scrollViewRectTrans.rect;
            m_CellObjectHeight = m_ContentHeight = m_scrollViewRectTransRect.height;
            m_CellObjectWidth = m_ContentWidth = m_scrollViewRectTransRect.width;

            //记录 Content 信息
            m_ContentRectTrans = m_Content.GetComponent<RectTransform>();

            InitPrefab(m_CellGameObject);

            m_ScrollRect.onValueChanged.RemoveAllListeners();
            //添加滑动事件
            m_ScrollRect.onValueChanged.AddListener(delegate(Vector2 value) { ScrollRectListener(value); });
            OnDragListener(Vector2.zero);

            m_isInited = true;

            m_RemoveItemCallBack += OnItemRemoved;
            m_RefreshItemCallBack += OnItemUpdated;
        }

        protected void InitPrefab(GameObject cellObj)
        {
            if (!m_isInited)
            {
                return;
            }
            
            if (cellObj)
            {
                var cellRectTrans = cellObj.GetComponent<RectTransform>();

                //记录 Cell 信息 只是prefab里的初始信息
                var needResetAllCell = !m_CellGameObject;

                if (needResetAllCell) m_CellGameObject = cellObj;

                if (m_Row == 1 && m_CellSizeType != CellItemSizeType.CellOriginSize)
                {
                    if (Application.isPlaying)
                    {
                        m_scrollViewRectTransRect = m_scrollViewRectTrans.rect;
                        var height = m_scrollViewRectTransRect.height;
                        var width = m_scrollViewRectTransRect.width;

                        if (m_CellSizeType == CellItemSizeType.ViewSize)
                            cellRectTrans.sizeDelta = new Vector2(width, height);
                        else
                            cellRectTrans.sizeDelta = m_Direction == e_Direction.Vertical
                                ? new Vector2(width, cellRectTrans.sizeDelta.y)
                                : new Vector2(cellRectTrans.sizeDelta.x, height);
                    }

                    if(needResetAllCell)
                    {
                        m_CellObjectWidth = cellRectTrans.sizeDelta.x;
                        m_CellObjectHeight = cellRectTrans.sizeDelta.y;
                    }

                    if (needResetAllCell && m_CellSizeType == CellItemSizeType.ViewSize) InitAllCellPos();
                }
                else if (needResetAllCell && cellItemSize == Vector2.one * -1)
                {
                    var rect = cellRectTrans.rect;
                    ResetCellSize(rect.width, rect.height);
                }
            }
        }

        protected void OnRectTransformDimensionsChange()
        {
            if (m_scrollViewRectTrans != null) m_scrollViewRectTransRect = m_scrollViewRectTrans.rect;
        }

        protected override void OnPrefabInstantiate(GameObject cell)
        {
            var cellRectTrans = cell.GetComponent<RectTransform>();
            CheckCellPrefabAnchor(cellRectTrans);
        }

        //检查 Anchor 是否正确
        private void CheckCellPrefabAnchor(RectTransform rectTrans)
        {
            if (!Application.isPlaying) return;

            rectTrans.pivot = Vector2.up;
            rectTrans.anchorMin = Vector2.up;
            rectTrans.anchorMax = Vector2.up;
            rectTrans.anchoredPosition = Vector2.zero;
        }

        /// <summary>
        /// index 从0开始
        /// </summary>
        /// <param name="index"></param>
        /// <param name="size"></param>
        public void OneCellSizeChanged(int index, float size)
        {
            if (index < 0 || index >= m_CellInfos.Count) return;

            var cellInfo = m_CellInfos[index];
            if (Mathf.Approximately(size, cellInfo.ItemSize)) return;

            var delta = size - cellInfo.ItemSize;
            cellInfo.SetItemSize(size);

            if ((m_CanAutoScroll && cellInfo.PageIdx > 0) ||
                ((m_IsDrag || m_ScrollRect.velocity.SqrMagnitude() > 0.0001f) && _curOffset == -1))
            {
                ReCalCellPos(0);

                Vector2 targetAnchorPos;
                var anchoredPosition = m_ContentRectTrans.anchoredPosition;
                if (m_Direction == e_Direction.Vertical)
                    targetAnchorPos = new Vector2(anchoredPosition.x, anchoredPosition.y + delta);
                else
                    targetAnchorPos = new Vector2(anchoredPosition.x - delta, anchoredPosition.y);

                m_ContentRectTrans.anchoredPosition = targetAnchorPos;
                m_ScrollRect.Rebuild(CanvasUpdate.PostLayout);

                if (_curOffset == -1 && _curEventData != null)
                {
                    var lastVelocity = m_ScrollRect.velocity;
                    m_ScrollRect.OnEndDrag(_curEventData);
                    m_ScrollRect.OnBeginDrag(_curEventData);
                    m_ScrollRect.velocity = lastVelocity;
                }
            }
            else
            {
                ReCalCellPos(index + 1, cellInfo.PageIdx);
            }

            ClampScrollView();

            m_Dirty = true;
        }

        public bool IsShowingLastItem()
        {
            if (m_CanAutoScroll) return false;

            var cellInfoCount = m_CellInfos.Count;
            var cellInfo = cellInfoCount > 0 ? m_CellInfos[cellInfoCount - 1] : null;

            if (cellInfo != null)
            {
                Vector3 pos = cellInfo.Pos;

                var rangePos = m_Direction == e_Direction.Vertical ? pos.y : pos.x;
                //判断是否超出显示范围
                return !IsOutRange(rangePos, cellInfo.ItemSize);
            }

            return true;
        }

        public void ReShowInRangeItems()
        {
            for (int i = 0, length = m_CellInfos.Count; i < length; i++)
            {
                if (!_inRangeIdxList.Contains(i)) continue;
                
                CellInfo cellInfo = m_CellInfos[i];
                if (!cellInfo.Go) continue;
                
                var uiView = cellInfo.Go.GetComponent<BaseView>();
                if (uiView)
                {
                    uiView.OnTopUIShow();
                }
            }
        }

        protected virtual void ShowList(int num, bool bJumpStart = false)
        {
            Init();

            m_lastNormalizedPos = -1f;
            m_firstChangeForList = true;

            var recycleIdx = bJumpStart ? 0 : num;

            //-> 过多的物体 扔到对象池
            if (m_CellInfos.Count > recycleIdx)
                for (var i = m_CellInfos.Count - 1; i >= recycleIdx; --i)
                {
                    var tmpCell = m_CellInfos[i];

                    RecycleCellInfo(tmpCell);

                    ReferencePool.Release(tmpCell);
                    m_CellInfos.RemoveAt(i);
                }

            var cellInfoCount = m_CellInfos.Count;
            //-> 1: 计算 每个Cell坐标并存储 2: 显示范围内的 Cell
            for (var i = 0; i < num; i++)
            {
                CellInfo cellInfo = null;
                if (i < cellInfoCount) cellInfo = m_CellInfos[i];

                InitCell(i, cellInfo);
                RecycleCellInfo(cellInfo);
            }

            InitAllCellPos();

            UpdateContentSize();

            if (DefaultJumpIndex >= 0)
            {
                if (DefaultJumpIndex < num)
                {
                    Jump2SpecificItem(DefaultJumpIndex);
                    DefaultJumpIndex = -1;
                }
            }
            else
            {
                if (bJumpStart && num != m_MaxCount) Jump2Start();
            }

            OnDragListener(Vector2.zero);
            m_MaxCount = num;
            m_IsListInited = true;
            m_Dirty = true;
        }

        public virtual void AddItems(int addCount)
        {
            m_MaxCount += addCount;
            var bLastShowing = IsShowingLastItem();
            var cellInfoCount = m_CellInfos.Count;

            //-> 1: 计算 每个Cell坐标并存储 2: 显示范围内的 Cell
            for (var i = cellInfoCount; i < m_MaxCount; i++) InitCell(i, null);

            ReCalCellPos(cellInfoCount, -1);

            if (bLastShowing) Jump2Start();

            ClampScrollView();

            m_Dirty = true;
        }

        //posIdx 从0开始的
        public virtual void InsertItems(int posIdx, int insertCount)
        {
            if (insertCount == 0) return;

            m_MaxCount += insertCount;

            var insertPageIdx = 0;
            //与posIdx前面一个的元素保持一致
            var prePosIdx = posIdx - 1;
            if (prePosIdx >= 0 && prePosIdx < m_CellInfos.Count)
            {
                var preCellInfo = m_CellInfos[prePosIdx];
                insertPageIdx = preCellInfo.PageIdx;
            }

            var endIndex = posIdx + insertCount;
            CellInfo firstCell = null;
            CellInfo lastCell = null;
            var delta = 0f;

            for (var i = posIdx; i < endIndex; i++)
            {
                InitCell(i, null, true);
                var cellInfo = m_CellInfos[i];
                cellInfo.PageIdx = insertPageIdx;
            }

            ReCalCellPos(endIndex, -1);

            if (!m_CanAutoScroll)
            {
                Jump2SpecificItem(endIndex - 1);
            }
            else
            {
                if (insertPageIdx > 0)
                {
                    ReCalCellPos(0);
                    for (var i = posIdx; i < endIndex; i++)
                    {
                        var cellInfo = m_CellInfos[i];
                        delta += cellInfo.ItemSize;
                        if (m_Direction == e_Direction.Vertical)
                            delta += m_SpacingY;
                        else
                            delta += m_SpacingX;
                    }

                    Vector2 targetAnchorPos;
                    var anchoredPosition = m_ContentRectTrans.anchoredPosition;
                    if (m_Direction == e_Direction.Vertical)
                        targetAnchorPos = new Vector2(anchoredPosition.x, anchoredPosition.y + delta);
                    else
                        targetAnchorPos = new Vector2(anchoredPosition.x - delta, anchoredPosition.y);

                    m_ContentRectTrans.anchoredPosition = targetAnchorPos;
                    m_ScrollRect.Rebuild(CanvasUpdate.PostLayout);
                }

                ClampScrollView();
                m_Dirty = true;
            }
        }

        public void Jump2SpecificItem(int index)
        {
            if (index <= 0)
            {
                Jump2Start();
                return;
            }
            _jumpIndex = index;
            if (m_ScrollRect == null) return;
            
            m_ScrollRect.velocity = Vector2.zero;

            if (index >= m_CellInfos.Count)
            {
                index = m_CellInfos.Count - 1;
            }
            
            var cellInfo = m_CellInfos[index];

            if (m_Direction == e_Direction.Vertical)
                m_ContentRectTrans.anchoredPosition = new Vector2(m_ContentRectTrans.anchoredPosition.x + m_PaddingLeft, -cellInfo.Pos.y);
            else
                m_ContentRectTrans.anchoredPosition = new Vector2(-cellInfo.Pos.x , m_ContentRectTrans.anchoredPosition.y - m_PaddingTop);

            ClampScrollView();

            m_Dirty = true;
        }

        public void Jump2Start()
        {
            Jump2StartOrEnd(true);
        }

        public void Jump2End()
        {
            Jump2StartOrEnd(false);
        }

        /// <summary>
        /// 滑动到绝对位置
        /// </summary>
        /// <param name="absolutePos">绝对位置</param>
        /// <param name="time">时间</param>
        public void ScrollToPos(Vector2 absolutePos, float time)
        {
            SetScroll2TargetPos(absolutePos, time);
        }

        /// <summary>
        /// 滑动一段相对的距离
        /// </summary>
        /// <param name="offset">相对距离</param>
        /// <param name="time">时间</param>
        public void ScrollOffsetDistance(Vector2 offset, float time)
        {
            var targetPos = m_ContentRectTrans.anchoredPosition + offset;
            
            SetScroll2TargetPos(targetPos, time);
        }

        /// <summary>
        /// 滑动到索引为itemIdx的item的位置，与Jump2SpecificItem对应，只不过有时间
        /// </summary>
        /// <param name="index">item 索引</param>
        /// <param name="offset">相对item的偏移量</param>
        /// <param name="time">时间</param>
        public void ScrollToSpecificItem(int index, Vector2 offset, float time)
        {
            if (index < 0)
            {
                return;
            }

            if (m_ScrollRect == null) return;

            if (index < m_CellInfos.Count)
            {
                var cellInfo = m_CellInfos[index];
                var targetPos = m_Direction == e_Direction.Vertical ? new Vector2(m_ContentRectTrans.anchoredPosition.x, -cellInfo.Pos.y) : new Vector2(-cellInfo.Pos.x, m_ContentRectTrans.anchoredPosition.y);
                targetPos += offset;
                SetScroll2TargetPos(targetPos, time);
            }
        }

        private void Jump2StartOrEnd(bool bStart)
        {
            if (m_ScrollRect == null) return;

            _jumpIndex = -1;
            m_ScrollRect.velocity = Vector2.zero;

            //-> 计算 Content 尺寸
            if (m_Direction == e_Direction.Vertical)
            {
                if ((bStart && m_DefaultBottom) || (!bStart && !m_DefaultBottom))
                    m_ContentRectTrans.anchoredPosition = new Vector2(m_ContentRectTrans.anchoredPosition.x,
                        m_ContentRectTrans.sizeDelta.y - m_scrollViewRectTransRect.height);
                else
                    m_ContentRectTrans.anchoredPosition = new Vector2(m_ContentRectTrans.anchoredPosition.x, 0);
            }
            else
            {
                if ((bStart && m_DefaultBottom) || (!bStart && !m_DefaultBottom))
                    m_ContentRectTrans.anchoredPosition = new Vector2(
                        m_scrollViewRectTransRect.width - m_ContentRectTrans.sizeDelta.x,
                        m_ContentRectTrans.anchoredPosition.y);
                else
                    m_ContentRectTrans.anchoredPosition = new Vector2(0, m_ContentRectTrans.anchoredPosition.y);
            }

            m_Dirty = true;
        }

        protected bool ClampScrollView()
        {
            bool clamped = false;
            //-> Clamp scrollView 位置
            if (m_Direction == e_Direction.Vertical)
            {
                var maxY = m_ContentRectTrans.sizeDelta.y - m_scrollViewRectTransRect.height;
                if (m_ContentRectTrans.anchoredPosition.y > maxY)
                {
                    m_ContentRectTrans.anchoredPosition = new Vector2(m_ContentRectTrans.anchoredPosition.x, maxY);
                    clamped = true;
                }

                if (m_ContentRectTrans.anchoredPosition.y < 0)
                {
                    m_ContentRectTrans.anchoredPosition = new Vector2(m_ContentRectTrans.anchoredPosition.x, 0);
                    clamped = true;
                }
            }
            else
            {
                var maxX = m_scrollViewRectTransRect.width - m_ContentRectTrans.sizeDelta.x;
                if (m_ContentRectTrans.anchoredPosition.x < maxX)
                {
                    m_ContentRectTrans.anchoredPosition = new Vector2(maxX, m_ContentRectTrans.anchoredPosition.y);
                    clamped = true;
                }

                if (m_ContentRectTrans.anchoredPosition.x > 0)
                {
                    m_ContentRectTrans.anchoredPosition = new Vector2(0, m_ContentRectTrans.anchoredPosition.y);
                    clamped = true;
                }
            }

            return clamped;
        }
        
        protected bool IsScrollViewOutView()
        {
            bool outView = false;
            //-> Clamp scrollView 位置
            if (m_Direction == e_Direction.Vertical)
            {
                var maxY = m_ContentRectTrans.sizeDelta.y - m_scrollViewRectTransRect.height;
                if (m_ContentRectTrans.anchoredPosition.y > maxY)
                {
                    outView = true;
                }

                if (m_ContentRectTrans.anchoredPosition.y < 0)
                {
                    outView = true;
                }
            }
            else
            {
                var maxX = m_scrollViewRectTransRect.width - m_ContentRectTrans.sizeDelta.x;
                if (m_ContentRectTrans.anchoredPosition.x < maxX)
                {
                    outView = true;
                }

                if (m_ContentRectTrans.anchoredPosition.x > 0)
                {
                    outView = true;
                }
            }

            return outView;
        }

        protected float UpdateContentSize(int maxPage = 0, bool checkDirectionOffset = false)
        {
            var delta = 0f;
            CellInfo lastCell = null;
            if (m_CellInfos.Count > 0) lastCell = m_CellInfos[m_CellInfos.Count - 1];

            float totalContentSize = 0;
            var bContentInView = false;
            var emptyContentSize = 0f;

            //-> 计算 Content 尺寸
            if (m_Direction == e_Direction.Vertical)
            {
                if (lastCell != null) totalContentSize = -lastCell.Pos.y + lastCell.ItemSize;

                m_ContentHeight = totalContentSize;
                totalContentSize = totalContentSize * (maxPage + 1) + m_SpacingY * maxPage;
                totalContentSize += m_PaddingBottom;
                totalContentSize += GetCellDirectionContentExtra();
                bContentInView = totalContentSize <= m_scrollViewRectTransRect.height;

                if (bContentInView) emptyContentSize = m_scrollViewRectTransRect.height - totalContentSize;

                totalContentSize = bContentInView ? m_scrollViewRectTransRect.height : totalContentSize;

                var sizeDelta = m_ContentRectTrans.sizeDelta;
                delta = totalContentSize - sizeDelta.y;
                sizeDelta = new Vector2(sizeDelta.x, totalContentSize);
                m_ContentRectTrans.sizeDelta = sizeDelta;
            }
            else
            {
                if (lastCell != null) totalContentSize = lastCell.Pos.x + lastCell.ItemSize;

                m_ContentWidth = totalContentSize;

                totalContentSize = totalContentSize * (maxPage + 1) + m_SpacingX * maxPage;
                totalContentSize += m_PaddingRight;
                totalContentSize += GetCellDirectionContentExtra();
                bContentInView = totalContentSize <= m_scrollViewRectTransRect.width;

                if (bContentInView) emptyContentSize = m_scrollViewRectTransRect.width - totalContentSize;

                totalContentSize = bContentInView ? m_scrollViewRectTransRect.width : totalContentSize;

                var sizeDelta = m_ContentRectTrans.sizeDelta;
                delta = totalContentSize - sizeDelta.x;
                sizeDelta = new Vector2(totalContentSize, sizeDelta.y);
                m_ContentRectTrans.sizeDelta = sizeDelta;
            }

            RefreshDragInView(bContentInView);

            if (Mathf.Approximately(_curOffset, 0) && checkDirectionOffset && bContentInView) UpdateDirectionOffset(emptyContentSize);

            return delta;
        }

        //滑动事件
        protected virtual void RefreshDragInView(bool bContentInView)
        {
            if (m_CanDragScrollView)
            {
                m_ScrollRect.enabled = m_CanDragInView || !bContentInView;
            }
            else
            {
                m_ScrollRect.enabled = false;
            }
        }

        //滑动事件
        protected virtual void ScrollRectListener(Vector2 value)
        {
            UpdateCheck(value);
            OnDragListener(value);
        }

        protected float m_lastNormalizedPos = -1f;
        protected bool m_firstChangeForList = false;

        private void UpdateCheck(Vector2 value)
        {
            if (m_CellInfos == null || m_CellInfos.Count == 0) return;

            var normalizedPos = m_Direction == e_Direction.Vertical ? value.y : value.x;

            if (Mathf.Abs(m_lastNormalizedPos - normalizedPos) < 0.0001f) return;

            if ((normalizedPos > 1.0001f && m_CellInfos[0].Go) ||
                (normalizedPos < 0f && m_CellInfos[m_CellInfos.Count - 1].Go))
                if (!m_firstChangeForList)
                    return;

            m_Dirty = true;
        }

        protected GameObject DynamicGetObj(string prefabPath)
        {
            if (!m_DynamicPrefabDic.ContainsKey(prefabPath))
            {
                m_DynamicPrefabDic.Add(prefabPath, null);
                LoadDynamicPrefab(prefabPath).Forget();
            }

            if (m_DynamicPrefabDic.TryGetValue(prefabPath, out var dynamicPrefab))
                return GetPoolsObj(dynamicPrefab, m_Content.transform);

            return null;
        }

        protected bool NeedShowCellItem(int idx)
        {
            return CellItemShowPredicate == null || CellItemShowPredicate(indexedEnumerable[idx]);
        }

        /// <summary>
        /// 取索引为idx的item对应的prefab
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        protected virtual GameObject GetPoolsObj(int idx)
        {
            if (!NeedShowCellItem(idx))
            {
                return null;
            }
            
            if (GetPrefabPathFunction != null)
            {
                var prefabTemplate = GetPrefabPathFunction(idx, indexedEnumerable[idx]);

                if (!string.IsNullOrEmpty(prefabTemplate)) return DynamicGetObj(prefabTemplate);
            }

            if (m_CellGameObject) return GetPoolsObj(m_CellGameObject, m_Content.transform);

            return null;
        }

        //存入 cell
        protected override void SetPoolsObj(GameObject cell)
        {
            if (cell != null)
            {
                m_RemoveItemCallBack?.Invoke(cell);

                base.SetPoolsObj(cell);
            }
        }

        //回调
        protected void Func(Action<GameObject, int> func, GameObject selectObject, bool isUpdate = false)
        {
            var num = int.Parse(selectObject.name);
            func?.Invoke(selectObject, num);
        }

        protected virtual void DisposeAll()
        {
            foreach (var item in m_DynamicPrefabDic)
            {
                if (!item.Value)
                {
                    continue;
                }
                
                AssetsManager.Release(item.Value);
            }
            m_DynamicPrefabDic.Clear();

            m_RefreshItemCallBack = null;
            GetPrefabPathFunction = null;
            // 释放Prefab资源
            PrefabPath = null;
            indexedEnumerable?.Invalidate();
            indexedEnumerable = null;
            Collection = null;
        }

        public virtual void Clear()
        {
            ShowList(0);
        }

        protected virtual bool IsLargeEnough()
        {
            var enough = false;

            if (m_Direction == e_Direction.Vertical)
                enough = m_ContentHeight > m_scrollViewRectTransRect.height * 2f;
            else
                enough = m_ContentWidth > m_scrollViewRectTransRect.width * 2f;

            return enough;
        }

        protected virtual void Update()
        {
            if (!m_IsDrag)
            {
                if (m_CanAutoScroll && IsLargeEnough())
                {
                    if (!m_autoScrolling && m_ScrollRect.velocity.sqrMagnitude < 0.1f)
                    {
                        m_autoScrolling = true;
                        _jumpIndex = -2;
                    }

                    if (m_autoScrolling) MoveContent(m_AutoScrollSpeed * Time.deltaTime);
                }
                
                CheckScrollView2TargetPos();
            }

            CheckDataDirty();
        }

        protected virtual void LateUpdate()
        {
            if (m_ContentRectTrans == null) return;

            if (m_CanAutoScroll && IsLargeEnough())
            {
                if (!m_IsDrag)
                {
                    var offset = CheckItemIsOutRangeForAutoScroll(null);
                    OnCheckItemOutRangeEnd(offset);
                }
            }
            else
            {
                if (m_Dirty) m_Dirty = !CheckItemIsOutRange();
            }
        }

        protected void OnDestroy()
        {
            DisposeAll();
        }

        protected Vector2 _lastPos;
        protected int _curOffset = 0; //1:向上滑动或向左滑动; -1:向下滑动或向右滑动
        protected int _jumpIndex = -2; //在拖动的时候无效
        protected PointerEventData _curEventData;

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (m_Dirty)
            {
                return;
            }
            
            if (!m_ScrollRect || !m_ScrollRect.enabled) return;

            _curEventData = eventData;
            _jumpIndex = -2;
            m_IsDrag = true;
            _curOffset = 0;
            m_autoScrolling = false;
            _lastPos = eventData.position;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!m_IsDrag)
            {
                return;
            }
            
            _curEventData = eventData;

            if (!m_ScrollRect || !m_ScrollRect.enabled) return;

            if ((m_Direction == e_Direction.Vertical && eventData.position.y > _lastPos.y)
                || (m_Direction == e_Direction.Horizontal && eventData.position.x < _lastPos.x))
                //向上滑动或向左滑动
                _curOffset = 1;
            else if ((m_Direction == e_Direction.Vertical && eventData.position.y < _lastPos.y)
                     || (m_Direction == e_Direction.Horizontal && eventData.position.x > _lastPos.x))
                //向下滑动或向右滑动
                _curOffset = -1;

            _lastPos = eventData.position;

            if (m_CanAutoScroll && IsLargeEnough()) CheckItemIsOutRangeForAutoScroll(eventData);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!m_IsDrag)
            {
                return;
            }
            
            _curEventData = eventData;
            m_IsDrag = false;

            if (!m_ScrollRect || !m_ScrollRect.enabled) return;

            if (!m_CanAutoScroll)
            {
                Vector3 listP = m_ContentRectTrans.anchoredPosition;
                Vector3 contentSize = m_ContentRectTrans.sizeDelta;

                if (m_Direction == e_Direction.Vertical)
                {
                    if (listP.y < -100)
                        //下拉到顶
                        OnReleaseTopCallback?.Invoke();

                    if (contentSize.y - listP.y < m_scrollViewRectTransRect.height - 100)
                        //上拉到底
                        OnReleaseDownCallback?.Invoke();
                }
            }
        }

        /// <summary>
        /// 处理靠边滚动的提示的显示或隐藏
        /// </summary>
        /// <param name="value"></param>
        private void OnDragListener(Vector2 value)
        {
            if (m_ShowTopTipCallBack == null && m_ShowBottomTipCallBack == null) return;

            if (m_Direction == e_Direction.Vertical)
            {
                if (m_ContentHeight - m_scrollViewRectTransRect.height < 10)
                {
                    m_ShowTopTipCallBack?.Invoke(false);
                    m_ShowBottomTipCallBack?.Invoke(false);
                    return;
                }
            }
            else
            {
                if (m_ContentWidth - m_scrollViewRectTransRect.width < 10)
                {
                    m_ShowTopTipCallBack?.Invoke(false);
                    m_ShowBottomTipCallBack?.Invoke(false);
                    return;
                }
            }

            var curContentPos = m_ContentRectTrans.anchoredPosition;
            if (m_Direction == e_Direction.Vertical)
            {
                m_ShowBottomTipCallBack?.Invoke(curContentPos.y + m_scrollViewRectTransRect.height <
                                                m_ContentHeight - m_ShowTipOffset);
                m_ShowTopTipCallBack?.Invoke(curContentPos.y > m_ShowTipOffset);
            }
            else
            {
                m_ShowBottomTipCallBack?.Invoke(-curContentPos.x + m_scrollViewRectTransRect.width <
                                                m_ContentWidth - m_ShowTipOffset);
                m_ShowTopTipCallBack?.Invoke(curContentPos.x < -m_ShowTipOffset);
            }
        }

        protected void MoveContent(float offset)
        {
            var curContentPos = m_ContentRectTrans.anchoredPosition;

            if (m_Direction == e_Direction.Vertical)
                m_ContentRectTrans.anchoredPosition = new Vector2(curContentPos.x, curContentPos.y + offset);
            else
                m_ContentRectTrans.anchoredPosition = new Vector2(curContentPos.x - offset, curContentPos.y);

            m_Dirty = true;
        }

        protected void SetScroll2TargetPos(Vector2 targetPos, float time = 0.5f)
        {
            m_TargetPos = targetPos;
            m_StartPos = m_ContentRectTrans.anchoredPosition;
            m_ScrollTime = m_ScrollLeftTime = time;

            OnMoveBegin();
        }

        protected void CheckScrollView2TargetPos()
        {
            if (m_ScrollLeftTime > 0f)
            {
                m_ScrollLeftTime -= Time.deltaTime;
                
                var t = Mathf.Clamp01((m_ScrollTime - m_ScrollLeftTime) / m_ScrollTime);
                var curPos = m_StartPos * (1 - t) + m_TargetPos * t;

                m_ContentRectTrans.anchoredPosition = curPos;
                if (!ClampScrollView())
                {
                    m_Dirty = true;
                }

                if (m_ScrollLeftTime < 0)
                {
                    OnMoveEnd();
                }
            }
        }

        protected virtual void OnMoveBegin()
        {
            
        }

        protected virtual void OnMoveEnd()
        {
            
        }

        protected Vector2 GetSpecificCellPos(int index)
        {
            if (index >= m_CellInfos.Count) return Vector2.zero;

            var cellInfo = m_CellInfos[index];

            var offset = 0f;

            if (m_Direction == e_Direction.Vertical)
                offset = -m_ContentHeight * cellInfo.PageIdx;
            else
                offset = m_ContentWidth * cellInfo.PageIdx;

            if (m_Direction == e_Direction.Vertical)
                return new Vector2(m_ContentRectTrans.anchoredPosition.x, -(cellInfo.Pos.y + offset));

            return new Vector2(-(cellInfo.Pos.x + offset), m_ContentRectTrans.anchoredPosition.y);
        }

        protected void CheckDataDirty()
        {
            if (dataDirty)
            {
                ShowList(indexedEnumerable?.Count ?? 0);
                m_Dirty = !CheckItemIsOutRange();
                dataDirty = false;
            }
        }
    }
}