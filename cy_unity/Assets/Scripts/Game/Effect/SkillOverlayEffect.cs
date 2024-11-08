using System;
using Cysharp.Threading.Tasks;

namespace DH.Game
{
    /// <summary>
    /// 叠加技能效果BUFF,用于辅助技能计算增伤或者其他
    /// 重复添加Effect只会增加层数，不会增加效果的引用计数
    /// </summary>
    public class SkillOverlayEffect : BaseEffect
    {
        /// <summary>
        /// 当前BUFF叠加层数
        /// </summary>
        internal long currentOverlay;
        internal long maxOverlay;

        public override async UniTask OnAddEffect(Func<string, long> paramsGetter, string name)
        {
            currentOverlay = 1;
            maxOverlay = paramsGetter.Invoke(SkillConst.Overlay);
            await base.OnAddEffect(paramsGetter, name);
        }

        public override void CombineEffect(Func<string,long> paramsGetter)
        {
            this.timer = 0;
            var time = paramsGetter.Invoke(SkillConst.Duration);
            if (time > duration)
            {
                duration = time;
            }
            
            currentOverlay++;
            if (maxOverlay != 0 && currentOverlay > maxOverlay)
            {
                currentOverlay = maxOverlay;
            }

            Count = (int)currentOverlay;
        }
    }
}