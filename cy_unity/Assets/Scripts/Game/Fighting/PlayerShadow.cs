using System;
using UnityEngine;

namespace DH.Game
{
    public class PlayerShadow : MonoBehaviour
    {
        private void Start()
        {
            gameObject.SetActive(BattleManager.Instance.IsStageForest());
        }
    }
}