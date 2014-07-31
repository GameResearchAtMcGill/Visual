using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class RRTGoal : MapChild {
	
	// Use this for initialization
	new protected void Awake()
	{
		gameObject.transform.localScale = new Vector3(4,4,4); 
		gameObject.name = "goal";
		renderer.material = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/goal.mat", typeof(Material));
		base.Awake(); 
	}
	void Start () {
		
	}
	
	// Update is called once per frame
	public void Update()
	{
		this.Validate(); 
	}
	public override void MapChanged()
	{
		
	}
	public override void Validate()
	{

		Vector3 scale = transform.localScale;
		scale.y = map.dimensions.y/2f;
		transform.localScale = scale;


		Vector3 pos = transform.position; 
		pos.y = map.dimensions.y/2;

		// Position clipping
		if (pos.x > 0.5f * map.dimensions.x) {
			pos.x = 0.5f * map.dimensions.x;
		}
		
		if (pos.z > 0.5f * map.dimensions.z) {
			pos.z = 0.5f * map.dimensions.z;
		}
		
		if (pos.x < -0.5f * map.dimensions.x) {
			pos.x = -0.5f * map.dimensions.x;
		}
		
		if (pos.z < -0.5f * map.dimensions.z) {
			pos.z = -0.5f * map.dimensions.z;
		}
		transform.position = pos; 
	}
}
