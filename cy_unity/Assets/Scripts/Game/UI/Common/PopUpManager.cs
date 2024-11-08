using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game;
using DH.Game.UIViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;
using Game.UI.CommonView;
using Game.UI.MainUi;

namespace Game.UI
{
    public partial class PopUpData:ObservableObject
    {
        /// <summary>
        /// 优先级，数值越大越先弹出
        /// </summary>
        [AutoNotify] private int priority;

        [AutoNotify] private Action callback;
        public PopUpData(Action callback, int priority)
        {
            Callback = callback;
            Priority = priority;
        }
    }

    public partial class FunctionOpenPopUpData:ObservableObject
    {
        /// <summary>
        /// 活动类型
        /// </summary>
        [AutoNotify] private EFunctionOpenType functionType;
        /// <summary>
        ///  是否弹出
        /// </summary>
        [AutoNotify] private bool isPopUp;
        /// <summary>
        /// 配置
        /// </summary>
        [AutoNotify] private FunctionOpenCfg  cfg;

        /// <summary>
        /// 回调
        /// </summary>
        [AutoNotify] private Action callback;

        public FunctionOpenPopUpData(EFunctionOpenType type, bool isPop)
        {
            FunctionType = type;
            IsPopUp = isPop;
            Cfg = ConfigCenter.FunctionOpenCfgColl.GetDataById((int)type);
        }
    }

    public class PopUpManager: DH.UIFramework.ObservableSingleton<PopUpManager>
    {
        private List<PopUpData> popUpList = new ();
        private List<FunctionOpenPopUpData> funtionOpenPopupList = new();
        public void Init()
        {
            InitFunctionOpenPopUp();
            Clear();
        }

        private void InitFunctionOpenPopUp()
        {
            funtionOpenPopupList.Clear();
            var cfgs = ConfigCenter.FunctionOpenCfgColl.DataItems;
            foreach (var cfg in cfgs)
            {
                EFunctionOpenType type =(EFunctionOpenType)cfg.Id;
                Action callback = () =>
                {
                     //FunctionJumpManager.Instance.FunctionJump(cfg.ObtainId);
                };
                bool isUnlock = MainUiManager.Instance.CheckFunctionIsUnlock(type);
                FunctionOpenPopUpData tempData = new(type,isUnlock);
                tempData.Callback = callback;
                funtionOpenPopupList.Add(tempData);
            }
        }
        public void Clear()
        {
            popUpList.Clear();
        }

        public void AddPopUp(Action callback, int priority)
        {
            var tempData = new PopUpData(callback, priority);
            popUpList.Add(tempData);
            popUpList.Sort((a, b) =>
            {
                return b.Priority - a.Priority;
            });
        }

        public void CheckAndPopUpView()
        {
            if(GameConst.IsIosAuditState)return;
            if (popUpList.Count <= 0) return;
            var tempData = popUpList[0];
            popUpList.RemoveAt(0);
            tempData.Callback?.Invoke();
           
        }

        public void CheckAndAddFunctionOpenPopUpWindows()
        {
            if(GameConst.IsIosAuditState)return;
            foreach (var item in funtionOpenPopupList)
            {
                if(item.IsPopUp) continue;
                bool isUnlock = MainUiManager.Instance.CheckFunctionIsUnlock(item.FunctionType);
                if(!isUnlock) continue;
                if (item.Cfg.Page != 1)continue;
                if (item.Cfg.Icon == null)
                {
                    DHLog.Warning($"g功能开启弹窗没有配置 icon  {item.Cfg.Id}");   
                    continue;
                }
                AddPopUp(() => { 
                    // FunctionOpenViewModel tempVm = new (item);
                    // UIManager.Instance.OpenDialog<FunctionOpenView>(tempVm).Forget();
                    NewFunctionUnlockViewModel tempVm = new (item.FunctionType);
                    UIManager.Instance.OpenDialog<NewFunctionUnlockView>(tempVm).Forget();
                    item.IsPopUp = true;
                }, 10 - item.Cfg.Seq);
            }
        }

        public bool GetFunctionOpenPopUpIsPopUp(EFunctionOpenType type)
        {
            var data = funtionOpenPopupList.Find(a => a.FunctionType == type);
            if (data == null) return true;
            return data.IsPopUp;
        }
        public void SetFunctionOpenPopUpIsPopUp(EFunctionOpenType type,bool isPop)
        {
            var data = funtionOpenPopupList.Find(a => a.FunctionType == type);
            if (data == null) return;
            data.IsPopUp = isPop;
        }
    }
}