using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Class1 : MonoBehaviour {
	private Map map_;
	public Map map {
		get {
			if (map_ == null) {
				map_ = gameObject.transform.parent.gameObject.GetComponent<Map>();
			}
			return map_;
		}
	}
	
	float rotation {
		get {
			return transform.rotation.eulerAngles.y;
		}
	}
	
	
	private SortedList<PolarCoord, Edge3Abs> list2_;
	public SortedList<PolarCoord, Edge3Abs> list2 {
		get {
			if (list2_ == null) {
				list2_ = new SortedList<PolarCoord, Edge3Abs>();
			}
			return list2_;
		}
	}
	
	public Vector3 twoDim {
		get {
			return new Vector3(transform.position.x, 0, transform.position.z);
		}
	}
	
	public int ind = 0;
	
	public float fov = 60.0f;
	public float smallFov {
		get { return fov / segments; }
	}
	
	public float viewDist = 10.0f;
	public float smallDist {
		get { return Mathf.Cos(Mathf.Deg2Rad * smallFov*0.5f) * viewDist; }
	}
	
	public int segments = 1;
	public float scaleArrows = 1;
	
	private SortedList<PolarCoord, Edge3Abs> slist_;
	public SortedList<PolarCoord, Edge3Abs> slist {
		get {
			if (slist_ == null) {
				slist_ = new SortedList<PolarCoord, Edge3Abs>();
			}
			return slist_;
		}
	}
	
	public bool toggleGreen = true;
	public bool toggleYellow = true;
	public bool toggleRed = true;
	public bool toggleBlue = true;
	
	void Awake()
	{
		name = "Test";
	}
	
	void Update()
	{
		Vector3 pos = transform.position;
		pos.y = 0;
		transform.position = pos;
		
		if (viewDist < 0) {
			viewDist = 0;
		}
		
		if (segments < 1) {
			segments = 1;
		}
		
		if (fov > 359) {
			fov = 359;
		}
		
		if (fov < 1) {
			fov = 1;
		}
	}
	
	public Shape3 Shape() {
		Shape3 s = new Shape3();
		
		s.AddVertex(twoDim);
		
		float angle = -transform.rotation.eulerAngles.y + fov * 0.5f;
		float step = fov / segments;
		
		for (int i = 0; i < segments; i++) {
			s.AddVertex(new Vector3(twoDim.x + Mathf.Cos(angle * Mathf.Deg2Rad)*viewDist, 0, twoDim.z + Mathf.Sin(angle * Mathf.Deg2Rad)*viewDist));
			angle -= step;
		}
		s.AddVertex(new Vector3(twoDim.x + Mathf.Cos(angle * Mathf.Deg2Rad)*viewDist, 0, twoDim.z + Mathf.Sin(angle * Mathf.Deg2Rad)*viewDist));
		
		return s;
	}
	
	bool processEdge(Edge3Abs e, Shape3 thisShape) {
		// Ignore edge if facing away
		if (Vector3.Cross(e.b - twoDim, e.b - e.a).y < 0) {
			return false;
		}
		
		Vector3 closest = e.closest(twoDim);
		
		// Ignore edge if closest point farther than view distance
		if (Vector3.Distance(closest, twoDim) > viewDist)
			return false;
		else {
			// Find angle of closest point
			Vector3 diff = Quaternion.Euler(0, -rotation, 0)  * (closest - twoDim);
			float angle = - Mathf.Atan2(diff.z, diff.x) * Mathf.Rad2Deg + fov * 0.5f;
			
			// Find out in which slice of the FoV it belongs
			int index;
			if (angle < 0) {
				index = 0;
			} else if (angle > fov) {
				index = segments + 1;
			} else {
				index = Mathf.FloorToInt(angle / fov * segments) + 1;
			}
			
			// Get the edge of the slice in the FoV 
			Edge3Abs e3 = thisShape.GetEdge(index);
			
			Gizmos.color = Color.magenta;
			Gizmos.DrawCube(e3.middle, Vector3.one * 2 * scaleArrows);
			Gizmos.color = Color.green;
			
			// If the edges do not intersect, further inspection is needed
			if (float.IsNaN(e3.IntersectXZ(e).x)) {
				if (index == 0 || index == segments + 1 || Vector3.Distance(closest, twoDim) > smallDist) {
					if (index == 0) {
						e3 = thisShape.GetEdge(segments + 1);
						if (float.IsNaN(e3.IntersectXZ(e).x)) {
							return false;
						}
					} else if (index == segments + 1) {
						e3 = thisShape.GetEdge(0);
						if (float.IsNaN(e3.IntersectXZ(e).x)) {
							return false;
						}
					} else {
						return false;
					}
				}
			// If they do intersect, it is fine
			} else {
				Gizmos.DrawCube(e3.IntersectXZ(e), Vector3.one * 2 * scaleArrows);
			}
		}
		
		if (toggleGreen) {
			Gizmos.DrawLine(twoDim, twoDim + (e.a - twoDim).normalized * Mathf.Max(viewDist, (e.a - twoDim).magnitude));
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(twoDim, twoDim + (e.b - twoDim).normalized * Mathf.Max(viewDist, (e.b - twoDim).magnitude));
		}
		
		return true;
	}
	
	public void testEdge(Edge3Abs e, Shape3 s) {
		// Split the edge in two if crosses over the back of the FoV
		Edge3Abs e2;
		e2.a = twoDim;
		e2.b = twoDim + Quaternion.Euler(0, rotation, 0) * new Vector3(-e.furthest(twoDim).sqrMagnitude, 0, 0);
		Gizmos.color = Color.gray;
		Gizmos.DrawLine(e2.a, e2.b);
		Gizmos.color = Color.green;
		
		Vector3 inter = e2.IntersectXZ(e);
		if (!float.IsNaN(inter.x)) {
			Edge3Abs e1;
			e1.a = twoDim + Quaternion.Euler(0, 0.5f, 0) * (inter - twoDim);
			e1.b = e.b;
			e2.a = e.a;
			e2.b = twoDim + Quaternion.Euler(0, -0.5f, 0) * (inter - twoDim);
			
			// Process the two new edges
			if (processEdge(e1, s)) {
				slist.Add(new PolarCoord(twoDim, e1, Quaternion.Euler(0, -rotation, 0)), e1);
			}
			if (processEdge(e2, s)) {
				slist.Add(new PolarCoord(twoDim, e2, Quaternion.Euler(0, -rotation, 0)), e2);
			}
			
		} else {
			// Process the edge
			if (processEdge(e, s)) {
				slist.Add(new PolarCoord(twoDim, e, Quaternion.Euler(0, -rotation, 0)), e);
			}
		}
	}
	
	void OnDrawGizmos()
	{
		slist.Clear();
		
		Gizmos.color = Color.yellow;
		Shape3 thisShape = Shape();
		// Add the edges of the fov
		foreach(Edge3Abs e in thisShape) {
			slist.Add(new PolarCoord(twoDim, e, Quaternion.Euler(0, -rotation, 0)), e);
			if (toggleYellow) {
				Gizmos.DrawLine(twoDim, e.a);
				Gizmos.DrawLine(twoDim, e.b);
			}
		}
		Edge3Abs f = new Edge3Abs(twoDim, twoDim + Quaternion.Euler(0, rotation -1, 0) * new Vector3(-viewDist, 0, 0));
		slist.Add(new PolarCoord(twoDim, f, Quaternion.Euler(0, -rotation, 0)), f);
		
		Gizmos.color = Color.green;
		// For each obstacle
		foreach (StealthObstacle so in map.GetObstacles()) {
			// Broad phase with bounding circles
			if (Vector3.Distance(twoDim, so.position) > viewDist + so.radius) {
				continue;
			}
			
			Gizmos.DrawWireSphere(so.position, so.radius);
			
			Shape3 shape = so.GetShape();
			// PoV in obstacle -> Consider all edges
			if (shape.PointInside(twoDim)) {
				foreach (Edge3Abs e in so.GetShape()) {
					testEdge(e, thisShape);
				}
			} else {
				foreach (Edge3Abs e in so.GetShape()) {
					Gizmos.color = Color.white;
					Gizmos.DrawLine(e.a, e.b);
					Gizmos.color = Color.green;
					
					Edge3Abs e2 = new Edge3Abs(e.b, e.a);
					
					testEdge(e2, thisShape);
				}
			}
		}
		
		Gizmos.DrawSphere(twoDim, 1);
		
		// Vision Algorithm
		Gizmos.color = Color.black;
		thisShape.Clear();
		thisShape.AddVertex(twoDim);
		list2.Clear();
		KeyValuePair<PolarCoord, Edge3Abs>? curr = null;
		//IEnumerator<KeyValuePair<PolarCoord, Edge3Abs>> en = slist.GetEnumerator();
		// TODO: Make this into a while loop
		foreach (KeyValuePair<PolarCoord, Edge3Abs> kv in slist) {
			// Remove any edge that are past, from list2
			while(list2.Count > 0 && curr != null && list2.Keys[0].angle1 <= curr.Value.Key.angle1 + 1e-4) {
				list2.RemoveAt(0);
			}
			
			if (kv.Key.angle1 * Mathf.Rad2Deg < -fov * 0.5 - 1e-4) {
				// Add inverted edge in list2
				list2.Add(kv.Key.GetReverse(), kv.Value);
				continue;
			}
			if (curr == null) {
				curr = kv;
				continue;
			}
			if (kv.Key.angle2 < curr.Value.Key.angle1) {
				list2.Add(kv.Key.GetReverse(), kv.Value);
				continue;
			}
			
			if (curr.Value.Key.angle2 <= kv.Key.angle1) {
				
				// Got at the end of curr before the new edge
				
				// list2 is empty -> Direct transition
//				if (list2.Count == 0) {
//					thisShape.addVertex(curr.Value.Value.b);
//					curr = kv;
//					continue;
//				}
				
				// Intersection test with all the edges in list2 to find any missed intersection
				Vector3 closest = curr.Value.Value.b;
				KeyValuePair<PolarCoord, Edge3Abs>? kvInter = null;
				foreach(KeyValuePair<PolarCoord, Edge3Abs> kv2 in list2) {
					Vector3 inter = kv2.Value.IntersectXZ(curr.Value.Value);
					if (!float.IsNaN(inter.x)) {
						Gizmos.DrawSphere(inter, 1);
						if ((inter - twoDim).sqrMagnitude < (closest - twoDim).sqrMagnitude) {
							closest = inter;
							kvInter = kv2;
						}
					}
				}
				
				
				
				// If no intersection occured, add end of curr, project to the edge that is closest
				if (kvInter == null) {
					// Unless kv is directly next
					if (curr.Value.Value.b == kv.Value.a) {
						thisShape.AddVertex(curr.Value.Value.b);
						Gizmos.DrawSphere(curr.Value.Value.b, 1);
						list2.Add(kv.Key.GetReverse(), kv.Value);
						curr = kv;
						continue;
					}
					
					thisShape.AddVertex(curr.Value.Value.b);
					
					if (list2.Count > 0) {
						closest = twoDim + (curr.Value.Value.b - twoDim).normalized * viewDist * 2f;
						Edge3Abs e = new Edge3Abs(twoDim, closest);
						foreach(KeyValuePair<PolarCoord, Edge3Abs> kv2 in list2) {
							Vector3 inter = kv2.Value.IntersectXZ(e);
							if (!float.IsNaN(inter.x)) {
								if ((inter - twoDim).sqrMagnitude < (closest - twoDim).sqrMagnitude) {
									closest = inter;
									kvInter = kv2;
								}
							}
						}
						
						if (closest != e.b) {
							// Pathos
							thisShape.AddVertex(closest);
							curr = new KeyValuePair<PolarCoord, Edge3Abs>(kvInter.Value.Key.GetReverse(), kvInter.Value.Value);
							continue;
						} else {
							curr = kv;
						}
					} else {
						curr = kv;
					}
				// Otherwise, add point at intersection, consider the new edge as curr
				} else {
					thisShape.AddVertex(closest);
					curr = new KeyValuePair<PolarCoord, Edge3Abs>(kvInter.Value.Key.GetReverse(), kvInter.Value.Value);
				}
				
				// Add the new edge in list2
				list2.Add(kv.Key.GetReverse(), kv.Value);
				
			} else {
				// Still on curr, but a wild edge appeared (kv)
				Gizmos.DrawCube(kv.Value.a, Vector3.one);
				// If the new edge is closer than curr, project from it
				if (curr.Value.Value.rightOf(kv.Value.a)) {
					Edge3Abs e = new Edge3Abs(twoDim, twoDim + (kv.Value.a - twoDim).normalized * viewDist * 2f);
					thisShape.AddVertex(e.IntersectXZ(curr.Value.Value));
					
					//Gizmos.DrawSphere(kv.Value.a, 1);
					thisShape.AddVertex(kv.Value.a);
					
					//list2.Add(curr.Value.Key.GetReverse(), curr.Value.Value);
					
					// Consider the new edge as curr
					curr = kv;
				// If the edge is farther thar curr, put it in list2
				} else {
					Vector3 inter = kv.Value.IntersectXZ(curr.Value.Value);
					
					// No intersection
					if (float.IsNaN(inter.x)) {
						list2.Add(kv.Key.GetReverse(), kv.Value);
					// Intersection -> Add vertex, go to kv
					} else {
						thisShape.AddVertex(inter);
						curr = kv;
					}
				}
			}
		}
		
		// Draw the result
		int i = 0;
		foreach (Edge3Abs e in thisShape) {
			Gizmos.DrawLine(e.a, e.b);
			if (i == ind) {
				Gizmos.color = Color.red;
				Gizmos.DrawSphere(e.a, 1);
				Gizmos.color = Color.black;
			}
			i++;
		}
		
		
		// Draw the problem space
		i = 0;
		Gizmos.color = Color.red;
		foreach (KeyValuePair<PolarCoord, Edge3Abs> kv in slist) {
			
			Edge3Abs e = kv.Value;
			//PolarCoord pc = kv.Key;
			
			if (toggleRed) {
				if (i++ == (ind/20) % slist.Count) {
					Gizmos.DrawSphere(e.a, 2*scaleArrows);
					Gizmos.DrawSphere(e.b, 2*scaleArrows);
				}
				
				Gizmos.DrawLine(e.a, e.b);
			}
			Vector3 ori = e.middle - e.GetDiff().normalized*2.5f*scaleArrows;
			float angle = Mathf.Atan2(e.b.z - e.a.z, e.b.x - e.a.x);
			Quaternion q = Quaternion.Euler(0, 90- angle*Mathf.Rad2Deg, 0);
			
			if (toggleRed) {
				Handles.color = Color.red;
				Handles.ArrowCap(0, ori, q, 5 * scaleArrows);
			}
			q = Quaternion.Euler(0, 90 - Mathf.Atan2(e.rightNormal.z, e.rightNormal.x) * Mathf.Rad2Deg, 0);
			
			if (toggleBlue) {
				Handles.color = Color.blue;
				Handles.ArrowCap(0, e.middle, q, 3 * scaleArrows);
			}
		}
	}
}

