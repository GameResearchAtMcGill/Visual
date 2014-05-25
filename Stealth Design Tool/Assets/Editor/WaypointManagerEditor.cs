using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(WaypointManager))]
public class WaypointManagerEditor : Editor {
	// Custom Editors are only called when the object is selected.
	// To do Gizmos draws, the code should be placed in the inspected class
	
	private WaypointManager wm;
	
	void Awake() {
		wm = (WaypointManager)target;
	}
	
	public override void OnInspectorGUI() {
		//DrawDefaultInspector();
		
		GUILayout.Label("Do not manage waypoints yourself!");
		
		GUILayout.Label("");
		GUILayout.Label("The height of the manager can be changed, so as to see the waypoints better.");
		
		GUILayout.Label("Waypoint Operations", EditorStyles.boldLabel);
		
		GUILayout.Label("A normal waypoint causes motion in a certain direction up to the waypoint.");
		if (GUILayout.Button("Add Waypoint")) {
			wm.AddWaypoint();
		}
		GUILayout.Label("A wainting waypoint causes inaction for a certain amount of time.");
		if (GUILayout.Button("Add Waiting Waypoint")) {
			wm.AddWaitingWaypoint();
		}
		GUILayout.Label("A rotation waypoint causes rotation upto a certain angle.");
		if (GUILayout.Button("Add Rotation Waypoint")) {
			wm.AddRotationWaypoint();
	    }
				
		GUILayout.Label("");
		
		GUILayout.Label("Clear all the waypoints.");
		if (GUILayout.Button("Clear")) {
			wm.Clear();
		}
	}
}