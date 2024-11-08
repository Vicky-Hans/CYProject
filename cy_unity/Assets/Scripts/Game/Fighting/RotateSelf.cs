using System;
using UnityEngine;

namespace DH.Game
{
    public class RotateSelf : MonoBehaviour
    {
        public float Spd { get; set; }
        public BaseBullet BulletIns { get; set; }
        private Transform transCache;
        private Vector3 eulerAngles;

        private void Start()
        {
            transCache = transform;
            eulerAngles = transCache.eulerAngles;
        }

        public void OnUpdate(float dt)
        {
            if(BulletIns == null)return;
            if(BulletIns.Recycled)return;
            eulerAngles.z -= Spd * dt;
            if (eulerAngles.z < 360f)
            {
                eulerAngles.z += 360f;
            }
            if(transCache)
            {
                transCache.eulerAngles = eulerAngles;
            }
        }
    }
}