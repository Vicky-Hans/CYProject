using System.Collections.Generic;
using FindingPath.Core;
using FindingPath.Data;
#if USE_FP
    using DH.LockStep.Framework;
#else
    using FP = System.Single;
    using TSVector2 = UnityEngine.Vector2;
    using TSVector = UnityEngine.Vector3;
    using TSRect = UnityEngine.Rect;
    using TSMatrix4x4 = UnityEngine.Matrix4x4;
    using TSMath = UnityEngine.Mathf;
#endif

namespace FindingPath.Path
{
    /// <summary>
    /// Stores temporary node data for a single pathfinding request.
    /// Every node has one PathNode per thread used.
    /// It stores e.g G score, H score and other temporary variables needed
    /// for path calculation, but which are not part of the graph structure.
    ///
    /// See: Pathfinding.PathHandler
    /// See: https://en.wikipedia.org/wiki/A*_search_algorithm
    public sealed class PathNode
    {
        public TSVector Position => MeshNode.Position;
        
        public TriangleMeshNode MeshNode { get; set; }

        /// <summary>Parent node in the search tree</summary>
        public PathNode Parent { get; set; }

        public uint Cost { get; set; }

        /// <summary>
        /// G score, cost to get to this node
        /// </summary>
        public uint G { get; set; }

        /// <summary>
        /// H score, estimated cost to get to to the target
        /// </summary>
        public uint H { get; set; }

        /// <summary>
        /// F score. H score + G score
        /// </summary>
        public uint F => H + G;

        public ushort HeapIndex { get; set; }

        /// <summary>
        /// 路径目标节点标志位
        /// </summary>
        public bool IsTargetNode { get; set; }

        /// <summary>
        /// 起始点、终止点标志位
        /// </summary>
        public bool IsPointNode { get; set; }

        public bool IsInOpenList { get; set; }

        public PathNode()
        {
            HeapIndex = BinaryHeap.NotInHeap;
        }

        public void UpdateG()
        {
            this.G = Parent.G + Cost + GetTraversalCost();
        }

        /// <summary>Returns the cost of traversing the given node</summary>
        public uint GetTraversalCost()
        {
            return 0;
        }
        
        /// <summary>
        /// Estimated cost from the specified node to the target.
        /// See: https://en.wikipedia.org/wiki/A*_search_algorithm
        /// </summary>
        public uint CalculateHScore(TSVector targetPosition)
        {
            return (uint)TSMath.Round((targetPosition - MeshNode.Position).magnitude * 1000);
        }

        public bool CanTraverse()
        {
            return true;
        }
    }

    public sealed class PathNodeSet
    {
        private readonly Dictionary<TriangleMeshNode, PathNode> pathNodeDic =
            new Dictionary<TriangleMeshNode, PathNode>();

        public PathNode GetPathNode(TriangleMeshNode meshNode)
        {
            if (!pathNodeDic.TryGetValue(meshNode, out var pathNode))
            {
                pathNode = new PathNode()
                {
                    MeshNode = meshNode,
                };
                
                pathNodeDic.Add(meshNode, pathNode);
            }
            
            return pathNode;
        }

        public void Clear()
        {
            pathNodeDic.Clear();
        }
    }
    
}