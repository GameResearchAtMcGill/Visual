using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

[ExecuteInEditMode]
public class RRT : MonoBehaviour {

	public List<RRTNode> tree = new List<RRTNode>(); 
	public Map map;

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
		o.name = "Sample";
		o.tag = "Sample";
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