using UnityEngine;

namespace DH.Game
{
    public static class Vector3Helper
    {
        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation) {
            return rotation  * (point - pivot) + pivot;
        }
    }
}