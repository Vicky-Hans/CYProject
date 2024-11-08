using Unity.Mathematics;

namespace PathFinding.DOTS
{
    public struct PathNode
    {
        public TriangleMeshNode MeshNode { get; set; }

        /// <summary>Parent node in the search tree</summary>
        public uint Parent { get; set; }

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

        public void UpdateG(PathNode parent)
        {
            this.G = parent.G + Cost + GetTraversalCost();
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
        public uint CalculateHScore(float3 targetPosition)
        {
            return (uint)math.round(math.distance(targetPosition, MeshNode.Position) * 1000);
        }

        public bool CanTraverse()
        {
            return true;
        }
    }
}