using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DH.Asset;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DH.Game
{
    public partial class DizzyEffect : BaseEffect
    {
        public const string MonsterDizzy = "Monster/Effects/DizzyEffect";

        [AssetPath] public string dizzyFxPath;

        private GameObject dizzyFx;

        public override async UniTask OnAddEffect(Func<string, long> paramsGetter, string effectName)
        {
            Owner.StopMove(nameof(DizzyEffect));
            await base.OnAddEffect(paramsGetter, effectName);
            if (cts == null || cts.IsCancellationRequested) return;
            CreateFx();
        }

        private void CreateFx()
        {
            var parent = Owner.transform;
            dizzyFx = InstantiateObj(dizzyFxPath, parent, false);
            dizzyFx.transform.position = parent.position;
        }

        public override void OnRemoveEffect()
        {
            Owner.ResumeMove(nameof(DizzyEffect));
            ReleaseObj(dizzyFx);
            base.OnRemoveEffect();
        }

        public override void CombineEffect(Func<string, long> paramsGetter)
        {
            timer = 0;
            var time = paramsGetter?.Invoke(SkillConst.Duration) ?? 0;
            if (time > duration) duration = time;
        }
    }
}