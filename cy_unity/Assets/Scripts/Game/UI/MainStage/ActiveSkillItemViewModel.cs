using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.UIFramework;

namespace DH.Game.ViewModels
{
    public partial class ActiveSkillItemViewModel : ViewModelBase
    {
		[AutoNotify] private string iconPath;
		[AutoNotify] private float progress;
		[AutoNotify] private int curSkillId;
		private readonly Action<int> onClickCallback;
		public GameDataManager Manager => GameDataManager.Instance;
		
		
		[Preserve]
		public ActiveSkillItemViewModel(int skillId, Action<int> clickCallback)
        {
	        curSkillId = skillId;
	        onClickCallback = clickCallback;
	        var skillCfg = ConfigCenter.HeroSkillCfgColl.GetDataById(skillId);
	        IconPath = skillCfg.Icon;
	        UpdateProgress();
        }

        public override void Update()
        {
	        base.Update();
	        UpdateProgress();
        }

        private void UpdateProgress()
        {
	        Progress = GameDataManager.Instance.HeroActiveEnergyProgress();
        }

        [Command]
        private void OnClickSkillBtn()
        {
	        if (!Manager.IsCanUseHeroActiveSkill)
	        {
		        string str;
		        if (Manager.WaveEnd)
		        {
			        str = LocalizeHelper.GetGlobal(GlobalLanguageId.hero_tips_22);
		        }
		        else
		        {
			        str = LocalizeHelper.GetGlobal(GlobalLanguageId.hero_tips_23);
		        }
		        ToastManager.Show(str);
		        return;
	        }

	        onClickCallback.Invoke(curSkillId);
        }
    }
}