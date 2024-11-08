using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Proto;
using DHFramework;
using UnityEngine;

namespace DH.Game
{
    /// <summary>
    /// 走马灯管理类 主要是触发游戏中的走马灯，游戏里面的通知;
    /// </summary>
    public class BroadCostManager:Singleton<BroadCostManager>
    {
        // private List<RspBroadCast> broadCastsList = new();
        private Transform toastRoot;
        private BroadCostView broadCostView;
        
        

        public async UniTask Init()
        {
            Clear();
            toastRoot = UIManager.Instance.GetUILayerRoot(UILayersConfig.Toast);
            await InitInternal();
        }

        public void Clear()
        {
            if (broadCostView != null)
            {
                broadCostView.HideBroadCost();
                Object.Destroy(broadCostView.gameObject);
            }
            broadCostView = null;
            toastRoot = null;
        }

        /// <summary>
        /// 添加广播
        /// </summary>
        /// <param name="broadCast"></param>
        /// <param name="level"></param>
        // public void AddBroadCast(RspBroadCast broadCast, int level = 0)
        // {
        //     broadCastsList.Add(broadCast);
        //     broadCastsList.Sort((a, b) => b.Level - a.Level);
        //     CheckAndShowBroadCast();
        // }
        
        /// <summary>
        /// 添加游戏中的公告
        /// </summary>
        /// <param name="broadCast"></param>
        public void AddAnnouncement(RspAnnouncement broadCast)
        {
    
        }
        
        /// <summary>
        /// 检查走马灯是否播放完毕，如果没播放完继续播下一条
        /// </summary>
        public void CheckAndShowBroadCast()
        {
           
        }
        
        private async UniTask InitInternal()
        {
            var broadCostNode = await AssetsManager.InstantiateWithParentAsync("UI/Common/BroadCostItem", toastRoot, false);
            broadCostView = broadCostNode.GetComponent<BroadCostView>();
            broadCostView.HideBroadCost();
        }
    }

    public enum EBroadCostType
    {
        /// <summary>
        /// 通知
        /// </summary>
        BroadCostTypeNotice = 1,
        
        /// <summary>
        /// 假数据
        /// </summary>
        BroadCostTypeFake = 2,
        
    }
}