public struct PolarCoord : IComparable<PolarCoord> {
	public float angle1;
	public float distance1;
	public float angle2;
	public float distance2;
	
	public PolarCoord(Vector3 origin, Edge3Abs e, Quaternion q) {
		Vector3 diff = q * (e.a - origin);
		if (diff != Vector3.zero) {
			angle1 = -Mathf.Atan2(diff.z, diff.x);
		} else {
			angle1 = float.NaN;
		}
		
		distance1 = diff.magnitude;
		diff = q * (e.b - origin);
		angle2 = -Mathf.Atan2(diff.z, diff.x);
		if (float.IsNaN(angle1)) {
			angle1 = angle2;
		}
		distance2 = diff.magnitude;
	}
	
	public int CompareTo(PolarCoord other) {
		int me = Mathf.RoundToInt(angle1*1000000);
		int you = Mathf.RoundToInt(other.angle1*1000000);
		if (me != you) {
			return me - you;
		} else {
			me = Mathf.RoundToInt(distance1*1000000);
			you = Mathf.RoundToInt(other.distance1*1000000);
			return me - you;
		}
	}
	
	public PolarCoord GetReverse() {
		PolarCoord r = this;
		float temp = r.angle2;
		r.angle2 = r.angle1;
		r.angle1 = temp;
		temp = r.distance2;
		r.distance2 = r.distance1;
		r.distance1 = temp;
		return r;
	}
}