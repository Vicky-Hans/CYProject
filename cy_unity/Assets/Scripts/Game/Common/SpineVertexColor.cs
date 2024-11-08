using System;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace DH.Game
{
    [RequireComponent(typeof(SkeletonAnimation))]
    [ExecuteInEditMode]
    public class SpineVertexColor : MonoBehaviour, IMonsterVertexColor
    {
        public Color color = Color.black;
        public Color blackColor = Color.black;
        public Color whiteColor = Color.white;
        
        private Skeleton skeleton;
        private const float Cd = 0.3f;
        private const float DeadCd = 1f;
        private float time = Cd;
        private float deadTime = DeadCd;
        
        private enum ESpineVertexState
        {
            Normal,
            Hurt,
            Dead
        }

        private ESpineVertexState eState;

        private void Start()
        {
            skeleton = GetComponent<SkeletonAnimation>().skeleton;
            color.r = 0f;
            color.g = 0f;
            skeleton?.SetColor(color);
            time = Cd;
            deadTime = DeadCd;
            eState = ESpineVertexState.Normal;
        }

        public void PlayHurt()
        {
            time = 0f;
            color.r = 1f;
            color.g = 0f;
            eState = ESpineVertexState.Hurt;
        }

        public void PlayDead()
        {
            deadTime = 0f;
            color.r = 0f;
            color.g = 0f;
            eState = ESpineVertexState.Dead;
        }

        private void UpdateHurt(float dt)
        {
            time += Time.deltaTime;
            if (time >= Cd)
            {
                time = Cd;
                eState = ESpineVertexState.Normal;
            }
            var factor = time / Cd;
            factor = factor * factor * factor;
            color.r = Mathf.Lerp(1, 0, factor);
            skeleton?.SetColor(color);
        }

        private void UpdateDead(float dt)
        {
            deadTime += Time.deltaTime;
            if (deadTime >= DeadCd)
            {
                deadTime = DeadCd;
                eState = ESpineVertexState.Normal;
            }
            var factor = deadTime / DeadCd;
            color.g = Mathf.Lerp(0, 1, factor);
            skeleton?.SetColor(color);
        }
        
        private void Update()
        {
            if(eState == ESpineVertexState.Normal)return;
            var dt = Time.deltaTime;
            if (eState == ESpineVertexState.Hurt)
            {
                UpdateHurt(dt);
            }
            else if (eState == ESpineVertexState.Dead)
            {
                UpdateDead(dt);
            }
            // skeleton?.SetColor(color);
        }
    }
}