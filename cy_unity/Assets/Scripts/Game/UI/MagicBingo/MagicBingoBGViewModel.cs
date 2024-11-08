using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DH.Config;
using DH.Data;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public partial class MagicBingoBGViewModel : ViewModelBase
    {
        public override bool AutoDispose => true;
        [AutoNotify] private CommonTopViewModel commonTopItemsVm;
        [AutoNotify] private ObservableList<MagicBingoItemViewModel> scrollViewList = new();
        [AutoNotify] private string timeDes;
        
        [AutoNotify] private string bingoPointText;
        [AutoNotify] private string toDayBingoNums;
        [AutoNotify] private float progressValue;
        [AutoNotify] private int bingoCount;
        public MagicBingoData Data => DataCenter.mgicBingoData;
        public ActivityUIManager Manager => ActivityUIManager.Instance;
        [Preserve]
        public MagicBingoBGViewModel()
        {
            BingoCount = -1;
            InitUI();
            InitScrollView();
            InitTopItems();
            RefreshTimeDesc();
            InitProgressAward();
            if (DataCenter.itemsData.ResourceDatas.TryGetValue((int)GameConst.ItemIdCode.BinGoCoin, out var data))
            {
                data.PropertyChanged += BinGoPointChangeEvent;
            }

            Data.PropertyChanged += DataPropertyEvent;
        }
        protected override void OnDispose()
        {
            if (DataCenter.itemsData.ResourceDatas.TryGetValue((int)GameConst.ItemIdCode.BinGoCoin, out var data))
            {
                data.PropertyChanged -= BinGoPointChangeEvent;
            }
            Data.PropertyChanged -= DataPropertyEvent;
            base.OnDispose();
        }
        private void InitTopItems()
        {
            CommonTopItemsVm = new CommonTopViewModel(new List<GameConst.ItemIdCode>
            {
                GameConst.ItemIdCode.BinGoPoint,
                GameConst.ItemIdCode.Stone,
            });	     
        }
        private void InitUI()
        {
            BingoPointText = DataCenter.itemsData.GetItemCount((int)GameConst.ItemIdCode.BinGoCoin).ToString();
            var BingoMaxNums = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Bingo_01).Content[0];
            ToDayBingoNums = $"{BingoMaxNums - Data.Grid.Count}/{BingoMaxNums}";
            
            var configItems = ConfigCenter.StageRewardCfgColl.GetDataByType((int)ActivityStageType.MagicBingo);
            ProgressValue = Data.Grid.BingoCount* 1f / configItems.Count * 1f;
            IsShowRestBut = BingoMaxNums == Data.Grid.Count;
        }

        private void InitScrollView()
        {
            ScrollViewList.Clear();
            int Row = 1;
            int Column = 0;
            for (int i = 0; i < 25; i++)
            {
                Column++;
                if (Column > 5)
                {
                    Column = 1;
                    Row++;
                }
                ScrollViewList.Add(new MagicBingoItemViewModel(Row * 100 + Column));
            }
        }

        private void BinGoPointChangeEvent(object sender, PropertyChangedEventArgs e)
        {
            BingoPointText = DataCenter.itemsData.GetItemCount((int)GameConst.ItemIdCode.BinGoCoin).ToString();
        }
        private void DataPropertyEvent(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Data.Grid.BingoCount) )
            {
                InitUI();
                InitProgressAward();
                BingoCount = Data.Grid.BingoCount;
            }

            if (e.PropertyName == nameof(Data.Grid.Count) )
            {
                InitUI();
            }
        }
        
        #region 倒计时相关
        private float interval;
        private void RefreshTimeDesc()
        {
            var times = Math.Max(0,
                DataCenter.mgicBingoData.EndStamp - ServerTime.Instance.GetNowTime());
            TimeDes =  ServerTime.Instance.SecondsDHAndMS(times);
        }
        public override void Update()
        {
            if (UIHelper.CalculateTime(ref interval))
            {
                RefreshTimeDesc();
            }
        }
        

        #endregion
        
        #region 累计翻牌奖励

        [AutoNotify] private ObservableList<MagicBingoProgressItemViewModel> progressGiftList = new();
        [AutoNotify] private int  progressGiftNowIndex;

        private void InitProgressAward()
        {
            progressGiftList.Clear();
            var configItems = ConfigCenter.StageRewardCfgColl.GetDataByType((int)ActivityStageType.MagicBingo);
            progressGiftList.Clear();
            for (int i = 0; i < configItems.Count; i++)
            {
                progressGiftList.Add(new MagicBingoProgressItemViewModel(configItems[i]));
            }
        }
        #endregion

        #region 重置

        [AutoNotify] private bool isShowRestBut;
        [Command]
        private async void OnClickRestBtn()
        {
            var req = new ReqBingoReset();
            var result =await GameNetworkManager.Instance.SendAsync<RspBingoReset>(req);
            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                if (result.rsp.Reward.Count>0)
                {
                    Lodash.DealRewards(result.rsp.Reward.ToList());
                    UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList());                    
                }

                Data.RestData();
                InitUI();
                InitScrollView();
                InitProgressAward();
                BingoCount = 0;
            }
        }
        [Command]
        private void OnClickRuleBut()
        {
            var activityCfg  = ConfigCenter.ActivityCfgColl.GetDataById((int)EActivityType.MagicBingo);
            UIHelper.OpenCommonRuleProbability(LocalizeHelper.GetGlobal(GlobalLanguageId.Bingo_08),activityCfg?.JackpotId[0] ?? 0);
        }
        #endregion

        [Command]
        private void OnClickBuyBtn()
        {
            ActivityUIManager.Instance.OpenBuyCoin();
        }
    }
}