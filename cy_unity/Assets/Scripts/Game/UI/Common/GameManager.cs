using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.Login;
using DH.Game.UIViews;
using DH.Game.ViewModels;
using DH.Proto;
using DH.UIFramework;
using DHFramework;
using Game.UI;
using Game.UI.MainUi;
using Newtonsoft.Json;
using UnityEngine;

namespace DH.Game.UI
{
    public partial class GameManager : ObservableSingleton<GameManager>
    {

        public int CurChapterId => GameDataManager.Instance.CurChapterId;
        private int Star { get; set; }
        public int LaseDragModelId;
        [AutoNotify] private bool isWin=true;
        private SecretTalentChooseViewModel secretTalentVm;
        [AutoNotify] private Vector2Int gridSize;//棋盘当前大小
        [AutoNotify] private EBlockState blockState;//棋盘状态
        [AutoNotify] private EDragState dragState;//拖动状态
        [AutoNotify] private int dragWeaponId;//拖动武器的Id
        [AutoNotify] private long dragUid;//拖动武器的UId
        public readonly int MoneyBagEquipId = 9;//钱袋装备Id
        public readonly float CellSize = 135.0f;//格子大小
        public readonly float IconSize = 124.0f;//Icon大小
        public MainMergeViewModel mainMergeVMInstance;//合成界面对象
        public int FreeAdTipsCnt = 0;//每轮战斗弹免广告的次数（不超过3次）
        /// <summary>
        /// 游戏已走的时间
        /// </summary>
        [AutoNotify] private float costTime;
        /// <summary>
        /// 游戏总时间
        /// </summary>
        [AutoNotify] private float totalTime = 1;
        /// <summary>
        /// 游戏结算的奖励
        /// </summary>
        [AutoNotify] private List<Resource> gameRewards = new ();
        /// <summary>
        /// 经验的进度条
        /// </summary>
        [AutoNotify] private float expProgressValue;
        /// <summary>
        /// 可以选择的次数
        /// </summary>
        [AutoNotify] private int chooseCount;
        /// <summary>
        /// 是否进了战斗
        /// </summary>
        [AutoNotify] private bool isEnterFight;
        /// <summary>
        /// 复活次数
        /// </summary>
        [AutoNotify] private int reviveCount;
        /// <summary>
        /// 是否发起游戏结算
        /// </summary>
        /// <returns></returns>
        private bool isSendGameEnd;
        [AutoNotify] private bool isCanRandomTalent;
        [AutoNotify] private bool isOpenExitTip;//是否打开退出提示（每日挑战/秘林 使用）

        [AutoNotify] private List<Action> requestTalentList = new();
        
        private void Init()
        {
            reviveCount = 0;
            isSendGameEnd = false;
            TotalTime = 0;
            IsOpenExitTip = false;
            DailyFightIsBegin = false;
            BlockState = EBlockState.Normal;
            if (secretTalentVm != null)
            {
                secretTalentVm.Dispose();
                secretTalentVm = null;
            }
            var initGridInfoCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.copy_GridQuantity);
            if (initGridInfoCfg != null && initGridInfoCfg.Content.Count == 2)
            {
                GridSize = new Vector2Int(initGridInfoCfg.Content[1],initGridInfoCfg.Content[0]);
            }
            requestTalentList.Clear();
        }

        /// <summary>
        /// 是否可以开启加速
        /// </summary>
        /// <returns></returns>
        public bool CheckIsCanOpenRatio()
        {
            var defCfg = ConfigCenter.DefinesCfgColl.GetDataById(316);
            if(defCfg ==null||defCfg.Content[0] == 0)
            {
                return false;
            }
            // 可以开启加速的条件是 1 当前关卡已经过关 2 当前关卡大于 配置表的开启关卡 3 开了月卡
            if (DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.BattleAcceleration)
                || DataCenter.mainStageData.IsPassChapter(CurChapterId)
                || defCfg.Content[0] < CurChapterId)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取复活消耗
        /// </summary>
        /// <returns></returns>
        public List<Reward> GetReviveCost()
        {
            // 获取复活消耗
            var costCfg = ConfigCenter.DefinesCfgColl.GetDataById(310);
            if (costCfg == null || costCfg.Reward.Count == 0)
            {
                DHLog.Error("复活消耗配置表不存在");
                return new List<Reward>();
            }

            return costCfg.Reward;
        }

        public void OnClickSpeedBtn()
        {
            var curTime = Time.timeScale;
            if(CheckIsCanOpenRatio())
            {
                if (curTime < 1.4f)
                {
                    Time.timeScale = 1.5f;
                }
                else
                {
                    Time.timeScale = 1;
                }
            }
            else
            {
                Time.timeScale = 1;
            }
        }

