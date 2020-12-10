using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubDivision : MonoBehaviour 
{
	List<Vector3> vertices = new List<Vector3>();
	List<int> triangles = new List<int>();
	Mesh mesh;
	// Use this for initialization
	void Start () {
		mesh = transform.GetComponent<MeshFilter>().mesh;
	}
	
	void Update () {
		if(Input.GetKeyDown(KeyCode.D))
		{
			LoopSubdivision.Smooth(mesh);
		}
	}
}
