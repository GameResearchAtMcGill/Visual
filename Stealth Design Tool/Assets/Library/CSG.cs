using UnityEngine;
using System.Collections.Generic;
using System;

// Port of cgsjs
// Runs pretty slow
// Results are too ugly
// Stack overflow with bigger inputs
// At least it works

public struct Vertex {
	public Vector3 position;
	public Vector3 normal;
	public Vector3 uv;
	
	public Vertex(Vector3 position, Vector3 normal, Vector3 uv) {
		this.position = position;
		this.normal = normal;
		this.uv = uv;
	}
	
	public Vertex(Vector3 position, Vector3 normal) {
		this.position = position;
		this.normal = normal;
		this.uv = Vector3.zero;
	}
	
	public static Vertex Flip(Vertex v) {
		v.normal = -v.normal;
		return v;
	}
	
	public static Vertex Interpolate(Vertex a, Vertex b, float t) {
		Vertex ret;
		ret.position = Vector3.Lerp(a.position, b.position, t);
		ret.normal = Vector3.Lerp(a.normal, b.normal, t);
		ret.uv = Vector3.Lerp(a.uv, b.uv, t);
		return ret;
	}
}

public class Model {
	public readonly List<Vertex> vertices;
	public readonly List<int> indices;
	
	public Model() {
		vertices = new List<Vertex>();
		indices = new List<int>();
	}
	
	public Model(int capV, int capI) {
		vertices = new List<Vertex>(capV);
		indices = new List<int>(capI);
	}
	
	public void Translate(Vector3 t) {
		for (int i = 0; i < vertices.Count; i++) {
			Vertex v = vertices[i];
			v.position += t;
			vertices[i] = v;
		}
	}
	
	public static List<Polygon> ToPolygons(Model model) {
		List<Polygon> list = new List<Polygon>();
		for (int i = 0; i < model.indices.Count; i += 3) {
			List<Vertex> triangle = new List<Vertex>();
			for (int j = 0; j < 3; j++) {
				Vertex v = model.vertices[model.indices[i + j]];
				triangle.Add(v);
			}
			list.Add(new Polygon(triangle));
		}
		return list;
	}
	
	public static Model FromPolygons(List<Polygon> polygons) {
		Model model = new Model();
		int p = 0;
		for (int i = 0; i < polygons.Count; i++) {
			Polygon poly = polygons[i];
			for (int j = 2; j < poly.vertices.Count; j++) {
				model.vertices.Add(poly.vertices[0]);		model.indices.Add(p++);
				model.vertices.Add(poly.vertices[j - 1]);	model.indices.Add(p++);
				model.vertices.Add(poly.vertices[j]);		model.indices.Add(p++);
			}
		}
		
		return model;
	}
	
	public static Model FromUnityMesh(Mesh m) {
		Vector3[] vs = m.vertices;
		Vector3[] ns = m.normals;
		Vector2[] uvs= m.uv;
		int[] ixs = m.triangles;
		
		
		Model model = new Model(vs.Length, ixs.Length);
		
		for (int i = 0; i < vs.Length; i++) {
			Vertex v = new Vertex();
			v.position = new Vector3(vs[i].z, vs[i].y, vs[i].x);
			v.normal = new Vector3(ns[i].z, ns[i].y, ns[i].x);
			v.uv = uvs[i];
			
			model.vertices.Add(v);
		}
		
		for (int i = 0; i < ixs.Length; i += 3) {
			model.indices.Add(ixs[i + 0]);
			model.indices.Add(ixs[i + 2]);
			model.indices.Add(ixs[i + 1]);
		}
		
		return model;
	}
	
	public Mesh ToUnityMesh() {
		Mesh m = new Mesh();
		Vector3[] vs = new Vector3[vertices.Count];
		Vector3[] ns = new Vector3[vertices.Count];
		Vector2[] uvs= new Vector2[vertices.Count];
		
		int i = 0;
		foreach (Vertex v in vertices) {
			vs[i] = new Vector3(v.position.z, v.position.y, v.position.x);
			ns[i] = new Vector3(v.normal.z, v.normal.y, v.normal.x);
			uvs[i] = new Vector2(v.uv.x, v.uv.y);
			
			i++;
		}
		
		int[] ixs = new int[indices.Count];
		
		for (int ix = 0; ix < indices.Count; ix += 3) {
			ixs[ix+0] = indices[ix+0];
			ixs[ix+1] = indices[ix+2];
			ixs[ix+2] = indices[ix+1];
		}
		
		m.vertices = vs;
		m.triangles = ixs;
		m.normals = ns;
		m.uv = uvs;
		
		m.name = "CSG result";
		
		return m;
	}
}

public class Polygon {
	public List<Vertex> vertices;
	public Plane plane;
	
	public Polygon() {
		vertices = new List<Vertex>();
		plane = new Plane();
	}
	
	public Polygon(List<Vertex> list) {
		vertices = new List<Vertex>(list);
		plane = new Plane(
			vertices[0].position,
			vertices[1].position,
			vertices[2].position
		);
	}
	
