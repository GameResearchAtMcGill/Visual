using UnityEngine;


[ExecuteInEditMode]
public class Class3 : MonoBehaviour {
	public Vector3 position = Vector3.zero;
	public float rotation = 0;
	
	public void OnDrawGizmos() {
		Shape3 shape1 = new Shape3();
		Quaternion q = Quaternion.Euler(0, 0, 0);
		shape1.AddVertex(q * new Vector3(-20, 0, 10));
		shape1.AddVertex(q * new Vector3(20, 0, 10));
		shape1.AddVertex(q * new Vector3(20, 0, 50));
		shape1.AddVertex(q * new Vector3(-20, 0, 50));
		
		
		q = Quaternion.Euler(0, rotation, 0);
		Shape3 shape2 = new Shape3();
		shape2.AddVertex(q * new Vector3(0, 0, -20) + position);
		shape2.AddVertex(q * new Vector3(-20, 0, 30) + position);
		shape2.AddVertex(q * new Vector3(0, 0, 35) + position);
		shape2.AddVertex(q * new Vector3(20, 0, 30) + position);
		
		if (shape1.SATCollision(shape2)) {
			Gizmos.color = Color.red;
		} else {
			Gizmos.color = Color.white;
		}
		
		foreach (Edge3Abs e in shape1) {
			Gizmos.DrawLine(e.a, e.b);
		}
		foreach (Edge3Abs e in shape2) {
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