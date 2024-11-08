using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DH.Config;
using DH.Data;
using DH.Game;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Game.ViewModels;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;

namespace Game.UI.MainUi
{
    public partial class MainUiManager: DH.Game.ObservableSingleton<MainUiManager>
    {

        /// <summary>
        /// 当前选择tab类型
        /// </summary>
        private ETabType curTabType = ETabType.TabTypeMainStage;
        public ETabType CurTabType
        {
            get => curTabType;
            set
            {
                if (curTabType == value) return;
                Set(ref curTabType, value);
                OnTabTypeChangeByJump(value);
            }
        }
        
        [AutoNotify] private int showChapterIndex;
        /// <summary>
        /// 当前tab类型改变回调
        /// </summary>
        [AutoNotify] private Action<ETabType> onTabTypeChangeByJumpCallback; 
        /// <summary>
        /// 当前选择tab类型
        /// </summary>
        private ERankType curRankType = ERankType.RankItemMainStage;
        public ERankType CurRankType
        {
            get => curRankType;
            set
            {
                if (curRankType == value) return;
                Set(ref curRankType, value);
            }
        }
        
        public void Init()
        {
            CurTabType = ETabType.TabTypeMainStage;
            UpdateShowIndex();
            DataCenter.itemsData.ResourceDatas[(int)GameConst.ItemIdCode.AdFreeVouche].PropertyChanged += AdFreeVoucheChange;
            DataCenter.livesData.RequestLivesCallback = RequestSyncLives;
        }

        public string DaysSubscribeGift => DataCenter.charcaterData.Digest.RoleId + "ShowAdFreeGift";//广告卷礼包
        private void AdFreeVoucheChange(object sender, PropertyChangedEventArgs e)
        {
            if (DataCenter.itemsData.GetItemCount((int)GameConst.ItemIdCode.AdFreeVouche)==0 &&
                TriggerGiftManager.Instance.GiftIsBuy(1102)  &&
                DHUnityUtil.PlayerPrefs.GetInt(DaysSubscribeGift)==0)
            {
                   UIManager.Instance.OpenDialog<AdFreeGiftView>(new AdFreeGiftViewModel()).Forget();
                   DHUnityUtil.PlayerPrefs.SetInt(DaysSubscribeGift, 1);
            }
        }

        public void UpdateShowIndex()
        {
            for (int i = 0; i < DataCenter.mainStageData.ChapterCfgs.Count; ++i)
            {
                var cfg = DataCenter.mainStageData.ChapterCfgs[i];
                if (cfg.Id == DataCenter.mainStageData.CurrChapter)
                {
                    ShowChapterIndex = i;
                    break;
                }
            }
        }

        public async void RequestSyncLives()
        {
            var sData = new ReqLivesSync();
            var result = await GameNetworkManager.Instance.SendAsync<RspLivesSync>(sData);
            if (result.rsp == null || result.rsp.Status != 0)
            {
                DataCenter.livesData.UpdateMaxLives();
                DHLog.Debug(" 请求同步数据失败");
                return;
            }
            // 当前体力
            DataCenter.livesData.Curr = result.rsp.Curr;
            // 倒计时开始的时间挫
            DataCenter.livesData.RefreshStamp = result.rsp.RefreshStamp;
            DataCenter.livesData.UpdateMaxLives();
        }

        /// <summary>
        ///  通关 下标 获取章节id 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetChapterId(int index)
        {
            if(index < 0 || index >= DataCenter.mainStageData.ChapterCfgs.Count)
            {
                DHLog.Error("获取章节id 错误 请检查 showIndex");
                return -1;
            }
            return DataCenter.mainStageData.ChapterCfgs[index].Id;
        }

        public string GetFunctionUnLockTipsByMainUI(ETabType type)
        {
            EFunctionOpenType functionOpenType;
            switch (type)
            {
                case ETabType.TabTypeShop:
                    functionOpenType = EFunctionOpenType.FunctionTypeShop;
                    break;
                case ETabType.TabTypeEquip:
                    functionOpenType = EFunctionOpenType.FunctionTypeEquip;
                    break;
                case ETabType.TabTypeActivity:
                    functionOpenType = EFunctionOpenType.FunctionEndless;
                    break;
                case ETabType.Role:
                    functionOpenType = EFunctionOpenType.FunctionRole;
                    break;
                // case ETabType.TabTypeChallengeChapter:
                //     functionOpenType = EFunctionOpenType.FunctionTypeChallengeStage;
                //     break;
                default:
                    functionOpenType = EFunctionOpenType.FunctionTypeChallengeStage;
                    break;
            }
            
            return GetFunctionUnLockTips(functionOpenType);
        }

