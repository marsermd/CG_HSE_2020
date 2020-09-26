using System.Collections.Generic;
using UnityEngine;
using System.Linq;


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
        float size = 0.2f;

        for (int x = 0; x < (Field.MaxX() - Field.MinX()) / size; x++) {
            for (int y = 0; y < (Field.MaxY() - Field.MinY()) / size; y++) {
                for (int z = 0; z < (Field.MaxZ() - Field.MinZ()) / size; z++) {
                    var x0 = Field.MinX() + x * size;
                    var x1 = Field.MinX() + (x + 1) * size;
                    var y0 = Field.MinY() + y * size;
                    var y1 = Field.MinY() + (y + 1) * size;
                    var z0 = Field.MinZ() + z * size;
                    var z1 = Field.MinZ() + (z + 1) * size;
                    var cube = new List<Vector3> {
                        new Vector3(x0, y0, z0),
                        new Vector3(x0, y1, z0),
                        new Vector3(x1, y1, z0),
                        new Vector3(x1, y0, z0),
                        new Vector3(x0, y0, z1),
                        new Vector3(x0, y1, z1),
                        new Vector3(x1, y1, z1),
                        new Vector3(x1, y0, z1)
                    };
                    go(cube);
                }
            }
        }

        // Here unity automatically assumes that vertices are points and hence (x, y, z) will be represented as (x, y, z, 1) in homogenous coordinates
        _mesh.Clear();
        _mesh.SetVertices(vertices);
        _mesh.SetTriangles(indices, 0);
        _mesh.SetNormals(normals); // Use _mesh.SetNormals(normals) instead when you calculate them

        // Upload mesh data to the GPU
        _mesh.UploadMeshData(false);
    }

    private void go(List<Vector3> cube) {
        var mask = getMask(cube);

        for (int i = 0; i < MarchingCubes.Tables.CaseToTrianglesCount[mask]; i++) {
            var t = MarchingCubes.Tables.CaseToVertices[mask][i];
            var edges = new List<int>{t.x, t.y, t.z};
            for (int e = 0; e < 3; e++) {
                var a = cube[MarchingCubes.Tables._cubeEdges[edges[e]][0]];
                var b = cube[MarchingCubes.Tables._cubeEdges[edges[e]][1]];
                var p = Field.F(b) / (Field.F(b) - Field.F(a));
                var point = a * p + b * (1 - p);

                indices.Add(vertices.Count);
                vertices.Add(point);
                addNormal(point);
            }
        }
    }

    private int getMask(List<Vector3> cube) {
        var mask = 0;
        var fValues = cube.Select(vertex => Field.F(vertex)).ToList();
        for (var i = 0; i < fValues.Count; i++) {
            var flag = fValues[i] > 0 ? 1 : 0;
            mask |= flag << i;
        }
        return mask;
    }

    private void addNormal(Vector3 point) {
        float eps = 0.001f;
        var dx = new Vector3(eps, 0, 0);
        var dy = new Vector3(0, eps, 0);
        var dz = new Vector3(0, 0, eps);
        normals.Add(new Vector3(Field.F(point - dx) - Field.F(point + dx),
            Field.F(point - dy) - Field.F(point + dy),
            Field.F(point - dz) - Field.F(point + dz)));
    }
}