using System;
using System.ComponentModel;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class TriggerGiftItemViewModel : ViewModelBase
    {
        [AutoNotify] private TriggerGiftCfg cfg;
        [AutoNotify] private BtnPriceNodeModel btnPriceNodeModel;
        [AutoNotify] private string nameStr;
        [AutoNotify] private ObservableList<SelectCellItemViewModel> scrollViewItemList = new();
        [AutoNotify] private bool isOpen;
        [AutoNotify] private string discountValueStr;
        [AutoNotify] private string titleIconPath;
        [AutoNotify] private bool isFree;
        [AutoNotify] private string weekLimitNumsDes;
        [AutoNotify] private string monthLimitNumsDes;
        [AutoNotify] private string dayLimitNumsDes;
        [AutoNotify] private string endTimeDesc;
        [AutoNotify] private bool isEndShow;
        [AutoNotify] private Vector2 titleIconSize;
        [AutoNotify] private Vector3 titleIconPosition;
        [AutoNotify] private string titleBgPath;
        private float interval;
        public Action BuySucceed;
        [AutoNotify] private bool isWeek;
        [AutoNotify] private bool isMonth;
        [AutoNotify] private bool isDay;
        private TriggerGiftData Data => DataCenter.triggerGiftData;
        
        [Preserve]
        public TriggerGiftItemViewModel(TriggerGiftCfg cfg,Action buySucceed = null)
        {
            BuySucceed = buySucceed;
            IsOpen = true;
            Cfg = cfg;
            NameStr = TriggerGiftManager.Instance.GetTriggerGiftName(Cfg.Id);
            BtnPriceNodeModel = new BtnPriceNodeModel(Cfg.Package);
            DiscountValueStr =  ShopManager.Instance.GetPackageDiscountDesc(Cfg.Package);
            IsFree = Cfg.Package == -1;
            WeekLimitNumsDes = LocalizeHelper.GetGlobal(GlobalLanguageId.Shop_tips18,cfg.BuyLimit-DataCenter.triggerGiftData.BuyTriggerGiftNums(Cfg.Type,Cfg.Id),cfg.BuyLimit);
            MonthLimitNumsDes = LocalizeHelper.GetGlobal(GlobalLanguageId.Shop_tips18,cfg.BuyLimit-DataCenter.triggerGiftData.BuyTriggerGiftNums(Cfg.Type,Cfg.Id),cfg.BuyLimit);
            dayLimitNumsDes = LocalizeHelper.GetGlobal(GlobalLanguageId.Shop_tips18,cfg.BuyLimit-DataCenter.triggerGiftData.BuyTriggerGiftNums(Cfg.Type,Cfg.Id),cfg.BuyLimit);
            InitRewardList();
            RefreshTimeDesc();
            IsWeek = ConfigCenter.TriggerGiftTypeCfgColl.GetDataById(Cfg.Type).Trigger == 3;
            IsMonth = ConfigCenter.TriggerGiftTypeCfgColl.GetDataById(Cfg.Type).Trigger == 4;
            IsDay = ConfigCenter.TriggerGiftTypeCfgColl.GetDataById(Cfg.Type).Trigger == 6;
            InitTitleIcon();
            Data.PropertyChanged += OptionalChange;
        }

        private void InitTitleIcon()
        {
            TitleIconPath = TriggerGiftManager.Instance.GetTriggerTitleIcon(Cfg.Id);
            TitleBgPath = TriggerGiftManager.Instance.GetTriggerTitleBgPath(Cfg.Id);
            if (cfg != null)
            {
                var cfgType = ConfigCenter.TriggerGiftTypeCfgColl.GetDataById(cfg.Type);
                if (cfgType.Trigger == 1)
                {
                    TitleIconSize = new Vector2(380,380);
                    TitleIconPosition = new Vector2(-352, -5);
                }else if (cfgType.Trigger == 2)
                {
                    TitleIconSize = new Vector2(352,465);
                    TitleIconPosition = new Vector2(-352, -5);
                }
            }
        }

        private void InitRewardList()
        {
            ScrollViewItemList.Clear();
            if (Cfg.Reward is { Count: > 0 })
            {
                for (int i = 0; i<Cfg.Reward.Count ; i++)
                {
                    var reward = Cfg.Reward[i];
                    var selectItemViewModel = SelectCellItemViewModel.Create(reward);
                    selectItemViewModel.CellItemBaseViewVm.IsOpenMask = true;
                    ScrollViewItemList.Add(selectItemViewModel);
                }
            }

            if (Cfg.OptionalReward is { Count: > 0 })
            {
                var selectItemViewModel = SelectCellItemViewModel.Create(Cfg.OptionalReward,selectIndex:Data.GetSelectPacket(cfg.Id));
                selectItemViewModel.ClickEvent = OnClickSelectReward;
                selectItemViewModel.CellItemBaseViewVm.IsOpenMask = true;
                ScrollViewItemList.Add(selectItemViewModel);
            }
        }


        [Command]
        private void OnClickBtnBuy()
        {
            if (cfg.Package == -1)
            {
                TriggerGiftManager.Instance.SendBuyTriggerGift(Cfg.Id, (id) =>
                {
                    RefreshList(id).Forget();
                }).Forget();
            }
            else
            {
                if (Cfg.OptionalReward != null && Cfg.OptionalReward.Count >0 && Data.GetSelectPacket(cfg.Id) == -1)
                {
                    ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.Trigger08));
                    return;
                }
                ShopManager.Instance.SendBuyPackageBuyRecharge(cfg.Package, (packId) =>
                {
                    RefreshList(cfg.Id).Forget();
                    TriggerGiftManager.Instance.PayProgress();
                },rewardIndex:Data.GetSelectPacket(cfg.Id));
            }
        }

        private async UniTaskVoid RefreshList(int id)
        {
            BuySucceed?.Invoke();
            IsOpen = DataCenter.triggerGiftData.IsBuyTriggerGift(cfg.Type,id);
            await UniTask.Delay(500);
            DataCenter.triggerGiftData.BuyTriggerGift(id);
        }

   

        private void RefreshTimeDesc()
        {
            if (IsWeek ||IsDay || IsMonth)return;
            var endTime = DataCenter.triggerGiftData.GetTriggerGiftEndTime(Cfg.Type);
            var time = ServerTime.Instance.RemainTime(endTime);
            if (time < 0) time = 0;
            IsEndShow = time == 0;
            
            EndTimeDesc = ServerTime.Instance.ConvertSecondsToTime(time);
        }

        public override void Update()
        {
            if (UIHelper.CalculateTime(ref interval))
            {
                RefreshTimeDesc();
            }
        }
        
        private void OnClickSelectReward()  
        {
            if(Cfg==null || Cfg.OptionalReward ==null || Cfg.OptionalReward.Count==0) return;
            UIManager.Instance.OpenDialog<CommonSelectRewardView>(new CommonSelectRewardViewModel(cfg.OptionalReward,Data.GetSelectPacket(cfg.Id)
                ,(selectIndex)=> {
                    Data.SetSelectPacket(cfg.Id,selectIndex);
                })).Forget();
        }

        private void OptionalChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Data.OptionalRecord))
            {
                InitRewardList();
            }
        }
        
        protected override void OnDispose()
        {
            Data.PropertyChanged -= OptionalChange;
            base.OnDispose();
        }
    }
}