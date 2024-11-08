using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FindingPath.Core;
using FindingPath.Data;
using PathFinding.DOTS;
using Unity.Profiling;
using PolygonMath = FindingPath.Core.PolygonMath;
using SpaceOctree = FindingPath.Data.SpaceOctree;
using TriangleMeshNode = FindingPath.Data.TriangleMeshNode;
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

namespace FindingPath.Path
{
    public partial class AStarPathManager
    {
        public static readonly ProfilerMarker AStartFindPathMarker = new ProfilerMarker("AStarFindPath");
        
        private List<NavMesh> allNavMeshTiles;

        private readonly BinaryHeap binaryHeap = new BinaryHeap(20);

        private TSMatrix4x4 localToWorldMatrix;
        private TSMatrix4x4 worldToLocalMatrix;
        
        private TSVector startPosition;
        private PathNode startNode;

        private TSVector endPosition;
        private PathNode endNode;

        private readonly Dictionary<uint, NavMesh> currentNavMeshDic = new Dictionary<uint, NavMesh>(9);
        private readonly PathNodeSet pathNodeSet = new PathNodeSet();

        private readonly List<PathNode> pathResult = new List<PathNode>();
        private List<TSVector> vectorPathResult = new List<TSVector>();
        private TSVector[] vectorPathResultArray;

        public NavMesh DefaultNavMesh => allNavMeshTiles.Count > 0 ? allNavMeshTiles[0] : null;
        
        public AStarPathManager()
        {
            allNavMeshTiles = new List<NavMesh>();
        }

        public void Init(TSMatrix4x4 localToWorldMatrix, TSMatrix4x4 worldToLocalMatrix)
        {
            this.localToWorldMatrix = localToWorldMatrix;
            this.worldToLocalMatrix = worldToLocalMatrix;
        }

        public void UpdateNavMesh(List<NavMesh> navMeshes)
        {
            allNavMeshTiles.Clear();
            allNavMeshTiles.AddRange(navMeshes);
            UpdateCurrentNavMeshData();
            
            AStarPathSystem.Instance.Init(navMeshes, localToWorldMatrix, worldToLocalMatrix);
        }

        public void Release()
        {
            
        }

        public TSVector GetNearestPoint(TSVector position)
        {
            var start = worldToLocalMatrix.MultiplyPoint(position);
            GetNearest(start, out var startMeshNode);
            return localToWorldMatrix.MultiplyPoint(start);
        }

        /// <summary>
        /// 参数都是世界坐标
        /// </summary>
        /// <param name="start">世界坐标</param>
        /// <param name="end">世界坐标</param>
        /// <param name="path">返回世界坐标</param>
        /// <returns></returns>
        public bool FindPath(TSVector start, TSVector end, List<TSVector> path)
        {
            AStartFindPathMarker.Begin();
            start = worldToLocalMatrix.MultiplyPoint(start);
            end = worldToLocalMatrix.MultiplyPoint(end);
            
            Prepare(start, end);
            if (pathResult.Count < 1)
            {
                CalculatePath();
            }
            
            ConstructFunnelPortals(pathResult);
            CalculateFunnel(out vectorPathResultArray);   
            
            vectorPathResult.Clear();
            foreach (var item in vectorPathResultArray)
            {
                vectorPathResult.Add(localToWorldMatrix.MultiplyPoint(item));
            }

            path.Clear();
            path.AddRange(vectorPathResult);
            
            pathNodeSet.Clear();
            pathResult.Clear();
            binaryHeap.Clear();
            AStartFindPathMarker.End();
            return true;
        }

