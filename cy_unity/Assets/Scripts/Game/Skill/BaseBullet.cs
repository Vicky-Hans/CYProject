using System;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public interface IBullet : IGamePlayElement
    {
        public GameObject Obj { get; }

        void InitWithTarget(DamageArgs damageArgs, Func<string, float> paramGetter, Vector3 startPosition,
            Vector3 targetPosition, IPool<GameObject> pool);
    }

    /// <summary>
    /// 子弹不再独立管理自身的资源生命周期
    /// 由子弹的父节点管理
    /// </summary>
    public partial class BaseBullet : BaseAssetEntity, IBullet
    {
        [AssetPath] public string modelPath;
        public float modelRefScale = 1.0f;

        protected IPool<GameObject> bulletPool;
        protected Func<string, float> configGetter;
        protected AttributeMgr attributeMgr;
        protected GameObject modelInstance;
        protected Transform cacheTransform;
        protected float bulletSize;
        public long SkillNum { get; set; }
        public int WeaponModelId { get; set; }

        private bool recycled;

        public bool Recycled => recycled;

        public GameObject Obj => gameObject;

        protected override bool ManageAssets => false;

        protected override void OnAssetsLoaded()
        {
            modelInstance = InstantiateObj(modelPath, transform);
            if (modelInstance == null) return;
            modelInstance.transform.localScale = Vector3.one * modelRefScale;
            modelInstance.name = "bulletEffect";
        }

        public virtual void InitWithTarget(DamageArgs damageArgs, Func<string, float> paramGetter,
            Vector3 startPosition,
            Vector3 targetPosition, IPool<GameObject> pool)
        {
            if (pool == null) throw new Exception("null pool");
            configGetter = paramGetter;
            recycled = false;
            bulletPool = pool;
            WeaponModelId = damageArgs.weaponModelId;
            var fightingManagerIns = BattleManager.Instance.fightingManagerIns;
            var manager = fightingManagerIns.gamePlayManager;
            manager.AddElement(this);
            cacheTransform = transform;
        }

        protected virtual void Recycle()
        {
            if (recycled) return;
            recycled = true;
            var figManager = BattleManager.Instance.fightingManagerIns;
            figManager.gamePlayManager.RemoveElement(this);
            if (bulletPool != null)
            {
                if (modelInstance != null)
                    ReleaseObj(modelInstance);
                if(gameObject!=null)
                    bulletPool.ReleaseObj(gameObject);
            }
            else
            {
                if (modelInstance != null)
                    ReleaseObj(modelInstance);
                if(gameObject!=null)
                    ReleaseObj(gameObject);
            }
            ReleaseAssets();
        }

        /// <summary>
        /// 用于技能取消时，强制释放子弹
        /// </summary>
        public void ForceDestroy()
        {
            Recycle();
        }
        public virtual void OnUpdate(float elapseSeconds)
        {
            throw new NotImplementedException();
        }
        
    }
}