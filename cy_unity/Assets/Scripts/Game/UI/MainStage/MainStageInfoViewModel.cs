using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;
using Game.UI;
using Game.UI.Guide;
using Game.UI.MainUi;
using UnityEngine;
using Object = System.Object;


namespace DH.Game.ViewModels
{
    public partial class MainStageInfoViewModel : ViewModelBase
    {
		[AutoNotify] private bool isShowLeftTopNode  = true;
		[AutoNotify] private bool isShowRightTopNode = true;
		[AutoNotify] private bool isShowLeftBtn;
		[AutoNotify] private bool isShowRightBtn;
		[AutoNotify] private ItemPriceNodeModel battleBtnCostVm;
		[AutoNotify] private ObservableDictionary<int, MainStageBoxItemViewModel> boxDictionary = new();
		[AutoNotify] private ChapterCellViewModel chapterCellVm;
		[AutoNotify] private string battleTextStr;
		[AutoNotify] private bool isShowCostView;
		[AutoNotify] private bool isShowSecretBattleBtn = true;
		[AutoNotify] private bool isShowSecretBtnLockNode;
		public Func<Object, object> GetBoxVmCallBack => GetBoxVmByIndex;
		[AutoNotify] private bool functionMenuRed;
		public RectTransform FunctionMenuTransform;
		public MainUiManager Manager => MainUiManager.Instance;
		[AutoNotify] private bool isShowPatrolBtn = true;
		[AutoNotify] private bool patrolRewardButRed;
		[AutoNotify] private bool isShowSecretRedDot;
		
        [Preserve]
        public MainStageInfoViewModel()
        {
	        UpdateChapterInfo();
	        UpdateLeftBtnAndRightBtnState();
	        UpdateBattleBtnInfo();
	        UpdateBoxList();
	        InitPupUpInfo();
	        UpdateSecretBtnRedDot();
	        var chapterId = MainUiManager.Instance.GetChapterId(MainUiManager.Instance.ShowChapterIndex);
	        chapterCellVm = new(chapterId, OnClickChapterCellBtn);
	        MainUiManager.Instance.PropertyChanged += OnMainUiManagerChanged;
	        DataCenter.charcaterData.Digest.PropertyChanged += PlayerLevelChange;
	        DataCenter.triggerGiftData.PropertyChanged += TriggerGiftDataChange;
	        DataCenter.charcaterData.PropertyChanged += SubscribeGiftChange;

	        InitRightBut();
	        InitLeftBut();
	        InitLeftToptBut();
	        UpdataFunc();
	        CheckLuckyIsValid();
	        CheckMagicIsValid();
	        CheckActivityIsUnlocked();
	        PatrolRewardRed();
	        CheckCollegeIsValid();
	        CheckLuckTravelIsValid();
	        CheckFreeBuyIsValid();
	        var enterCount = GuideManager.Instance.EnterCount;
	        if (enterCount < 1 &&DataCenter.mainStageData.CurrChapter == 1)
	        {
		        GuideManager.Instance.EnterCount = 1;
		        OnClickBattleBtn();
	        }

	        IsShowSecretBtnLockNode = !MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionSecret);

        }

        protected override void OnDispose()
        {
	        MainUiManager.Instance.PropertyChanged -= OnMainUiManagerChanged;
	        DataCenter.charcaterData.Digest.PropertyChanged -= PlayerLevelChange;
	        DataCenter.triggerGiftData.PropertyChanged -= TriggerGiftDataChange;
	        DataCenter.charcaterData.PropertyChanged -= SubscribeGiftChange;
	        BattleBtnCostVm?.Dispose();
	        base.OnDispose();
        }

        private void TriggerGiftDataChange(object sender, PropertyChangedEventArgs e)
        {
	        CheckTriggerGift();
        }

        private void OnMainUiManagerChanged(object sender, PropertyChangedEventArgs e)
        {
	        if(e.PropertyName== nameof(MainUiManager.Instance.ShowChapterIndex))
	        {
		        UpdateLeftBtnAndRightBtnState();
		        UpdateBattleBtnInfo();
		        UpdateChapterInfo();
	        }
        }

        [Command]
        private void OnClickLeftBtn()
        {
	        if(MainUiManager.Instance.ShowChapterIndex == 0) return;
	        MainUiManager.Instance.ShowChapterIndex -= 1;
	        AudioManager.Instance.Play(AudioType.ExchangeChapter);
        }

        [Command]
        private void OnClickRightBtn()
        {
	        var maxIndex = DataCenter.mainStageData.ChapterCfgs.Count -1;
	        if(MainUiManager.Instance.ShowChapterIndex >= maxIndex) return;
	        
	        MainUiManager.Instance.ShowChapterIndex += 1;
	        AudioManager.Instance.Play(AudioType.ExchangeChapter);
        }
        
