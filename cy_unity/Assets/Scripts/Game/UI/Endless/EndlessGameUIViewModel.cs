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
using Game.UI.MainUi;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class EndlessGameUIViewModel : ViewModelBase
    {
	    [AutoNotify] private string timeStr;
        [AutoNotify] private string speedTextStr;
        [AutoNotify] private string killTextStr;
        [AutoNotify] private string coinNumStr;
        [AutoNotify] private bool isShowAdBtn;
        [AutoNotify] private bool isShowAdIcon;
        [AutoNotify] private bool isShowRefreshCost;
        [AutoNotify] private string hpValueStr;
        [AutoNotify] private string defValueStr;
        [AutoNotify] private float hpSliderProgress;
        [AutoNotify] private float defSliderProgress;
        [AutoNotify] private ResItemViewModel adResIteVm;
        private bool isShowBottomBtn;
        public bool IsShowBottomBtn
        {
	        get=> isShowBottomBtn;
	        set
	        {
		        Set(ref isShowBottomBtn, value);
		        if(MainMergeVm ==null || MainMergeVm.WishVm == null) return;
		        MainMergeVm.WishVm.IsShowWishNode = value;
	        }
        }
        [AutoNotify] private string refreshBtnTextStr;
        [AutoNotify] private bool isShowRefreshBtn = true;
        [AutoNotify] private bool isShowPauseBtn = true;
        [AutoNotify] private bool isShowBattle = true;
        [AutoNotify] private ItemPriceNodeModel refreshCostViewVm;
        [AutoNotify] private CommonTopViewModel commonTopViewVm;
        [AutoNotify] private MainMergeViewModel mainMergeVm;
        [AutoNotify] private Vector2 bgRectPos;
        [AutoNotify] private Vector3 mergeAreaScale;
        [AutoNotify] private string wishItemSprite;
        [AutoNotify] private bool isShowWishItem;
        public CommonAdvIconViewModel CommonAdvVm;
        [AutoNotify] private ObservableList<ActiveSkillItemViewModel> activeSkillList = new();
        [AutoNotify] private Vector2 contentSize;
        private WishItemView curWishView;
        private Transform wishUINode;
        public Transform WishUINode
        {
	        get => null;
	        set
	        {
		        wishUINode = value;
		        if(value == null) return;
		        UpdateWishBtnPos();
	        }
        }
        private readonly BattleResource curBattleResource;
        [Preserve]
        public EndlessGameUIViewModel()
        {
	        var resourceInfo = DataCenter.itemsData.ResourceDatas[(int)GameConst.ItemIdCode.AdFreeVouche];
	        AdResIteVm = new ResItemViewModel(resourceInfo, 1);
	        AdResIteVm.IsShowAddIcon = false;
	        
	        IsShowBottomBtn = true;
	        GameTime.Instance.Pause = true;
	        if (DataCenter.endlessData.HasArchive)//如果是存档
	        {
		        GameDataManager.Instance.Wave = GameDataManager.Instance.Wave;
		        GameManager.Instance.CostTime =
			        GameDataManager.Instance.SurvivalTime;
		        PlayerStats.Instance.KillCount = GameDataManager.Instance.killedCount;
		        GameDataManager.Instance.EndlessRewardCoinNum = GetRewardCoinNumByArchive();
		        DataCenter.endlessData.HasArchive = false;
	        }
	        else
	        {
		        GameDataManager.Instance.EndlessRewardCoinNum = 0;
		        GameManager.Instance.CostTime = 0;
	        }
	        GameManager.Instance.TotalTime = 60;
	        var limitCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.endless_10);
	        if (limitCfg != null && limitCfg.Content.Count > 0)
	        {
		        GameManager.Instance.TotalTime = limitCfg.Content[0];
	        }
	        GameManager.Instance.TotalTime -= GameManager.Instance.CostTime;
	        var talentList = GameDataManager.Instance.GetEndlessWeaponTalentList();//获取可用的武器天赋列表
	        for (var i = 0; i < talentList.Count; i++) //添加所有可添加的武器天赋
	        {
		        if (!EquipManager.Instance.CheckEquipSkillUnlockByTalentId(talentList[i])) continue;
		        var equipId = GameDataManager.Instance.GetEquipIdByTalentId(talentList[i]);
		        if (!DataCenter.equipData.IsUseIng(equipId)) continue;
		        GameDataManager.Instance.AddChooseTalent(talentList[i]);
		        BattleManager.Instance.fightingManagerIns.playerCtrl.Player.AddTalent(talentList[i]);
	        }
            curBattleResource = BattleManager.Instance.fightingManagerIns.playerCtrl.Data.resource;
            var showList = new List<GameConst.ItemIdCode>{GameConst.ItemIdCode.GameCoin};
            commonTopViewVm = new CommonTopViewModel(showList);
            commonTopViewVm.SetIsShowAddIcon((int)GameConst.ItemIdCode.GameCoin,false);
            MainMergeVm = new MainMergeViewModel(OnBindWishNodeCallBack);
            Time.timeScale = 1.5f;
            SpeedTextStr = $"X{GameManager.Instance.GetCurTimeRatio()}";
            UpdatePlayerHpAndArmorInfo();
            UpdateBottomInfo();
            OnTimeChange();
            KillTextStr = PlayerStats.Instance.KillCount.ToString();
            CoinNumStr = GameDataManager.Instance.EndlessRewardCoinNum.ToString();
            curBattleResource.PropertyChanged += OnBattleResourceChanged;
            PlayerStats.Instance.PropertyChanged += OnPlayerStatsChanged;
            GameManager.Instance.PropertyChanged += OnGameManagerChanged;
            GameDataManager.Instance.PropertyChanged += OnGameDataManagerChanged;
            AdapterPos();
            DealActiveSkillList();
            CommonAdvVm = new CommonAdvIconViewModel();
        }

        public void OnBindWishNodeCallBack(WishItemView wishView)
        {
	        curWishView = wishView;
	        UpdateWishBtnPos();
        }

        private void AdapterPos()
        {
	        var defaultPos = new Vector3(0, -500, 0);
	        var ratio = Lodash.GetResolutionRation();
	     
	        if (ratio > 1)
	        {
		        defaultPos.y = defaultPos.y * ratio;
		        MergeAreaScale = Vector3.one / ratio;
	        }
	        else
	        {
		        defaultPos.y = defaultPos.y / ratio;
		        MergeAreaScale = Vector3.one;
	        }

	        Debug.Log($"defaultPos.y == {defaultPos.y}");
	        BgRectPos = defaultPos;
	        float screenWidth = 1080 *  ratio;
	        ContentSize = new Vector2(screenWidth, 0);
        }

        private void DealActiveSkillList()
        {
	        if (ActiveSkillList.Count > 0)
	        {
		        ActiveSkillList.ClearAndDispose();
	        }
	        var tempList = GameManager.Instance.GetActiveSkillList();
	        foreach (var skillId in tempList)
	        {
		        ActiveSkillItemViewModel tempVm = new(skillId,GameManager.Instance.OnClickActiveSkill);
		        ActiveSkillList.Add(tempVm);
	        }                                                                                                                                                                                                                                                                                                                                                                                                                         
	       
        }
        public override void Update()
        {
	        base.Update();
	        if(GameTime.Instance.Pause) return;
	        GameManager.Instance.TotalTime -= Time.deltaTime;
	        GameManager.Instance.CostTime += Time.deltaTime;
	        if (GameManager.Instance.TotalTime < 0) GameManager.Instance.TotalTime = 0;
	        OnTimeChange();
	        if (GameManager.Instance.TotalTime <= 0 && !GameTime.Instance.Pause)
	        {
		        GameTime.Instance.Pause = true;
		        GameManager.Instance.OnGameEnd(false);
	        }
        }
        protected override void OnDispose()
        {
	        curBattleResource.PropertyChanged -= OnBattleResourceChanged;
	        PlayerStats.Instance.PropertyChanged -= OnPlayerStatsChanged;
	        GameManager.Instance.PropertyChanged -= OnGameManagerChanged;
	        GameDataManager.Instance.PropertyChanged -= OnGameDataManagerChanged;
	        RefreshCostViewVm?.Dispose();
	        CommonAdvVm?.Dispose();
	        AdResIteVm?.Dispose();
	        base.OnDispose();
        }
        private void OnTimeChange()
        {
	        var curTime = GameManager.Instance.TotalTime;
	        TimeStr = UIHelper.ConvertTimeSecondToString((int)curTime, ETimeFormatType.TimeFormatTypeHourMinute);
        }
        /// <summary>
        /// 玩家属性变化监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBattleResourceChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName is nameof(curBattleResource.Hp) or nameof(curBattleResource.MaxHp) or nameof(curBattleResource.Armor))
	        {
		        UpdatePlayerHpAndArmorInfo();
	        }
        }
        /// <summary>
        /// 战斗波次变化监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameDataManagerChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(GameDataManager.Instance.EndlessRewardCoinNum))
	        {
		        CoinNumStr = GameDataManager.Instance.EndlessRewardCoinNum.ToString();
	        } else if (e.PropertyName == nameof(GameDataManager.Instance.CurWishCount))
	        {
		        UpdateWishBtnState();
	        }
        }
        /// <summary>
        /// 棋盘扩建状态变化监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameManagerChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(GameManager.Instance.BlockState) && GameManager.Instance.BlockState == EBlockState.AddCell)
	        {
		        IsShowBottomBtn = false;
	        }
        }
        /// <summary>
        /// 玩家属性变化监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlayerStatsChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName is not (nameof(PlayerStats.Instance.KillExp)
	            or nameof(PlayerStats.Instance.KillCount))) return;
	        GameDataManager.Instance.Exp = (int)PlayerStats.Instance.KillExp;
	        KillTextStr = PlayerStats.Instance.KillCount.ToString();
        }
        /// <summary>
        /// 检测玩家生命值
        /// </summary>
        private void UpdatePlayerHpAndArmorInfo()
        {
	        HpValueStr = curBattleResource.Hp.ToString();
	        HpSliderProgress = curBattleResource.Progress;
	        DefValueStr = curBattleResource.Armor.ToString();
	        DefSliderProgress = curBattleResource.ArmorProgress;
	        if(curBattleResource.Hp > 0) return;
	        if(GameTime.Instance.Pause) return;
	        GameTime.Instance.Pause = true;
	        GameManager.Instance.OnGameEnd(false);
        }
        /// <summary>
        /// 点击暂停按钮
        /// </summary>
        [Command]
        private void OnClickPauseBtn()
        {
	        if (MainMergeVm != null && MainMergeVm.CheckIsDraging()) return;
	        GameTime.Instance.Pause = true;
	        UIManager.Instance.OpenDialog<MainStagePauseView, MainStagePauseViewModel>().Forget();
        }
        /// <summary>
        /// 倍速点击效果
        /// </summary>
		[Command]
		private void OnClickSpeedBtn()
		{
			if (MainMergeVm != null && MainMergeVm.CheckIsDraging()) return;
			Time.timeScale = Time.timeScale < 1.4f ? 1.5f : 1.0f;
			SpeedTextStr = $"X{GameManager.Instance.GetCurTimeRatio()}";
		}
		/// <summary>
		/// 广告点击按钮
		/// </summary>
		[Command]
		private void OnClickAdBtn()
		{
			if (MainMergeVm != null && MainMergeVm.CheckIsDraging()) return;
			// 广告完成后给奖励
			UIHelper.ShowRewardAds(() =>
			{
				// 1-波次免费刷新，2-广告免费刷新，3-金币刷新；
				RequestRefresh(ERefreshType.RefreshTypeAd).Forget();
			});
		}
		/// <summary>
		/// 刷新点击按钮
		/// </summary>
		[Command]
		private void OnClickRefreshBtn()
		{
			if (MainMergeVm != null && MainMergeVm.CheckIsDraging()) return;
			// 这里需要判断是免费还是 用金币
			if (GameDataManager.Instance.EquipFreeRefreshCount > 0)
			{
				RequestRefresh(ERefreshType.RefreshTypeFree).Forget();
			}
			else
			{
				// 显示消耗
				var count = GameDataManager.Instance.EquipRefreshMoney;
				if(GameDataManager.Instance.GameCoin < count)
				{
					// 提示金币不够
					var str = LocalizeHelper.GetGlobal(GlobalLanguageId.Equip_15);
					ToastManager.Show(str);
					return;
				}
				RequestRefresh(ERefreshType.RefreshTypeMoney).Forget();
			}
		}
		/// <summary>
		/// 扩建确定按钮
		/// </summary>
		[Command]
		private void OnClickConfirmBtn()
		{
			if (MainMergeVm != null && MainMergeVm.CheckIsDraging()) return;
			IsShowBottomBtn = true;
			MainMergeVm?.ExtendConfirmLogic();
		}
		/// <summary>
		/// 战斗开始点击
		/// </summary>
		[Command]
		private void OnClickBattleBtn()
		{
			if (MainMergeVm != null && MainMergeVm.CheckIsDraging()) return;
			if(!GameDataManager.Instance.WaveEnd) return;
			IsShowBattle = false;
			GameDataManager.Instance.WaveEnd = false;
			var fightingIns = BattleManager.Instance.fightingManagerIns;
			var wave =GameDataManager.Instance.Wave;
			wave %= GameManager.Instance.EndlessMaxWaveId;
			if (wave == 0) wave = GameManager.Instance.EndlessMaxWaveId;
			var startId = GetStartIdByWaveId(wave);
			if (fightingIns.spawnManager is EndlessSpawnManager manager)
			{
				manager.TotalSpawnNum = GameDataManager.Instance.Wave-1;
				if (manager.TotalSpawnNum < 0) manager.TotalSpawnNum = 0;
			}
			//根据波次计算当前的StartId以及实际的waveId
			fightingIns.StartWave(startId, wave);
		}
		/// <summary>
		/// 无尽模式下装备刷新
		/// </summary>
		/// <param name="refreshType"></param>
		private async UniTaskVoid RequestRefresh(ERefreshType refreshType)
		{
			var data = new ReqBattleEndlessEquip();
            data.Uid = GameDataManager.Instance.Uid;
            data.Op = (int)refreshType;//1-波次免费刷新，2-广告免费刷新，3-金币刷新；
            data.BattleBegin = CheckFighting();//战斗是否开始；用于判断存盘；
            data.EquipMergeCount.Add(GameDataManager.Instance.PhysicsMergeNum);
            data.EquipMergeCount.Add(GameDataManager.Instance.MagicMergeNum);
            if (!data.BattleBegin)
            {
	            data.EquipSlots.Clear();//棋盘信息
	            // 这里更新槽位信息
	            for (var i = 0; i < GameDataManager.Instance.GMaxRow; i++)
	            {
		            for (var j = 0; j < GameDataManager.Instance.GMaxColumn; j++)
		            {
			            var index = (i + 1) * 100 + j + 1;
			            var gridValue = GameDataManager.Instance.GridData[i][j];
			            if (gridValue is 0 or 1) data.EquipSlots.Add(index,gridValue);
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
			            if (!data.EquipSlots.TryAdd(curIdx,gridValue)) data.EquipSlots[curIdx] = gridValue;
		            }
	            }
            }
            var result = await GameNetworkManager.Instance.SendAsync<RspBattleEndlessEquip>(data);
            if (result.rsp ==null ||result.rsp.Status != 0)
            {
                //DHLog.Debug(" 请求 刷新武器失败 ");
                if(result.rsp == null) return;
                var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
                ToastManager.Show(str);
                // 这里给提示
                return;
            }
            AudioManager.Instance.Play(AudioType.WeaponRefresh);
            GameDataManager.Instance.Equips.Clear();
            GameDataManager.Instance.PhysicsMergeNum = 0;
            GameDataManager.Instance.MagicMergeNum = 0;
            // 更新刷新出来的装备
            GameDataManager.Instance.Equips = result.rsp.Equips.ToList();
            // 扣钱
            GameDataManager.Instance.GameCoin -= result.rsp.CostMoney;
            if (refreshType == ERefreshType.RefreshTypeFree)
            {
	            GameDataManager.Instance.EquipFreeRefreshCount -= 1;
            } 
            else if (refreshType == ERefreshType.RefreshTypeAd)
            {
	            GameDataManager.Instance.EquipAdRefreshTotalCount -= 1;
	            GameDataManager.Instance.EquipWaveAdRefreshCount -= 1;
            }
            if (GameManager.Instance.FreeAdTipsCnt < 3) GameManager.Instance.CheckPopFreeAdTips();
			UpdateBottomInfo();
		}
		/// <summary>
		/// 检测是否在战斗中
		/// </summary>
		/// <returns></returns>
		private bool CheckFighting()
		{
			var fightingIns = BattleManager.Instance.fightingManagerIns as EndlessFightingManager;
			if (fightingIns == null) return false;
			var curSpawnManager = fightingIns.spawnManager as EndlessSpawnManager;
			if (curSpawnManager == null) return false;
			var timerList = curSpawnManager.SpawnTimerList;
			if (timerList.Count > 0)
			{
				for (var i = timerList.Count-1; i >= 0; i--)
				{
					var tmpTimer = TimerManager.Instance.FindTimer(timerList[i]);
					if (tmpTimer == null) timerList.RemoveAt(i);
				}
			}
			return timerList.Count > 0 && !GameTime.Instance.Pause;
		}
		/// <summary>
		/// 刷新心愿显示
		/// </summary>
		private void UpdateWishBtnState()
		{
			if(mainMergeVm ==null) return;
			if(mainMergeVm.WishVm ==null) return;
			// 没解锁，不处理
			MainMergeVm.WishVm.UpdateWishBtnState();
		}

		private void UpdateWishBtnPos()
		{
			if(wishUINode ==null )return;
			if(curWishView == null) return;
			var uiPos = wishUINode.position;
			Vector3 localStartPos = curWishView.gameObject.transform.parent.InverseTransformPoint(uiPos);
			if (curWishView.gameObject.TryGetComponent(out RectTransform rectTransform))
			{
				rectTransform.anchoredPosition = localStartPos;
			}
	
			if(MainMergeVm ==null || MainMergeVm.WishVm == null) return; 
			UpdateWishBtnState();
		}

		/// <summary>
		/// 底部按钮的显隐逻辑
		/// </summary>
		private void UpdateBottomInfo()
		{
			if(GameDataManager.Instance.EquipAdRefreshTotalCount > 0 && GameDataManager.Instance.EquipWaveAdRefreshCount > 0)
			{
				IsShowAdBtn = true;
				IsShowAdIcon = !DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.AdRreeReward)||
				               !DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.ADFreeForever);
			}
			else
			{
				IsShowAdBtn = false;
			}
			if (GameDataManager.Instance.EquipFreeRefreshCount > 0)
			{
				IsShowRefreshCost = false;
				RefreshBtnTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Shop_tips21);
			}
			else
			{
				RefreshBtnTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips06);
				IsShowRefreshCost = true;
				// 显示消耗
				var count = GameDataManager.Instance.EquipRefreshMoney;
				var cost = new Reward(RewardType.Item, (int)GameConst.ItemIdCode.GameCoin, count);
				RefreshCostViewVm?.Dispose();
				RefreshCostViewVm = new ItemPriceNodeModel(cost,true,null,true);
				RefreshCostViewVm.IsShowBg = false;
			}
		}
		/// <summary>
		/// 根据waveId获取对应的StartId
		/// </summary>
		/// <param name="waveId"></param>
		/// <returns></returns>
		private int GetStartIdByWaveId(int waveId)
		{
			var startId = 1;
			var cfgList = ConfigCenter.EndlessStageGrinDingCfgColl.DataItems;
			for (var i = 0; i < cfgList.Count; i++)
			{
				if (cfgList[i].WaveId == waveId)
				{
					startId = cfgList[i].Id;
					break;
				}
			}
			return startId;
		}
		/// <summary>
		/// 根据波次+击杀数算出应获得金币数
		/// </summary>
		/// <returns></returns>
		private int GetRewardCoinNumByArchive()
		{
			var maxPassChapter = DataCenter.mainStageData.GetMaxPassChapter();
			var monsterCoin = GameDataManager.Instance.GetMonsterCoinNumByChapterId(maxPassChapter);
			var cfg = ConfigCenter.CopyCfgColl.GetDataById(maxPassChapter);
			var totalNum = GameDataManager.Instance.Wave / 10;
			var evenNum = GameDataManager.Instance.Wave / 20;
			var oddNum = totalNum-evenNum;
			if (oddNum < 0) oddNum = 0;
			var waveCoinCoin = 0;
			if (cfg?.Coin.Count == 3)
			{
				waveCoinCoin = cfg.Coin[1] * oddNum + evenNum * cfg.Coin[2];
			}
			var totalRewardCoin = PlayerStats.Instance.KillCount * monsterCoin + waveCoinCoin;
			return totalRewardCoin;
		}
    }
}