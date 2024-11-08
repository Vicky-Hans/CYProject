using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.UIFramework.Observables;

namespace DH.Game
{
    public interface ICharacteristicEffect
    {
    
    }

    public interface IShieldEffect : ICharacteristicEffect
    {
        DamageArgs TakeDamage(DamageArgs damageArgs);
    }
    
    public class EffectManager : ObservableObject
    {
        private BaseMonoUnit owner;
        private readonly List<BaseEffect> pendingList = new List<BaseEffect>();
        private readonly Dictionary<string, BaseEffect> effectsMap =
            new Dictionary<string, BaseEffect>();
        private readonly Dictionary<Type, List<BaseEffect>> characteristicEffects =
            new Dictionary<Type, List<BaseEffect>>();
        /// <summary>
        /// 仅用于UI展示
        /// </summary>
        public ObservableList<BaseEffect> Effects { get; } = new ObservableList<BaseEffect>();

        public BaseMonoUnit Owner
        {
            get => owner;
            set => Set(ref owner,value);
        }

        public void Initialize(BaseMonoUnit entity)
        {
            Owner = entity;
        }

        public DamageArgs TryTakeDamage(DamageArgs args)
        {
            var type = typeof(IShieldEffect);
            foreach (var item in characteristicEffects)
            {
                if (!type.IsAssignableFrom(item.Key))
                {
                    continue;
                }
                
                foreach (var effect in item.Value)
                {
                    args = (effect as IShieldEffect).TakeDamage(args);
                }

                return args;
            }
            
            return args;
        }

        public T GetBuff<T>() where T : BaseEffect
        {
            var type = typeof(T);
            foreach (var item in effectsMap)
            {
                if (!type.IsInstanceOfType(item))
                {
                    continue;
                }

                return (T)item.Value;
            }

            return null;
        }
        
        public T GetBuff<T>(string effectPath) where T : BaseEffect
        {
            var effectName = Path.GetFileNameWithoutExtension(effectPath);
            foreach (var item in effectsMap)
            {
                if (item.Value.Name != effectName)
                {
                    continue;
                }

                return (T)item.Value;
            }

            return null;
        }

        public BaseEffect AddEffect(string effectPath, float? duration, Func<string, long> paramsGetter = null)
        {
            var effectName = Path.GetFileNameWithoutExtension(effectPath);
            var ownerTransform = owner.transform;
            if (effectsMap.ContainsKey(effectName))
            {
                var targetEffect = effectsMap[effectName];
                targetEffect.CombineEffect(paramsGetter);
                return targetEffect;
            }
            
            var effectInstance = AssetsManager.InstantiateSync(effectPath, ownerTransform.position,ownerTransform.rotation, ownerTransform);
            var effect = effectInstance.GetComponent<BaseEffect>();
            if (duration.HasValue)
            {
                effect.Duration = duration.Value;
            }
            effect.Manager = this;
            effect.Owner = owner;
            if (effect is ICharacteristicEffect characteristicEffect)
            {
                var type = characteristicEffect.GetType();
                if (characteristicEffects.TryGetValue(type, out var effects))
                {
                    effects.Add(effect);
                }
                else
                {
                    characteristicEffects.Add(type,new List<BaseEffect>(){effect});
                }
            }

            // 只显示配置了Icon的buff
            if (!string.IsNullOrEmpty(effect.iconPath))
            {
                Effects.Add(effect);
            }
           
            effectsMap.Add(effectName,effect);
            AddEffectWrap(effect,paramsGetter,effectName).Forget();

            return effect;
        }

        public T AddEffect<T>(string effectPath,float? duration,Func<string,long> paramsGetter = null) where T : BaseEffect, new()
        {
            return AddEffect(effectPath,duration,paramsGetter) as T;
        }

        private async UniTaskVoid AddEffectWrap(BaseEffect effect,Func<string,long> paramsGetter,string effectName)
        {
            await effect.OnAddEffect(paramsGetter,effectName);
            effect.viewReady = true;
        }
        
        public void RemoveEffect(string name)
        {
            name = Path.GetFileNameWithoutExtension(name);
            foreach (var effect in effectsMap)
            {
                if (effect.Value.Name != name)
                {
                    continue;
                }
                RemoveEffect(effect.Value);
            }
        }
        
        public void RemoveEffectByType<T>()
        {
            var type = typeof(T);
            foreach (var effect in effectsMap)
            {
                if (effect.Value.GetType() != type)
                {
                    continue;
                }
                RemoveEffect(effect.Value);
            }
        }

        public void RemoveEffect<T>(T effect,bool ignoreRefCount = false) where T : BaseEffect
        {
            if (effect.removed)
            {
                return;
            }

            if (!ignoreRefCount && effect.referenceCount > 0)
            {
                effect.referenceCount--;
                return;
            }

            effect.OnRemoveEffect();
            pendingList.Add(effect);
        }

        public bool ContainsEffect(string effectName)
        {
            return effectsMap.ContainsKey(effectName);
        }

        public void RemoveAllEffect()
        {
            foreach (var effect in effectsMap)
            { 
                RemoveEffect(effect.Value);
            }
        }

        public void ReleaseAssets()
        {
            foreach (var effect in effectsMap)
            {
                effect.Value.OnRemoveEffect();
                AssetsManager.ReleaseInstance(effect.Value.gameObject);
            }

            Effects.Clear();
            effectsMap.Clear();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var effect in effectsMap)
            {
                effect.Value.OnUpdate(deltaTime);
            }

            foreach (var effect in pendingList)
            {
                var effectName = effect.Name;
                Effects.Remove(effect);
                effectsMap.Remove(effectName);

                if (effect is ICharacteristicEffect characteristicEffect)
                {
                    var type = characteristicEffect.GetType();
                    if (characteristicEffects.TryGetValue(type, out var effects))
                    {
                        effects.Remove(effect);
                        if (effects.Count == 0)
                        {
                            characteristicEffects.Remove(type);
                        }
                    }
                }
                
                AssetsManager.ReleaseInstance(effect.gameObject);
            }
            pendingList.Clear();
        }
    }
}