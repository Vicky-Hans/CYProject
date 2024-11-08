using System;
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

namespace FindingPath.Core
{

	public static class VectorMath
	{
		/// <summary>
		/// 3D minimum distance between 2 segments.
		/// Input: two 3D line segments S1 and S2
		/// Returns: the shortest squared distance between S1 and S2
		/// </summary>
		public static FP SqrDistanceSegmentSegment(TSVector s1, TSVector e1, TSVector s2, TSVector e2)
		{
			TSVector u = e1 - s1;
			TSVector v = e2 - s2;
			TSVector w = s1 - s2;
			FP a = TSVector.Dot(u, u); // always >= 0
			FP b = TSVector.Dot(u, v);
			FP c = TSVector.Dot(v, v); // always >= 0
			FP d = TSVector.Dot(u, w);
			FP e = TSVector.Dot(v, w);
			FP D = a * c - b * b; // always >= 0
			FP sc, sN, sD = D; // sc = sN / sD, default sD = D >= 0
			FP tc, tN, tD = D; // tc = tN / tD, default tD = D >= 0

			// compute the line parameters of the two closest points
			// D is approximately |v|^2|u|^2*(1-cos alpha), where alpha is the angle between the lines
			if (D < 0.00001)
			{
				// the lines are almost parallel
				sN = 0.0f; // force using point P0 on segment S1
				sD = 1.0f; // to prevent possible division by 0.0 later
				tN = e;
				tD = c;
			}
			else
			{
				// get the closest points on the infinite lines
				sN = (b * e - c * d);
				tN = (a * e - b * d);
				if (sN < 0.0)
				{
					// sc < 0 => the s=0 edge is visible
					sN = 0.0f;
					tN = e;
					tD = c;
				}
				else if (sN > sD)
				{
					// sc > 1  => the s=1 edge is visible
					sN = sD;
					tN = e + b;
					tD = c;
				}
			}

			if (tN < 0.0)
			{
				// tc < 0 => the t=0 edge is visible
				tN = 0.0f;
				// recompute sc for this edge
				if (-d < 0.0f)
					sN = 0.0f;
				else if (-d > a)
					sN = sD;
				else
				{
					sN = -d;
					sD = a;
				}
			}
			else if (tN > tD)
			{
				// tc > 1  => the t=1 edge is visible
				tN = tD;
				// recompute sc for this edge
				if ((-d + b) < 0.0f)
					sN = 0;
				else if ((-d + b) > a)
					sN = sD;
				else
				{
					sN = (-d + b);
					sD = a;
				}
			}

			// finally do the division to get sc and tc
			sc = (TSMath.Abs(sN) < 0.00001f ? 0.0f : sN / sD);
			tc = (TSMath.Abs(tN) < 0.00001f ? 0.0f: tN / tD);

			// get the difference of the two closest points
			TSVector dP = w + (sc * u) - (tc * v); // =  S1(sc) - S2(tc)

			return dP.sqrMagnitude; // return the closest distance
		}
		
		/// <summary>
		/// Returns the closest point on the segment.
		/// The segment is NOT treated as infinite.
		/// See: ClosestPointOnLine
		/// See: ClosestPointOnSegmentXZ
		/// </summary>
		public static TSVector ClosestPointOnSegment (TSVector lineStart, TSVector lineEnd, TSVector point) {
			var dir = lineEnd - lineStart;
			FP sqrMagn = dir.sqrMagnitude;

			if (sqrMagn <= 0.000001f) return lineStart;

			FP factor = TSVector.Dot(point - lineStart, dir) / sqrMagn;
			return lineStart + TSMath.Clamp01(factor)*dir;
		}
		
		/// <summary>
		/// Complex number multiplication.
		/// Returns: a * b
		///
		/// Used to rotate vectors in an efficient way.
		///
		/// See: https://en.wikipedia.org/wiki/Complex_number<see cref="Multiplication_and_division"/>
		/// </summary>
		public static TSVector2 ComplexMultiply (TSVector2 a, TSVector2 b) {
			return new TSVector2(a.x * b.x - a.y * b.y, a.x * b.y + a.y * b.x);
		}

		/// <summary>
		/// Complex number multiplication.
		/// Returns: a * conjugate(b)
		///
		/// Used to rotate vectors in an efficient way.
		///
		/// See: https://en.wikipedia.org/wiki/Complex_number<see cref="Multiplication_and_division"/>
		/// See: https://en.wikipedia.org/wiki/Complex_conjugate
		/// </summary>
		public static TSVector2 ComplexMultiplyConjugate (TSVector2 a, TSVector2 b) {
			return new TSVector2(a.x * b.x + a.y * b.y, a.y * b.x - a.x * b.y);
		}
		
		/// <summary>
		/// Normalize vector and also return the magnitude.
		/// This is more efficient than calculating the magnitude and normalizing separately
		/// </summary>
		public static TSVector Normalize (TSVector v, out FP magnitude) {
			magnitude = v.magnitude;
			// This is the same constant that Unity uses
			if (magnitude > 1E-05f) {
				return v / magnitude;
			} else {
				return TSVector.zero;
			}
		}
	}
}