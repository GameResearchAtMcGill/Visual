using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(StealthCamera))]
public class StealthCameraEditor : Editor {
	private StealthCamera c;

	void Awake() {
		c = (StealthCamera)target;
	}
	
	public override void OnInspectorGUI()
	{
		GUI.skin.label.wordWrap = true;
		
		GUILayout.Label("A camera is meant to represent an enemy which cannot move, but can rotate regularly and see. Its field of view is occluded by obstacles.");
		GUILayout.Label("Camera Parameters", EditorStyles.boldLabel);
		c.rotation = EditorGUILayout.FloatField ("Rotation", c.rotation);
		c.posX = EditorGUILayout.FloatField ("X Position", c.posX);
		c.posZ = EditorGUILayout.FloatField ("Z Position", c.posZ);
		c.type = (StealthCamera.Type) EditorGUILayout.EnumPopup ("Type", c.type);
		c.omega = EditorGUILayout.FloatField ("Angular speed", c.omega);
		if (c.type == StealthCamera.Type.Sweeping)
			c.amplitude = EditorGUILayout.FloatField ("Amplitude", c.amplitude);
		c.viewDistance = EditorGUILayout.FloatField ("View Distance", c.viewDistance);
		c.fieldOfView = EditorGUILayout.Slider("Field of View", c.fieldOfView, StealthFov.minFov, StealthFov.maxFov);
		c.frontSegments = EditorGUILayout.IntSlider("Front segments", c.frontSegments, c.minSegments, c.maxSegments);
		c.pause = EditorGUILayout.FloatField ("Pause", c.pause);
		StealthFov.debug = EditorGUILayout.Toggle("Debug Gizmos", StealthFov.debug);
		
		GUILayout.Label("");
		if (StealthFov.calculateEasiness = EditorGUILayout.Toggle("Calculate Easiness", StealthFov.calculateEasiness)) {
		GUILayout.Label("Easiness: " + (Mathf.Round(c.easiness*10000)*0.01) + "%");
		GUILayout.Label("Combined Easiness: " + (Mathf.Round(c.combinedEasiness*10000)*0.01) + "%");
		}
		
		GUILayout.Label("");
		GUILayout.Label("Move it, rotate it, or change the FoV, View Distance and Amplitude or Angular speed using the Tools in the editor, or the fields above.");
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
			Quaternion result = Handles.RotationHandle(c.rotationQ, c.position);
			// disable once CompareOfFloatsByEqualityOperator
			if (result.eulerAngles.y != c.rotation) {
				c.rotation = result.eulerAngles.y;
			}
		} else if (lastTool == Tool.Move) {
			Vector3 result = Handles.PositionHandle(c.position, c.rotationQ);
			if (result != c.position) {
				c.posX = result.x;
				c.posZ = result.z;
			}

		} else if (lastTool == Tool.Scale) {

			if (c.type == StealthCamera.Type.Sweeping) {
				Vector3 result = Handles.ScaleHandle (new Vector3 (c.viewDistance, c.omega, c.amplitude),
				                                      c.position, c.rotationQ,
				                                      HandleUtility.GetHandleSize(c.position));
				if (result != new Vector3(c.viewDistance, c.omega, c.amplitude)) {
					c.viewDistance = result.x;
					c.omega = result.y;
					c.amplitude = result.z;
				}

			} else {
				Vector3 result = Handles.ScaleHandle (new Vector3 (c.viewDistance, c.omega, c.fieldOfView),
				                                      c.position, c.rotationQ,
														HandleUtility.GetHandleSize(c.position));

				if (result != new Vector3(c.viewDistance, c.omega, c.fieldOfView)) {
					c.viewDistance = result.x;
					c.omega = result.y;
					c.fieldOfView = result.z;
				}
			}
		}
	}
}
