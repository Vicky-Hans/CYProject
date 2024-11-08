using UnityEngine;

namespace DH.Game
{
    [ExecuteInEditMode]
    public class SpriteVertexColor: MonoBehaviour, IMonsterVertexColor
    {
        public Color color = Color.black;
        public Color blackColor = Color.black;
        public Color whiteColor = Color.white;

        private SpriteRenderer sprite;
        private const float Cd = 0.3f;
        private const float DeadCd = 1f;
        private float time = Cd;
        private float deadTime = DeadCd;

        private enum ESpriteVertexState
        {
            Normal,
            Hurt,
            Dead
        }
        private ESpriteVertexState eState;
        
        private void Start()
        {
            sprite = GetComponent<SpriteRenderer>();
            Reset();
        }

        private void OnDestroy()
        {
            Reset();
        }

        private void Reset()
        {
            color.r = 0f;
            color.g = 0f;
            SetColor(color);
            time = Cd;
            deadTime = DeadCd;
            eState = ESpriteVertexState.Normal;
        }

        private void SetColor(Color sColor)
        {
            if(sprite != null)
                sprite.color = sColor;
        }

        private void OnDisable()
        {
            Reset();
        }

        public void PlayHurt()
        {
            time = 0f;
            color.r = 1f;
            color.g = 0f;
            eState = ESpriteVertexState.Hurt;
        }

        public void PlayDead()
        {
            deadTime = 0f;
            color.r = 0f;
            color.g = 0f;
            eState = ESpriteVertexState.Dead;
        }
        
        private void UpdateHurt(float dt)
        {
            time += Time.deltaTime;
            if (time >= Cd)
            {
                time = Cd;
                eState = ESpriteVertexState.Normal;
            }
            var factor = time / Cd;
            factor = factor * factor * factor;
            color.r = Mathf.Lerp(1, 0, factor);
            SetColor(color);
        }

        private void UpdateDead(float dt)
        {
            deadTime += Time.deltaTime;
            if (deadTime >= DeadCd)
            {
                deadTime = DeadCd;
                eState = ESpriteVertexState.Normal;
            }
            var factor = deadTime / DeadCd;
            color.g = Mathf.Lerp(0, 1, factor);
            SetColor(color);
        }

        
        private void Update()
        {
            if(eState == ESpriteVertexState.Normal)return;
            var dt = Time.deltaTime;
            if (eState == ESpriteVertexState.Hurt)
            {
                UpdateHurt(dt);
            }
            else if (eState == ESpriteVertexState.Dead)
            {
                UpdateDead(dt);
            }
        }
    }
}