using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(StealthWaypointGuard))]
public class StealthWaypointGuardEditor : Editor {
	private StealthWaypointGuard g;
	
	void Awake()
	{
		g = (StealthWaypointGuard)target;
	}
	
	public override void OnInspectorGUI()
	{
		GUILayout.Label("A guard is meant to represent an enemy which can move and see. Its field of view is occluded by obstacles.");
		GUILayout.Label("Waypoint Guard Parameters", EditorStyles.boldLabel);
		
		g.viewDistance = EditorGUILayout.FloatField("View Distance:", g.viewDistance);
		g.fieldOfView = EditorGUILayout.FloatField("Field of view:", g.fieldOfView);
		g.frontSegments = EditorGUILayout.IntField("Front segments:", g.frontSegments);
		g.maxSpeed = EditorGUILayout.FloatField("Max speed:", g.maxSpeed);
		g.maxOmega = EditorGUILayout.FloatField("Max angular speed:", g.maxOmega);
		StealthFov.debug = EditorGUILayout.Toggle("Debug Gizmos", StealthFov.debug);
		
		GUILayout.Label("");
		GUILayout.Label("Easiness: " + (Mathf.Round(g.easiness*10000)*0.01) + "%");
		GUILayout.Label("Combined Easiness: " + (Mathf.Round(g.combinedEasiness*10000)*0.01) + "%");
		
		GUILayout.Label("");
		GUILayout.Label("Change the FoV, View Distance and Front Segments using the Scale Tool in the editor, or the fields above.");
		
		GUILayout.Label("");
		GUILayout.Label("To control motion, select the waypoint manger by clicking on the button below, and start adding waypoints.");
		GUILayout.Label("");
		
		if (GUILayout.Button("Select Waypoints")) {
			Selection.activeTransform = g.waypoints.transform;
		}
		
	}
	
	Tool lastTool = Tool.None;
	
	void OnEnable()
	{
		lastTool = Tools.current;
		Tools.current = Tool.None;
	}
	
	void OnDisable()
	{
		Tools.current = lastTool;
	}
	
	void OnSceneGUI ()
	{
		if (Tools.current != Tool.None) {
			lastTool = Tools.current;
			Tools.current = Tool.None;
		}
		
		if (lastTool == Tool.Rotate) {
			Tools.current = Tool.None;
		} else if (lastTool == Tool.Move) {
			Tools.current = Tool.None;
		} else if (lastTool == Tool.Scale) {
			Vector3 result = Handles.ScaleHandle (new Vector3 (g.viewDistance, g.frontSegments, g.fieldOfView),
				                                      g.position, g.rotationQ,
				                                      HandleUtility.GetHandleSize(g.position));

			if (result != new Vector3(g.viewDistance, 1, g.fieldOfView)) {
				g.viewDistance = result.x;
				g.fieldOfView = result.z;
				g.frontSegments = Mathf.RoundToInt(result.y);
			}
		}
	}
}