	public void Flip() {
		vertices.Reverse();
		for (int i = 0; i < vertices.Count; i++) {
			Vertex v = vertices[i];
			v.normal = -v.normal;
			vertices[i] = v;
		}
		plane.Flip();
	}
	
	public Polygon Clone() {
		Polygon ret = new Polygon();
		foreach (Vertex v in vertices) {
			ret.vertices.Add(v);
		}
		ret.plane = plane.Clone();
		
		return ret;
	}
}

public class Plane {
	Vector3 normal;
	float w;
	
	enum Types : int {
		COPLANAR = 0,
		FRONT = 1,
		BACK = 2,
		SPANNING = 3
	}
	
	public Plane() {
		normal = Vector3.zero;
		w = 0.0f;
	}
	
	public Plane(Vector3 a, Vector3 b, Vector3 c) {
		normal = Vector3.Cross(b - a, c - a).normalized;
		w = Vector3.Dot(normal, a);
	}
	
	public bool Ok() {
		return normal.magnitude > 0.0f;
	}
	
	public void Flip() {
		normal = -normal;
		w *= -1f;
	}
	
	public void SplitPolygon(Polygon polygon, List<Polygon> coplanarFront, List<Polygon> coplanarBack, List<Polygon> front, List<Polygon> back) {
		Types polygonType = 0;
		List<Types> types = new List<Types>();
		
		for (int i = 0; i < polygon.vertices.Count; i++) {
			float t = Vector3.Dot(normal, polygon.vertices[i].position) - w;
			Types type = (t < -CSG.EPSILON) ? Types.BACK : ((t > CSG.EPSILON) ? Types.FRONT : Types.COPLANAR);
			polygonType = (Types)((int)polygonType | (int)type);
			types.Add(type);
		}
		
		switch (polygonType) {
			case Types.COPLANAR:
				if (Vector3.Dot(normal, polygon.plane.normal) > 0) {
					coplanarFront.Add(polygon);
				} else {
					coplanarBack.Add(polygon);
				}
				break;
			case Types.FRONT:
				front.Add(polygon);
				break;
			case Types.BACK:
				back.Add(polygon);
				break;
			case Types.SPANNING:
				List<Vertex> f = new List<Vertex>();
				List<Vertex> b = new List<Vertex>();
				for (int i = 0; i < polygon.vertices.Count; i++) {
					int j = (i + 1) % polygon.vertices.Count;
					Types ti = types[i], tj = types[j];
					Vertex vi = polygon.vertices[i], vj = polygon.vertices[j];
					if (ti != Types.BACK) f.Add(vi);
					if (ti != Types.FRONT) b.Add(vi);
					if ((Types)((int)ti | (int)tj) == Types.SPANNING) {
						float t = (w - Vector3.Dot(normal, vi.position)) / Vector3.Dot(normal, vj.position - vi.position);
						Vertex v = Vertex.Interpolate(vi, vj, t);
						f.Add(v);
						b.Add(v);
					}
				}
				if (f.Count >= 3) front.Add(new Polygon(f));
				if (b.Count >= 3) back.Add(new Polygon(b));
				break;
		}
	}
	
	public Plane Clone() {
		Plane ret = new Plane();
		ret.normal = normal;
		ret.w = w;
		
		return ret;
	}
}

class Node {
	public List<Polygon> polygons;
	public Node front;
	public Node back;
	public Plane plane;
	
	public Node() {
		plane = null;
		front = null;
		back = null;
		polygons = new List<Polygon>();
	}
	
	public Node(List<Polygon> list) {
		plane = null;
		front = null;
		back = null;
		polygons = new List<Polygon>();
		Build(list);
	}
	
	public static Node Union(Node a1, Node b1) {
		Node a = a1.Clone();
		Node b = b1.Clone();
		a.ClipTo(b);
		b.ClipTo(a);
		b.Invert();
		b.ClipTo(a);
		b.Invert();
		a.Build(b.AllPolygons());
		Node ret = new Node(a.AllPolygons());
		
		return ret;
	}
	
	public static Node Subtract(Node a1, Node b1) {
		Node a = a1.Clone();
		Node b = b1.Clone();
		a.Invert();
		a.ClipTo(b);
		b.ClipTo(a);
		b.Invert();
		b.ClipTo(a);
		b.Invert();
		a.Build(b.AllPolygons());
		a.Invert();
		Node ret = new Node(a.AllPolygons());
		
		return ret;
	}
	
	public static Node Intersect(Node a1, Node b1) {
		Node a = a1.Clone();
		Node b = b1.Clone();
		
		a.Invert();
	    b.ClipTo(a);
	    b.Invert();
	    a.ClipTo(b);
	    b.ClipTo(a);
	    a.Build(b.AllPolygons());
	    a.Invert();
		Node ret = new Node(a.AllPolygons());
		
		return ret;
	}
	
