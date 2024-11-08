using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Pool;

namespace DH.Game
{
    public static class ParticleHelper
    {
        public const float CircleHintScale = 1 / 0.94f * 2;
        public static readonly Vector3 CircleHintOffset = new Vector3(0f,0.05f,0f);
        
        public static void SetLifetime(GameObject fx, float lifetime)
        {
            var fxs = fx.GetComponentsInChildren<ParticleSystem>();
            fxs[0].Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            
            foreach (var item in fxs)
            {
                var module = item.main;
                module.startLifetime = lifetime;
            }

            if (fxs.Length > 0)
            {
                fxs[0].Play();
            }
        }

        public static void PlayFx(GameObject fx)
        {
            var particle = fx.GetComponentInChildren<ParticleSystem>();
            if (!particle)
            {
                return;
            }
            particle.Clear(true);
            particle.Play();
        }

        public static void ClearTrailRenderer(GameObject fx)
        {
            var trails = ListPool<TrailRenderer>.Get();
            fx.GetComponentsInChildren(trails);
            foreach (var trail in trails)
            {
                trail.Clear();
            }
            ListPool<TrailRenderer>.Release(trails);
        }
    }
}