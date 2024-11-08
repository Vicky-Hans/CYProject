using UnityEngine;

#if USE_FP
using DH.LockStep.Framework;
#else
using FP = System.Single;
using TSVector2 = UnityEngine.Vector2;
using TSVector = UnityEngine.Vector3;
using TSRect = UnityEngine.Rect;
using TSMatrix4x4 = UnityEngine.Matrix4x4;
using DHLogger = UnityEngine.Debug;
using TSMath = UnityEngine.Mathf;
#endif

namespace PathFinding
{
   /// <summary>Helper methods for drawing gizmos and debug lines</summary>
	public class Draw {
		public static readonly Draw Debug = new Draw { gizmos = false };
		public static readonly Draw Gizmos = new Draw { gizmos = true };

		bool gizmos;
		TSMatrix4x4 matrix = TSMatrix4x4.identity;

		void SetColor (Color color) {
			if (gizmos && UnityEngine.Gizmos.color != color) UnityEngine.Gizmos.color = color;
		}

		public void Polyline (System.Collections.Generic.List<TSVector> points, Color color, bool cycle = false) {
			for (int i = 0; i < points.Count - 1; i++) {
				Line(points[i], points[i+1], color);
			}
			if (cycle && points.Count > 1) Line(points[points.Count - 1], points[0], color);
		}

		public void Line (TSVector a, TSVector b, Color color) {
			SetColor(color);
			if (gizmos) UnityEngine.Gizmos.DrawLine(matrix.MultiplyPoint3x4(a), matrix.MultiplyPoint3x4(b));
			else UnityEngine.Debug.DrawLine(matrix.MultiplyPoint3x4(a), matrix.MultiplyPoint3x4(b), color);
		}

		public void CircleXZ (TSVector center, FP radius, Color color, FP startAngle = 0f, FP endAngle = 2*TSMath.PI) {
			int steps = 40;

#if UNITY_EDITOR
			if (gizmos) steps = (int)TSMath.Clamp(TSMath.Sqrt(radius / UnityEditor.HandleUtility.GetHandleSize((UnityEngine.Gizmos.matrix * matrix).MultiplyPoint3x4(center))) * 25, 4, 40);
#endif
			while (startAngle > endAngle) startAngle -= 2*TSMath.PI;

			TSVector prev = new TSVector(TSMath.Cos(startAngle)*radius, 0, TSMath.Sin(startAngle)*radius);
			for (int i = 0; i <= steps; i++) {
				TSVector c = new TSVector(TSMath.Cos(TSMath.Lerp(startAngle, endAngle, i/(FP)steps))*radius, 0, TSMath.Sin(TSMath.Lerp(startAngle, endAngle, i/(FP)steps))*radius);
				Line(center + prev, center + c, color);
				prev = c;
			}
		}

		public void Cylinder (TSVector position, TSVector up, FP height, FP radius, Color color) {
			var tangent = TSVector.Cross(up, TSVector.one).normalized;

			matrix = TSMatrix4x4.TRS(position, Quaternion.LookRotation(tangent, up), new TSVector(radius, height, radius));
			CircleXZ(TSVector.zero, 1, color);

			if (height > 0) {
				CircleXZ(TSVector.up, 1, color);
				Line(new TSVector(1, 0, 0), new TSVector(1, 1, 0), color);
				Line(new TSVector(-1, 0, 0), new TSVector(-1, 1, 0), color);
				Line(new TSVector(0, 0, 1), new TSVector(0, 1, 1), color);
				Line(new TSVector(0, 0, -1), new TSVector(0, 1, -1), color);
			}

			matrix = TSMatrix4x4.identity;
		}

		public void CrossXZ (TSVector position, Color color, FP size = 1) {
			size *= 0.5f;
			Line(position - TSVector.right*size, position + TSVector.right*size, color);
			Line(position - TSVector.forward*size, position + TSVector.forward*size, color);
		}
   }
}