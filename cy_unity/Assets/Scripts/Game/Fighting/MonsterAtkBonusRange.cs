using System;
using DH.Config;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public class MonsterAtkBonusRange : MonoBehaviour
    {
        public Collider2D collider2d;
        private float defaultRadius = 5f;
        private MonsterController monsterController;
        private FightingBaseManager fightingBaseManager;
        private float atkBonus;

        public void Init(MonsterController monster)
        {
            monsterController = monster;
            fightingBaseManager = BattleManager.Instance.fightingManagerIns;
            var radius = monster.MonsterData.attr.Calc(AttributeType.AttackBonusRange);
            atkBonus = monster.MonsterData.attr.Calc(AttributeType.AttackBonus);
            var monsterScale = 0.5f;
            if (monster.MonsterData.cfg.MonsterType == (int)MonsterType.Boss)
            {
                monsterScale = 1f;
            }
            transform.localScale = Vector3.one * radius / defaultRadius / monsterScale;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var monsterObj = other.GetComponent<MonsterController>();
            if (monsterObj == null)return;
            if(monsterObj.CheckMonsterIsDead())return;
            monsterObj.MonsterData.AtkBonus = atkBonus;
            monsterObj.MonsterData.AtkBonusCount++;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var monsterObj = other.GetComponent<MonsterController>();
            if (monsterObj == null)return;
            if(monsterObj.CheckMonsterIsDead())return;
            monsterObj.MonsterData.AtkBonusCount--;
        }
    }
}