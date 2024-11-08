using DH.Config;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public class MonsterDownCdRange : MonoBehaviour
    {
        public Collider2D collider2d;
        private float defaultRadius = 5f;
        private MonsterController monsterController;
        private FightingBaseManager fightingBaseManager;
        private float downCd;

        public void Init(MonsterController monster)
        {
            monsterController = monster;
            fightingBaseManager = BattleManager.Instance.fightingManagerIns;
            var radius = monster.MonsterData.attr.Calc(AttributeType.DownCdRange);
            downCd = monster.MonsterData.attr.Calc(AttributeType.DownCd);
            var monsterScale = 0.5f;
            if (monster.MonsterData.cfg.MonsterType == (int)MonsterType.Boss)
            {
                monsterScale = 1f;
            }
            transform.localScale = Vector3.one * radius / defaultRadius / monsterScale;
            // var transCache = transform;
            // var localScale = transCache.localScale;
            // transCache.localScale = Vector3.one * (1f/localScale.x);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var monsterObj = other.GetComponent<MonsterController>();
            if (monsterObj == null)return;
            if(monsterObj.CheckMonsterIsDead())return;
            monsterObj.MonsterData.DownCd = downCd;
            monsterObj.MonsterData.DownCdCount++;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var monsterObj = other.GetComponent<MonsterController>();
            if (monsterObj == null)return;
            if(monsterObj.CheckMonsterIsDead())return;
            monsterObj.MonsterData.DownCdCount--;
        }
    }
}