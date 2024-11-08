using System;
using Cysharp.Threading.Tasks;
using DH.Data;

namespace DH.Game
{
    /// <summary>
    /// 可以叠加层数的属性Buff
    /// </summary>
    public class AttributeOverlayModifyEffect : BaseEffect
    {
        public AttributeType attrType;
        public AttributeCalcType calcType;
        public AttributeValueType valueType;
        public long value;

        internal long maxOverlay;
        internal long currentOverlay;

        public override async UniTask OnAddEffect(Func<string,long> paramsGetter,string effectName)
        {
            currentOverlay = 1;
            Count = 1;
            var attrManager = owner.AttributeMgr;
            value = paramsGetter.Invoke(SkillConst.Value);
            attrManager.Modify(attrType,valueType,value);
            await base.OnAddEffect(paramsGetter,effectName);
        }

        public override void OnRemoveEffect()
        {
            var attrManager = owner.AttributeMgr;
            attrManager.Modify(attrType,valueType,value * currentOverlay);
            base.OnRemoveEffect();
        }
        
        public override void CombineEffect(Func<string,long> paramsGetter)
        {
            currentOverlay++;
            if (currentOverlay > maxOverlay)
            {
                currentOverlay = maxOverlay;
            }
            else
            {
                var attrManager = owner.AttributeMgr;
                attrManager.Modify(attrType,valueType,value);
            }

            Count = (int)currentOverlay;
        }
    }
}