using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using DHFramework;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public partial class CommonTopViewModel : ViewModelBase
    {
        [AutoNotify] private ObservableList<ResItemViewModel> resItemsList = new ();
        [AutoNotify] private Action<ResourceData,int> clickItemAction;

        /// <summary>
        /// 用于区分是哪个脚本创建的
        /// </summary>
        private int ownerKey;
        private bool isShowAddIcon;
        
        private readonly List<GameConst.ItemIdCode> notJumpIdList = new List<GameConst.ItemIdCode> {
            GameConst.ItemIdCode.WeaponCore,
            GameConst.ItemIdCode.ChampionExchange,
            GameConst.ItemIdCode.blindBoxCommemorate
        };

        public void SetIsShowAddIcon(int id, bool isShow)
        {
            foreach (var item in ResItemsList)
            {
                if (item.CurData.Id == id)
                {
                    item.IsShowAddIcon = isShow;
                }
            }
        }

        [Preserve]
        public CommonTopViewModel(List<int> ids)
        {
            Init(ids,null);
        }
        public CommonTopViewModel(List<int> ids, Action<ResourceData,int> clickCommand = null)
        {
            Init(ids,clickCommand);
        }

        public CommonTopViewModel(List<GameConst.ItemIdCode> ids, Action<ResourceData,int> clickCommand = null)
        {
            StackFrame frame = new StackFrame(1);
            var callerMethod = frame.GetMethod();
            List<int> intIds = ids.Select(id => (int)id).ToList();
            Init(intIds,clickCommand);
        }

        private void Init(List<int> ids, Action<ResourceData, int> clickCommand)
        {
            for (int i = 0; i < ids.Count; i++)
            {
                int id = (int)ids[i];
                var resourceInfo = DataCenter.itemsData.ResourceDatas[id];
                if (id == (int)GameConst.ItemIdCode.Yuan && resourceInfo != null &&
                    resourceInfo.Count <= 0) continue;
                if(DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.ADFreeForever) && id ==(int)GameConst.ItemIdCode.AdFreeVouche) continue;
                var item = new ResItemViewModel(resourceInfo, 1);
                item.OnClickItemCallback = OnClickResItem;
                item.IsShowAddIcon = true;
                ResItemsList.Add(item);
            }

            clickItemAction = clickCommand;
            ownerKey = UIEffectManager.Instance.GetCommonTopVmNewId();
            DHLog.Debug($"muzili log top ownKey {ownerKey}");
            UIEffectManager.Instance.CommonTopVmDataList.Add(ownerKey, this);
        }

        protected override void OnDispose()
        {
            UIEffectManager.Instance.CommonTopVmDataList.Remove(ownerKey);
            base.OnDispose();
        }

        
        public void UpdateResList(List<int> list, bool showAdd = true)
        {
            foreach (var resItem in ResItemsList)
            {
                resItem.Dispose();
            }
            ResItemsList.Clear();
            Init(list,clickItemAction);
            foreach (var item in ResItemsList)
            {
                item.IsShowAddIcon = showAdd;
            }
        }

        /// <summary>
        /// 检查是否是在顶部展示
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsInShowItem(int id)
        {
            bool ret = false;
            foreach (var item in ResItemsList)
            {
                if (item.CurData.Id == id)
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }
        /// <summary>
        /// 获取顶部资源显示数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ResItemViewModel GetResItem(int id)
        {
            foreach (var item in ResItemsList)
            {
                if (item.CurData.Id == id) return item;
            }
            return null;
        }

        public async void OnClickResItem(ResourceData data, int tag)
        {
            //暂时只处理了道具，其他的内容自行维护
            if (data.Type == (int)RewardType.Item)
            {
                var itemCfg = ConfigCenter.ItemCfgColl.GetDataById(data.Id);
                if (itemCfg is { Obtain: { Count: > 0 } })
                {
                    JumpManager.Instance.Jump((FunctionJumpCfgId)itemCfg.Obtain[0]);
                }
            }
            clickItemAction?.Invoke(data, tag);
        }
    }
}