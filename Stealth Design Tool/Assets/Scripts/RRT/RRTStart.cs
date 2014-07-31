using UnityEngine;
using System.Collections;

public class RRTStart : MapChild {

	// Use this for initialization
	new protected void Awake()
	{
		base.Awake(); 
	}
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public override void MapChanged()
	{
		
	}
	public override void Validate()
	{
		position.y = 0;
		
		// Position clipping
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
	}
}