        public int GetCurTimeRatio()
        {
            var curTime = Time.timeScale;
            if (curTime > 1.4f) return 2;
            return 1;
        }
        /// <summary>
        /// 判断是否免广告
        /// </summary>
        /// <returns></returns>
        public bool CheckIsVipForAd()
        {
            var isAdRreeReward = DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.AdRreeReward);
            var isADFreeForever = DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.ADFreeForever);
            return isAdRreeReward || isADFreeForever;
        }
        
        private bool showAdFreePlusAni = false;
        public bool ShowAdFreePlusAni
        {
            get => showAdFreePlusAni;
            set
            {
                if (showAdFreePlusAni == value) return;
                Set(ref showAdFreePlusAni, value);
            }
        }
        /// <summary>
        /// 检测是否弹免广告提示
        /// </summary>
        public void CheckPopFreeAdTips()
        {
            if (CheckIsVipForAd()) return;
            if (GameDataManager.Instance.CurStageType != EStateType.StageTypeMainStage) return;
            if (GameDataManager.Instance.RandomWeaponDataList.Count <= 0) return;
            foreach (var weaponData in GameDataManager.Instance.RandomWeaponDataList)
            {
                if (weaponData.WeaponAttrType == 2) continue;
                var weaponCfg = ConfigCenter.EquipModelCfgColl.GetDataById(weaponData.WeaponId/10);
                var weaponCopyCfg = ConfigCenter.CopyEquipWeightsCfgColl.GetDataById(weaponData.WeaponId/10);
                if (weaponCfg == null || weaponCfg.Type == 1) continue;
                if (weaponCopyCfg is { Adv: 1 } && weaponData.WeaponId%10 == 0) // 看广告但是未看过
                {
                    //To-Do
                    if (ShowAdFreePlusAni)return;
                    ShowAdFreePlusAni = true;
                    FreeAdTipsCnt++;
                }
            }
        }
        /// <summary>
        /// 更新复活数据
        /// </summary>
        /// <param name="fightData"></param>
        public void UpdateReviveData(FightData fightData)
        {
            GameDataManager.Instance.SetFightData(fightData);
            BattleManager.Instance.fightingManagerIns.ResetFightState();
            var maxHp = BattleManager.Instance.fightingManagerIns.playerCtrl.Data.resource.MaxHp;
            BattleManager.Instance.fightingManagerIns.playerCtrl.Player.AddHp(maxHp);
            ReviveCount++;
            GameDataManager.Instance.GameCoin = fightData.Stage.Money;
            if (fightData.Stage.Money > 0 && mainMergeVMInstance!= null && mainMergeVMInstance.GridListRect!= null)
            {
                var coinNum = 1;
                if (fightData.Stage.Money > coinNum)
                {
                    coinNum = Mathf.RoundToInt(fightData.Stage.Money * 0.5f) > 30 ? 30 : Mathf.RoundToInt(fightData.Stage.Money * 0.5f);
                }
                UIEffectManager.Instance.PlayerGameCoinAction(mainMergeVMInstance.GridListRect.transform.position,coinNum, true,null,false).Forget();
            }
            // 复活后，需要重新回到当前波次开始
            GameDataManager.Instance.OnReviveSuccess();
            GameTime.Instance.Pause = GameDataManager.Instance.WaveEnd;
        }

        public FightStat GetFightStat(bool isDebug = false)
        {
            var data = new FightStat();
            if(isDebug)
            {
                var cfg = ConfigCenter.CopyCfgColl.GetDataById(CurChapterId);
                data.Wave = IsWin ? cfg.LevelWaves : 0;
                data.Kills = IsWin ? 1 : 0;
                if (GameDataManager.Instance.CurStageType == EStateType.StageTypeEndless)
                {
                    data.Kills = PlayerStats.Instance.KillCount;
                }
                data.BossKills = IsWin ? 1 : 0;
                data.Harm = IsWin ? 1 : 0;
            }
            else
            {
                data.Wave = IsWin ? GameDataManager.Instance.Wave : GameDataManager.Instance.Wave - 1;
                data.Kills = PlayerStats.Instance.KillCount;
                data.BossKills = PlayerStats.Instance.KillBossCount;
                data.Harm = 0;
                if (GameDataManager.Instance.CurStageType == EStateType.StageTypeEndless) //无尽关卡
                {
                    if (data.Wave <= 0 && DataCenter.endlessData.Count >= 0) DataCenter.endlessData.Count += 1; //挑战次数+1
                    data.SurvivalTime = (int)CostTime;//生存时间；秒
                }
                else if (GameDataManager.Instance.CurStageType == EStateType.StageTypeChallenge)
                {
                    if (!DataCenter.dailyFightData.HasArchive && !DailyFightIsBegin) DataCenter.dailyFightData.Count += 1; //挑战次数+1
                }
            }
            return data;
        }

        public void EnterGame(EStateType type, int chapterId, SFightData data)
        {
            Init();
            GameDataManager.Instance.CurStageType = type;
            GameDataManager.Instance.CurChapterId = chapterId;
            GameDataManager.Instance.Init(data);
            PlayerStats.Instance.Init();
            UIManager.Instance.CloseAllTopDialog();
            PopUpManager.Instance.Clear();
            var cfg = ConfigCenter.CopySecretCfgColl.GetDataById(chapterId);
            if (cfg != null)
            {
                GameDataManager.Instance.InitEndlessBaseAttr(cfg.MainId);
            }
            else
            {
                GameDataManager.Instance.InitEndlessBaseAttr(chapterId);
            }
            // GameDataManager.Instance.InitEndlessBaseAttr(chapterId);
            // 进入战斗
            ProcedureManager.Instance.ChangeAsync<StartFightMenuView>(nameof(MapFightingProcedure), true).Forget();
        }

        /// <summary>
        /// 检测物理武器合成增益属性
        /// </summary>
        /// <returns></returns>
        public void CheckPhyEquipMergedGain()
        {
            DataCenter.itemsData.ChangeItemsCount((int)GameConst.ItemIdCode.GameCoin, GameDataManager.Instance.PhyEquipMergedGainValue, true);
            GameDataManager.Instance.GameCoin += GameDataManager.Instance.PhyEquipMergedGainValue;
        }
        /// <summary>
        /// 检测魔法武器合成增益出行
        /// </summary>
        public void CheckMagicEquipMergedGain()
        {
            DataCenter.itemsData.ChangeItemsCount((int)GameConst.ItemIdCode.GameCoin, GameDataManager.Instance.MagicEquipMergedGainValue, true);
            GameDataManager.Instance.GameCoin += GameDataManager.Instance.MagicEquipMergedGainValue;
        }
        /// <summary>
        /// 进入游戏的接口
        /// </summary>
        /// <param name="type"></param>
        /// <param name="chapterId"></param>
        /// <param name="data"></param>
        public void EnterGame(EStateType type, int chapterId, FightData data)
        {
            Init();
            GameDataManager.Instance.CurStageType = type;
            GameDataManager.Instance.CurChapterId = chapterId;
            GameDataManager.Instance.CurChapterId = chapterId;
            GameDataManager.Instance.Init(data,type);
            PlayerStats.Instance.Init();
            UIManager.Instance.CloseAllTopDialog();
            PopUpManager.Instance.Clear();
            if (GameDataManager.Instance.CurStageType == EStateType.StageTypeEndless)
            {
                GameDataManager.Instance.InitEndlessBaseAttr(CurChapterId);
                ProcedureManager.Instance.ChangeAsync<StartFightMenuView>(nameof(EndlessFightingProcedure), true).Forget();
            }
            else if (GameDataManager.Instance.CurStageType == EStateType.StageTypeChallenge)//每日挑战
            {
                if (DataCenter.dailyFightData.HasArchive)
                {
                    DataCenter.itemsData.ResourceDatas[(int)GameConst.ItemIdCode.GameCoin].Count = data.Stage.Money;
                }
                else
                {
                    DataCenter.itemsData.ResourceDatas[(int)GameConst.ItemIdCode.GameCoin].Count = data.Stage.Money;
                }
                PlayerStats.Instance.KillCount = data.Stage.Kills;
                // 进入战斗
                ProcedureManager.Instance.ChangeAsync<StartFightMenuView>(nameof(ChallengeFightingProcedure), true).Forget();
            }
            else
            {
                if (DataCenter.mainStageData.HasArchive)
                {
                    DataCenter.itemsData.ResourceDatas[(int)GameConst.ItemIdCode.GameCoin].Count = data.Stage.Money;
                }
                else
                {
                    DataCenter.itemsData.ResourceDatas[(int)GameConst.ItemIdCode.GameCoin].Count = 0;
                }
                // 进入战斗
                ProcedureManager.Instance.ChangeAsync<StartFightMenuView>(nameof(MainFightingProcedure), true).Forget();
                // 初始化经验，等级这些数据
            }
        }
        /// <summary>
        /// 结束游戏的统一接口
        /// </summary>
        public void OnGameEnd(bool isWin,bool isFinish = true,  bool isDebug = false, bool isFight = true)
        {
            if(isSendGameEnd) return;
            isSendGameEnd = true;
            mainMergeVMInstance = null;
            GlobalSchedule.Instance.SetTimeScale(GameConst.TimeDefaultScale);
            IsWin = isWin;
            IsEnterFight = isFight;
            var fightstar = GetFightStat(isDebug);
            DealGameResult(fightstar,isFinish).Forget();
        }
        /// <summary>
        /// 退出游戏
        /// </summary>
        public async void OnExitGame()
        {
            secretTalentVm = null;
            GameDataManager.Instance.GameCoin = 0;
            Time.timeScale = 1.0f;
            GameRewards.Clear();
            
            if (IsEnterFight)
            {
                await UIManager.Instance.CloseAllTopDialogAsync(false);

                if (MainUiManager.Instance.CurTabType == ETabType.TabTypeActivity)
                {
                    MainUiManager.Instance.CurTabType = ETabType.TabTypeActivity;
                    await ProcedureManager.Instance.ChangeAsync(nameof(MainGameProcedure), true);
                    switch (GameDataManager.Instance.CurStageType)
                    {
                        case EStateType.StageTypeChallenge:
                        {
                            UIManager.Instance.OpenDialog<ChallengeActivityView,ChallengeActivityViewModel>().Forget();
                            
                        } break;
                        case EStateType.StageTypeEndless:
                        {
                            UIManager.Instance.OpenDialog<EndlessActivityView,EndlessActivityViewModel>().Forget();
                        } break;
                        case EStateType.StageTypeSecret:
                        {
                            UIManager.Instance.OpenDialog<SecretView, SecretViewModel>().Forget();
                        } break;
                    }
                    
                }
                else
                {
                    // 进入主场景
                    await ProcedureManager.Instance.ChangeAsync(nameof(MainGameProcedure), true);
                    if (GameDataManager.Instance.CurStageType == EStateType.StageTypeSecret)
                    {
                        await UIManager.Instance.OpenDialog<SecretView, SecretViewModel>();
                    }
                }
            }
            else
            {
                if (GameDataManager.Instance.CurStageType == EStateType.StageTypeEndless)
                {
                    UIManager.Instance.CloseDialog<MainStageGameResultView>();
                }
                else
                {
                    UIManager.Instance.CloseDialog<MainStageGameResultView>();
                }
            }
        }
        private async UniTaskVoid DealGameResult(FightStat fightData, bool isFinish)
        {
            var result = await SendAndDealGameEnd(fightData, isFinish);
            if (result != 0) IsWin = false;
            if (GameDataManager.Instance.CurStageType == EStateType.StageTypeEndless)
            {
                DealEndlessGameResult(fightData);
            }
            else
            {
                DealMainStageGameResult(fightData).Forget();
            }
        }
        /// <summary>
        /// 发送游戏结束请求
        /// </summary>
        /// <param name="fightData"></param>
        /// <returns></returns>
        private async UniTask<int> SendAndDealGameEnd(FightStat fightData, bool isFinish)
        {
            DHLog.Debug("muzili  请求战斗结算 ");
            var result = -1;
            switch (GameDataManager.Instance.CurStageType)
            {
                case EStateType.StageTypeEndless: result = await OnEndlessFightingEnd(fightData); break;
                case EStateType.StageTypeChallenge: result = await OnDailyFightingEnd(fightData); break;
                case EStateType.StageTypeSecret: result = await OnSecretFightingEnd(fightData); break;
                case EStateType.StageTypeMainStage: result = await OnMainFightEnd(fightData, isFinish); break;
                
            }
            return result;
        }

        /// <summary>
        /// 秘林探险结算
        /// </summary>
        /// <param name="fightData"></param>
        /// <returns></returns>
        private async UniTask<int> OnSecretFightingEnd(FightStat fightData)
        {
            var sData = new ReqSecretFightEnd();
            sData.StageId = CurChapterId;
            sData.Uid = GameDataManager.Instance.Uid;
            sData.Stat = fightData;
            sData.Archive = !IsEnterFight;;
            sData.IsPass = IsWin;
            
            var result = await GameNetworkManager.Instance.SendAsync<RspSecretFightEnd>(sData);
            if (result.rsp ==null ||result.rsp.Status != 0)
            {
                if(result.rsp == null) return -1;
                DHLog.Debug($" 请求结算游戏失败  {result.rsp.Status}");
                var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
                ToastManager.Show(str);
                // 这里给提示
                return -1;
            }
            GameRewards = result.rsp.Reward.ToList();
            DataCenter.secretData.HasArchive = false;
            if (IsWin)
            {
                var isClaim = DataCenter.secretData.IsCanClaimReward(CurChapterId);
                DataCenter.secretData.UpdateStageInfo(CurChapterId,IsWin,isClaim);
                var cfg = ConfigCenter.CopySecretCfgColl.GetDataById(CurChapterId);
                if (cfg != null && cfg.NextPass != 0)
                {
                    if (DataCenter.secretData.CurrStage < cfg.NextPass)
                    {
                        DataCenter.secretData.CurrStage = cfg.NextPass;
                    }
                }
            }
            
            return 0;
        }

        /// <summary>
        /// 主线战斗结束
        /// </summary>
        /// <param name="fightData"></param>
        /// <param name="isFinish"></param>
        /// <returns></returns>
        private async UniTask<int> OnMainFightEnd(FightStat fightData,bool isFinish)
        {
            var data = new ReqMainFightEnd();
            data.Uid = GameDataManager.Instance.Uid;
            data.ChapterId = CurChapterId;
            data.Stat = fightData;
            data.Archive = !IsEnterFight;
            data.EquipSlots.Clear();
            data.Finish = isFinish;
            for (var i = 0; i < GameDataManager.Instance.BackpackWeaponList.Count; i++)
            {
                var curWeaponData = GameDataManager.Instance.BackpackWeaponList[i];
                for (var j = 0; j < curWeaponData.OccupyList.Count; j++)
                {
                    var curRow = curWeaponData.RowIdx + curWeaponData.OccupyList[j].x;
                    var curColumn = curWeaponData.ColumnIdx + curWeaponData.OccupyList[j].y;
                    var curIdx = (curRow + 1) * 100 + curColumn + 1;
                    var gridValue = 1;
                    if (j == 0) gridValue = curWeaponData.WeaponId;
                    
                    data.EquipSlots[curIdx] = gridValue;
                }
            }
            var result = await GameNetworkManager.Instance.SendAsync<RspMainFightEnd>(data);
            if (result.rsp ==null ||result.rsp.Status != 0)
            {
                DHLog.Debug(" 请求结算游戏失败");
                if(result.rsp == null) return -1;
                var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
                ToastManager.Show(str);
                // 这里给提示
                return -1;
            }

            Star = result.rsp.Star;
            GameRewards = result.rsp.Rewards.ToList();
            DataCenter.mainStageData.HasArchive = false;
            
            return 0;
        }
        
        //主关卡结算
        private async UniTaskVoid DealMainStageGameResult(FightStat fightData)
        {
            if (IsWin)
            {
                var cfg = ConfigCenter.CopyCfgColl.GetDataById(CurChapterId);
                if (cfg.NextPass > 0)
                {
                    if (DataCenter.mainStageData.CurrChapter < cfg.NextPass)
                    {
                        DataCenter.mainStageData.CurrChapter = cfg.NextPass;
                        DataCenter.mainStageData.UpdateChapterList();
                        MainUiManager.Instance.UpdateShowIndex();
                    }
                }
                if (!DataCenter.mainStageData.IsPassChapter(CurChapterId))
                {
                    EquipManager.Instance.CheckUnLockNewEquip(CurChapterId);
                }
            }
            DataCenter.mainStageData.OnPassCurChapter(CurChapterId, fightData, Star);
            // 展示结算
            var tempVm = new MainStageGameResultViewModel(GameRewards, IsWin);
            UIManager.Instance.OpenDialog<MainStageGameResultView>(tempVm).Forget();
            //引导好评
            AddGuidePositiveReviews();
        }
        
        public int GetRewardCompareValue(Resource reward)
        {
            return (int)reward.Type * 10000 + reward.Id;
        }

        /// <summary>
        /// 获取主动将列表
        /// </summary>
        /// <returns></returns>
        public List<int> GetActiveSkillList()
        {
            List<int> ret = new();
            var heroId = GameDataManager.Instance.HeroId;
            var heroCfg = ConfigCenter.HeroMainCfgColl.GetDataById(heroId);
            if (heroCfg == null || heroCfg.MainSkill == 0) return ret;
            var skillCfg = ConfigCenter.HeroSkillCfgColl.GetDataById(heroCfg.MainSkill);
            if (skillCfg is { SkillType: 1 }) ret.Add(heroCfg.MainSkill);
            return ret;
        }
        /// <summary>
        /// 使用主动技能
        /// </summary>
        /// <param name="skillId"></param>
        public void OnClickActiveSkill(int skillId)
        {
            // 修改能量
            var heroSkillCfg = ConfigCenter.HeroSkillCfgColl.GetDataById(skillId);
            if (heroSkillCfg == null)
            {
                DHLog.Debug($"heroSkillCfg is null 请检查配置 Skill id is {skillId}");
                return;
            }
            // 消耗能量
            GameDataManager.Instance.HeroActiveEnergy -= heroSkillCfg.Energy;
            // TO-DO 主动 释放技能 
            BattleManager.Instance.fightingManagerIns.TakeHeroSkill();
        }

        /// <summary>
        /// 天赋的特殊处理
        /// </summary>
        /// <param name="talentId"></param>
        public void DealSpecialTalent(int talentId)
        {
            switch (talentId)
            {
                // 随机装备升级
                case (int)ESpecialTalentId.RandomEquipUpgrade:
                {
                    mainMergeVMInstance?.RandomUpgradeWeapon();
                } break;
            }
        }
        /// <summary>
        /// 获取天心愿图标路径
        /// </summary>
        /// <param name="wishId"></param>
        /// <returns></returns>
        public string GetWishIconPath(int wishId)
        {
            var type = GameDataManager.Instance.ParseWishType(wishId);
            var dataId = GameDataManager.Instance.ParseWishDataId(wishId);
            switch (type)
            {
                case EWishType.WishTypeEquip:
                {
                    var cfg = ConfigCenter.EquipModelCfgColl.GetDataById(dataId);
                    return cfg.Pic;
                }
                case EWishType.WishTypeHoly:
                {
                    var cfg = ConfigCenter.HolyCfgColl.GetDataById(dataId);
                    return cfg.HolyIcon;
                }
            }
            return UIHelper.NoneImagePath();
        }

        /// <summary>
        /// 请求升级
        /// </summary>
        public async UniTaskVoid RequestUpGrade()
        {
            ReqBattleLvUp req = new();
            req.Uid = GameDataManager.Instance.Uid;
            req.Lv = GameDataManager.Instance.Level;
            req.Exp = GameDataManager.Instance.Exp;
            var result = await GameNetworkManager.Instance.SendAsync<RspBattleLvUp>(req);
            if (result.rsp is not { Status: 0 })
            {
                if(result.rsp == null) return;
                var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
                ToastManager.Show(str);
            }
        }
        /// <summary>
        /// 天赋刷新
        /// </summary>
        /// <param name="opType"> 1-升级免费刷新，2-广告免费刷新</param>
        /// <param name="chooseTalentCount">由于可以存储这个刷新，这个参数传递的是需要刷新哪个等级的天赋不是当前等级；广告的时候传当前已经刷新到哪个等级了</param>
        /// <param name="callback">刷新完成的回调</param>
        public async UniTaskVoid RequestRefreshTalent(ERefreshType opType, int chooseTalentCount, Action<List<int>> callback)
        {
            // DHLog.Debug($"muzili RequestRefreshTalent log level = {GameDataManager.Instance.Level} requestCount = {chooseTalentCount}  count = {GameDataManager.Instance.ChooseTalentCount}");
            var req = new ReqBattleTalentRefresh();
            req.Uid = GameDataManager.Instance.Uid;
            req.Op = (int)opType;
            req.Param = chooseTalentCount;
	        
            var result = await GameNetworkManager.Instance.SendAsync<RspBattleTalentRefresh>(req);
            if (result.rsp is not { Status: 0 })
            {
                if(result.rsp == null) return;
                var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
                ToastManager.Show(str);
                return;
            }
            if (opType == ERefreshType.RefreshTypeFree)
            {
                GameDataManager.Instance.ChooseTalentCount += 1;
            }
            
            // 更新数据
            GameDataManager.Instance.UnOpTalents = result.rsp.Talents.ToList();
            // 刷新天赋列表
            callback(result.rsp.Talents.ToList());
        }
        
        public async UniTaskVoid RequestSecretTalent(ESecretTalentRefreshType opType, Action<List<int>,ESecretTalentRefreshType> callback, int level, int exp)
        {
            IsCanRandomTalent = false;
            DHLog.Debug($"muzili log RequestSecretTalent  level is {level}  ");
            // 
            ReqSecretBattleTalentRefresh sData = new();
            sData.Uid = GameDataManager.Instance.Uid;
            sData.Op = (int)opType;
            sData.Lv = level;  
            sData.Exp = exp;
            sData.Dur = GameDataManager.Instance.CurGameDuration;
            var result = await GameNetworkManager.Instance.SendAsync<RspSecretBattleTalentRefresh>(sData);
            if (result.rsp is not { Status: 0 })
            {
                if(result.rsp == null) return;
                var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
                ToastManager.Show(str);
                return;
            }
            
            if (opType == ESecretTalentRefreshType.Init)
            {
                // 更新数据
                GameDataManager.Instance.TalentBeginRefresh -= 1;
                
            } else if (opType == ESecretTalentRefreshType.AdRefresh)
            {
                GameDataManager.Instance.TalentAdReFreshCount -= 1;
            } 

            // 更新天赋列表
            callback(result.rsp.Talents.ToList(), opType);
            // 移除 天赋刷新队列
            PopupRequestTalentList();
           
        }

        public async UniTaskVoid RequestSecretBattleWaveDone()
        {
            var req = new ReqSecretBattleWaveDone();
            req.Uid = GameDataManager.Instance.Uid;
            req.Energy = GameDataManager.Instance.HeroActiveEnergy;
            req.Wave = GameDataManager.Instance.Wave;
            req.Lv = GameDataManager.Instance.Level; //服务器等级
            req.Exp = GameDataManager.Instance.Exp;
            req.Dur = GameDataManager.Instance.CurGameDuration;
            req.Kills = PlayerStats.Instance.KillCount;
            req.Boss = GameDataManager.Instance.GetCurWaveBossId();
            req.EquipSlots.Clear();
            foreach (var item in GameDataManager.Instance.EquipSlots)
            {
                if(item.Value == 0) continue;
                req.EquipSlots.Add(item.Key, item.Value);
            }
            req.Attr.Clear();
            // 属性信息
            foreach (var item in GameDataManager.Instance.StageAttr)
            {
                req.Attr.Add(item.Key,item.Value);
            }

            var curHp = BattleManager.Instance.fightingManagerIns.playerCtrl.Data.resource.Hp;
            var maxHp = BattleManager.Instance.fightingManagerIns.playerCtrl.Data.resource.MaxHp;
            var curArmor = BattleManager.Instance.fightingManagerIns.playerCtrl.Data.resource.Armor;
            
            req.Attr[AttributeName.Hp] = (int)curHp;
            req.Attr["maxHp"] = (int)maxHp;
            req.Attr["armor"] = (int)curArmor;
            var trident = BattleManager.Instance.fightingManagerIns.playerCtrl.Data.skill.GetSkill(2400);
            if (trident != null) req.Attr["tridentKillNum"] = (int)trident.KillNum;
            GameDataManager.Instance.HurtStat.Clear();
            req.HurtStat.Clear();
            // 伤害值
            foreach (var item in PlayerStats.Instance.SkillHurtsDic)
            {
                req.HurtStat.Add(item.Key, item.Value);
                GameDataManager.Instance.HurtStat.Add(item.Key, item.Value);
            }
            var result = await GameNetworkManager.Instance.SendAsync<RspSecretBattleWaveDone>(req);
            if (result.rsp is not { Status: 0 })
            {
                if(result.rsp == null) return;
                var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
                ToastManager.Show(str);
            }
        }
        /// <summary>
        /// 主线关卡波次结算请求
        /// </summary>
        /// <param name="callback"></param>
        public async UniTaskVoid RequestReqBattleWaveDone(Action callback)
        {
            DHLog.Debug($" muzili log net RequestReqBattleWaveDone  level = {GameDataManager.Instance.Level } exp = {GameDataManager.Instance.Exp}");
            var req = new ReqBattleWaveDone();
            req.Uid = GameDataManager.Instance.Uid;
            req.Lv = GameDataManager.Instance.Level;
            req.Exp = GameDataManager.Instance.Exp;
            req.Energy = GameDataManager.Instance.HeroActiveEnergy;
            req.Wave = GameDataManager.Instance.Wave - 1;
            if (req.Wave < 0) req.Wave = 0;
            req.EquipMergeCount.Add(GameDataManager.Instance.PhysicsMergeNum);
            req.EquipMergeCount.Add(GameDataManager.Instance.MagicMergeNum);
            if (GameDataManager.Instance.CurStageType == EStateType.StageTypeChallenge)
            {
                req.Kills = PlayerStats.Instance.KillCount;
            }
            req.Equips.Clear();
            foreach (var weaponDataItem in GameDataManager.Instance.RandomWeaponDataList)
            {
                req.Equips.Add(weaponDataItem.WeaponId);
            }
            req.EquipSlots.Clear();
            // 这里更新槽位信息
            for (var i = 0; i < GameDataManager.Instance.GMaxRow; i++)
            {
                for (var j = 0; j < GameDataManager.Instance.GMaxColumn; j++)
                {
                    var index = (i + 1) * 100 + j + 1;
                    var gridValue = GameDataManager.Instance.GridData[i][j];
                    if (gridValue != 0 && gridValue != 1) continue;
                    req.EquipSlots.Add(index,gridValue);
                }
            }
            for (var i = 0; i < GameDataManager.Instance.BackpackWeaponList.Count; i++)
            {
                var curWeaponData = GameDataManager.Instance.BackpackWeaponList[i];
                for (var j = 0; j < curWeaponData.OccupyList.Count; j++)
                {
                    var curRow = curWeaponData.RowIdx + curWeaponData.OccupyList[j].x;
                    var curColumn = curWeaponData.ColumnIdx + curWeaponData.OccupyList[j].y;
                    var curIdx = (curRow + 1) * 100 + curColumn + 1;
                    var gridValue = 1;
                    if (j == 0) gridValue = curWeaponData.WeaponId;
                    if (!req.EquipSlots.TryAdd(curIdx,gridValue)) 
                        req.EquipSlots[curIdx] = gridValue;
                }
            }
            req.Attr.Clear();
            // 属性信息
            foreach (var item in GameDataManager.Instance.StageAttr)
            {
                req.Attr.Add(item.Key,item.Value);
            }
            var curHp = BattleManager.Instance.fightingManagerIns.playerCtrl.Data.resource.Hp;
            var maxHp = BattleManager.Instance.fightingManagerIns.playerCtrl.Data.resource.MaxHp;
            
            req.Attr[AttributeName.Hp] = (int)curHp;
            req.Attr["maxHp"] = (int)maxHp;
            var trident = BattleManager.Instance.fightingManagerIns.playerCtrl.Data.skill.GetSkill(2400);
            if (trident != null) req.Attr["tridentKillNum"] = (int)trident.KillNum;
            GameDataManager.Instance.HurtStat.Clear();
            req.HurtStat.Clear();
            // 伤害值
            foreach (var item in PlayerStats.Instance.SkillHurtsDic)
            {
                req.HurtStat.Add(item.Key, item.Value);
                GameDataManager.Instance.HurtStat.Add(item.Key, item.Value);
            }
            req.Wish.Clear();
            foreach (var id in GameDataManager.Instance.Wish)
            {
                req.Wish.Add(id);
            }
            var result = await GameNetworkManager.Instance.SendAsync<RspBattleWaveDone>(req);
            if (result.rsp is not { Status: 0 })
            {
                if(result.rsp == null) return;
                var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
                ToastManager.Show(str);
                return;
            }
            // 更新数据
            GameDataManager.Instance.PhysicsMergeNum = 0;
            GameDataManager.Instance.MagicMergeNum= 0;
            GameDataManager.Instance.GameCoin = result.rsp.Money;
            GameDataManager.Instance.EquipWaveRefresh = result.rsp.EquipWaveRefresh;
            GameDataManager.Instance.EquipWaveAdRefresh = result.rsp.EquipWaveAdRefresh;
            // 这里更新主界面UI
            callback();
        }
        
        /// <summary>
        /// 检测双属性武器合成条件是否解锁
        /// </summary>
        /// <param name="equipId"></param>
        public bool CheckWeaponDoubleAttrUnlocked(int equipId)
        {
            return GameDataManager.Instance.CheckWeaponDoubleAttrUnlocked(equipId);
        }
        /// <summary>
        /// 更新当前格子的大小
        /// </summary>
        public void UpdateGridSize()
        {
            var minRow = GameDataManager.Instance.GridBorderRect.xMin;
            var maxRow = GameDataManager.Instance.GridBorderRect.xMax;
            var minColumn = GameDataManager.Instance.GridBorderRect.yMin;
            var maxColumn = GameDataManager.Instance.GridBorderRect.yMax;
            if (BlockState == EBlockState.Normal)
            {
                var rowSize = (int)(maxRow - minRow + 1);
                var columnSize = (int)(maxColumn - minColumn + 1);
                if (rowSize > 0 && columnSize > 0)
                {
                    GridSize = new Vector2Int(rowSize,columnSize);
                }
            }
            else if (BlockState == EBlockState.AddCell)
            { 
                GridSize= new Vector2Int(GameDataManager.Instance.GMaxRow,GameDataManager.Instance.GMaxColumn);
            }
        }
        /// <summary>
        /// 获取最大的边界值
        /// </summary>
        /// <returns></returns>
        public Rect GetMaxGridBorder()
        {
            var maxBorder = new Rect
            {
                xMin = 1, xMax = GameDataManager.Instance.GMaxRow, 
                yMin = 1, yMax = GameDataManager.Instance.GMaxColumn
            };
            return maxBorder;
        }
        /// <summary>
        /// 检测屏幕坐标是否在rect有效范围内
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        public bool CheckScreenPosValidity(RectTransform rect,Vector2 screenPoint)
        {
            if (rect == null) return false;
            if (!RectTransformUtility.RectangleContainsScreenPoint(rect,screenPoint, AppGlobal.Instance.UICamera)) return false;
            return true;
        }
        /// <summary>
        /// 根据行列号算出格子的本地坐标
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public Vector3 CalculationGridLocalPos(int row, int column)
        {
            var minRow = GameDataManager.Instance.GridBorderRect.xMin;
            var maxRow = GameDataManager.Instance.GridBorderRect.xMax;
            var minColumn = GameDataManager.Instance.GridBorderRect.yMin;
            var maxColumn = GameDataManager.Instance.GridBorderRect.yMax;
            var rowCenter = (maxRow + minRow)*0.5f;
            var columnCenter = (maxColumn + minColumn)*0.5f;
            var rowDiff = row+1 - rowCenter;
            var columnDiff = column+1 - columnCenter;
            var tmpLocalX = columnDiff * CellSize;
            var tmpLocalY = -rowDiff * CellSize;
            var resultPos = new Vector3(tmpLocalX,tmpLocalY,0);
            return resultPos;
        }
        /// <summary>
        /// 计算格子的本地坐标
        /// </summary>
        /// <param name="idxPos"></param>
        /// <param name="gridType"></param>
        /// <returns></returns>
        public Vector3 CalculationLocalPos(Vector2Int idxPos,GridType gridType)
        {
            var rowCenter = (GameDataManager.Instance.GridBorderRect.xMax+GameDataManager.Instance.GridBorderRect.xMin)*0.5f;
            var columnCenter = (GameDataManager.Instance.GridBorderRect.yMax+GameDataManager.Instance.GridBorderRect.yMin)*0.5f;
            var rowDiff = idxPos.x+1 - rowCenter;
            var columnDiff = idxPos.y+1 - columnCenter;
            var tmpLocalX = columnDiff * CellSize;
            var tmpLocalY = -rowDiff * CellSize;
            switch (gridType)
            {
                case GridType.LTwoNum://横排2格 sizeInfo = new Vector2Int(2,1);
                {
                    tmpLocalX += CellSize * 0.5f;
                } break; 
                case GridType.HTwoNum://竖排2格 sizeInfo = new Vector2Int(1,2);
                {
                    tmpLocalY -=CellSize * 0.5f;
                } break; 
                case GridType.LThreeNum://横排3格 sizeInfo = new Vector2Int(3,1);
                {
                    tmpLocalX += CellSize;
                } break; 
                case GridType.HThreeNum://竖排3格 sizeInfo = new Vector2Int(1,3);
                {
                    tmpLocalY -=CellSize;
                } break; 
                case GridType.LtThreeNum: //L型3格（向下）sizeInfo = new Vector2Int(2,2);
                {
                    tmpLocalX += CellSize * 0.5f;
                    tmpLocalY -=CellSize * 0.5f;
                } break;
                case GridType.LzThreeNum: //L型3格（向上）sizeInfo = new Vector2Int(2,2);
                {
                    tmpLocalX -= CellSize * 0.5f;
                    tmpLocalY -=CellSize * 0.5f;
                } break;
                case GridType.DFourNum://丁字4格 sizeInfo = new Vector2Int(3,2);
                {
                    tmpLocalX += CellSize;
                    tmpLocalY -=CellSize * 0.5f;
                } break; 
                case GridType.LtFourNum://L型4格 sizeInfo = new Vector2Int(2,3);
                {
                    tmpLocalX += CellSize * 0.5f;
                    tmpLocalY -=CellSize;
                } break;
                case GridType.SFourNum://4方格 sizeInfo = new Vector2Int(2,2);
                {
                    tmpLocalX += CellSize * 0.5f;
                    tmpLocalY -=CellSize * 0.5f;
                } break; 
            }
            return new Vector3(tmpLocalX,tmpLocalY,0);
        }
        /// <summary>
        /// 添加随机库中的元素
        /// </summary>
        /// <param name="weaponParamId"></param>
        /// <param name="weaponAttrType"></param>
        public void AddRandomWeaponData(int weaponParamId,int weaponAttrType)
        {
            var tmpData = new RandomWeaponData{ WeaponId = weaponParamId,WeaponAttrType = weaponAttrType};
            GameDataManager.Instance.RandomWeaponDataList.Add(tmpData);
            GameDataManager.Instance.CurFightData.Stage.Equips.Add(weaponParamId);
        }
        /// <summary>
        /// 移除随机库中的元素
        /// </summary>
        /// <param name="weaponParamId"></param>
        /// <param name="weaponAttrType"></param>
        public void RemoveRandomWeaponData(int weaponParamId,int weaponAttrType)
        {
            for (var i = GameDataManager.Instance.RandomWeaponDataList.Count-1; i >= 0; i--)
            {
                var curRandomWeaponData = GameDataManager.Instance.RandomWeaponDataList[i];
                if (curRandomWeaponData.WeaponId != weaponParamId || curRandomWeaponData.WeaponAttrType != weaponAttrType) continue;
                GameDataManager.Instance.RandomWeaponDataList.RemoveAt(i);
                break;
            }
            
            for (var i = 0; i < GameDataManager.Instance.CurFightData.Stage.Equips.Count; i++)
            {
                var curWeaponId = GameDataManager.Instance.CurFightData.Stage.Equips[i];
                if (curWeaponId == weaponParamId)
                {
                    GameDataManager.Instance.CurFightData.Stage.Equips.RemoveAt(i);
                    break;
                }
            }
        }
        /// <summary>
        /// 添加武器到背包
        /// </summary>
        public void AddWeaponToBackpack(BackpackWeaponData weaponData)
        {
           GameDataManager.Instance.AddWeaponToBackpack(weaponData);
        }

        /// <summary>
        /// 刷新背包中武器数据（位置变化）
        /// </summary>
        /// <param name="weaponData"></param>
        public void UpdateWeaponDataInBackpack(BackpackWeaponData weaponData)
        {
            for (var i = GameDataManager.Instance.BackpackWeaponList.Count-1; i >= 0; i--)
            {
                var curWeapon = GameDataManager.Instance.BackpackWeaponList[i];
                if (curWeapon.Uid != weaponData.Uid) continue;
                GameDataManager.Instance.BackpackWeaponList[i] = weaponData;
                break;
            }
        }
        /// <summary>
        /// 从背包里移除武器
        /// </summary>
        public void RemoveWeaponFromBackpack(BackpackWeaponData weaponData)
        {
            GameDataManager.Instance.RemoveWeaponFromBackpack(weaponData);
        }
      
        /// <summary>
        /// 添加心愿池数据
        /// </summary>
        /// <param name="weaponParamId"></param>
        /// <param name="weaponAttrType"></param>
        public void AddWeaponToWishPool(int weaponParamId,int weaponAttrType)
        {
            var wishId = weaponParamId * 10 + weaponAttrType;
            GameDataManager.Instance.Wish.Add(wishId);
        } 
        /// <summary>
        /// 移除心愿池数据
        /// </summary>
        /// <param name="weaponParamId"></param>
        /// <param name="weaponAttrType"></param>
        public void RemoveWeaponFromWishPool(int weaponParamId,int weaponAttrType)
        {
            var wishId = weaponParamId * 10 + weaponAttrType;
            for (var i = GameDataManager.Instance.Wish.Count-1; i >= 0; i--)
            {
                var curWishId = GameDataManager.Instance.Wish[i];
                if (curWishId == wishId)
                {
                    GameDataManager.Instance.Wish.RemoveAt(i);
                }
            }
        } 
        /// <summary>
        /// 根据传入的武器Id+装备Id判断是否为最大等级武器
        /// </summary>
        /// <param name="weaponId"></param>
        /// <param name="equipId"></param>
        /// <returns></returns>
        public bool CheckWeaponMaxLevel(int weaponId,int equipId)
        {
            return GameDataManager.Instance.CheckWeaponMaxLevel(weaponId,equipId);
        }

        /// <summary>
        /// 点击 退出游戏
        /// </summary>
        public void OnClickExitBtn()
        {
            var titleStr = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips05);
            var contentStr =  LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips06);
            var cancelStr =  LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips07);
            var conformStr = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips08);
            Action cancelFunc = () => { OnGameEnd(false, false);};
            Action conformFunc = () => { };
            var tempVm = new CommonMessageBoxViewModel(titleStr,contentStr,cancelStr,conformStr,cancelFunc,conformFunc, null);
            UIManager.Instance.OpenDialog<CommonMessageBox>(tempVm).Forget();
        }
        #region 无尽关卡模块
        public readonly int EndlessMaxWaveId = 20; //无尽关卡最大波次数
        /// <summary>
        /// 无尽关卡结束
        /// </summary>
        /// <param name="fightData"></param>
        /// <returns></returns>
        private async UniTask<int> OnEndlessFightingEnd(FightStat fightData) 
        {
            var data = new ReqEndlessEnd();
            data.Uid = GameDataManager.Instance.Uid;
            data.Stat = fightData;
            data.Archive = !IsEnterFight;
            var result = await GameNetworkManager.Instance.SendAsync<RspEndlessEnd>(data);
            if (result.rsp.Status != 0)
            {
                DHLog.Debug(" 请求结算游戏失败");
                var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
                ToastManager.Show(str);
                // 这里给提示
                return -1;
            }
            GameRewards = result.rsp.Rewards.ToList();
            DataCenter.endlessData.MaxDayCoin = result.rsp.MaxDayCoin;
            DataCenter.endlessData.HasArchive = false;
            return 0;
        }
        /// <summary>
        /// 处理无尽关卡结算逻辑
        /// </summary>
        /// <param name="fightData"></param>
        private void DealEndlessGameResult(FightStat fightData)
        {
            // 展示结算
            PlayerStats.Instance.KillCount = fightData.Kills;
            var tempVm = new EndlessGameResultViewModel(GameRewards, fightData);
            UIManager.Instance.OpenDialog<EndlessGameResultView>(tempVm).Forget();
        }
         /// <summary>
        /// 无尽模式波次结算请求
        /// </summary>
        /// <param name="callback"></param>
        public async UniTaskVoid ReqBattleEndlessWaveDone(Action callback)
        {
            var req = new ReqBattleEndlessWaveDone();
            req.Uid = GameDataManager.Instance.Uid;
            req.Kills = PlayerStats.Instance.KillCount;
            req.SurvivalTime = (int)CostTime;
            req.Energy = GameDataManager.Instance.HeroActiveEnergy;
            req.EquipMergeCount.Add(GameDataManager.Instance.PhysicsMergeNum);
            req.EquipMergeCount.Add(GameDataManager.Instance.MagicMergeNum);
            if (req.Kills > 1500) req.Kills = 1500;
            req.Equips.Clear();//随机库装备信息
            foreach (var weaponDataItem in GameDataManager.Instance.RandomWeaponDataList)
            {
                req.Equips.Add(weaponDataItem.WeaponId);
            }
            req.EquipSlots.Clear();//棋盘信息
            // 这里更新槽位信息
            for (var i = 0; i < GameDataManager.Instance.GMaxRow; i++)
            {
                for (var j = 0; j < GameDataManager.Instance.GMaxColumn; j++)
                {
                    var index = (i + 1) * 100 + j + 1;
                    var gridValue = GameDataManager.Instance.GridData[i][j];
                    if (gridValue is 0 or 1) req.EquipSlots.Add(index,gridValue);
                }
            }
            for (var i = 0; i < GameDataManager.Instance.BackpackWeaponList.Count; i++)
            {
                var curWeaponData = GameDataManager.Instance.BackpackWeaponList[i];
                for (var j = 0; j < curWeaponData.OccupyList.Count; j++)
                {
                    var curRow = curWeaponData.RowIdx + curWeaponData.OccupyList[j].x;
                    var curColumn = curWeaponData.ColumnIdx + curWeaponData.OccupyList[j].y;
                    var curIdx = (curRow + 1) * 100 + curColumn + 1;
                    var gridValue = 1;
                    if (j == 0) gridValue = curWeaponData.WeaponId;
                    if (!req.EquipSlots.TryAdd(curIdx,gridValue)) 
                        req.EquipSlots[curIdx] = gridValue;
                }
            }
            req.Attr.Clear();
            // 属性信息
            foreach (var item in GameDataManager.Instance.StageAttr)
            {
                req.Attr.Add(item.Key,item.Value);
            }
            var curHp = BattleManager.Instance.fightingManagerIns.playerCtrl.Data.resource.Hp;
            var maxHp = BattleManager.Instance.fightingManagerIns.playerCtrl.Data.resource.MaxHp;
            req.Attr[AttributeName.Hp] = (int)curHp;
            req.Attr["maxHp"] = (int)maxHp;
            var trident = BattleManager.Instance.fightingManagerIns.playerCtrl.Data.skill.GetSkill(2400);
            if (trident != null) req.Attr["tridentKillNum"] = (int)trident.KillNum;
            GameDataManager.Instance.HurtStat.Clear();
            req.HurtStat.Clear();
            // 伤害值
            foreach (var item in PlayerStats.Instance.SkillHurtsDic)
            {
                req.HurtStat.Add(item.Key, item.Value);
                GameDataManager.Instance.HurtStat.Add(item.Key, item.Value);
            }
            // 心愿
            req.Wish.Clear();
            foreach (var id in GameDataManager.Instance.Wish)
            {
                req.Wish.Add(id);
            }
            
            var result = await GameNetworkManager.Instance.SendAsync<RspBattleEndlessWaveDone>(req);
            if (result.rsp is not { Status: 0 })
            {
                var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
                ToastManager.Show(str);
                return;
            }
            GameDataManager.Instance.PhysicsMergeNum = 0;
            GameDataManager.Instance.MagicMergeNum = 0;
            // 这里更新主界面UI
            callback();
        }
        /// <summary>
        /// 根据当前最大通关章节获取奖励
        /// </summary>
        /// <param name="chapterId"></param>
        /// <param name="waveNum"></param>
        /// <returns></returns>
        public int GetRewardCoinNumByChapterId(int chapterId,int waveNum)
        {
            var coinNum = 0;
            var cfg = ConfigCenter.CopyCfgColl.GetDataById(chapterId);
            if (cfg == null) return coinNum;
            if (waveNum % 10 > 0) return coinNum;
            if (waveNum % 20 == 0)//偶数必然没20波次倍数奖励
            {
                if (cfg.Coin.Count > 2) coinNum += cfg.Coin[2];
            } 
            else if (waveNum % 10 == 0) //奇数必然没10波次倍数奖励
            {
                if (cfg.Coin.Count > 1) coinNum += cfg.Coin[1];
            }
            return coinNum;
        }
        #endregion

        #region 每日挑战模块

        public bool DailyFightIsBegin = false;//每日战斗是否开始过
        /// <summary>
        /// 每日挑战是否开启
        /// </summary>
        /// <returns></returns>
        public bool IsOpenDailyFight()
        {
            var isOpen = false;
            var dailyFightOpenTime = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.dailyStage_05);
            if (dailyFightOpenTime != null && dailyFightOpenTime.Content.Count > 0)
            {
                var curTime = ServerTime.Instance.GetNowTime();
                isOpen = curTime >= dailyFightOpenTime.Content[0];
            }
            return isOpen;
        }
        //每日挑战最大波次数
        public int GetDailyFightMaxWave()
        {
            var maxWave = 10;
            var cfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.dailyStage_08);
            if (cfg != null && cfg.Content.Count > 0) maxWave = cfg.Content[0];
            return maxWave;
        }
        /// <summary>
        /// 每日挑战结束
        /// </summary>
        /// <param name="fightData"></param>
        /// <returns></returns>
        private async UniTask<int> OnDailyFightingEnd(FightStat fightData) 
        {
            var data = new ReqDailyFightEnd();
            data.Uid = GameDataManager.Instance.Uid;
            data.Stat = fightData;
            data.Archive = !IsEnterFight;
            var result = await GameNetworkManager.Instance.SendAsync<RspDailyFightEnd>(data);
            if (result.rsp.Status != 0)
            {
                DHLog.Debug(" 请求结算游戏失败");
                var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
                ToastManager.Show(str);
                // 这里给提示
                return -1;
            }
            GameRewards = result.rsp.Reward.ToList();
            if (fightData.Wave >= GetDailyFightMaxWave())//完成了最大波次，则成功挑战
            {
                DataCenter.dailyFightData.Sweep = true;
            }
            DataCenter.dailyFightData.HasArchive = false;
            DataCenter.dailyFightData.DailyKills = result.rsp.DailyKills;
            return 0;
        }
        /// <summary>
        /// 获取无尽关卡是否可弹出
        /// </summary>
        /// <returns></returns>
        public bool CheckEndlessIsPopupValid()
        {
            var isPopup = true;
            var key = $"{LoginManager.Instance.CurrentAccountId}-Endless";
            if (PlayerPrefs.GetInt(key) > 0) isPopup = false;
            return isPopup;
        }
        /// <summary>
        /// 设置无尽关卡已弹出
        /// </summary>
        /// <returns></returns>
        public void SetEndlessPopupValid()
        {
            var key = $"{LoginManager.Instance.CurrentAccountId}-Endless";
            PlayerPrefs.SetInt(key, 1);
        }
        /// <summary>
        /// 获取每日挑战是否可弹出
        /// </summary>
        /// <returns></returns>
        public bool CheckDailyFightIsPopupValid()
        {
            var isPopup = true;
            var key = $"{LoginManager.Instance.CurrentAccountId}-DailyFight";
            var archiveValue = PlayerPrefs.GetInt(key);
            if (archiveValue > 0) isPopup = false;
            if (isPopup)
            {
                var dailyFightOpenTime = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.dailyStage_05);
                if (dailyFightOpenTime != null && dailyFightOpenTime.Content.Count > 0)
                {
                    var curTime = ServerTime.Instance.GetNowTime();
                    isPopup = curTime >= dailyFightOpenTime.Content[0];
                }
            }
            return isPopup;
        }
        /// <summary>
        /// 设置每日挑战已弹出
        /// </summary>
        /// <returns></returns>
        public void SetDailyFightPopupValid()
        {
            var key = $"{LoginManager.Instance.CurrentAccountId}-DailyFight";
            PlayerPrefs.SetInt(key, 1);
        }
        #endregion
        #region 引导好评

        public readonly string GuidePositiveReviewsSrt = DataCenter.charcaterData.Digest.RoleId + "GuidePositiveReviews_isShow";//引导好评是否展示
        public readonly string GuidePositiveReviewsUnLockLevelSrt = DataCenter.charcaterData.Digest.RoleId + "GuidePositiveReviews_UnLockLevelNums";//引导好评解锁关卡记录
        public bool IsRegisterPopView = false;
        public void CheckGuidePositiveReviewsView()
        {
            var isOpen = MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.GuidePositiveReviews);
            var isShow = DHUnityUtil.PlayerPrefs.GetInt(GuidePositiveReviewsSrt) == 1;
            
            if (!isOpen || isShow)return;
            
            var limitList = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.like_appStore).Content;
            
            var temp = DHUnityUtil.PlayerPrefs.GetString(GuidePositiveReviewsUnLockLevelSrt);
            if (string.IsNullOrEmpty(temp))return;
            
            List<(int, int)> levelsStar = JsonConvert.DeserializeObject<List<(int, int)>>(temp);
            
            if (CheckConsecutiveElements(levelsStar,limitList[0],3) && !IsRegisterPopView)//，连续k星通关n个关卡后触发；
            {
                //DHLog.Error("触发引导好评！");
                IsRegisterPopView = true;
                PopUpManager.Instance.AddPopUp(AddPop, 500);
            }
        }
        void AddPop()
        {
            var isOpen = MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.GuidePositiveReviews);
            var isShow = DHUnityUtil.PlayerPrefs.GetInt(GuidePositiveReviewsSrt) == 1;
            if (!isOpen || isShow)
            {              
                PopUpManager.Instance.CheckAndPopUpView();
                return;
            }
            UIManager.Instance.OpenDialog<GuidePositiveReviewsView>(new GuidePositiveReviewsViewModel()).Forget();
        }
        static bool CheckConsecutiveElements(List<(int,int)> list, int n, int k)
        {
            int count = 0;
            var levelNums = ConfigCenter.FunctionOpenCfgColl.GetDataById((int)EFunctionOpenType.GuidePositiveReviews).Stage;
            var lastNums = 0;
            foreach ((int,int) num in list)
            {
                if (lastNums == 0)
                {
                    lastNums = num.Item1;
                }
                else
                {
                    if (lastNums == num.Item1)
                    {
                        lastNums = num.Item1;
                        count = 0;
                        continue;
                    }
                }
                
                
                if (num.Item2 >= k && num.Item1 >= levelNums)
                {
                    count++;
                    if (count == n)
                    {
                        return true;
                    }
                }
                else
                {
                    count = 0;
                }
            }
            return false;
        }
        
        void AddGuidePositiveReviews()
        {
            //好评引导
            var isOpen = MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.GuidePositiveReviews);
            var isShow = DHUnityUtil.PlayerPrefs.GetInt(GuidePositiveReviewsSrt) == 1;
            if (!isOpen || isShow)return;
            
            var limitList = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.like_appStore).Content;
            
            var savedJson =  DHUnityUtil.PlayerPrefs.GetString(GuidePositiveReviewsUnLockLevelSrt);
            if (string.IsNullOrEmpty(savedJson))
            {
                var levelsList = new List<(int, int)>();
                levelsList.Add((CurChapterId,Star));
                var json = JsonConvert.SerializeObject(levelsList);
                DHUnityUtil.PlayerPrefs.SetString(GuidePositiveReviewsUnLockLevelSrt,json);
            }
            else
            {
                var temp = DHUnityUtil.PlayerPrefs.GetString(GuidePositiveReviewsUnLockLevelSrt);
                List<(int, int)> savedIntList = JsonConvert.DeserializeObject<List<(int, int)>>(temp);
                if (savedIntList.Count < limitList[0])
                {
                    savedIntList.Add((CurChapterId,Star));
                }
                else if(savedIntList.Count == limitList[0])
                {
                    savedIntList.RemoveAt(0);
                    savedIntList.Add((CurChapterId, Star));
                }
                var json = JsonConvert.SerializeObject(savedIntList);
                DHUnityUtil.PlayerPrefs.SetString(GuidePositiveReviewsUnLockLevelSrt,json);
            }
        }
        #endregion

        /// <summary>
        ///  检查天赋是否刷新
        /// </summary>
        public void CheckUnChooseTalent()
        {
            if(GameDataManager.Instance.CurStageType != EStateType.StageTypeSecret) return;
            
            IsCanRandomTalent = true;
            // 检查刷出来的天赋是否选择完成
            var unOpTalent = GameDataManager.Instance.UnOpTalents;
            if ( unOpTalent.Count > 0)
            {
                var curRefreshType = GameDataManager.Instance.TalentBeginRefresh > 0? ESecretTalentRefreshType.Init : ESecretTalentRefreshType.Upgrade;  
                // 打开天赋选择界面
                OpenTalentChooseView(unOpTalent, curRefreshType);
                // 清空数据 
                GameDataManager.Instance.ClearUnOpTalent();
                return;
            }
            if (GameDataManager.Instance.TalentBeginRefresh > 0)
            {
                RequestSecretTalent(ESecretTalentRefreshType.Init,OpenTalentChooseView, GameDataManager.Instance.Level, GameDataManager.Instance.Exp).Forget();
                return;
            }
            
            // 如果还打开在，请关闭界面
            if (secretTalentVm != null)
            {
                UIManager.Instance.CloseDialog<SecretTalentChooseView>();
                secretTalentVm = null;
                GameDataManager.Instance.WaveEnd = false;
                GameTime.Instance.Pause = GameDataManager.Instance.WaveEnd;
            }
            // 检查是否合成
            DelayCheckIsCanMerge().Forget();
        }
        public async UniTaskVoid  DelayCheckIsCanMerge()
        {
            if(requestTalentList.Count > 0) return;
            await UniTask.Delay(800);
            if(EStateType.StageTypeSecret != GameDataManager.Instance.CurStageType) return;
            if(!GameDataManager.Instance.IsCanNeedMerge) return;
            GameDataManager.Instance.CheckEquipCanMerge(AddEquipMergePopup);
        }

        public void OpenTalentChooseView(List<int> talentList, ESecretTalentRefreshType refreshType)
        {
            if (GameDataManager.Instance.CurStageType == EStateType.StageTypeSecret)
            {
                GameTime.Instance.Pause = true;
                if (secretTalentVm != null)
                {
                    secretTalentVm.UpdatePanel(talentList);
                }
                else
                {
                    secretTalentVm = new(talentList, refreshType,RequestChooseTalent, CheckUnChooseTalent);
                    UIManager.Instance.OpenDialog< SecretTalentChooseView>(secretTalentVm).Forget();   
                }
            }
        }

        /// <summary>
        /// 请求选择天赋
        /// </summary>
        /// <param name="chooseIndexList"></param>
        /// <param name="chooseIdList"></param>
        private async void RequestChooseTalent(List<int> chooseIndexList,List<int> chooseIdList)
        {
            ReqSecretBattleTalentSelect sData = new();
            sData.Uid = GameDataManager.Instance.Uid;
            sData.Index.Clear();
            foreach (var index in chooseIndexList)
            {
                sData.Index.Add(index);
            }
            var result = await GameNetworkManager.Instance.SendAsync<RspSecretBattleTalentSelect>(sData);
            if (result.rsp is not { Status: 0 })
            {
                if(result.rsp == null) return;
                var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
                ToastManager.Show(str);
                return;
            }

            foreach (var talentId in result.rsp.Talent)
            {
                var talentCfg = ConfigCenter.SecretCopyTalentCfgColl.GetDataById(talentId);
                if(talentCfg == null) continue;
                if (talentCfg.Type != 3)
                {
                    BattleManager.Instance.fightingManagerIns.playerCtrl.Player.AddTalent(talentId);
                }
                
                // GameDataManager.Instance.AddChooseTalent(1003);
            }
            GameDataManager.Instance.AddChooseTalent(result.rsp.Talent.ToList());
        }
        
        private void AddEquipMergePopup(int equipModelId, int qeuipId, Action<int> callback)
        {
            GameTime.Instance.Pause = true;
            EquipAdvanceViewModel tempVm = new(qeuipId);
            tempVm.ChooseCallback = callback;
            UIManager.Instance.OpenDialog<EquipAdvanceView>(tempVm).Forget();
        }

        public void AddTalentRequestToList(int level,ESecretTalentRefreshType opType = ESecretTalentRefreshType.Upgrade)
        {
            requestTalentList.Add(() => { 
                RequestSecretTalent(opType, OpenTalentChooseView, level,GameDataManager.Instance.Exp).Forget();  
            });
        }

        public void PopupRequestTalentList()
        {
            if (requestTalentList.Count > 0)
            {
                requestTalentList.RemoveAt(0);
            }
        }
    }
}
