using System;
using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.Observables;
using DHFramework;
using Spine.Unity;
using UnityEngine;

namespace DH.Game.ViewModels
{
	public enum RoleShowType
	{
		Level,
		Awakening,
	}

	public partial class RoleInfoViewModel : ViewModelBase
    {
	    #region 组件 属性

	    public CommonTopViewModel TopVm;
	    [AutoNotify] private int quality;
	    [AutoNotify] private string heroNameStr;
	    [AutoNotify] private string attrHPDesStr;
	    [AutoNotify] private string attrDEFDesStr;
	    [AutoNotify] private string skillIconPath;
	    [AutoNotify] private string skillNameStr;
	    [AutoNotify] private string skillDesStr;
	    [AutoNotify] private string heroLevelStr;
	    
	    [AutoNotify] private bool showUpArrow;
	    
	    [AutoNotify] private bool lvUpEffectGo;
	    [AutoNotify] private bool lvUpMainEffectGo;
	    [AutoNotify] private bool starUpMainEffectGo;
	    
	    [AutoNotify] private ObservableList<RoleLevelSkillItemViewModel> levelEffectDesList = new();
	    [AutoNotify] private ObservableList<RoleAwakeingSkillItemViewModel> awakeUpEffectDesList = new();
	    [AutoNotify] private ObservableList<ItemPriceNodeModel> cosScrollviewList = new();

	    [AutoNotify] private string shardBgPath;
	    [AutoNotify] private string roleIconPath;
	    [AutoNotify] private float shardSliderValue;
	    [AutoNotify] private string shardNumsStr;
	    
	    [AutoNotify] private bool showRestBut;
	    [AutoNotify] private bool showAwakeUpBut;
	    [AutoNotify] private bool showGettingBut;
	    [AutoNotify] private bool showLevelUpBut;
	    [AutoNotify] private bool showFastLevelUpBut;
	    [AutoNotify] private Color awakeUpButTextColor;
	    [AutoNotify] private Color gettingButTextColor;
	    
	    [AutoNotify] private bool levelEffectDesGo;
	    [AutoNotify] private string levelEffectDes;
	    [AutoNotify] private bool cosGo;
	    [AutoNotify] private bool shardGo;
	    [AutoNotify] private bool maxLvDesGo;
	    [AutoNotify] private bool maxStarDesGo;
	    
	    [AutoNotify] private bool showDiaBuyBut;
	    public ItemPriceNodeModel DiaBuyPriceNodeModel;
	    [AutoNotify] private bool showMoneyBuyBut;
	    public BtnPriceNodeModel MoneyBuyPriceNodeModel;
	    
	    [AutoNotify] private int jumpScrollPos;
	    [AutoNotify] private int jumpSkillScrollPos;
	    
	    [AutoNotify] RoleShowType showType;
		
	    public int RoleId;
	    private RoleData Data => DataCenter.roleData;

	    [AutoNotify]  int star;
        
	    
	    //红点
	    [AutoNotify] private bool levelUpRed;
	    [AutoNotify] private bool awakeUpRed;
	    [AutoNotify] private bool gettingRed;
	    
	    #endregion
	    
		#region 配置表

		private HeroMainCfg cfg;
		public HeroLevelCfg levelCfg
		{
			get
			{
				var level = Data.GetHeroLevel(RoleId);
				if (level == 0)//没解锁展示一级
				{
					return ConfigCenter.HeroLevelCfgColl.GetDataById(1);
				}
				return ConfigCenter.HeroLevelCfgColl.GetDataById(level);
			}
		}
		private HeroSkillCfg mainSkillCfg;
		public HeroSkillCfg MainSkillCfg
		{
			get
			{
				if (mainSkillCfg == null)
				{
					mainSkillCfg = ConfigCenter.HeroSkillCfgColl.GetDataById(RoleId);
				}

				return mainSkillCfg;
			}

		}

