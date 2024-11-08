using System;
using DG.Tweening;
using DH.Data;
using TMPro;
using UnityEngine;

namespace DH.Game
{
    public enum HurtNumType
    {
        /// <summary>
        /// 普通伤害
        /// </summary>
        Plain = 0,
        /// <summary>
        /// 暴击伤害
        /// </summary>
        Crit,
        /// <summary>
        /// 额外伤害
        /// </summary>
        Extra,
        /// <summary>
        /// 回复
        /// </summary>
        Recovery,
        /// <summary>
        /// 闪避
        /// </summary>
        Miss,
        /// <summary>
        /// 免疫
        /// </summary>
        Immune
    }
    public class HurtNum : BaseAssetEntity
    {
        public TMP_Text tmpNum;
        public AnimationCurve curve;
        private AnimationCurve scaleCurve;
        private AnimationCurve alphaCurve;
        public Color plainColor;
        public Color critColor;
        public Color extraColor;
        public Color recoveryColor;

        private float effectTime = 0.5f;
        private float duration = 1.0f;
        private float timer;
        private Vector3 startPos;
        private Vector3 endPos;
        private readonly Vector3 endOffset = new Vector3(0f, 0.5f, 0);
        private Vector3 endPos2;
        private Vector3 delPos = new Vector3(Lodash.RandRangeFloat(-0.15f, 0.15f), Lodash.RandRangeFloat(0.4f, 0.7f), 0);
        private Color color;
        private float startAlpha = 0.3f;

        private Vector3 startScale = Vector3.one * 0.5f;

        private float scaleFactor = 1f;

        private const float TmpScale = 1f;

        private bool fixedY;

        private Transform transformCache;

        private void Start()
        {
            transformCache = transform;
            transform.localScale = Vector3.one;
            scaleCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.3f, 1.3f),
                new Keyframe(0.5f, 1.3f), new Keyframe(0.67f, 1));
            alphaCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1),
                new Keyframe(0.5f, 1), new Keyframe(0.67f, 0));
        }


        private bool Inited { get; set; }
        public void Init(long num, HurtNumType type, IPool<GameObject> pool)
        {
            timer = 0;
            transform.localScale = startScale;
            if (type == HurtNumType.Recovery)
            {
                // tmpNum.text = $"+{num}";
                tmpNum.text = Lodash.GetKNum(num);
            }
            else if(type == HurtNumType.Miss)
            {
                tmpNum.text = "miss";
            }
            else if (type == HurtNumType.Immune)
            {
                tmpNum.text = "immune";
            }
            else
            {
                // tmpNum.text = $"{num}";
                tmpNum.text = Lodash.GetKNum(num);
            }
            startPos = transform.position;
            if (!fixedY)
            {
                var radius = Lodash.RandRangeFloat(0.4f, 0.7f);
                endPos = Lodash.PosOnCircle(startPos, radius);
                endPos2 = endPos;
            }
            else
            {
                endPos = startPos + delPos;
                // startPos = endPos;
                endPos2 = endPos;
            }
            
            scaleFactor = 1f;
            var tmpColor = plainColor;
            if (type == HurtNumType.Crit)
            {
                tmpColor = critColor;
                scaleFactor = 1.1f;
            }
            else if (type == HurtNumType.Extra)
            {
                tmpColor = extraColor;
                scaleFactor = 1.3f;
            }
            else if (type == HurtNumType.Recovery)
            {
                tmpColor = recoveryColor;
            }
            tmpNum.color = tmpColor;
            color = tmpNum.color;
            color.a = 1;// startAlpha;
            tmpNum.color = color;
            Inited = true;
            // PlayAnim();
            BattleManager.Instance.fightingManagerIns.AddAutoReleaseUnit(gameObject, duration, pool, () =>
            {
                Inited = false;
            });
        }

        private void PlayAnim()
        {
            var trans = transform;
            var scaleSequence = DOTween.Sequence();
            scaleSequence.Append(trans.DOScale(Vector3.one * (1.3f * TmpScale * scaleFactor), 0.3f));
            scaleSequence.Append(trans.DOScale(Vector3.one * (1.3f * TmpScale * scaleFactor), 0.2f));
            scaleSequence.Append(trans.DOScale(Vector3.one * (1f * TmpScale * scaleFactor), 0.17f));
            scaleSequence.SetLoops(1);
            scaleSequence.Play();
            var tmpColor = tmpNum.color;
            var tmpColor0 = new Color(tmpColor.r, tmpColor.g, tmpColor.b, 0);
            // var tmpColor1 = new Color(tmpColor.r, tmpColor.g, tmpColor.b, 1); 
            // var tmpColor2 = new Color(tmpColor.r, tmpColor.g, tmpColor.b, 0.7f);
            tmpNum.color = tmpColor0;
            var alphaSequence = DOTween.Sequence();
            alphaSequence.Append(DOTween.ToAlpha(()=>tmpNum.color, (x) => tmpNum.color = x, 1f, 0.3f));
            alphaSequence.Append(DOTween.ToAlpha(()=>tmpNum.color, (x) => tmpNum.color = x, 1f, 0.2f));
            alphaSequence.Append(DOTween.ToAlpha(()=>tmpNum.color, (x) => tmpNum.color = x, 0f, 0.17f));
            alphaSequence.SetLoops(1);
            alphaSequence.Play();
        }

        public void SetDeltaPosY(float y)
        {
            delPos.y = y;
            fixedY = true;
            scaleFactor = 1f;
        }

        public void Update()
        {
            if(!Inited) return;
            if(timer > 1) return;
            var dt = Time.deltaTime;
            timer += dt;
            UpdateCurve(timer);
            if (timer <= effectTime)
            {
                UpdateShow(timer);
            }
            else
            {
                UpdateHide(timer);
            }
        }

        private void UpdateCurve(float timer)
        {
            transformCache.localScale = Vector3.one * (scaleCurve.Evaluate(timer) * TmpScale * scaleFactor);
            var tmpColor = tmpNum.color;
            tmpColor.a = alphaCurve.Evaluate(timer);
            tmpNum.color = tmpColor;
        }

        public void UpdateShow(float ctime)
        {
            transform.position = Vector3.Lerp(startPos, endPos, curve.Evaluate((ctime / effectTime)));
            // transform.localScale = Vector3.Lerp(startScale, Vector3.one*1.2f * scaleFactor, scaleCurve.Evaluate(ctime / effectTime));
        }

        public void UpdateHide(float ctime)
        {
            transform.position = Vector3.Lerp(endPos, endPos2, Mathf.Clamp01(((ctime-0.5f) / 0.5f)));
            // transform.localScale = Vector3.Lerp(Vector3.one*1.2f * scaleFactor, startScale, ctime / effectTime);
            // color.a = Mathf.Lerp(1, 0, curve.Evaluate(ctime / effectTime));
            // tmpNum.color = color;
        }
    }
}