        public async UniTask RequestEnterGame( int chapterId)
        {
            ReqMainFightBegin data = new ReqMainFightBegin();
            data.ChapterId = chapterId;
            var result = await GameNetworkManager.Instance.SendAsync<RspMainFightBegin>(data);
            if (result.rsp ==null ||result.rsp.Status != 0)
            {
                DHLog.Debug(" 请求进入游戏失败");
                if(result.rsp == null) return;
                var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
                ToastManager.Show(str);
                // 这里给提示
                return;
            }
            GameManager.Instance.EnterGame(EStateType.StageTypeMainStage, chapterId, result.rsp.Data);
            // 这里扣资源
            Lodash.DealRewards(result.rsp.Cost.ToList(), false);
        }

        public string GetFunctionUnLockTips(EFunctionOpenType type)
        {
            return GetFunctionUnLockTips((int)type);
        }
        

        public string GetFunctionUnLockTips(int id)
        {
            //检查挑战是否解锁
            var functionOpenCfg = ConfigCenter.FunctionOpenCfgColl.GetDataById(id);
            if (functionOpenCfg == null)
            {
                DHLog.Error($"没有配置表， FunctionOpenCfg 请检查配置  {id}");
                return "";
            }
            if (functionOpenCfg.Level != 0)
            {
                string retStr = LocalizeHelper.GetGlobal(GlobalLanguageId.FunctionOpen_tips_3,functionOpenCfg.Level);
                return retStr;
            }
            if (functionOpenCfg.Stage != 0 && functionOpenCfg.Star == 0)
            {
                string retStr = LocalizeHelper.GetGlobal(GlobalLanguageId.FunctionOpen_tips_2, functionOpenCfg.Stage);
                return retStr;
            }
            if (functionOpenCfg.Stage != 0 && functionOpenCfg.Star != 0)
            {
                // string retStr = LocalizeHelper.GetGlobal(GlobalLanguageId.functionOpen_tips_4,functionOpenCfg.Stage, functionOpenCfg.Star);
                return "";
            }
            return "";
        }
        /// <summary>
        /// 检查功能是否解锁
        /// </summary>
        /// <param name="tabType"></param>
        /// <returns></returns>
        public bool CheckFunctionIsUnlockByMainUITab(int tabType)
        {
            EFunctionOpenType functionType;
            switch ((ETabType)tabType)
            {
                case ETabType.TabTypeShop:
                    functionType = EFunctionOpenType.FunctionTypeShop;
                    break;
                case ETabType.TabTypeEquip:
                    functionType = EFunctionOpenType.FunctionTypeEquip;
                    break;
                case ETabType.TabTypeActivity:
                    functionType = EFunctionOpenType.FunctionEndless;
                    break;
                // case ETabType.TabTypeSearch:
                //     functionType = EFunctionOpenType.FunctionTypeSearch;
                //     break;
                case ETabType.TabTypeMainStage:
                    functionType = EFunctionOpenType.FunctionTypeMainStage;
                    break;
                case ETabType.Role:
                    functionType = EFunctionOpenType.FunctionRole;
                    break;
                default:
                {
                    DHLog.Error($"muzili log  not support type {tabType}");
                    return true;
                }
            }

            return CheckFunctionIsUnlock(functionType);
        }

        public bool CheckFunctionIsUnlock(EFunctionOpenType functionType)
        {
            return CheckFunctionIsUnlock((int)functionType);
        }

