using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public static class SkillConst
    {
        /// <summary>
        /// 攻击力
        /// </summary>
        public const string AttackPoint = "attackPoint";
        /// <summary>
        /// Buff效果数值
        /// 如减速BUFF的减速30%
        /// </summary>
        public const string Value = "value";
        /// <summary>
        /// 主动技能持续时间
        /// </summary>
        public const string Duration = "duration";
        /// <summary>
        /// 眩晕效果持续时间
        /// </summary>
        public const string EffectDuration = "effectDur";
        public const string Intervals = "attackInterval";
        /// <summary>
        /// 能量值
        /// 护盾防御值
        /// </summary>
        public const string Energy = "energy";
        public const string Overlay = "overlay";
    }

    public abstract class BaseSkill : BaseAssetEntity
    {
        public enum State
        {
            Idle,
            Cooldown,
            Ready,
            Taking,
            Working,
            Prepare
        }

        internal SkillType skillType;
        internal Func<string, long> CustomConfig;
        protected UnitBase owner;
        protected State currentState;
        protected Transform cacheTransform;
        protected float attackPreferDistance;
        protected IPool<GameObject> pool;
        private List<BaseBullet> bulletList = new();
        /// <summary>
        /// 只用于Inspector展示
        /// </summary>
        [SerializeField] [DebugData] protected long damagePoint;
        [SerializeField] [DebugData] protected float timeInterval;
        [SerializeField] [DebugData] protected float duration;
        private float timer;
        private Skill skillData;
        private int remainTime;
        private float progress;
        private BaseMonoUnit monoOwner;
        private bool isMonster;
        public virtual bool CheckTargetInRange()
        {
            var range = skillData.Range();
            var monster = BattleManager.Instance.fightingManagerIns.GetNearestMonster(cacheTransform.position, range);
            if (monster != null)
            {
                return true;
            }
            return false;
        }
        
        public Skill SkillData
        {
            get => skillData;
            set => Set(ref skillData, value);
        }

        public virtual bool CanTakeSkill()
        {
            var result = currentState == State.Ready;
            if (!result) return false;

            if (attackPreferDistance == 0) return true;

            var target = MonoOwner.currentTarget;
            if (!target) return false;

            var delta = target.transform.position - cacheTransform.position;
            if (delta.sqrMagnitude < attackPreferDistance * attackPreferDistance) return true;

            return false;
        }

        protected virtual bool AutoTakeSkill => skillType == SkillType.Auto;

        public State CurrentState
        {
            get => currentState;
            internal set
            {
                if(currentState == value) return;
                var oldState = currentState;
                if (!Set(ref currentState, value)) return;
                OnStateChanged(oldState, currentState);
            }
        }
        public float Progress
        {
            get => progress;
            set => Set(ref progress, value);
        }
        public BaseMonoUnit MonoOwner => monoOwner;
        public float TimeInterval => timeInterval;
        public float Duration => duration;

        public float GetConfigArgs(string key)
        {
            if (isMonster)
            {
                //走配置里获取
                if (skillData.Cfg.Result == null) return 0;
                var list = skillData.Cfg.Result;
                foreach (var attr in list)
                    if (attr.Type == key)
                        return AttributeHelper.GetAttrValue(key, attr.Value);
            }
            else
            {
                return skillData.attrMgr.Calc(AttributeHelper.GetAttributeTypeByName(key));
            }
            return 0;
        }
        public virtual void Release()
        {
            ReleaseAssets();
            if (skillData == null) return;
            skillData.PropertyChanged -= DataOnPropertyChanged;
            foreach (var comp in bulletList)
            {
                if (comp == null) continue;
                comp.ForceDestroy();
            }
        }
        protected virtual void OnStateChanged(State old, State current)
        {
        }

#pragma warning disable CS1998
        public virtual async UniTask Init(Skill data, UnitBase unitBase, BaseMonoUnit behaviour)
#pragma warning restore CS1998
        {
            cacheTransform = transform;
            monoOwner = behaviour;
            isMonster = behaviour.MonsterData != null;
            SkillData = data;
            skillData.PropertyChanged += DataOnPropertyChanged;
            owner = unitBase;
            if (data == null)
            {
                await LoadAssets();
                CurrentState = State.Cooldown;
                return;
            }

            timeInterval = skillData.AttackInterval(null);
            duration  = GetConfigArgs(SkillConst.Duration) * 0.001f;
            timer = TimeInterval + Duration - 0.5f;
            skillType = isMonster ? SkillType.Normal : SkillType.Auto;
            await LoadAssets();
            CurrentState = State.Cooldown;
            damagePoint = GetDamagePoint();
        }

        protected virtual void DataOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            timeInterval = GetConfigArgs(SkillConst.Intervals) * 0.001f;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected long GetDamagePoint()
        {
            //@TODO: mock data
            if (owner.IsMonster) return (long)owner.attr.Calc(AttributeType.Atk);
            return 0;
        }
        protected DamageArgs CreateDamage()
        {
            var args = new DamageArgs
            {
                effect = DamageArgs.SkillFactionEffect(SkillFaction.None),
                damagePoint = GetDamagePoint(),
                sender = monoOwner,
                skillIns = this,
                skillId = SkillData.id,
                skillData = SkillData,
                dmgType = DmgType.Normal
            };
            return args;
        }
        /// <summary>
        /// 主动释放技能
        /// </summary>
        public virtual void Shoot(WeaponSkill weaponSkill)
        {
        }
        public virtual void TakeSkill()
        {
            CurrentState = State.Cooldown;
            Progress = 1;
        }
        public virtual void CancelSkill()
        {
            CurrentState = State.Cooldown;
        }
        public virtual void OnUpdate(float deltaTime)
        {
            // 对怪物的冷却进行特殊处理
            if (isMonster)
            {
                if (CurrentState != State.Cooldown) return;
            }
            else
            {
                // 对主角的冷却进行特殊处理
                if (CurrentState == State.Ready) return;
                if(currentState == State.Idle) return;
                if(CurrentState == State.Taking) return;
            }

            if (CurrentState == State.Cooldown)
            {
                timer += deltaTime;
                Progress = Mathf.Clamp01(1 - timer / TimeInterval);
            }
            else if (CurrentState == State.Working)
            {
                timer += deltaTime;
                Progress = Mathf.Clamp01(timer / Duration);
            }

            if (timer > Duration && currentState == State.Working)
            {
                timer = 0;
                CurrentState = State.Cooldown;
                timeInterval = skillData.AttackInterval(null);
                duration = SkillData.Duration() * GameConst.TimeDivisor;
            }
            if (timer > TimeInterval && currentState == State.Cooldown)
            {
                timer = 0;
                CurrentState = State.Ready;
                timeInterval = skillData.AttackInterval(null);
                duration  = SkillData.Duration() * GameConst.TimeDivisor;
                if (AutoTakeSkill) TakeSkill();
            }
        }
        /// <summary>
        /// 清理所持有创建的子弹
        /// </summary>
        public void ClearBulletList()
        {
            bulletList.Clear();
        }
    }
}