#if USE_FP
using DH.LockStep.Framework;
#else
using FP = System.Single;
using TSVector2 = UnityEngine.Vector2;
using TSVector = UnityEngine.Vector3;
using TSRect = UnityEngine.Rect;
using TSMatrix4x4 = UnityEngine.Matrix4x4;
using DHLogger = UnityEngine.Debug;
#endif

namespace Pathfinding.DHRVO {
    /// <summary>
    /// One vertex in an obstacle.
    /// This is a linked list and one vertex can therefore be used to reference the whole obstacle
    /// </summary>
    public class ObstacleVertex {
        public bool ignore;

        /// <summary>Position of the vertex</summary>
        public TSVector position;
        public TSVector2 dir;

        /// <summary>Height of the obstacle in this vertex</summary>
        public FP height;

        /// <summary>Collision layer for this obstacle</summary>
        public RVOLayer layer = RVOLayer.DefaultObstacle;


        /// <summary>Next vertex in the obstacle</summary>
        public ObstacleVertex next;
        /// <summary>Previous vertex in the obstacle</summary>
        public ObstacleVertex prev;
    }
}