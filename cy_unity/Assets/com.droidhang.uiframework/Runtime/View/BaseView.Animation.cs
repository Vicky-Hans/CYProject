using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DH.UIFramework
{
    public partial class BaseView
    {
        private static readonly int Open = Animator.StringToHash("Open");
        private static readonly int Close = Animator.StringToHash("Close");

        private bool disabled;

        protected virtual bool IgnoreAnimator => false;

        private IEnumerator WaitAnimationEnd(Animator animator, int stateNameHash)
        {
            float timer = 1.0f;
            while (timer > 0)
            {
                if (disabled)
                {
                    break;
                }
                float delta = Time.unscaledDeltaTime;
                timer -= delta;
                animator.Update(delta);
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.tagHash != stateNameHash)
                {
                    yield break;
                }
                yield return null;
            }
        }

        public async UniTask PlayCloseAnimation()
        {
            if (IgnoreAnimator)
            {
                return;
            }
            var animator = GetComponent<Animator>();
            if (!animator)
            {
                return;
            }
            animator.enabled = false;
            animator.SetTrigger(Close);
            await WaitAnimationEnd(animator, Close);;
            if (disabled)
            {
                return;
            }
            animator.Play("KeepClose");
            animator.Update(Time.unscaledDeltaTime);
        }
        
        public async UniTask PlayOpenAnimation()
        {
            if (IgnoreAnimator)
            {
                return;
            }
            
            var animator = GetComponent<Animator>();
            if (!animator)
            {
                return;
            }
            animator.enabled = false;
            animator.SetTrigger(Open);
            await WaitAnimationEnd(animator, Open);
            if (disabled)
            {
                return;
            }
            animator.Play("KeepOpen");
            animator.Update(Time.unscaledDeltaTime);
        }
    }
}