		public HeroStarCfg StarCfg
		{
			get
			{
				return ConfigCenter.HeroStarCfgColl.GetDataById(Data.GetHeroStar(RoleId));
			}
		}

		/// <summary>
		/// 英雄技能表（升级+觉醒）
		/// </summary>
		private List<HeroSkillCfg> skillCfgs;
		List<HeroSkillCfg> SkillCfgs
		{
			get
			{
				if (skillCfgs == null)
				{
					var items = ConfigCenter.HeroSkillCfgColl.DataItems;
					skillCfgs = new();
					for (int i = 0; i < items.Count; i++)
					{
						if (items[i].HeroId == RoleId)
						{
							skillCfgs.Add(items[i]);
						}
					}
				}
				return skillCfgs;
			}
		}

		#endregion

		#region 属性

		public List<DH.Config.Attribute> attribute
		{
			get
			{
				var attr = levelCfg.AttrAdd[cfg.Qlt switch
				{
					3 => 0,
					4 => 1,
					_ => 2
				}];
				return attr;
			}
		}
		/// <summary>
		/// 升级消耗
		/// </summary>
		public List<Reward> levelcos
		{
			get
			{
				if (Data.IsMaxLevel2(RoleId)) return new List<Reward>();
				var rewards = levelCfg.LevelCost[cfg.Qlt switch
				{
					3 => 0,
					4 => 1,
					_ => 2
				}];
				return rewards;
			}
		}
		/// <summary>
		/// 升星消耗取
		/// </summary>
		public int StarCos
		{
			get
			{
				if (!Data.IsUnlock(RoleId))
					return cfg.UnlockItemNum;
				return StarCfg.StarCost;
			}
		}
		

		#endregion

		#region spine
		[AutoNotify] private bool isShowChapterEffectNode;
		private GameObject effectParentNode;
		private readonly string effectPath = "UI/Role/MapEffect/";
		private SkeletonGraphic curSpine;
		public GameObject EffectParentNode
		{
			get=> null;
			set
			{
				effectParentNode = value;
				if (effectParentNode != null)
				{
					UpdateChapterMapEffect();
				}
			}
		}
		private async UniTaskVoid UpdateChapterMapEffect()
		{
			for (int i = 0; i < effectParentNode.transform.childCount; i++)
			{
				var child = effectParentNode.transform.GetChild(i);
				AssetsManager.ReleaseInstance(child.gameObject);
			}
			curSpine = null;
			var path = $"{effectPath}{cfg.Model}";
			var effectNode= await AssetsManager.InstantiateWithParentAsync(path, effectParentNode.transform, false);
			if (effectNode == null) return;
			curSpine = effectNode.GetComponent<SkeletonGraphic>();
			if (curSpine == null) return;
			IsShowChapterEffectNode = true;
			//curSpine.AnimationState.SetAnimation(0, GameConst.MapAniName.Idle, false);
		}

		#endregion
		
		[Preserve]
        public RoleInfoViewModel(int mRoleId ,RoleShowType mShowType)
        {
	        RoleId = mRoleId;
	        cfg = ConfigCenter.HeroMainCfgColl.GetDataById(RoleId);
	        ShowType = mShowType;
	        
	        List<int> list = new List<int>() {(int)GameConst.ItemIdCode.HeroUpRes, (int)GameConst.ItemIdCode.Money, (int)GameConst.ItemIdCode.Stone};
	        TopVm = new(list);

	        InitAllUI();
	        Data.PropertyChanged += HeroLevelUpEvent;
        }
        protected override void OnDispose()
        {
	        Data.PropertyChanged -= HeroLevelUpEvent;
	        foreach (var item in levelEffectDesList)
	        {
		        item.Dispose();
	        }
	        foreach (var item in awakeUpEffectDesList)
	        {
		        item.Dispose();
	        }
	        foreach (var item in cosScrollviewList)
	        {
		        item.Dispose();
	        }
	        TopVm.Dispose();
	        DiaBuyPriceNodeModel?.Dispose();
	        MoneyBuyPriceNodeModel?.Dispose();
			base.OnDispose();
        }
        private void InitAllUI()
        {
	        InitHeroUI();
	        InitLevelAndAwakeUpUI();
	        InitBotButtons();
	        RedShow();
        }

