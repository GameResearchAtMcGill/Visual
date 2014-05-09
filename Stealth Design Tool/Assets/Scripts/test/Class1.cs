using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Class1 : MonoBehaviour {
	SetOfPoints sop = new SetOfPoints();
	
	void Awake()
	{
		name = "Test";
		
		for (int i = 0; i < 500; i++) {
			sop.AddPoint(new Vector3(100.0f*Random.value, 0, 100.0f*Random.value));
		}
	}
	
	void OnDrawGizmos()
	{
		foreach (Edge3Abs e in sop.ConvexHull()) {
			Gizmos.DrawLine(e.a, e.b);
		}
	}
}
