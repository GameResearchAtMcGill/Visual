using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class PrefabShim : MonoBehaviour {
	void Start ()
	{
		
	}
	
	void Update ()
	{
		if (transform.parent == null) {
			MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();
			
			foreach (MonoBehaviour mb in components) {
				if (!mb.enabled)
					mb.enabled = true;
			}
			
			PrefabUtility.DisconnectPrefabInstance(gameObject);
			
			Object.DestroyImmediate(this);
		}
	}
}
