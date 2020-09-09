using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    private MeshFilter _filter;
    private Mesh _mesh;

    /// <summary>
    /// Executed by Unity upon object initialization. <see cref="https://docs.unity3d.com/Manual/ExecutionOrder.html"/>
    /// </summary>
    private void Awake()
    {
        _filter = GetComponent<MeshFilter>();
        _mesh = _filter.mesh = new Mesh();
        _mesh.MarkDynamic();
    }

    /// <summary>
    /// Executed by Unity on every frame <see cref="https://docs.unity3d.com/Manual/ExecutionOrder.html"/>
    /// You can use it to animate something in runtime.
    /// </summary>
    private void Update()
    {
        List<Vector3> sourceVertices = new List<Vector3>
        {
            new Vector3(0, 0, 0), // 0
            new Vector3(0, 1, 0), // 1
            new Vector3(1, 1, 0), // 2
            new Vector3(1, 0, 0), // 3
            new Vector3(0, 0, 1), // 4
            new Vector3(0, 1, 1), // 5
            new Vector3(1, 1, 1), // 6
            new Vector3(1, 0, 1), // 7
        };

        int[] sourceTriangles =
        {
            0, 1, 2, 2, 3, 0, // front
            3, 2, 6, 6, 7, 3, // right
            7, 6, 5, 5, 4, 7, // back
            0, 4, 5, 5, 1, 0, // left
            0, 3, 7, 7, 4, 0, // bottom
            1, 5, 6, 6, 2, 1, // top
        };

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // What is going to happen if we don't split the vertices? Check it out by yourself by passing
        // sourceVertices and sourceTriangles to the mesh.
        for (int i = 0; i < sourceTriangles.Length; i++)
        {
            triangles.Add(vertices.Count);
            Vector3 vertexPos = sourceVertices[sourceTriangles[i]];
            
            //Uncomment for some animation:
            //vertexPos += new Vector3
            //(
            //    Mathf.Sin(Time.time + vertexPos.z),
            //    Mathf.Sin(Time.time + vertexPos.y),
            //    Mathf.Sin(Time.time + vertexPos.x)
            //);
            
            vertices.Add(vertexPos);
        }

        // Here unity automatically assumes that vertices are points and hence will be represented as (x, y, z, 1) in homogenous coordinates
        _mesh.SetVertices(vertices);
        _mesh.SetTriangles(triangles, 0);
        _mesh.RecalculateNormals();

        // Upload mesh data to the GPU
        _mesh.UploadMeshData(false);
    }
}