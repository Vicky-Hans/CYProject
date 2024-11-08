using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;
namespace DH.Game.ViewModels
{
    public partial class ChallengeGameViewModel : ViewModelBase
    {
        [AutoNotify] private string speedTextStr;
		[AutoNotify] private string waveTextStr;
		[AutoNotify] private string leftCountStr;
		[AutoNotify] private ItemPriceNodeModel refreshCostViewVm;
		[AutoNotify] private bool isShowAdBtn;
		[AutoNotify] private bool isShowAdIcon;
		[AutoNotify] private CommonTopViewModel commonTopViewVm;
		[AutoNotify] private MainMergeViewModel mainMergeVm;
		[AutoNotify] private bool isShowRefreshCost;
		[AutoNotify] private string hpValueStr;
		[AutoNotify] private string defValueStr;
		[AutoNotify] private string talentCountStr;
		[AutoNotify] private float talentProgressValue;
		[AutoNotify] private float hpSliderProgress;
		[AutoNotify] private float defSliderProgress;
		[AutoNotify] private bool isShowTalentCount;
		[AutoNotify] private string addBuffIconStr;
		[AutoNotify] private string reduceBuffIconStr;
		[AutoNotify] private string killCountStr;
		[AutoNotify] private string refreshBtnTextStr;
		[AutoNotify] private bool isShowWaveEffect;
		[AutoNotify] private string waveEffectTextStr;
		[AutoNotify] private float waveEffectAlpha;
		[AutoNotify] private bool isShowRefreshBtn = true;
		[AutoNotify] private bool isShowPauseBtn = true;
		[AutoNotify] private bool isShowTalentBtn;
		[AutoNotify] private Vector2 bgRectPos;
		[AutoNotify] private Vector3 mergeAreaScale;
		[AutoNotify] private ObservableList<ActiveSkillItemViewModel> activeSkillList = new();
		[AutoNotify] private ObservableList<ChallengeBuffItemViewModel> buffList = new();
		[AutoNotify] private Vector2 contentSize;
		[AutoNotify] private ResItemViewModel adResIteVm;
		private WishItemView curWishView;
		private Transform wishUINode;
		private bool isShowBottomBtn;
		public CommonAdvIconViewModel CommonAdvVm;
		public Transform WishUINode
		{
			get => null;
			set
			{
				wishUINode = value;
				if(value != null) UpdateWishBtnPos();
			}
		}
		public bool IsShowBottomBtn
		{
			get=> isShowBottomBtn;
			set
			{
				Set(ref isShowBottomBtn, value);
				if(MainMergeVm?.WishVm == null) return;
				MainMergeVm.WishVm.IsShowWishNode = value;
			}
		}
		private BattleResource curBattleResource;
		public GameDataManager DataManager => GameDataManager.Instance;
		private int maxWave;//最大波次
        [Preserve]
        public ChallengeGameViewModel()
        {
	        var resourceInfo = DataCenter.itemsData.ResourceDatas[(int)GameConst.ItemIdCode.AdFreeVouche];
	        AdResIteVm = new ResItemViewModel(resourceInfo, 1);
	        AdResIteVm.IsShowAddIcon = false;
	        
	        curBattleResource = BattleManager.Instance.fightingManagerIns.playerCtrl.Data.resource;
	        maxWave = GameManager.Instance.GetDailyFightMaxWave();
	        var showList = new List<GameConst.ItemIdCode>{GameConst.ItemIdCode.GameCoin };
	        commonTopViewVm = new(showList);
	        commonTopViewVm.SetIsShowAddIcon((int)GameConst.ItemIdCode.GameCoin,false);
	        MainMergeVm = new MainMergeViewModel(OnBindWishNodeCallBack);
	        UpdateInfo();
	        curBattleResource.PropertyChanged += OnBattleResourceChanged;
	        PlayerStats.Instance.PropertyChanged += OnPlayerStatsChanged;
	        GameManager.Instance.PropertyChanged += OnGameManagerChanged;
	        GameDataManager.Instance.PropertyChanged += OnGameDataManagerChanged;
	        GameTime.Instance.PropertyChanged += OnGameTimeChanged;
	        IsShowBottomBtn = true;
	        for (var i = 0; i < DataCenter.dailyFightData.Buff.Count; i++)
	        {
		        var buff = new ChallengeBuffItemViewModel(DataCenter.dailyFightData.Buff[i]);
		        BuffList.Add(buff);
	        }
	        KillCountStr = PlayerStats.Instance.KillCount.ToString();
	        GameTime.Instance.Pause = true;
	        IsShowWaveEffect = false;
	        WaveEffectAlpha = 0;
	        AdapterPos();
	        DealActiveSkillList();
	        // 开局默认倍数
	        InitDefaultSpeed();
	        CommonAdvVm = new CommonAdvIconViewModel();
        }
        public void OnBindWishNodeCallBack(WishItemView wishView)
        {
	        curWishView = wishView;
	        UpdateWishBtnPos();
        }

