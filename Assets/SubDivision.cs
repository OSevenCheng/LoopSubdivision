using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubDivision : MonoBehaviour {

List<Vector3> vertices = new List<Vector3>();
	List<int> triangles = new List<int>();
	Mesh mesh;
	// Use this for initialization
	void Start () {
		/* 
		 * 因为没有指针，所有设计函数的 传入参数的 索引，导致函数在可阅读性上有些问题
		 *   必须遵循严格的函数执行步骤。
		 *   
		 *  这里因为半边结构的特殊性，必须保证mesh的每个vertex的唯一性，（即每个vertex不重复保存），
		 *    所以unity自带的cube模型不能用（因为其每个面是独立保存的），
		 *    所以重新计算mesh的vertex与triangle结构
		 */

		/*************** mesh初始化 ******************/
		mesh = transform.GetComponent<MeshFilter>().mesh;

		//<0>Cube
		//CreateCube();

		//<1>Cylinder 
		//CreateCylinder();

		//LoopSubdivision.Smooth(mesh);
	}
	void CreateCube()
	{
		vertices.Add(new Vector3(0.0f, 0.0f, 0.0f));
		vertices.Add(new Vector3(1.0f, 0.0f, 0.0f));
		vertices.Add(new Vector3(1.0f, 0.0f, 1.0f));
		vertices.Add(new Vector3(0.0f, 0.0f, 1.0f));
		vertices.Add(new Vector3(0.0f, 1.0f, 0.0f));
		vertices.Add(new Vector3(1.0f, 1.0f, 0.0f));
		vertices.Add(new Vector3(1.0f, 1.0f, 1.0f));
		vertices.Add(new Vector3(0.0f, 1.0f, 1.0f));

		triangles.Add(4);
		triangles.Add(5);
		triangles.Add(0);
		triangles.Add(5);
		triangles.Add(1);
		triangles.Add(0);

		triangles.Add(5);
		triangles.Add(6);
		triangles.Add(1);
		triangles.Add(6);
		triangles.Add(2);
		triangles.Add(1);

		triangles.Add(7);
		triangles.Add(6);
		triangles.Add(4);
		triangles.Add(6);
		triangles.Add(5);
		triangles.Add(4);

		triangles.Add(7);
		triangles.Add(4);
		triangles.Add(3);
		triangles.Add(4);
		triangles.Add(0);
		triangles.Add(3);

		triangles.Add(6);
		triangles.Add(7);
		triangles.Add(3);
		triangles.Add(6);
		triangles.Add(3);
		triangles.Add(2);

		triangles.Add(0);
		triangles.Add(1);
		triangles.Add(2);
		triangles.Add(0);
		triangles.Add(2);
		triangles.Add(3);



		//<1>Grid 
		int r = 5, c = 5;
		float offset = 0.2f;

		for (int i = 0; i < r; ++i)
		{
			for (int j = 0; j < c; ++j)
			{
				vertices.Add(new Vector3(i * offset, 0.0f, j * offset));
			}
		}

		for (int i = 0; i < r - 1; ++i)
		{
			for (int j = 0; j < c - 1; ++j)
			{
				int a0 = i * c + j;
				int a1 = a0 + 1;
				int a2 = (i + 1) * c + j;
				int a3 = a2 + 1;

				triangles.Add(a0);
				triangles.Add(a1);
				triangles.Add(a2);

				triangles.Add(a1);
				triangles.Add(a3);
				triangles.Add(a2);

			}
		}
	}
	void CreateCylinder()
	{
		int r = 2, pre = 24;
		float PI = 3.1415926f;
		float offset = 2 * PI / pre;

		for (int i = 0; i < pre; ++i)
		{
			float x = r * Mathf.Cos(i * offset);
			float y = r * Mathf.Sin(i * offset);

			vertices.Add(new Vector3(x, 1.0f, y));
		}
		for (int i = 0; i < pre; ++i)
		{
			float x = r * Mathf.Cos(i * offset);
			float y = r * Mathf.Sin(i * offset);

			vertices.Add(new Vector3(x, -1.0f, y));
		}


		vertices.Add(new Vector3(0.0f, 1.0f, 0.0f));
		vertices.Add(new Vector3(0.0f, -1.0f, 0.0f));

		for (int i = 0; i < pre; ++i)
		{
			if (i == pre - 1)
			{
				//两个圆面
				triangles.Add(i);
				triangles.Add(2 * pre);
				triangles.Add(0);

				triangles.Add(pre + i);
				triangles.Add(pre + 0);
				triangles.Add(2 * pre + 1);

				//壁
				triangles.Add(i);
				triangles.Add(pre);
				triangles.Add(pre + i);

				triangles.Add(pre);
				triangles.Add(i);
				triangles.Add(0);
			}
			else
			{
				//两个圆面
				triangles.Add(i);
				triangles.Add(2 * pre);
				triangles.Add(i + 1);

				triangles.Add(pre + i);
				triangles.Add(pre + i + 1);
				triangles.Add(2 * pre + 1);

				//壁
				triangles.Add(i);
				triangles.Add(pre + i + 1);
				triangles.Add(pre + i);

				triangles.Add(pre + i + 1);
				triangles.Add(i);
				triangles.Add(i + 1);

			}
		}
		mesh.Clear();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();
	}
	void Update () {
		if(Input.GetKeyDown(KeyCode.D))
		{
			LoopSubdivision.Smooth(mesh);
		}
	}
}
