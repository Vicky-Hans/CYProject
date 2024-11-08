using System;
using Unity.Collections;
using Unity.Mathematics;

namespace PathFinding.DOTS
{
    public struct FunnelPortals : IDisposable
    {
        public NativeList<float3> left;
        public NativeList<float3> right;

        public void Dispose()
        {
            left.Dispose();
            right.Dispose();
        }
        
        /// <summary>
        /// 转换到xz平面
        /// </summary>
        /// <param name="leftXZ"></param>
        /// <param name="rightXZ"></param>
        public void ToXZ(out NativeList<float2> leftXZ, out NativeList<float2> rightXZ)
        {
            leftXZ = new NativeList<float2>(left.Length, Allocator.Temp);
            rightXZ = new NativeList<float2>(right.Length, Allocator.Temp);

            for (int i = 0; i < left.Length; i++)
            {
                float3 vectorXYZ = left[i];
                leftXZ.Add(ToXZ(vectorXYZ));
            }
                
            for (int i = 0; i < right.Length; i++)
            {
                float3 vectorXYZ = right[i];
                rightXZ.Add(ToXZ(vectorXYZ));
            }
        }
        
        private float2 ToXZ (float3 p) {
            return new float2(p.x, p.z);
        }

        private float3 FromXZ (float2 p) {
            return new float3(p.x, 0, p.y);
        }
    }
}