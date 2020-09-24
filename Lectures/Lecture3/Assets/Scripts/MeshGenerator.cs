using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    private const int Threads = 80;
    private const int ThreadsPerGroup = 10;
    private const int Groups = Threads / ThreadsPerGroup;
    private const float Step = 0.1f;
    private const float Eps = 0.001f;
    public MetaBallField Field = new MetaBallField();
    
    private MeshFilter _filter;
    private Mesh _mesh;
    
    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();
    private List<int> indices = new List<int>();

    private ComputeShader _shader;
    private ComputeBuffer _cubeVerticesBuffer;
    private ComputeBuffer _cubeEdgesBuffer;
    private ComputeBuffer _caseToTrianglesCountBuffer;
    private ComputeBuffer _caseToVerticesBuffer;
    private ComputeBuffer _verticesBuffer;
    private ComputeBuffer _countBuffer;
    private ComputeBuffer _ballsBuffer;

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

        _shader = Resources.Load<ComputeShader>("GenerateMesh");
        InitShaderValues();
    }

    private void GenerateMeshGPU()
    {
        vertices.Clear();
        indices.Clear();
        normals.Clear();
        Field.Update();
        Field.UpdateBuffer(_ballsBuffer);

        int kernel = _shader.FindKernel("Generate");
        ComputeBuffer.CopyCount(_verticesBuffer, _countBuffer, 0);
        _verticesBuffer.SetCounterValue(0);
        _shader.Dispatch(kernel, Groups, Groups, Groups);
        ComputeBuffer.CopyCount(_verticesBuffer, _countBuffer, 0);
        int[] count = {0};
        _countBuffer.GetData(count);
        float3[] verts = new float3[count[0] * 3 * 2];
        _verticesBuffer.GetData(verts);
        for (int i = 0; i < verts.Length; i += 3 * 2)
        {
            for (int j = 0; j < 3; ++j)
            {
                indices.Add(vertices.Count);
                vertices.Add(verts[i + j]);
                normals.Add(verts[i + 3 + j]);
            }
        }
    }

    private void InitShaderValues()
    {
        int kernel = _shader.FindKernel("Generate");
        
        _cubeVerticesBuffer = new ComputeBuffer(MarchingCubes.FlatTables._cubeVertices.Length * 3,
            sizeof(float) * 3,
            ComputeBufferType.Structured);
        _cubeVerticesBuffer.SetData(MarchingCubes.FlatTables._cubeVertices);
        _shader.SetBuffer(kernel, "cubeVertices", _cubeVerticesBuffer);

        _cubeEdgesBuffer = new ComputeBuffer(MarchingCubes.FlatTables._cubeEdges.Length,
            sizeof(int) * 2,
            ComputeBufferType.Structured);
        _cubeEdgesBuffer.SetData(MarchingCubes.FlatTables._cubeEdges);
        _shader.SetBuffer(kernel, "cubeEdges", _cubeEdgesBuffer);

        _caseToTrianglesCountBuffer = new ComputeBuffer(MarchingCubes.FlatTables.CaseToTrianglesCount.Length,
            sizeof(int),
            ComputeBufferType.Structured);
        _caseToTrianglesCountBuffer.SetData(MarchingCubes.FlatTables.CaseToTrianglesCount);
        _shader.SetBuffer(kernel, "caseToTrianglesCount", _caseToTrianglesCountBuffer);

        _caseToVerticesBuffer = new ComputeBuffer(MarchingCubes.FlatTables.CaseToVertices.Length / 5,
            sizeof(int) * 3 * 5,
            ComputeBufferType.Structured);
        _caseToVerticesBuffer.SetData(MarchingCubes.FlatTables.CaseToVertices);
        _shader.SetBuffer(kernel, "caseToVertices", _caseToVerticesBuffer);
        
        _verticesBuffer = new ComputeBuffer(Threads * Threads * Threads * 5,
            sizeof(float) * 3 * 3 * 2,
            ComputeBufferType.Append);
        _shader.SetBuffer(kernel, "vertexBuffer", _verticesBuffer);

        _countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Default);

        _ballsBuffer = new ComputeBuffer(Field.Balls.Length, sizeof(float) * 3, ComputeBufferType.Structured);
        _shader.SetBuffer(kernel, "ballPositions", _ballsBuffer);
        
        _shader.SetInt("numBalls", Field.Balls.Length);
        _shader.SetFloat("ballRadius", Field.BallRadius);
    }

    private void GenerateMeshCPU()
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
        GenerateMeshGPU();
        // Here unity automatically assumes that vertices are points
        // and hence (x, y, z) will be represented as (x, y, z, 1) in homogenous coordinates
        _mesh.Clear();
        _mesh.SetVertices(vertices);
        _mesh.SetTriangles(indices, 0);
        _mesh.SetNormals(normals);

        // Upload mesh data to the GPU
        _mesh.UploadMeshData(false);
    }

    private void OnDestroy()
    {
        _cubeVerticesBuffer.Release();
        _cubeEdgesBuffer.Release();
        _caseToTrianglesCountBuffer.Release();
        _caseToVerticesBuffer.Release();
        _verticesBuffer.Release();
        _countBuffer.Release();
        _ballsBuffer.Release();
    }
}