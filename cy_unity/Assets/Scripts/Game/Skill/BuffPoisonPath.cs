using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class BuffPoisonPath: BaseBuff
    {
        [AssetPath] public string tmpBulletPathStr;
        private FightingBaseManager fightingManager;
        public DamageArgs curDamageArgs { get; set; }
        private bool isInit = false;
        private float takeCd = 0.2f;
        private float takeCdTime = 0f;
        public override void InitWithTarget(BaseMonoUnit unit, Buff buff, IPool<GameObject> pool)
        {
            base.InitWithTarget(unit, buff, pool);
            LoadAssetSync(tmpBulletPathStr);
            fightingManager = BattleManager.Instance.fightingManagerIns;
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one * 2;
            isInit = true;
        }
        public override void OnUpdate(float deltaTime)
        {
            if (!isInit || Recycled) return;
            if (buff == null)
            {
                Recycle();
                return;
            }
            if (!buff.IsValid(GameTime.Instance.GTime)) Recycle();
            takeCdTime += deltaTime;
            if (takeCdTime < takeCd) return;
            takeCdTime = 0f;
            TakeShootInternal();
        }
        /// <summary>
        /// 发射剧毒路径点
        /// </summary>
        private void TakeShootInternal()
        {
            var bullet = InstantiateObj(tmpBulletPathStr, transform.position, Quaternion.identity, fightingManager.fightPanelTrans);
            if (bullet == null) return;
            var bulletComp = bullet.GetComponent<PlayerClothesBullet910058>();
            if (bulletComp == null) return;
            bulletComp.InitWithTarget(curDamageArgs.Clone(), GetConfigArgs, transform.position, transform.position, fightingManager.entityPool);
        }
        private float GetConfigArgs(string key)
        {
            return 0;
        }
    }
}