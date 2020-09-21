using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    private const float Step = 0.1f;
    private const float Eps = 0.001f;
    public MetaBallField Field = new MetaBallField();
    
    private MeshFilter _filter;
    private Mesh _mesh;
    
    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();
    private List<int> indices = new List<int>();

    /// <summary>
    /// Executed by Unity upon object initialization. <see cref="https://docs.unity3d.com/Manual/ExecutionOrder.html"/>
    /// </summary>
    private void Awake()
    {
        // Getting a component, responsible for storing the mesh
        _filter = GetComponent<MeshFilter>();
        
        // instantiating the mesh
        _mesh = _filter.mesh = new Mesh();
        
        // Just a little optimization, telling unity that the mesh is going to be updated frequently
        _mesh.MarkDynamic();
        
        GenerateMesh();
    }

    private void GenerateMesh()
    {
        vertices.Clear();
        indices.Clear();
        normals.Clear();
        
        Field.Update();
        // ----------------------------------------------------------------
        // Generate mesh here. Below is a sample code of a cube generation.
        // ----------------------------------------------------------------

        Bounds aabb = Field.GetBounds();
        for (float x = aabb.min.x - 2 * Step; x <= aabb.max.x + Step; x += Step)
        {
            for (float y = aabb.min.y - 2 * Step; y <= aabb.max.y + Step; y += Step)
            {
                for (float z = aabb.min.z - 2 * Step; z <= aabb.max.z + Step; z += Step)
                {
                    int mask = 0;
                    for (int i = 0; i < 8; ++i)
                    {
                        Vector3 vertex = MarchingCubes.Tables._cubeVertices[i];
                        Vector3 pos1 = new Vector3
                        (
                            x + vertex.x * Step,
                            y + vertex.y * Step,
                            z + vertex.z * Step
                        );
                        float f = Field.F(pos1);
                        mask |= (f > 0 ? 1 : 0) << i;
                    }
                    byte trianglesCount = MarchingCubes.Tables.CaseToTrianglesCount[mask];
                    for (int i = 0; i < trianglesCount; ++i)
                    {
                        int3 triangleEdges = MarchingCubes.Tables.CaseToVertices[mask][i];
                        for (int e = 0; e < 3; ++e)
                        {
                            int[] edge = MarchingCubes.Tables._cubeEdges[triangleEdges[e]];
                            Vector3 vertex1 = MarchingCubes.Tables._cubeVertices[edge[0]];
                            Vector3 vertex2 = MarchingCubes.Tables._cubeVertices[edge[1]];
                            Vector3 a = new Vector3
                            (
                                x + vertex1.x * Step,
                                y + vertex1.y * Step,
                                z + vertex1.z * Step
                            );
                            Vector3 b = new Vector3
                            (
                                x + vertex2.x * Step,
                                y + vertex2.y * Step,
                                z + vertex2.z * Step
                            );
                            float fa = Field.F(a);
                            float fb = Field.F(b);
                            float t = -fb / (fa - fb);
                            Vector3 v = new Vector3
                            (
                                a.x * t + b.x * (1 - t),
                                a.y * t + b.y * (1 - t),
                                a.z * t + b.z * (1 - t)
                            );
                            Vector3 dx = Vector3.right * Eps;
                            Vector3 dy = Vector3.up * Eps;
                            Vector3 dz = Vector3.forward * Eps;
                            Vector3 n = new Vector3
                            (
                                Field.F(v - dx) - Field.F(v + dx),
                                Field.F(v - dy) - Field.F(v + dy),
                                Field.F(v - dz) - Field.F(v + dz)
                            ).normalized;
                            indices.Add(vertices.Count);
                            vertices.Add(v);
                            normals.Add(n);
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Executed by Unity on every frame <see cref="https://docs.unity3d.com/Manual/ExecutionOrder.html"/>
    /// You can use it to animate something in runtime.
    /// </summary>
    private void Update()
    {
        // Here unity automatically assumes that vertices are points and hence (x, y, z) will be represented as (x, y, z, 1) in homogenous coordinates
        _mesh.Clear();
        _mesh.SetVertices(vertices);
        _mesh.SetTriangles(indices, 0);
        _mesh.SetNormals(normals);

        // Upload mesh data to the GPU
        _mesh.UploadMeshData(false);
    }
}