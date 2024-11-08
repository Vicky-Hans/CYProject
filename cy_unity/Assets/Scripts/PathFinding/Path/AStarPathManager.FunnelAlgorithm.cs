using System.Collections.Generic;
using FindingPath.Data;
using UnityEngine;
#if USE_FP
    using DH.LockStep.Framework;
#else
    using FP = System.Single;
    using TSVector2 = UnityEngine.Vector2;
    using TSVector = UnityEngine.Vector3;
    using TSRect = UnityEngine.Rect;
    using TSMatrix4x4 = UnityEngine.Matrix4x4;
#endif

namespace FindingPath.Path
{
    /// <summary>
    /// 来自于此算法：http://digestingduck.blogspot.com/2010/03/simple-stupid-funnel-algorithm.html
    /// </summary>
    public partial class AStarPathManager
    {

        /// <summary>Funnel in which the path to the target will be</summary>
        private struct FunnelPortals
        {
            public List<TSVector> left;
            public List<TSVector> right;

            /// <summary>
            /// 转换到xz平面
            /// </summary>
            /// <param name="leftXZ"></param>
            /// <param name="rightXZ"></param>
            public void ToXZ(out List<TSVector2> leftXZ, out List<TSVector2> rightXZ)
            {
                leftXZ = new List<TSVector2>(left.Count);
                rightXZ = new List<TSVector2>(right.Count);

                for (int i = 0; i < left.Count; i++)
                {
                    TSVector vectorXYZ = left[i];
                    leftXZ.Add(AStarPathManager.ToXZ(vectorXYZ));
                }
                
                for (int i = 0; i < right.Count; i++)
                {
                    TSVector vectorXYZ = right[i];
                    rightXZ.Add(AStarPathManager.ToXZ(vectorXYZ));
                }
            }
        }

        private FunnelPortals funnelPortal = new FunnelPortals();
        
        private void ConstructFunnelPortals(List<PathNode> pathResult)
        {
            funnelPortal.left = new List<TSVector>();
            funnelPortal.right = new List<TSVector>();
            
            funnelPortal.left.Add(startPosition);
            funnelPortal.right.Add(startPosition);

            for (int i = 0; i < pathResult.Count - 1; i++)
            {
                var pathNode = pathResult[i];
                var pathNextNode = pathResult[i + 1];
                bool result = GetPortal(pathNode, pathNextNode, funnelPortal.left, funnelPortal.right);
                
            }
            
            funnelPortal.left.Add(endPosition);
            funnelPortal.right.Add(endPosition);
        }

        private void CalculateFunnel(out TSVector[] path)
        {
            funnelPortal.ToXZ(out var leftXZ, out var rightXZ);

            int startIndex = 0;
            int apexIndex = startIndex + 0;
            int rightIndex = startIndex + 1;
            int leftIndex = startIndex + 1;
            List<int> funnelPathIndex = new List<int>(leftXZ.Count);

            var portalApex = leftXZ[apexIndex];
            var portalLeft = leftXZ[leftIndex];
            var portalRight = rightXZ[rightIndex];

            int numPortals = leftXZ.Count;

            funnelPathIndex.Add(apexIndex);

            for (int i = startIndex + 2; i < numPortals; i++)
            {

                if (funnelPathIndex.Count > 2000)
                {
                    Debug.LogWarning("Avoiding infinite loop. Remove this check if you have this long paths.");
                    break;
                }

                var pLeft = leftXZ[i];
                var pRight = rightXZ[i];

                if (LeftOrColinear(portalRight - portalApex, pRight - portalApex))
                {
                    if (portalApex == portalRight || RightOrColinear(portalLeft - portalApex, pRight - portalApex))
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
                    if (portalApex == portalLeft || LeftOrColinear(portalRight - portalApex, pLeft - portalApex))
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
            path = new TSVector[funnelPathIndex.Count];
            for (int i = 0; i < funnelPathIndex.Count; i++)
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

        }

        public byte FindSharedEdge(PathNode fromNode, PathNode toNode)
        {
            byte edgeIndex = 0xFF;
            
            for (int i = 0; i < fromNode.MeshNode.Connections.Length; i++)
            {
                var conn = fromNode.MeshNode.Connections[i];
                var connMeshNode = GetNavMeshNode(conn.NodeIndex);
                if (connMeshNode == toNode.MeshNode)
                {
                    edgeIndex = conn.ShapeEdge;
                }
            }

            return edgeIndex;
        }
        
        public byte FindSharedEdge(TriangleMeshNode fromNode, TriangleMeshNode toNode)
        {
            byte edgeIndex = 0xFF;
            
            for (int i = 0; i < fromNode.Connections.Length; i++)
            {
                var conn = fromNode.Connections[i];
                var connMeshNode = GetNavMeshNode(conn.NodeIndex);
                if (connMeshNode == toNode)
                {
                    edgeIndex = conn.ShapeEdge;
                }
            }

            return edgeIndex;
        }

        public bool GetPortal(PathNode fromNode, PathNode toNode, List<TSVector> left, List<TSVector> right)
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
            
            TSVector vertexA = GetTriangleMeshNodeVertex(fromNode.MeshNode.NodeIndex, vertexIndexA);
            TSVector vertexB = GetTriangleMeshNodeVertex(fromNode.MeshNode.NodeIndex, vertexIndexB);
            
            left.Add(vertexA);
            right.Add(vertexB);
            
            return true;
        }
        
        
        private static TSVector2 ToXZ (TSVector p) {
            return new TSVector2(p.x, p.z);
        }

        private static TSVector FromXZ (TSVector2 p) {
            return new TSVector(p.x, 0, p.y);
        }
        
        /// <summary>True if b is to the right of or on the line from (0,0) to a</summary>
        private static bool RightOrColinear (TSVector2 a, TSVector2 b) {
            return (a.x*b.y - b.x*a.y) <= 0;
        }

        /// <summary>True if b is to the left of or on the line from (0,0) to a</summary>
        private static bool LeftOrColinear (TSVector2 a, TSVector2 b) {
            return (a.x*b.y - b.x*a.y) >= 0;
        }
        
    }
}