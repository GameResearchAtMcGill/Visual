using UnityEngine;
using System.Collections;
using UnityEditor;
using Vectrosity;

public class CameraUpdate : MonoBehaviour {

	RRT rrt = null; 
	public float fSpeed = 4; 
	public float fDistance = 10; 

	public float timeStep = 1; 
	private float timer = 0; 
	// Use this for initialization
	void Start () 
	{
		if(!GameObject.Find("RRT"))
		{
			GameObject go = new GameObject();
			Map m = GameObject.Find("Map").GetComponent<Map>();

			go.transform.parent = m.transform;
			go.AddComponent("RRT");
			go.name = "RRT";
			
			rrt = GameObject.Find("RRT").GetComponent<RRT>();
			rrt.map =m;
		}
		else
		{
			rrt = GameObject.Find("RRT").GetComponent<RRT>();
			 
		}

	}
	// Update is called once per frame
	void Update () 
	{
		float step = fSpeed * Time.deltaTime;
		float fOrbitCircumfrance = 2F * fDistance * Mathf.PI;
		float fDistanceDegrees = (fSpeed / fOrbitCircumfrance) * 360;
		float fDistanceRadians = (fSpeed / fOrbitCircumfrance) * 2 * Mathf.PI;
		transform.RotateAround(Vector3.zero, Vector3.up, -fDistanceRadians);
		
		timer += Time.deltaTime; 
		if(timer>timeStep && !rrt.foundPath)
		{
	    	
			rrt.Step2(); 
			timer = 0; 
		}
		if (Input.GetKeyDown("space"))
		{

			//Stop draw 3d auto
			foreach(VectorLine l in rrt.lines)
				l.StopDrawing3DAuto();


			DestroyImmediate(GameObject.Find("RRT")); 
			Map m = GameObject.Find("Map").GetComponent<Map>();

			GameObject go = new GameObject();
			go.transform.parent = m.transform;
			go.AddComponent("RRT");
			go.name = "RRT";
			
			GameObject.Find("RRT").GetComponent<RRT>().map = m;
			rrt =  GameObject.Find("RRT").GetComponent<RRT>(); 
		}

	}
     

}
