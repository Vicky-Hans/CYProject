using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;

namespace PathFinding.DOTS
{
    internal struct PathFindingRequest : IDisposable
    {
        internal static int RequestIdCounter = 1;

        internal AutoResetUniTaskCompletionSource<List<Vector3>> utcs { get; set; }
        internal float3 StartPos { get; private set; }
        internal float3 EndPos{ get; private set; }
        
        private int RequestId { get; set; }
        private List<Vector3> PathResult { get; set; }
        
        internal NativeArray<float3> nativePathResult;
        internal NativeArray<int> nativePathResultCount;
        internal NativeBinaryHeap nativeBinaryHeap;
        internal NativeParallelHashMap<uint, NavMeshData> currentNavMeshDic;
        internal NativeParallelHashMap<uint, PathNode> pathNodeHashMap;

        public PathFindingRequest(int requestId, Vector3 start, Vector3 end)
        {
            RequestId = requestId;
            StartPos = new float3(start.x, start.y, start.z);
            EndPos = new float3(end.x, end.y, end.z);
            PathResult = null;
            nativePathResult = default;
            nativePathResultCount = default;
            nativeBinaryHeap = default;
            currentNavMeshDic = default;
            pathNodeHashMap = default;
            utcs = null;
        }

        public void Cancel()
        {
            ReleaseNativeMemory();

            utcs.TrySetCanceled();
        }

        public void Failed()
        {
            ReleaseNativeMemory();

            utcs.TrySetException(new OperationCanceledException());
        }

        public void Complete()
        {
            FetchNativePathResult();
            ReleaseNativeMemory();
            utcs.TrySetResult(PathResult);
        }

        private void FetchNativePathResult()
        {
            var count = nativePathResultCount[0];
            PathResult = ListPool<Vector3>.Get();
            for (int i = 0; i < count; i++)
            {
                PathResult.Add(nativePathResult[i]);
            }
        }

        private void ReleaseNativeMemory()
        {
            nativePathResult.Dispose();
            nativePathResultCount.Dispose();
            nativeBinaryHeap.Dispose();
            currentNavMeshDic.Dispose();
            pathNodeHashMap.Dispose();
        }

        public void Dispose()
        {
            ReleaseNativeMemory();
        }
    }

    public struct PathFindJob
    {
        public JobHandle Job { get; set; }

        internal PathFindingRequest Request { get; set; }
    }
}