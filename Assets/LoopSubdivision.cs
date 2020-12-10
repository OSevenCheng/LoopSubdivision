using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System;
public class LoopSubdivision
{
    private static Vector3[] iVertices;
    private static int[] iFaces;
    private static Vertex[] mVertices;
    private static Dictionary<string, Edge> EdgesDic;
    private static Dictionary<string, int> Map_Edge_Vertex;
    private static Dictionary<string, List<EdgeVertex>> Map_Pos_EdgeVertex;
    private static string edgekey = "{0}_{1}";

    private static Dictionary<string, List<int>> PointMap;
    private static string Vector2String(Vector3 v)
    {
        StringBuilder str = new StringBuilder();
        str.Append(v.x).Append(",").Append(v.y).Append(",").Append(v.z);
        return str.ToString();
    }
    private static Vector3 String2Vector(string vstr)
    {
        try{
            string[] strings = vstr.Split(',');
            return new Vector3(float.Parse(strings[0]), float.Parse(strings[1]), float.Parse(strings[2]));
        }catch(Exception e){
            Debug.LogError(e.ToString());
            return Vector3.zero;
        }
    }

    private static float sourceVertexWeight = 0;
    private static float connectingVertexWeight = 0;
    private static float edgeVertexWeight = 0.375f;
    private static float adjacentVertexWeight = 0.125f;
    private static void BuildPointMap(Mesh mesh)
    {
        PointMap = new Dictionary<string, List<int>>(0);
        for (int i = 0, l = mesh.vertices.Length; i < l; i++)
        {
            string vstr = Vector2String(mesh.vertices[i]);
            if (!PointMap.ContainsKey(vstr))
            {
                PointMap.Add(vstr, new List<int>());
            }
            PointMap[vstr].Add(i);
        }
    }
    public static void Smooth(Mesh mesh)
    {
        iVertices = mesh.vertices;
        iFaces = mesh.triangles;
        /******************************************************
		 *
		 * Step 0: 
		 *
		 *******************************************************/
        

        mVertices = new Vertex[iVertices.Length];//创建一个和老顶点数组一样大的空数组，里面存的是使用该顶点做端点的边的数组
        EdgesDic = new Dictionary<string, Edge>(0); // Edge => { oldVertex1, oldVertex2, faces[]  }//边的表
        BuildPointMap(mesh);
        GenerateLookups();
        /******************************************************
		 *
		 * Step 1: 在每条边上插入一个顶点
		 *
		 *******************************************************/
        Map_Pos_EdgeVertex = new Dictionary<string, List<EdgeVertex>>(0);
        Map_Edge_Vertex= new Dictionary<string, int>();
        Vector3[] newEdgeVertices = new Vector3[EdgesDic.Count];
        
        int index = 0;
        foreach (var e in EdgesDic)//先找到每条边的中点，看看有没有相同位置的
        {
            Edge currentEdge = e.Value;
            Vector3 newEdgeVertex = Vector3.zero;
            // int connectedFaces = currentEdge.adjVertex.Count;
            // SetWeight1(connectedFaces);
            // newEdgeVertex += ((iVertices[currentEdge.a] + iVertices[currentEdge.b]) * edgeVertexWeight);

            // Vector3 tmp = Vector3.zero;
            // for (int j = 0; j < connectedFaces; j++)
            // {
            //     int v = currentEdge.adjVertex[j];
            //     tmp += iVertices[v];
            // }//边两旁的两个顶点。如果边的两边都是面，则权重为1/8，如果位于边界处，权重为0
            // newEdgeVertex += (tmp * adjacentVertexWeight);
            newEdgeVertex = iVertices[currentEdge.a]+iVertices[currentEdge.b];
            Map_Edge_Vertex[e.Key] = index;//记录的是新顶点的标号

             // for (int j = 0; j < connectedFaces; j++)
            // {
            //     int v = currentEdge.adjVertex[j];
            //     tmp += iVertices[v];
            // }//边两旁的两个顶点。如果边的两边都是面，则权重为1/8，如果位于边界处，权重为0

            string vstr = Vector2String(newEdgeVertex);
             if (!Map_Pos_EdgeVertex.ContainsKey(vstr))
            {
                Map_Pos_EdgeVertex.Add(vstr, new List<EdgeVertex>());
            }
            Map_Pos_EdgeVertex[vstr].Add(new EdgeVertex(index,currentEdge));
                                                        //currentEdge.edgeVertex = newEdgeVertices.Count;

            newEdgeVertices[index++] = newEdgeVertex;
        }
        foreach(var evs in Map_Pos_EdgeVertex)
        {
            List<EdgeVertex> edgeVertices = evs.Value;
            int connectedFaces=0;
            Vector3 tmp1 = String2Vector(evs.Key);
            Vector3 tmp2 = Vector3.zero;
            foreach(var ev in edgeVertices)
            {
                int c = ev.edge.adjVertex.Count;
                connectedFaces += c;
                for (int j = 0; j < c; j++)
                {
                    int v = ev.edge.adjVertex[j];
                    tmp2 += iVertices[v];
                }//边两旁的两个顶点。如果边的两边都是面，则权重为1/8，如果位于边界处，权重为0
            }
            SetWeight1(connectedFaces);
            Vector3 newPos = tmp1 * edgeVertexWeight + tmp2 * adjacentVertexWeight;
            foreach(var ev in edgeVertices)
            {
                newEdgeVertices[ev.edgeVertexIndex] = newPos;
            }
        }


        /******************************************************
        *
        * Step 2: 根据新插入的顶点，改变原始顶点的位置
        *
        *******************************************************/
        Vector3[] newSourceVertices = new Vector3[iVertices.Length];

        foreach (var point in PointMap)
        {//对每一个原始顶点
            List<int> PointsAtPos = point.Value;
            Vector3 oldVertex = iVertices[PointsAtPos[0]];
            Vector3 ConnPointPosAve = Vector3.zero;
            int n = 0;
            foreach (var p in PointsAtPos)
            {
                Vertex mVertex = mVertices[p];
                var vlist = mVertex.conVertexIndices;//与该点相连的顶点
                n += vlist.Count;
                foreach (var v in vlist)
                {
                    ConnPointPosAve += iVertices[v];
                }
            }
            SetWeight2(n);

            Vector3 newSourceVertex = Vector3.zero;
            newSourceVertex += (oldVertex * sourceVertexWeight);

            ConnPointPosAve *= connectingVertexWeight;
            newSourceVertex += ConnPointPosAve;

            foreach (var p in PointsAtPos)
            {
                newSourceVertices[p] = newSourceVertex;
            }

        }
        /******************************************************
        *
        * Step 3: 所有顶点构成新的面
        *
        *******************************************************/
        int sl = newSourceVertices.Length;
        Vector3[] newVertices = newSourceVertices.Concat(newEdgeVertices).ToArray<Vector3>();
        int[] newTriangles = new int[iFaces.Length * 4];
        for (int i = 0, l = iFaces.Length; i < l; i += 3)
        {
            int va = iFaces[i];
            int vb = iFaces[i + 1];
            int vc = iFaces[i + 2];

            // find the 3 new edges vertex of each old face

            int v1 = Map_Edge_Vertex[GetEdgeKey(va, vb)] + sl;
            int v2 = Map_Edge_Vertex[GetEdgeKey(vb, vc)] + sl;
            int v3 = Map_Edge_Vertex[GetEdgeKey(vc, va)] + sl;

            // create 4 faces.
            NewFace(newTriangles, i * 4, v1, v2, v3);
            NewFace(newTriangles, i * 4 + 3, va, v1, v3);
            NewFace(newTriangles, i * 4 + 6, vb, v2, v1);
            NewFace(newTriangles, i * 4 + 9, vc, v3, v2);
        }
        // Overwrite old arrays
        mesh.vertices = newVertices;
        mesh.triangles = newTriangles;
        mesh.SetIndices(newTriangles, MeshTopology.Triangles, 0);
        //if ( hasUvs ) geometry.faceVertexUvs = newUVs;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

    }
    private static void SetWeight1(int n)
    {
        edgeVertexWeight = 0.375f;
        adjacentVertexWeight = 0.125f;
        // check how many linked faces. 2 should be correct.
        //边上的两个顶点。如果边的两边都是面，则权重为3/8，如果位于边界处，权重为1/2。只能处理2个或者一个，如果一条边被三个面共有，则报错
        if (n != 2)
        {// if length is not 2, handle condition
            edgeVertexWeight = 0.5f;
            adjacentVertexWeight = 0;
            Debug.Log(n);
            if (n != 1)
            {
                // console.warn( 'Subdivision Modifier: Number of connected faces != 2, is: ', connectedFaces, currentEdge );
            }
        }
    }
    private static void SetWeight2(int n)
    {
        float beta = 0f;
        if (n == 3)
        {
            beta = 0.1875f;// 3f / 16f;
        }
        else if (n > 3)
        {
            beta = 0.375f / n; // Warren's modified formula
        }

        // Loop's original beta formula
        // beta = 1 / n * ( 5/8 - Math.pow( 3/8 + 1/4 * Math.cos( 2 * Math. PI / n ), 2) );

        sourceVertexWeight = 1f - n * beta;
        connectingVertexWeight = beta;

        if (n <= 2)
        {
            // crease and boundary rules
            // console.warn('crease and boundary rules');
            if (n == 2)
            {

                // console.warn( '2 connecting edges', connectingEdges );
                sourceVertexWeight = 0.75f;
                connectingVertexWeight = 0.125f;

                // sourceVertexWeight = 1;
                // connectingVertexWeight = 0;

            }
            else if (n == 1)
            {
                // console.warn( 'only 1 connecting edge' );
            }
            else if (n == 0)
            {
                // console.warn( '0 connecting edges' );
            }
        }
    }
    private static void NewFace(int[] newTri, int index, int a, int b, int c)
    {
        newTri[index] = a;
        newTri[index + 1] = b;
        newTri[index + 2] = c;
    }
    private static string GetEdgeKey(int a, int b)
    {
        var vertexIndexA = Mathf.Min(a, b);
        var vertexIndexB = Mathf.Max(a, b);
        return string.Format(edgekey, vertexIndexA, vertexIndexB);
    }
    private static void GenerateLookups()
    {
        for (int i = 0, l = mVertices.Length; i < l; i++)
        {
            mVertices[i].conVertexIndices = new List<int>(0);
        }
        for (int i = 0, l = iFaces.Length; i < l; i += 3)
        {
            ProcessEdge(iFaces[i], iFaces[i + 1], iFaces[i + 2]);
            ProcessEdge(iFaces[i + 1], iFaces[i + 2], iFaces[i]);
            ProcessEdge(iFaces[i + 2], iFaces[i], iFaces[i + 1]);
        }
    }
    private static void ProcessEdge(int a, int b, int c)
    {
        string key = GetEdgeKey(a, b);

        Edge edge;

        if (EdgesDic.ContainsKey(key))
        {
            edge = EdgesDic[key];
        }
        else
        {
            // var vertexA = iVertices[ a ];
            // var vertexB = iVertices[ b ];
            edge = new Edge(a, b);
            EdgesDic[key] = edge;
        }

        edge.adjVertex.Add(c);
        mVertices[a].conVertexIndices.Add(b);
        mVertices[b].conVertexIndices.Add(a);
    }
    struct Vertex
    {
        public List<int> conVertexIndices;
    }
    struct EdgeVertex
    {
        public EdgeVertex(int n,Edge e){edgeVertexIndex = n;edge = e;}
        public int edgeVertexIndex;
        public Edge edge;
    }
    struct Edge
    {
        public Edge(int _a, int _b)
        {
            a = _a;
            b = _b;
            adjVertex = new List<int>(0);
        }
        public int a;
        public int b;
        public List<int> adjVertex;
    }
}
