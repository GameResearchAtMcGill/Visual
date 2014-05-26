﻿using System.Linq;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/**
 * An edge between two points (a and b).
 */
public struct Edge3Abs
{
	public Vector3 a;
	public Vector3 b;
	
	public Edge3Abs(Vector3 a, Vector3 b)
	{
		this.a = a;
		this.b = b;
	}
	
	public Vector3 middle { get { return 0.5f * (b + a); } }
	public Vector3 rightNormal { get { return (new Vector3(-(b.z - a.z), 0, b.x - a.x)).normalized; } }
	public Vector3 leftNormal { get { return (new Vector3(b.z - a.z, 0, -(b.x - a.x))).normalized; } }
	
	public bool RightOf(Vector3 v)
	{
		return FPComp.nearlyLessThan(Vector3.Cross(v - a, b - a).y, 0);
	}
	
	public bool LeftOf(Vector3 v)
	{
		return FPComp.nearlyGreaterThan(Vector3.Cross(v - a, b - a).y, 0);
	}
	
	public Vector3 ClosestTo(Vector3 point)
	{
		// Find point on edge closest to the point
		float proj_v = Vector3.Dot(point - a, (b - a).normalized);
		Vector3 closest;
		if (proj_v < 0)
			closest = a;
		else if (proj_v > (b - a).magnitude)
			closest = b;
		else {
			closest = a + proj_v * (b - a).normalized;
		}
		return closest;
	}
	
	public Vector3 FarthestFrom(Vector3 point)
	{
		float da = Vector3.Distance(point, a);
		float db = Vector3.Distance(point, b);
		
		if (da > db) {
			return a;
		} else {
			return b;
		}
	}

	/** Returns the vector from a to b */
	public Vector3 GetDiff()
	{
		return b - a;
	}
	

	/**
	 * Performs an intersection test in 2D on the XZ between two line segments.
	 * Returns:
	 *     The intersection point if there is an intersection
	 *     Vector3(Float.nan, Float.nan, Float.nan) if there is not
	 */
	public Vector3 IntersectXZ(Edge3Abs other)
	{
		
		double p1x = a.x;
		double p1y = a.z;
		double p2x = b.x;
		double p2y = b.z;

		double q1x = other.a.x;
		double q1y = other.a.z;
		double q2x = other.b.x;
		double q2y = other.b.z;

		double d = (p1x - p2x) * (q1y - q2y) - (p1y - p2y) * (q1x - q2x);

		if (d == 0)
			return new Vector3(float.NaN, float.NaN, float.NaN);
		double xi = ((q1x - q2x) * (p1x * p2y - p1y * p2x) - (p1x - p2x) * (q1x * q2y - q1y * q2x)) / d;
		double yi = ((q1y - q2y) * (p1x * p2y - p1y * p2x) - (p1y - p2y) * (q1x * q2y - q1y * q2x)) / d;
		
		
		if (DPComp.nearlyLessThan(xi, Math.Min(p1x, p2x)) || DPComp.nearlyGreaterThan(xi, Math.Max(p1x, p2x)))
			return new Vector3(float.NaN, float.NaN, float.NaN);
		if (DPComp.nearlyLessThan(xi, Math.Min(q1x, q2x)) || DPComp.nearlyGreaterThan(xi, Math.Max(q1x, q2x)))
			return new Vector3(float.NaN, float.NaN, float.NaN);
		if (DPComp.nearlyLessThan(yi, Math.Min(p1y, p2y)) || DPComp.nearlyGreaterThan(yi, Math.Max(p1y, p2y)))
			return new Vector3(float.NaN, float.NaN, float.NaN);
		if (DPComp.nearlyLessThan(yi, Math.Min(q1y, q2y)) || DPComp.nearlyGreaterThan(yi, Math.Max(q1y, q2y)))
			return new Vector3(float.NaN, float.NaN, float.NaN);

		return new Vector3((float)xi, a.y, (float)yi);
	}
	
