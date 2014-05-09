using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class WaitingWaypoint : Waypoint {
	
	public float waitingTime;
	
	[HideInInspector]
	public Dictionary<int, float> times = new Dictionary<int, float>();
	
	void OnDrawGizmos () {
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(transform.position, 1.9f);
		Gizmo();
	}
	
	new void Update() {
		base.Update();
		if (waitingTime < 0) {
			waitingTime = 0;
		}
	}
}