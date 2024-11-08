using System;
using Newtonsoft.Json;
#if USE_FP
    using DH.LockStep.Framework;
#else
    using FP = System.Single;
    using TSVector2 = UnityEngine.Vector2;
    using TSVector = UnityEngine.Vector3;
    using TSRect = UnityEngine.Rect;
    using TSMatrix4x4 = UnityEngine.Matrix4x4;
#endif

namespace FindingPath.Data
{
    /// <summary>
    /// 顺时针三角形，顶点索引
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class TriangleMeshNode
    {
        public const uint TileXByteMask = 0xff000000;
        public const byte TileXByteOffset = 24;
        
        public const uint TileYByteMask = 0x00ff0000;
        public const byte TileYByteOffset = 16;
        
        public const uint MeshNodeIndexByteMask = 0x0000ffff;
        public const byte MeshNodeIndexByteOffset = 0;


        private const int NavMesh_VertexCount = 3;

        public uint TileX => GetTileX(NodeIndex);

        public uint TileY => GetTileY(NodeIndex);
        
        public uint MeshNodeIndex => GetMeshNodeIndex(NodeIndex);

        /// <summary>
        /// 由 tileX & tileY & MeshNodeIndex 组成，字节占用从高到低为：1 + 1 + 2 = 4 个字节
        /// </summary>
        [JsonProperty]
        public uint NodeIndex;
        
        [JsonProperty] public int V0;

        [JsonProperty] public int V1;

        [JsonProperty] public int V2;

        /// <summary>
        /// Position of the node in local space.
        /// </summary>
        [JsonProperty] public TSVector Position;

        [JsonProperty] public Connection[] Connections;

        public TriangleMeshNode() : this(0, 0, 0)
        {
            
        }

        public TriangleMeshNode(int v0, int v1, int v2)
        {
            this.V0 = v0;
            this.V1 = v1;
            this.V2 = v2;
        }
        
        public int GetVertexCount()
        {
            return NavMesh_VertexCount;
        }
        
        public int GetVertexIndex (int i) {
            return i == 0 ? V0 : (i == 1 ? V1 : V2);
        }
        
        public static uint ConvertToNodeIndex(uint tileX, uint tileY, uint meshNodeIndex)
        {
            return (uint)(((tileX << TileXByteOffset) & TileXByteMask) | ((tileY << TileYByteOffset) & TileYByteMask) |
                   ((meshNodeIndex << MeshNodeIndexByteOffset) & MeshNodeIndexByteMask));
        }

        public static uint GetMeshNodeIndex(uint nodeIndex)
        {
            return (uint)((nodeIndex & MeshNodeIndexByteMask) >> MeshNodeIndexByteOffset);
        }

        public static uint GetTileX(uint nodeIndex)
        {
            return (uint)((nodeIndex & TileXByteMask) >> TileXByteOffset);
        }
        
        public static uint GetTileY(uint nodeIndex)
        {
            return (uint)((nodeIndex & TileYByteMask) >> TileYByteOffset);
        }

    }


    /// <summary>
    /// Represents a connection to another node
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Connection
    {
        [JsonProperty]
        public uint NodeIndex;
        
        /// <summary>
        /// Cost of moving along this connection.
        /// A cost of 1000 corresponds approximately to the cost of moving one world unit.
        /// </summary>
        [JsonProperty] public uint Cost;

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
        [JsonProperty] public byte ShapeEdge;

        public Connection() : this(0, 0)
        {
            
        }

        public Connection(uint cost, byte shapeEdge)
        {
            Cost = cost;
            ShapeEdge = shapeEdge;
        }
    }
}