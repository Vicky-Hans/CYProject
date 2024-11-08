using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QD.Base
{
    [ExecuteInEditMode]
    public class SpriteShadowmapSettings : MonoBehaviour
    {
        public Vector2 Size = Vector2.one;
        public float Near = 0.1f;
        public float Far = 200;

        public Matrix4x4 ProjMatrix
        {
            get
            {
                var worldPos = transform.position;
                Matrix4x4 orthProjMatrix = Matrix4x4.Ortho(-Size.x, Size.x,
                    -Size.y, Size.y, Near, Far);

                return orthProjMatrix;
            }
        }

        public Matrix4x4 ViewMatrix
        {
            get
            {
                Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
                return m * transform.worldToLocalMatrix;
            }
        }

        public bool IsEnable => gameObject.activeInHierarchy && enabled;

        private static SpriteShadowmapSettings instance;
        public static SpriteShadowmapSettings Instance => instance;

        private void Awake()
        {
            instance = this;
        }

        private void OnDestroy()
        {
            instance = null;
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            var y = Size.y * 2;
            var x = Size.x * 2;
            var z = Far - Near;
            var worldPos = transform.position;
            worldPos = new Vector3(worldPos.x, worldPos.y, worldPos.z + (z * 0.5f + Near));


            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(worldPos, new Vector3(x, y, z));

        }

#endif

    }
}