        /// <summary>
        /// 检查功能是否解锁
        /// </summary>
        /// <param name="tabType"></param>
        /// <returns></returns>
        public bool CheckFunctionIsUnlock(int functionId)
        {
            var cfg = ConfigCenter.FunctionOpenCfgColl.GetDataById(functionId);
            if (cfg == null)
            {
                DHLog.Warning($"muzili log  not support type {(EFunctionOpenType)functionId}, id is  {functionId} ");
                return true;
            }

            var starCount = 0;
            var chapter = cfg.Stage;
            if (chapter == 0)
            {
                starCount = 0;
            }
            else
            {
                var chapterInfo = DataCenter.mainStageData.GetChapterInfo(chapter);
                if (chapterInfo == null)
                {
                    starCount = 0;
                }
                else
                {
                    starCount = chapterInfo.Star;
                }
            }

            if (DataCenter.charcaterData.Digest.Lv < cfg.Level) return false;
            if (DataCenter.mainStageData.CurrChapter <= chapter) return false;
            if (starCount < cfg.Star) return false;

            return true;
        }
        
        
        private Tween delayAction = null;
        /// <summary>
        /// 检查是否弹出弹窗
        /// </summary>
        public void CheckIsPopUpDlg()
        {
            if (!UIManager.Instance.CheckIsTopDlg<MainUiView>())
            {
                return;
            }

            if (curTabType != ETabType.TabTypeMainStage)
            {
                return;
            }
            // 检查弹窗
            if (delayAction != null)
            {
                delayAction.Kill();
            }
            float delayTime = 0.2f;
            delayAction = DOVirtual.DelayedCall(delayTime, CheckIsShowPopUp);
        }
        private void CheckIsShowPopUp()
        {
            delayAction = null;
            if (!UIManager.Instance.CheckIsTopDlg<MainUiView>())
                return;
            if(curTabType != ETabType.TabTypeMainStage)
                return;
            PopUpManager.Instance.CheckAndPopUpView();
        }
        /// <summary>
        /// 跳转用于改变tab
        /// </summary>
        /// <param name="tabType"></param>
        public void OnTabTypeChangeByJump(ETabType tabType)
        {
            onTabTypeChangeByJumpCallback?.Invoke(tabType);
        }

        /// <summary>
        /// 跳转用于改变Chapter的显示章节
        /// </summary>
        public void OnJumpCanFrameChapter()
        {
            // 检查功能开启没有
            var isUnlock = CheckFunctionIsUnlock(EFunctionOpenType.FunctioonTypeFarm);
            if (!isUnlock)
            {
                return;
            }
            
            if(ShowChapterIndex == 0) return;

            var chapterId = GetChapterId(ShowChapterIndex - 1);
            var chapterInfo = DataCenter.mainStageData.GetChapterInfo(chapterId);
            if (chapterInfo is not { Star: > 2 })
            {
                return;
            }
            ShowChapterIndex -= 1;
        }

        public string GetFunctionName(EFunctionOpenType functionOpenType)
        {
            var cfg = ConfigCenter.FunctionOpenLanguageCfgColl.GetDataById((int)functionOpenType);
            return cfg?.Name ?? string.Empty;
        }

        #region 主界面右侧按钮栏

        public ObservableList<CommonButViewModel> RightButItems = new();

        public void AddRightBut(MainStageInfoNodeRightButType mType, string mIconPath,string name,Action<List<object>> onClick,Func<bool> showRed = null,long time = 0)
        {
            if (RightButItems.Any(item => item.Type == mType))
            {
                // DHLog.Debug("存在相同类型的按钮");
                return;
            }

            int index = 0;
            int newData =  (int)mType;
            bool isMax = true;
            for (int i = 0; i < RightButItems.Count; i++)
            {
                if (newData < (int)RightButItems[i].Type)
                {
                    index = i;
                    isMax = false;
                    break;
                }
            }

            if (isMax)
            {
                RightButItems.Add(new CommonButViewModel(mType,mIconPath,name,onClick,showRed,time));
            }
            else
            {
                RightButItems.Insert(index, new CommonButViewModel(mType,mIconPath,name,onClick,showRed,time));
            }

        }
        public void RemoveRightBut(MainStageInfoNodeRightButType mType)
        {
            if (RightButItems.All(item => item.Type != mType))
            {
                return;
            }
            RightButItems.RemoveAll(item => item.Type == mType);
        }

        #endregion
        
        
        #region 主界面左侧按钮栏

        public ObservableList<CommonButViewModel> LeftButItems = new();

        public void AddLeftBut(MainStageInfoNodeRightButType mType, string mIconPath,string name,Action<List<object>> onClick,Func<bool> showRed = null,long time = 0)
        {
            if (LeftButItems.Any(item => item.Type == mType))
            {
                return;
            }

            int index = 0;
            int newData =  (int)mType;
            bool isMax = true;
            for (int i = 0; i < LeftButItems.Count; i++)
            {
                if (newData < (int)LeftButItems[i].Type)
                {
                    index = i;
                    isMax = false;
                    break;
                }
            }

            if (isMax)
            {
                LeftButItems.Add(new CommonButViewModel(mType,mIconPath,name,onClick,showRed,time));
            }
            else
            {
                LeftButItems.Insert(index, new CommonButViewModel(mType,mIconPath,name,onClick,showRed,time));
            }

        }
        public void RemoveLeftBut(MainStageInfoNodeRightButType mType)
        {
            if (LeftButItems.All(item => item.Type != mType))
            {
                return;
            }
            LeftButItems.RemoveAll(item => item.Type == mType);
        }

