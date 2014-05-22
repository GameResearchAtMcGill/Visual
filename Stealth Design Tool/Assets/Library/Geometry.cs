using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * An edge between two points (a and b).
 */
public struct Edge3Abs
{
	public Vector3 a;
	public Vector3 b;
	
	public Vector3 rightNormal {
		get {
			return (new Vector3(-(b.z - a.z), 0, b.x - a.x)).normalized;
		}
	}
	
	public Vector3 leftNormal {
		get {
			return (new Vector3(b.z - a.z, 0, -(b.x - a.x))).normalized;
		}
	}
	
	public bool rightOf(Vector3 v)
	{
		return Vector3.Cross(v - a, b - a).y < 0;
	}
	
	public bool leftOf(Vector3 v)
	{
		return Vector3.Cross(v - a, b - a).y > 0;
	}
	
	public Vector3 middle {
		get {
			return 0.5f * (b + a);
		}
	}
	
	public Vector3 closest(Vector3 point)
	{
		// Find point on edge closest to PoV
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
	
	public Vector3 furthest(Vector3 point)
	{
		float da = Vector3.Distance(point, a);
		float db = Vector3.Distance(point, b);
		
		if (da > db) {
			return a;
		} else {
			return b;
		}
	}
	
	public Edge3Abs(Vector3 a, Vector3 b)
	{
		this.a = a;
		this.b = b;
	}

	public Edge3Rel ToRel()
	{
		return new Edge3Rel(a, b - a);
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
		Vector3 p1 = a;
		Vector3 p2 = b;

		Vector3 q1 = other.a;
		Vector3 q2 = other.b;

		float d = (p1.x - p2.x) * (q1.z - q2.z) - (p1.z - p2.z) * (q1.x - q2.x);

		if (d == 0)
			return new Vector3(float.NaN, float.NaN, float.NaN);

		float xi = ((q1.x - q2.x) * (p1.x * p2.z - p1.z * p2.x) - (p1.x - p2.x) * (q1.x * q2.z - q1.z * q2.x)) / d;
		float yi = ((q1.z - q2.z) * (p1.x * p2.z - p1.z * p2.x) - (p1.z - p2.z) * (q1.x * q2.z - q1.z * q2.x)) / d;

		if (xi + 1e-4 < Mathf.Min(p1.x, p2.x) || xi - 1e-4 > Mathf.Max(p1.x, p2.x))
			return new Vector3(float.NaN, float.NaN, float.NaN);
		if (xi + 1e-4 < Mathf.Min(q1.x, q2.x) || xi - 1e-4 > Mathf.Max(q1.x, q2.x))
			return new Vector3(float.NaN, float.NaN, float.NaN);
		if (yi + 1e-4 < Mathf.Min(p1.z, p2.z) || yi - 1e-4 > Mathf.Max(p1.z, p2.z))
			return new Vector3(float.NaN, float.NaN, float.NaN);
		if (yi + 1e-4 < Mathf.Min(q1.z, q2.z) || yi - 1e-4 > Mathf.Max(q1.z, q2.z))
			return new Vector3(float.NaN, float.NaN, float.NaN);

		return new Vector3(xi, a.y, yi);
	}

	public Vector3 IntersectXZ(Edge3Rel other)
	{
		return IntersectXZ(other.ToAbs());
	}
	
	public override bool Equals(object other)
	{
		if (other is Edge3Abs) {
			return (a == ((Edge3Abs)other).a && b == ((Edge3Abs)other).b) ||
			(b == ((Edge3Abs)other).a && a == ((Edge3Abs)other).a);
		} else if (other is Edge3Rel) {
			return (a == ((Edge3Rel)other).pos && b == ((Edge3Rel)other).pos + ((Edge3Rel)other).vec) ||
			(a == ((Edge3Rel)other).pos + ((Edge3Rel)other).vec && b == ((Edge3Rel)other).pos);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}

/**
 * An edge between a point (pos) and its offset (vec)
 */
public struct Edge3Rel
{
	public Vector3 pos;
	public Vector3 vec;

	public Edge3Rel(Vector3 pos, Vector3 vec)
	{
		this.pos = pos;
		this.vec = vec;
	}

	public Edge3Abs ToAbs()
	{
		return new Edge3Abs(pos, pos + vec);
	}

	public Vector3 IntersectXZ(Edge3Abs other)
	{
		return (ToAbs().IntersectXZ(other));
	}

	public Vector3 IntersectXZ(Edge3Rel other)
	{
		return (ToAbs().IntersectXZ(other.ToAbs()));
	}

	public override bool Equals(object other)
	{
		if (other is Edge3Abs) {
			return (pos == ((Edge3Abs)other).a && pos + vec == ((Edge3Abs)other).b) ||
			(pos == ((Edge3Abs)other).b && pos + vec == ((Edge3Abs)other).a);
		} else if (other is Edge3Rel) {
			return (pos == ((Edge3Rel)other).pos && vec == ((Edge3Rel)other).vec) ||
			(pos == ((Edge3Rel)other).pos + vec && vec == -((Edge3Rel)other).vec);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}

public struct IndexEdge
{
	public int i1;
	public int i2;

	public IndexEdge(int a, int b)
	{
		i1 = a;
		i2 = b;
	}

	public Edge3Abs GetEdge(Vector3[] vertices)
	{
		return new Edge3Abs(vertices[i1], vertices[i2]);
	}

	public override bool Equals(object other)
	{
		if (other is IndexEdge) {
			return ((IndexEdge)other).i1 == i1 && ((IndexEdge)other).i2 == i2;
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
	
	public float posX {
		get {
			return position.x;
		}
		set {
			position.x = value;
		}
	}
	
	public float time {
		get {
			return position.y;
		}
		set {
			position.y = value;
		}
	}
	
	public float posZ {
		get {
			return position.z;
		}
		set {
			position.z = value;
		}
	}
	
	public float rotation {
		get {
			return rotationQ.eulerAngles.y;
		}
		set {
			rotationQ = Quaternion.Euler(0, value, 0);
		}
	}
	
	public float velX {
		get {
			return velocity.x;
		}
		set {
			velocity.x = value;
		}
	}
	
	public float velZ {
		get {
			return velocity.z;
		}
		set {
			velocity.z = value;
		}
	}
}

public enum Handedness
{
	Right,
	Left,
	Unknown
}

public struct Circle
{
	public Vector3 center;
	public float radius;
}

public class Shape3: IEnumerable
{
	private List<Vector3> vertices = new List<Vector3>();
	private Handedness hand = Handedness.Unknown;
	
	public Handedness handedness {
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
	
	
	// Sutherland-Hodgman
	public Shape3 Clip(Shape3 convex)
	{
		if (convex.handedness != handedness) {
			convex.Reverse();
		}
		
		Shape3 temp = this;
		Shape3 clipped = new Shape3();
		
		foreach (Edge3Abs boundary in convex) {
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
	
	// Only works in specific cases, not robust!
	public Shape3[] SplitInTwo(Vector3 o, Vector3 dir)
	{
		if (PointInside(o))
			return null;
		
		// Find farthest point
		float farthest = (vertices[0] - o).sqrMagnitude;
		foreach (Vector3 v in vertices) {
			float dst = (v - o).sqrMagnitude;
			if (dst > farthest) {
				farthest = dst;
			}
		}
		
		// Split edge
		Edge3Abs spl = new Edge3Abs(o, o + dir.normalized * Mathf.Sqrt(farthest) * 1.2f);
		
		// New halves
		Shape3 first = new Shape3();
		Shape3 second = new Shape3();
		
		
		int iCls = -1, iFar = -1;
		Vector3 intCls = Vector3.zero, intFar = Vector3.zero, inter = Vector3.zero;
		
		int i = 0;
		foreach (Edge3Abs e in this) {
			inter = e.IntersectXZ(spl);
			
			if (!float.IsNaN(inter.x)) {
				if (iCls == -1) {
					iCls = i;
					intCls = inter;
				} else {
					iFar = i;
					intFar = inter;
					break;
				}
			}
			i++;
		}
		
		// Swap if wrong
		if ((intFar - o).sqrMagnitude < (intCls - o).sqrMagnitude) {
			i = iCls;
			iCls = iFar;
			iFar = i;
			
			inter = intCls;
			intCls = intFar;
			intFar = inter;
		}
		
		// First half
		first.AddVertex(intCls);
		i = (iCls + 1) % vertices.Count;
		while (i != (iFar + 1) % vertices.Count) {
			first.AddVertex(vertices[i]);
			i = (i + 1) % vertices.Count;
		}
		first.AddVertex(intFar);
		first.Offset(-2);
		
		// Second half
		second.AddVertex(intCls);
		second.AddVertex(intFar);
		i = (iFar + 1) % vertices.Count;
		while (i != (iCls + 1) % vertices.Count) {
			second.AddVertex(vertices[i]);
			i = (i + 1) % vertices.Count;
		}
		second.Offset(2);
		
		return new Shape3[]{ first, second };
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
			Debug.Log("NaN");
			return;
		}
		if (float.IsInfinity(point.x) || float.IsInfinity(point.y) || float.IsInfinity(point.z)) {
			Debug.Log("Inf");
			return;
		}
		point.y = 0;
		if (!points.Contains(point)) {
			points.Add(point);
			dirty = true;
		}
	}
	
	public Shape3 ConvexHull()
	{
		// TODO: Infinite loop lurking around here
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
					if (Vector3.Distance(pointOnHull, endpoint) < 1e-4 || leftOfLine(ptList[j], pointOnHull, endpoint)) {
						endpoint = ptList[j];
					}
				}
				pointOnHull = endpoint;
			} while (Vector3.Distance(start, endpoint) >= 1e-4);
			dirty = false;
		}
		
		return hull;
	}
	
	private static Vector3 LeftMost(List<Vector3> points)
	{
		Vector3 leftMost = points[0];
		float leftX = leftMost.x;
		
		foreach (Vector3 p in points) {
			if (p.x < leftX) {
				leftMost = p;
				leftX = p.x;
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
		float x1 = p1.x;
		float y1 = p1.z;
		float x2 = p2.x;
		float y2 = p2.z;
		
		float x3 = p3.x;
		float y3 = p3.z;
		float x4 = p4.x;
		float y4 = p4.z;
		
		float denom = (x1 - x2)*(y3 - y4) - (y1 - y2)*(x3 - x4);
		
		if (denom == 0) {
			return new Vector3(float.NaN, float.NaN, float.NaN);
		}
		
		denom = 1f/denom;
		float x = ((x1 * y2 - y1 * x2)*(x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4))*denom;
		float y = ((x1 * y2 - y1 * x2)*(y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4))*denom;
		
		return new Vector3(x, 0, y);
	}
	
	public static bool isLeft(Vector3 l1, Vector3 l2, Vector3 p) {
		return ((l2.x - l1.x)*(p.z - l1.z) - (l2.z - l1.z)*(p.x - l1.x)) > 0;
	}
}