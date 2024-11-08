using System;
using Cysharp.Threading.Tasks;
using DH.Data;
using DH.UIFramework.Observables;
using UnityEngine;

namespace DH.Game
{
    public abstract class BaseEffect : BaseAssetEntity
    {
        public string iconPath;
        /// <summary>
        /// 资源加载完毕，可以进行渲染逻辑
        /// </summary>
        internal bool viewReady;
        internal bool removed;

        /// <summary>
        /// Buff Add次数和叠加层数无关，不受到叠加层数最大值限制
        /// </summary>
        internal int referenceCount;
        
        protected float duration;
        protected EffectManager manager;
        protected BaseMonoUnit owner;
        protected float timer;
        protected Func<string, long> configGetter;
        /// <summary>
        /// 用于展示部分Buff的叠加层数
        /// </summary>
        private int count;

        private string effectName;

        public string Name => effectName;

        public virtual bool IsDebuff => false;

        public EffectManager Manager
        {
            get => manager;
            set => Set(ref manager,value);
        }

        public float Duration
        {
            get => duration;
            set => Set(ref duration,value);
        }

        public BaseMonoUnit Owner
        {   
            get => owner;
            set => Set(ref owner,value);
        }

        /// <summary>
        /// 用于展示部分Buff的叠加层数
        /// </summary>
        public int Count
        {
            get => count;
            set => Set(ref count,value);
        }

        public virtual async UniTask OnAddEffect(Func<string,long> paramsGetter,string addEffectName)
        {
            configGetter = paramsGetter;
            effectName = addEffectName;
            removed = false;
            await LoadAssets();
        }

        public virtual void OnRemoveEffect()
        {
            removed = true;
            ReleaseAssets();
        }

        public virtual void OnUpdate(float deltaTime)
        {
            if (removed)
            {
                return;
            }
            
            timer += deltaTime;
            
            if (Duration != 0 && timer > Duration)
            {
                // remove effect
                manager.RemoveEffect(this);
            }
        }

        public virtual void CombineEffect(Func<string,long> paramsGetter)
        {
            
        }
    }
}