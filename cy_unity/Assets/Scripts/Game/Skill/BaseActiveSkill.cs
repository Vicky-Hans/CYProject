using Cysharp.Threading.Tasks;
using DH.Data;
using DH.UIFramework.Commands;

namespace DH.Game
{
    /// <summary>
    /// 针对主角的主动技能，涉及到UI按钮输入等功能
    /// 如果为怪物的技能，只需要继承BaseSkill即可
    /// </summary>
    public class BaseActiveSkill : BaseSkill
    {
        private float takingTimer;
        private float duration;
        private int skillRemainTime;

        protected float TakingTimer => takingTimer;

        public virtual bool SupportInput => true;

        public virtual bool DisplayTakingTime => CurrentState == State.Taking;

        public int SkillRemainTime
        {
            get => skillRemainTime;
            private set => Set(ref skillRemainTime, value);
        }
        public RelayCommand TakeSkillCmd { get; private set; }

        public override async UniTask Init(Skill data, UnitBase unitBase,BaseMonoUnit behaviour)
        {
            await base.Init(data, unitBase,behaviour);

            TakeSkillCmd = new RelayCommand(TakeSkill,CanTakeSkill);
            Progress = 0;
            CurrentState = State.Ready;
        }

        public override void TakeSkill()
        {
            CurrentState = State.Taking;
            Progress = 1;
            // 每次读取技能属性
            duration = GetConfigArgs(SkillConst.Duration) * 0.001f;
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (currentState != State.Taking)
            {
                return;
            }

            takingTimer += deltaTime;
            if (takingTimer > duration)
            {
                takingTimer = 0;
                CurrentState = State.Cooldown;
                OnSkillEnd();
                return;
            }

            SkillRemainTime = (int)(duration - takingTimer);
        }

        protected virtual void OnSkillEnd()
        {
            
        }
    }
}