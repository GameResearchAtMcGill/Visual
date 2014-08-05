using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using Vectrosity;

[ExecuteInEditMode]
public class RRT : MonoBehaviour {

	public List<RRTNode> tree = new List<RRTNode>(); 
	public Map map;
	public int nodeNb =0;
	public bool foundPath = false;  
	public Camera refCamera; 

	public List<VectorLine> lines = new List<VectorLine>(); 

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
		RRTNode closest = null;
		float dist = 1000000; 

		//Linear search why not?
		foreach(RRTNode n in tree)
		{
			//Check if the node is lower in time
			//Should be change for permited angle.
			//is it a valid colour.  

			//Angle
			//Vector3 goingThere = n.transform.position - v;
			Vector3 goingThere = v - n.transform.position ;

			if(Vector3.Angle(transform.up,goingThere) > 80)
				continue; 	
			
			//Check if higher
			if(v.y<n.transform.position.y)
				continue; 	

			//Not good candidate
			if(n.colour == Color.red)
				continue; 

			//find the closest	
			if(Vector3.Distance(n.transform.position,v) < dist)
			{
				dist = Vector3.Distance(n.transform.position,v); 
				closest = n; 
			}
		}	

		//Could not find a good candidate
		if(closest == null)
			return; 

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
		
		//Create the vectrocity segment here. 


		VectorLine line = new VectorLine("linecast", 
										new Vector3[] {
										 	node.transform.position, node.parent.transform.position
											}, 
										node.colour, null, 2.0f);
		
		VectorLine.SetCamera3D(GameObject.Find("Camera").GetComponent<Camera>() as Camera);
		line.Draw3D();
		line.vectorObject.transform.parent = gameObject.transform;
		lines.Add(line);


		//Add to the tree if possible
		tree.Add(node);

		if(!coll)
		{
			//Check if in the goal positon. 
			GameObject goal = GameObject.Find("goal");
			Vector2 v1 = new Vector2(goal.transform.position.x,goal.transform.position.z);
			Vector2 v2 = new Vector2(v.x,v.z);
			if(Vector2.Distance(v1,v2)<2)
			{	
				Debug.Log("Found path");
				//Create a path with player Eugene
				List<Vector3> path = new List<Vector3>(); 
				RRTNode read = node; 
				
				while(read != null)
				{
					path.Add(read.transform.position);
					//Debug.Log(read.transform.position);
					//Thick green lines

					if(read.parent!=null)
					{
						VectorLine line2 = new VectorLine("linecast", 
											new Vector3[] {
											 	read.transform.position, read.parent.transform.position
												}, 
											Color.green, null, 5.0f);
						
						line2.Draw3D();
						line2.vectorObject.transform.parent = gameObject.transform;
						lines.Add(line2);

					}
					read = read.parent;


				}
				

				////Create the object
				//GameObject g = new GameObject(); 
				//RRTPath ppp = g.AddComponent<RRTPath>();
				//ppp.path =  path; 
				//g.transform.parent = gameObject.transform; 
				//g.name = "RRTPath"; 

				//ppp.CreateMesh(); 

				//Found path 
				foundPath= true;
			}
		}



	}

	public void Step2()
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
		RRTNode closest = null;
		float dist = 1000000; 

		//Linear search why not?
		foreach(RRTNode n in tree)
		{
			//Check if the node is lower in time
			//Should be change for permited angle.
			//is it a valid colour.  

			//Angle
			//Vector3 goingThere = n.transform.position - v;
			Vector3 goingThere = v - n.transform.position ;

			if(Vector3.Angle(transform.up,goingThere) > 80)
				continue; 	
			
			//Check if higher
			if(v.y<n.transform.position.y)
				continue; 	

			//Not good candidate
			if(n.colour == Color.red)
				continue; 

			//find the closest	
			if(Vector3.Distance(n.transform.position,v) < dist)
			{
				dist = Vector3.Distance(n.transform.position,v); 
				closest = n; 
			}
		}	

		//Could not find a good candidate
		if(closest == null)
			return; 

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
		
		//Create the vectrocity segment here. 


		VectorLine line = new VectorLine("linecast", 
										new Vector3[] {
										 	node.transform.position, node.parent.transform.position
											}, 
										node.colour, null, 2.0f);
		
		VectorLine.SetCamera3D(GameObject.Find("Camera").GetComponent<Camera>() as Camera);
		line.Draw3DAuto();
		line.vectorObject.transform.parent = gameObject.transform;
		lines.Add(line);



		//Add to the tree if possible
		tree.Add(node);

		if(!coll)
		{
			//Check if in the goal positon. 
			GameObject goal = GameObject.Find("goal");
			Vector3 v1 = v;

			Vector3 v2 = new Vector3(goal.transform.position.x,0,goal.transform.position.z);
				
			//Find minimum vector
			float angle = Vector3.Angle(v2-v1,transform.up);
			while(angle > 80)
			{
				v2.y+=3;
				angle = Vector3.Angle(v2-v1,transform.up);
				if(v2.y>map.dimensions.y)
					return; 
			}

			hit = new RaycastHit();
		 
			coll = Physics.Raycast(v1,
									v2 - v1, 
									out hit,
									Vector3.Distance(v2,v1),1);
			//Check collision free

			if(!coll)
			{	
				//Last node at v2

				o = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				o.transform.position = v2;
				o.transform.parent = this.transform; 

				o.name = "Sample " + nodeNb.ToString();
				nodeNb++; 
				o.tag = "Sample";
				//o.GetComponent("Collider").Active = false; 
				o.AddComponent("RRTNode");
				DestroyImmediate(o.collider);

				o.GetComponent<RRTNode>().parent = node; 
				//Draw segment
				line = new VectorLine("linecast", 
										new Vector3[] {
										 	v1, v2
											}, 
										Color.blue, null, 2.0f);
		
				line.Draw3DAuto();
				line.vectorObject.transform.parent = gameObject.transform;
				lines.Add(line);


				Debug.Log("Found path");
				//Create a path with player Eugene
				List<Vector3> path = new List<Vector3>(); 
				RRTNode read = o.GetComponent<RRTNode>(); 
				
				while(read != null)
				{
					path.Add(read.transform.position);
					//Debug.Log(read.transform.position);
					//Thick green lines

					if(read.parent!=null)
					{
						VectorLine line2 = new VectorLine("linecast", 
											new Vector3[] {
											 	read.transform.position, read.parent.transform.position
												}, 
											Color.green, null, 5.0f);
						
						line2.Draw3DAuto();
						line2.vectorObject.transform.parent = gameObject.transform;
						lines.Add(line2);

					}
					read = read.parent;


				}
				

				////Create the object
				//GameObject g = new GameObject(); 
				//RRTPath ppp = g.AddComponent<RRTPath>();
				//ppp.path =  path; 
				//g.transform.parent = gameObject.transform; 
				//g.name = "RRTPath"; 

				//ppp.CreateMesh(); 

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