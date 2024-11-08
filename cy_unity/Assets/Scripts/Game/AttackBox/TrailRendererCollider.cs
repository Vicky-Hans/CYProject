using System;
using UnityEngine;

namespace DH.Game
{
    [RequireComponent(typeof(MeshCollider))]
    public class TrailRendererCollider : MonoBehaviour
    {
        public MeshCollider meshCollider;
        public TrailRenderer trail;
        private Mesh mesh;
        private void Start()
        {
            meshCollider = GetComponent<MeshCollider>();
            trail = GetComponent<TrailRenderer>();
            mesh = new Mesh();
            meshCollider.sharedMesh = mesh;
            meshCollider.convex = true;
            meshCollider.isTrigger = true;
        }

        private void OnDestroy()
        {
            Destroy(mesh);
        }

        private void Update()
        {
            trail.BakeMesh(mesh,true);
        }
    }
}