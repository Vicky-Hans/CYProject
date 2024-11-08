using System.Collections.Generic;
using DH.Data;

namespace DH.Game
{
    public class PlayerHpTrigger
    {
        private Player player;
        private bool isTriggeredInvincible = false;//是否触发过无敌技能
        private FightingBaseManager fightingBaseManager;
        private Dictionary<int, bool> waveTriggerDic = new Dictionary<int, bool>();
        private Dictionary<int, bool> waveTrigger15Dic = new Dictionary<int, bool>(); // equip 15 波次触发
        public void Init(Player p)
        {
            player = p;
            fightingBaseManager = BattleManager.Instance.fightingManagerIns;
            //检测是否触发过无敌模式
            var heroInvincibleTrigger = GetHeroInvincibleTrigger();
            if (heroInvincibleTrigger == null)
            {
                isTriggeredInvincible = true;
            }
            else
            {
                heroInvincibleTrigger.trigger.TryGetValue(AttributeName.HeroHpValueLess, out var hpThresholdValue);
                if (player.resource.Progress <= hpThresholdValue * GameConst.AttributeDivisor) isTriggeredInvincible = true;
            }
        }
        public void Trigger(float hpProgress)
        {
            Trigger606(hpProgress);
            TriggerEquip15(hpProgress);
        }
        /// <summary>
        /// 触发无敌，英雄4000技能
        /// </summary>
        /// <param name="hpProgress"></param>
        public void TriggerInvincible(float hpProgress)
        {
            if (isTriggeredInvincible) return;
            var heroInvincibleTrigger = GetHeroInvincibleTrigger();
            if (heroInvincibleTrigger == null) return;
            heroInvincibleTrigger.trigger.TryGetValue(AttributeName.HeroHpValueLess, out var hpThresholdValue);
            if (hpProgress >= hpThresholdValue * GameConst.AttributeDivisor) return;
            if (player.buffMgr.FindBuffById((int)AttributeType.AtkReplay) != null) return;
            var heroImmuneTime = heroInvincibleTrigger.attrMgr.Calc(AttributeType.HeroImmuneTime)*GameConst.TimeDivisor;
            var upSpd = heroInvincibleTrigger.attrMgr.Calc(AttributeType.UpSpd);
            if (heroImmuneTime > 0)
            {
                fightingBaseManager.playerCtrl.AddInvincibleBuff(new Buff
                {
                    id = (int)AttributeType.HeroHpValueLess,
                    attrName = AttributeName.HeroHpValueLess,
                    startTime = GameTime.Instance.GTime,
                    duration = heroImmuneTime,multi = false,
                    valueType = BuffValueType.Positive
                });
            }
            if (upSpd > 0)//移动速度提升100%
            {
                player.buffMgr.AddBuff(new Buff
                {
                    id = (int)AttributeType.UpSpd,
                    attrName = AttributeName.UpSpd,
                    startTime = GameTime.Instance.GTime,
                    duration = heroImmuneTime,multi = false,
                    valueType = BuffValueType.Positive,
                    value = upSpd
                });
            }
            isTriggeredInvincible = true;
        }
        /// <summary>
        /// 获取无敌技能Trigger
        /// </summary>
        /// <returns></returns>
        private SkillTrigger GetHeroInvincibleTrigger()
        {
            var heroInvincibleTrigger = player.triggerSkill.GetTriggerByNameAndComplete(AttributeName.HeroHpValueLess,AttributeType.HeroImmune);
            return heroInvincibleTrigger;
        }
        private void Trigger606(float hpProgress)
        {
            var wave = fightingBaseManager.wave;
            if (waveTriggerDic.ContainsKey(wave))return;
            var weapons = fightingBaseManager.playerCtrl.WeaponSkillController.weapons;
            BaseWeapon weaponIns = null;
            foreach (var weapon in weapons)
            {
                if (weapon.weaponData.WeaponModelId == 606)
                {
                    weaponIns = weapon;
                    break;
                }
            }
            if(weaponIns == null)return;
            var skillData = weaponIns.weaponData.SkillData;
            var trigger =
                skillData.GetTriggerByNameAndComplete(AttributeName.AsHpValueTh,
                    AttributeType.AtkSpdGp);
            if (trigger == null) return;
            trigger.trigger.TryGetValue(AttributeName.AsHpValueTh, out var asHpValueThValue);
            var asHpValueTh = asHpValueThValue * GameConst.AttributeDivisor;
            if (hpProgress < asHpValueTh)
            {
                var atkSpdGp = trigger.attrMgr.Calc(AttributeType.AtkSpdGp);
                atkSpdGp += skillData.attrMgr.Calc(AttributeType.AtkSpdGp);
                var atkSpGpTime = skillData.attrMgr.Calc(AttributeType.AtkSpdGpTime) * GameConst.TimeDivisor;
                var buff = new Buff
                {
                    id = (int)AttributeType.AtkSpdGp,
                    attrName = AttributeName.AtkSpdGp,
                    value = atkSpdGp,
                    valueType = BuffValueType.Positive,
                    startTime = GameTime.Instance.GTime,
                    duration = atkSpGpTime,
                    multi = false,
                };
                player.buffMgr.AddBuff(buff);
                waveTriggerDic.Add(wave, true);
            }
        }
        private void TriggerEquip15(float hpProgress)
        {
            var wave = fightingBaseManager.wave;
            if (waveTrigger15Dic.ContainsKey(wave))return;
            var weapons = fightingBaseManager.playerCtrl.WeaponSkillController.weapons;
            foreach (var weapon in weapons)
            {
                if (weapon.weaponData.EquipId != 15)continue;
                var skillData = weapon.weaponData.SkillData;
                var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.AsHpValueTh,
                            AttributeType.HpRevert);
                    if (trigger == null) break;
                    trigger.trigger.TryGetValue(AttributeName.AsHpValueTh,
                        out var asHpValueThValue);
                    var asHpValueTh = asHpValueThValue * GameConst.AttributeDivisor;
                    if (hpProgress >= asHpValueTh) break;
                    var hpRevert = trigger.attrMgr.Calc(AttributeType.HpRevert);
                    hpRevert += skillData.attrMgr.Calc(AttributeType.HpRevert);
                    player.resource.AddHpPer(hpRevert);
                    CheckShockWaveTrigger();//生命恢复时有概率触发冲击波
                    waveTrigger15Dic.Add(wave, true);
                    break;
            }
        }
        private void CheckShockWaveTrigger()
        {
            fightingBaseManager.playerCtrl.PlayerShockWaveTrigger?.Trigger1();
        }
    }
}