        /// <summary>
        /// 初始化英雄相关UI
        /// </summary>
        private void InitHeroUI()
        {
	        Quality = cfg.Qlt;
	        HeroNameStr = ConfigCenter.HeroMainLanguageCfgColl.GetDataById(MainSkillCfg.Id).Name;
		        
	        var nameCfg = ConfigCenter.AttributesCfgColl.GetDataByName(cfg.Attr[1].Type);
	        var attr = Data.GetAttribute(RoleId,nameCfg.Id);
	        string AddAttr = "";
	        if (Data.IsUnlock(RoleId)&&!Data.IsMaxLevel2(RoleId))
	        {
		        var now = Data.GetLvAttribute(RoleId,nameCfg.Id,Data.GetHeroLevel(RoleId));
		        var next = Data.GetLvAttribute(RoleId,nameCfg.Id,Data.GetHeroLevel(RoleId)+1);
		        AddAttr =$"<color=#80F040>(+{next.Value-now.Value})</color>";
	        }
	        AttrHPDesStr =UIHelper.ParseValueToStringByType((int)Math.Floor(attr), (GameConst.ENumType)nameCfg.ShowType,nameCfg.Type)+AddAttr;
	        
	        nameCfg = ConfigCenter.AttributesCfgColl.GetDataByName(cfg.Attr[0].Type);
	        attr = Data.GetAttribute(RoleId,nameCfg.Id);
	        if (Data.IsUnlock(RoleId)&&!Data.IsMaxLevel2(RoleId))
	        {
		        var now = Data.GetLvAttribute(RoleId,nameCfg.Id,Data.GetHeroLevel(RoleId));
		        var next = Data.GetLvAttribute(RoleId,nameCfg.Id,Data.GetHeroLevel(RoleId)+1);
		        AddAttr =$"<color=#80F040>(+{next.Value-now.Value})</color>";
	        }
	        AttrDEFDesStr = UIHelper.ParseValueToStringByType((int)Math.Floor(attr), (GameConst.ENumType)nameCfg.ShowType,nameCfg.Type)+AddAttr;

	        SkillIconPath = MainSkillCfg.Icon;
	        SkillNameStr = ConfigCenter.HeroSkillLanguageCfgColl.GetDataById(MainSkillCfg.Id).Name;
	        SkillDesStr =SkillDes(ConfigCenter.HeroSkillLanguageCfgColl.GetDataById(MainSkillCfg.Id).Dec,MainSkillCfg.Value);
	        HeroLevelStr = string.Format(ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.Shop_tips01).Name,levelCfg.Id);
	        Star = Data.GetHeroStar(RoleId);
	        RoleIconPath = Data.HeroCardIcon(RoleId);
	        ShowUpArrow = !Data.IsMaxLevel2(RoleId)&&Data.IsUnlock(RoleId);
        }

