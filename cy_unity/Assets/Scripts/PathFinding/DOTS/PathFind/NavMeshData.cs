using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;


namespace PathFinding.DOTS
{
    public unsafe struct NavMeshData : IDisposable
    {
        public unsafe struct Rect
        {
            public float xMin;
            public float yMin;
            public float width;
            public float height;

            public float xMax => xMin + width;
            public float yMax => yMin + height;


            public bool Contains(float2 point)
            {
                return (double)point.x >= (double)xMin &&
                       (double)point.x < (double)xMax &&
                       (double)point.y >= (double)yMin &&
                       (double)point.y < (double)yMax;
            }
        }

        public ushort TileX;
        public ushort TileY;

        public Rect RectXZ;

        public float3 CenterPosition;

        public bool IgnoreAxisY;

        public UnsafeList<float3> Vertexes;

        public UnsafeList<TriangleMeshNode> MeshNodes;

        public SpaceOctree Octree;

        public void Dispose()
        {
            Vertexes.Dispose();

            foreach (var triangleMeshNode in MeshNodes) triangleMeshNode.Dispose();
            MeshNodes.Dispose();
            Octree.Dispose();
        }
    }

    public unsafe struct TriangleMeshNode : IDisposable
    {
        public uint TileX => TriangleMeshNodeUtil.GetTileX(NodeIndex);

        public uint TileY => TriangleMeshNodeUtil.GetTileY(NodeIndex);

        public uint MeshNodeIndex => TriangleMeshNodeUtil.GetMeshNodeIndex(NodeIndex);

        /// <summary>
        /// 由 tileX & tileY & MeshNodeIndex 组成，字节占用从高到低为：1 + 1 + 2 = 4 个字节
        /// </summary>
        public uint NodeIndex;

        public int V0;

        public int V1;

        public int V2;

        /// <summary>
        /// Position of the node in local space.
        /// </summary>
        public float3 Position;

        public UnsafeList<Connection> Connections;

        public TriangleMeshNode(int v0, int v1, int v2)
        {
            NodeIndex = 0;
            Position = float3.zero;
            Connections = new UnsafeList<Connection>();

            V0 = v0;
            V1 = v1;
            V2 = v2;
        }

        public void Dispose()
        {
            Connections.Dispose();
        }
    }


    /// <summary>
    /// Represents a connection to another node
    /// </summary>
    public unsafe struct Connection
    {
        public uint NodeIndex;

        /// <summary>
        /// Cost of moving along this connection.
        /// A cost of 1000 corresponds approximately to the cost of moving one world unit.
        /// </summary>
        public uint Cost;

        /// <summary>
        /// Side of the node shape which this connection uses.
        /// Used for mesh nodes.
        /// A value of 0 corresponds to using the side for vertex 0 and vertex 1 on the node. 1 corresponds to vertex 1 and 2, etc.
        /// A negative value means that this connection does not use any side at all (this is mostly used for off-mesh links).
        ///
        /// Note: Due to alignment, the <see cref="node"/> and <see cref="cost"/> fields use 12 bytes which will be padded
        /// to 16 bytes when used in an array even if this field would be removed.
        /// So this field does not contribute to increased memory usage.
        /// </summary>
        public byte ShapeEdge;

        public Connection(uint cost, byte shapeEdge, uint nodeIndex)
        {
            Cost = cost;
            ShapeEdge = shapeEdge;
            NodeIndex = nodeIndex;
        }
    }

    public unsafe struct SpaceOctree
    {
        /// <summary>
        /// local坐标系的轴对齐包围盒
        /// </summary>
        public struct AABB
        {
            /// <summary>
            /// The maximum point of the box.
            /// </summary>
            public float3 min;

            /// <summary>
            /// The minimum point of the box.
            /// </summary>
            public float3 max;

            public float3 center => (min + max) * .5f;

            public float3 size => max - min;

            public float3 extents => size * .5f;

            /// <summary>
            /// Checks whether a point is inside, outside or intersecting
            /// a point.
            /// </summary>
            /// <param name="point">A point in space.</param>
            /// <returns>The ContainmentType of the point.</returns>
            public bool Contains(float3 point)
            {
                return min.x <= point.x && point.x <= max.x &&
                       min.y <= point.y && point.y <= max.y &&
                       min.z <= point.z && point.z <= max.z;
            }

            /// <summary>
            /// 获得点到轴对齐包围盒最近的距离的平方
            /// </summary>
            /// <param name="point"></param>
            /// <returns></returns>
            public float PointClosestSqrDistance(float3 point)
            {
                point = point - center;

                var xDelta = math.abs(point.x) - extents.x;
                xDelta = math.max(xDelta, 0);

                var yDelta = math.abs(point.y) - extents.y;
                yDelta = math.max(yDelta, 0);

                var zDelta = math.abs(point.z) - extents.z;
                zDelta = math.max(zDelta, 0);
                return xDelta * xDelta + yDelta * yDelta + zDelta * zDelta;
            }

            /// <summary>
            /// 获得点到轴对齐包围盒最近的距离
            /// </summary>
            /// <param name="point"></param>
            /// <returns></returns>
            public float PointClosestDistance(float3 point)
            {
                var sqrResult = PointClosestSqrDistance(point);
                return math.sqrt(sqrResult);
            }
        }

        public AABB Box { get; set; }

        public UnsafeList<int> Nodes;

        public UnsafeList<SpaceOctree> ChildTree;


        public void Dispose()
        {
            Nodes.Dispose();
            ChildTree.Dispose();
        }
    }
}