	public void Invert() {
		for (int i = 0; i < polygons.Count; i++) {
			polygons[i].Flip();
		}
		plane.Flip();
		if (front != null) front.Invert();
		if (back != null) back.Invert();
		Node temp = front;
		front = back;
		back = temp;
	}
	
	public List<Polygon> ClipPolygons(List<Polygon> list) {
		if (plane == null) {
			List<Polygon> ret = new List<Polygon>(list.Count);
			foreach (Polygon p in list) {
				ret.Add(p);
			}
			return ret;
		}
		List<Polygon> listFront = new List<Polygon>();
		List<Polygon> listBack = new List<Polygon>();
		for (int i = 0; i < list.Count; i++) {
			plane.SplitPolygon(list[i], listFront, listBack, listFront, listBack);
		}
		if (front != null) listFront = front.ClipPolygons(listFront);
		if (back != null) listBack = back.ClipPolygons(listBack);
		else listBack.Clear();
		
		foreach (Polygon p in listBack) {
			listFront.Add(p);
		}
		return listFront;
	}
	
	public void ClipTo(Node other) {
		polygons = other.ClipPolygons(polygons);
		if (front != null) front.ClipTo(other);
		if (back != null) back.ClipTo(other);
	}
	
	public List<Polygon> AllPolygons() {
		List<Polygon> list = new List<Polygon>(polygons.Count);
		foreach (Polygon p in polygons) {
			list.Add(p.Clone());
		}
		if (front != null) {
			foreach (Polygon p in front.AllPolygons()) {
				list.Add(p.Clone());
			}
		}
		if (back != null) {
			foreach (Polygon p in back.AllPolygons()) {
				list.Add(p.Clone());
			}
		}
		
		return list;
	}
	
	public Node Clone() {
		Node ret = new Node();
		if (plane != null) ret.plane = plane.Clone();
		if (front != null) ret.front = front.Clone();
		if (back != null) ret.back = back.Clone();
		ret.polygons = new List<Polygon>(polygons.Count);
		foreach (Polygon p in polygons) {
			ret.polygons.Add(p.Clone());
		}
		return ret;
	}
	
	public void Build(List<Polygon> list) {
		if (list.Count == 0) return;
		if (plane == null) plane = list[0].plane.Clone();
		List<Polygon> listFront = new List<Polygon>();
		List<Polygon> listBack = new List<Polygon>();
		for (int i = 0; i < list.Count; i++) {
			plane.SplitPolygon(list[i], polygons, polygons, listFront, listBack);
		}
		
		if (listFront.Count > 0) {
			if (front == null) front = new Node();
			front.Build(listFront);
		}
		if (listBack.Count > 0) {
			if (back == null) back = new Node();
			back.Build(listBack);
		}
	}
}

public static class CSG {
	public const float EPSILON = 1e-4f;
	
	private static Model Operation(Model a, Model b, Func<Node, Node, Node> operation) {
		Node A = new Node(Model.ToPolygons(a));
		Node B = new Node(Model.ToPolygons(b));
		
		Node AB = operation.Invoke(A, B);
		List<Polygon> polygons = AB.AllPolygons();
		return Model.FromPolygons(polygons);
	}
	
	public static Model Union(Model a, Model b) {
		return Operation(a, b, Node.Union);
	}
	
	public static Model Intersection(Model a, Model b) {
		return Operation(a, b, Node.Intersect);
	}
	
	public static Model Difference(Model a, Model b) {
		return Operation(a, b, Node.Subtract);
	}
	
	private static Vertex SphereVertex(List<Vertex> vertices, float theta, float phi, float r, Vector3 c) {
		theta *= Mathf.PI * 2;
		phi *= Mathf.PI;
		
		Vector3 dir = new Vector3(
			Mathf.Cos(theta) * Mathf.Sin(phi),
			Mathf.Cos(phi),
			Mathf.Sin(theta) * Mathf.Sin(phi)
		);
		Vertex v = new Vertex(c + dir*r, dir, new Vector3(theta / Mathf.PI * 0.5f,  phi / Mathf.PI, 0));
		vertices.Add(v);
		return v;
	}
	
	public static Model Sphere(Vector3 center, float radius, int slices, int stacks) {
		List<Polygon> polygons = new List<Polygon>();
		List<Vertex> vertices;
		
		float istacks = 1f/stacks;
		float islices = 1f/slices;
		
		for (int i = 0; i < slices; i++) {
			for (int j = 0; j < stacks; j++) {
				vertices = new List<Vertex>();
				SphereVertex(vertices, i * islices, j * istacks, radius, center);
				if (j > 0) SphereVertex(vertices, (i + 1) * islices, j * istacks, radius, center);
				if (j < stacks - 1) SphereVertex(vertices, (i + 1) * islices, (j + 1) * istacks, radius, center);
				SphereVertex(vertices, i * islices, (j + 1) * istacks, radius, center);
				polygons.Add(new Polygon(vertices));
			}
		}
		
		return Model.FromPolygons(polygons);
	}
}