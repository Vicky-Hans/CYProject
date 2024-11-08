using System;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public interface IBuff : IGamePlayElement
    {
        public GameObject Obj { get; }

        void InitWithTarget(BaseMonoUnit unit, Buff buff, IPool<GameObject> pool);
    }
    public partial class BaseBuff : BaseAssetEntity, IBuff
    {
        [AssetPath] public string modelPath;
        protected BaseMonoUnit baseMonoUnit;
        protected Buff buff;
        protected float time;
        private bool recycled;
        protected IPool<GameObject> buffPool;
        public bool Recycled => recycled;
        public GameObject Obj => gameObject;
        protected GameObject modelInstance;
        protected override bool ManageAssets => false;
        
        public virtual void InitWithTarget(BaseMonoUnit unit, Buff buff, IPool<GameObject> pool)
        {
            var fightingManagerIns = BattleManager.Instance.fightingManagerIns;
            if (pool == null) throw new Exception("null pool");
            baseMonoUnit = unit;
            baseMonoUnit.Data.buffMgr.AddBuff(buff);
            this.buff = buff;
            recycled = false;
            buffPool = pool;
            var manager = fightingManagerIns.gamePlayManager;
            manager.AddElement(this);
        }
        
        protected override void OnAssetsLoaded()
        {
            modelInstance = InstantiateObj(modelPath, transform);
            if(modelInstance)
                modelInstance.name = "buffEffect";
        }

        public virtual void Recycle()
        {
            if (recycled) return;
            recycled = true;
            var fightingManagerIns = BattleManager.Instance.fightingManagerIns;
            var manager = fightingManagerIns.gamePlayManager;
            manager.RemoveElement(this);
            
            time = 0;
            baseMonoUnit.Data.buffMgr.RemoveBuff(buff);
            // buff = null;
            baseMonoUnit = null;
            
            if (buffPool != null)
            {
                buffPool.ReleaseObj(gameObject);
            }
            else
            {
                ReleaseObj(gameObject);
            }

            if (modelInstance)
            {
                ReleaseObj(modelInstance);
                modelInstance = null;
            }
            ReleaseAssets();
        }

        
        public virtual void OnUpdate(float elapseSeconds)
        {
            throw new NotImplementedException();
        }

        
        
    }
}