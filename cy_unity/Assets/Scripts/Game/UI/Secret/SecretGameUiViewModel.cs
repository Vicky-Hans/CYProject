using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class SecretGameUiViewModel : ViewModelBase
    {
        
		[AutoNotify] private float expSliderValue;
	    [AutoNotify] private ObservableList<SecretWeaponCellViewModel> weaponScrollviewList = new();
		[AutoNotify] private string expLevelTextStr;
		[AutoNotify] private bool isShowTimeNode = true;
		[AutoNotify] private string timeTextStr;
		[AutoNotify] private float scrollviewSpacingX = -20;
		[AutoNotify] private float scrollviewSpacingY;
		[AutoNotify] private int scrollviewRowCount = 7;
		[AutoNotify] private float scrollviewPaddingLeft;
		[AutoNotify] private float scrollviewPaddingTop;
		[AutoNotify] private string killTextStr;
		[AutoNotify] private string speedTextStr;
		[AutoNotify] private bool isShowBossInfoNode;
		[AutoNotify] private float bossHpSliderValue;
		[AutoNotify] private string bossHpInfoTextStr;
		[AutoNotify] private float bossLeftTimeSliderValue;
		[AutoNotify] private string bossLeftTimeTextStr;
		[AutoNotify] private Color bossLeftTimeTextColor;
		[AutoNotify] private Vector3 bossLeftTimeTextScale = Vector3.one;
		private float bossEndTime;
		private DefinesCfg secretBossTimeCfg;
		private ECellItemSizeType cellSize = ECellItemSizeType.Size200X180;
		private ICollectionView weaponScrollview;
		private float minScale = 0.7f;
		private float maxScale = 1.3f;
		private float duration =1.0f;
		private float totlaTime = 1;
		private Sequence timeTextActionSequence;
         public ICollectionView WeaponScrollview
         {
        	 get => null;
        	 set
        	 {
		         weaponScrollview = value;
        		 if (weaponScrollview == null) return;
		         // weaponScrollview.Comparer = this;
		         weaponScrollview.Refresh();
	         }
         }
		private BattleResource curBattleResource;
		private CopySecretCfg copySecretCfg;
        [Preserve]
        public SecretGameUiViewModel()
        {
	        copySecretCfg = ConfigCenter.CopySecretCfgColl.GetDataById(GameManager.Instance.CurChapterId);
	        curBattleResource = BattleManager.Instance.fightingManagerIns.playerCtrl.Data.resource;
	        InitPanel();
	        curBattleResource.PropertyChanged += OnBattleResourceChanged;
	        // GameDataManager.Instance.BackpackWeaponList.CollectionChanged += OnWeaponCollectionChanged;
	        GameDataManager.Instance.PropertyChanged += OnGameDataChanged;
	        
	        PlayerStats.Instance.PropertyChanged += OnPlayerStatsChanged;
	        StartGame();
	        secretBossTimeCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_27);
	        if (secretBossTimeCfg is { Content: { Count: > 0 } })
	        {
		        totlaTime = secretBossTimeCfg.Content[0];
	        }
        }


        private void StartGame()
        {
	        // 还原护甲
	        BattleManager.Instance.fightingManagerIns.playerCtrl.Data.resource.Armor = GameDataManager.Instance.Armor;
	        int chapterId = GameManager.Instance.CurChapterId;
	        int wave = GameDataManager.Instance.Wave;
	        BattleManager.Instance.fightingManagerIns.StartWave(chapterId,wave);
	        CheckIsUpGradeLevel();
	        GameDataManager.Instance.WaveEnd = false;
        }

        protected override void OnDispose()
        {
	        PlayerStats.Instance.PropertyChanged -= OnPlayerStatsChanged;
	        GameDataManager.Instance.PropertyChanged -= OnGameDataChanged;
	        // GameDataManager.Instance.BackpackWeaponList.CollectionChanged -= OnWeaponCollectionChanged;
	        curBattleResource.PropertyChanged -= OnBattleResourceChanged;
	        base.OnDispose();
        }

        public override void Update()
        {
	        base.Update();
	        UpdateTimeInfo();
	        CheckIsRequestTalent();
	        UpdateBossLeftTimeInfo();
	        UpdateBossShowInfo();
	        CheckSeasonEnd();
        }
        
        private void CheckSeasonEnd()
        {
	        if(GameManager.Instance.IsOpenExitTip) return;
	        var leftTime = ServerTime.Instance.RemainTime(DataCenter.secretData.SeasonTime);
	        if(leftTime > GameConst.SecretShowTipsTime)return;
	        long timeDiff = 10;
	        GameTime.Instance.Pause = true;
	        if(leftTime < timeDiff)
	        {
		        timeDiff = leftTime;
	        }
	        var vm = new ChallengeTipsViewModel(GlobalLanguageId.Secret_15, (int)timeDiff, () =>
	        {
		        GameManager.Instance.OnGameEnd(false);
	        });
	        UIManager.Instance.OpenDialog<ChallengeTipsView>(vm).Forget();
	        GameManager.Instance.IsOpenExitTip = true;
        }

        private void CheckIsRequestTalent()
        {
	        if(!UIManager.Instance.CheckIsTopDlg<SecretGameUiView>()) return;
	        if(GameManager.Instance.IsCanRandomTalent != true) return;
	        var callbackList = GameManager.Instance.RequestTalentList;
	        if(callbackList.Count <= 0)  return;
	        GameManager.Instance.IsCanRandomTalent = false;
	        GameTime.Instance.Pause = true;
	        GameDataManager.Instance.WaveEnd = false;
	        callbackList[0]();
        }



       
        private void UpdateTimeInfo()
        {
	        if(GameTime.Instance.Pause) return;
	        if(GameDataManager.Instance.WaveEnd) return;
	        if(!UIHelper.CalculateTime(ref curTime)) return;
	        UpdateTimeTextStr();
        }

        private void UpdateBossLeftTimeInfo()
        {
	        if(GameTime.Instance.Pause) return;
	        UpdateBossLeftTimeTextStr();
        }

        private void UpdateBossShowInfo()
        {
	        if(!IsShowBossInfoNode) return;
	        var curBoss = BattleManager.Instance.MapFightingManager.CurrentBoss;
	        if(curBoss == null) return;
	        BossHpSliderValue = curBoss.Data.resource.Progress;
	        BossHpInfoTextStr = $"{curBoss.Data.resource.Hp}/{curBoss.Data.resource.MaxHp}";
        }

        private void UpdateTimeTextStr()
        {
	        GameDataManager.Instance.CurGameDuration += 1;
	        TimeTextStr = ServerTime.Instance.Seconds2Mmss(GameDataManager.Instance.CurGameDuration);
        }

        private float curTime = 0;
        private void UpdateBossLeftTimeTextStr()
        {
	        var leftTime = bossEndTime - GameTime.Instance.GTime;
	        BossLeftTimeTextStr = ServerTime.Instance.Seconds2Mmss((int)leftTime);
	        BossLeftTimeSliderValue = leftTime / totlaTime;
	        BossLeftTimeTextColor =leftTime > 10 ? UIHelper.HexColorStrToColor("#ffffff"):UIHelper.HexColorStrToColor("#ff0000");
	       
	        if (leftTime <= 0 && IsShowBossInfoNode)
	        {
		        //游戏结束
		        GameTime.Instance.Pause = true;
		        GameManager.Instance.OnGameEnd(false);
	        }
	        
	        if(leftTime > 10 || leftTime <= 0) return;
	        if(timeTextActionSequence!=null)return;
	        PlayTimeActionSequence();
        }

        private void PlayTimeActionSequence()
        {
	        var toMinAction = DOVirtual.Vector3(bossLeftTimeTextScale, Vector3.one * maxScale,
		        duration / 2, (curScale) =>
		        {
			        BossLeftTimeTextScale = curScale;
		        }).SetEase(Ease.InSine);
	        var toMaxAction = DOVirtual.Vector3(bossLeftTimeTextScale, Vector3.one * minScale,
		        duration / 2,
		        (tempScale) =>
		        {
			        BossLeftTimeTextScale = tempScale;
		        }).SetEase(Ease.OutQuint);
	        timeTextActionSequence = DOTween.Sequence();
	        timeTextActionSequence
		        .Append(toMaxAction)
		        .Append(toMinAction)
		        .SetLoops(10)
		        .SetEase(Ease.InOutQuad)
		        .OnComplete(() =>
		        {
			        timeTextActionSequence.Kill();
			        timeTextActionSequence = null;
		        });
        }

        private void OnPlayerStatsChanged(object sender, PropertyChangedEventArgs e)
        {
	        if(e.PropertyName == nameof(PlayerStats.Instance.KillExp))
	        {
		        GameDataManager.Instance.Exp = (int)PlayerStats.Instance.KillExp;
		        CheckIsUpGradeLevel();
	        } else if (e.PropertyName == nameof(PlayerStats.Instance.KillCount))
	        {
		        UpdateKillText();
	        }

        }

        private void OnGameDataChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (nameof(GameDataManager.Instance.WaveEnd) == e.PropertyName)
	        {
		        IsShowBossInfoNode = false;
		        if (GameDataManager.Instance.ShowBossInfo)
		        {
			        GameDataManager.Instance.ShowBossInfo = false;
		        }
	        } 
	        else if (e.PropertyName == nameof(GameDataManager.Instance.CurGameState))
	        {
		        // 结束游戏
		        if (GameDataManager.Instance.CurGameState == EGameState.Success)
		        {
			        if(GameTime.Instance.Pause) return;
			        GameTime.Instance.Pause = true;
			        // 结束游戏
			        GameManager.Instance.OnGameEnd(true);
		        } else if (GameDataManager.Instance.CurGameState == EGameState.Fail)
		        {
			        if(GameTime.Instance.Pause) return;
			        GameTime.Instance.Pause = true;
			        BattleManager.Instance.MapFightingManager.playerCtrl.hpBar.SetPercent(0);
			        // 结束游戏
			        GameManager.Instance.OnGameEnd(false);
		        }
	        } 
	        else if (e.PropertyName == nameof(GameDataManager.Instance.BackpackWeaponList))
	        {
		        UpdateWeaponList();
	        }
	        else if (e.PropertyName == nameof(GameDataManager.Instance.ShowBossInfo))
	        {
		        IsShowBossInfoNode = GameDataManager.Instance.ShowBossInfo;
		        if (GameDataManager.Instance.ShowBossInfo)
		        {
			        // 这里更新时间
			        bossEndTime = GameTime.Instance.GTime + totlaTime;
			        BossLeftTimeTextScale = Vector3.one;
			        if (timeTextActionSequence != null)
			        {
				        timeTextActionSequence.Kill();
				        timeTextActionSequence = null;
			        }
		        }
	        }
        }

        private void OnWeaponCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
	        
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

        private void InitPanel()
        {
	        // UpdatePlayerHpAndArmorInfo();
	        CheckIsUpGradeLevel();
	        UpdateWeaponList();
	        UpdateTimeTextStr();
	        // InitDefaultSpeed();
	        var curSpeed = DHUnityUtil.PlayerPrefs.GetFloat(GameConst.SecretBattleSpeedKey, 1.0f);
	        if (curSpeed > 1.4f)
	        {
		        OnClickSpeedBtn();
	        }
	        else
	        {
		        UpdateSpeedInfo();
	        }
	        UpdateKillText();
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

        private void UpdatePanel()
        {
	        
        }

        private async void UpdateWeaponList()
        {
	        foreach (var item in weaponScrollviewList)
	        {
				item.Dispose();   
	        }
	        weaponScrollviewList.Clear();
	        UpdateWeaponScrollviewInfo();
	        var tempList = GameDataManager.Instance.BackpackWeaponList.ToList();
	        tempList.Sort((a, b) =>
	        {
				  // 先判断等级
				  if (a.WeaponLev != b.WeaponLev)
				  {
					  return b.WeaponLev.CompareTo(a.WeaponLev);
				  }
				  var equipCfgA = ConfigCenter.EquipCfgColl.GetDataById(a.EquipId);
				  var equipCfgB = ConfigCenter.EquipCfgColl.GetDataById(b.EquipId);
				  // 再判断品质
				  if(equipCfgA.Quality != equipCfgB.Quality)
				  {
					return equipCfgB.Quality.CompareTo(equipCfgA.Quality);
				  }
		        // 最后判断id
		        return a.WeaponId.CompareTo( b.WeaponId);
	        });
	        DHLog.Debug("muzili log refresh weapon list ");
	        // await UniTask.Delay(200);
	        foreach (var item in tempList)
	        {
		        SecretWeaponCellViewModel tempVm = new(item);
		        tempVm.CellSize = cellSize;
				WeaponScrollviewList.Add(tempVm);
	        }
	        BattleManager.Instance.fightingManagerIns.OnWeaponChanged();
	        if (weaponScrollview != null)
	        {
		        weaponScrollview.Refresh();
	        }
	        DelayPlayMergeEffect();
        }

        private async void DelayPlayMergeEffect()
        {
	        await UniTask.Delay(1100);
	        if(GameDataManager.Instance.MergeWeaponList.Count <= 0) return;
	        foreach (var item in WeaponScrollviewList)
	        {
				item.DelayShowMergeEffect().Forget();
	        }
	        GameDataManager.Instance.MergeWeaponList.Clear();
        }

        private void UpdatePlayerHpAndArmorInfo()
        {
	        if (curBattleResource != null)
	        {
		        // HpSliderValue = curBattleResource.Progress;
		        // DefSliderValue = curBattleResource.ArmorProgress;
		        if (curBattleResource.Hp <= 0 )
		        {
			        GameDataManager.Instance.CurGameState = EGameState.Fail;
		        }
	        }
        }
        private void UpdateSpeedInfo()
        {
	        
	        var speed = GameManager.Instance.GetCurTimeRatio();
	        SpeedTextStr = $"X{speed}";
	     
        }
        [Command]
        private void OnClickSpeedBtn()
        {
	        if (!GameManager.Instance.CheckIsCanOpenRatio())
	        {
		        return;
	        }
	        GameManager.Instance.OnClickSpeedBtn();
	        DHUnityUtil.PlayerPrefs.SetFloat(GameConst.SecretBattleSpeedKey, Time.timeScale);
	        UpdateSpeedInfo();
        }

        [Command]
        private void OnClickPauseBtn()
        {
	        GameTime.Instance.Pause = true;
	        UIManager.Instance.OpenDialog<MainStagePauseView,MainStagePauseViewModel>().Forget();
	        
        }


        private void RequestChooseTalent(int level, int exp)
        {
	        // 主动打开天赋选择界面
	        GameManager.Instance.RequestSecretTalent(ESecretTalentRefreshType.Upgrade,
		        GameManager.Instance.OpenTalentChooseView, level,exp).Forget();;  
        }

        private void UpdateKillText()
        {
	        KillTextStr = PlayerStats.Instance.KillCount.ToString();
        }

        public void CheckIsUpGradeLevel()
        {
	        // 默认1级
	        // 检查是否升级
	        var cfgId = GameDataManager.Instance.Level + 1;
	        var lvCfg = ConfigCenter.SecretCopyLevelCfgColl.GetDataById(cfgId);
	        if (lvCfg == null)
	        {
		        DHLog.Debug($"muzili log 没有等级配置 {cfgId}");
		        return;
	        }
	        var tempExp = GameDataManager.Instance.Exp;
	        bool isUpgrade = false;
	        var offsetExp = 0;
	        if (lvCfg.Total_exp_1 <= tempExp)
	        {
		        GameDataManager.Instance.Level += 1;
		        var curLevel = GameDataManager.Instance.Level;
		        GameManager.Instance.AddTalentRequestToList(curLevel);
		        tempExp -= lvCfg.Total_exp_1;
		        tempExp = CheckNextLevel(tempExp);
		        isUpgrade = true;
		        
	        }
	        else
	        {
		        if (GameDataManager.Instance.Level > 0)
		        {
			        var tempCfg = ConfigCenter.SecretCopyLevelCfgColl.GetDataById(GameDataManager.Instance.Level);
			        offsetExp = tempCfg.Total_exp_1;
		        }
	        }
	        ExpLevelTextStr = $"{GameDataManager.Instance.Level + 1 }";
	        var curLevelCfgId = GameDataManager.Instance.Level + 1;
	        var curCfg = ConfigCenter.SecretCopyLevelCfgColl.GetDataById(curLevelCfgId);
	        if (curCfg != null)
	        {
		        ExpSliderValue = (tempExp - offsetExp) / (float)curCfg.Exp;
	        }
        }
        private int CheckNextLevel(int endExp)
        {
	        var cfgId = GameDataManager.Instance.Level + 1;
	        var curLevelCfg = ConfigCenter.SecretCopyLevelCfgColl.GetDataById(cfgId);
	        if (curLevelCfg == null) return 0;
	        // 这里 是经验值到达这个值就可以升级
	        if (endExp >= curLevelCfg.Total_exp_1)
	        {
		        GameDataManager.Instance.Level += 1;
		        var curLevel = GameDataManager.Instance.Level;
		        GameManager.Instance.AddTalentRequestToList(curLevel);
		        return CheckNextLevel(endExp-curLevelCfg.Total_exp_1);
	        }
	        return endExp;
        }
        
        /// <summary>
        /// 排序函数
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
   //      public int Compare(object x, object y)
   //      {
	  //       if (!(x is SecretWeaponCellViewModel item1) ||
	  //           !(y is SecretWeaponCellViewModel item2)) return 0;
	  //       
	  //       // 先判断等级
	  //       if (item1.CurEquipModelCfg.Class != item2.CurEquipModelCfg.Class)
	  //       {
		 //        return item2.CurEquipModelCfg.Class.CompareTo(item1.CurEquipModelCfg.Class);
	  //       }
	  //       // 再判断品质
	  //       if(item1.CurEquipCfg.Quality != item2.CurEquipCfg.Quality)
	  //       {
		 //        return item2.CurEquipCfg.Quality.CompareTo(item1.CurEquipCfg.Quality);
	  //       }
			// // 最后判断id
	  //       return item1.CurEquipModelCfg.Id.CompareTo(item2.CurEquipModelCfg.Id);
   //      }

        public void UpdateWeaponScrollviewInfo()
        {
	        // var count = GameDataManager.Instance.BackpackWeaponList.Count;
	        // if (count < 5)
	        // {
		       //  cellSize = ECellItemSizeType.Size200X180;
		       //  ScrollviewSpacingX = 50;
		       //  ScrollviewSpacingY = 0;
		       //  ScrollviewRowCount = 5;
		       //  ScrollviewPaddingLeft = 0;
		       //  ScrollviewPaddingTop = 30;
	        // }
	        // else if(count < 6)
	        // {
		       //  cellSize = ECellItemSizeType.Size180X150;
		       //  ScrollviewSpacingX = 20;
		       //  ScrollviewSpacingY = 0;
		       //  ScrollviewRowCount = 20;
		       //  ScrollviewPaddingLeft = 0;
		       //  ScrollviewPaddingTop = 0;
	        // }
	        // else if(count < 7)
	        // {
		       //  cellSize = ECellItemSizeType.Size166X150;
		       //  ScrollviewSpacingX = 7;
		       //  ScrollviewSpacingY = 0;
		       //  ScrollviewRowCount = 6;
		       //  ScrollviewPaddingLeft = 0;
		       //  ScrollviewPaddingTop = 0;
	        // }
	        // else
	        // {
		       cellSize = ECellItemSizeType.Size90X80;
		       ScrollviewSpacingX = -70;
		       ScrollviewSpacingY = -70;
		       ScrollviewRowCount = 7;
		       ScrollviewPaddingLeft = 15;
		       ScrollviewPaddingTop = -70;
	        // }
        }
    }
}