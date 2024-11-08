// using Spine.Unity;

using System;
using DH.Data;
using Spine.Unity;
using UnityEngine;
using AnimationState = Spine.AnimationState;

namespace DH.Game
{
    public class SpineAnimator : MonoBehaviour, IMonsterAnimator
    {
        private SkeletonAnimation animator;
        private AnimationState animationState;

        private long attackDuration = 600; //ms
        private long lastTime;
        
        public SkeletonAnimation Animator => animator;
        
        public CharacterController CharacterController { get; set; }

        private bool CanPlayAtk()
        {
            var nowTime = Lodash.GetUnixTimeMs();
            if (nowTime - lastTime <= attackDuration) return false;
            lastTime = nowTime;
            return true;
        }

        private void Awake()
        {
            animator = GetComponent<SkeletonAnimation>();
            animationState = animator.AnimationState;
        }

        public void PlayWalk()
        {
            if (animator.skeleton.Data.FindAnimation("walk") != null)
            {
                animationState.SetAnimation(0,"walk", true);
            }
        }

        public void PlayAttack()
        {
            if(!CanPlayAtk())return;
            animationState.SetAnimation(0,"atk", false);
            animationState.AddAnimation(0, "idle", true, 0);
        }

        public void PlayWalkAttack()
        {
            if(!CanPlayAtk())return;
            if (CharacterController != null && CharacterController.MoveComponent.IsMoving)
            {
                animationState.SetAnimation(0,"walk_atk", false);
                animationState.AddAnimation(0, "walk", true, 0);
            }
            else
            {
                animationState.SetAnimation(0,"atk", false);
                animationState.AddAnimation(0, "idle", true, 0);
            }
        }

        public void PlaySkill1()
        {
            animationState.SetAnimation(0,"skill1", false);
            animationState.AddAnimation(0, "walk", true, 0);
        }

        public void PlaySkill2()
        {
            animationState.SetAnimation(0,"skill2", false);
            animationState.AddAnimation(0, "walk", true, 0);
        }

        public void PlayIdle()
        {
            animationState.SetAnimation(0,"idle", true);
        }

        public void PlayDead()
        {
            //animationState.SetAnimation(0,"dead", false);
        }

        public void PlayerEnter()
        {
            animationState.SetAnimation(0,"chuchang", false);
        }

        public void PlayAnimation(string aniName)
        {
            switch (aniName)
            {
                case GameConst.AnimationName.Attack:
                case GameConst.AnimationName.Atk:
                    PlayAttack();
                    break;
                case GameConst.AnimationName.WalkAtk:
                    PlayWalkAttack();
                    break;
                case GameConst.AnimationName.Walk:
                    PlayWalk();
                    break;
                case GameConst.AnimationName.Skill1:
                    PlaySkill1();
                    break;
                case GameConst.AnimationName.Skill2:
                    PlaySkill2();
                    break;
                case GameConst.AnimationName.Idle:
                    PlayIdle();
                    break;
                case GameConst.AnimationName.Enter:
                    PlayerEnter();
                    break;
            }
        }

        public void PlaySpecAnimation(string aniName, bool loop = false)
        {
            animationState.SetAnimation(0, aniName, false);
        }
        
        public void AddSpecAnimation(string aniName, bool loop = false)
        {
            animationState.AddAnimation(0, aniName, loop, 0);
        }

        public void FlipX(bool flag)
        {
            animator.skeleton.ScaleX = flag ? -1 : 1;
        }

        public void Pause()
        {
            if (animationState != null)
                animationState.TimeScale = 0;
        }

        public void Resume()
        {
            if (animationState != null)
                animationState.TimeScale = 1;
        }
    }
}