using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(StealthCoordGuard))]
public class StealthCoordGuardEditor : Editor {
	private StealthCoordGuard g;
	private bool[] showCoordinate;
	
	void Awake()
	{
		g = (StealthCoordGuard)target;
		showCoordinate = new bool[g.getPositions().Count];
	}
	
	public override void OnInspectorGUI()
	{
		GUI.skin.label.wordWrap = true;
		
		GUILayout.Label("A guard is meant to represent an enemy which can move and see. Its field of view is occluded by obstacles.");
		GUILayout.Label("Coordinate Guard Parameters", EditorStyles.boldLabel);
		
		g.posX = EditorGUILayout.FloatField ("X Position:", g.posX);
		g.posZ = EditorGUILayout.FloatField ("Z Position:", g.posZ);
		g.rotation = EditorGUILayout.FloatField ("Rotation:", g.rotation);
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
		GUILayout.Label("Move it, rotate it, or change the FoV, View Distance and Front Segments using the Tools in the editor, or the fields above.");
		
		GUILayout.Label("");
		GUILayout.Label("To control motion, change the velocities of a particular coordinate. The velocities will be effective starting from the time of the coordinate.");
		GUILayout.Label("To add a coordinate, click on the button below.");
		GUILayout.Label("");
		
		GUILayout.Label("Coordinates", EditorStyles.boldLabel);
		List<StealthGuardPosition> positions = g.getSGP ();
		if (showCoordinate.Length < positions.Count) {
			showCoordinate = new bool[positions.Count];
		}
		int ind = 0;
		foreach (StealthGuardPosition gp in positions) {
			if (showCoordinate[ind] = EditorGUILayout.Foldout(showCoordinate[ind], "Coordinate " + ind + (ind == 0 ? " (Base)" : ""))) {
				Vector2 vel = new Vector2(gp.velocity.x, gp.velocity.z);
				vel = EditorGUILayout.Vector2Field("Velocity:", vel);
				gp.velocity = new Vector3(vel.x, 1, vel.y);
				if (ind != 0)
					gp.time = EditorGUILayout.FloatField("Time:", gp.time);
				gp.omega = EditorGUILayout.FloatField("Omega", gp.omega);
				
				if (GUILayout.Button("Select")) {
					Selection.activeGameObject = gp.gameObject;
				}
			}
			ind++;
		}

		if (GUILayout.Button ("Add Coordinate")) {
			GameObject go = g.AddCoordinate();
			
			if (go!= null) {
				int count = g.getPositions().Count;
				bool[] newBoolArr = new bool[count];
				for (int i = 0; i < count-1; i++) {
					newBoolArr[i] = showCoordinate[i];
				}
				newBoolArr[showCoordinate.Length] = true;
				showCoordinate = newBoolArr;
				g.UpdateMesh();
				Selection.activeTransform = go.transform;
			}
			
		}
		
		GUILayout.Label("");
		GUILayout.Label("Easiness: " + (g.easiness*100) + "%");
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
			Quaternion result = Handles.RotationHandle(g.rotationQ, g.position);
			if (result != g.rotationQ) {
				g.rotation = result.eulerAngles.y;
			}
		} else if (lastTool == Tool.Move) {
			Vector3 result = Handles.PositionHandle(g.position, g.rotationQ);
			if (result != g.position) {
				g.posX = result.x;
				g.posZ = result.z;
			}

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