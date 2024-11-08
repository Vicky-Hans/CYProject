#if USE_FP
    using DH.LockStep.Framework;
#else
    using FP = System.Single;
    using TSVector2 = UnityEngine.Vector2;
    using TSVector = UnityEngine.Vector3;
    using TSRect = UnityEngine.Rect;
    using TSMatrix4x4 = UnityEngine.Matrix4x4;
#endif

namespace FindingPath.Core
{
	/// <summary>
	/// 
	/// Utility functions for working with polygons, lines, and other vector math.
	/// All functions which accepts Vector3s but work in 2D space uses the XZ space if nothing else is said.
	///
	/// Version: A lot of functions in this class have been moved to the VectorMath class
	/// the names have changed slightly and everything now consistently assumes a left handed
	/// coordinate system now instead of sometimes using a left handed one and sometimes
	/// using a right handed one. This is why the 'Left' methods redirect to methods
	/// named 'Right'. The functionality is exactly the same.
	/// 
	/// </summary>
    public static class PolygonMath
    {
	    /// <summary>
		/// Closest point on the triangle abc to the point p.
		/// See: 'Real Time Collision Detection' by Christer Ericson, chapter 5.1, page 141
		/// </summary>
		public static TSVector2 ClosestPointOnTriangle (TSVector2 a, TSVector2 b, TSVector2 c, TSVector2 p) {
			// Check if p is in vertex region outside A
			var ab = b - a;
			var ac = c - a;
			var ap = p - a;

			var d1 = TSVector2.Dot(ab, ap);
			var d2 = TSVector2.Dot(ac, ap);

			// Barycentric coordinates (1,0,0)
			if (d1 <= 0 && d2 <= 0) {
				return a;
			}

			// Check if p is in vertex region outside B
			var bp = p - b;
			var d3 = TSVector2.Dot(ab, bp);
			var d4 = TSVector2.Dot(ac, bp);

			// Barycentric coordinates (0,1,0)
			if (d3 >= 0 && d4 <= d3) {
				return b;
			}

			// Check if p is in edge region outside AB, if so return a projection of p onto AB
			if (d1 >= 0 && d3 <= 0) {
				var vc = d1 * d4 - d3 * d2;
				if (vc <= 0) {
					// Barycentric coordinates (1-v, v, 0)
					var v = d1 / (d1 - d3);
					return a + ab*v;
				}
			}

			// Check if p is in vertex region outside C
			var cp = p - c;
			var d5 = TSVector2.Dot(ab, cp);
			var d6 = TSVector2.Dot(ac, cp);

			// Barycentric coordinates (0,0,1)
			if (d6 >= 0 && d5 <= d6) {
				return c;
			}

			// Check if p is in edge region of AC, if so return a projection of p onto AC
			if (d2 >= 0 && d6 <= 0) {
				var vb = d5 * d2 - d1 * d6;
				if (vb <= 0) {
					// Barycentric coordinates (1-v, 0, v)
					var v = d2 / (d2 - d6);
					return a + ac*v;
				}
			}

			// Check if p is in edge region of BC, if so return projection of p onto BC
			if ((d4 - d3) >= 0 && (d5 - d6) >= 0) {
				var va = d3 * d6 - d5 * d4;
				if (va <= 0) {
					var v = (d4 - d3) / ((d4 - d3) + (d5 - d6));
					return b + (c - b) * v;
				}
			}

			return p;
		}

