﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

public class RRTPath : StealthWaypointPlayer 
{

	public List<Vector3> path = new List<Vector3>(); 


	public RRTPath(List<Vector3> path)
	{
		this.path = path; 
	}
	public override List<Pose> getPositions()
	{
		List<Pose> poses = new List<Pose>(); 
		foreach(Vector3 v in path)
		{
			poses.Add(new Pose(v,Quaternion.Euler(0, 1, 0)));
		}
		return poses;  
	}
	public override void Validate()
	{

	}
	new protected void Awake()
	{

		if (gameObject.GetComponent<MeshCollider>() == null) {
			gameObject.AddComponent("MeshCollider");
		}
		
		if (gameObject.GetComponent<Rigidbody>() == null) {
			var rb = (Rigidbody)gameObject.AddComponent("Rigidbody");
			rb.useGravity = false;
		}	
		if (gameObject.GetComponent<MeshFilter> () == null) {
			gameObject.AddComponent ("MeshFilter");
		}
		
		if (gameObject.GetComponent<MeshRenderer> () == null)
			gameObject.AddComponent ("MeshRenderer");


		//CreateMesh(); 

	}
	public override void CreateMesh() {
		
		Map map = GameObject.Find("Map").GetComponent<Map>() as Map; 

		List<Pose> pos = getPositions();
		Debug.Log(pos.Count);



		mf.sharedMesh = null;
		var m = new Mesh();
		m.name = "Player trail";
		Vector3[] vertices;
		bool capIt = false;
		// Last position is at the ceiling
		if (pos[pos.Count-1].position.y >= map.timeLength) 
		{
			vertices = new Vector3[8*pos.Count+2];
		// Last position must be capped by ceiling
		} else {
			vertices = new Vector3[8*(pos.Count+1)+2];
			capIt = true;
		}
		
		int cap1 = vertices.Length-2;
		int cap2 = vertices.Length-1;
		
		Vector3 curr = pos[0].position;
		int ind = 0;
		Pose prev = pos[0];
		foreach (Pose spp in pos) {
			curr += prev.velocity*(spp.time - prev.time);
			for (int d = 0; d < 8; d++) {
				vertices[ind++] = new Vector3(curr.x + Mathf.Cos(-d*0.25f*Mathf.PI)*radius, curr.y, curr.z + Mathf.Sin(-d*0.25f*Mathf.PI)*radius);
			}
			prev = spp;
		}
		if (capIt) {
			curr += prev.velocity*(map.timeLength - prev.time);
			for (int d = 0; d < 8; d++) {
				vertices[ind++] = new Vector3(curr.x + Mathf.Cos(-d*0.25f*Mathf.PI)*radius, curr.y, curr.z + Mathf.Sin(-d*0.25f*Mathf.PI)*radius);
			}
		}
		vertices[cap1] = new Vector3(pos[0].posX, 0, pos[0].posZ);
		vertices[cap2] = capIt ?
			new Vector3(curr.x, map.timeLength, curr.z) : new Vector3(curr.x, curr.y, curr.z);
		
		m.vertices = vertices;
		
		var triangles = new int[((vertices.Length-2)/8-1)*16*3+16*3];
		ind = 0;
		for (int i=0; i<vertices.Length-2-8; i+=8) {
			for (int j=0; j<8; j++) {
				
				if (j < 7) {
					triangles[ind++] = 0 + i + j;
					triangles[ind++] = 1 + i + j;
					triangles[ind++] = 9 + i + j;
					triangles[ind++] = 0 + i + j;
					triangles[ind++] = 9 + i + j;
					triangles[ind++] = 8 + i + j;
				} else {
					triangles[ind++] = 7 + i;
					triangles[ind++] = 0 + i;
					triangles[ind++] = 8 + i;
					triangles[ind++] = 7 + i;
					triangles[ind++] = 8 + i;
					triangles[ind++] = 15 + i;
				}
			}
		}
		
		// Caps
		for (int i=0; i < 8; i++) {
			triangles[ind++] = cap1;
			triangles[ind++] = (1 + i) % 8;
			triangles[ind++] = 0 + i;
		}
		int end = vertices.Length-2-8;
		for (int i=0; i < 8; i++) {
			triangles[ind++] = cap2;
			triangles[ind++] = 0 + i + end;
			triangles[ind++] = (1 + i) % 8 + end;
		}
		
		m.triangles = triangles;
		m.uv = new Vector2[vertices.Length];
		m.RecalculateNormals();
		
		mf.sharedMesh = m;
		gameObject.GetComponent<MeshCollider>().sharedMesh = m;
	}
}
