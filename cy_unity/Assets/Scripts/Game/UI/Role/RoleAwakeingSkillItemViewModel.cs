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
    public partial class RoleAwakeingSkillItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private string lvBgPath;
		[AutoNotify] private string skilDesStr;

        [AutoNotify] private bool isLock;
        [AutoNotify] private bool heroTarget;
        [AutoNotify] private int star;
        [AutoNotify] private bool effectGo;
        [AutoNotify] private float skilDesStAlpha;
        public HeroSkillCfg MCfg;
        private RoleData Data => DataCenter.roleData;
        private int mRoleId;
        [Preserve]
        public RoleAwakeingSkillItemViewModel(HeroSkillCfg Cfg,int roleId)
        {
            MCfg = Cfg;
            mRoleId = roleId;
            InitUI();
            Data.PropertyChanged += StarChanged;
        }

        public void InitUI()
        {
            var isUnlock = Data.GetHeroStar(MCfg.HeroId) >= MCfg.LvAstrict;
            LvBgPath = isUnlock ? "hero[hero_panel_12]":"hero[hero_panel_8]";
            IsLock = !isUnlock;
            SkilDesStAlpha = (isUnlock ? 1f : 0.5f);
            var skillLanguage = ConfigCenter.HeroSkillLanguageCfgColl.GetDataById(MCfg.Id);
            SkilDesStr = SkillDes(skillLanguage.Dec,MCfg.Value);
            Star = MCfg.LvAstrict;
            HeroTarget = false; //mCfg.Effect == -1;
        }
        
        private string SkillDes(string Des,List<double> value)
        {
            if(value!=null)
                for (int i = 0; i < value.Count; i++)
                {
                    var temp = "{" + i + "}";
                    Des = Des.Replace(temp, "<color=#358801>" + value[i] +"</color>");
                    Des = Des.Replace("%", $"<color=#358801>%</color>");
                }

            return Des;
        }

        private void StarChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Data.HeroStarUpId)&& Data.GetHeroStar(mRoleId) == MCfg.LvAstrict )
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
            Data.PropertyChanged -= StarChanged;
            base.OnDispose();
        }
    }
}