using UnityEngine;

namespace DH.Game
{
    /// <summary>
    /// 单独使用Trigger方式攻击敌人
    /// </summary>
    public class AttackBox : AttackComponent
    {
        public void OnTriggerEnter2D(Collider2D other)
        {
            OnTargetEnter(other);
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            var colider = GetComponent<Collider2D>();
            Gizmos.color = colider && colider.enabled ? Color.red : Color.green;
            var matrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            if (colider is BoxCollider2D)
            {
                var box = colider as BoxCollider2D;
                Gizmos.DrawWireCube(box.offset, box.size);
            }
            else if (colider is CircleCollider2D)
            {
                var circle = colider as CircleCollider2D;
                Gizmos.DrawWireSphere(circle.offset, circle.radius);
            }

            Gizmos.matrix = matrix;
        }

#endif
    }
}