using System.Collections.Generic;
using FindingPath.Data;
using FindingPath.Path;
using PathFinding.Data;
using UnityEngine.Pool;
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
namespace FindingPath.Core
{
    public static class AstarMath
    {
        /// <summary>
		/// Traces the contour of a navmesh.
		///
		/// [Open online documentation to see images]
		///
		/// This image is just used to illustrate the difference between chains and cycles. That it shows a grid graph is not relevant.
		/// [Open online documentation to see images]
		///
		/// See: <see cref="GetContours(NavGraph)"/>
		/// </summary>
		/// <param name="navmesh">The navmesh-like object to trace. This can be a recast or navmesh graph or it could be a single tile in one such graph.</param>
		/// <param name="results">Will be called once for each contour with the contour as a parameter as well as a boolean indicating if the contour is a cycle or a chain (see second image).</param>
		public static void GetContours(AStarPathManager pathManager,System.Action<List<TSVector>, bool> results) {
			// Assume 3 vertices per node
			var uses = new bool[3];

			var outline = new Dictionary<int, int>();
			var vertexPositions = new Dictionary<int, TSVector>();
			var hasInEdge = new HashSet<int>();

			var navMesh = pathManager.DefaultNavMesh;
			var nodes = navMesh.MeshNodes;
			foreach (var node in nodes)
			{
				uses[0] = uses[1] = uses[2] = false;

				if (node != null) {
					// Find out which edges are shared with other nodes
					for (int j = 0; j < node.Connections.Length; j++) {
						var other = nodes[node.Connections[j].NodeIndex];

						// Not necessarily a TriangleMeshNode
						if (other != null) {
							int a = pathManager.FindSharedEdge(node,other);
							if (a != 0xFF) uses[a] = true;
						}
					}

					// Loop through all edges on the node
					for (int j = 0; j < 3; j++) {
						// The edge is not shared with any other node
						// I.e it is an exterior edge on the mesh
						if (!uses[j]) {
							var i1 = j;
							var i2 = (j+1) % node.GetVertexCount();

							outline[node.GetVertexIndex(i1)] = node.GetVertexIndex(i2);
							hasInEdge.Add(node.GetVertexIndex(i2));
							vertexPositions[node.GetVertexIndex(i1)] = navMesh.Vertexes[node.GetVertexIndex(i1)];
							vertexPositions[node.GetVertexIndex(i2)] = navMesh.Vertexes[node.GetVertexIndex(i1)];
						}
					}
				}
			}

			TraceContours(outline, hasInEdge, (chain, cycle) => {
				List<TSVector> vertices = ListPool<TSVector>.Get();
				for (int i = 0; i < chain.Count; i++) vertices.Add(vertexPositions[chain[i]]);
				results(vertices, cycle);
			});
		}
        
        /// <summary>
        /// Given a set of edges between vertices, follows those edges and returns them as chains and cycles.
        ///
        /// [Open online documentation to see images]
        /// </summary>
        /// <param name="outline">outline[a] = b if there is an edge from a to b.</param>
        /// <param name="hasInEdge">hasInEdge should contain b if outline[a] = b for any key a.</param>
        /// <param name="results">Will be called once for each contour with the contour as a parameter as well as a boolean indicating if the contour is a cycle or a chain (see image).</param>
        public static void TraceContours(Dictionary<int, int> outline, HashSet<int> hasInEdge,
            System.Action<List<int>, bool> results)
        {
            // Iterate through chains of the navmesh outline.
            // I.e segments of the outline that are not loops
            // we need to start these at the beginning of the chain.
            // Then iterate over all the loops of the outline.
            // Since they are loops, we can start at any point.
            var obstacleVertices = ListPool<int>.Get();
            var outlineKeys = ListPool<int>.Get();

            outlineKeys.AddRange(outline.Keys);
            for (int k = 0; k <= 1; k++)
            {
                bool cycles = k == 1;
                for (int i = 0; i < outlineKeys.Count; i++)
                {
                    var startIndex = outlineKeys[i];

                    // Chains (not cycles) need to start at the start of the chain
                    // Cycles can start at any point
                    if (!cycles && hasInEdge.Contains(startIndex))
                    {
                        continue;
                    }

                    var index = startIndex;
                    obstacleVertices.Clear();
                    obstacleVertices.Add(index);

                    while (outline.ContainsKey(index))
                    {
                        var next = outline[index];
                        outline.Remove(index);

                        obstacleVertices.Add(next);

                        // We traversed a full cycle
                        if (next == startIndex) break;

                        index = next;
                    }

                    if (obstacleVertices.Count > 1)
                    {
                        results(obstacleVertices, cycles);
                    }
                }
            }

            ListPool<int>.Release(outlineKeys);
            ListPool<int>.Release(obstacleVertices);
            outlineKeys = null;
            obstacleVertices = null;
        }
    }
}