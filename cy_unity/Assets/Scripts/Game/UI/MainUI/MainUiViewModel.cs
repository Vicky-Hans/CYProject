using System.Collections.Generic;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DHFramework;
using Game.UI;
using Game.UI.MainUi;

namespace DH.Game.ViewModels
{
    public partial class MainUiViewModel : ViewModelBase
    {
        
		[AutoNotify] private CommonTopViewModel commonTopItemsVm;
		[AutoNotify] private BottomItemViewModel bottomNodeVm;
        public MainUiManager MainUI = MainUiManager.Instance;
        [AutoNotify] private CommonHeadItemViewModel headVm;
        [AutoNotify] private string playerLevelStr;
        [AutoNotify] private float playerExpValue;
        [AutoNotify] private MainStageInfoViewModel mainStageInfoVm = new();
        [AutoNotify] private EquipViewModel equipModelVm = new();
        [AutoNotify] private ShopViewModel shopModelVm = new();
        [AutoNotify] private RoleViewModel roleViewVm = new();
        [AutoNotify] private ActivityViewModel activityViewVm = new();
        
        [AutoNotify] private CommonPlayerNameViewModel commonPlayerNameVm;
        [AutoNotify] private ClothesViewModel clothesViewVm = new();
        [Preserve]
        public MainUiViewModel()
        {
            UpdatePlayerInfo();
            List<int> list = new List<int>() {(int)GameConst.ItemIdCode.EnergyDrink, (int)GameConst.ItemIdCode.Money, (int)GameConst.ItemIdCode.Stone};
            CommonTopItemsVm = new CommonTopViewModel(list, OnClickResItem);
            bottomNodeVm = new BottomItemViewModel(OnClickTabBtn);
            MainUI.PropertyChanged += OnMainUiChanged;
            DataCenter.charcaterData.Digest.PropertyChanged += OnCharcaterDataChanged;
            DataCenter.monthCardData.PropertyChanged += MonthCardDataChanged;
            DataCenter.charcaterData.PropertyChanged += OnInviteNumberChanged;
            // 这里做一个初始化 
            DelayUpdateBottomInfo().Forget();
        }

        private async  UniTaskVoid DelayUpdateBottomInfo()
        {
            await UniTask.Delay(500);
            MainUiManager.Instance.OnTabTypeChangeByJumpCallback(MainUI.CurTabType);
            // if (GameDataManager.Instance.CurStageType == EStateType.StageTypeEndless ||
            //     GameDataManager.Instance.CurStageType == EStateType.StageTypeSecret || 
            //     GameDataManager.Instance.CurStageType == EStateType.StageTypeChallenge)
            // {
            //     MainUiManager.Instance.OnTabTypeChangeByJumpCallback(ETabType.TabTypeActivity);
            // }
            // else
            // {
            //     MainUiManager.Instance.OnTabTypeChangeByJumpCallback(ETabType.TabTypeMainStage);
            // }
        }

        protected override void OnDispose()
        {
            MainUI.PropertyChanged -= OnMainUiChanged;;
            DataCenter.charcaterData.Digest.PropertyChanged += OnCharcaterDataChanged;
            DataCenter.monthCardData.PropertyChanged -= MonthCardDataChanged;
            DataCenter.charcaterData.PropertyChanged += OnInviteNumberChanged;
            commonTopItemsVm.Dispose();
            bottomNodeVm.Dispose();
            equipModelVm.Dispose();
            shopModelVm.Dispose();
            roleViewVm.Dispose();
            clothesViewVm.Dispose();
            base.OnDispose();
        }

