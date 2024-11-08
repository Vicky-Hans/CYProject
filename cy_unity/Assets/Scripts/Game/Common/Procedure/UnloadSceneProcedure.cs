using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DH.Game
{
    [ProcedureDeep(3)]
    public class UnloadSceneProcedure : ProcedureBase
    {
        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
            ProcedureManager.Instance.Change(nameof(MainGameProcedure), true).Forget();
        }

        public override string GetSceneConfig()
        {
            return "Scenes/Empty";
        }
    }
}