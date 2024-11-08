using Unity.Collections;
using Unity.Mathematics;

namespace PathFinding.DOTS
{
    public static class TriangleMeshNodeUtil
    {
        public const uint TileXByteMask = 0xff000000;
        public const byte TileXByteOffset = 24;
        
        public const uint TileYByteMask = 0x00ff0000;
        public const byte TileYByteOffset = 16;
        
        public const uint MeshNodeIndexByteMask = 0x0000ffff;
        public const byte MeshNodeIndexByteOffset = 0;


        private const int NavMesh_VertexCount = 3;
        
        public static int GetVertexCount(this TriangleMeshNode meshNode)
        {
            return NavMesh_VertexCount;
        }
        
        public static int GetVertexIndex (this TriangleMeshNode meshNode, int i) {
            return i == 0 ? meshNode.V0 : (i == 1 ? meshNode.V1 : meshNode.V2);
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
    
    public static class SpaceOctreeUtil
    {
        /// <summary>
        /// 根据空间划分，查找对应部分的TriangleMeshNode索引，开始遍历查找
        /// </summary>
        /// <param name="position"></param>
        /// <param name="octree"></param>
        /// <param name="meshNodes"></param>
        public static void FindNode(float3 position, SpaceOctree octree, ref NativeList<int> meshNodes)
        {
            if (octree.Box.Contains(position))
            {
                FindNodeInternal(position, octree, ref meshNodes);
            }

            if (meshNodes.Length < 1)
            {
                //如果坐标点在包围盒之外，按照距离包围盒的距离进行查找
                FindNodeWithDistanceInternal(position, octree, ref meshNodes);
            }
        }

        private static void FindNodeWithDistanceInternal(float3 position, SpaceOctree octreeNode, ref NativeList<int> meshNodes)
        {
            if (octreeNode.ChildTree.IsEmpty)
            {
                foreach (var meshNode in octreeNode.Nodes)
                {
                    meshNodes.Add(meshNode);
                }
                return;
            }

            float miniDistance = float.MaxValue;
            SpaceOctree minDistanceTree = default;
            foreach (var child in octreeNode.ChildTree)
            {
                var distance = child.Box.PointClosestSqrDistance(position);
                if (distance < miniDistance)
                {
                    miniDistance = distance;
                    minDistanceTree = child;
                }
            }

            FindNodeWithDistanceInternal(position, minDistanceTree, ref meshNodes);
        }

        private static bool FindNodeInternal(float3 position, SpaceOctree octreeNode, ref NativeList<int> meshNodes)
        {
            var octreeNodeBox = octreeNode.Box;
            if (! octreeNodeBox.Contains(position))
            {
                return false;
            }

            if (octreeNode.ChildTree.IsEmpty)
            {
                foreach (var node in octreeNode.Nodes)
                {
                    meshNodes.Add(node);
                }
                return true;
            }
            
            foreach (var child in octreeNode.ChildTree)
            {
                if (FindNodeInternal(position, child, ref meshNodes))
                {
                    break;
                }
            }

            return true;
        }
        
    }
}