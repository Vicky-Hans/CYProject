using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class MainStagePauseViewModel : ViewModelBase
    {
        
		 [AutoNotify] private ObservableList<TalentCellItemViewModel> talentScrollViewList = new();
		 private bool effectState = DataCenter.userinfo.EffectState;
		 public bool EffectState
		 {
			 get => effectState;
			 set
			 {
				 DataCenter.userinfo.EffectState = value;
				 AudioManager.Instance.AudioMute = !value;
				 Set(ref effectState, value);
			 }
		 }
		 private bool musicState = DataCenter.userinfo.MusicState;
		 public bool MusicState
		 {
			 get => musicState;
			 set
			 {
				 DataCenter.userinfo.MusicState = value;
				 AudioManager.Instance.MusicMute = !value;
				 Set(ref musicState, value);
			 }
		 }

        [Preserve]
        public MainStagePauseViewModel()
        {
	        var chooseTalentDic = GameDataManager.Instance.GetChooseTalent(ETalentType.TalentTypeNone);
	        foreach (var item in chooseTalentDic)
	        {
		        TalentCellItemViewModel tempVm = new(item.Key, item.Value);
		        tempVm.SetSize(Vector2.one * 130);
		        TalentScrollViewList.Add(tempVm);
	        }
        }


        [Command]
        private void OnClickContinueBtn()
        {
	        GameTime.Instance.Pause = GameDataManager.Instance.WaveEnd;
	        UIManager.Instance.CloseDialog<MainStagePauseView>();
        }

        [Command]
        private void OnClickHurtBtn()
        {
	        var hurtDic = PlayerStats.Instance.SkillHurtsDic;
	        HurtViewModel tempVm = new(hurtDic);
	        UIManager.Instance.OpenDialog<HurtView>(tempVm).Forget();
        }

		[Command]
		private void OnClickExitBtn()
		{
			GameManager.Instance.OnClickExitBtn();
		}

		[Command]
		private void OnClickEffectBtn()
		{
			EffectState = !EffectState;
		}

		[Command]
		private void OnClickMusicBtn()
		{
			MusicState = !MusicState;
		}
    }
}