        /// <summary>
        /// 设置初始速度
        /// </summary>
        private void InitDefaultSpeed()
        {
	        if (!GameManager.Instance.CheckIsCanOpenRatio())
	        {
		        return;
	        }
	        GameManager.Instance.OnClickSpeedBtn();
	        UpdateSpeedInfo();
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
        private void UpdateInfo()
        {
	        CheckIsUpGradeLevel();
	        UpdateTalentInfo();
	        UpdatePlayerHpAndArmorInfo();
	        UpdateBottomInfo();
	        UpdateSpeedInfo();
	        UpdateWaveStr();
        }

        protected override void OnDispose()
        {
	        curBattleResource.PropertyChanged -= OnBattleResourceChanged;
	        GameDataManager.Instance.PropertyChanged -= OnGameDataManagerChanged;
	        GameTime.Instance.PropertyChanged -= OnGameTimeChanged;
	        GameManager.Instance.PropertyChanged -= OnGameManagerChanged;
	        PlayerStats.Instance.PropertyChanged -= OnPlayerStatsChanged;
	        CommonAdvVm?.Dispose();
	        AdResIteVm?.Dispose();
	        if (RefreshCostViewVm != null)
	        {
		        RefreshCostViewVm.Dispose();
	        }
	        base.OnDispose();
        }
        private void OnBattleResourceChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(curBattleResource.Hp)
	            || e.PropertyName == nameof(curBattleResource.MaxHp)
	            ||e.PropertyName == nameof(curBattleResource.Armor))
	        {
		        UpdatePlayerHpAndArmorInfo();
	        }
        }

