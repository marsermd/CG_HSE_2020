using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DefaultNamespace
{
    public class MarchingCube
    {
        public MarchingCube(Vector3 position, float sideLength)
        {
            Position = position;
            SideLength = sideLength;
            Vertices = MarchingCubes.Tables._cubeVertices
                .Select(dPosition => position + dPosition * sideLength)
                .ToList();
        }

        private int PlaneSliceIndex(Func<Vector3, float> planeFunction)
        {
            int index = 0;
            for (var i = 0; i < Vertices.Count; i++)
            {
                if (planeFunction(Vertices[i]) > 0)
                {
                    index |= 1 << i;
                }
            }

            if (index != 0)
            {
                Debug.Log(index);
            }

            return index;
        }

        public List<Vector3> GetPlaneSliceTriangleVertices(Func<Vector3, float> planeFunction)
        {
            var planeSliceTriangleVertices = new List<Vector3>();

            var planeSliceIndex = PlaneSliceIndex(planeFunction);
            var planeSliceCaseTriangles = MarchingCubes.Tables.CaseToVertices[planeSliceIndex];
            int planeSliceCaseTrianglesCount = MarchingCubes.Tables.CaseToTrianglesCount[planeSliceIndex];

            for (var i = 0; i < planeSliceCaseTrianglesCount; i++)
            {
                int[] triangleSideIndexes = {
                    planeSliceCaseTriangles[i][0],
                    planeSliceCaseTriangles[i][1],
                    planeSliceCaseTriangles[i][2]
                };

                foreach (var triangleSideIndex in triangleSideIndexes)
                {
                    var sideVerticesIndexes = MarchingCubes.Tables._cubeEdges[triangleSideIndex];
                    
                    var vertex0 = Vertices[sideVerticesIndexes[0]];
                    var vertex1 = Vertices[sideVerticesIndexes[1]];

                    var planeSliceVertex = planeFunction(vertex0) < 0 && planeFunction(vertex1) > 0 ?
                        Vector3.Lerp(vertex1, vertex0, -planeFunction(vertex1) / (planeFunction(vertex0) - planeFunction(vertex1))) :
                        Vector3.Lerp(vertex0, vertex1, -planeFunction(vertex0) / (planeFunction(vertex1) - planeFunction(vertex0)));
                    planeSliceTriangleVertices.Add(planeSliceVertex);
                }
            }

            return planeSliceTriangleVertices;
        }

        public float SideLength { get; }

        public Vector3 Position { get; }

        public List<Vector3> Vertices { get; }
    }
}