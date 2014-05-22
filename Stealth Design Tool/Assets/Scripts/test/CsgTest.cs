using UnityEngine;


[ExecuteInEditMode]
public class CsgTest : MonoBehaviour {
	public bool initialized = false;
	public bool reset = false;
	
	public Vector3 position;
	
	public enum Operation {
		INTERSECTION,
		UNION,
		DIFFERENCE
	}
	
	public Operation op = Operation.INTERSECTION;
	
	// Called at every initialization, and everytime recompilation occurs
	public void OnEnable() {
		//Debug.Log("enabled");
	}
	
	// Called at every initialization
	public void Awake() {
		//Debug.Log("awoke");
	}
	
	// Called at every initialization
	public void Start() {
		//Debug.Log("started");
	}
	
	// Called once when created, thereafter only when clicking on reset
	public void Reset() {
		//Debug.Log("reset");
		if (!initialized) {
			if (GetComponent<MeshFilter>() == null) {
				gameObject.AddComponent("MeshFilter");
			}
			if (GetComponent<MeshRenderer>() == null) {
				gameObject.AddComponent("MeshRenderer");
			}
			
			initialized = true;
		}
	}
	
	public void OnValidate() {
		Debug.Log("validate");
		
		Model a = CSG.Sphere(Vector3.up*3, 10, 16, 8);
		Model b = CSG.Sphere(position, 10, 16, 8);
		
		switch(op) {
			case Operation.DIFFERENCE:
				GetComponent<MeshFilter>().sharedMesh = CSG.Difference(a, b).ToUnityMesh();
				break;
			case Operation.INTERSECTION:
				GetComponent<MeshFilter>().sharedMesh = CSG.Intersection(a, b).ToUnityMesh();
				break;
			case Operation.UNION:
				GetComponent<MeshFilter>().sharedMesh = CSG.Union(a, b).ToUnityMesh();
				break;
		}
	}
	
	// Called at every frame
	public void Update() {
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;
		transform.localScale = Vector3.one;
	}
}