        private void OnGameDataManagerChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(GameDataManager.Instance.WaveEnd))
	        {
		        // Debug.Log($"muzili log  notify {GameDataManager.Instance.WaveEnd}");
		        // 这里处理波次结束的逻辑
		        if (GameDataManager.Instance.WaveEnd)
		        {
			        if(GameTime.Instance.Pause) return;
			        GameTime.Instance.Pause = true;
			        if (maxWave == GameDataManager.Instance.Wave)
			        {
				        GameManager.Instance.OnGameEnd(true);
			        }
			        else
			        {
				        // 更新波次
				        GameDataManager.Instance.Wave++;
				        //这里开始刷新玩家操作
				        GameManager.Instance.RequestReqBattleWaveDone(OnRequestWaveEndCallback).Forget();
				        if (GameDataManager.Instance.CheckIsCanRandomTalent())
				        {
					        // 主动打开天赋选择界面
					        OnClickTalentBtn();   
				        }
			        }
		        }
		        else
		        {
			        UpdateWaveStr();
		        }
	        }
	        else if (e.PropertyName == nameof(GameDataManager.Instance.CurWishCount))
	        {
		        // 刷新心愿显示
		        UpdateWishBtnState();
	        }
        }

        private void OnRequestWaveEndCallback()
        {
	        UpdateBottomInfo();
        }

        private void OnGameTimeChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(GameTime.Instance.Pause))
	        {
		        
	        }
        }

        private void OnGameManagerChanged(object sender, PropertyChangedEventArgs e)
        {
	        if(e.PropertyName == nameof(GameManager.Instance.ExpProgressValue))
	        {
		        UpdateTalentInfo();
	        }
	        else if (e.PropertyName == nameof(GameManager.Instance.BlockState))
	        {
		        if (GameManager.Instance.BlockState == EBlockState.AddCell)
		        {
			        IsShowBottomBtn = false;
		        }
	        } else if (e.PropertyName == nameof(GameManager.Instance.ReviveCount))
	        {
		        if(GameTime.Instance.Pause) return;
			        
		        GameTime.Instance.Pause = true;
		        GameDataManager.Instance.Wave++;
	        }
        }

        private void OnPlayerStatsChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName is not (nameof(PlayerStats.Instance.KillExp)
	            or nameof(PlayerStats.Instance.KillCount))) return;
	        GameDataManager.Instance.Exp = (int)PlayerStats.Instance.KillExp;
	        KillCountStr = PlayerStats.Instance.KillCount.ToString();
	        CheckIsUpGradeLevel();
	        UpdateTalentInfo();
        }

        private void UpdateTalentInfo()
        {
	        
			if (GameDataManager.Instance.CheckIsCanRandomTalent()) 
			{
				var count = GameDataManager.Instance.GetCanChooseTalentCount();
				TalentCountStr = count.ToString();
				IsShowTalentCount = true;
			}
			else
			{
				TalentCountStr = "";
				IsShowTalentCount = false;
			}

        }
        
        private void UpdatePlayerHpAndArmorInfo()
        {
	        HpValueStr = curBattleResource.Hp.ToString();
	        HpSliderProgress = curBattleResource.Progress;
	        DefValueStr = curBattleResource.Armor.ToString();
	        DefSliderProgress = curBattleResource.ArmorProgress;
	        
	        if(curBattleResource.Hp > 0) return;
	        if( GameTime.Instance.Pause) return;
	        GameTime.Instance.Pause = true;
	        // 先检查资产是否足够
	        var costList = GameManager.Instance.GetReviveCost();
	        if (costList.Count == 0)
	        {
		        GameManager.Instance.OnGameEnd(false);
	        }
	        else
	        {
		        // 这里检查是否可以复活
		        if ( GameDataManager.Instance.Wave > 1 && GameManager.Instance.ReviveCount  < 1 && Lodash.CheckRewardIsEnough(costList))
		        {
			        UIManager.Instance.OpenDialog<ReviveView,ReviveViewModel>().Forget();
		        }
		        else
		        {
			        GameManager.Instance.OnGameEnd(false);
		        }
	        }
        }
        
        [Command]
        private void OnClickPauseBtn()
        {
	        if (MainMergeVm != null && MainMergeVm.CheckIsDraging()) return;
	        GameTime.Instance.Pause = true;
	        // 暂停
	        UIManager.Instance.OpenDialog<MainStagePauseView, MainStagePauseViewModel>().Forget();
        }

		[Command]
		private void OnClickSpeedBtn()
		{
			if (MainMergeVm != null && MainMergeVm.CheckIsDraging()) return;
			if (!GameManager.Instance.CheckIsCanOpenRatio())
			{
				var tips = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips35);
				ToastManager.Show(tips);
				return;
			}
			GameManager.Instance.OnClickSpeedBtn();
			UpdateSpeedInfo();
		}
		[Command]
		private void OnClickTalentBtn()
		{
			if (MainMergeVm != null && MainMergeVm.CheckIsDraging()) return;
			if (GameDataManager.Instance.Level <= 1)
			{
				GameTime.Instance.Pause = true;
				TalentChooseViewModel tempVm = new(new List<int>(), OnSelectedTalent);
				UIManager.Instance.OpenDialog<TalentChooseView>(tempVm).Forget();
				return;
			}
			if (!GameDataManager.Instance.CheckIsCanRandomTalent() )
			{
				GameTime.Instance.Pause = true;
				TalentChooseViewModel tempVm = new(new List<int>(), OnSelectedTalent);
				UIManager.Instance.OpenDialog<TalentChooseView>(tempVm).Forget();
				return;
			}
			// 这里要检查是否有随机了没有操作的
			if (GameDataManager.Instance.UnOpTalents.Count > 0)
			{
				GameTime.Instance.Pause = true;
				TalentChooseViewModel tempVm = new(GameDataManager.Instance.UnOpTalents, OnSelectedTalent);
				UIManager.Instance.OpenDialog<TalentChooseView>(tempVm).Forget();
				return;
			}
			// 这个请求刷新
			GameManager.Instance.RequestRefreshTalent(ERefreshType.RefreshTypeFree, GameDataManager.Instance.ChooseTalentCount + 1,(talentList) => {
				GameTime.Instance.Pause = true;
				TalentChooseViewModel tempVm = new(talentList, OnSelectedTalent);
				UIManager.Instance.OpenDialog<TalentChooseView>(tempVm).Forget();
			}).Forget();
		}
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

		[Command]
		private void OnClickConfirmBtn()
		{
			if (MainMergeVm != null && MainMergeVm.CheckIsDraging()) return;
			IsShowBottomBtn = true;
			MainMergeVm?.ExtendConfirmLogic();
			UpdateWishBtnState();
		}
		private void OnSelectedTalent(int talentId,Action<List<int>> callback)
		{
			if (talentId == -1)
			{
				UpdateTalentInfo();
				return;
			}
			if (GameDataManager.Instance.CheckIsCanRandomTalent())
			{
				GameManager.Instance.RequestRefreshTalent(ERefreshType.RefreshTypeFree, GameDataManager.Instance.ChooseTalentCount  + 1, callback).Forget();
			}
			else
			{
				UIManager.Instance.CloseDialog<TalentChooseView>();
			}
			UpdateTalentInfo();
		}

		[Command]
		private void OnClickBattleBtn()
		{
			if (MainMergeVm != null && MainMergeVm.CheckIsDraging()) return;
			if(!GameDataManager.Instance.WaveEnd) return;
			GameManager.Instance.DailyFightIsBegin = true;
			GameTime.Instance.Pause = true;
			GameDataManager.Instance.WaveEnd = false;
			var fightingIns = BattleManager.Instance.fightingManagerIns;
			var stageId = 1;
			var wave = GameDataManager.Instance.Wave;
			var cfg = ConfigCenter.DailyStageGrindingCfgColl.GetDataByNowWave(wave);
			if (cfg is { Count: > 0 }) stageId = cfg[0].Id;
			fightingIns.StartWave(stageId, wave);
			DoWaveEffectAction().Forget();
		}
		private async UniTaskVoid RequestRefresh(ERefreshType refreshType)
		{
			var data = new ReqBattleEquipRefresh();
			data.Uid = GameDataManager.Instance.Uid;
			data.Op = (int)refreshType;
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
			data.EquipMergeCount.Add(DataManager.PhysicsMergeNum);
			data.EquipMergeCount.Add(DataManager.MagicMergeNum);
			var result = await GameNetworkManager.Instance.SendAsync<RspBattleEquipRefresh>(data);
			if (result.rsp ==null ||result.rsp.Status != 0)
			{
				if(result.rsp == null) return;
				var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
				ToastManager.Show(str);
				// 这里给提示
				return;
			}
			DataManager.PhysicsMergeNum = 0;
			DataManager.MagicMergeNum= 0;
			AudioManager.Instance.Play(AudioType.WeaponRefresh);
			GameDataManager.Instance.Equips.Clear();
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
		/// 刷新心愿显示
		/// </summary>
		private void UpdateWishBtnState()
		{
			if(mainMergeVm ==null) return;
			if(mainMergeVm.WishVm ==null) return;
			MainMergeVm.WishVm.UpdateWishBtnState();
		}
		private void UpdateWishBtnPos()
		{
			if(wishUINode ==null ||curWishView == null )return;
			var uiPos = wishUINode.position;
			var localStartPos = curWishView.gameObject.transform.parent.InverseTransformPoint(uiPos);
			if (curWishView.gameObject.TryGetComponent(out RectTransform rectTransform))
			{
				rectTransform.anchoredPosition = localStartPos;
			}
			var cueWishVm = curWishView.GetDataContext() as WishItemViewModel; 
			if(cueWishVm == null ) return;
			UpdateWishBtnState();
		}
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
				var tempStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Shop_tips16);
				LeftCountStr = $"{tempStr} {GameDataManager.Instance.EquipFreeRefreshCount}";
				IsShowRefreshCost = false;
				RefreshBtnTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Shop_tips21);
			}
			else
			{
				RefreshBtnTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips06);
				IsShowRefreshCost = true;
				LeftCountStr = "";
				// 显示消耗
				var count = GameDataManager.Instance.EquipRefreshMoney;
				var cost = new Reward(RewardType.Item, (int)GameConst.ItemIdCode.GameCoin, count);
				RefreshCostViewVm?.Dispose();
				RefreshCostViewVm = new(cost,true,null,true);
				RefreshCostViewVm.IsShowBg = false;
			}
		}
		private void UpdateSpeedInfo()
		{
			var speed = GameManager.Instance.GetCurTimeRatio();
			SpeedTextStr = $"X{speed}";
		}
		private void UpdateWaveStr()
		{
			var repStr = $"{GameDataManager.Instance.Wave}/{maxWave}";
			WaveTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips04, repStr);
			WaveEffectTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips04, GameDataManager.Instance.Wave);
		}
		private void CheckIsUpGradeLevel()
		{
			// 默认1级 检查是否升级
			if (GameDataManager.Instance.Level <= 0) GameDataManager.Instance.Level = 1;
			var lvCfg = ConfigCenter.CopyLevelCfgColl.GetDataById(GameDataManager.Instance.Level);
			if (lvCfg == null) return;
			var tempExp = GameDataManager.Instance.Exp;
			var isUpgrade = false;
			var offsetExp = 0;
			if (lvCfg.Total_exp_1 <= tempExp)
			{
				GameDataManager.Instance.Level += 1;
				tempExp -= lvCfg.Total_exp_1;
				tempExp = CheckNextLevel(tempExp);
				isUpgrade = true;
			}
			else
			{
				if (GameDataManager.Instance.Level > 1)
				{
					var tempCfg = ConfigCenter.CopyLevelCfgColl.GetDataById(GameDataManager.Instance.Level - 1);
					offsetExp = tempCfg.Total_exp_1;
				}
			}
			var curCfg = ConfigCenter.CopyLevelCfgColl.GetDataById(GameDataManager.Instance.Level > 1 ? GameDataManager.Instance.Level - 1 : 1);
			TalentProgressValue = (tempExp - offsetExp) / (float)curCfg.Exp;
			UpdateTalentInfo();
			if (isUpgrade) GameManager.Instance.RequestUpGrade().Forget(); // 这里跟新数据 可以选择的次数
		}
		private int CheckNextLevel(int endExp)
		{
			var curLevelCfg = ConfigCenter.CopyLevelCfgColl.GetDataById(GameDataManager.Instance.Level);
			if (curLevelCfg == null) return 0;
			// 这里 是经验值到达这个值就可以升级
			if (endExp >= curLevelCfg.Total_exp_1)
			{
				GameDataManager.Instance.Level += 1;
				return CheckNextLevel(endExp-curLevelCfg.Total_exp_1);
			}
			return endExp;
		}
		private async UniTaskVoid DoWaveEffectAction()
		{
			WaveEffectAlpha = 0;
			IsShowWaveEffect = true;
			DOVirtual.Float(WaveEffectAlpha, 1, 0.085f, (v) =>
			{
				WaveEffectAlpha = v;
			});
			await UniTask.Delay(500);
			DOVirtual.Float(WaveEffectAlpha, 0, 0.5f, (v) =>
			{
				WaveEffectAlpha = v;
			});
			await UniTask.Delay(500);
			IsShowWaveEffect = false;
		}
    }
}