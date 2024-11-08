using System.Collections.Generic;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;



namespace DH.Game.ViewModels
{
    public partial class RoleLevelSkillItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private string lvBgPath;
		[AutoNotify] private string lvDesStr;
		[AutoNotify] private string skillDesStr;
		[AutoNotify] private bool showLockGo;
		[AutoNotify] private bool heroTarget;
		[AutoNotify] private bool effectGo;
		[AutoNotify] private float skilDesStAlpha;
		public HeroSkillCfg MCfg;
		private RoleData Data => DataCenter.roleData;
		public int MRoleId;
        [Preserve]
        public RoleLevelSkillItemViewModel(HeroSkillCfg Cfg,int roleId)
        {
	        MCfg = Cfg;
	        MRoleId = roleId;
	        InitUI();
	        Data.PropertyChanged += LevelChanged;
        }

        public void InitUI()
        {
	        var isUnlock = Data.GetHeroLevel(MCfg.HeroId) >= MCfg.LvAstrict;
	        LvBgPath = isUnlock ? "hero[hero_panel_12]":"hero[hero_panel_8]";
	        ShowLockGo = !isUnlock;
	        LvDesStr = string.Format(ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.Shop_tips01).Name,MCfg.LvAstrict);
	        var skillLanguage = ConfigCenter.HeroSkillLanguageCfgColl.GetDataById(MCfg.Id);
	        SkillDesStr = SkillDes(skillLanguage.Dec,MCfg.Value); 
	        HeroTarget = MCfg.Effect == -1;
	        SkilDesStAlpha = (isUnlock ? 1f : 0.5f);
        }
        
        private string SkillDes(string Des,List<double> value)
        {
	        if(value!=null)
	        for (int i = 0; i < value.Count; i++)
	        {
		        var temp ="{" + i + "}";
		        Des = Des.Replace(temp, "<color=#358801>" + value[i] +"</color>") ;
		        Des = Des.Replace("%", $"<color=#358801>%</color>");
	        }

	        return Des;
        }
        
        private void LevelChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(Data.HeroLevelUpId) && Data.GetHeroLevel(MRoleId) ==  MCfg.LvAstrict)
	        {
		        RefreshRec();
	        }
        }
        async void RefreshRec()
        {
	        await UniTask.Delay(200);
	        EffectGo = false;
	        EffectGo = true;
	        await UniTask.Delay(200);
	        EffectGo = false;
        }
        protected override void OnDispose()
        {
	        Data.PropertyChanged -= LevelChanged;
	        base.OnDispose();
        }
    }
}