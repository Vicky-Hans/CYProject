using System.Collections.Generic;
using DH.Config;
using DH.Data;

namespace DH.Game
{
    public class FightingSoundHelper:ObservableSingleton<FightingSoundHelper>
    {
        private const string SndFxDeadTag = "SndFxDeadTag";
        private const string SndFxMonsterSkillTag = "SndFxMonsterSkillTag";
        private const int MaxDeadSndCount = 2;
        private readonly List<string> hurtSndList = new List<string> {"game_common_attackeda1", "game_common_attackeda2", "game_common_attackeda3"};
        private readonly string shipHurtSnd = "game_common_shieldOvera1";
        private const string SndShipOver = "game_common_shipOver";
        private const string SndBuffFrozen = "game_buff_frozen";
        private const string SndBuffBurn = "game_buff_burn";
        private const string SndBuffVertigo = "game_buff_dizzy";
        private const string SndBuffRangeBurn = "game_buff_rangeBurn";
        private const string Bgm = "BGM/cy_bgm_battle";
        private const string SndWeaponHit = "battle_monsterHit";
        private const string SndPlayerHurt = "battle_playerHit";
        private const string SndBuffShieldBroken = "battle_buff_shieldBroken";
        private const string PickUpExp = "battle_Secret_pickUpEXP";
        private const string PickUpBoom = "battle_Secret_boom";
        private const string PickUpItem = "battle_Secret_pickUp";
        private readonly List<string> monsterFarSkillSndList = new List<string> {"battle_monster_long01", "battle_monster_long02"};
        private readonly List<string> monsterNearSkillSndList = new List<string> {"battle_monster_nearly"};

        private SkillCfg GetSkillCfg(int skillId, bool isFrigate = false)
        {
            return ConfigCenter.SkillCfgColl.GetDataById(skillId);
        }
        public void PlaySkillPrepare(int skillId, bool isFrigate = false)
        {
            
        }

        public void PlaySkillLaunch(int skillId, bool isFrigate = false)
        {
            // var cfg = GetSkillCfg(skillId, isFrigate);
            // if(cfg.Sfx01 == null)return;
            // var snd = cfg.Sfx01.RandomOneInList();
            // if(CheckSndCountLimit(snd))return;
            // AudioManager.Instance.PlayFightingAudio(snd, tag:snd);
        }

        public void PlayWeaponLaunch(int weaponModelId)
        {
            var modelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(weaponModelId);
            if(modelCfg == null || string.IsNullOrEmpty(modelCfg.EquipSfx))return;
            var snd = modelCfg.EquipSfx;
            if(CheckSndCountLimit(snd))return;
            AudioManager.Instance.PlayFightingAudio(snd, tag:snd);
        }

        public void PlayWeaponHit(int weaponModelId)
        {
            var modelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(weaponModelId);
            if(modelCfg == null || string.IsNullOrEmpty(modelCfg.EquipSfx01))return;
            var snd = modelCfg.EquipSfx01;
            if(CheckSndCountLimit(snd))return;
            AudioManager.Instance.PlayFightingAudio(snd, tag:snd);
        }
        
        public void PlayPlayerHurt()
        {
            if(CheckSndCountLimit(SndPlayerHurt))return;
            AudioManager.Instance.PlayFightingAudio(SndPlayerHurt, tag:SndPlayerHurt);
        }

        public void PlayMonsterFarSkillSnd()
        {
            if (AudioManager.Instance.GetAudioCount(SndFxMonsterSkillTag) > MaxDeadSndCount)
            {
                return;
            }
            var snd = monsterFarSkillSndList.RandomOneInList();
            AudioManager.Instance.PlayFightingAudio(snd, tag:SndFxMonsterSkillTag);
        }

        public void PlayMonsterNearSkillSnd()
        {
            if (AudioManager.Instance.GetAudioCount(SndFxMonsterSkillTag) > MaxDeadSndCount)
            {
                return;
            }
            var snd = monsterNearSkillSndList.RandomOneInList();
            AudioManager.Instance.PlayFightingAudio(snd, tag:SndFxMonsterSkillTag);
        }

        public void PlayShieldBroken()
        {
            if(CheckSndCountLimit(SndBuffShieldBroken))return;
            AudioManager.Instance.PlayFightingAudio(SndBuffShieldBroken, tag:SndBuffShieldBroken);
        }

        public void PlayHeroActiveSkill(int heroId)
        {
            var heroMainCfg = ConfigCenter.HeroMainCfgColl.GetDataById(heroId);
            if(heroMainCfg == null || string.IsNullOrEmpty(heroMainCfg.MainSkillSfx))return;
            var snd = heroMainCfg.MainSkillSfx;
            AudioManager.Instance.PlayFightingAudio(snd, tag:snd);
        }
        
        public void PlaySkillBoom(int skillId, bool isFrigate = false)
        {
           
        }
        
        public void PlaySkillSpec(int skillId, bool isFrigate = false)
        {
           
        }

        public void PlaySkillAudioPath(string audioPath)
        {
            if(CheckSndCountLimit(audioPath))return;
            AudioManager.Instance.PlayFightingAudio(audioPath, tag:audioPath);
        }

        public void PlayBgm()
        {
            AudioManager.Instance.PlayMusic(Bgm, 0f);
        }
        
        public void PlayHurt()
        {
            if (AudioManager.Instance.GetAudioCount(SndFxDeadTag) > MaxDeadSndCount)
            {
                return;
            }
            var snd = hurtSndList.RandomOneInList();
            // AudioManager.Instance.PlayFightingAudio(snd, tag:SndFxDeadTag);
        }

        public void PlayShipHurt()
        {
            var snd = shipHurtSnd;
            if(CheckSndCountLimit(snd))return;
            AudioManager.Instance.PlayFightingAudio(snd, tag:snd);
        }

        public void PlayShipOver()
        {
            AudioManager.Instance.PlayFightingAudio(SndShipOver);
        }

        public void PlayBuffFrozen()
        {
            AudioManager.Instance.PlayFightingAudio(SndBuffFrozen);
        }

        public void PlayBuffBurn()
        {
            // AudioManager.Instance.PlayFightingAudio(SndBuffBurn);
        }

        public void PlayBuffVertigo()
        {
            // AudioManager.Instance.PlayFightingAudio(SndBuffVertigo);
        }

        public void PlayBuffRangeBurn()
        {
            // AudioManager.Instance.PlayFightingAudio(SndBuffRangeBurn);
        }
        public void PlaySecretPickUpExp()
        {
            AudioManager.Instance.PlayFightingAudio(PickUpExp);
        }

        public void PlaySecretPickUpBoom()
        {
            AudioManager.Instance.PlayFightingAudio(PickUpBoom);
        }
        public void PlaySecretPickUpItem()
        {
            AudioManager.Instance.PlayFightingAudio(PickUpItem);
        }

        private bool CheckSndCountLimit(string snd)
        {
            if(string.IsNullOrEmpty(snd))return true;
            if (AudioManager.Instance.GetAudioCount(snd) > MaxDeadSndCount)
            {
                return true;
            }

            return false;
        }
    }
}