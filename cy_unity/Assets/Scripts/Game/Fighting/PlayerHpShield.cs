using System.Collections.Generic;
using DH.Data;
using UnityEngine;
namespace DH.Game
{
    public class PlayerHpShield : MonoBehaviour
    {
        public ParticleSystem hpShield;
        public ParticleSystem hpShieldBroken;
        public Dictionary<int, bool> WaveDmgDic = new Dictionary<int, bool>(); // 每回合一次盾击
        public Dictionary<int, bool> WaveStartShieldDic = new Dictionary<int, bool>(); // 每回合开始加盾

        private readonly float dmgEffectRadius = 4.2f;
        public CharacterController CharacterController { get; set; }
        public bool IsInit { get; set; }

        public Player Player => CharacterController.Player;
        
        public int HpShield { get; set; }
        
        public bool IsActiveImmuneHpShield { get; set; }
        public float ImmuneHpShield { get; set; }
        
        public bool IsActiveWaveStartHpShield { get; set; }
        public float WaveStartHpShield { get; set; }
        
        public bool IsActiveShieldAttack { get; set; }
        public float ShieldAttacKDmg { get; set; }
        public float ShieldRange { get; set; }
        
        public bool IsActiveMissHpShield { get; set; }
        public float MissHpShield { get; set; }
        public void Init(CharacterController characterController)
        {
            CharacterController = characterController;
            var trigger =
                Player.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.ImmuneDmg,
                    AttributeType.HpShield);
            if(trigger != null)
            {
                IsActiveImmuneHpShield = true;
                ImmuneHpShield = trigger.attrMgr.Calc(AttributeType.HpShield);
            }
            trigger = Player.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.RoundStart,
                AttributeType.HpShield);
            if (trigger != null)
            {
                IsActiveWaveStartHpShield = true;
                WaveStartHpShield = trigger.attrMgr.Calc(AttributeType.HpShield);
            }
            trigger = Player.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.RoundShieldFirstBroken,
                AttributeType.ShieldAttackDmg);
            if (trigger != null)
            {
                IsActiveShieldAttack = true;
                ShieldAttacKDmg = trigger.attrMgr.Calc(AttributeType.ShieldAttackDmg);
                ShieldRange = trigger.attrMgr.Calc(AttributeType.ShieldRange);
            }
            trigger = Player.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.Missing,AttributeType.HpShield);
            if (trigger != null)
            {
                IsActiveMissHpShield = true;
                MissHpShield = trigger.attrMgr.Calc(AttributeType.HpShield);
            }
            IsInit = true;
        }

        public void TriggerImmune()
        {
            if(!IsActiveImmuneHpShield)return;
            var armor = Lodash.RoundToInt(Player.resource.MaxHp * ImmuneHpShield);
            Player.resource.AddArmor(armor);
        }
        public void TriggerMiss()
        {
            if(!IsActiveMissHpShield)return;
            var armor = Lodash.RoundToInt(Player.resource.MaxHp * MissHpShield);
            Player.resource.AddArmor(armor);
        }
        public void TriggerWaveStart()
        {
            if(!IsActiveWaveStartHpShield)return;
            var waveId = BattleManager.Instance.fightingManagerIns.wave;
            if (WaveStartShieldDic.ContainsKey(waveId)) return;
            // HpShield += Lodash.RoundToInt(Player.resource.MaxHp * WaveStartHpShield);
            var armor = Lodash.RoundToInt(Player.resource.MaxHp * WaveStartHpShield);
            Player.resource.AddArmor(armor);
            WaveStartShieldDic[waveId] = true;
        }

        private void PlayDmgEffect()
        {
            var effectObjTrans = hpShieldBroken.gameObject.transform;
            effectObjTrans.localScale = Vector3.one * ShieldRange / dmgEffectRadius;
            effectObjTrans.localPosition = Vector3.up;
            hpShieldBroken.gameObject.SetActive(true);
            TimerManager.Instance.AddTimer(() =>
                {
                    hpShieldBroken.gameObject.SetActive(false);
                }, 999f, 2.5f, 1,
                GameConst.TimerTagName.PauseTask);
        }
        
        public void TriggerHpShieldZero()
        {
            var waveId = BattleManager.Instance.fightingManagerIns.wave;
            if (WaveDmgDic.ContainsKey(waveId)) return;
            WaveDmgDic[waveId] = true;
            // @TODO: 盾击
            PlayDmgEffect();
            var heroAtk = Player.clothesTriggerSkill.GetHeroAtk();
            var dmgValue = Lodash.RoundToInt(heroAtk * ShieldAttacKDmg);
            var unitPos = CharacterController.transform.position;
            var layerMask = LayerMask.GetMask("Enemy");
            var shieldAtkRepelTrigger = Player.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.ShieldAttack, AttributeType.RepelPro);
            var repelRange = 0f;
            if (shieldAtkRepelTrigger != null) repelRange = shieldAtkRepelTrigger.attrMgr.Calc(AttributeType.RepelRange);//盾击附带强力击退效果
            int count = Physics2D.OverlapCircleNonAlloc(unitPos, ShieldRange,
                PhysicsUtility.CacheCollider, layerMask);
            for (var i = 0; i < count; i++)
            {
                var target = PhysicsUtility.CacheCollider[i];
                if (target == null) continue;
                var monster = target.GetComponent<MonsterController>();
                if (monster == null) continue;
                monster.DecHp((int)dmgValue);
                if (repelRange > 0) monster.Repeled(repelRange);
            }
        }
        
        public void DecHpShield(int dmg)
        {
            HpShield -= dmg;
            if (HpShield > 0) return;
            HpShield = 0;
            // TriggerHpShieldZero();
        }

        public void TriggerShieldAttack()
        {
            if (!IsActiveShieldAttack) return;
            TriggerHpShieldZero();
        }

        public void Update()
        {
            if(!IsInit)return;
            hpShield.gameObject.SetActive(false);
        }
    }
}