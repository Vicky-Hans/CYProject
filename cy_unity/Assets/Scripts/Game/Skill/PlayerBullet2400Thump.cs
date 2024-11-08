using System;
using DH.Data;
using UnityEngine;
namespace DH.Game
{
    public partial class PlayerBullet2400Thump : BaseBullet
    {
        private int weaponModelId;
        private float takeCd = 0f;
        public float dmgValue = 0;
        private float startTime = 0f;
        private int dmgCount = 0;
        private const float Interval = 0.8f;
        private const float Duration = 2.5f;
        public override void InitWithTarget(DamageArgs damageArgs, Func<string, float> paramGetter,
            Vector3 startPosition, Vector3 targetPosition, IPool<GameObject> pool)
        {
            base.InitWithTarget(damageArgs, paramGetter, startPosition, targetPosition, pool);
            weaponModelId = damageArgs.weaponModelId;
        }
        private void ApplyDamageOnMonster()
        {
            takeCd = 0f;
            dmgCount++;
            var tmpStartPos = BattleManager.Instance.fightingManagerIns.playerCtrl.transform.position;
            var playerStartPos = new Vector3(tmpStartPos.x,tmpStartPos.y+0.6f,tmpStartPos.z);
            var effectStartPos = new Vector3((playerStartPos.x + 9f) * 0.5f,playerStartPos.y);
            var rectSize = new Vector2(9,2f);
            var layerMask = LayerMask.GetMask("Enemy");
            var count = Physics2D.OverlapBoxNonAlloc(effectStartPos,rectSize,Lodash.Direction2Angle(Vector3.right),PhysicsUtility.CacheCollider,layerMask);
            for (var i = 0; i < count; ++i)
            {
                var target = PhysicsUtility.CacheCollider[i];
                if (target == null) continue;
                var monster = target.GetComponent<MonsterController>();
                if (monster == null || monster.CheckMonsterIsDead()) continue;
                monster.DecHp(Lodash.RoundToInt(dmgValue), weaponModelId);
            }
        }
        public override void OnUpdate(float elapseSeconds)
        {
            if(Recycled)return;
            takeCd += elapseSeconds;
            startTime += elapseSeconds;
            if (takeCd > Interval && dmgCount == 0) ApplyDamageOnMonster();
            if (startTime > Duration) Recycle();
        }
    }
}