        [Command]
        private async void OnClickBattleBtn()
        {
	        if (EquipManager.Instance.IsExistNoneSlots())
	        {
		        
		        var titleStr = LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_Title);
		        var contentStr =  LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips32);
		        var cancleStr =  LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_CancelTxt);
		        var conformStr = LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_ConfirmTxt);
		        Action cancleFunc = () =>
		        {
			        
		        };
		        Action confirmFunc = () =>
		        {
			        JumpManager.Instance.Jump(FunctionJumpCfgId.go_MainEquip);
		        };
		        CommonMessageBoxViewModel tempVm = new(titleStr,contentStr,cancleStr,conformStr,cancleFunc,confirmFunc, null);
		        UIManager.Instance.OpenDialog<CommonMessageBox>(tempVm).Forget();
		        
		        // ToastManager.ShowLanguage(GlobalLanguageId.Equip_16);
		        return;
	        }

	        var chapterId = MainUiManager.Instance.GetChapterId(MainUiManager.Instance.ShowChapterIndex);
	        // 检查资产是否足够
	        var copyCfg = ConfigCenter.CopyCfgColl.GetDataById(chapterId);
	        var costRes = new Reward(RewardType.Lives, (int)GameConst.ItemIdCode.EnergyDrink, copyCfg.Expend);
	        if (!UIHelper.CheckRewardIsEnough(costRes,isJump:true))
	        {
		        return;
	        }
	        // 点击开始
	       Manager.RequestEnterGame(chapterId);
        }
        /// <summary>
        /// 是否有存档
        /// </summary>
        private void CheckIsHasAchieve()
        {
	        
	        if (DataCenter.mainStageData.HasArchive)
	        {
		        PopUpManager.Instance.AddPopUp(() =>
		        {
			        var titleStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips24);
			        var contentStr =  LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips25);
			        var cancelStr =  LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_CancelTxt);
			        var confirmStr =  LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips26);
			        Action cancelCallback = () =>
			        {
				        UIManager.Instance.CloseDialog<CommonMessageBox>();
				        RequestEnterArchive(false).Forget();
				        UpdateBattleBtnInfo();
			        };
			        Action confirmCallback = () =>
			        {
				        RequestEnterArchive(true).Forget();
			        };
			        CommonMessageBoxViewModel tempVm = new(titleStr,contentStr,cancelStr,confirmStr,cancelCallback,confirmCallback,null);
			        UIManager.Instance.OpenDialog<CommonMessageBox>(tempVm).Forget(); 
		        }, 10000);
	        }
	        else if (DataCenter.endlessData.HasArchive)//无尽关卡是否有存档
	        {
		        EnterEndlessGameByArchive();
	        }
	        else if (DataCenter.dailyFightData.HasArchive)//每日挑战是否有存档
	        {
		        EnterDailyFightGameByArchive();
	        } 
	        else if (DataCenter.secretData.HasArchive) // 秘林 是否有存档
	        {
		        EnterSecretGameByArchive();
	        }
        }
        private async UniTaskVoid RequestEnterArchive(bool useArchive)
        {
	        var req = new ReqMainFightArchiveBegin();
	        var result = await GameNetworkManager.Instance.SendAsync<RspMainFightArchiveBegin>(req);
	        if (result.rsp is not { Status: 0 })
	        {
		        if(result.rsp == null) return;
		        var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
		        ToastManager.Show(str);
		        return;
	        }
	        if (useArchive)
	        {
		        GameManager.Instance.EnterGame(EStateType.StageTypeMainStage, result.rsp.StageId, result.rsp.Data);
	        }
	        else
	        {
		        GameDataManager.Instance.SetFightData(result.rsp.Data);
		        GameDataManager.Instance.CurChapterId = result.rsp.StageId;
		        GameDataManager.Instance.CurStageType = EStateType.StageTypeMainStage;
		        PlayerStats.Instance.Init();
		        PlayerStats.Instance.KillCount = GameDataManager.Instance.killedCount;
		        PlayerStats.Instance.SkillHurtsDic.Clear();
		        foreach (var item in GameDataManager.Instance.HurtStat)
		        {
			        PlayerStats.Instance.SkillHurtsDic.Add(item.Key, item.Value);
		        }
		        // 主动结算
		        GameManager.Instance.OnGameEnd(false,false,false, false);
		        DataCenter.mainStageData.HasArchive = false;
	        }
        }
        /// <summary>
        /// 进入无尽关卡游戏存档
        /// </summary>
        private void EnterEndlessGameByArchive()
        {
	        PopUpManager.Instance.AddPopUp(() =>
	        {
		        var titleStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips24);
		        var contentStr =  LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips25);
		        var cancelStr =  LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_CancelTxt);
		        var confirmStr =  LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips26);
		        Action cancelCallback = () =>
		        {
			        UIManager.Instance.CloseDialog<CommonMessageBox>();
			        RequestEnterEndlessArchive(false).Forget();
		        };
		        Action confirmCallback = () =>
		        {
			        RequestEnterEndlessArchive(true).Forget();
		        };
		        var tempVm = new CommonMessageBoxViewModel(titleStr,contentStr,cancelStr,confirmStr,cancelCallback,confirmCallback,null);
		        UIManager.Instance.OpenDialog<CommonMessageBox>(tempVm).Forget(); 
	        }, 10000);
        }
        /// <summary>
        /// 是否走请求存档战斗
        /// </summary>
        /// <param name="useArchive"></param>
        private async UniTaskVoid RequestEnterEndlessArchive(bool useArchive)
        {
	        var req = new ReqEndlessArchiveBegin();
	        var result = await GameNetworkManager.Instance.SendAsync<RspEndlessArchiveBegin>(req);
	        if (result.rsp is not { Status: 0 })
	        {
		        if(result.rsp == null) return;
		        var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
		        ToastManager.Show(str);
		        return;
	        }
	        if (useArchive)//使用存档
	        {
		        GameManager.Instance.EnterGame(EStateType.StageTypeEndless,DataCenter.mainStageData.GetMaxPassChapter(),result.rsp.Data);
	        }
	        else//主动结算
	        {
		        GameDataManager.Instance.SetFightData( result.rsp.Data);
		        GameDataManager.Instance.CurStageType = EStateType.StageTypeEndless;
		        PlayerStats.Instance.KillCount = GameDataManager.Instance.killedCount;
		        GameManager.Instance.CostTime =  GameDataManager.Instance.SurvivalTime;//生存时间；秒
		        // 主动结算
		        GameManager.Instance.OnGameEnd(false,false,false, false);
		        DataCenter.endlessData.HasArchive = false;
		        PlayerStats.Instance.Hp = GameDataManager.Instance.Hp;
	        }
        }

        /// <summary>
        /// 进入每日挑战游戏存档
        /// </summary>
        private void EnterDailyFightGameByArchive()
        {
	        PopUpManager.Instance.AddPopUp(() =>
	        {
		        var titleStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips24);
		        var contentStr =  LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips25);
		        var cancelStr =  LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_CancelTxt);
		        var confirmStr =  LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips26);
		        Action cancelCallback = () =>
		        {
			        UIManager.Instance.CloseDialog<CommonMessageBox>();
			        RequestEnterDailyFightArchive(false).Forget();
		        };
		        Action confirmCallback = () =>
		        {
			        RequestEnterDailyFightArchive(true).Forget();
		        };
		        var tempVm = new CommonMessageBoxViewModel(titleStr,contentStr,cancelStr,confirmStr,cancelCallback,confirmCallback,null);
		        UIManager.Instance.OpenDialog<CommonMessageBox>(tempVm).Forget(); 
	        }, 10000);
        }

        private void EnterSecretGameByArchive()
        {
	        if (ServerTime.Instance.RemainTime(DataCenter.secretData.SeasonTime) < GameConst.SecretShowTipsTime)
	        {
		        return;
	        }
	        
	        PopUpManager.Instance.AddPopUp(() =>
	        {
		        var titleStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips24);
		        var contentStr =  LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips25);
		        var cancelStr =  LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_CancelTxt);
		        var confirmStr =  LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips26);
		        Action cancelCallback = () =>
		        {
			        var leftTime = ServerTime.Instance.RemainTime(DataCenter.secretData.SeasonTime);
			        if (leftTime < GameConst.SecretShowTipsTime)
			        {
				        var tipsStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Secret_18);
				        ToastManager.Show(tipsStr);
				        UIManager.Instance.CloseDialog<CommonMessageBox>();
				        return;
			        }
			        UIManager.Instance.CloseDialog<CommonMessageBox>();
			        ReqEnterSecretFightArchiveBegin(false).Forget();
		        };
		        Action confirmCallback = () =>
		        {
			        var leftTime = ServerTime.Instance.RemainTime(DataCenter.secretData.SeasonTime);
			        if (leftTime < GameConst.SecretShowTipsTime)
			        {
				        var tipsStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Secret_18);
				        ToastManager.Show(tipsStr);
				        UIManager.Instance.CloseDialog<CommonMessageBox>();
				        return;
			        }
			        
			        ReqEnterSecretFightArchiveBegin(true).Forget();
		        };
		        var tempVm = new CommonMessageBoxViewModel(titleStr,contentStr,cancelStr,confirmStr,cancelCallback,confirmCallback,null);
		        UIManager.Instance.OpenDialog<CommonMessageBox>(tempVm).Forget(); 
	        }, 10000);
        }


        private async UniTaskVoid ReqEnterSecretFightArchiveBegin(bool useArchive)
        {
	        var result = await GameNetworkManager.Instance.SendAsync<RspSecretFightArchiveBegin>(new ReqSecretFightArchiveBegin());
	        if (result.rsp is not { Status: 0 })
	        {
		        if(result.rsp == null) return;
		        var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
		        ToastManager.Show(str);
		        return;
	        }

	        if (useArchive) //使用存档
	        {
		        GameManager.Instance.EnterGame(EStateType.StageTypeSecret,result.rsp.StageId, result.rsp.Data);
	        }
	        else
	        {
		        GameDataManager.Instance.CurFightData = result.rsp.Data;
		        PlayerStats.Instance.Init();
		        GameDataManager.Instance.CurStageType = EStateType.StageTypeSecret;
		        PlayerStats.Instance.KillCount = GameDataManager.Instance.killedCount;
		        GameManager.Instance.CostTime =  GameDataManager.Instance.SurvivalTime;//生存时间；秒
		        GameDataManager.Instance.CurChapterId = result.rsp.StageId;
		        PlayerStats.Instance.Hp = GameDataManager.Instance.Hp;
		        PlayerStats.Instance.SkillHurtsDic.Clear();
		        foreach (var item in GameDataManager.Instance.HurtStat)
		        {
			        PlayerStats.Instance.SkillHurtsDic.Add(item.Key, item.Value);
		        }
		        // 主动结算
		        GameManager.Instance.OnGameEnd(false,false,false, false);
		        DataCenter.secretData.HasArchive = false;
	        }
        }

        /// <summary>
        /// 是否走请求存档战斗
        /// </summary>
        /// <param name="useArchive"></param>
        private async UniTaskVoid RequestEnterDailyFightArchive(bool useArchive)
        {
	        var result = await GameNetworkManager.Instance.SendAsync<RspDailyFightArchiveBegin>(new ReqDailyFightArchiveBegin());
	        if (result.rsp is not { Status: 0 })
	        {
		        if(result.rsp == null) return;
		        var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
		        ToastManager.Show(str);
		        return;
	        }
	        if (useArchive)//使用存档
	        {
		        if (result.rsp.Buff is { Count: > 0 })
		        {
			        DataCenter.dailyFightData.Buff.Clear();
			        for (var i = 0; i < result.rsp.Buff.Count; i++)
			        {
				        DataCenter.dailyFightData.Buff.Add(result.rsp.Buff[i]);
			        }
		        }
		        if (result.rsp.Mons is { Count: > 0 })
		        {
			        DataCenter.dailyFightData.Mons.Clear();
			        foreach (var monsterInfo in result.rsp.Mons)
			        {
				        DataCenter.dailyFightData.Mons.Add(monsterInfo.Key,monsterInfo.Value);
			        }
		        }
		        GameManager.Instance.EnterGame(EStateType.StageTypeChallenge,DataCenter.mainStageData.GetMaxPassChapter(),result.rsp.Data);
	        }
	        else//主动结算
	        {
		        GameDataManager.Instance.SetFightData(result.rsp.Data);
		        PlayerStats.Instance.Init();
		        GameDataManager.Instance.CurStageType = EStateType.StageTypeChallenge;
		        PlayerStats.Instance.KillCount = GameDataManager.Instance.killedCount;
		        GameManager.Instance.CostTime =  GameDataManager.Instance.SurvivalTime;//生存时间；秒
		        PlayerStats.Instance.SkillHurtsDic.Clear();
		        foreach (var item in GameDataManager.Instance.HurtStat)
		        {
			        PlayerStats.Instance.SkillHurtsDic.Add(item.Key, item.Value);
		        }
		        // 主动结算
		        GameManager.Instance.OnGameEnd(false,false,false, false);
		        DataCenter.dailyFightData.HasArchive = false;
		        PlayerStats.Instance.Hp = GameDataManager.Instance.Hp;
	        }
        }
        private void OnClickChapterCellBtn(int chapterId)
        {
	        // 点击章节cell 的回调
	        DHLog.Debug($"点击 章节 cell 章节 id {chapterId} ");
        }

        private void UpdateLeftBtnAndRightBtnState()
        {
	        IsShowLeftBtn = MainUiManager.Instance.ShowChapterIndex > 0;
	        IsShowRightBtn = MainUiManager.Instance.ShowChapterIndex < DataCenter.mainStageData.ChapterCfgs.Count - 1;
        }
        
        private void UpdateBattleBtnInfo()
        {
	        var chapterId = MainUiManager.Instance.GetChapterId(MainUiManager.Instance.ShowChapterIndex);
	        var chapterInfo = DataCenter.mainStageData.GetChapterInfo(chapterId);
	        if(chapterInfo == null) return;
	        
	        var copyCfg = ConfigCenter.CopyCfgColl.GetDataById(chapterId);


	        if (DataCenter.mainStageData.HasArchive)
	        {
		        IsShowCostView = false;
	        }
	        else
	        {
		        IsShowCostView = true;

		        if (BattleBtnCostVm != null)
		        {
			        BattleBtnCostVm.Dispose();
		        }
		        var battleCost = new Reward(RewardType.Lives, (int)GameConst.ItemIdCode.EnergyDrink, copyCfg.Expend);
		        BattleBtnCostVm = new(battleCost);
		        BattleBtnCostVm.IsShowBg = false;
	        }
        }

        private void UpdateBoxList()
        {
	        foreach (var item in BoxDictionary)
	        {
		        item.Value.Dispose();
	        }
	        BoxDictionary.Clear();
	        var boxId = DataCenter.mainStageData.GetUnClaimedChapterBoxId();
	        if (boxId == 0)
	        {
		        var cfg = ConfigCenter.CopyCfgColl.GetDataById(DataCenter.mainStageData.CurrChapter);
		        var tempVm = new MainStageBoxItemViewModel(cfg,true,3, OnClickChapterBoxBtn);
		        tempVm.IsShowTips = true;
		        BoxDictionary.Add(0,tempVm);
	        }
	        else
	        {
		        var boxIndex = DataCenter.mainStageData.GetBoxIndexByBoxId(boxId);
		        var chapterId = DataCenter.mainStageData.GetChapterIdByBoxId(boxId);
		        var cfg = ConfigCenter.CopyCfgColl.GetDataById(chapterId);
		        var tempVm = new MainStageBoxItemViewModel(cfg,true,boxIndex, OnClickChapterBoxBtn);
		        tempVm.IsShowTips = true;
		        BoxDictionary.Add(0,tempVm);
	        }
        }
        private object GetBoxVmByIndex(object index)
        {
	        if (BoxDictionary.TryGetValue((int)index, out MainStageBoxItemViewModel ret))
	        {
		        return ret;
	        }
	        return null;
        }

        public async void OnClickChapterBoxBtn(int chapterId, int index, Action<List<Resource>> callback)
        {
	        var req = new ReqMainFightBox();

	        req.ChapterId = chapterId;
	        req.BoxIndex = index;
	        var result = await GameNetworkManager.Instance.SendAsync<RspMainFightBox>(req);
	        if (result.rsp is not { Status: 0 })
	        {
		        if(result.rsp == null) return;
		        var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
		        ToastManager.Show(str);
		        return;
	        }
	        MainChapter chapterInfo = DataCenter.mainStageData.GetChapterInfo(chapterId);
	        // 修改宝箱状态
	        chapterInfo.BoxClaimStatus = Lodash.ParseStateValue(chapterInfo.BoxClaimStatus, index);
	        List<Resource> tempResourse = new List<Resource>();
	        foreach (var itme in result.rsp.Rewards)
	        {
		        Resource tempRes = new();
		        tempRes.Type = itme.Type;
		        tempRes.Count = itme.Count;
		        tempRes.Id = itme.Id;
		        tempRes.HeroEquip = itme.HeroEquip;
		        tempResourse.Add(tempRes);
	        }
	        callback?.Invoke(tempResourse);
	        // 加奖励
	        Lodash.DealRewards(result.rsp.Rewards.ToList(), true);
	        UpdateBoxList();
        }
        

        private void UpdateChapterInfo()
        {
	        var chapterId = MainUiManager.Instance.GetChapterId(MainUiManager.Instance.ShowChapterIndex);
	        if(chapterCellVm == null) return;
	        chapterCellVm.ChapterId = chapterId;
	        UpdateBoxList();
        }

        private void UpdateSecretBtnRedDot()
        {
	        var leftTime = ServerTime.Instance.RemainTime(DataCenter.secretData.SeasonTime);
	        if (leftTime < 0)
	        {
		        IsShowSecretRedDot = false;
		        return;
	        }
	        
	        var info = DataCenter.secretData.StageInfo;
	        foreach (var item in info)
	        {

		        if (item.Value.Pass && !item.Value.Claim)
		        {
			        IsShowSecretRedDot = true;
			        return;
		        }
	        }
	        IsShowSecretRedDot = false;
        }
        private void InitPupUpInfo()
        {
	        if (GameConst.IsIosAuditState)
	        {
		        return;
	        }
	        // 检查是的有重连
	        CheckIsHasAchieve();
	        GameManager.Instance.CheckGuidePositiveReviewsView();
	        CheckTriggerGift();
	        CheckSecretIsOpen();
	        PopUpManager.Instance.CheckAndAddFunctionOpenPopUpWindows();
	        CheckNewbieView();
	        CheckBoosterPackView();
	        CheckInvitedView();
        }

        private void CheckSecretIsOpen()
        {
	        bool isPopUp = PopUpManager.Instance.GetFunctionOpenPopUpIsPopUp(EFunctionOpenType.FunctionSecret);
	        if (!isPopUp && MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionSecret))
	        {
		        PopUpManager.Instance.AddPopUp(() =>
		        {
			        PopUpManager.Instance.SetFunctionOpenPopUpIsPopUp(EFunctionOpenType.FunctionSecret, true);
			        UIManager.Instance.OpenDialog<SecretOpenView,SecretOpenViewModel>().Forget();
		        }, 100);
	        }
        }

        private void CheckNewbieView()
        {
	        bool isPopUp = PopUpManager.Instance.GetFunctionOpenPopUpIsPopUp(EFunctionOpenType.FunctionTypeNewPackage);
	        if (!isPopUp && MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionTypeNewPackage))
	        {
		        PopUpManager.Instance.AddPopUp(() =>
		        {
			        PopUpManager.Instance.SetFunctionOpenPopUpIsPopUp(EFunctionOpenType.FunctionTypeNewPackage, true);
			        UIManager.Instance.OpenDialog<NewbieView,NewbieViewModel>().Forget();
		        }, 300);
	        }
        }

        public void CheckBoosterPackView()
        {
	        if(GameManager.Instance.IsWin) return;
	        
	        var typeCfgs = ConfigCenter.TriggerGiftTypeCfgColl.GetDataByTrigger(7);
	        foreach (var typeCfg in typeCfgs)
	        {
		        if (typeCfg.TriggerNum == null || typeCfg.TriggerNum.Count < 2) continue;
		        // 判断是否在指定章节
		        if (DataCenter.mainStageData.CurrChapter < typeCfg.TriggerNum[0]
		            || DataCenter.mainStageData.CurrChapter > typeCfg.TriggerNum[1])
			        continue;
		        // 判断是否已经弹出过
		        if (DataCenter.triggerGiftData.CheckIsPopupBoosterPack(typeCfg.Id)) continue;
		        
		        // 检查是否在有效期内
		        var endTime = DataCenter.triggerGiftData.GetTriggerGiftEndTime(typeCfg.Id);
		        var leftTime = ServerTime.Instance.RemainTime(endTime);
		        if(leftTime <= 0) continue;
		        
		        if(!DataCenter.triggerGiftData.CheckIsCanOp(typeCfg.Id)) continue;
		        
		        BoosterPackViewModel tempVm = new(typeCfg.Id);
		        UIManager.Instance.OpenDialog<BoosterPackView>(tempVm).Forget();
		        // 修改状态为已弹出
		        DataCenter.triggerGiftData.SetBoosterPackInfo(typeCfg.Id,true);
		        break;
	        }
        }

        /// <summary>
        /// 分享弹窗
        /// </summary>
        private void CheckInvitedView()
        {
	        bool isPopUp = PopUpManager.Instance.GetFunctionOpenPopUpIsPopUp(EFunctionOpenType.InviterAndShare);
	        if (!isPopUp && MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.InviterAndShare))
	        {
		        PopUpManager.Instance.AddPopUp(() =>
		        {
			        PopUpManager.Instance.SetFunctionOpenPopUpIsPopUp(EFunctionOpenType.InviterAndShare, true);
			        UIManager.Instance.OpenDialog<InvitedView,InvitedViewModel>().Forget();
		        }, 0);
	        }
        }


        private void CheckTriggerGift()
        {
	        var typeList = TriggerGiftManager.Instance.GetSatisfyConditionsTypeList();
	        foreach (var item in typeList)
	        {
		        if (TriggerGiftManager.Instance.CheckTriggerTypeFirst(item))
		        {
			        var showId = TriggerGiftManager.Instance.GetShowTypeFirstTriggerGift(item);
			        if(showId==0 ) continue;
			        var triggerCfg = ConfigCenter.TriggerGiftCfgColl.GetDataById(showId); 
			        var giftTypeCfg = ConfigCenter.TriggerGiftTypeCfgColl.GetDataById(triggerCfg.Type); 
			        if(giftTypeCfg.Trigger == 1) 
			        {
				        TriggerGiftManager.Instance.SetTriggerGiftRed();
				        TriggerGiftManager.Instance.SaveTriggerTypeFirst(triggerCfg.Type);
						continue;
			        }
			        TriggerGiftManager.Instance.AddMainAddPopList(item);
			        PopUpManager.Instance.AddPopUp(() =>
			        {
				        var cfg = ConfigCenter.TriggerGiftCfgColl.GetDataById(showId); 
				        if (cfg.Type == 6)
				        {
					        UIManager.Instance.OpenDialog<AdFreeGiftView>(new AdFreeGiftViewModel()).Forget();
				        }
				        else
				        {
					        var tempVm = new TriggerGiftDialogViewModel(showId);
					        UIManager.Instance.OpenDialog<TriggerGiftDialogView>(tempVm).Forget();
					        DataCenter.triggerGiftData.IsPopup = false;
				        }

			        }, 100);
		        }
	        }

	        AddTriggerButton();
        }

        [Command]
        private void OnClickSettingBtn()
        {
	        var vm = new FunctionMenuViewModel();
	        vm.Pos = FunctionMenuTransform.position;
	        vm.Offset = new Vector3(-150, -270, 0);
	        UIManager.Instance.OpenDialog<FunctionMenuView>(vm).Forget();
        }

        [Command]
        private void OnClickSecretBattleBtn()
        {
	        bool isOpen = MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionSecret);
	        if (isOpen != true)
	        {
		        var tips = MainUiManager.Instance.GetFunctionUnLockTips(EFunctionOpenType.FunctionSecret);
		        ToastManager.Show(tips);
		        return;
	        }
	        
	        // 检查时间
	        var leftTime = ServerTime.Instance.RemainTime(DataCenter.secretData.SeasonTime);
	        if (leftTime < 0)
	        {
		        var tips = LocalizeHelper.GetGlobal(GlobalLanguageId.Secret_19);
		        ToastManager.Show(tips);
		        return;
	        }
	        UIManager.Instance.OpenDialog<SecretView,SecretViewModel>().Forget();
        }

        #region 巡逻相关

        /// <summary>
        /// 巡逻
        /// </summary>
        [Command]
        private async void OnClickPatrolRewardBtn()
        {
	        if (!MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctioonTypeFarm))
	        {			    
		        var str = MainUiManager.Instance.GetFunctionUnLockTips(EFunctionOpenType.FunctioonTypeFarm);
		        ToastManager.Show(str);
		        return;
	        }
	        UIManager.Instance.OpenDialog<PatrolRewardView,PatrolRewardViewModel>().Forget();
        }
        
        /// <summary>
        /// 巡逻收益红点
        /// </summary>
        private void PatrolRewardRed()
        {
	        PatrolRewardButRed = DataCenter.mainStageData.Hangup.IsRed() && MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctioonTypeFarm);
        }

        #endregion


        #region 初始化左侧按钮

        private void InitLeftBut()
        {
	        Manager.LeftButItems.Clear();
	        
	        if(GameConst.IsIosAuditState) return;

	        //月卡按钮
	        bool isOpenMonthCard = MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.MonthCard);
	        if (isOpenMonthCard)
	        {
		        var name = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.MonthlyVip_tips01).Name;
		        Manager.AddLeftBut(MainStageInfoNodeRightButType.MonthCard,"mainui[icon_home_2]",name,
			        (objs) =>
			        {		
				       UIManager.Instance.OpenDialog<MonthCardView>(new MonthCardViewModel()).Forget();
			        },
			        DataCenter.monthCardData.IsCanGetAward
		        );
	        }
	        

	        // 排行榜
	        bool isOpenRank = MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionTypeRank);
	        if (isOpenRank)
	        {
		        var name = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips12);
		        Manager.AddLeftBut(MainStageInfoNodeRightButType.Rank,"mainui[icon_home_1]",name,
			        (objs) =>
			        {
				        RankViewModel tempVm= new RankViewModel(ERankType.RankItemMainStage);
				        UIManager.Instance.OpenDialog<RankView>(tempVm).Forget();
			        },
			        ()=>false);
	        }
	        
	        AddTriggerButton();
	        AddInvitedBtn();

        }
        

        #endregion
        
        #region 初始化右侧按钮

        private void InitRightBut()
        {
	        Manager.RightButItems.Clear();
	        // 计费点
	        if (GameConst.IsIosAuditState)
	        {
		        var name = "Packages"; //LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips16);
		        Manager.AddRightBut(MainStageInfoNodeRightButType.RechargePoint,"mainui[icon_home_2]",name,
			        (objs) =>
			        {
				        UIManager.Instance.OpenDialog<RechargePointView, RechargePointViewModel>().Forget();
			        },() => false
		        );
		        return;
	        }
	        
	        //新手礼包
	        bool isOpen = MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionTypeNewPackage);
	        if (isOpen && DataCenter.newBieData!=null && !DataCenter.newBieData.Isover())
	        {
		        var name = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.Xinshou_01).Name;
		        Manager.AddRightBut(MainStageInfoNodeRightButType.Newbie,"mainui[icon_home_3]",name,
			        (objs) => {UIManager.Instance.OpenDialog<NewbieView>(new NewbieViewModel()).Forget();},DataCenter.newBieData.IsCnaGetAward);
	        }
	        // 通行证
	        bool isPassportsOpen = MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionTypePassports);
	        if (isPassportsOpen && DataCenter.allPassportData !=null && DataCenter.allPassportData.CheckIsVail())
	        {
		        var name = LocalizeHelper.GetGlobal(GlobalLanguageId.Tehuijijin_07);
		        Manager.AddRightBut(MainStageInfoNodeRightButType.Passports,"mainui[icon_home_5]",name,
			        (objs) => {UIManager.Instance.OpenDialog<PassportsMainView,PassportsMainViewModel>().Forget();},PassportAllRed);
	        }
	        //每日特惠
	        bool isDailySpecialOffersOpen = MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.DailySpecialOffers);
	        if (isDailySpecialOffersOpen)
	        {
		        var name = LocalizeHelper.GetGlobal(GlobalLanguageId.dailySpecialPackage_tips_01);
		        Manager.AddRightBut(MainStageInfoNodeRightButType.DailySpecialOffers,"mainui[icon_home_4]",name,
			        (objs) => {UIManager.Instance.OpenDialog<DailySpecialOffersView,DailySpecialOffersViewModel>().Forget();},DataCenter.dailyPackData.IsCanGetFree);
	        }
	        
	        // //关注有礼
	        // if ( DataCenter.charcaterData.IsShowSubscribeGift())
	        // {
		       //  var name = LocalizeHelper.GetGlobal(GlobalLanguageId.GiftCode_tips01);
		       //  Manager.AddRightBut(MainStageInfoNodeRightButType.SubscribeGift,"mainui[icon_home_4]",name,
			      //   (objs) => {UIManager.Instance.OpenDialog<SubscribeGiftView,SubscribeGiftViewModel>().Forget();}, DataCenter.charcaterData.IsShowSubscribeGiftRed);
	        // }
	        
	        // 魔法时代
	        UpdateMagicEraBtnState();
	        
	        
	        //幸运扭蛋
	        UpdateLuckyEggBtnState();
	        
	        // 助力礼包
	        CheckIsShowTBoosterPackBtn();

	        //金秋特惠
	        UpdateAutumnSpecialBtnState();

	        //魔法宾果
	        UpdateMagicBingoBtnState();

        }
		/// <summary>
		/// 通行证总红点
		/// </summary>
		/// <returns></returns>
        private bool PassportAllRed()
        {
	        return DataCenter.allPassportData.CheckIsShowRedDot() ||(DataCenter.chapterFundData.IsRed() &&
			        MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionChapterFund));
        }


        private void AddTriggerButton()
        {
	        if(GameConst.IsIosAuditState)return;
	        if (TriggerGiftManager.Instance.IsOpenTriggerGiftShop())
	        {
		        var name = LocalizeHelper.GetGlobal(GlobalLanguageId.Trigger03);
		        Manager.AddLeftBut(MainStageInfoNodeRightButType.TriggerGift,"mainui[icon_home_8]",name,
			        (objs) => {UIManager.Instance.OpenDialog<TriggerGiftView,TriggerGiftViewModel>().Forget();},
			        showRed: TriggerButton);
	        }
	        else
	        {
		        Manager.RemoveLeftBut(MainStageInfoNodeRightButType.TriggerGift);
	        }
        }

        private void AddInvitedBtn()
        {
	        return;
	        if(!MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.InviterAndShare)) return;
	        
	        var cfgs = ConfigCenter.ShareRewardProgressCfgColl.DataItems;
	        bool isShow = false;
	        foreach (var cfg in cfgs)
	        {
		        if (cfg.Value1 > DataCenter.charcaterData.InviteNumber)
		        {
			        isShow = true;
			        break;
		        }

		        if (!DataCenter.charcaterData.CheckIsClaimedInvitedReward(cfg.Id))
		        {
			        isShow = true;
			        break;
		        }
	        }

	        if (!isShow)
	        {
		        Manager.RemoveLeftBut(MainStageInfoNodeRightButType.Invited);
	        }
	        else
	        {
		        var name = LocalizeHelper.GetGlobal(GlobalLanguageId.Share_Tips_01);
		        Manager.AddLeftBut(MainStageInfoNodeRightButType.Invited,"mainui[icon_home_8]",name,
			        (objs) => {UIManager.Instance.OpenDialog<InvitedView,InvitedViewModel>().Forget();},
			        showRed: DataCenter.charcaterData.IsShowInvitedRedDot);
	        }
        }

        private bool TriggerButton()
        {
	        return TriggerGiftManager.Instance.IsRed() ||
	               DataCenter.triggerGiftData.IsProgressRed();
        }

        private void CheckCollegeIsValid()
        {
	        if (MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.College) && !CollegeActivityManager.Instance.CheckEndTime())
	        {
		        var name = MainUiManager.Instance.GetFunctionName(EFunctionOpenType.College);
		        Manager.AddRightBut(MainStageInfoNodeRightButType.College,"mainui[icon_home_10]",name,
			        (objs) => {UIManager.Instance.OpenDialog<CollegeActivityView,CollegeActivityModel>().Forget();},CollegeActivityManager.Instance.CheckCollegeRedDot);
	        }
	        else
	        {
		        Manager.RemoveRightBut(MainStageInfoNodeRightButType.College);
	        }
        }
        
        private void CheckLuckTravelIsValid()
        {
	        if ( MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.LuckTravel) && ActivityUIManager.Instance.CheckLuckTravelOpen())
	        {
		        var name = MainUiManager.Instance.GetFunctionName(EFunctionOpenType.LuckTravel);
		        Manager.AddRightBut(MainStageInfoNodeRightButType.LuckTravel,"mainui[icon_home_13]",name,
			        (objs) => {UIManager.Instance.OpenDialog<LuckTravelView,LuckTravelViewModel>().Forget();},ActivityUIManager.Instance.CheckLuckTravelRed);
	        }
	        else
	        {
		        Manager.RemoveRightBut(MainStageInfoNodeRightButType.LuckTravel);
	        }
        }
        
        private void CheckFreeBuyIsValid()
        {
	        if (MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.ActivityFreeBuy) && !ActivityUIManager.Instance.CheckAllGetReward())
	        {
		        var name = MainUiManager.Instance.GetFunctionName(EFunctionOpenType.ActivityFreeBuy);
		        Manager.AddLeftBut(MainStageInfoNodeRightButType.ActivityFreeBuy,"mainui[icon_home_15]",name,
			        (objs) => {UIManager.Instance.OpenDialog<FreeBuyActivityView,FreeBuyActivityViewModel>().Forget();},ActivityUIManager.Instance.CheckFreeBuyRed);
	        }
	        else
	        {
		        Manager.RemoveLeftBut(MainStageInfoNodeRightButType.ActivityFreeBuy);
	        }
        }
        #region 魔法时代按钮状态

        private void UpdateMagicEraBtnState()
        {
	        // 魔法时代
	        bool isOpenMagicEra= MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.MagicEra);
	        var isTime = ServerTime.Instance.GetNowTime() >= DataCenter.magicAgeData.StartStamp && ServerTime.Instance.GetNowTime() < DataCenter.magicAgeData.EndStamp;
	        var IsBuyOver = DataCenter.magicAgeData.IsBuyOver();
	        if (isOpenMagicEra && isTime && !IsBuyOver)
	        {
		        var name =  ConfigCenter.FunctionOpenLanguageCfgColl.GetDataById((int)EFunctionOpenType.MagicEra).Name;
		        Manager.AddRightBut(MainStageInfoNodeRightButType.MagicEr,"mainui[icon_home_14]",name,
			        (objs) =>
			        {
				        UIManager.Instance.OpenDialog<MagicEraView,MagicEraViewModel>().Forget();
			        }, DataCenter.magicAgeData.IsRed);
	        }
	        else
	        {
		        Manager.RemoveRightBut(MainStageInfoNodeRightButType.MagicEr);
	        }
        }

        #endregion
        
        #region 扭蛋活动按钮状态

        private void UpdateLuckyEggBtnState()
        {
	        bool isOpenLuckyEgg= MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.LuckyEgg);
	        var isTime = ServerTime.Instance.GetNowTime() >= DataCenter.luckyEggData.StartStamp && ServerTime.Instance.GetNowTime() < DataCenter.luckyEggData.EndExchangeStamp;
	        if (isOpenLuckyEgg && isTime)
	        {
		        var name =  ConfigCenter.FunctionOpenLanguageCfgColl.GetDataById((int)EFunctionOpenType.LuckyEgg).Name;
		        Manager.AddRightBut(MainStageInfoNodeRightButType.LuckyEgg,"mainui[icon_home_16]",name,
			        (objs) =>
			        {
				        UIManager.Instance.OpenDialog<LuckEggMainView,LuckEggMainViewModel>().Forget();
			        },showRed:ActivityUIManager.Instance.CheckLuckEggAllRed,time:DataCenter.luckyEggData.EndExchangeStamp);
	        }
	        else
	        {
		        Manager.RemoveRightBut(MainStageInfoNodeRightButType.LuckyEgg);
		        if (UIManager.Instance.IsOpen<LuckEggMainView>())
		        {
			        UIManager.Instance.CloseDialog<LuckEggMainView>();
		        }
	        }
        }

        #endregion

        #region 魔法宾果

        private void UpdateMagicBingoBtnState()
        {
	        bool isOpenMagicBingo= MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.MagicBingo);
	        var isTime = ServerTime.Instance.GetNowTime() >= DataCenter.mgicBingoData.StartStamp &&
	                     ServerTime.Instance.GetNowTime() < DataCenter.mgicBingoData.EndExchangeStamp;
	        if (isOpenMagicBingo && isTime)
	        {
		        var name = ConfigCenter.FunctionOpenLanguageCfgColl.GetDataById((int)EFunctionOpenType.MagicBingo).Name;
		        Manager.AddRightBut(MainStageInfoNodeRightButType.MagicBingo,"bingo[bingo_icon_1]",name,
			        (objs) =>
			        {
				        UIManager.Instance.OpenDialog<MagicBingoView,MagicBingoViewModel>().Forget();
			        },showRed:MagicBingoRed);
	        }
	        else
	        {
		        Manager.RemoveRightBut(MainStageInfoNodeRightButType.MagicBingo);
		        if (UIManager.Instance.IsOpen<MagicBingoView>())
		        {
			        UIManager.Instance.CloseDialog<MagicBingoView>();
		        }
	        }
        }

        private bool MagicBingoRed()
        {
	        return DataCenter.mgicBingoData.BinGoTaskRed() || DataCenter.mgicBingoData.BinGoCountAwardRed();
        }

        #endregion

        #region 检查是否显示助力礼包

        
        private void CheckIsShowTBoosterPackBtn()
        {
	        var typeCfgs = ConfigCenter.TriggerGiftTypeCfgColl.GetDataByTrigger(7);
	        foreach (var typeCfg in typeCfgs)
	        {
		        if (typeCfg.TriggerNum == null || typeCfg.TriggerNum.Count < 2) continue;

		        if (DataCenter.mainStageData.CurrChapter < typeCfg.TriggerNum[0]
		            || DataCenter.mainStageData.CurrChapter > typeCfg.TriggerNum[1])
			        continue;
		        
		        if(!DataCenter.triggerGiftData.Data.ContainsKey(typeCfg.Id)) return;
		        
		        if(!DataCenter.triggerGiftData.CheckIsCanOp(typeCfg.Id)) return;
		        
		        var curTriggerData = DataCenter.triggerGiftData.Data[typeCfg.Id];
		        var leftTime = ServerTime.Instance.RemainTime(curTriggerData.EndStamp);
		        if (leftTime > 0)
		        {
			        Manager.AddRightBut(MainStageInfoNodeRightButType.BoosterPack, "mainui[icon_home_17]",
				        LocalizeHelper.GetGlobal(GlobalLanguageId.Trigger05),
				        (objs) =>
				        {
					        BoosterPackViewModel tempVm = new(typeCfg.Id);
					        UIManager.Instance.OpenDialog<BoosterPackView>(tempVm).Forget();
				        }, showRed: DataCenter.triggerGiftData.CheckIsShowBoosterPackRedDot);
		        }
		        break;
	        }
        }
        
        #endregion
        
        #region 金秋活动按钮状态
        [AutoNotify] private bool isShowAutumnGo;
        [AutoNotify] private CommonButViewModel autumnBut;
        private void UpdateAutumnSpecialBtnState()
        {
	        bool isOpenAutumnSpecialOffe= MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.AutumnSpecialOffer);
	        var isTime = ServerTime.Instance.GetNowTime() >= DataCenter.autumnSpecialData.StartStamp && ServerTime.Instance.GetNowTime() < DataCenter.autumnSpecialData.EndStamp;
	        IsShowAutumnGo = isOpenAutumnSpecialOffe && isTime && !DataCenter.autumnSpecialData.IsBuyAllOver();
	        var name =  ConfigCenter.FunctionOpenLanguageCfgColl.GetDataById((int)EFunctionOpenType.AutumnSpecialOffer).Name;
	        if (IsShowAutumnGo)
	        {
		        if (AutumnBut != null)return;
		        AutumnBut = new CommonButViewModel(MainStageInfoNodeRightButType.AutumnSpecialOffer,
			        UIHelper.NoneImagePath()
			        , name, (objs) =>
			        {
				        UIManager.Instance
					        .OpenDialog<AutumnSpecialOfferView, AutumnSpecialOfferViewModel>().Forget();
			        }, time: DataCenter.autumnSpecialData.EndStamp);
	        }
        }

        #endregion
        
        private void CheckLuckyIsValid()
        {
	        if(GameConst.IsIosAuditState)return;
	        //幸运转盘
	        if ( MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.LuckyDraw) &&  DataCenter.luckyDrawData.CheckIsValid())
	        {
		        var name = LocalizeHelper.GetGlobal(GlobalLanguageId.xingyun_01);
		        Manager.AddRightBut(MainStageInfoNodeRightButType.LuckyDraw,"mainui[icon_home_6]",name,
			        (objs) => {UIManager.Instance.OpenDialog<LuckDrawView,LuckDrawViewModel>().Forget();}, DataCenter.luckyDrawData.CheckIsShowRedDot);
	        }
	        else
	        {
		        Manager.RemoveRightBut(MainStageInfoNodeRightButType.LuckyDraw);
	        }
        }
        private void CheckMagicIsValid()
        {
	        //魔法祈愿
	        if ( MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.MagicDraw) && DataCenter.magicDrawData.CheckIsValid() )
	        {
		        var name = LocalizeHelper.GetGlobal(GlobalLanguageId.MakeWish_01);
		        Manager.AddRightBut(MainStageInfoNodeRightButType.MagicDraw,"mainui[icon_home_9]",name,
			        (objs) => {UIManager.Instance.OpenDialog<MagicDrawView,MagicDrawViewModel>().Forget();}, DataCenter.magicDrawData.CheckIsShowRedDot);
	        }
	        else
	        {
		        Manager.RemoveRightBut(MainStageInfoNodeRightButType.MagicDraw);
	        }
        }

        /// <summary>
        /// 检测每日挑战/无尽关卡是否解锁弹出
        /// </summary>
        private void CheckActivityIsUnlocked()
        {
	        //每日挑战
	        if (MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionDailyFight) && GameManager.Instance.CheckDailyFightIsPopupValid())
	        {
		        GameManager.Instance.SetDailyFightPopupValid();
		       
		        PopUpManager.Instance.AddPopUp(() =>
		        {
			        UIManager.Instance.OpenDialog<ChallengeOpenView,ChallengeOpenViewModel>().Forget();
		        }, 100);
	        }
	        //无尽关卡
	        if (MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionEndless) && GameManager.Instance.CheckEndlessIsPopupValid())
	        {
		        GameManager.Instance.SetEndlessPopupValid();
		        PopUpManager.Instance.AddPopUp(() =>
		        {
			        UIManager.Instance.OpenDialog<EndlessOpenView,EndlessOpenViewModel>().Forget();
		        }, 100);
	        }
        }
        #endregion
        
        #region 初始化左侧顶部按钮

        private void InitLeftToptBut()
        {
	        if(GameConst.IsIosAuditState)return;
	        Manager.LeftTopButItems.Clear();
	        //关注有礼
	        if ( DataCenter.charcaterData.IsShowSubscribeGift())
	        {
	         var name = LocalizeHelper.GetGlobal(GlobalLanguageId.GiftCode_tips01);
	         Manager.AddLeftTopBut(MainStageInfoNodeRightButType.SubscribeGift,"mainui[icon_home_7]",name,
	          (objs) => {UIManager.Instance.OpenDialog<SubscribeGiftView,SubscribeGiftViewModel>().Forget();}, DataCenter.charcaterData.IsShowSubscribeGiftRed);
	        }
        }

        #endregion

        #region 功能按钮 设置 邮件

        private void FuncBtnRed()
        {
	        DataCenter.maildata.UpdateRedDotCount();
	        FunctionMenuRed = DataCenter.maildata.RedDotCount > 0;
        }


        #endregion
        
        #region Update

        private float time;
        public override void Update()
        {
	        base.Update();
	        if (!UIHelper.CalculateTime(ref time)) return;
	        UpdataFunc();
	        AddTriggerButton();//检查触发按钮功能是否需要开启
	        CheckLuckyIsValid();
	        CheckMagicIsValid();
	        PatrolRewardRed();
	        CheckCollegeIsValid();
	        CheckLuckTravelIsValid();
	        CheckFreeBuyIsValid();
	        UpdateSecretBtnRedDot();
	        UpdateMagicEraBtnState();
	        UpdateLuckyEggBtnState();
	        UpdateAutumnSpecialBtnState();
	        AddInvitedBtn();
	        UpdateMagicBingoBtnState();
        }

        private void UpdataFunc()
        {
	        FuncBtnRed();
        }

        #endregion

        #region 玩家等级变化

        void PlayerLevelChange(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(DataCenter.charcaterData.Digest.Lv))
	        {
		        InitRightBut();
		        InitLeftBut();
		        InitLeftToptBut();
	        }
        }
        void SubscribeGiftChange(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(DataCenter.charcaterData.DiscordFlag))
	        {
		        if (DataCenter.charcaterData.DiscordFlag == 1)
					MainUiManager.Instance.RemoveLeftTopBut(MainStageInfoNodeRightButType.SubscribeGift);
	        }
        }

        #endregion
        
    }
}