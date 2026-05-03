using System;
using System.Collections.Generic;
using UnityEngine;
using DHFramework;
using UnityEditor;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace DH.UIFramework
{
    public class BulletChatView : PoolRecyclable
    {
        //当前通道的一些基本信息
        protected class ChannelInfo
        {
            public float YPos;  //所在Y位置
            public float NextBulletTime; //取下一个消息的时间
            public float CurSpeed = 0f;
        }
        
        protected class CellInfo : IReference
        {
            public int ItemIndex = 0;
            public float ItemWidth = 0f;
            public Transform ObjTrans = null;
            public float CurSpeed = 0f;
            public float CurPosX = 0f;
            public float CurPosY = 0f;

            private bool bPaused = false;
            private Button CellButton;
            private Action<CellInfo> ClickCallback;
            
            public void InitCell(GameObject cellObj, float speed, float posX, float posY)
            {
                CellButton = cellObj.GetComponent<Button>();
                CurSpeed = speed;
                CurPosX = posX;
                CurPosY = posY;
                ObjTrans = cellObj.transform;
                ObjTrans.localPosition = new Vector3(CurPosX, CurPosY, 0);
            }

            public void RegisterClickCallback(Action<CellInfo> callback)
            {
                if (CellButton)
                {
                    CellButton.onClick.AddListener(OnClickCallback);
                    ClickCallback = callback;
                } 
            }

            public void Resume()
            {
                bPaused = false;
            }
            
            public void RefreshObjPos()
            {
                if (bPaused)
                {
                   return;
                }
                
                
                if (ObjTrans != null)
                {
                    CurPosX -= Time.deltaTime * CurSpeed;
                    ObjTrans.localPosition = new Vector3(CurPosX, CurPosY, 0);
                }
            }

            public void Clear()
            {
                if (CellButton)
                {
                    CellButton.onClick.RemoveListener(OnClickCallback);
                }
                
                ClickCallback = null;
                ObjTrans = null;
            }

            void OnClickCallback()
            {
                bPaused = true;
                ClickCallback?.Invoke(this);

                if (ClickCallback != null)
                {
                    ObjTrans.SetAsLastSibling();
                }
            }
        }
        
        public Action<GameObject, int> m_RemoveItemCallBack; //删除该item时的回调
        public Action<GameObject, int> m_RefreshItemCallBack; //需要刷新该item
        public Action<int> m_OnItemPauseCallBack; //点击暂停
        public Action<int> m_OnItemResumeCallBack; //恢复的回调
        public Func<int, float> m_GetItemAdaptingSize; //得到Item自适应方向的大小

        public float m_LineSpacing = 10f;
        public float m_BaseCellSpeed = 120f;
        public float m_SpeedRandomRange = 50f;
        public float m_BaseCellSpacing = 80f;
        [HideInInspector]public Transform m_CellParentTrans;
        [HideInInspector]public GameObject m_CellGameObject; //指定的cell
        
        private float m_CellObjectWidth;
        private float m_CellObjectHeight;
        private RectTransform m_CellContentRectTrans;
        
        private List<ChannelInfo> m_ChannelList = new List<ChannelInfo>(); //可滚动的轨道
        private Queue<CellInfo> m_CellList = new Queue<CellInfo>(); //待出现的Cell
        private List<CellInfo> m_InScrollingCellList = new List<CellInfo>(); //正在滚动中的Cell

        #region 对外的函数

        // Start is called before the first frame update
        public void Init()
        {
            RectTransform cellRectTrans = m_CellGameObject.GetComponent<RectTransform>();
            cellRectTrans.pivot = new Vector2(0f, 1f);
            
            //记录 Cell 信息 只是prefab里的初始信息
            var rect = cellRectTrans.rect;
            m_CellObjectHeight = rect.height;
            m_CellObjectWidth = rect.width;

            m_CellContentRectTrans = m_CellParentTrans as RectTransform;
            
            if (m_CellContentRectTrans.pivot != new Vector2(0, 1))
            {
                Debug.LogError(m_CellContentRectTrans.name + "的pivot 需要是（0，1）");
            }
            
            var rectChangeListener = m_CellParentTrans.GetComponent<OnTransformDimensionChanged>();
            if (rectChangeListener == null)
            {
                rectChangeListener = m_CellParentTrans.gameObject.AddComponent<OnTransformDimensionChanged>();
            }
            
            rectChangeListener.OnSizeChangedRect += OnContentRtChanged;
            
            InitChannelList();
        }

        public virtual void ShowList(int num)
        {
            //-> 把正在滚动的弹幕回收
            if (m_InScrollingCellList.Count > 0)
            {
                for (int i = m_InScrollingCellList.Count - 1; i >= 0; --i)
                {
                    var tmpCell = m_InScrollingCellList[i];
                    if (tmpCell.ObjTrans != null)
                    {
                        SetPoolsObj(tmpCell.ObjTrans.gameObject);
                        tmpCell.Clear();
                    }
                    
                    ReferencePool.Release(tmpCell);
                }
                
                m_InScrollingCellList.Clear();
            }

            while (m_CellList.Count > 0)
            {
                var tmpCell = m_CellList.Dequeue();
                if (tmpCell.ObjTrans != null)
                {
                    SetPoolsObj(tmpCell.ObjTrans.gameObject);
                    tmpCell.Clear();
                }
                    
                ReferencePool.Release(tmpCell);
            }

            for (int i = 0; i < num; ++i)
            {
                var tmpCell = ReferencePool.Acquire<CellInfo>();
                tmpCell.ItemIndex = i;
                tmpCell.ItemWidth = 0;
                
                m_CellList.Enqueue(tmpCell);
            }
        }

        public void Resume()
        {
            if (lastPauseCell != null)
            {
                m_OnItemResumeCallBack?.Invoke(lastPauseCell.ItemIndex + 1);
            }
            
            lastPauseCell?.Resume();
            lastPauseCell = null;
        }
        
        public void Clear()
        {
            ShowList(0);
        }

        #endregion

        protected void OnContentRtChanged(RectTransform contentRt)
        {
            m_CellContentRectTrans = contentRt;
            
            //-> 把正在滚动的弹幕回收
            if (m_InScrollingCellList.Count > 0)
            {
                for (int i = m_InScrollingCellList.Count - 1; i >= 0; --i)
                {
                    var tmpCell = m_InScrollingCellList[i];
                    if (tmpCell.ObjTrans != null)
                    {
                        SetPoolsObj(tmpCell.ObjTrans.gameObject);
                        tmpCell.Clear();
                    }
                    
                    m_CellList.Enqueue(tmpCell);
                }
                
                m_InScrollingCellList.Clear();
            }
            
            InitChannelList();
        }
        
        // 初始化可以滚动的通道
        private void InitChannelList()
        {
            if (m_ChannelList == null)
            {
                m_ChannelList = new List<ChannelInfo>();
            }
            
            int count = (int)((m_CellContentRectTrans.rect.height - m_CellObjectHeight) / (m_CellObjectHeight + m_LineSpacing)) + 1;

            for (int i = 0; i < count; ++i)
            {
                ChannelInfo info = null;
                if (i < m_ChannelList.Count)
                {
                    info = m_ChannelList[i];
                }

                if (info == null)
                {
                    info = new ChannelInfo();
                    m_ChannelList.Add(info);
                }
                
                info.YPos = -m_LineSpacing - (m_CellObjectHeight + m_LineSpacing) * i;
                info.NextBulletTime = Time.time + Random.Range(0, 1f);
                info.CurSpeed = m_BaseCellSpeed + Random.Range(0, m_SpeedRandomRange);
            }

            for (int i = m_ChannelList.Count - 1; i >= count; --i)
            {
                m_ChannelList.RemoveAt(i);
            }
        }

        //生成一个弹幕
        private void BulletOneChat(ChannelInfo info)
        {
            if (m_CellList.Count > 0)
            {
                var cell = m_CellList.Dequeue();
                var cellObj = GetPoolsObj(m_CellGameObject, m_CellParentTrans);

                var speed = info.CurSpeed;
                cell.InitCell(cellObj, speed, m_CellContentRectTrans.rect.width, info.YPos);
                cell.RegisterClickCallback(OnClickCellCallback);

                if (Mathf.Approximately(cell.ItemWidth, 0))
                {
                    float size = -1;
                    if (m_GetItemAdaptingSize != null)
                    {
                        size = m_GetItemAdaptingSize(cell.ItemIndex + 1);
                    }

                    if (size < 0)
                    {
                        size = m_CellObjectWidth;
                    }
                    
                    cell.ItemWidth = size;
                }
                
                m_RefreshItemCallBack?.Invoke(cellObj, cell.ItemIndex + 1);
                m_InScrollingCellList.Add(cell);
                
                //计算下一次的时间
                float space = Mathf.Min(m_BaseCellSpacing, cell.ItemWidth);
                float time = (cell.ItemWidth + space) / speed;
                info.NextBulletTime = Time.time + time;
            }
            else
            {
                info.NextBulletTime = Time.time + 1f;
            }
        }

        private CellInfo lastPauseCell;
        
        private void OnClickCellCallback(CellInfo cell)
        {
            bool bSameCell = cell == lastPauseCell;
            Resume();

            if (!bSameCell)
            {
                lastPauseCell = cell;
                m_OnItemPauseCallBack?.Invoke(cell.ItemIndex + 1);
            }
        }

        private void RecycleOneChat(CellInfo cell)
        {
            var cellObj = cell.ObjTrans.gameObject;
            m_RemoveItemCallBack?.Invoke(cellObj, -1);
            
            SetPoolsObj(cellObj);
            cell.Clear();
        }

        private void Update()
        {
            //检查是否需要弹出弹幕
            foreach (var channelInfo in m_ChannelList)
            {
                if (Time.time > channelInfo.NextBulletTime)
                {
                    BulletOneChat(channelInfo);
                }
            }
            
            for (int i = m_InScrollingCellList.Count - 1; i >= 0; --i)
            {
                var cell = m_InScrollingCellList[i];
                cell.RefreshObjPos();
            }
        }
        
        protected void LateUpdate()
        {
            for (int i = m_InScrollingCellList.Count - 1; i >= 0; --i)
            {
                var cell = m_InScrollingCellList[i];
                
                if (cell.ObjTrans.localPosition.x < -cell.ItemWidth)
                {
                    RecycleOneChat(cell);
                    m_InScrollingCellList.RemoveAt(i);
                    m_CellList.Enqueue(cell);
                }
            }
        }
        
        protected void OnDestroy()
        {
            DisposeAll();
        }
        
        protected virtual void DisposeAll()
        {
            m_GetItemAdaptingSize = null;
            m_RemoveItemCallBack = null;
            m_RefreshItemCallBack = null;
            m_OnItemPauseCallBack = null;
        }
    }
}
