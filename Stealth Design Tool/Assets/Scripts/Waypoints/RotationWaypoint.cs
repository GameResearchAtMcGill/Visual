using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class RotationWaypoint : Waypoint {
	
	public float theta;
	
	void OnDrawGizmos () {
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(transform.position, 1.9f);
		Gizmo();
	}
}