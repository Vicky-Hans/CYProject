using DH.Data;
using DHFramework;
using UnityEngine;
namespace DH.Game
{
    public class MonsterSelfDestruct : MonoBehaviour
    {
        public CircleCollider2D collider2d;
        public GameObject brokenEffectObj;
        private MonsterController monsterController;
        private FightingBaseManager fightingBaseManager;
        private float selfDestructDmg;//自爆伤害
        
        public void Init(MonsterController monster, SkillTrigger trigger)
        {
            monsterController = monster;
            fightingBaseManager = BattleManager.Instance.fightingManagerIns;
            selfDestructDmg = trigger.attrMgr.Calc(AttributeType.SelfDestructDmg);
            var colliderCmp = monster.GetComponent<CircleCollider2D>();
            collider2d.offset = colliderCmp.offset;
            collider2d.radius = colliderCmp.radius;
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            var playerObj = other.GetComponent<CharacterController>();
            if (playerObj == null || playerObj.Data.IsDead()) return;
            monsterController.DecHp((int)monsterController.Data.resource.Hp);
            var dmgArgs = monsterController.attackBox.damageArgs.Clone();
            var destructDmg = dmgArgs.damagePoint * selfDestructDmg;
            dmgArgs.damagePoint = (long)destructDmg;
            playerObj.OnDamage(dmgArgs);
            PlayBroken();
        }
        private void PlayBroken()
        {
            var pos = transform.position;
            FightingSoundHelper.Instance.PlayShieldBroken();
            var brokenObj = Instantiate(brokenEffectObj, pos, Quaternion.identity, fightingBaseManager.fightPanelTrans);
            if (brokenObj != null) fightingBaseManager.AddAutoReleaseGObj(brokenObj, 3f);
        }
    }
}