        /// <summary>
        /// 初始化升级觉醒相关
        /// </summary>
        private void InitLevelAndAwakeUpUI()
        {

	        if (LevelEffectDesList.Count == 0)
	        {
		        LevelEffectDesList.Clear();
		        AwakeUpEffectDesList.Clear();
		        for (int i = 0; i < SkillCfgs.Count; i++)
		        {
			        if (SkillCfgs[i].SkillType == 3)//1 英雄技能  2 星级技能   3等级技能
			        {
				        LevelEffectDesList.Add(new RoleLevelSkillItemViewModel(SkillCfgs[i],RoleId));
			        }
			        else if (SkillCfgs[i].SkillType == 2)
			        {
				        AwakeUpEffectDesList.Add(new RoleAwakeingSkillItemViewModel(SkillCfgs[i],RoleId));
			        }
		        }
	        }
	        else
	        {
		        for (int i = 0; i < LevelEffectDesList.Count; i++)
		        {
			        LevelEffectDesList[i].InitUI();
		        }
		        for (int i = 0; i < AwakeUpEffectDesList.Count; i++)
		        {
			        AwakeUpEffectDesList[i].InitUI();
		        }
	        }

	        
	        //升级消耗
	        cosScrollviewList.Clear();
	        for (int i = 0; i < levelcos.Count; i++)
	        {
		        ItemPriceNodeModel CosVm = new(levelcos[i], true, null);
		        CosVm.IsShowBg = true;
		        CosScrollviewList.Add(CosVm);

	        }		        
	        
	        
	        //升星消耗
	        Reward Rew = new Reward(RewardType.Item,cfg.ItemId,StarCos);
	        ShardBgPath = UIHelper.GetRewardsIconPath(Rew);
	        var value = Math.Min(DataCenter.itemsData.GetItemCount(cfg.ItemId),StarCos);
	        ShardSliderValue = value*1f/ StarCos;
	        ShardNumsStr = $"{DataCenter.itemsData.GetItemCount(cfg.ItemId)}/{StarCos}";
        }


        /// <summary>
        /// 初始化底部按钮显示相关
        /// </summary>
        private void InitBotButtons()
        {
			//重置按钮
			ShowRestBut = Data.GetHeroLevel(RoleId) > 1 && showType == RoleShowType.Level;
			//觉醒按钮
			ShowAwakeUpBut = Data.IsUnlock(RoleId) && showType == RoleShowType.Awakening && !Data.IsMaxStar(RoleId);//&& Data.IsCanStarUp(RoleId)
			AwakeUpButTextColor = Data.IsCanStarUp(RoleId)
				? UIHelper.HexColorStrToColor("#FFF8E4")
				: UIHelper.HexColorStrToColor("#FF0411");
			
			//激活按钮
			ShowGettingBut =  !Data.IsUnlock(RoleId); //&& Data.IsCanGettingRole(RoleId)
			GettingButTextColor = Data.IsCanGettingRole(RoleId)
				? UIHelper.HexColorStrToColor("#FFF8E4")
				: UIHelper.HexColorStrToColor("#FF0411");
			
			
			//满星提示
			MaxStarDesGo = Data.IsUnlock(RoleId) && showType == RoleShowType.Awakening && Data.IsMaxStar(RoleId);
			LevelEffectDesGo  = Data.IsUnlock(RoleId) && showType == RoleShowType.Level && !Data.IsMaxLevel2(RoleId) && Data.IsMaxLevel(RoleId);
			if (!Data.IsMaxStar(RoleId))
			{
				var tempcfg =
					ConfigCenter.HeroStarCfgColl.GetDataById(Data.GetHeroStar(RoleId) + 1);
				LevelEffectDes  = string.Format(ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.hero_tips_13).Name,tempcfg.Id,tempcfg.LevelLimitAdd);
			}

			//消耗展示
			CosGo =  Data.IsUnlock(RoleId) && showType == RoleShowType.Level && !Data.IsMaxLevel2(RoleId) && !Data.IsMaxLevel(RoleId);
			//碎片展示
			ShardGo =  Data.GetHeroLevel(RoleId) <= 0 || (showType == RoleShowType.Awakening && !Data.IsMaxStar(RoleId)) ;

			//升级
			ShowLevelUpBut = Data.IsUnlock(RoleId) && showType == RoleShowType.Level && !Data.IsMaxLevel2(RoleId);
			
			//满级提示
			MaxLvDesGo = Data.IsUnlock(RoleId) && showType == RoleShowType.Level && Data.IsMaxLevel2(RoleId);
				
			//快速升级
			ShowFastLevelUpBut = Data.IsUnlock(RoleId) && showType == RoleShowType.Level && !Data.IsMaxLevel2(RoleId);
			
