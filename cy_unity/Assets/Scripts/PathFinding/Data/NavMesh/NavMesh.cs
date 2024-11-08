using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
#if USE_FP
using DH.LockStep.Framework;
#else
using FP = System.Single;
using TSVector2 = UnityEngine.Vector2;
using TSVector = UnityEngine.Vector3;
using TSRect = UnityEngine.Rect;
using TSMatrix4x4 = UnityEngine.Matrix4x4;
using TSMath = UnityEngine.Mathf;
#endif

namespace FindingPath.Data
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class NavMesh
    {
        [JsonProperty]
        public ushort TileX;
        [JsonProperty]
        public ushort TileY;

        [JsonProperty]
        public TSRect RectXZ;

        [JsonProperty]
        public TSVector CenterPosition;

        [JsonProperty]
        public bool IgnoreAxisY { get; set; }

        /// <summary>
        /// 顶点在局部空间坐标
        /// </summary>
        [JsonProperty]
        public TSVector[] Vertexes;
        
        /// <summary>
        /// 三角形数组
        /// </summary>
        [JsonProperty]
        public TriangleMeshNode[] MeshNodes;

        /// <summary>
        /// RVO障碍物顶点数据
        /// </summary>
        [JsonProperty]
        public RvoObstacle[] RvoObstacles;

        [JsonProperty]
        public SpaceOctree Octree;
    }

    /// <summary>
    /// 空间八叉树
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [Serializable]
    public sealed class SpaceOctree
    {
        public static int NodeCount { get; set; } = 10;

        /// <summary>
        /// local坐标系的轴对齐包围盒
        /// </summary>
        [Serializable]
        [JsonObject(MemberSerialization.OptIn)]
        public struct AABB
        {
            /// <summary>
            /// The maximum point of the box.
            /// </summary>
            [SerializeField]
            [JsonProperty]
            public TSVector min;

            /// <summary>
            /// The minimum point of the box.
            /// </summary>
            [SerializeField]
            [JsonProperty]
            public TSVector max;

            public TSVector center => (min + max) * .5f;

            public TSVector size => (max - min);

            public TSVector extents => size * .5f;

            public AABB(TSVector min, TSVector max)
            {
                this.min = min;
                this.max = max;
            }

            /// <summary>
            /// 获得点到轴对齐包围盒最近的距离的平方
            /// </summary>
            /// <param name="point"></param>
            /// <returns></returns>
            public FP PointClosestSqrDistance(TSVector point)
            {
                point = point - center;
                
                var xDelta = TSMath.Abs(point.x) - extents.x;
                xDelta = TSMath.Max(xDelta, 0);
                
                var yDelta = TSMath.Abs(point.y) - extents.y;
                yDelta = TSMath.Max(yDelta, 0);
                
                var zDelta = TSMath.Abs(point.z) - extents.z;
                zDelta = TSMath.Max(zDelta, 0);
                return xDelta * xDelta + yDelta * yDelta + zDelta * zDelta;
            }

            /// <summary>
            /// 获得点到轴对齐包围盒最近的距离
            /// </summary>
            /// <param name="point"></param>
            /// <returns></returns>
            public FP PointClosestDistance(TSVector point)
            {
                var sqrResult = PointClosestSqrDistance(point);
                return TSMath.Sqrt(sqrResult);
            }

            /// <summary>
            /// Checks whether a point is inside, outside or intersecting
            /// a point.
            /// </summary>
            /// <param name="point">A point in space.</param>
            /// <returns>The ContainmentType of the point.</returns>
            public bool Contains(TSVector point)
            {
                return (((this.min.x <= point.x) && (point.x <= this.max.x)) &&
                        ((this.min.y <= point.y) && (point.y <= this.max.y))) &&
                       ((this.min.z <= point.z) && (point.z <= this.max.z));
            }
            
            private FP Max(FP a, FP b, FP c)
            {
                return TSMath.Max(TSMath.Max(a, b), c);
            }

            private FP Min(FP a, FP b, FP c)
            {
                return TSMath.Min(TSMath.Min(a, b), c);
            }

            /// <summary>
            /// 判断是否和三角形重合，a,b,c代表三角形的三个顶点
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <param name="c"></param>
            /// <returns></returns>
            public bool Overlaps(TSVector a, TSVector b, TSVector c)
            {
                var v0 = a - center;
                var v1 = b - center;
                var v2 = c - center;

                // Compute edge vectors for triangle
                var f0 = b - a;
                var f1 = c - b;
                var f2 = a - c;

                //// region Test axes a00..a22 (category 3)

                // Test axis a00
                var a00 = new TSVector(0, -f0.z, f0.y);
                var p0 = TSVector.Dot(v0, a00);
                var p1 = TSVector.Dot(v1, a00);
                var p2 = TSVector.Dot(v2, a00);
                var r = extents.y * TSMath.Abs(f0.z) + extents.z * TSMath.Abs(f0.y);
                if (TSMath.Max(-Max(p0, p1, p2), Min(p0, p1, p2)) > r)
                    return false;

                // Test axis a01
                var a01 = new TSVector(0, -f1.z, f1.y);
                p0 = TSVector.Dot(v0, a01);
                p1 = TSVector.Dot(v1, a01);
                p2 = TSVector.Dot(v2, a01);
                r = extents.y * TSMath.Abs(f1.z) + extents.z * TSMath.Abs(f1.y);
                if (TSMath.Max(-Max(p0, p1, p2), Min(p0, p1, p2)) > r)
                    return false;

                // Test axis a02
                var a02 = new TSVector(0, -f2.z, f2.y);
                p0 = TSVector.Dot(v0, a02);
                p1 = TSVector.Dot(v1, a02);
                p2 = TSVector.Dot(v2, a02);
                r = extents.y * TSMath.Abs(f2.z) + extents.z * TSMath.Abs(f2.y);
                if (TSMath.Max(-Max(p0, p1, p2), Min(p0, p1, p2)) > r)
                    return false;

                // Test axis a10
                var a10 = new TSVector(f0.z, 0, -f0.x);
                p0 = TSVector.Dot(v0, a10);
                p1 = TSVector.Dot(v1, a10);
                p2 = TSVector.Dot(v2, a10);
                r = extents.x * TSMath.Abs(f0.z) + extents.z * TSMath.Abs(f0.x);
                if (TSMath.Max(-Max(p0, p1, p2), Min(p0, p1, p2)) > r)
                    return false;

                // Test axis a11
                var a11 = new TSVector(f1.z, 0, -f1.x);
                p0 = TSVector.Dot(v0, a11);
                p1 = TSVector.Dot(v1, a11);
                p2 = TSVector.Dot(v2, a11);
                r = extents.x * TSMath.Abs(f1.z) + extents.z * TSMath.Abs(f1.x);
                if (TSMath.Max(-Max(p0, p1, p2), Min(p0, p1, p2)) > r)
                    return false;

                // Test axis a12
                var a12 = new TSVector(f2.z, 0, -f2.x);
                p0 = TSVector.Dot(v0, a12);
                p1 = TSVector.Dot(v1, a12);
                p2 = TSVector.Dot(v2, a12);
                r = extents.x * TSMath.Abs(f2.z) + extents.z * TSMath.Abs(f2.x);
                if (TSMath.Max(-Max(p0, p1, p2), Min(p0, p1, p2)) > r)
                    return false;

                // Test axis a20
                var a20 = new TSVector(-f0.y, f0.x, 0);
                p0 = TSVector.Dot(v0, a20);
                p1 = TSVector.Dot(v1, a20);
                p2 = TSVector.Dot(v2, a20);
                r = extents.x * TSMath.Abs(f0.y) + extents.y * TSMath.Abs(f0.x);
                if (TSMath.Max(-Max(p0, p1, p2), Min(p0, p1, p2)) > r)
                    return false;

                // Test axis a21
                var a21 = new TSVector(-f1.y, f1.x, 0);
                p0 = TSVector.Dot(v0, a21);
                p1 = TSVector.Dot(v1, a21);
                p2 = TSVector.Dot(v2, a21);
                r = extents.x * TSMath.Abs(f1.y) + extents.y * TSMath.Abs(f1.x);
                if (TSMath.Max(-Max(p0, p1, p2), Min(p0, p1, p2)) > r)
                    return false;

                // Test axis a22
                var a22 = new TSVector(-f2.y, f2.x, 0);
                p0 = TSVector.Dot(v0, a22);
                p1 = TSVector.Dot(v1, a22);
                p2 = TSVector.Dot(v2, a22);
                r = extents.x * TSMath.Abs(f2.y) + extents.y * TSMath.Abs(f2.x);
                if (TSMath.Max(-Max(p0, p1, p2), Min(p0, p1, p2)) > r)
                    return false;

                //// endregion

                //// region Test the three axes corresponding to the face normals of AABB b (category 1)

                // Exit if...
                // ... [-extents.X, extents.X] and [Min(v0.X,v1.X,v2.X), Max(v0.X,v1.X,v2.X)] do not overlap
                if (Max(v0.x, v1.x, v2.x) < -extents.x || Min(v0.x, v1.x, v2.x) > extents.x)
                    return false;

                // ... [-extents.Y, extents.Y] and [Min(v0.Y,v1.Y,v2.Y), Max(v0.Y,v1.Y,v2.Y)] do not overlap
                if (Max(v0.y, v1.y, v2.y) < -extents.y || Min(v0.y, v1.y, v2.y) > extents.y)
                    return false;

                // ... [-extents.Z, extents.Z] and [Min(v0.Z,v1.Z,v2.Z), Max(v0.Z,v1.Z,v2.Z)] do not overlap
                if (Max(v0.z, v1.z, v2.z) < -extents.z || Min(v0.z, v1.z, v2.z) > extents.z)
                    return false;

                //// endregion

                //// region Test separating axis corresponding to triangle face normal (category 2)

                var plane_normal = TSVector.Cross(f0, f1);
                var plane_distance = TSMath.Abs(TSVector.Dot(plane_normal, v0));

                // Compute the projection interval radius of b onto L(t) = b.c + t * p.n
                r = extents.x * TSMath.Abs(plane_normal.x) + extents.y * TSMath.Abs(plane_normal.y) +
                    extents.z * TSMath.Abs(plane_normal.z);

                // Intersection occurs when plane distance falls within [-r,+r] interval
                if (plane_distance > r)
                    return false;

                //// endregion

                return true;
            }
        }

        [JsonProperty]
        public AABB Box { get; private set; }
        
        [JsonProperty]
        public List<int> Nodes = null;

        [JsonProperty]
        public List<SpaceOctree> ChildTree { get; private set; }

        public SpaceOctree()
        {
            
        }

        public SpaceOctree(TSVector min, TSVector size)
        {
            Nodes = new List<int>(NodeCount);
            Box = new AABB(min, min + size);
        }

        /// <summary>
        /// 移除非必要的叶子节点，可以在预处理时调用
        /// </summary>
        /// <param name="octreeNode"></param>
        /// <returns></returns>
        public static void OptimizeBuild(SpaceOctree octree)
        {
            OptimizeBuildInternal(octree);
            if (octree.Nodes.Count <= 0)
            {
                octree.Nodes = null;
            }
        }
        
        private static bool OptimizeBuildInternal(SpaceOctree octreeNode)
        {
            if (octreeNode.Nodes.Count <= 0 && octreeNode.ChildTree == null)
            {
                return true;
            }

            int childIndex = 0;
            while (octreeNode.ChildTree != null && childIndex < octreeNode.ChildTree.Count)
            {
                var child = octreeNode.ChildTree[childIndex];
                var isOptimize = OptimizeBuildInternal(child);
                if (isOptimize)
                {
                    octreeNode.ChildTree.RemoveAt(childIndex);
                }
                else
                {
                    childIndex++;
                }
            }

            return false;
        }
        
        public static void AddNode(SpaceOctree octree, TriangleMeshNode meshNode, NavMesh owner)
        {
            var A = owner.Vertexes[meshNode.V0];
            var B = owner.Vertexes[meshNode.V1];
            var C = owner.Vertexes[meshNode.V2];
            AddNodeInternal(octree, (int)meshNode.MeshNodeIndex, owner);
        }

        private static bool AddNodeInternal(SpaceOctree octree, int nodeIndex, NavMesh owner)
        {
            TriangleMeshNode meshNode = owner.MeshNodes[nodeIndex];
            var A = owner.Vertexes[meshNode.V0];
            var B = owner.Vertexes[meshNode.V1];
            var C = owner.Vertexes[meshNode.V2];
            
            if (!octree.Box.Overlaps(A, B, C))
            {
                return false;
            }

            if (octree.ChildTree == null)
            {
                if (octree.Nodes.Count < NodeCount)
                {
                    octree.Nodes.Add(nodeIndex);
                    return true;
                }
            
                //创建八叉树的子节点，把所有node放到子节点中
                RebuildChild(octree, owner.IgnoreAxisY);
                foreach (var node in octree.Nodes)
                {
                    AddNodeInternal(octree, node, owner);
                }
                octree.Nodes.Clear();
                
                AddNodeInternal(octree, nodeIndex, owner);
                return true;
            }

            foreach (var child in octree.ChildTree)
            {
                AddNodeInternal(child, nodeIndex, owner);
            }
            
            return true;
        }

        private static void RebuildChild(SpaceOctree octree, bool isIgnoreAxisY = false)
        {
            var half = octree.Box.size / 2;
            var min = octree.Box.min;

            octree.ChildTree = new List<SpaceOctree>(8);

            var childMini = min;
            octree.ChildTree.Add(new SpaceOctree(childMini, half));

            childMini = new TSVector(min.x, min.y, min.z + half.z);
            octree.ChildTree.Add(new SpaceOctree(childMini, half));

            childMini = new TSVector(min.x + half.x, min.y, min.z + half.z);
            octree.ChildTree.Add(new SpaceOctree(childMini, half));

            childMini = new TSVector(min.x + half.x, min.y, min.z);
            octree.ChildTree.Add(new SpaceOctree(childMini, half));

            //只计算xz平面，忽略y轴
            if (isIgnoreAxisY)
            {
                return;
            }
            childMini = new TSVector(min.x, min.y + half.y, min.z);
            octree.ChildTree.Add(new SpaceOctree(childMini, half));

            childMini = new TSVector(min.x, min.y + half.y, min.z + half.z);
            octree.ChildTree.Add( new SpaceOctree(childMini, half));

            childMini = new TSVector(min.x + half.x, min.y + half.y, min.z + half.z);
            octree.ChildTree.Add(new SpaceOctree(childMini, half));

            childMini = new TSVector(min.x + half.x, min.y + half.y, min.z);
            octree.ChildTree.Add(new SpaceOctree(childMini, half));
        }

        /// <summary>
        /// 根据空间划分，查找对应部分的TriangleMeshNode索引，开始遍历查找
        /// </summary>
        /// <param name="position"></param>
        /// <param name="octree"></param>
        /// <param name="meshNodes"></param>
        public static void FindNode(TSVector position, SpaceOctree octree, ref List<int> meshNodes)
        {
            if (octree.Box.Contains(position))
            {
                FindNodeInternal(position, octree, ref meshNodes);
            }

            if (meshNodes.Count < 1)
            {
                //如果坐标点在包围盒之外，按照距离包围盒的距离进行查找
                FindNodeWithDistanceInternal(position, octree, ref meshNodes);
            }
        }

        private static void FindNodeWithDistanceInternal(TSVector position, SpaceOctree octreeNode, ref List<int> meshNodes)
        {
            if (octreeNode.ChildTree == null)
            {
                meshNodes.AddRange(octreeNode.Nodes);
                return;
            }

            FP miniDistance = FP.MaxValue;
            SpaceOctree minDistanceTree = null;
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

        private static bool FindNodeInternal(TSVector position, SpaceOctree octreeNode, ref List<int> meshNodes)
        {
            if (!octreeNode.Box.Contains(position))
            {
                return false;
            }

            if (octreeNode.ChildTree == null)
            {
                meshNodes.AddRange(octreeNode.Nodes);
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