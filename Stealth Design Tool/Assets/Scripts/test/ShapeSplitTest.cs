using UnityEngine;


[ExecuteInEditMode]
public class ShapeSplitTest : MonoBehaviour {
	public bool initialized = false;
	public bool reset = false;
	
	public void OnDrawGizmos() {
		Shape3 sh = new Shape3();
		sh.addVertex(new Vector3(10, 0, -5));
		sh.addVertex(new Vector3(-10, 0, -15));
		sh.addVertex(new Vector3(-15, 0, -5));
		sh.addVertex(new Vector3(-15, 0, 5));
		sh.addVertex(new Vector3(-10, 0, 15));
		sh.addVertex(new Vector3(10, 0, 5));
		
		foreach (Edge3Abs e in sh) {
			Gizmos.DrawLine(e.a, e.b);
		}
		
		Shape3[] shapes = sh.splitInTwo(new Vector3(20, 0, 0), new Vector3(-1, 0, 0));
		
		Gizmos.color = Color.red;
		foreach (Edge3Abs e in shapes[0]) {
			Gizmos.DrawLine(e.a, e.b);
		}
		
		Gizmos.color = Color.green;
		foreach (Edge3Abs e in shapes[1]) {
			Gizmos.DrawLine(e.a, e.b);
		}
	}
	
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
			
			initialized = true;
		}
	}
	
	public void OnValidate() {
		if (reset) {
			reset = false;
		}
	}
	
	// Called at every frame
	public void Update() {
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;
		transform.localScale = Vector3.one;
	}
}