        #endregion
        
        #region 主界面左侧顶部按钮栏

        public ObservableList<CommonButViewModel> LeftTopButItems = new();

        public void AddLeftTopBut(MainStageInfoNodeRightButType mType, string mIconPath,string name,Action<List<object>> onClick,Func<bool> showRed = null,long time = 0)
        {
            if (LeftTopButItems.Any(item => item.Type == mType))
            {
                return;
            }

            int index = 0;
            int newData =  (int)mType;
            bool isMax = true;
            for (int i = 0; i < LeftTopButItems.Count; i++)
            {
                if (newData < (int)LeftTopButItems[i].Type)
                {
                    index = i;
                    isMax = false;
                    break;
                }
            }

            if (isMax)
            {
                LeftTopButItems.Add(new CommonButViewModel(mType,mIconPath,name,onClick,showRed,time));
            }
            else
            {
                LeftTopButItems.Insert(index, new CommonButViewModel(mType,mIconPath,name,onClick,showRed,time));
            }

        }
        public void RemoveLeftTopBut(MainStageInfoNodeRightButType mType)
        {
            if (LeftTopButItems.All(item => item.Type != mType))
            {
                return;
            }
            LeftTopButItems.RemoveAll(item => item.Type == mType);
        }

        #endregion

        #region 头像

        public class HeadCompare : IComparer
        {
            public static readonly HeadCompare Default = new();
            public int Compare(object x, object y)
            {
                if (!(x is KeyValuePair<int, HeadItemViewModel> item1) ||
                    !(y is KeyValuePair<int, HeadItemViewModel> item2))
                    return 0;
            
                var itemRank1 = item1.Value.IsLock ? 99999 + item1.Value.Cfg.Id : item1.Value.Id;
                var itemRank2 = item2.Value.IsLock ? 99999 + item1.Value.Id : item1.Value.Id;
                return itemRank1.CompareTo(itemRank2);
            }
        }

        #endregion

        #region 后台切前台刷新邮件
        
        public async void UpDataMailAndDiscord()
        {
            if (DataCenter.charcaterData.DiscordFlag == 1)return;
            var oldFlag = DataCenter.charcaterData.DiscordFlag;

            var req = new ReqDiscordSync();
            var result =await GameNetworkManager.Instance.SendAsync<RspDiscordSync>(req); 
            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                if (oldFlag != result.rsp.DiscordFlag)
                    UpDataMail();
            }

        }
        public async void UpDataMail()
        {
            var req = new ReqMailSync();
            var result = await GameNetworkManager.Instance.SendAsync<RspMailSync>(req); 
            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                if (DataCenter.maildata != null)
                    DataCenter.maildata.MergeFrom(result.rsp.Mails,true);
            }
        }
        #endregion

        #region 新秀榜
        /// <summary>
        /// 新秀时期 创号后7天内
        /// </summary>
        /// <returns></returns>
        public bool IsNewcomer()
        {
            return ServerTime.Instance.GetTimeDay(DataCenter.charcaterData.RoleCreateTime)<7;
        }
        /// <summary>
        /// 剩余多久进入全球榜
        /// </summary>
        /// <returns></returns>
        public long NewcomerTimes()
        {        
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(DataCenter.charcaterData.RoleCreateTime);
            DateTime dateTime = dateTimeOffset.LocalDateTime;
        
            // 计算到下一天凌晨零点的时间戳
            DateTime nextDay = dateTime.AddDays(7);
            DateTime nextMidnight = new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, 0, 0, 0);
            long nextMidnightTimestamp = (long)(nextMidnight - Lodash.GetOriginDateTime()).TotalSeconds;
            Console.WriteLine("下一天凌晨零点的时间戳: " + nextMidnightTimestamp);
            return Math.Max(0,nextMidnightTimestamp - ServerTime.Instance.GetNowTime());
        }

        #endregion
    }
}