using System;
using UnityEngine;

namespace DH.Game
{
    public class CircularArcAttackBox : AttackBox
    {
        public float angle;

        private SphereCollider sphereCollider;
        private float minRadius;
        
        public void SetAttackRange(float closeRadius, float maxRadius)
        {
            if (!sphereCollider)
            {
                sphereCollider = GetComponent<SphereCollider>();
            }

            sphereCollider.radius = maxRadius;
            this.minRadius = closeRadius;
        }
        
        public override bool CheckInRange(Vector3 target)
        {
            var cacheTransform = transform;
            var direction = target - cacheTransform.position;

            // 扇形攻击框
            if (angle < 360)
            {
                var forward = cacheTransform.forward;
                forward.y = direction.y = 0;
                float diffAngle = Vector3.Angle(forward, direction);
                if (diffAngle > (angle * 0.5f))
                {
                    return false;
                }
            }

            // 扇形环状或者圆环状攻击框
            if (direction.sqrMagnitude < minRadius * minRadius)
            {
                return false;
            }
            
            return true;
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Draws a wire arc.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="dir">The direction from which the anglesRange is taken into account</param>
        /// <param name="anglesRange">The angle range, in degrees.</param>
        /// <param name="radius"></param>
        /// <param name="maxSteps">How many steps to use to draw the arc.</param>
        public static void DrawWireArc(Vector3 position, Vector3 dir, float anglesRange, float radius, float maxSteps = 20)
        {
            var srcAngles = GetAnglesFromDir(position, dir);
            var initialPos = position;
            var posA = initialPos;
            var stepAngles = anglesRange / maxSteps;
            var angle = srcAngles - anglesRange / 2;
            for (var i = 0; i <= maxSteps; i++)
            {
                var rad = Mathf.Deg2Rad * angle;
                var posB = initialPos;
                posB += new Vector3(radius * Mathf.Cos(rad), 0, radius * Mathf.Sin(rad));

                Gizmos.DrawLine(posA, posB);

                angle += stepAngles;
                posA = posB;
            }
            Gizmos.DrawLine(posA, initialPos);
        }

        static float GetAnglesFromDir(Vector3 position, Vector3 dir)
        {
            var forwardLimitPos = position + dir;
            var srcAngles = Mathf.Rad2Deg * Mathf.Atan2(forwardLimitPos.z - position.z, forwardLimitPos.x - position.x);

            return srcAngles;
        }
        
        protected override void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            var matrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            var sphereCollider = GetComponent<SphereCollider>();
            if (!sphereCollider)
            {
                Gizmos.matrix = matrix;
                return;
            }
            DrawWireArc(Vector3.zero, Vector3.forward, angle, sphereCollider.radius);
            Gizmos.matrix = matrix;
        }
#endif   
    }
}