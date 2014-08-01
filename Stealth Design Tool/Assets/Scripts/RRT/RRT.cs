using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

[ExecuteInEditMode]
public class RRT : MonoBehaviour {

	public List<RRTNode> tree = new List<RRTNode>(); 
	public Map map;
	public int nodeNb =0;
	public bool foundPath = false;  
	public void Awake()
	{
		gameObject.AddComponent("RRTNode");
		tree.Add(gameObject.GetComponent<RRTNode>()); 
		//Change position for RRT starting position
		transform.position = GameObject.Find("start").transform.position; 

	}

	public void Update()
	{
		if(!map)
		{
			map = GameObject.Find("Map").GetComponent<Map>(); 
		}
		foreach(RRTNode n in tree)
		{
			//Fuck this and change it for vectrocity
			if(n.parent)
				Debug.DrawLine(n.transform.position,n.parent.transform.position,n.colour);
		}
	}
	public void Step()
	{
		//sample random nodes inside the map

		Vector3 v = new Vector3(Random.Range(-map.dimensions.x/2 ,map.dimensions.x/2), 
								Random.Range(0 ,map.dimensions.y),
								Random.Range(-map.dimensions.z/2 ,map.dimensions.z/2));

		GameObject o = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		o.transform.position = v;
		o.transform.parent = this.transform; 
		
		o.name = "Sample " + nodeNb.ToString();
		nodeNb++; 
		o.tag = "Sample";
		//o.GetComponent("Collider").Active = false; 
		o.AddComponent("RRTNode");
		DestroyImmediate(o.collider);

		RRTNode node = o.GetComponent<RRTNode>() as RRTNode; 


		//Find the closest
		RRTNode closest = tree[0];
		float dist = Vector3.Distance(closest.transform.position,v); 

		//Linear search why not?
		foreach(RRTNode n in tree)
		{
			//Check if the node is lower in time
			//Should be change for permited angle.
			//is it a valid colour.  
			if(n.colour == Color.red || n.transform.position.y>v.y)
				continue; 

			//find the closest	
			if(Vector3.Distance(n.transform.position,v) < dist)
			{
				dist = Vector3.Distance(n.transform.position,v); 
				closest = n; 
			}
		}	

		//Add the parent
		node.parent = closest; 

		RaycastHit hit;
		//Check for collision. 
		bool coll = Physics.Raycast(node.parent.transform.position,
									v - node.parent.transform.position, 
									out hit,
									Vector3.Distance(node.parent.transform.position,v),1);
		if (coll)
		{
			node.colour = Color.red; 
			//Debug.Log(hit.transform.gameObject.name);
		}
		else
			node.colour = Color.blue; 
		

		//Add to the tree if possible
		tree.Add(node);

		if(!coll)
		{
			//Check if in the goal positon. 
			GameObject goal = GameObject.Find("goal");
			if(Vector2.Distance(goal.transform.position,v)<5)
			{	
				Debug.Log("Found path");
				//Create a path with player Eugene
				List<Vector3> path = new List<Vector3>(); 
				RRTNode read = node; 
				
				while(read != null)
				{
					path.Add(read.transform.position);
					//Debug.Log(read.transform.position);
					read = read.parent;
				}
				path.Reverse();

				//Create the object
				GameObject g = new GameObject(); 
				RRTPath ppp = g.AddComponent<RRTPath>();
				ppp.path =  path; 
				g.transform.parent = gameObject.transform; 
				g.name = "RRTPath"; 

				ppp.CreateMesh(); 

				//Found path 
				foundPath= true;
			}
		}
	}
	public void Find()
	{
		for(int i = 0;i<1000;i++)
		{
			this.Step(); 
			if(foundPath)
				return; 
		}
	}
	public void Clear()
	{
		tree.Clear();
		foreach(GameObject g in GameObject.FindGameObjectsWithTag("Sample"))
		{
			DestroyImmediate(g);
		}
	}
}