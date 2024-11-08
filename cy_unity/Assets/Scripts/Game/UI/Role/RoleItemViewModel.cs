using System;
using System.ComponentModel;
using DH.Config;
using DH.Data;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;


namespace DH.Game.ViewModels
{
    public partial class RoleItemViewModel : ViewModelBase
    {
	    #region 组件 参数

	    [AutoNotify] private string roleIconBgPath;
	    [AutoNotify] private string roleIconPath;
	    [AutoNotify] private string levelTextStr;
	    [AutoNotify] private string nameStr;
	    [AutoNotify] private string shardNumsStr;
	    [AutoNotify] private string botBgPath;
	    
	    [AutoNotify] private bool showButtonsGo;
	    [AutoNotify] private bool showDeployGo;
	    [AutoNotify] private bool showLevelGo;
	    [AutoNotify] private bool showStar;
	    [AutoNotify] private bool showShard;
	    [AutoNotify] private bool redGo;
	    
	    [AutoNotify] private float shardSliderValue;
		
	    public bool IsUnLock;
	    public int RoleId;
	    private readonly Action<int,bool> callback;
	    private RoleData Data => DataCenter.roleData;

	    private bool isSelect;

	    [AutoNotify] private int star;
		
	    public bool IsSelect
	    {
		    get => isSelect;
		    set
		    {
			    isSelect = value;
			    ShowButtonsGo = isSelect && Data.IsUnlock(RoleId) && Data.FmtHero != RoleId;
			    PrSelect = isSelect;
		    }
	    }
	    [AutoNotify] private bool  prSelect;
	    #endregion
	    


		#region 配置表

		private HeroMainCfg cfg;

		public HeroStarCfg StarCfg
		{
			get
			{
				return ConfigCenter.HeroStarCfgColl.GetDataById(Data.GetHeroStar(RoleId));
			}
		}

		public int StarCos
		{
			get
			{
				if (Data.GetHeroLevel(RoleId) == 0)
					return cfg.UnlockItemNum;
				return StarCfg.StarCost;
			}
		}
		
		#endregion

		
        [Preserve]
        public RoleItemViewModel(int roleId, Action<int,bool> mCallback)
        {
	        RoleId = roleId;
	        callback = mCallback;
	        cfg = ConfigCenter.HeroMainCfgColl.GetDataById(RoleId);
	        InitUI();
	        Data.PropertyChanged += HeroInBattleEvent;
	        if (DataCenter.itemsData.ResourceDatas.TryGetValue(cfg.ItemId, out var data))
	        {
		        data.PropertyChanged += ShardChangeEvent;
	        }

	        RedShow();
        }

        /// <summary>
        /// 初始化ui
        /// </summary>
        private void InitUI()
        {
	        //英雄解锁
	        var isUnLock = Data.GetHeroLevel(RoleId) != 0;
	        BotBgPath = isUnLock ? "hero[hero_panel_1]":"hero[hero_panel_3]";
	        ShowDeployGo = Data.FmtHero == RoleId;//上阵
	        RoleIconBgPath = isUnLock
		        ? (cfg.Qlt switch
		        {
			        3 => "hero[heroCard_panel_1]",
			        4 => "hero[heroCard_panel_2]",
			        _ => "hero[heroCard_panel_3]"
		        }) :"hero[heroCard_panel_4]";
	        RoleIconPath = Data.HeroCardIcon(RoleId);
	        LevelTextStr = Data.GetHeroLevel(RoleId).ToString();
	        ShowLevelGo = isUnLock;
	        var value = Math.Min(DataCenter.itemsData.GetItemCount(cfg.ItemId),StarCos);
	        ShardSliderValue = value*1f / StarCos;
	        ShardNumsStr = DataCenter.itemsData.GetItemCount(cfg.ItemId) +"/"+ StarCos;
	        Star = Data.GetHeroStar(RoleId);
	        ShowStar = isUnLock;
	        ShowShard = !isUnLock;
        }


        #region 按钮事件

        [Command]
        private  void OnClickButton()
        {
	        // if (Data.FmtHero == RoleId || Data.GetHeroLevel(RoleId)==0)
	        // {
		       //  UIManager.Instance.OpenDialog<RoleInfoView>(new RoleInfoViewModel(RoleId)).Forget();
		       //  return;
	        // }
	        callback?.Invoke(RoleId,false);
        }
        
        [Command]
        private async void OnClickDeployButton()
        {
	        if (Data.GetHeroLevel(RoleId) == 0)return;
	        if (Data.FmtHero == RoleId)return;
	        var req = new ReqHeroInBattle();
	        req.HeroId = RoleId;
	        var result = await GameNetworkManager.Instance.SendAsync<RspHeroInBattle>(req);
	        if (NetHelper.CheckNetErrorMessage(result.rsp, true))
	        {
		        Data.HeroInBattle(RoleId);
	        }
        }
        private void HeroInBattleEvent(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(Data.FmtHero))
	        {
		        ShowDeployGo = Data.FmtHero == RoleId;//上阵
	        }

	        if (e.PropertyName is  nameof(Data.HeroStarUpId) or  nameof(Data.HeroLevelUpId))
	        {
		        Star = Data.GetHeroStar(RoleId);
		        LevelTextStr = Data.GetHeroLevel(RoleId).ToString();
	        }
        }
        private void ShardChangeEvent(object sender, PropertyChangedEventArgs e)
        {
	        InitUI();
        }

        protected override void OnDispose()
        {
	        Data.PropertyChanged -= HeroInBattleEvent;
	        if (DataCenter.itemsData.ResourceDatas.TryGetValue(cfg.ItemId, out var data))
	        {
		        data.PropertyChanged -= ShardChangeEvent;
	        }
	        base.OnDispose();
        }
        #endregion

        #region red
	        public void RedShow()
	        {
		        RedGo = Data.HeroRed(RoleId) || Data.IsCanGettingRole(RoleId);
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