using System;
using System.Collections.Specialized;
using System.ComponentModel;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.UIFramework.ViewModels;
using DH.UIFramework;
using DH.UIFramework.Commands;
using DHFramework;
using Extend;
using Game.UI.MainUi;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class TabBtnItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private string iconPath;
		[AutoNotify] private string opBtnTextStr;
        [AutoNotify] private Action< ETabType,Tuple<Vector3, Vector3>> onClickTabCallback;
        [AutoNotify] private ETabType tabType;
        [AutoNotify] private bool isLocked;
        [AutoNotify] private bool isShowRedDot;
        [AutoNotify] private bool isShowOpBtnText;
        [AutoNotify] private bool isShowTips;
        [AutoNotify] private Vector3 iconScale = Vector3.one;
        [AutoNotify] private DhImage cellIcon;
        private readonly SimpleCommand<Tuple<Vector3, Vector3>> clickOpBtn;
        private int[][] functionIdArray = new int[][] {
             new int[] {(int)EFunctionOpenType.FunctionTypeShop},
             new int[] {(int)EFunctionOpenType.FunctionRole},
             new int[] {(int)EFunctionOpenType.FunctionTypeMainStage},
             new int[] {(int)EFunctionOpenType.FunctionTypeEquip},
             new int[] {(int)EFunctionOpenType.FunctionDailyFight,(int)EFunctionOpenType.FunctionEndless}
             
        };
        // private string[] chooseImgNameArray = new string[] { "mainui_icon_1", "mainui_icon_2" , "mainui_icon_3", "mainui_icon_4", "mainui_icon_5"};
        private string[] chooseImgNameArray = new string[] { "mainui_icon_1","mainui_icon_2", "mainui_icon_3" , "mainui_icon_4","mainui_icon_5"};
        // private string[] chooseImgNameArray = new string[] { "mainui_icon_1", "mainui_icon_3" , "mainui_icon_2"};
        public ICommand OpBtnClick=>clickOpBtn;
        static  string roleName => ConfigCenter.FunctionOpenLanguageCfgColl.GetDataById((int)EFunctionOpenType.FunctionRole).Name;
        
        public TabBtnItemViewModel(ETabType type, Action<ETabType,Tuple<Vector3, Vector3>> callback)
        {
            TabType = type;
            onClickTabCallback = callback;
            clickOpBtn = new SimpleCommand<Tuple<Vector3, Vector3>>(OnClickOpBtn);
            IsShowOpBtnText = MainUiManager.Instance.CurTabType == TabType;
            IconScale = Vector3.one;
            RefreshTips();
            UpdateTabBtnInfo();
            RefreshRedState();
            DataCenter.endlessData.PropertyChanged += OnEndlessDataChanged;
            MainUiManager.Instance.PropertyChanged += OnMainUiManagerChanged;
            DataCenter.equipData.PropertyChanged += OnEquipDataItemChanged;
            DataCenter.equipData.Items.CollectionChanged += OnEquipDataItemCollection;
            DataCenter.equipData.Formations.CollectionChanged += OnEquipDataItemCollection;
            DataCenter.shopData.Recruit.PropertyChanged += RecruitChanged;
            DataCenter.shopData.PropertyChanged += RecruitChanged;
            DataCenter.roleData.PropertyChanged += RecruitChanged;
            DataCenter.dailyFightData.PropertyChanged += OnDailyFightDataChanged;
            ClothesManager.Instance.PropertyChanged += ClothesManagerPropertyChanged;
        }

        protected override void OnDispose()
        {
            DataCenter.endlessData.PropertyChanged -= OnEndlessDataChanged;
            MainUiManager.Instance.PropertyChanged -= OnMainUiManagerChanged;
            DataCenter.equipData.PropertyChanged -= OnEquipDataItemChanged;
            DataCenter.equipData.Items.CollectionChanged -= OnEquipDataItemCollection;
            DataCenter.equipData.Formations.CollectionChanged -= OnEquipDataItemCollection;
            DataCenter.shopData.Recruit.PropertyChanged -= RecruitChanged;
            DataCenter.shopData.PropertyChanged -= RecruitChanged;
            DataCenter.roleData.PropertyChanged -= RecruitChanged;
            DataCenter.dailyFightData.PropertyChanged -= OnDailyFightDataChanged;
            ClothesManager.Instance.PropertyChanged -= ClothesManagerPropertyChanged;
            base.OnDispose();
        }

        private void ClothesManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshRedState();
        }

        private void OnDailyFightDataChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshRedState();
        }

        private void RecruitChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshRedState();
        }

        private void OnEquipDataItemCollection(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshTips();
        }
        /// <summary>
        /// 监听无尽关卡挑战次数变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEndlessDataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(DataCenter.endlessData.Count) ||
                tabType != ETabType.TabTypeActivity) return;
            RefreshRedState();
        }
        /// <summary>
        /// 刷新当前tab红点
        /// </summary>
        private void RefreshRedState()
        {
            if (tabType == ETabType.TabTypeActivity)
            {
                if (MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionEndless))
                {
                    IsShowRedDot = DataCenter.endlessData.Count > 0;
                }
                if (MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionDailyFight))
                {
                    var dailyFightOpenTime = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.dailyStage_05);
	        
                    if (dailyFightOpenTime != null && dailyFightOpenTime.Content.Count > 0)
                    {
                        IsShowRedDot = ServerTime.Instance.GetNowTime() >= dailyFightOpenTime.Content[0] && DataCenter.dailyFightData.CheckIsShowRedDot();
                    }
                }
            }else if (tabType == ETabType.TabTypeShop && MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionTypeShop))
            {
                IsShowRedDot = ShopManager.Instance.CheckShopRed();
            }else if (tabType == ETabType.Role && MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionRole))
            {
                 IsShowRedDot = DataCenter.roleData.AllHeroRed() || ClothesManager.Instance.ClothesRed;
            }
        }
        private void OnEquipDataItemChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshTips();
        }
        
        private void OnMainUiManagerChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(MainUiManager.Instance.CurTabType))
            {
                // 根据当前页刷新显示
                IsShowOpBtnText = MainUiManager.Instance.CurTabType == TabType;
                IconScale = Vector3.one;
                UpdateTabBtnInfo();
            }
        }

        private void RefreshTips()
        {
            IsShowTips = tabType == ETabType.TabTypeEquip && EquipManager.Instance.IsExistNoneSlots();
        }

        public void OnClickOpBtn(Tuple<Vector3, Vector3> info)
        {
            // 锁住了点击无响应
            if (IsLocked)
            {
                var str = GetLockTipsStr();
                ToastManager.Show(str);
            }
            onClickTabCallback?.Invoke(TabType,info);
        }

        public string GetLockTipsStr()
        {
            var ret = "";
            var ids = functionIdArray[(int)TabType];
            for (var i = 0; i < ids.Length; i++)
            {
                var functionId = ids[i];
                if (functionId == (int)EFunctionOpenType.FunctionDailyFight&&!GameManager.Instance.IsOpenDailyFight()) continue;//特殊判断每日挑战没有到开启时间，不加入解锁判断
                if (!MainUiManager.Instance.CheckFunctionIsUnlock(functionId))
                {
                    ret = MainUiManager.Instance.GetFunctionUnLockTips(functionId);
                    break; 
                }
            }
            // foreach (var id in ids)
            // {
            //     var functionId = id;
            //     if (functionId == (int)EFunctionOpenType.FunctionDailyFight&&!GameManager.Instance.IsOpenDailyFight()) continue;//特殊判断每日挑战没有到开启时间，不加入解锁判断
            //     if (!MainUiManager.Instance.CheckFunctionIsUnlock(id))
            //     {
            //         ret = MainUiManager.Instance.GetFunctionUnLockTips(functionId);
            //         break; 
            //     }
            // }
            return ret;
        }

        private void UpdateTabBtnInfo()
        {
            var nameStr= chooseImgNameArray[(int)tabType];
            var unChooseName = $"mainui[{nameStr}]";
            var chooseName = $"mainui[{nameStr}_2]";
            IconPath = MainUiManager.Instance.CurTabType == TabType ? chooseName: unChooseName;
            var functionId = functionIdArray[(int)TabType];
            OpBtnTextStr = LocalizeHelper.GetFunctionOpenGlobal(functionId[0]);
            if (ETabType.TabTypeActivity == TabType)
            {
                OpBtnTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.endless_15);
            }

            var ids = functionIdArray[(int)TabType];
            bool isUnLock = false;
            foreach (var id in ids)
            {
                isUnLock = MainUiManager.Instance.CheckFunctionIsUnlock(id);
                if (isUnLock && TabType == ETabType.TabTypeActivity &&
                    id == (int)EFunctionOpenType.FunctionDailyFight)
                {
                    var dailyFightOpenTime = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.dailyStage_05);
                    if (dailyFightOpenTime != null && dailyFightOpenTime.Content.Count > 0)
                    {
                        isUnLock = ServerTime.Instance.GetNowTime() >= dailyFightOpenTime.Content[0];
                    }
                    else
                    {
                        isUnLock = false;
                    }
                }

                if(isUnLock) break;
            }

            IsLocked = !isUnLock;

        }
    }
}