        private TriangleMeshNode GetNavMeshNode(uint nodeIndex)
        {
            uint tileX = TriangleMeshNode.GetTileX(nodeIndex);
            uint tileY = TriangleMeshNode.GetTileY(nodeIndex);
            uint meshNodeIndex = TriangleMeshNode.GetMeshNodeIndex(nodeIndex);
            
            var currentTileIndex = TriangleMeshNode.ConvertToNodeIndex(tileX, tileY, 0);
            if (!currentNavMeshDic.TryGetValue(currentTileIndex, out var targetNavTile))
            {
                DHLogger.LogError($"不能发现对应的 nav mesh tile");
                return null;
            }

            if (targetNavTile.MeshNodes == null)
            {
                DHLogger.LogError($"不能发现对应的 targetNavTile.MeshNodes");
                return null;
            }

            return targetNavTile.MeshNodes[meshNodeIndex];

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeIndex">NodeIndex</param>
        /// <param name="vertexIndex">三角形的顶点索引：0、1、2</param>
        /// <returns></returns>
        private TSVector GetTriangleMeshNodeVertex(uint nodeIndex, int vertexIndex)
        {
            uint tileX = TriangleMeshNode.GetTileX(nodeIndex);
            uint tileY = TriangleMeshNode.GetTileY(nodeIndex);
            uint meshNodeIndex = TriangleMeshNode.GetMeshNodeIndex(nodeIndex);
            
            var currentTileIndex = TriangleMeshNode.ConvertToNodeIndex(tileX, tileY, 0);
            if (!currentNavMeshDic.TryGetValue(currentTileIndex, out var targetNavTile))
            {
                DHLogger.LogError($"不能发现对应的 nav mesh tile");
                return new TSVector(float.MaxValue,float.MaxValue,float.MaxValue);
            }

            if (targetNavTile.MeshNodes == null)
            {
                DHLogger.LogError($"不能发现对应的 targetNavTile.MeshNodes");
                return new TSVector(float.MaxValue,float.MaxValue,float.MaxValue);
            }
            
            var meshNode = targetNavTile.MeshNodes[meshNodeIndex];
            var vertexArrayIndex =meshNode.GetVertexIndex(vertexIndex);
            return targetNavTile.Vertexes[vertexArrayIndex];
        }

        private void UpdateCurrentNavMeshData()
        {
            foreach (var navMeshTile in allNavMeshTiles)
            {
                var navMeshTileIndex = TriangleMeshNode.ConvertToNodeIndex(navMeshTile.TileX, navMeshTile.TileY, 0);
                currentNavMeshDic.Add(navMeshTileIndex, navMeshTile);
            }
        }

        /// <summary>
        /// 并不是每一个position都在三角形mesh内（可行走、可到达区域），此方法获取最接近position的 node 和 node对应的position
        ///
        /// 已经使用八叉树优化 mesh node节点查询，不要用遍历node
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private TSVector GetNearest(TSVector position, out TriangleMeshNode node)
        {
            NavMesh targetNavMesh = null;
            foreach (var currentNavMesh in currentNavMeshDic.Values)
            {
                if (currentNavMesh.RectXZ.Contains(new TSVector2(position.x, position.z)))
                {
                    targetNavMesh = currentNavMesh;
                    break;
                }
            }

            if (targetNavMesh.IgnoreAxisY)
            {
                position.y = 0;
            }

            List<int> meshNodeIndexList = new List<int>();
            SpaceOctree.FindNode(position, targetNavMesh.Octree, ref meshNodeIndexList);
            
            FP minDistance = FP.MaxValue;
            TriangleMeshNode minNode = null;
            TSVector minPosition = position;
            foreach (var meshNodeIndex in meshNodeIndexList)
            {
                var meshNode = targetNavMesh.MeshNodes[meshNodeIndex];
                
            // foreach (var meshNode in targetNavMesh.MeshNodes)
            // {
                var v0 = targetNavMesh.Vertexes[meshNode.V0];
                var v1 = targetNavMesh.Vertexes[meshNode.V1];
                var v2 = targetNavMesh.Vertexes[meshNode.V2];

                TSVector closestPoint = PolygonMath.ClosestPointOnTriangle(v0, v1, v2, position);
                var distance = (position - closestPoint).sqrMagnitude;
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

        private void Prepare(TSVector start, TSVector end)
        {
            startNode = null;
            endNode = null;

            startPosition = GetNearest(start, out var startMeshNode);
            endPosition = GetNearest(end, out var endMeshNode);

            startNode =  pathNodeSet.GetPathNode(startMeshNode);
            startNode.IsPointNode = true;
            startNode.Cost = 0;
            startNode.Parent = null;
            startNode.IsInOpenList = true;
            startNode.G = startNode.GetTraversalCost();
            startNode.H = startNode.CalculateHScore(endPosition);


            endNode = pathNodeSet.GetPathNode(endMeshNode);
            endNode.IsPointNode = true;
            endNode.IsTargetNode = true;

            if (CheckIfStartIsValidTarget())
            {
                TracePath(startNode);
                return;
            }
            
            OpenList(startNode);
            
            // Any nodes left to search?
            if (binaryHeap.isEmpty)
            {
                throw new ArgumentException(
                    "The start node either had no neighbours, or no neighbours that the path could traverse");
                return;
            }
            
        }

        private bool CheckIfStartIsValidTarget()
        {
            return startNode.IsTargetNode;
        }

        private void CalculatePath()
        {
            // Pop the first node off the open list
            var currentR = binaryHeap.Remove();
            int searchedNodes = 0;
            int counter = 0;
            
            var partialBestTarget = startNode;
            
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
                OpenList(currentR);

                // Any nodes left to search?
                if (binaryHeap.isEmpty)
                {
                    throw new ArgumentException(
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

            
            TracePath(currentR);
        }

        private void TracePath(PathNode from)
        {
            pathResult.Clear();
            
            // Current node we are processing
            PathNode c = from;
            int count = 0;

            while (c != null)
            {
                c = c.Parent;
                count++;
                if (count > 2048)
                {
                    DHLogger.LogWarning(
                        "Infinite loop? >2048 node path. Remove this message if you really have that long paths (Path.cs, Trace method)");
                    
                    return;
                }
            }

            if (pathResult.Capacity < count)
            {
                pathResult.Capacity = count;
            }

            c = from;

            for (int i = 0; i < count; i++)
            {
                pathResult.Add(c);
                c = c.Parent;
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

        private void OpenList(PathNode pathNode)
        {
            if (pathNode == null)
            {
                return;
            }

            if (pathNode.MeshNode.Connections == null)
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
                PathNode pathOther = pathNodeSet.GetPathNode(other);
                if (!pathOther.CanTraverse())
                {
                    continue;
                }

                if (pathOther == pathNode.Parent) {
                    continue;
                }
                
                uint cost = conn.Cost;
                if (isPointNode || pathOther.IsPointNode)
                {
                    // Get special connection cost from the path
                    // This is used by the start and end nodes
                    cost = GetConnectionSpecialCost(pathNode.MeshNode, pathOther.MeshNode, cost);
                }

                if (!pathOther.IsInOpenList)
                {
                    pathOther.Parent = pathNode;
                    pathOther.IsInOpenList = true;

                    pathOther.Cost = cost;

                    pathOther.H = pathOther.CalculateHScore(endPosition);
                    pathOther.UpdateG();

                    binaryHeap.Add(pathOther);
                }
                else
                {
                    // If not we can test if the path from this node to the other one is a better one than the one already used
                    if (pathNode.G + cost + pathNode.GetTraversalCost() < pathOther.G)
                    {
                        pathOther.Cost = cost;
                        pathOther.Parent = pathNode;

                        UpdateRecursiveG(pathOther);
                    }
                }
                

            }
        }

        private void UpdateRecursiveG(PathNode pathNode)
        {
            pathNode.UpdateG();

            binaryHeap.Add(pathNode);

            if (pathNode.MeshNode.Connections == null)
            {
                return;
            }

            foreach (var con in pathNode.MeshNode.Connections)
            {
                var otherNodeIndex = con.NodeIndex;
                var other = GetNavMeshNode(otherNodeIndex);
                PathNode otherPN = pathNodeSet.GetPathNode(other);

                if (otherPN.Parent == pathNode && otherPN.IsInOpenList)
                {
                    UpdateRecursiveG(otherPN);
                }
            }
        }

        private uint GetConnectionSpecialCost(TriangleMeshNode a, TriangleMeshNode b, uint currentCost)
        {
            FP newCost = -1;
            if (startNode != null && endNode != null) {
                if (a == startNode.MeshNode) {
                    newCost = (startPosition - (b == endNode.MeshNode ? endPosition : b.Position)).magnitude * (currentCost*1.0f/(a.Position - b.Position).magnitude);
                }
                if (b == startNode.MeshNode) {
                    newCost = ((startPosition - (a == endNode.MeshNode ? endPosition : a.Position)).magnitude * (currentCost*1.0f/(a.Position-b.Position).magnitude));
                }
                if (a == endNode.MeshNode) {
                    newCost = ((endPosition - b.Position).magnitude * (currentCost*1.0f/(a.Position-b.Position).magnitude));
                }
                if (b == endNode.MeshNode) {
                    newCost = ((endPosition - a.Position).magnitude * (currentCost*1.0f/(a.Position-b.Position).magnitude));
                }
            } else {
                // endNode is null, startNode should never be null for an ABPath
                if (a == startNode.MeshNode) {
                    newCost = ((startPosition - b.Position).magnitude * (currentCost*1.0f/(a.Position-b.Position).magnitude));
                }
                if (b == startNode.MeshNode) {
                    newCost = ((startPosition - a.Position).magnitude * (currentCost*1.0f/(a.Position-b.Position).magnitude));
                }
            }

            if (newCost > 0)
            {
                return  (uint)(newCost);
            }

            return currentCost;
        }

    }
}