        private void OnInviteNumberChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataCenter.charcaterData.InviteNumber))
            {
                InvitedSuccessViewModel tempVm = new (DataCenter.charcaterData.InvitedChangeCount);
                UIManager.Instance.OpenDialog<InvitedSuccessView>(tempVm).Forget();
            }
        }

        private void OnCharcaterDataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataCenter.charcaterData.Digest.Name)
                || e.PropertyName == nameof(DataCenter.charcaterData.Digest.HeadFrame)
                || e.PropertyName == nameof(DataCenter.charcaterData.Digest.HeadId)
                || e.PropertyName == nameof(DataCenter.charcaterData.Digest.Exp)
                || e.PropertyName == nameof(DataCenter.charcaterData.Digest.Lv))
            {
                UpdatePlayerInfo();
            }
        }
        private void MonthCardDataChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdatePlayerInfo();
        }
        private void OnMainUiChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(MainUI.CurTabType))
            {
                
                MainUI.CheckIsPopUpDlg();
                // 根据需求去显示资源
                // UpdateTopResList();
                if (MainUI.CurTabType != ETabType.TabTypeEquip)
                {
                    EquipManager.Instance.SwitchTabClear();
                }

                bool isShop = MainUI.CurTabType == ETabType.TabTypeShop;
                List<int> list = new List<int>() { (isShop?(int)GameConst.ItemIdCode.AdFreeVouche:(int)GameConst.ItemIdCode.EnergyDrink), (int)GameConst.ItemIdCode.Money, (int)GameConst.ItemIdCode.Stone};
                if (CommonTopItemsVm == null)
                {
                    CommonTopItemsVm = new CommonTopViewModel(list, OnClickResItem);
                }
                else
                {
                    CommonTopItemsVm.UpdateResList(list);
                }
                
                DataCenter.clothesData.SaveClothesRedInfo();
            }
        }
        private void OnClickTabBtn(ETabType type)
        {
            if (!BottomNodeVm.GetTabBtnIsUnLock(type))
            {
                // 提示金币不够
                var str = BottomNodeVm.GetLockTabBtnTips(type);
                ToastManager.Show(str);
                return;
            }
            MainUiManager.Instance.CurTabType = type;
            DHLog.Debug($"点击了tab按钮 tabType is {type}");;
        }
        private void UpdateTopResList()
        {
            List<int> list = new List<int>();
            commonTopItemsVm.UpdateResList(list);
        }

        private void UpdatePlayerInfo()
        {
            CommonHeadData headData = new(DataCenter.charcaterData.Digest.HeadId, DataCenter.charcaterData.Digest.HeadFrame,OnClickHead);
            if (HeadVm == null)
            {
                HeadVm = new CommonHeadItemViewModel(headData, true,true);
            }
            else
            {
                HeadVm.UpdatePanel(headData);
            }
            PlayerLevelStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Shop_tips01,DataCenter.charcaterData.Digest.Lv);
            
            var cfg = ConfigCenter.ProLevelCfgColl.GetDataById(DataCenter.charcaterData.Digest.Lv);
            if (cfg == null)
            {
                PlayerExpValue = 0;
            }
            else
            {
                var value = DataCenter.charcaterData.Digest.Exp / (float)cfg.Exp;
                PlayerExpValue = 1 - value;
            }
            if (CommonPlayerNameVm ==null)
            {
                CommonPlayerNameVm = new CommonPlayerNameViewModel(UIHelper.GetStringByLength(DataCenter.charcaterData.Digest.Name),UIHelper.HexColorStrToColor("#EBD8CC"), 
                    DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.GoldenNickname));
            }
            else
            {
                CommonPlayerNameVm.InitUI(UIHelper.GetStringByLength(DataCenter.charcaterData.Digest.Name),UIHelper.HexColorStrToColor("#EBD8CC"),
                    DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.GoldenNickname));
            }
        }

        private void OnClickHead()
        {
            UIManager.Instance.OpenDialog<PlayerInfoView>(new PlayerInfoViewModel()).Forget();
        }
        private void  OnClickResItem(ResourceData data, int tag)
        {
            // 点击资源
            DHLog.Debug($"点击了资源 itemId is {data.Id}");
        }

    }
}