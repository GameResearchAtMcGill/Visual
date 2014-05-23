﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public abstract class StealthFov : MeshMapChild {
	public static bool debug = true;
	
	public float viewDist_ = 30f;
	public float fieldOfView_ = 50f;
	public int frontSegments_ = 8;
	
	public SetOfPoints setOfPoints = new SetOfPoints();
	
	public float viewDistance {
		get { return viewDist_; }
		set {
			if (value != viewDist_) {
				dirty = true;
				viewDist_ = value;
				Validate();
			}
		}
	}
	
	public float fieldOfView {
		get { return fieldOfView_; }
		set {
			if (value != fieldOfView_) {
				dirty = true;
				fieldOfView_ = value;
				Validate();
			}
		}
	}
	
	public int frontSegments {
		get { return frontSegments_; }
		set {
			if (value != frontSegments_) {
				dirty = true;
				frontSegments_ = value;
				Validate();
			}
		}
	}
	
	private List<Shape3> shapes_ = null;
	private List<Shape3> shapes {
		get {
			if (shapes_ == null || dirty) {
				shapes_ = Shapes();
			}
			return shapes_;
		}
	}
	
	public Shape3 convexHull {
		get {
			return setOfPoints.ConvexHull();
		}
	}
	
	public float easiness {
		get {
			return 1.0f;
			float volume = convexHull.Area * map.timeLength;
			return (volume - vlm_)/volume;
		}
	}
	
	private float vlm_ = 0;
	public float shVolume {
		get {
			return vlm_;
		}
	}
	
	new protected void Awake() {
		base.Awake();
		
		gameObject.name = "Field of view";
		
		Material mat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/ShadowVolumeMat.mat", typeof(Material));
		gameObject.renderer.material = mat;
	}
	
	public override void MapChanged()
	{
		
	}
	
	public void OnDrawGizmos()
	{
		if (!debug) {
			return;
		}
		foreach (Edge3Abs e in setOfPoints.ConvexHull()) {
			Gizmos.DrawLine(e.a, e.b);
		}
		
		foreach (IObstacle so in map.GetObstacles()) {
			
			if (fieldOfView_ > 180)
				Gizmos.DrawLine(position, position + rotationQ * new Vector3(-viewDist_, 0, 0));
			
			
			bool cont = true;
			foreach (Edge3Abs e in so.GetShape()) {
				if (Vector3.Distance(e.closest(new Vector3(position.x, 0, position.z)), new Vector3(position.x, 0, position.z)) <= viewDist_) {
					cont = false;
					break;
				}
			}
			if (cont || Vector3.Distance(so.position, position) > viewDistance + Mathf.Sqrt(so.sizeX*so.sizeX+so.sizeZ*so.sizeZ)*0.5f) {
				Gizmos.color = new Color32(0, 255, 128, 255);
			} else {
				Gizmos.color = new Color32(255, 128, 0, 255);
			}
			Shape3 sp = so.ShadowPolygon(position, viewDistance);
			Gizmos.DrawSphere(sp[0], 1);
			foreach (Edge3Abs e in sp) {
				Gizmos.DrawLine(e.a, e.b);
			}
			
			if (!cont && !so.GetShape().PointInside(position) && fieldOfView_ > 180) {
				Gizmos.color = Color.red;
				foreach (Shape3 sp2 in sp.SplitInTwo(position, rotationQ * new Vector3(-1, 0, 0))) {
					foreach (Edge3Abs e in sp2) {
						Gizmos.DrawLine(e.a, e.b);
					}
				}
			}
			
		}
	}
	
	public override void CreateMesh()
	{
		Mesh m = new Mesh();
		m.name = "FoV mesh";
		mf.sharedMesh = m;
		UpdateMesh ();
	}
	
	public override void UpdateMesh()
	{
		if (map == null)
			return;
		List<Vector3> vertexList = new List<Vector3> ();
		
		float timeStep = map.timeLength / Mathf.FloorToInt((map.timeLength) * map.sub_);

		vlm_ = 0;
		setOfPoints.points.Clear();
		foreach (Shape3 s in shapes) {
			// Add the vertices in the set of points
			foreach (Vector3 v3 in s.Vertices()) {
				setOfPoints.AddPoint(new Vector3(v3.x, 0, v3.z));
			}
			
			vlm_ += s.Area / map.subdivisionsPerSecond;
			
			foreach (Edge3Abs e in s) {
				vertexList.Add(e.a);
			}
			foreach (Edge3Abs e in s) {
				vertexList.Add(e.a + new Vector3(0, timeStep, 0));
			}
		}

		Mesh m = mf.sharedMesh;

		m.Clear ();

		m.vertices = vertexList.ToArray ();

		List<int> triangles = new List<int> ();

		int v = 0;
		int sh = 0;
		int maxS = shapes.Count;
		foreach (Shape3 s in shapes) {
			sh++;
			int count = s.Count;

			// Bottom
			for (int i = 1; i < count - 1; i++) {
				triangles.Add(v);
				triangles.Add(v + count - i);
				triangles.Add(v + count - i - 1);
			}

			// Top
			if (sh <= maxS) {
				for (int i = 1; i < count - 1; i++) {
					triangles.Add(v + count);
					triangles.Add(v + count +i);
					triangles.Add(v + count +i+1);
				}
			}
			
			// Sides
			for (int i = 0; i < count; i++) {
				if (sh <= maxS) {
					if (i < count - 1) {
						triangles.Add (v + i + count);
						triangles.Add (v + i);
						triangles.Add (v + i + count + 1);
						
						triangles.Add (v + i + count + 1);
						triangles.Add (v + i);
						triangles.Add (v + i + 1);
					} else {
						triangles.Add (v + i + count);
						triangles.Add (v + i);
						triangles.Add (v + count);
						
						triangles.Add (v + i - i + count);
						triangles.Add (v + i);
						triangles.Add (v + i - i);
					}
				}
				
			}
			v+= 2* count;
		}

		m.triangles = triangles.ToArray ();

		m.uv = new Vector2[mf.sharedMesh.vertices.Length];

		m.RecalculateNormals ();

		mf.sharedMesh = m;
	}
	
	public abstract List<Shape3> Shapes();
	
	private float ToTheta(Vector3 p) {
		Vector3 diff = new Vector3(p.x - position.x, 0, p.z - position.z);
		diff = Quaternion.Euler(0, -rotation, 0) * diff;
		
		return Mathf.Atan2(diff.z, diff.x);
	}
	
	private float ToDist(Vector3 p) {
		return new Vector3(position.x - p.x, 0, position.z - p.z).magnitude;
	}
	
	public Shape3 Occlude(Vector3 position, float rotation) {
		//TODO: This is quite slow
		
		Shape3 vision_ = new Shape3 ();
		foreach (Edge3Abs e in Vertices(position, rotation)) {
			vision_.AddVertex(e.a);
		}

		// Left handed iterator of the vision shape
		IEnumerator visionIteratorLH = vision_.GetEnumerator ();
		List<Shape3> clipping = new List<Shape3>();
		foreach (IObstacle o in map.GetObstacles()) {
			
			// Very broad phase
			if (Vector3.Distance(o.position, new Vector3(position.x, 0, position.z)) > viewDist_ + Mathf.Sqrt(o.sizeX*o.sizeX+o.sizeZ*o.sizeZ)*0.5f) {
				continue;
			}
			
			// Not-so-broad phase
			bool cont = true;
			foreach (Edge3Abs e in o.GetShape()) {
				if (Vector3.Distance(e.closest(new Vector3(position.x, 0, position.z)), new Vector3(position.x, 0, position.z)) <= viewDist_) {
					cont = false;
					break;
				}
			}
			if (cont) continue;
			
			Shape3[] shadows = null;
			// Treat an obstacle surrounding the fov as 4 obstacles
			if (o.GetShape().PointInside(position)) {
				clipping.Add(o.GetShape());
			} else {
				shadows = new []{o.ShadowPolygon(position, viewDist_)};
				
				// Split the obstacle in two, if it collide more than once
				if (fieldOfView_ > 180) {
					bool shouldSplit = false;
					
					Edge3Abs back = new Edge3Abs(position, position + (Quaternion.Euler(0, rotation, 0) * new Vector3(-viewDist_, 0, 0)));
					Vector3 inter;
					
					foreach (Edge3Abs e in o.GetShape()) {
						inter = e.IntersectXZ(back);
						if (!float.IsNaN(inter.x)) {
							shouldSplit = true;
							break;
						}
					}
					
					if (shouldSplit) {
						Shape3[] temp = o.ShadowPolygon(position, viewDist_).SplitInTwo(position, Quaternion.Euler(0, rotation, 0) * new Vector3(-1*viewDist_, 0, 0));
						if (temp != null)
							shadows = temp;
						else {
							Debug.Log("null");
						}
					}
				}
				
				
			}
			
			if (shadows == null) {
				continue;
			}
			
			foreach (Shape3 shadow in shadows) {
				if (shadow.PointInside(position)) {
					vision_ = vision_.Clip(shadow);
					
					int offset = -1;
					for (int i = 0; i < vision_.Count; i++) {
						Vector3 v = vision_[i];
						v.y = position.y;
						vision_[i] = v;
						
						if (v == position) {
							offset = i;
						}
					}
					// Clip will stride the shape, but the center should be at 0
					vision_.Offset(offset);
					
					continue;
				}
				
				// Occluded shape
				Shape3 occludedLeft = new Shape3 ();
				
				// Right handed iterator of the shadow polygon
				IEnumerator shadowIterator;
	
				Edge3Abs intersecting = new Edge3Abs(Vector3.zero, Vector3.zero);
				while(visionIteratorLH.MoveNext()) {
					Edge3Abs e = (Edge3Abs)visionIteratorLH.Current;
	
					occludedLeft.AddVertex(e.a);
	
					shadowIterator = shadow.GetReverseEnumerator();
					float distance = float.PositiveInfinity;
					while(shadowIterator.MoveNext()) {
	
						Edge3Abs se = (Edge3Abs)shadowIterator.Current;
						Vector3 intersection;
						if (!float.IsNaN((intersection = e.IntersectXZ(se)).x)) {
							if (Vector3.Distance(e.a, intersection) < distance) {
								distance = Vector3.Distance(e.a, intersection);
								e.b = intersection;
								occludedLeft.AddVertex(e.b);
								intersecting = se;
								intersecting.b = e.b;
							}
						}
					}
					if (intersecting.a != intersecting.b) {
						break;
					}
				}
				
				// If an intersection occured
				if (intersecting.a != intersecting.b) {
					// Traverse the shadow shape right-handedly up to the intersection segment
					shadowIterator = shadow.GetReverseEnumerator();
					int offset = -1;
					while (shadowIterator.MoveNext()) {
						if (((Edge3Abs) shadowIterator.Current).b == intersecting.a) {
							break;
						}
	
						offset -= 1;
					}
					shadow.Offset(offset);
					shadowIterator = shadow.GetReverseEnumerator();
	
					shadowIterator = shadow.GetReverseEnumerator();
					while (shadowIterator.MoveNext()) {
						Edge3Abs se = (Edge3Abs) shadowIterator.Current;
	
						visionIteratorLH = vision_.GetEnumerator();
						bool intersect = false;
						while (visionIteratorLH.MoveNext()) {
							Edge3Abs e = (Edge3Abs) visionIteratorLH.Current;
							Vector3 intersection = e.IntersectXZ(se);
							if (!float.IsNaN (intersection.x) && intersecting.b != intersection) {
								se.b = intersection;
								occludedLeft.AddVertex(se.b);
								intersect= true;
								break;
							}
						}
	
						if (intersect)
							break;
						
						occludedLeft.AddVertex (se.b);
					}
					
					while(visionIteratorLH.MoveNext()) {
						Edge3Abs e = (Edge3Abs)visionIteratorLH.Current;
	
						occludedLeft.AddVertex (e.a);
					}
				}
				
				vision_.Clear();
				foreach (Edge3Abs e in occludedLeft) {
					vision_.AddVertex(e.a);
				}
				visionIteratorLH = vision_.GetEnumerator ();
			}
		}
		
		foreach (Shape3 clip in clipping) {
			vision_ = vision_.Clip(clip);
		}
		
		if (clipping.Count > 0) {
			int offset = -1;
			for (int i = 0; i < vision_.Count; i++) {
				Vector3 v = vision_[i];
				v.y = position.y;
				vision_[i] = v;
				if (v == position) {
					offset = i;
				}
			}
			vision_.Offset(offset);
		}
		
		return vision_;
	}
	
	public Shape3 Vertices(Vector3 position, float rotation) {
		if (frontSegments < 1) {
			frontSegments = 1;
		}

		Shape3 shape = new Shape3 ();
		shape.AddVertex (position);

		float step = fieldOfView_ / frontSegments;
		float halfFov = fieldOfView_ * 0.5f;
		for (int i = 0; i < frontSegments + 1; i++) {
			shape.AddVertex (new Vector3(
				Mathf.Cos((halfFov - i*step - rotation) * Mathf.Deg2Rad) * viewDist_,
				0,
				Mathf.Sin((halfFov - i*step - rotation) * Mathf.Deg2Rad) * viewDist_
				) + position);
		}

		return shape;
	}
	
	public override void Validate()
	{
		position.y = 0;
		
		if (position.x > 0.5f * map.dimensions.x) {
			position.x = 0.5f * map.dimensions.x;
		}
		
		if (position.z > 0.5f * map.dimensions.z) {
			position.z = 0.5f * map.dimensions.z;
		}
		
		if (position.x < -0.5f * map.dimensions.x) {
			position.x = -0.5f * map.dimensions.x;
		}
		
		if (position.z < -0.5f * map.dimensions.z) {
			position.z = -0.5f * map.dimensions.z;
		}
		
		if (fieldOfView_ < 1f) {
			fieldOfView_ = 1f;
		}
		if (fieldOfView_ > 359) {
			fieldOfView_ = 359f;
		}
		
		if (frontSegments_ < Mathf.CeilToInt(fieldOfView_/90)) {
			frontSegments_ = Mathf.CeilToInt(fieldOfView_/90);
		}
		
		if (frontSegments_ > Mathf.CeilToInt(fieldOfView_/22.5f)) {
			frontSegments_ = Mathf.CeilToInt(fieldOfView_/22.5f);
		}
		
		if (viewDist_ < 0) {
			viewDist_ = 0;
		}
		
		if (dirty) {
			UpdateMesh();
			dirty = false;
		}
	}
}