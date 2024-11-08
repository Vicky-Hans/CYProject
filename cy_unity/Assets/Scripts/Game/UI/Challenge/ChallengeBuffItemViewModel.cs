using System;
using DH.Config;
using UnityEngine;
using DH.UIFramework;
using UnityEngine.Scripting;
using DH.UIFramework.ViewModels;
namespace DH.Game.ViewModels
{
    public partial class ChallengeBuffItemViewModel : ViewModelBase
    {
        [AutoNotify] private string buffIconStr;
        [AutoNotify] private int buffId;
        [Preserve]
        public ChallengeBuffItemViewModel(int buffId)
        {
            BuffId = buffId;
            var cfg = ConfigCenter.DailyStageBuffCfgColl.GetDataById(BuffId);
            if (cfg != null) BuffIconStr = $"daily_buff[{cfg.Icon}]";
        }
        [Command]
        private void OnClickBuff(Tuple<Vector3, Vector3> info)
        {
            var languageCfg = ConfigCenter.DailyStageBuffLanguageCfgColl.GetDataById(BuffId);
            if (languageCfg == null) return;
            var cfg = ConfigCenter.DailyStageBuffCfgColl.GetDataById(BuffId);
            if (cfg == null) return;
            var title = LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_Title);;
            var desc = string.Format(languageCfg.Dec,cfg.Value[0]);
            UIHelper.OpenItemTips(title,desc, info);
        }
    }
}