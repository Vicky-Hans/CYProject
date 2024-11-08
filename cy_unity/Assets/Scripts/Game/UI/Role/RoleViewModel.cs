using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;
using Game.UI.MainUi;
using Spine.Unity;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class RoleViewModel : ViewModelBase
    {
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
        [AutoNotify] private int star;
        
        [AutoNotify] private ObservableList<RoleItemViewModel> scrollViewList=new();
        
        [AutoNotify] private bool isOpenState;
        private RoleData Data => DataCenter.roleData;
        public MainUiManager MainUI = MainUiManager.Instance;

        public int RoleId;
        
        //红点
        [AutoNotify] private bool levelUpRed;
        [AutoNotify] private bool awakeUpRed;
        [AutoNotify] private bool gettingRed;
        
        #region 配置表

        private HeroMainCfg cfg
        {
            get => ConfigCenter.HeroMainCfgColl.GetDataById(RoleId);
        }

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
        public HeroSkillCfg MainSkillCfg => ConfigCenter.HeroSkillCfgColl.GetDataById(RoleId);
       

        #endregion
        
        [Preserve]
        public RoleViewModel()
        {
            RoleId = DataCenter.roleData.FmtHero;
            InitUI();
            Data.Heroes.PropertyChanged += HerosChange;
            Data.PropertyChanged += HeroLevelUpEvent;
            MainUI.PropertyChanged += OnMainUiChanged;
            RedShow();
        }

        public void InitUI()
        {
            ScrollViewList.Clear();
            var items = ConfigCenter.HeroMainCfgColl.DataItems;
            var temp1 = new List<RoleItemViewModel>();
            for (int i = 0; i < items.Count; i++)
            {
                var Vm = new RoleItemViewModel(items[i].Id, SelectHero);
                temp1.Add(Vm);
            }
            
            temp1 = temp1.OrderByDescending(o => Data.FmtHero == o.RoleId)
                .ThenByDescending(o => Data.GetHeroStar(o.RoleId))
                .ThenByDescending(o => Data.GetHeroLevel(o.RoleId)).
                ThenByDescending(o => ConfigCenter.HeroMainCfgColl.GetDataById(o.RoleId).Qlt).
                ThenBy(o => o.RoleId).ToList();

            ScrollViewList.AddRange(temp1);
            SelectHero(RoleId,true);

        }

        public void SelectHero(int id, bool isFrist = false)
        {
            if (!isFrist && RoleId == id)return;
            RoleId = id;
            InitHeroUI();
            if (!isFrist)
                UpdateChapterMapEffect();
            for (int i = 0; i < scrollViewList.Count; i++)
            {
                var temp = scrollViewList[i];
                temp.IsSelect = temp.RoleId == RoleId;
            }
        }
        private void HeroLevelUpEvent(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(Data.FmtHero) or nameof(Data.HeroLevelUpId) or nameof(Data.HeroStarUpId))
            {
                InitUI();
            }
        }
        private void OnMainUiChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName is nameof(MainUI.CurTabType))
            {
                //SelectHero(0);
            }
        }
        private void HerosChange(object sender, PropertyChangedEventArgs e)
        {
            InitUI();
        }
        
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
            HeroMainCfg cfg = ConfigCenter.HeroMainCfgColl.GetDataById(RoleId);
            var path = $"{effectPath}{cfg.Model}";
            var effectNode= await AssetsManager.InstantiateWithParentAsync(path, effectParentNode.transform, false);
            if (effectNode == null) return;
            curSpine = effectNode.GetComponent<SkeletonGraphic>();
            if (curSpine == null) return;
            IsShowChapterEffectNode = true;
            //curSpine.AnimationState.SetAnimation(0, GameConst.MapAniName.Idle, false);
        }

        #endregion

        #region 顶部UI初始化

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
	        //RoleIconPath = Data.HeroCardIcon(RoleId);
	        ShowUpArrow = !Data.IsMaxLevel2(RoleId)&&Data.IsUnlock(RoleId);
            InitButs();
            RedShow();
        }
        private string SkillDes(string Des,List<double> value)
        {

            for (int i = 0; i < value.Count; i++)
            {
                var temp = "{" + i + "}";
                Des = Des.Replace(temp, value[i].ToString());
            }

            return Des;
        }

        #endregion


        #region 打开按钮

        [AutoNotify] private bool showAwakeBut;
        [AutoNotify] private bool showLevelUpBut;
        [AutoNotify] private bool showGettingBut;
        
        /// <summary>
        /// 初始化按钮
        /// </summary>
        private void InitButs()
        {
            ShowAwakeBut = Data.IsUnlock(RoleId);
            ShowLevelUpBut = Data.IsUnlock(RoleId);
            ShowGettingBut = !Data.IsUnlock(RoleId);
        }

        /// <summary>
        /// 觉醒
        /// </summary>
        [Command]
        private void OnClickAwakeBut()
        {
            UIManager.Instance.OpenDialog<RoleInfoView>(new RoleInfoViewModel(RoleId,RoleShowType.Awakening)).Forget();
        }
        /// <summary>
        /// 升级
        /// </summary>
        [Command]
        private void OnClickLevelUpBut()
        {
            UIManager.Instance.OpenDialog<RoleInfoView>(new RoleInfoViewModel(RoleId,RoleShowType.Level)).Forget();
        }
        /// <summary>
        /// 激活
        /// </summary>
        [Command]
        private void OnClickGettingBut()
        {
            UIManager.Instance.OpenDialog<RoleInfoView>(new RoleInfoViewModel(RoleId,RoleShowType.Awakening)).Forget();
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
        
        protected override void OnDispose()
        {
            Data.Heroes.PropertyChanged -= HerosChange;
            Data.PropertyChanged -= HeroLevelUpEvent;
            MainUI.PropertyChanged -= OnMainUiChanged;
            foreach (var item in scrollViewList)
            {
                item.Dispose();
            }

            scrollViewList.Clear();

            base.OnDispose();
        }
    }
}