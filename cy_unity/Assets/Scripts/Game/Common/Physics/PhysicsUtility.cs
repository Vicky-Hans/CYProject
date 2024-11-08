using UnityEngine;

namespace DH.Game
{
    public static class PhysicsUtility
    {
        public static readonly int MaxCacheCount = 50;
        public static readonly Collider2D[] CacheCollider = new Collider2D[MaxCacheCount];
    }
}