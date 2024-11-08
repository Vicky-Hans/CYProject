using System;
using System.Collections.Generic;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Commands;
using DH.UIFramework.Observables;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public enum TriggerGiftViewShowType
    {
        Gift,
        DayGift,
        WeekGift,
        MonthGift,
    }

    public partial class TriggerGiftViewModel : ViewModelBase
    {
        public override bool AutoDispose => true;
        [AutoNotify] private int showType;
        [AutoNotify] private bool isWeek;
        [AutoNotify] private ObservableList<TriggerGiftItemViewModel> triggerGiftList = new();
        [AutoNotify] private ObservableList<TriggerGiftTypeShowItemViewModel> weekGiftList = new();
        private float interval;
        [AutoNotify] private CommonTopViewModel commonTopViewModel;
        [AutoNotify] private TriggerGiftViewShowType bShowType;
        [AutoNotify] private string weekTimeDes;
        [AutoNotify] private string monthTimeDes;
        [AutoNotify] private string dayTimeDes;
        [Preserve]
        public TriggerGiftViewModel()
        {
            List<GameConst.ItemIdCode> list = new(){GameConst.ItemIdCode.EnergyDrink,GameConst.ItemIdCode.Money,GameConst.ItemIdCode.Stone};
            commonTopViewModel = new CommonTopViewModel(list);
            InitBotScrollUI();
            InitTriggerGiftList();
            InitProgressGiftScrollview();
            TimesDes();
            // var typeList = TriggerGiftManager.Instance.GetSatisfyConditionsTypeList();
            // if (typeList.Count > 0)
            // {
            //     ShowType = typeList[0];
            //     InitTriggerGiftList();
            // }

            DataCenter.triggerGiftData.PropertyChanged += DataPropertyChanged;
            PlayerInfoManager.Instance.PropertyChanged += DataPropertyChanged;
            TriggerGiftManager.Instance.ClearTriggerGiftRed();
            clickIconBtn = new SimpleCommand<Tuple<Vector3, Vector3>>(OnClickInfoBtn);
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            DataCenter.triggerGiftData.PropertyChanged -= DataPropertyChanged;
            PlayerInfoManager.Instance.PropertyChanged -= DataPropertyChanged;
        }

        private void DataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(DataCenter.triggerGiftData.Data) or nameof(PlayerInfoManager.Instance.SecondDay))
            {
                InitBotScrollUI(false);
                InitTriggerGiftList();
                InitProgressGiftScrollview();
            }

            if (e.PropertyName is nameof(DataCenter.triggerGiftData.BuyDay))
            {
                InitProgressGiftScrollview();
            }
        }
        private void InitTriggerGiftList()
        {
            triggerGiftList.ClearAndDispose();
            
            bool week = BShowType == TriggerGiftViewShowType.WeekGift;
            List<TriggerGiftCfg> cfgList = new ();
            switch (BShowType)
            {
                case TriggerGiftViewShowType.Gift:
                    cfgList = TriggerGiftManager.Instance.GetSatisfyConditionsList();
                    break;
                case TriggerGiftViewShowType.DayGift:
                    cfgList = TriggerGiftManager.Instance.GetDaySatisfyConditionsList();
                    break;
                case TriggerGiftViewShowType.WeekGift:
                    cfgList = TriggerGiftManager.Instance.GetWeekSatisfyConditionsList();
                    break;
                case TriggerGiftViewShowType.MonthGift:
                    cfgList = TriggerGiftManager.Instance.GetMonthSatisfyConditionsList();
                    break;
            }

            foreach (var item in cfgList)
            {
                triggerGiftList.Add(new TriggerGiftItemViewModel(item));
            }
            
            UIHelper.SortList(triggerGiftList, (itemA, itemB) =>
            {
                if (itemA.Cfg.Sorting == itemB.Cfg.Sorting)
                {
                    return itemA.Cfg.Id < itemB.Cfg.Id;
                }

                return itemA.Cfg.Sorting > itemB.Cfg.Sorting;
            });
        }

        public void InitBotScrollUI(bool isFirst = true)
        {
            
            // IsWeek =TriggerGiftManager.Instance.GetWeekSatisfyConditionsList().Count > 0;
            // var isDay = TriggerGiftManager.Instance.GetDaySatisfyConditionsList().Count > 0;
             var isGift = TriggerGiftManager.Instance.GetSatisfyConditionsList(showType).Count > 0;
            // var isMothGift = TriggerGiftManager.Instance.GetMonthSatisfyConditionsList().Count > 0;
            List<int> typeList = new ();
            if (isGift)
            {
                typeList.Add((int)TriggerGiftViewShowType.Gift);
            }
             // if (isDay)
            typeList.Add((int)TriggerGiftViewShowType.DayGift);
            //if (IsWeek)
            typeList.Add((int)TriggerGiftViewShowType.WeekGift);
            //if (isMothGift)
            typeList.Add((int)TriggerGiftViewShowType.MonthGift);

            if (typeList.Count == 0) return;
            
            if (isFirst)
            {
                BShowType = (TriggerGiftViewShowType)typeList[0];
            }
            else
            {
                if (!typeList.Contains((int)BShowType))
                {
                    BShowType = (TriggerGiftViewShowType)typeList[0];
                }
            }
            
            weekGiftList.Clear();

            for (int i = 0; i < typeList.Count; i++)
            {
                WeekGiftList.Add(new TriggerGiftTypeShowItemViewModel(typeList[i],BShowType == (TriggerGiftViewShowType)typeList[i],SelectType));
            }
        }

        [Command]
        private void OnClickClose()
        {
            UIManager.Instance.CloseDialog<TriggerGiftView>();
        }
        public void TimesDes()
        {
            RefreshTimeEndShow().Forget();
            WeekTimeDes = TriggerGiftManager.Instance.GetWeekTriggerGiftTime();
            MonthTimeDes = TriggerGiftManager.Instance.GetMonthTriggerGiftTime();
            DayTimeDes = TriggerGiftManager.Instance.GetDayTriggerGiftTime();
            TimeCountdown =LocalizeHelper.GetGlobal(GlobalLanguageId.Trigger13)+ TriggerGiftManager.Instance.GetProgressTime();
        }
        public override void Update()
        {
            if (UIHelper.CalculateTime(ref interval))
            {
                TimesDes();
            }
            
        }

        private async UniTaskVoid RefreshTimeEndShow()
        {
            bool isRemove = false;
            foreach (var itemModel in triggerGiftList)
            {
                if (itemModel.IsEndShow)
                {
                    itemModel.IsOpen = false;
                    isRemove = true;
                }
            }

            if (isRemove)
            {
                await UniTask.Delay(500);
                InitBotScrollUI(false);
                InitTriggerGiftList(); 
            }
        }
        
        
        public void SelectType(int id)
        {
            if (BShowType ==(TriggerGiftViewShowType)id)return;
            BShowType = (TriggerGiftViewShowType)id;
            foreach (var item in WeekGiftList)
            {
                item.IsSelect = item.ShowType == id;
            }

            InitTriggerGiftList();
        }


        #region 累充天数奖励
        [AutoNotify] private ObservableList<TriggerGiftProgressItemViewModel> progressGiftList = new();
        [AutoNotify] private string progressText;
        [AutoNotify] private string timeCountdown;
        [AutoNotify] private int  progressGiftNowIndex;
        private readonly SimpleCommand<Tuple<Vector3, Vector3>> clickIconBtn;
        public ICommand OnClickIconBtn => clickIconBtn;
        private void InitProgressGiftScrollview()
        {
            ProgressGiftNowIndex = DataCenter.triggerGiftData.GetProgressGiftNowIndex();
            ProgressText = DataCenter.triggerGiftData.BuyDay.ToString();
            var configItems = ConfigCenter.StageRewardCfgColl.GetDataByType((int)ActivityStageType.TriggerGiftProgress);
            progressGiftList.Clear();
            for (int i = 0; i < configItems.Count; i++)
            {
                progressGiftList.Add(new TriggerGiftProgressItemViewModel(configItems[i]));
            }
        }   
        
        public void OnClickInfoBtn(Tuple<Vector3, Vector3> info)
        {

            UIHelper.OpenCommonTips (LocalizeHelper.GetGlobal(GlobalLanguageId.GiftCode_tips03),
                LocalizeHelper.GetGlobal(GlobalLanguageId.Trigger14),info.Item1,info.Item2);
        }

        #endregion
    }
}