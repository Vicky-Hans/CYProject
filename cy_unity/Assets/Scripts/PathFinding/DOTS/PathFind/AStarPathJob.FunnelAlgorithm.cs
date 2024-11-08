using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace PathFinding.DOTS
{
    public partial struct AStarPathJob
    {
        private void ConstructFunnelPortals(ref FunnelPortals funnelPortal, NativeArray<PathNode> pathResult)
        {
            var left = new NativeList<float3>(10, Allocator.Temp);
            var right = new NativeList<float3>(10, Allocator.Temp);
            
            left.Add(startPosition);
            right.Add(startPosition);

            for (int i = 0; i < pathResult.Length - 1; i++)
            {
                var pathNode = pathResult[i];
                var pathNextNode = pathResult[i + 1];
                bool result = GetPortal(pathNode, pathNextNode, left, right);
                
            }
            
            left.Add(endPosition);
            right.Add(endPosition);

            funnelPortal.left = left;
            funnelPortal.right = right;
        }

        private void CalculateFunnel(ref FunnelPortals funnelPortal, out NativeArray<float3> path)
        {
            funnelPortal.ToXZ(out var leftXZ, out var rightXZ);

            int startIndex = 0;
            int apexIndex = startIndex + 0;
            int rightIndex = startIndex + 1;
            int leftIndex = startIndex + 1;
            NativeList<int> funnelPathIndex = new NativeList<int>(leftXZ.Length, Allocator.Temp);

            var portalApex = leftXZ[apexIndex];
            var portalLeft = leftXZ[leftIndex];
            var portalRight = rightXZ[rightIndex];

            int numPortals = leftXZ.Length;

            funnelPathIndex.Add(apexIndex);

            for (int i = startIndex + 2; i < numPortals; i++)
            {

                if (funnelPathIndex.Length > 2000)
                {
                    Debug.LogWarning("Avoiding infinite loop. Remove this check if you have this long paths.");
                    break;
                }

                var pLeft = leftXZ[i];
                var pRight = rightXZ[i];

                if (LeftOrColinear(portalRight - portalApex, pRight - portalApex))
                {
                    if (portalApex.Equals(portalRight) || RightOrColinear(portalLeft - portalApex, pRight - portalApex))
                    {
                        portalRight = pRight;
                        rightIndex = i;
                    }
                    else
                    {
                        portalApex = portalRight = portalLeft;
                        i = apexIndex = rightIndex = leftIndex;

                        funnelPathIndex.Add(apexIndex);
                        continue;
                    }
                }

                if (RightOrColinear(portalLeft - portalApex, pLeft - portalApex))
                {
                    if (portalApex.Equals(portalLeft) || LeftOrColinear(portalRight - portalApex, pLeft - portalApex))
                    {
                        portalLeft = pLeft;
                        leftIndex = i;
                    }
                    else
                    {
                        portalApex = portalLeft = portalRight;
                        i = apexIndex = leftIndex = rightIndex;

                        // Negative value because we are referring
                        // to the right side
                        funnelPathIndex.Add(-apexIndex);

                        continue;
                    }
                }
            }

            funnelPathIndex.Add(numPortals - 1);

            // 转换为转角点
            path = new NativeArray<float3>(funnelPathIndex.Length, Allocator.Temp);
            for (int i = 0; i < funnelPathIndex.Length; i++)
            {
                var funnelIndex = funnelPathIndex[i];
                if (funnelIndex >= 0)
                {
                    var pathCorner = funnelPortal.left[funnelIndex];
                    path[i] = pathCorner;
                }
                else
                {
                    var pathCorner = funnelPortal.right[-funnelIndex];
                    path[i] = pathCorner;
                }
            }


            funnelPathIndex.Dispose();
        }

        private byte FindSharedEdge(PathNode fromNode, PathNode toNode)
        {
            byte edgeIndex = 0xFF;
            
            for (int i = 0; i < fromNode.MeshNode.Connections.Length; i++)
            {
                var conn = fromNode.MeshNode.Connections[i];
                var connMeshNode = GetNavMeshNode(conn.NodeIndex);
                if (connMeshNode.NodeIndex == toNode.MeshNode.NodeIndex)
                {
                    edgeIndex = conn.ShapeEdge;
                }
            }

            return edgeIndex;
        }

        private bool GetPortal(PathNode fromNode, PathNode toNode, NativeList<float3> left, NativeList<float3> right)
        {
            byte edgeIndex = FindSharedEdge(fromNode, toNode);
            if (edgeIndex == 0xFF)
            {
                //无连接三角形，此处需要判断是否在其他tile 或 真的不可行走
                return true;
            }

            //nav mesh 三角形的局部顶点索引0，1，2
            int vertexIndexA = edgeIndex;
            int vertexIndexB = (edgeIndex + 1) % fromNode.MeshNode.GetVertexCount();
            
            float3 vertexA = GetTriangleMeshNodeVertex(fromNode.MeshNode.NodeIndex, vertexIndexA);
            float3 vertexB = GetTriangleMeshNodeVertex(fromNode.MeshNode.NodeIndex, vertexIndexB);
            
            left.Add(vertexA);
            right.Add(vertexB);
            
            return true;
        }
        
        /// <summary>True if b is to the right of or on the line from (0,0) to a</summary>
        private static bool RightOrColinear (float2 a, float2 b) {
            return (a.x*b.y - b.x*a.y) <= 0;
        }

        /// <summary>True if b is to the left of or on the line from (0,0) to a</summary>
        private static bool LeftOrColinear (float2 a, float2 b) {
            return (a.x*b.y - b.x*a.y) >= 0;
        }
        
        
    }
}