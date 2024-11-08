using System.Collections.Generic;
using DH.Data;
using DHFramework;
using UnityEngine;

namespace DH.Game
{
    public class PlayerShockWaveTrigger
    {
        private Player player;
        private FightingBaseManager fightingBaseManager;
        private string tmpBulletPath = $"Fighting/Weapon/skill_clothes_bullet_910043";
        private Dictionary<int, bool> waveTriggerShockWave = new Dictionary<int, bool>(); // 魔法套装波次触发冲击波
        public void Init(Player p)
        {
            player = p;
            fightingBaseManager = BattleManager.Instance.fightingManagerIns;
        }
        public void Trigger()
        {
            var wave = fightingBaseManager.wave;
            if (waveTriggerShockWave.ContainsKey(wave)) return;
            var heroUnderAttackTrigger = player.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.HeroUnderAttack, AttributeType.ShockWaveDmg);
            if (heroUnderAttackTrigger == null) return;
            waveTriggerShockWave.Add(wave,true);
            var dmgArgs = new DamageArgs
            {
                effect = DamageArgs.SkillFactionEffect(SkillFaction.None),
                sender = fightingBaseManager.playerCtrl,
                skillData = player.clothesTriggerSkill,
                skillId = 910043,
                clothesId = 2,
                dmgType = DmgType.Normal
            };
            CreateShockWaveBullet(heroUnderAttackTrigger,dmgArgs);
        }
        public void Trigger1()
        {
            var revertingTrigger = player.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.Reverting, AttributeType.ShockWaveDmg);
            if (revertingTrigger == null) return;
            var dmgArgs = new DamageArgs
            {
                effect = DamageArgs.SkillFactionEffect(SkillFaction.None),
                sender = fightingBaseManager.playerCtrl,
                skillData = player.clothesTriggerSkill,
                skillId = 910048,
                clothesId = 2,
                dmgType = DmgType.Normal
            };
            CreateShockWaveBullet(revertingTrigger,dmgArgs);
        }
        private void CreateShockWaveBullet(SkillTrigger shockWaveTrigger,DamageArgs dmgArgs)
        {
            var factor = 1;//1朝右，-1=朝左
            var shockWaveDmg = shockWaveTrigger.attrMgr.Calc(AttributeType.ShockWaveDmg);
            if (BattleManager.Instance.fightingManagerIns.playerCtrl.GetPlayerOrientation() == 0) factor = -1;
            var tmpTargetPos = new Vector3(fightingBaseManager.FightPanelSize.x*factor,fightingBaseManager.playerCtrl.transform.position.y,0f);
            fightingBaseManager.entityPool.LoadAssetSync(tmpBulletPath);
            var bullet = fightingBaseManager.entityPool.InstantiateObj(tmpBulletPath, fightingBaseManager.playerCtrl.transform.position, Quaternion.identity, fightingBaseManager.fightPanelTrans);
            var bulletComp = bullet.GetComponent<PlayerClothesBullet910043>();
            dmgArgs.damagePoint = Mathf.RoundToInt(player.attr.Calc(AttributeType.Atk) * shockWaveDmg);
            bulletComp.InitWithTarget(dmgArgs, GetConfigArgs, fightingBaseManager.playerCtrl.transform.position, tmpTargetPos, fightingBaseManager.entityPool);
        }
        public float GetConfigArgs(string key)
        {
            return 0;
        }
    }
}