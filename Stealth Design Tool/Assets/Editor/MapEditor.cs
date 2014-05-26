using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Map))]
public class MapEditor : Editor {
	private Map m;
	
	void Awake ()
	{
		m = (Map)target;
	}
	
	public Vector3 CameraPointingAt() {
		var view = SceneView.currentDrawingSceneView;
		
		if (view != null) {
			Vector3 o = view.camera.transform.position;
			Vector3 t = view.camera.transform.forward;
			float c = -o.y/t.y;
			return new Vector3(o.x + c * t.x, 0, o.z + c * t.z);
		} else {
			return Vector3.zero;
		}
	}
	
	public override void OnInspectorGUI()
	{
		GUI.skin.label.wordWrap = true;
		
		GUILayout.Label("Map Parameters", EditorStyles.boldLabel);
		m.sizeX = EditorGUILayout.FloatField ("Size X:", m.sizeX);
		m.timeLength = EditorGUILayout.FloatField ("Time length:", m.timeLength);
		m.sizeZ = EditorGUILayout.FloatField ("Size Z:", m.sizeZ);
		m.subdivisionsPerSecond = EditorGUILayout.FloatField("Subdivisions/sec:", m.subdivisionsPerSecond);
		m.clipMap = EditorGUILayout.Toggle("Clip map", m.clipMap);
		
		GUILayout.Label("");
		GUILayout.Label("Only add primitives using the buttons below. The active selection will be set automatically to the created primitive.");
		
		GUILayout.Label("");
		GUILayout.Label("Obstacle", EditorStyles.boldLabel);
		GUILayout.Label("An obstacle that can be moved, rotated, and scaled around.");
		if (GUILayout.Button("Add Obstacle")) {
			GameObject go = new GameObject();
			go.transform.parent = m.transform;
			go.AddComponent("StealthObstacle");
			Vector3 pos = CameraPointingAt();
			go.GetComponent<StealthObstacle>().posX = pos.x;
			go.GetComponent<StealthObstacle>().posZ = pos.z;
			Selection.activeTransform = go.transform;
		}
		
		GUILayout.Label("");
		GUILayout.Label("Guards", EditorStyles.boldLabel);
		GUILayout.Label("A guard for which the velocity is controlled piece-wise.");
		if (GUILayout.Button("Add Coordinate Guard")) {
			GameObject go = new GameObject();
			go.transform.parent = m.transform;
			go.AddComponent("StealthCoordGuard");
			Selection.activeTransform = go.transform;
		}
		
		GUILayout.Label("A guard for which the position is controlled with waypoints.");
		if (GUILayout.Button("Add Waypoint Guard")) {
			GameObject go = new GameObject();
			go.transform.parent = m.transform;
			go.AddComponent("StealthWaypointGuard");
			Selection.activeTransform = go.transform;
		}
		
		GUILayout.Label("");
		GUILayout.Label("Camera", EditorStyles.boldLabel);
		GUILayout.Label("A camera for which the rotation is regular.");
		if (GUILayout.Button("Add Camera")) {
			GameObject go = new GameObject();
			go.transform.parent = m.transform;
			go.AddComponent("StealthCamera");
			Selection.activeTransform = go.transform;
		}
		
		GUILayout.Label("");
		GUILayout.Label("Players", EditorStyles.boldLabel);
		GUILayout.Label("A player for which the velocity is controlled piece-wise.");
		if (GUILayout.Button("Add Coordinate Player")) {
			GameObject go = new GameObject();
			go.transform.parent = m.transform;
			go.AddComponent("StealthCoordPlayer");
			Selection.activeTransform = go.transform;
		}
		GUILayout.Label("A player for which the position is controlled with waypoints.");
		if (GUILayout.Button("Add Waypoint Player")) {
			GameObject go = new GameObject();
			go.transform.parent = m.transform;
			go.AddComponent("StealthWaypointPlayer");
			Selection.activeTransform = go.transform;
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

		Vector3 result = Handles.ScaleHandle (new Vector3 (m.sizeX, m.timeLength, m.sizeZ),
		                    Vector3.zero, new Quaternion (0, 0, 0, 1),
		                    HandleUtility.GetHandleSize(Vector3.zero));
		m.sizeX = result.x;
		m.timeLength = result.y;
		m.sizeZ = result.z;


	}
}
