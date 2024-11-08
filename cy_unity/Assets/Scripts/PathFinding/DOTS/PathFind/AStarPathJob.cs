using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace PathFinding.DOTS
{
    [BurstCompile]
    public partial struct AStarPathJob : IJob
    {
        [ReadOnly]
        public NativeArray<NavMeshData> AllNavMeshes;
        [ReadOnly]
        public float3 startPosition;
        [ReadOnly]
        public float3 endPosition;
        [ReadOnly]
        public float4x4 localToWorldMatrix;
        [ReadOnly]
        public float4x4 worldToLocalMatrix;
        [WriteOnly]
        public NativeArray<float3> PathResult;
        [WriteOnly]
        public NativeArray<int> PathResultCount;

        /// <summary>
        /// NavMeshData的index 到NavMeshData的map
        /// </summary>
        public NativeParallelHashMap<uint, NavMeshData> currentNavMeshDic;
        
        /// <summary>
        /// TriangleMeshNode的index 到PathNode的map
        /// </summary>
        public NativeParallelHashMap<uint, PathNode> pathNodeHashMap;
        public NativeBinaryHeap binaryHeap;
        
        private uint startPathNodeId;
        private uint endPathNodeId;

        public void Execute()
        {
            Init();

            FindPath();
        }

        private void Init()
        {
            UpdateCurrentNavMeshData();
        }
        
        private void UpdateCurrentNavMeshData()
        {
            foreach (var navMeshTile in AllNavMeshes)
            {
                var navMeshTileIndex = TriangleMeshNodeUtil.ConvertToNodeIndex(navMeshTile.TileX, navMeshTile.TileY, 0);
                currentNavMeshDic.Add(navMeshTileIndex, navMeshTile);
            }
        }

        private void FindPath()
        {
            var startLocal = MultiplyPoint(worldToLocalMatrix, startPosition);
            var endLocal = MultiplyPoint(worldToLocalMatrix, endPosition);

            NativeArray<PathNode> pathResult;
            Prepare(out pathResult, startLocal, endLocal);
            if (!pathResult.IsCreated)
            {
                CalculatePath(out pathResult);
            }
            
            var funnelPortal = new FunnelPortals();
            ConstructFunnelPortals(ref funnelPortal, pathResult);

            CalculateFunnel(ref funnelPortal, out var localPathResult);
            
            for (int i = 0; i < localPathResult.Length; i++)
            {
                PathResult[i] = MultiplyPoint(localToWorldMatrix, localPathResult[i]);
            }

            PathResultCount[0] = localPathResult.Length;
        }

        /// <summary>
        /// 并不是每一个position都在三角形mesh内（可行走、可到达区域），此方法获取最接近position的 node 和 node对应的position
        ///
        /// 已经使用八叉树优化 mesh node节点查询，不要用遍历node
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private float3 GetNearest(float3 position, out TriangleMeshNode node)
        {
            NavMeshData targetNavMesh = new NavMeshData();
            foreach (var currentNavMesh in currentNavMeshDic)
            {
                if (currentNavMesh.Value.RectXZ.Contains(new float2(position.x, position.z)))
                {
                    targetNavMesh = currentNavMesh.Value;
                    break;
                }
            }

            if (targetNavMesh.IgnoreAxisY)
            {
                position.y = 0;
            }

            NativeList<int> meshNodeIndexList = new NativeList<int>(10, Allocator.Temp);
            SpaceOctreeUtil.FindNode(position, targetNavMesh.Octree, ref meshNodeIndexList);

            float minDistance = float.MaxValue;
            float3 minPosition = position;
            TriangleMeshNode minNode = new TriangleMeshNode()
            {
                V0 = -1,
                V1 = -1,
                V2 = -1,
            };
            
            foreach (var meshNodeIndex in meshNodeIndexList)
            {
                var meshNode = targetNavMesh.MeshNodes[meshNodeIndex];
                var v0 = targetNavMesh.Vertexes[meshNode.V0];
                var v1 = targetNavMesh.Vertexes[meshNode.V1];
                var v2 = targetNavMesh.Vertexes[meshNode.V2];

                float3 closestPoint = PolygonMath.ClosestPointOnTriangle(v0, v1, v2, position);
                var distance = math.distancesq(position, closestPoint);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minNode = meshNode;
                    minPosition = closestPoint;
                }
            }

            node = minNode;
            return minPosition;
        }

        private void Prepare(out NativeArray<PathNode> pathResult, float3 start, float3 end)
        {
            startPathNodeId = 0;
            endPathNodeId = 0;
            
            startPosition = GetNearest(start, out var startMeshNode);
            endPosition = GetNearest(end, out var endMeshNode);
            
            var startNode = GetOrCreatePathNode(startMeshNode);
            startNode.IsPointNode = true;
            startNode.Cost = 0;
            startNode.IsInOpenList = true;
            startNode.G = startNode.GetTraversalCost();
            startNode.H = startNode.CalculateHScore(endPosition);
            startPathNodeId = startMeshNode.NodeIndex;
            UpdatePathNode(startNode);
            
            
            var endNode = GetOrCreatePathNode(endMeshNode);
            endNode.IsPointNode = true;
            endNode.IsTargetNode = true;
            endPathNodeId = endMeshNode.NodeIndex;
            UpdatePathNode(endNode);
            
            if (CheckIfStartIsValidTarget())
            {
                TracePath(out pathResult, ref startNode);
                return;
            }
            
            OpenList(ref startNode);
            
            // Any nodes left to search?
            if (binaryHeap.isEmpty)
            {
                pathResult = default;
                Debug.LogError(
                    "The start node either had no neighbours, or no neighbours that the path could traverse");
                return;
            }

            pathResult = default;
        }
        
        
        private void CalculatePath(out NativeArray<PathNode> pathResult)
        {
            // Pop the first node off the open list
            var currentR = binaryHeap.Remove();
            int searchedNodes = 0;
            int counter = 0;
            
            var partialBestTarget = GetPathNode(startPathNodeId);
            
            while (true)
            {
                searchedNodes++;
                
                // Close the current node, if the current node is the target node then the path is finished
                if (currentR.IsTargetNode)
                {
                    break;
                }

                if (currentR.H < partialBestTarget.H)
                {
                    partialBestTarget = currentR;
                }
                
                // Loop through all walkable neighbours of the node and add them to the open list.
                OpenList(ref currentR);

                // Any nodes left to search?
                if (binaryHeap.isEmpty)
                {
                    Debug.LogError(
                        "Searched all reachable nodes, but could not find target. This can happen if you have nodes with a different tag blocking the way to the goal. You can enable path.calculatePartial to handle that case workaround (though this comes with a performance cost).");
                }

                // Select the node with the lowest F score and remove it from the open list
                currentR = binaryHeap.Remove();

                // Check for time every 500 nodes, roughly every 0.5 ms usually
                if (counter > 500)
                {
                    counter = 0;

                    // Mostly for development
                    if (searchedNodes > 1000000)
                    {
                        throw new System.Exception("Probable infinite loop. Over 1,000,000 nodes searched");
                    }
                }

                counter++;
            }

            TracePath(out pathResult, ref currentR);
        }
        
        private void TracePath(out NativeArray<PathNode> pathResult, ref PathNode from)
        {
            // Current node we are processing
            uint c = from.MeshNode.NodeIndex;
            int count = 0;

            while (c != uint.MaxValue)
            {
                var curPathNode = GetPathNode(c);
                c = curPathNode.Parent;
                count++;
                if (count > 2048)
                {
                    pathResult = default;
                    Debug.LogWarning(
                        "Infinite loop? >2048 node path. Remove this message if you really have that long paths (Path.cs, Trace method)");
                    
                    return;
                }
            }

            pathResult = new NativeArray<PathNode>(count, Allocator.Temp);

            c = from.MeshNode.NodeIndex;
            for (int i = 0; i < count; i++)
            {
                var curPathNode = GetPathNode(c);
                pathResult[i] = curPathNode;
                c = curPathNode.Parent;
            }

            // Reverse
            int half = count / 2;
            for (int i = 0; i < half; i++)
            {
                var tmp = pathResult[i];
                pathResult[i] = pathResult[count - i - 1];
                pathResult[count - i - 1] = tmp;
            }
        }
        
        private void OpenList(ref PathNode pathNode)
        {
            if (pathNode.MeshNode.NodeIndex == uint.MaxValue)
            {
                return;
            }
            
            if (pathNode.MeshNode.Connections.IsEmpty)
            {
                return;
            }

            bool isPointNode = pathNode.IsPointNode;
            var connections = pathNode.MeshNode.Connections;
            
            // Loop through all connections
            for (int i = connections.Length - 1; i >= 0; i--)
            {
                var conn = connections[i];
                var other = GetNavMeshNode(conn.NodeIndex);
                PathNode pathOther = GetOrCreatePathNode(other);
                if (!pathOther.CanTraverse())
                {
                    continue;
                }

                if (pathOther.MeshNode.NodeIndex == pathNode.Parent) {
                    continue;
                }
                
                uint cost = conn.Cost;
                if (isPointNode || pathOther.IsPointNode)
                {
                    // Get special connection cost from the path
                    // This is used by the start and end nodes
                    var aMeshNode = pathNode.MeshNode;
                    var bMeshNode = pathOther.MeshNode;
                    cost = GetConnectionSpecialCost(ref aMeshNode, ref bMeshNode, cost);
                }

                if (!pathOther.IsInOpenList)
                {
                    pathOther.Parent = pathNode.MeshNode.NodeIndex;
                    pathOther.IsInOpenList = true;

                    pathOther.Cost = cost;

                    pathOther.H = pathOther.CalculateHScore(endPosition);

                    var parentPathNode = GetPathNode(pathOther.Parent);
                    pathOther.UpdateG(parentPathNode);

                    binaryHeap.Add(pathOther);
                }
                else
                {
                    // If not we can test if the path from this node to the other one is a better one than the one already used
                    if (pathNode.G + cost + pathNode.GetTraversalCost() < pathOther.G)
                    {
                        pathOther.Cost = cost;
                        pathOther.Parent = pathNode.MeshNode.NodeIndex;
                        UpdateRecursiveG(ref pathOther);
                    }
                }
                
                UpdatePathNode(pathOther);
            }
        }

        private void UpdateRecursiveG(ref PathNode pathNode)
        {
            var parentPathNode = GetPathNode(pathNode.Parent);
            pathNode.UpdateG(parentPathNode);

            binaryHeap.Add(pathNode);

            if (pathNode.MeshNode.Connections.IsEmpty)
            {
                return;
            }

            foreach (var con in pathNode.MeshNode.Connections)
            {
                var otherNodeIndex = con.NodeIndex;
                var other = GetNavMeshNode(otherNodeIndex);
                PathNode otherPN = GetOrCreatePathNode(other);

                if (otherPN.Parent == pathNode.MeshNode.NodeIndex && otherPN.IsInOpenList)
                {
                    UpdateRecursiveG(ref otherPN);
                }
            }
        }

        private uint GetConnectionSpecialCost(ref TriangleMeshNode a, ref TriangleMeshNode b, uint currentCost)
        {
            float newCost = -1;
            if (startPathNodeId != 0 && endPathNodeId != 0)
            {
                var startNode = GetPathNode(startPathNodeId);
                var endNode = GetPathNode(endPathNodeId);
                
                if (a.NodeIndex == startNode.MeshNode.NodeIndex)
                {
                    newCost = math.distance(startPosition, (b.NodeIndex == endNode.MeshNode.NodeIndex ? endPosition : b.Position)) *
                              (currentCost * 1.0f / math.distance(a.Position, b.Position));
                }

                if (b.NodeIndex == startNode.MeshNode.NodeIndex)
                {
                    newCost = math.distance(startPosition, (a.NodeIndex == endNode.MeshNode.NodeIndex ? endPosition : a.Position)) *
                               (currentCost * 1.0f / math.distance(a.Position, b.Position));
                }

                if (a.NodeIndex == endNode.MeshNode.NodeIndex)
                {
                    newCost = (math.distance(endPosition, b.Position) *
                               (currentCost * 1.0f / math.distance(a.Position, b.Position)));
                }

                if (b.NodeIndex == endNode.MeshNode.NodeIndex)
                {
                    newCost = (math.distance(endPosition, a.Position) *
                               (currentCost * 1.0f / math.distance(a.Position, b.Position)));
                }
            }
            else
            {
                // endNode is null, startNode should never be null for an ABPath
                if (a.NodeIndex == startPathNodeId)
                {
                    newCost = (math.distance(startPosition, b.Position) *
                               (currentCost * 1.0f / math.distance(a.Position, b.Position)));
                }

                if (b.NodeIndex == startPathNodeId)
                {
                    newCost = (math.distance(startPosition, a.Position) *
                               (currentCost * 1.0f / math.distance(a.Position, b.Position)));
                }
            }

            if (newCost > 0)
            {
                return (uint) (newCost);
            }

            return currentCost;
        }

        private bool CheckIfStartIsValidTarget()
        {
            return pathNodeHashMap[startPathNodeId].IsTargetNode;
        }
        
        private TriangleMeshNode GetNavMeshNode(uint nodeIndex)
        {
            uint tileX = TriangleMeshNodeUtil.GetTileX(nodeIndex);
            uint tileY = TriangleMeshNodeUtil.GetTileY(nodeIndex);
            uint meshNodeIndex = TriangleMeshNodeUtil.GetMeshNodeIndex(nodeIndex);
            
            var currentTileIndex = TriangleMeshNodeUtil.ConvertToNodeIndex(tileX, tileY, 0);
            if (!currentNavMeshDic.TryGetValue(currentTileIndex, out var targetNavTile))
            {
                Debug.LogError($"不能发现对应的 nav mesh tile");
                return default;
            }

            if (!targetNavTile.MeshNodes.IsCreated)
            {
                Debug.LogError($"不能发现对应的 targetNavTile.MeshNodes");
                return default;
            }

            return targetNavTile.MeshNodes[(int) meshNodeIndex];

        }
        
        private PathNode GetOrCreatePathNode(TriangleMeshNode meshNode)
        {
            if (!pathNodeHashMap.TryGetValue(meshNode.NodeIndex, out var pathNode))
            {
                pathNode = new PathNode()
                {
                    MeshNode = meshNode,
                    HeapIndex = NativeBinaryHeap.NotInHeap,
                    Parent = uint.MaxValue,
                };
                
                pathNodeHashMap.Add(meshNode.NodeIndex, pathNode);
            }
            
            return pathNode;
        }
        
        private PathNode GetPathNode(uint meshNodeIndex)
        {
            if (!pathNodeHashMap.TryGetValue(meshNodeIndex, out var pathNode))
            {
                return default;
            }
            
            return pathNode;
        }

        private void UpdatePathNode(PathNode newPathNode)
        {
            if (pathNodeHashMap.ContainsKey(newPathNode.MeshNode.MeshNodeIndex))
            {
                pathNodeHashMap[newPathNode.MeshNode.MeshNodeIndex] = newPathNode;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeIndex">NodeIndex</param>
        /// <param name="vertexIndex">三角形的顶点索引：0、1、2</param>
        /// <returns></returns>
        private float3 GetTriangleMeshNodeVertex(uint nodeIndex, int vertexIndex)
        {
            uint tileX = TriangleMeshNodeUtil.GetTileX(nodeIndex);
            uint tileY = TriangleMeshNodeUtil.GetTileY(nodeIndex);
            uint meshNodeIndex = TriangleMeshNodeUtil.GetMeshNodeIndex(nodeIndex);
            
            var currentTileIndex = TriangleMeshNodeUtil.ConvertToNodeIndex(tileX, tileY, 0);
            if (!currentNavMeshDic.TryGetValue(currentTileIndex, out var targetNavTile))
            {
                Debug.LogError($"不能发现对应的 nav mesh tile");
                return new float3(float.MaxValue,float.MaxValue,float.MaxValue);
            }

            if (targetNavTile.MeshNodes.IsEmpty)
            {
                Debug.LogError($"不能发现对应的 targetNavTile.MeshNodes");
                return new float3(float.MaxValue,float.MaxValue,float.MaxValue);
            }
            
            var meshNode = targetNavTile.MeshNodes[(int) meshNodeIndex];
            var vertexArrayIndex =meshNode.GetVertexIndex(vertexIndex);
            return targetNavTile.Vertexes[vertexArrayIndex];
        }
        
        /// <summary>
        ///   <para>Transforms a position by this matrix (generic).</para>
        /// </summary>
        /// <param name="point"></param>
        private float3 MultiplyPoint(float4x4 matrix, float3 point)
        {
            float3 vector3;
            vector3.x = (float) ((double) matrix.c0.x * (double) point.x + (double) matrix.c1.x * (double) point.y + (double) matrix.c2.x * (double) point.z) + matrix.c3.x;
            vector3.y = (float) ((double) matrix.c0.y * (double) point.x + (double) matrix.c1.y * (double) point.y + (double) matrix.c2.y * (double) point.z) + matrix.c3.y;
            vector3.z = (float) ((double) matrix.c0.z * (double) point.x + (double) matrix.c1.z * (double) point.y + (double) matrix.c2.z * (double) point.z) + matrix.c3.z;
            float num = 1f / ((float) ((double) matrix.c0.w * (double) point.x + (double) matrix.c1.w * (double) point.y + (double) matrix.c2.w * (double) point.z) + matrix.c3.w);
            vector3.x *= num;
            vector3.y *= num;
            vector3.z *= num;
            return vector3;
        }
        
    }
}