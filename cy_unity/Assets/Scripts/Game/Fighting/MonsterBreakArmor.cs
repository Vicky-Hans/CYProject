using DH.Data;
using DHFramework;
using UnityEngine;
namespace DH.Game
{
    public class MonsterBreakArmor : MonoBehaviour
    {
        public CircleCollider2D collider2d;
        private MonsterController monsterController;
        public GameObject breakffectObj;
        private float breakArmorRange = 5f;//破甲范围
        private float breakArmorDmg;//破甲伤害
        public void Init(MonsterController monster)
        {
            monsterController = monster;
            breakArmorRange = monster.MonsterData.attr.Calc(AttributeType.BreakArmorRange);
            breakArmorDmg = monster.MonsterData.attr.Calc(AttributeType.BreakArmorDmg);
            var colliderCmp = monster.GetComponent<CircleCollider2D>();
            collider2d.offset = colliderCmp.offset;
            collider2d.radius = breakArmorRange;
            monsterController.MonsterData.BreakArmorDmg = breakArmorDmg;
            monsterController.MonsterData.BreakArmorCount = 1;
            breakffectObj.transform.localScale = Vector3.one * (breakArmorRange *0.4f);
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            var monsterObj = other.GetComponent<MonsterController>();
            if (monsterObj == null || monsterObj.CheckMonsterIsDead())return;
            monsterObj.MonsterData.BreakArmorCount += 1;
            monsterObj.MonsterData.BreakArmorDmg = breakArmorDmg;
        }
        private void OnTriggerExit2D(Collider2D other)
        {
            var monsterObj = other.GetComponent<MonsterController>();
            if (monsterObj == null || monsterObj.CheckMonsterIsDead())return;
            monsterObj.MonsterData.BreakArmorCount -= 1;
        }
    }
}