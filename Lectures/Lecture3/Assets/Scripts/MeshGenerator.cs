using System.Collections.Generic;
using DefaultNamespace;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
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
    }

    private (Vector3, Vector3) GetBorderingPoints(Vector3[] ballPositions)
    {
        Vector3 minPoint = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 maxPoint = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        foreach (var ballPosition in ballPositions)
        {
            minPoint = Vector3.Min(minPoint, ballPosition);
            maxPoint = Vector3.Max(maxPoint, ballPosition);
        }
        
        minPoint -= new Vector3(Field.BallRadius, Field.BallRadius, Field.BallRadius);
        maxPoint += new Vector3(Field.BallRadius, Field.BallRadius, Field.BallRadius);

        return (minPoint, maxPoint);
    }

    private List<MarchingCube> GetMeshCubes(Vector3[] ballPositions, float meshCubeSize = 0.1f)
    {
        var (minPoint, maxPoint) = GetBorderingPoints(ballPositions);
        var cubeSlices = new List<MarchingCube>();

        for (var x = minPoint.x - meshCubeSize; x <= maxPoint.x + meshCubeSize; x += meshCubeSize)
        {
            for (var y = minPoint.y - meshCubeSize; y <= maxPoint.y + meshCubeSize; y += meshCubeSize)
            {
                for (var z = minPoint.z - meshCubeSize; z <= maxPoint.z + meshCubeSize; z += meshCubeSize)
                {
                    cubeSlices.Add(new MarchingCube(new Vector3(x, y, z), meshCubeSize));
                }
            }
        }

        return cubeSlices;
    }

    /// <summary>
    /// Executed by Unity on every frame <see cref="https://docs.unity3d.com/Manual/ExecutionOrder.html"/>
    /// You can use it to animate something in runtime.
    /// </summary>
    private void Update()
    {
        vertices.Clear();
        indices.Clear();
        normals.Clear();

        Field.Update();

        // ----------------------------------------------------------------
        // Generate mesh here. Below is a sample code of a cube generation.
        // ----------------------------------------------------------------

        // What is going to happen if we don't split the vertices? Check it out by yourself by passing
        // sourceVertices and sourceTriangles to the mesh.

        var meshCubes = GetMeshCubes(Field.BallPositions);
        foreach (var meshCube in meshCubes)
        {
            var planeSliceTriangleVertices = meshCube.GetPlaneSliceTriangleVertices(Field.F);
            foreach (var planeSliceTriangleVertex in planeSliceTriangleVertices)
            {
                var delta = 0.0001f;
                indices.Add(vertices.Count);
                vertices.Add(planeSliceTriangleVertex);
                normals.Add(Vector3.Normalize(new float3(
                    - Field.F(planeSliceTriangleVertex + new Vector3(delta, 0, 0)) + 
                    Field.F(planeSliceTriangleVertex - new Vector3(delta, 0, 0)),
                    - Field.F(planeSliceTriangleVertex + new Vector3(0, delta, 0)) +
                    Field.F(planeSliceTriangleVertex - new Vector3(0, delta, 0)),
                    - Field.F(planeSliceTriangleVertex + new Vector3(0, 0, delta)) +
                    Field.F(planeSliceTriangleVertex - new Vector3(0, 0, delta)))));
            }
        }

        // Here unity automatically assumes that vertices are points and hence (x, y, z) will be represented as (x, y, z, 1) in homogenous coordinates
        _mesh.Clear();
        _mesh.SetVertices(vertices);
        _mesh.SetTriangles(indices, 0);
        _mesh.SetNormals(normals);

        // Upload mesh data to the GPU
        _mesh.UploadMeshData(false);
    }
}