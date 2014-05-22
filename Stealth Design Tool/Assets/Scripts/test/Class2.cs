using UnityEngine;


[ExecuteInEditMode]
public class Class2 : MonoBehaviour {
	
	public void OnDrawGizmos() {
		Shape3 clipper = new Shape3();
		Quaternion q = Quaternion.Euler(0, 20, 0);
		clipper.AddVertex(q * new Vector3(10, 0, 10));
		clipper.AddVertex(q * new Vector3(10, 0, -10));
		clipper.AddVertex(q * new Vector3(-10, 0, -10));
		clipper.AddVertex(q * new Vector3(-10, 0, 10));
		
		Gizmos.color = Color.red;
		foreach (Edge3Abs e in clipper) {
			Gizmos.DrawLine(e.a, e.b);
		}
		
		Shape3 clippee = new Shape3();
		clippee.AddVertex(new Vector3(0, 0, -20));
		clippee.AddVertex(new Vector3(-20, 0, 30));
		clippee.AddVertex(new Vector3(20, 0, 30));
		
		Gizmos.color = Color.white;
		foreach (Edge3Abs e in clippee) {
			Gizmos.DrawLine(e.a, e.b);
		}
		
		Shape3 clipped = clippee.Clip(clipper);
		
		Gizmos.color = Color.green;
		foreach (Edge3Abs e in clipped) {
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
		
	}
	
	public void OnValidate() {
		Debug.Log("validate");
		
	}
	
	// Called at every frame
	public void Update() {
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;
		transform.localScale = Vector3.one;
	}
}