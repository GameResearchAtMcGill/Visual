using UnityEngine;


[ExecuteInEditMode]
public class Class2 : MonoBehaviour {
	public Vector3 position = Vector3.zero;
	public float rotation = 0;
	public bool reverseClipper = false;
	public bool reverseClippee = false;
	public int offsetClipper = 0;
	public int offsetClippee = 0;
	
	public void OnDrawGizmos() {
		Shape3 clipper = new Shape3();
		Quaternion q = Quaternion.Euler(0, 0, 0);
		clipper.AddVertex(q * new Vector3(-20, 0, 10));
		clipper.AddVertex(q * new Vector3(20, 0, 10));
		clipper.AddVertex(q * new Vector3(20, 0, 50));
		clipper.AddVertex(q * new Vector3(-20, 0, 50));
		if (reverseClipper) clipper.Reverse();
		clipper.Offset(offsetClipper);
		
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(clipper[0], 1);
		foreach (Edge3Abs e in clipper) {
			Gizmos.DrawLine(e.a, e.b);
		}
		
		q = Quaternion.Euler(0, rotation, 0);
		Shape3 clippee = new Shape3();
		clippee.AddVertex(q * new Vector3(0, 0, -20) + position);
		clippee.AddVertex(q * new Vector3(-20, 0, 30) + position);
		clippee.AddVertex(q * new Vector3(0, 0, 35) + position);
		clippee.AddVertex(q * new Vector3(20, 0, 30) + position);
		if (reverseClippee) clippee.Reverse();
		clippee.Offset(offsetClippee);
		
		Gizmos.color = Color.white;
		foreach (Edge3Abs e in clippee) {
			Gizmos.DrawLine(e.a, e.b);
		}
		
		Shape3 clipped = clippee.ClipOut(clipper);
		
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