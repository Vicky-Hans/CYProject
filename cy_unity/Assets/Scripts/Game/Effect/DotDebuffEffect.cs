using System;
using Cysharp.Threading.Tasks;

namespace DH.Game
{
    public class DotDebuffEffect : BaseEffect
    {
        private float attackTime;
        
        internal float attackInterval;
        /// <summary>
        /// 自定义伤害行为
        /// </summary>
        internal Action<BaseMonoUnit> damageAction;
        internal DamageArgs args = new DamageArgs();

        public override bool IsDebuff => true;

        public override async  UniTask OnAddEffect(Func<string, long> paramsGetter, string name)
        {
            args.damagePoint = paramsGetter.Invoke(SkillConst.AttackPoint);
            attackInterval = paramsGetter.Invoke(SkillConst.Intervals) * 0.001f;
            
            await base.OnAddEffect(paramsGetter, name);
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (removed)
            {
                return;
            }
            
            if (timer > attackTime)
            {
                if (damageAction == null)
                {
                    OnDamageTarget(args.Clone());
                }
                else
                {
                    damageAction.Invoke(owner);
                }
               attackTime += attackInterval;
            }
        }

        /// <summary>
        /// 合并Debuff时强制受到一次伤害
        /// 此时受到的伤害值为当前合并Buff的伤害值
        /// </summary>
        /// <param name="paramsGetter"></param>
        public override void CombineEffect(Func<string, long> paramsGetter)
        {
            var damageArgs = args.Clone();
            var damage = paramsGetter.Invoke(SkillConst.AttackPoint);
            damageArgs.damagePoint = damage;
            
            if (damage > args.damagePoint)
            {
                args.damagePoint = damage;
            }
            
            var time = paramsGetter.Invoke(SkillConst.EffectDuration) * 0.001f;
            if (time> duration)
            {
                duration = time;
            }

            var intervals = paramsGetter.Invoke(SkillConst.Intervals) * 0.001f;
            if (intervals > attackInterval)
            {
                attackInterval = intervals;
            }
            
            OnDamageTarget(damageArgs);

            timer = 0;
            attackTime = attackInterval;
        }
        
        private void OnDamageTarget(DamageArgs damageArgs)
        {
            damageArgs.effect |= DamageEffect.Dot;
            damageArgs.target = owner.gameObject;
            damageArgs.ApplyDamage(owner as IDamageable);
        }

    }
}