		/// <summary>
		/// Closest point on the triangle abc to the point p when seen from above.
		/// See: 'Real Time Collision Detection' by Christer Ericson, chapter 5.1, page 141
		/// </summary>
		public static TSVector ClosestPointOnTriangleXZ (TSVector a, TSVector b, TSVector c, TSVector p) {
			// Check if p is in vertex region outside A
			var ab = new TSVector2(b.x - a.x, b.z - a.z);
			var ac = new TSVector2(c.x - a.x, c.z - a.z);
			var ap = new TSVector2(p.x - a.x, p.z - a.z);

			var d1 = TSVector2.Dot(ab, ap);
			var d2 = TSVector2.Dot(ac, ap);

			// Barycentric coordinates (1,0,0)
			if (d1 <= 0 && d2 <= 0) {
				return a;
			}

			// Check if p is in vertex region outside B
			var bp = new TSVector2(p.x - b.x, p.z - b.z);
			var d3 = TSVector2.Dot(ab, bp);
			var d4 = TSVector2.Dot(ac, bp);

			// Barycentric coordinates (0,1,0)
			if (d3 >= 0 && d4 <= d3) {
				return b;
			}

			// Check if p is in edge region outside AB, if so return a projection of p onto AB
			var vc = d1 * d4 - d3 * d2;
			if (d1 >= 0 && d3 <= 0 && vc <= 0) {
				// Barycentric coordinates (1-v, v, 0)
				var v = d1 / (d1 - d3);
				return (1-v)*a + v*b;
			}

			// Check if p is in vertex region outside C
			var cp = new TSVector2(p.x - c.x, p.z - c.z);
			var d5 = TSVector2.Dot(ab, cp);
			var d6 = TSVector2.Dot(ac, cp);

			// Barycentric coordinates (0,0,1)
			if (d6 >= 0 && d5 <= d6) {
				return c;
			}

			// Check if p is in edge region of AC, if so return a projection of p onto AC
			var vb = d5 * d2 - d1 * d6;
			if (d2 >= 0 && d6 <= 0 && vb <= 0) {
				// Barycentric coordinates (1-v, 0, v)
				var v = d2 / (d2 - d6);
				return (1-v)*a + v*c;
			}

			// Check if p is in edge region of BC, if so return projection of p onto BC
			var va = d3 * d6 - d5 * d4;
			if ((d4 - d3) >= 0 && (d5 - d6) >= 0 && va <= 0) {
				var v = (d4 - d3) / ((d4 - d3) + (d5 - d6));
				return b + (c - b) * v;
			} else {
				// P is inside the face region. Compute the point using its barycentric coordinates (u, v, w)
				// Note that the x and z coordinates will be exactly the same as P's x and z coordinates
				var denom = 1f / (va + vb + vc);
				var v = vb * denom;
				var w = vc * denom;

				return new TSVector(p.x, (1 - v - w)*a.y + v*b.y + w*c.y, p.z);
			}
		}

		/// <summary>
		/// Closest point on the triangle abc to the point p.
		/// See: 'Real Time Collision Detection' by Christer Ericson, chapter 5.1, page 141
		/// </summary>
		public static TSVector ClosestPointOnTriangle (TSVector a, TSVector b, TSVector c, TSVector p) {
			// Check if p is in vertex region outside A
			var ab = b - a;
			var ac = c - a;
			var ap = p - a;

			var d1 = TSVector.Dot(ab, ap);
			var d2 = TSVector.Dot(ac, ap);

			// Barycentric coordinates (1,0,0)
			if (d1 <= 0 && d2 <= 0)
				return a;

			// Check if p is in vertex region outside B
			var bp = p - b;
			var d3 = TSVector.Dot(ab, bp);
			var d4 = TSVector.Dot(ac, bp);

			// Barycentric coordinates (0,1,0)
			if (d3 >= 0 && d4 <= d3)
				return b;

			// Check if p is in edge region outside AB, if so return a projection of p onto AB
			var vc = d1 * d4 - d3 * d2;
			if (d1 >= 0 && d3 <= 0 && vc <= 0) {
				// Barycentric coordinates (1-v, v, 0)
				var v = d1 / (d1 - d3);
				return a + ab * v;
			}

			// Check if p is in vertex region outside C
			var cp = p - c;
			var d5 = TSVector.Dot(ab, cp);
			var d6 = TSVector.Dot(ac, cp);

			// Barycentric coordinates (0,0,1)
			if (d6 >= 0 && d5 <= d6)
				return c;

			// Check if p is in edge region of AC, if so return a projection of p onto AC
			var vb = d5 * d2 - d1 * d6;
			if (d2 >= 0 && d6 <= 0 && vb <= 0) {
				// Barycentric coordinates (1-v, 0, v)
				var v = d2 / (d2 - d6);
				return a + ac * v;
			}

			// Check if p is in edge region of BC, if so return projection of p onto BC
			var va = d3 * d6 - d5 * d4;
			if ((d4 - d3) >= 0 && (d5 - d6) >= 0 && va <= 0) {
				var v = (d4 - d3) / ((d4 - d3) + (d5 - d6));
				return b + (c - b) * v;
			} else {
				// P is inside the face region. Compute the point using its barycentric coordinates (u, v, w)
				var denom = 1f / (va + vb + vc);
				var v = vb * denom;
				var w = vc * denom;

				// This is equal to: u*a + v*b + w*c, u = va*denom = 1 - v - w;
				return a + ab * v + ac * w;
			}
		}
		
    }
}