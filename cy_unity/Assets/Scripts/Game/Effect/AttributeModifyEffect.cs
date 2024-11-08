using System;
using Cysharp.Threading.Tasks;
using DH.Data;

namespace DH.Game
{
    /// <summary>
    /// 不可以叠加属性的Buff
    /// </summary>
    public class AttributeModifyEffect : BaseEffect
    {
        public AttributeType attrType;
        public AttributeCalcType calcType;
        public AttributeValueType valueType;
        public long value;
        public bool onlyCombineDuration;
        /// <summary>
        /// 针对部分技能的Buff持续时间参数key有区别，可以按需进行配置
        /// </summary>
        public string durationKey = "duration";

        public override async UniTask OnAddEffect(Func<string,long> paramsGetter,string effectName)
        {
            var attrManager = owner.AttributeMgr;
            value = paramsGetter.Invoke(SkillConst.Value);
            attrManager.Modify(attrType,valueType,value);
            await base.OnAddEffect(paramsGetter,effectName);
        }

        public override void OnRemoveEffect()
        {
            var attrManager = owner.AttributeMgr;
            attrManager.Modify(attrType,valueType,value);
            base.OnRemoveEffect();
        }
        
        public override void CombineEffect(Func<string,long> paramsGetter)
        {
            if (onlyCombineDuration)
            {
                timer = 0;
                var time = paramsGetter.Invoke(durationKey) * 0.001f;
                if (time > duration)
                {
                    duration = time;
                }
            }
            else
            {
                referenceCount++;
            }
        }
    }
}