using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FindingPath.Data;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Pool;

namespace PathFinding.DOTS
{
    public class AStarPathSystem : MonoBehaviour
    {
        public static readonly ProfilerMarker AStarPathSystemMaker = new ProfilerMarker("AStarPathSystem");
        
        private const int MaxJobCount = 20;
        
        private NativeArray<NavMeshData> allNavMeshes;
        private float4x4 localToWorldMatrix;
        private float4x4 worldToLocalMatrix;

        private Queue<PathFindingRequest> requests;
        private List<PathFindJob> jobHandles = new List<PathFindJob>();

        #region Instance

        private static AStarPathSystem instance;
        public static AStarPathSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    var obj = new GameObject("AStarPathSystem");
                    instance = obj.AddComponent<AStarPathSystem>();
                }
                
                return instance;
            }
        }
        
        #endregion


        private void Awake()
        {
            requests = new Queue<PathFindingRequest>();
            instance = this;
        }

        private void OnDestroy()
        {
            foreach (var jobHandle in jobHandles)
            {
                jobHandle.Job.Complete();
            }
            
            foreach (var request in requests)
            {
                request.Cancel();
            }
            requests.Clear();
            
            foreach (var navMeshData in allNavMeshes)
            {
                navMeshData.Dispose();
            }
            allNavMeshes.Dispose();
            instance = null;
        }

        public void Init(List<NavMesh> navMeshes, Matrix4x4 LWMatrix, Matrix4x4 WLMatrix)
        {
            this.localToWorldMatrix = new float4x4(
                LWMatrix.m00, LWMatrix.m01, LWMatrix.m02, LWMatrix.m03,
                LWMatrix.m10, LWMatrix.m11, LWMatrix.m12, LWMatrix.m13,
                LWMatrix.m20, LWMatrix.m21, LWMatrix.m22, LWMatrix.m23,
                LWMatrix.m30, LWMatrix.m31, LWMatrix.m32, LWMatrix.m33
                );
            
            this.worldToLocalMatrix = new float4x4(
                WLMatrix.m00, WLMatrix.m01, WLMatrix.m02, WLMatrix.m03,
                WLMatrix.m10, WLMatrix.m11, WLMatrix.m12, WLMatrix.m13,
                WLMatrix.m20, WLMatrix.m21, WLMatrix.m22, WLMatrix.m23,
                WLMatrix.m30, WLMatrix.m31, WLMatrix.m32, WLMatrix.m33
            );


            allNavMeshes = new NativeArray<NavMeshData>(navMeshes.Count, Allocator.Persistent);
            for (int i = 0; i < navMeshes.Count; i++)
            {
                var navMesh = navMeshes[i];
                var navMeshData = new NavMeshData()
                {
                    TileX = navMesh.TileX,
                    TileY = navMesh.TileY,
                    CenterPosition = navMesh.CenterPosition,
                    IgnoreAxisY = navMesh.IgnoreAxisY,
                    RectXZ = new NavMeshData.Rect()
                    {
                        xMin = navMesh.RectXZ.xMin,
                        yMin = navMesh.RectXZ.yMin,
                        width = navMesh.RectXZ.width,
                        height = navMesh.RectXZ.height,
                    }
                };

                navMeshData.Vertexes = new UnsafeList<float3>(navMesh.Vertexes.Length, Allocator.Persistent);
                for (int j = 0; j < navMesh.Vertexes.Length; j++)
                {
                    navMeshData.Vertexes.Add(navMesh.Vertexes[j]);
                }

                navMeshData.MeshNodes =
                    new UnsafeList<TriangleMeshNode>(navMesh.MeshNodes.Length, Allocator.Persistent);
                for (int j = 0; j < navMesh.MeshNodes.Length; j++)
                {
                    var meshNode = navMesh.MeshNodes[j];
                    TriangleMeshNode newMeshNode = new TriangleMeshNode()
                    {
                        NodeIndex = meshNode.NodeIndex,
                        V0 = meshNode.V0,
                        V1 = meshNode.V1,
                        V2 = meshNode.V2,
                        Position = meshNode.Position
                    };

                    newMeshNode.Connections =
                        new UnsafeList<Connection>(meshNode.Connections.Length, Allocator.Persistent);
                    for (int k = 0; k < meshNode.Connections.Length; k++)
                    {
                        var oldConnection = meshNode.Connections[k];
                        Connection connection = new Connection()
                        {
                            NodeIndex = oldConnection.NodeIndex,
                            Cost = oldConnection.Cost,
                            ShapeEdge = oldConnection.ShapeEdge,
                        };

                        newMeshNode.Connections.Add(connection);
                    }
                    
                    navMeshData.MeshNodes.Add(newMeshNode);
                }

                InitSpaceOctree(out var octree, navMesh.Octree);
                navMeshData.Octree = octree;

                allNavMeshes[i] = navMeshData;
            }
        }

        private void InitSpaceOctree(out SpaceOctree newSpaceOctree, in FindingPath.Data.SpaceOctree oldSpaceOctree)
        {
            newSpaceOctree = new SpaceOctree()
            {
                Box = new SpaceOctree.AABB()
                {
                    min = oldSpaceOctree.Box.min,
                    max = oldSpaceOctree.Box.max,
                },
            };

            if (oldSpaceOctree.Nodes != null && oldSpaceOctree.Nodes.Count > 0)
            {
                newSpaceOctree.Nodes = new UnsafeList<int>(oldSpaceOctree.Nodes.Count, Allocator.Persistent);
                for (int j = 0; j < oldSpaceOctree.Nodes.Count; j++)
                {
                    newSpaceOctree.Nodes.Add(oldSpaceOctree.Nodes[j]);
                }
            }

            if (oldSpaceOctree.ChildTree != null && oldSpaceOctree.ChildTree.Count > 0)
            {
                newSpaceOctree.ChildTree =
                    new UnsafeList<SpaceOctree>(oldSpaceOctree.ChildTree.Count, Allocator.Persistent);
                for (int i = 0; i < oldSpaceOctree.ChildTree.Count; i++)
                {
                    SpaceOctree newChild = new SpaceOctree();
                    InitSpaceOctree(out newChild, oldSpaceOctree.ChildTree[i]);
                    newSpaceOctree.ChildTree.Add(newChild);
                }
            }
        }

        private void Update()
        {
            if (jobHandles.Count < MaxJobCount && requests.Count > 0)
            {
                var remainCount = Mathf.Min(MaxJobCount - jobHandles.Count, requests.Count);
                for (int i = 0; i < remainCount; i++)
                {
                    var findingRequest = requests.Dequeue();
                    findingRequest.nativePathResult = new NativeArray<float3>(100, Allocator.TempJob);
                    findingRequest.nativePathResultCount = new NativeArray<int>(1, Allocator.TempJob);
                    findingRequest.nativeBinaryHeap = new NativeBinaryHeap(400);
                    findingRequest.currentNavMeshDic =
                        new NativeParallelHashMap<uint, NavMeshData>(100, Allocator.TempJob);
                    findingRequest.pathNodeHashMap = new NativeParallelHashMap<uint, PathNode>(100, Allocator.TempJob);
                    
                    var job = new AStarPathJob()
                    {
                        AllNavMeshes = allNavMeshes,
                        localToWorldMatrix = localToWorldMatrix,
                        worldToLocalMatrix = worldToLocalMatrix,
                        PathResult = findingRequest.nativePathResult,
                        PathResultCount = findingRequest.nativePathResultCount,
                        startPosition = findingRequest.StartPos,
                        endPosition = findingRequest.EndPos,
                        binaryHeap = findingRequest.nativeBinaryHeap,
                        currentNavMeshDic = findingRequest.currentNavMeshDic,
                        pathNodeHashMap = findingRequest.pathNodeHashMap,
                    };

                    jobHandles.Add( new PathFindJob()
                    {
                        Job = job.Schedule(),
                        Request = findingRequest,
                    });   
                }
            }
            
            
            if (jobHandles.Count > 0)
            {
                int index = 0;
                while (index < jobHandles.Count)
                {
                    var jobHandle = jobHandles[index];
                    var job = jobHandle.Job;
                    var request = jobHandle.Request;
                    if (job.IsCompleted)
                    {
                        job.Complete();
                        jobHandles.RemoveAt(index);
                        request.Complete();
                        continue;
                    }

                    index++;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns>需要调用Recycle 回收</returns>
        public UniTask<List<Vector3>> RequestFindPath(Vector3 startPos, Vector3 endPos)
        {
            AStarPathSystemMaker.Begin();
            
            var tcs = AutoResetUniTaskCompletionSource<List<Vector3>>.Create();

            var request = new PathFindingRequest(PathFindingRequest.RequestIdCounter++, startPos, endPos)
            {
                utcs = tcs
            };
            requests.Enqueue(request);
            
            AStarPathSystemMaker.End();
            
            return tcs.Task;
        }

        public void Recycle(List<Vector3> list)
        {
            if (list == null)
            {
                return;
            }
            
            ListPool<Vector3>.Release(list);
        }

    }
}