			//钻石解锁
			ShowDiaBuyBut = false;
			if (cfg.Unlock == 1)//1钻石 2 付费  其他默认
			{
				ShowDiaBuyBut = !Data.IsUnlock(RoleId);
				if (DiaBuyPriceNodeModel == null)
				{
					DiaBuyPriceNodeModel = new(cfg.GemCost[0],true,null);
					DiaBuyPriceNodeModel.IsShowBg = false;
				}
			}
			 
			//付费解锁
			ShowMoneyBuyBut = false;
			if (cfg.Unlock == 2)//1钻石 2 付费  其他默认
			{
				ShowMoneyBuyBut = !Data.IsUnlock(RoleId);
				if (MoneyBuyPriceNodeModel == null)
				{
					var packageCfg = ConfigCenter.PackageCfgColl.GetDataById(cfg.PackageId);
					var cfgInfo = ConfigCenter.RechargeCfgColl.GetDataById(packageCfg.RechargeId);
					string priceStr = "";
					if (cfgInfo != null) priceStr = PayController.Instance.GetLocalizePrice(cfgInfo.PayId);
					MoneyBuyPriceNodeModel = new BtnPriceNodeModel(priceStr);
				}
			}

			FjumpScrollPos();
        }

        #region  底部按钮事件相关
                
        /// <summary>
        /// 重置英雄
        /// </summary>
        [Command]
        private void OnClickRestBut()
        {
	        UIManager.Instance.OpenDialog<RoleTierDownView>(new RoleTierDownViewModel(RoleId)).Forget();
        }

        /// <summary>
        /// 觉醒
        /// </summary>
        [Command]
        private async void OnClickAwakeUpBut()
        {
	        if (!Data.IsUnlock(RoleId))return;
	        
	        if (Data.IsMaxStar(RoleId))
	        {
		        ToastManager.Show(ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.hero_tips_15).Name);
		        return;
	        }
	        
	        Reward Rew = new Reward(RewardType.Item,cfg.ItemId,StarCfg.StarCost);
	        if (!UIHelper.CheckRewardIsEnough(Rew,true))return;
	        
	        var req = new ReqHeroStarUp();
	        req.HeroId = RoleId;
	        var result = await GameNetworkManager.Instance.SendAsync<RspHeroStarUp>(req);
	        if (NetHelper.CheckNetErrorMessage(result.rsp, true))
	        {
		        Lodash.DealRewards(new List<Resource>(result.rsp.Costs),false);
		        if (Data.Heroes.ContainsKey(RoleId))
		        {
			        Data.Heroes[RoleId].Star = result.rsp.Star;
			        Data.HeroStarUp(RoleId);
		        }
		        ShowEffect(false);
		        InitAllUI();
		        AudioManager.Instance.PlayEquipLevelUp();
		        
	        }
        }
        /// <summary>
        /// 激活
        /// </summary>
        [Command]
        private async void OnClickGettingBut()
        {
	        //if (!Data.IsCanGettingRole(RoleId))return;
	        
	        Reward Rew = new Reward(RewardType.Item,cfg.ItemId,cfg.UnlockItemNum);
	        if (!UIHelper.CheckRewardIsEnough(Rew,true))return;
	        
	        var req = new ReqHeroUnlock();
	        req.HeroId = RoleId;
	        req.Op = 0;
	        var result = await GameNetworkManager.Instance.SendAsync<RspHeroUnlock>(req);
	        if (NetHelper.CheckNetErrorMessage(result.rsp, true))
	        {
		        Lodash.DealRewards(new List<Resource>(result.rsp.Costs),false);
		        Lodash.DealRewards(result.rsp.Rewards.ToList());
		        if (!Data.Heroes.ContainsKey(RoleId))
		        {
			        var temp = new HeroInfo();
			        temp.Star = 0;
			        temp.Lv = 1;
			        Data.Heroes.Add(RoleId,temp);
		        }
		        Data.HeroStarUp(RoleId);
		        InitAllUI();
		        UIManager.Instance.OpenDialog<NewRoleShowView>(new NewRoleShowViewModel(RoleId)).Forget();
		        AudioManager.Instance.PlayLevelUp();
	        }
        }
        /// <summary>
        /// 升级
        /// </summary>
        [Command]
        private async void OnClickLevelUpBut()
        {
	        if (Data.IsMaxLevel(RoleId))
	        {
		        ToastManager.Show(ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.hero_tips_14).Name);
		        return;
	        }

	        if (!UIHelper.CheckRewardIsEnough(levelcos,isJump:true))return;
	        
	        var req = new ReqHeroLvUp();
	        req.HeroId = RoleId;
	        var result = await GameNetworkManager.Instance.SendAsync<RspHeroLvUp>(req);
	        if (NetHelper.CheckNetErrorMessage(result.rsp, true))
	        {
		        Lodash.DealRewards(new List<Resource>(result.rsp.Costs),false); 
		        Data.Heroes[RoleId].Lv = result.rsp.Lv;
		        Data.HeroLevelUp(RoleId);
		        ShowEffect(true);
		        AudioManager.Instance.PlayEquipLevelUp();
	        }
        }

        private void HeroLevelUpEvent(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(Data.HeroLevelUpId) &&  Data.HeroLevelUpId == RoleId)
	        {
		        InitAllUI();
	        }
        }

        /// <summary>
        /// 快速升级
        /// </summary>
        [Command]
        private async void OnClickFastLevelUpBut()
        {
	        if (Data.IsMaxLevel(RoleId))
	        {
		        ToastManager.Show(ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.hero_tips_14).Name);
		        return;
	        }
	        
	        if (!UIHelper.CheckRewardIsEnough(levelcos,isJump:true))return;
	        
	        var req = new ReqHeroLvUpAuto();
	        req.HeroId = RoleId;
	        var result = await GameNetworkManager.Instance.SendAsync<RspHeroLvUpAuto>(req);
	        if (NetHelper.CheckNetErrorMessage(result.rsp, true))
	        {
		        Lodash.DealRewards(new List<Resource>(result.rsp.Costs),false);
		        Data.Heroes[RoleId].Lv = result.rsp.Lv;
		        Data.HeroLevelUp(RoleId);
		        ShowEffect(true);
		        AudioManager.Instance.PlayEquipLevelUp();
	        }
        }

        [Command]
        private void OnClickClose()
        {
	        UIManager.Instance.CloseDialog<RoleInfoView>();
        }
        /// <summary>
        /// 钻石购买
        /// </summary>
        [Command]
        private async void OnClickDiaBuyBut()
        {

	        if (!UIHelper.CheckRewardIsEnough(cfg.GemCost[0],true))return;
	        
	        var req = new ReqHeroUnlock();
	        req.HeroId = RoleId;
	        req.Op = 1;
	        var result = await GameNetworkManager.Instance.SendAsync<RspHeroUnlock>(req);
	        if (NetHelper.CheckNetErrorMessage(result.rsp, true))
	        {
		        Lodash.DealRewards(result.rsp.Rewards.ToList());
		        Lodash.DealRewards(new List<Resource>(result.rsp.Costs),false);
		        if (!Data.Heroes.ContainsKey(RoleId))
		        {
			        var temp = new HeroInfo();
			        temp.Star = 0;
			        temp.Lv = 1;
			        Data.Heroes.Add(RoleId,temp);
		        }
		        UIManager.Instance.OpenDialog<NewRoleShowView>(new NewRoleShowViewModel(RoleId)).Forget();
		        InitAllUI();
	        }
        }
        
        /// <summary>
        /// 付费购买
        /// </summary>
        [Command]
        private async void OnClickMoneyBuyBut()
        {
	    //     PayController.Instance.Pay(cfg.PackageId, callback:(result) =>
	    //     {
		   //      if (result == null || result.Status != 0)
		   //      {
					// DHLog.Error($"付费解锁英雄失败{RoleId}");
			  //       return;
		   //      }
		   //      Lodash.DealRewards(result.Rewards.ToList());
		   //      if (!Data.Heroes.ContainsKey(RoleId))
		   //      {
			  //       var temp = new HeroInfo();
			  //       temp.Star = 0;
			  //       temp.Lv = 1;
			  //       Data.Heroes.Add(RoleId,temp);
		   //      }
		   //      InitAllUI();
		   //      UIManager.Instance.OpenDialog<NewRoleShowView>(new NewRoleShowViewModel(RoleId)).Forget();
	    //     });
	        
	        ShopManager.Instance.SendBuyPackageBuyRecharge(cfg.PackageId,null,0,-1,1, (rewardList,costList) =>
	        {
		        Lodash.DealRewards(rewardList,costList);
		        if (!Data.Heroes.ContainsKey(RoleId))
		        {
			        var temp = new HeroInfo();
			        temp.Star = 0;
			        temp.Lv = 1;
			        Data.Heroes.Add(RoleId,temp);
		        }
		        InitAllUI();
		        UIManager.Instance.OpenDialog<NewRoleShowView>(new NewRoleShowViewModel(RoleId)).Forget();
	        });
        }
        
        #endregion
        
        private string SkillDes(string Des,List<double> value)
        {

	        for (int i = 0; i < value.Count; i++)
	        {
		        var temp = "{" + i + "}";
		        Des = Des.Replace(temp, value[i].ToString());
	        }

	        return Des;
        }

        private void ShowEffect(bool isLevel)
        {
	        LvUpEffectGo = false;
	        LvUpEffectGo = true;
	        if (isLevel)
	        {
		        LvUpMainEffectGo = false;
		        LvUpMainEffectGo = true;
	        }
	        else
	        {
		        StarUpMainEffectGo = false;
		        StarUpMainEffectGo = true;
	        }
        }

        private void FjumpScrollPos()
        {
	        if (showType == RoleShowType.Awakening)
	        {
		        jumpScrollPos = -1;
		        for (int i = 0; i < awakeUpEffectDesList.Count; i++)
		        {
			        if ( Data.GetHeroStar(RoleId) == awakeUpEffectDesList[i].MCfg.LvAstrict)
			        {
				        JumpScrollPos = i;
				        break;
			        }
		        } 
	        }

	        if (showType == RoleShowType.Level)
	        {
		        JumpSkillScrollPos = -1;
		        for (int i = 0; i < levelEffectDesList.Count; i++)
		        {
			        if (Data.GetHeroLevel(RoleId) <= levelEffectDesList[i].MCfg.LvAstrict)
			        {
				        if (Data.GetHeroLevel(RoleId) < levelEffectDesList[i].MCfg.LvAstrict)
				        {
					        JumpSkillScrollPos = i-1;
					        return;
				        }

				        JumpSkillScrollPos = i;
				        return;
			        }
		        } 
	        }
        }

        #region 页签切换事件

        [Command]
        private async void OnClickTabLevelBut()
        {
	        if (ShowType == RoleShowType.Level)return;
	        ShowType = RoleShowType.Level;
	        InitLevelAndAwakeUpUI();
	        InitBotButtons();
	        RedShow();
        }
        [Command]
        private async void OnClickTabAwakeBut()
        {
	        if (ShowType == RoleShowType.Awakening)return;
	        ShowType = RoleShowType.Awakening;
	        InitLevelAndAwakeUpUI();
	        InitBotButtons();
	        RedShow();
        }
        #endregion
        #region 红点
        public void RedShow()
        {
	        LevelUpRed = Data.IsCanLevelUp(RoleId);
	        AwakeUpRed = Data.IsCanStarUp(RoleId);
	        GettingRed = Data.IsCanGettingRole(RoleId);
        }
        private float time;
        public override void Update()
        {
	        base.Update();
	        if (!UIHelper.CalculateTime(ref time)) return;
	        RedShow();
        }
        #endregion
    }
}