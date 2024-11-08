using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace DH.Game
{
    public partial class ShieldEffect : BaseEffect, IShieldEffect
    {
        [AssetPath] public string fxPath;

        public Action OnShieldBroken;

        private float shieldEnergy;
        private float currentValue;
        private GameObject shieldFx;

        public float CurrentValue
        {
            get => currentValue;
            set
            {
                if (!Set(ref currentValue, value)) return;

                RaisePropertyChanged(nameof(Progress));
                RaisePropertyChanged(nameof(Desc));
            }
        }

        public float ShieldEnergy
        {
            get => shieldEnergy;
            set
            {
                if (!Set(ref shieldEnergy, value)) return;

                RaisePropertyChanged(nameof(Progress));
                RaisePropertyChanged(nameof(Desc));
            }
        }

        public float Progress => ShieldEnergy == 0 ? 0 : (float)currentValue / shieldEnergy;

        public string Desc => $"{currentValue}/{shieldEnergy}";

        public override async UniTask OnAddEffect(Func<string, long> paramsGetter, string effectName)
        {
            ShieldEnergy = paramsGetter.Invoke(SkillConst.Energy);
            CurrentValue = ShieldEnergy;
            await base.OnAddEffect(paramsGetter, effectName);
            shieldFx = Instantiate(GetAsset<GameObject>(fxPath), transform, false);
        }

        public override void OnUpdate(float deltaTime)
        {
            // 不根据持续时间进行移除
        }

        public override void OnRemoveEffect()
        {
            base.OnRemoveEffect();
            if (shieldFx)
            {
                ReleaseObj(shieldFx);
                shieldFx = null;
            }

            OnShieldBroken = null;
        }

        public DamageArgs TakeDamage(DamageArgs args)
        {
            // 已经破盾了
            if (CurrentValue <= 0) return args;

            CurrentValue -= args.damagePoint;
            if (CurrentValue <= 0)
            {
                OnShieldBroken?.Invoke();
                manager.RemoveEffect(this);
            }

            // 伤害全部阻挡
            args.damagePoint = 0;
            return args;
        }
    }
}