	public override bool Equals(object other)
	{
		if (other == null) return false;
		if (other is Edge3Abs) {
			return (a == ((Edge3Abs)other).a && b == ((Edge3Abs)other).b) ||
			(b == ((Edge3Abs)other).a && a == ((Edge3Abs)other).a);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}


public struct Pose
{
	public Vector3 position;
	public Quaternion rotationQ;
	public Vector3 velocity;
	public float omega;
	
	public Pose(Vector3 position, Quaternion rotation)
	{
		this.position = position;
		this.rotationQ = rotation;
		velocity = Vector3.zero;
		velocity.y = 1;
		omega = 0;
	}
	
	public Pose(float x, float time, float z, float rotation)
	{
		this.position = new Vector3(x, time, z);
		this.rotationQ = Quaternion.Euler(0, rotation, 0);
		velocity = Vector3.zero;
		velocity.y = 1;
		omega = 0;
	}
	
	public float posX
	{
		get { return position.x; }
		set { position.x = value; }
	}
	
	public float time
	{
		get { return position.y; }
		set { position.y = value; }
	}
	
	public float posZ
	{
		get { return position.z; }
		set { position.z = value; }
	}
	
	public float rotation
	{
		get { return rotationQ.eulerAngles.y; }
		set { rotationQ = Quaternion.Euler(0, value, 0); }
	}
	
	public float velX
	{
		get { return velocity.x; }
		set { velocity.x = value; }
	}
	
	public float velZ
	{
		get { return velocity.z; }
		set { velocity.z = value; }
	}
}

public enum Handedness
{
	Right,
	Left,
	Unknown
}

public class Shape3: IEnumerable
{
	private List<Vector3> vertices = new List<Vector3>();
	private Handedness hand = Handedness.Unknown;
	
	public Handedness handedness
	{
		get {
			if (Count < 3) {
				hand = Handedness.Unknown;
				return hand;
			}
			
			if (hand == Handedness.Unknown) {
				
				float sum = 0;
				
				for (int i = 0; i < Count; i++) {
					Edge3Abs e1 = GetEdge(i);
					Edge3Abs e2 = GetEdge(i + 1);
					
					sum += Vector3.Cross(e1.GetDiff(), e2.GetDiff()).y;
				}
				
				if (sum == 0) {
					hand = Handedness.Unknown;
				} else if (sum < 0) {
					hand = Handedness.Right;
				} else {
					hand = Handedness.Left;
				}
			}
			
			return hand;
		}
	}
	
	/**
	 * Clip this shape so as to stay within a convex clipper using the
	 * Sutherland-Hodgman clipping algorithm and returns the result.
	 */ 
	public Shape3 ClipIn(Shape3 convexClipper)
	{
		if (convexClipper.handedness != handedness) {
			convexClipper.Reverse();
		}
		
		Shape3 temp = this;
		Shape3 clipped = new Shape3();
		
		foreach (Edge3Abs boundary in convexClipper) {
			foreach (Edge3Abs e in temp) {
				Vector3 previous = e.a;
				Vector3 current = e.b;
				
				bool prvIns = Geometry.isLeft(boundary.b, boundary.a, previous);
				bool curIns = Geometry.isLeft(boundary.b, boundary.a, current);
				
				if (prvIns && curIns) {
					clipped.AddVertex(current);
				} else if (prvIns && !curIns) {
					clipped.AddVertex(Geometry.LineIntersectionXZ(previous, current, boundary.a, boundary.b));
				} else if (!prvIns && curIns) {
					clipped.AddVertex(Geometry.LineIntersectionXZ(previous, current, boundary.a, boundary.b));
					clipped.AddVertex(current);
				} else {
					// Do nothing
				}
			}
			temp = clipped;
			clipped = new Shape3();
		}
		
		return temp;
	}
	
	/**
	 * Clip a clipper shape out of this shape using the Weiler-Atherton algorithm
	 * and returns the result. The shapes should be simple polygons.
	 */ 
	public Shape3 ClipOut(Shape3 clipper) {
		int iter = 0;
		// Make the shape clockwise
		if (handedness != Handedness.Left) {
			Reverse();
		}
		// Make the clipper counterclockwise
		if (clipper.handedness != Handedness.Right) {
			
			clipper.Reverse();
		}
		
		int ai = 0, bi = 0;
		Shape3 clipped = new Shape3();
		Vector3 start = vertices[0];
		
		// Traverse the shape to find a vertex outside of the clipper
		while (clipper.PointInside(start)) {
			if (ai >= vertices.Count - 1) {
				return new Shape3();
			}
			start = vertices[++ai];
		}
		
		
		clipped.AddVertex(start);
		
		// a: traversed; b: checked against
		Shape3 a = this;
		Shape3 b = clipper;
		
		Vector3 inter;
		Vector3 current = start;
		Vector3 previous = start;
		bool mayContinue = true;
		do {
			if (iter++ > vertices.Count*2) {
				Debug.LogError("hit max");
				return clipped;
			}
			current = a.vertices[(ai + 1) % a.Count];
			Edge3Abs e = new Edge3Abs(previous, current);
			
			// Find the closest intersection in b
			float closest = float.PositiveInfinity;
			Vector3 closestInter = current;
			int j = 0;
			foreach (Edge3Abs f in b) {
				inter = e.IntersectXZ(f);
				if (!float.IsNaN(inter.x)) {
					if ((previous - inter).sqrMagnitude < closest && inter != previous) {
						closest = (previous - inter).sqrMagnitude;
						closestInter = inter;
						bi = j;
					}
				}
				j++;
			}
			
			// If there has been any intersection, add vertex and change shapes around
			if (!float.IsInfinity(closest)) {
				clipped.AddVertex(closestInter);
				
				Shape3 t = a;
				a = b;
				b = t;
				
				ai = bi;
				
				previous = closestInter;
				mayContinue = true;
			} else {
				// Otherwise add the vertex and carry along
				if (current != start) clipped.AddVertex(current);
				previous = current;
				ai++;
				mayContinue = false;
			}
		} while (current != start || mayContinue);
		// I guess this will be an infinite loop with improper shapes
		
		
		return clipped;
	}
	
	public void Translate(Vector3 t)
	{
		List<Vector3> l = new List<Vector3>();
		foreach (Vector3 v in vertices) {
			l.Add(v + t);
		}
		vertices = l;
	}
	
	public void RotatedScale(Vector3 o, float rotation, Vector3 scale)
	{
		List<Vector3> l = new List<Vector3>();
		
		Quaternion qinv = Quaternion.Euler(0, -rotation, 0);
		Quaternion q = Quaternion.Euler(0, rotation, 0);
		
		Vector3 v2;
		foreach (Vector3 v in vertices) {
			v2 = v - o; // Un-translate
			v2 = qinv * v2; // Un-rotate
			
			// Scale
			v2.x *= scale.x;
			v2.y *= scale.y;
			v2.z *= scale.z;
			
			v2 = q * v2; // Re-rotate
			v2 += o; // Re-translate
			
			l.Add(v2);
		}
		
		vertices = l;
	}
	
	public bool DumbCollision(Shape3 other)
	{
		if (Count > other.Count) {
			return other.DumbCollision(this);
		}
		
		foreach (Vector3 p in vertices) {
			if (other.PointInside(p)) {
				return true;
			}
		}
		return false;
	}
	
	private Vector3[] GetAxes() {
		Vector3[] axes = new Vector3[vertices.Count];
		int i = 0;
		foreach (Edge3Abs e in this) {
			axes[i++] = e.rightNormal.normalized;
		}
		
		return axes;
	}
	
	struct Projection {
		float min, max;
		public Projection(float m, float n) {
			min = m;
			max = n;
		}
		
		public bool Overlap(Projection other) {
			return Mathf.Max(min, other.min) <= Mathf.Min(max, other.max);
		}
	}
	
	private Projection Project(Vector3 axis) {
		float min = Vector3.Dot(axis, vertices[0]);
		float max = min;
		foreach (Vector3 v in vertices) {
			float p = Vector3.Dot(v, axis);
			if (p < min) {
				min = p;
			} else if (p > max) {
				max = p;
			}
		}
		return new Projection(min, max);
	}
	
	/**
	 * Separating Axis Theorem
	 */
	public bool SATCollision(Shape3 convex) {
		// Loop over the axes
		foreach (Vector3[] axes in new []{GetAxes(), convex.GetAxes()}) {
			foreach (Vector3 axis in axes) {
				// Project
				Projection p1 = Project(axis);
				Projection p2 = convex.Project(axis);
				// If there is no overlap, the shapes don't intersect
				if (!p1.Overlap(p2)) {
					return false;
				}
			}
		}
		
		return true;
	}
	
	public bool PerimeterIntersect(Shape3 other) {
		Vector3 inter;
		
		foreach (Edge3Abs e in this) {
			foreach (Edge3Abs f in other) {
				inter = e.IntersectXZ(f);
				if (!float.IsNaN(inter.x)) return true;
			}
		}
		
		return false;
	}
	
	public void AddVertex(Vector3 vert)
	{
		if (vertices.Count == 0 || vertices[vertices.Count - 1] != vert) {
			hand = Handedness.Unknown;
			vertices.Add(vert);
		}
	}

	public void Clear()
	{
		hand = Handedness.Unknown;
		vertices.Clear();
	}

	public void Offset(int offset)
	{
		if (vertices.Count == 0) return;
		offset = ((offset % vertices.Count) + vertices.Count) % vertices.Count;
		List<Vector3> newList = new List<Vector3>();

		for (int i = offset; i - offset < vertices.Count; i++) {
			newList.Add(vertices[i % vertices.Count]);
		}
	
		vertices = newList;
	}

	public Vector3[] Vertices()
	{
		return vertices.ToArray();
	}

	public IEnumerator GetEnumerator()
	{
		for (int i = 0; i < vertices.Count; i++) {
			yield return new Edge3Abs(vertices[i], vertices[(i + 1) % vertices.Count]);
		}
	}

	public IEnumerator GetReverseEnumerator()
	{
		for (int i = vertices.Count; i > 0; i--) {
			yield return new Edge3Abs(vertices[i % vertices.Count], vertices[i - 1]);
		}
	}
	
	public void Reverse()
	{
		vertices.Reverse();
		if (hand == Handedness.Left) {
			hand = Handedness.Right;
		} else if (hand == Handedness.Right) {
			hand = Handedness.Left;
		}
	}

	public Vector3 this[int i] {
		get { return vertices[i]; }
		set { vertices[i] = value; }
	}

	public Edge3Abs GetEdge(int i)
	{
		i = ((i % vertices.Count) + vertices.Count) % vertices.Count;
		return new Edge3Abs(vertices[i], vertices[(i + 1) % vertices.Count]);
	}

	public int Count {
		get {
			return vertices.Count;
		}
	}
	
	public bool PointInside(Vector3 point)
	{
		bool c = false;
		int i = 0, j = 0;

		for (i = 0, j = vertices.Count - 1; i < vertices.Count; j = i++) {
			if (((vertices[i].z > point.z) != (vertices[j].z > point.z)) &&
			    (point.x < (vertices[j].x - vertices[i].x) * (point.z - vertices[i].z) / (vertices[j].z - vertices[i].z) + vertices[i].x))
				c = !c;
		}

		return c;
	}
	
	public float Area {
		get {
			// Find lowerst z for offset
			float lowestZ = float.PositiveInfinity;
			foreach (Vector3 v in vertices) {
				if (v.z < lowestZ) {
					lowestZ = v.z;
				}
			}
			
			float area = 0;
			foreach (Edge3Abs e in this) {
				float subArea = 0.5f * (e.a.z + e.b.z) + lowestZ;
				subArea *= (e.b.x - e.a.x);
				area += subArea;
			}
			
			return Mathf.Abs(area);
		}
	}
}

public class SetOfPoints
{
	public HashSet<Vector3> points;
	Shape3 hull;
	bool dirty;
	
	public SetOfPoints()
	{
		points = new HashSet<Vector3>();
		hull = new Shape3();
		dirty = true;
	}
	
	public void AddPoint(Vector3 point)
	{
		if (float.IsNaN(point.x) || float.IsNaN(point.y) || float.IsNaN(point.z)) {
			return;
		}
		if (float.IsInfinity(point.x) || float.IsInfinity(point.y) || float.IsInfinity(point.z)) {
			return;
		}
		// Rounding to avoid the infinite loop
		point.x = Mathf.Round(point.x * 100)*0.01f;
		point.y = 0;
		point.z = Mathf.Round(point.z * 100)*0.01f;
		if (!points.Contains(point)) {
			points.Add(point);
			dirty = true;
		}
	}
	
	public Shape3 ConvexHull()
	{
		if (dirty) {
			List<Vector3> ptList = points.ToList();
			hull.Clear();
			
			if (ptList.Count == 0) {
				return hull;
			} else if (ptList.Count == 1) {
				hull.AddVertex(ptList[0]);
				return hull;
			}
			
			Vector3 pointOnHull = LeftMost(ptList);
			Vector3 endpoint;
			Vector3 start = pointOnHull;
			
			do {
				hull.AddVertex(pointOnHull);
				endpoint = ptList[0];
				for (int j = 1; j < ptList.Count; j++) {
					if (pointOnHull == endpoint || leftOfLine(ptList[j], pointOnHull, endpoint)) {
						endpoint = ptList[j];
					}
				}
				pointOnHull = endpoint;
			} while ((start - endpoint).sqrMagnitude > 2e-3f);
			dirty = false;
		}
		
		return hull;
	}
	
	private static Vector3 LeftMost(List<Vector3> points)
	{
		Vector3 leftMost = points[0];
		
		foreach (Vector3 p in points) {
			if (p.x < leftMost.x) {
				leftMost = p;
			} else if (p.x == leftMost.x) {
				if (p.z < leftMost.z) {
					leftMost = p;
				}
			}
		}
		
		return leftMost;
	}
	
	private static bool leftOfLine(Vector3 p2, Vector3 p0, Vector3 p1)
	{
		return (p1.x - p0.x) * (p2.z - p0.z) - (p2.x - p0.x) * (p1.z - p0.z) > 0;
	}
}

public static class Geometry {
	public static Vector3 LineIntersectionXZ(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4) {
		double x1 = p1.x;
		double y1 = p1.z;
		double x2 = p2.x;
		double y2 = p2.z;
		
		double x3 = p3.x;
		double y3 = p3.z;
		double x4 = p4.x;
		double y4 = p4.z;
		
		double denom = (x1 - x2)*(y3 - y4) - (y1 - y2)*(x3 - x4);
		
		if (denom == 0) {
			return new Vector3(float.NaN, float.NaN, float.NaN);
		}
		
		double x = ((x1 * y2 - y1 * x2)*(x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4))/denom;
		double y = ((x1 * y2 - y1 * x2)*(y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4))/denom;
		
		return new Vector3((float)x, 0, (float)y);
	}
	
	public static bool isLeft(Vector3 l1, Vector3 l2, Vector3 p) {
		return ((l2.x - l1.x)*(p.z - l1.z) - (l2.z - l1.z)*(p.x - l1.x)) > 0;
	}
}

public static class FPComp {
	const float EPSILON = 1e-5f;
	
	/** a == b */
	public static bool nearlyEqual(float a , float b, float epsilon=EPSILON) {
		float absA = Mathf.Abs(a);
		float absB = Mathf.Abs(b);
		float diff = Mathf.Abs(a - b);
		
		if (a == b) {
			return true;
		}
		if (a == 0 || b == 0 || diff < float.MinValue) {
			return diff < (epsilon * float.MinValue);
		}
		return diff / (absA + absB) < epsilon;
	}
	
	/** a &lt; b */
	public static bool nearlyLessThan(float a, float b, float epsilon=EPSILON) {
		return !nearlyEqual(a, b, epsilon) && a < b;
	}
	
	/** a &lt;= b */
	public static bool nearlyLessOrEqual(float a, float b, float epsilon=EPSILON) {
		return nearlyEqual(a, b, epsilon) && a < b;
	}
	
	/** a &gt; b */
	public static bool nearlyGreaterThan(float a, float b, float epsilon=EPSILON) {
		return !nearlyEqual(a, b, epsilon) && a > b;
	}
	
	/** a &gt;= b */
	public static bool nearlyGreaterOrEqual(float a, float b, float epsilon=EPSILON) {
		return nearlyEqual(a, b, epsilon) && a > b;
	}
}

public static class DPComp {
	const double EPSILON = 1e-8f;
	
	/** a == b */
	public static bool nearlyEqual(double a , double b, double epsilon=EPSILON) {
		double absA = Math.Abs(a);
		double absB = Math.Abs(b);
		double diff = Math.Abs(a - b);
		
		if (a == b) {
			return true;
		}
		if (a == 0 || b == 0 || diff < double.MinValue) {
			return diff < (epsilon * double.MinValue);
		}
		return diff / (absA + absB) < epsilon;
	}
	
	/** a &lt; b */
	public static bool nearlyLessThan(double a, double b, double epsilon=EPSILON) {
		return !nearlyEqual(a, b, epsilon) && a < b;
	}
	
	/** a &lt;= b */
	public static bool nearlyLessOrEqual(double a, double b, double epsilon=EPSILON) {
		return nearlyEqual(a, b, epsilon) && a < b;
	}
	
	/** a &gt; b */
	public static bool nearlyGreaterThan(double a, double b, double epsilon=EPSILON) {
		return !nearlyEqual(a, b, epsilon) && a > b;
	}
	
	/** a &gt;= b */
	public static bool nearlyGreaterOrEqual(double a, double b, double epsilon=EPSILON) {
		return nearlyEqual(a, b, epsilon) && a > b;
	}
}