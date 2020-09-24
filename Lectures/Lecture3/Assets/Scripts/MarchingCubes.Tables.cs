using Unity.Mathematics;
using UnityEngine;

public partial class MarchingCubes
{
    /// <summary>
    /// Converted from https://github.com/QianMo/GPU-Gems-Book-Source-Code/blob/master/GPU-Gems-3-CD-Content/content/01/demo/models/tables.nma
    /// CaseIndex can be calculated as a bit mask. I.e. index[i] = F(vertex[i]) > c
    /// Where F is the scalar field;
    /// Index is an int interpreted as an array of bits with index[0] being the least significant bit;
    /// vertex[i] is the coordinate of i'th vertex.
    ///
    /// See lecture slides for details.
    /// </summary>
    public class Tables
    {
        /// <summary>
        /// Positions of vertices of a single cube.
        /// </summary>
        public static Vector3[] _cubeVertices = new Vector3[]
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
        
        /// <summary>
        /// Indices of vertices for a given edge
        /// </summary>
        public static readonly int[][] _cubeEdges = new int[][]
        {
            new int[] {0, 1}, // 0
            new int[] {1, 2}, // 1
            new int[] {2, 3}, // 2
            new int[] {3, 0}, // 3
            new int[] {4, 5}, // 4
            new int[] {5, 6}, // 5
            new int[] {6, 7}, // 6
            new int[] {7, 4}, // 7
            new int[] {0, 4}, // 8
            new int[] {1, 5}, // 9
            new int[] {2, 6}, // 10
            new int[] {3, 7}, // 11
        };
        
        /// <summary>
        /// How many triangles should be there in a cube for a given CaseIndex?
        /// </summary>
        public static readonly byte[] CaseToTrianglesCount = new byte[]
        {
            0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 2, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 3,
            1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 3, 2, 3, 3, 2, 3, 4, 4, 3, 3, 4, 4, 3, 4, 5, 5, 2,
            1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 3, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 4,
            2, 3, 3, 4, 3, 4, 2, 3, 3, 4, 4, 5, 4, 5, 3, 2, 3, 4, 4, 3, 4, 5, 3, 2, 4, 5, 5, 4, 5, 2, 4, 1,
            1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 3, 2, 3, 3, 4, 3, 4, 4, 5, 3, 2, 4, 3, 4, 3, 5, 2,
            2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 4, 3, 4, 4, 3, 4, 5, 5, 4, 4, 3, 5, 2, 5, 4, 2, 1,
            2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 2, 3, 3, 2, 3, 4, 4, 5, 4, 5, 5, 2, 4, 3, 5, 4, 3, 2, 4, 1,
            3, 4, 4, 5, 4, 5, 3, 4, 4, 5, 5, 2, 3, 4, 2, 1, 2, 3, 3, 2, 3, 4, 2, 1, 3, 2, 4, 1, 2, 1, 1, 0
        };

        /// <summary>
        /// Given a case index, on which edged should the triangle's vertices lie? -1 means no edge.
        /// For details on edge indexing please refer to the lecture slides.
        /// </summary>
        // Unfortunately there is no such struct as byte3 in Unity
        public static readonly int3[][] CaseToVertices = 
        {
            new[]
            {
                new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(0, 8, 3), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(0, 1, 9), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(1, 8, 3), new int3(9, 8, 1), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(1, 2, 10), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(0, 8, 3), new int3(1, 2, 10), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(9, 2, 10), new int3(0, 2, 9), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(2, 8, 3), new int3(2, 10, 8), new int3(10, 9, 8), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(3, 11, 2), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(0, 11, 2), new int3(8, 11, 0), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(1, 9, 0), new int3(2, 3, 11), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(1, 11, 2), new int3(1, 9, 11), new int3(9, 8, 11), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(3, 10, 1), new int3(11, 10, 3), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(0, 10, 1), new int3(0, 8, 10), new int3(8, 11, 10), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(3, 9, 0), new int3(3, 11, 9), new int3(11, 10, 9), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(9, 8, 10), new int3(10, 8, 11), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(4, 7, 8), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(4, 3, 0), new int3(7, 3, 4), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(0, 1, 9), new int3(8, 4, 7), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(4, 1, 9), new int3(4, 7, 1), new int3(7, 3, 1), new int3(-1, -1, -1), new int3(-1, -1, -1)},
            new[]
            {
                new int3(1, 2, 10), new int3(8, 4, 7), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(3, 4, 7), new int3(3, 0, 4), new int3(1, 2, 10), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(9, 2, 10), new int3(9, 0, 2), new int3(8, 4, 7), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(2, 10, 9), new int3(2, 9, 7), new int3(2, 7, 3), new int3(7, 9, 4), new int3(-1, -1, -1)},
            new[]
            {
                new int3(8, 4, 7), new int3(3, 11, 2), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(11, 4, 7), new int3(11, 2, 4), new int3(2, 0, 4), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(9, 0, 1), new int3(8, 4, 7), new int3(2, 3, 11), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(4, 7, 11), new int3(9, 4, 11), new int3(9, 11, 2), new int3(9, 2, 1), new int3(-1, -1, -1)},
            new[]
            {
                new int3(3, 10, 1), new int3(3, 11, 10), new int3(7, 8, 4), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(1, 11, 10), new int3(1, 4, 11), new int3(1, 0, 4), new int3(7, 11, 4), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(4, 7, 8), new int3(9, 0, 11), new int3(9, 11, 10), new int3(11, 0, 3), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(4, 7, 11), new int3(4, 11, 9), new int3(9, 11, 10), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(9, 5, 4), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(9, 5, 4), new int3(0, 8, 3), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(0, 5, 4), new int3(1, 5, 0), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(8, 5, 4), new int3(8, 3, 5), new int3(3, 1, 5), new int3(-1, -1, -1), new int3(-1, -1, -1)},
            new[]
            {
                new int3(1, 2, 10), new int3(9, 5, 4), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(3, 0, 8), new int3(1, 2, 10), new int3(4, 9, 5), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(5, 2, 10), new int3(5, 4, 2), new int3(4, 0, 2), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(2, 10, 5), new int3(3, 2, 5), new int3(3, 5, 4), new int3(3, 4, 8), new int3(-1, -1, -1)},
            new[]
            {
                new int3(9, 5, 4), new int3(2, 3, 11), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(0, 11, 2), new int3(0, 8, 11), new int3(4, 9, 5), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(0, 5, 4), new int3(0, 1, 5), new int3(2, 3, 11), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(2, 1, 5), new int3(2, 5, 8), new int3(2, 8, 11), new int3(4, 8, 5), new int3(-1, -1, -1)},
            new[]
            {
                new int3(10, 3, 11), new int3(10, 1, 3), new int3(9, 5, 4), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(4, 9, 5), new int3(0, 8, 1), new int3(8, 10, 1), new int3(8, 11, 10), new int3(-1, -1, -1)},
            new[]
            {
                new int3(5, 4, 0), new int3(5, 0, 11), new int3(5, 11, 10), new int3(11, 0, 3), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(5, 4, 8), new int3(5, 8, 10), new int3(10, 8, 11), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(9, 7, 8), new int3(5, 7, 9), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(9, 3, 0), new int3(9, 5, 3), new int3(5, 7, 3), new int3(-1, -1, -1), new int3(-1, -1, -1)},
            new[] {new int3(0, 7, 8), new int3(0, 1, 7), new int3(1, 5, 7), new int3(-1, -1, -1), new int3(-1, -1, -1)},
            new[]
            {
                new int3(1, 5, 3), new int3(3, 5, 7), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(9, 7, 8), new int3(9, 5, 7), new int3(10, 1, 2), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(10, 1, 2), new int3(9, 5, 0), new int3(5, 3, 0), new int3(5, 7, 3), new int3(-1, -1, -1)},
            new[] {new int3(8, 0, 2), new int3(8, 2, 5), new int3(8, 5, 7), new int3(10, 5, 2), new int3(-1, -1, -1)},
            new[]
            {
                new int3(2, 10, 5), new int3(2, 5, 3), new int3(3, 5, 7), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(7, 9, 5), new int3(7, 8, 9), new int3(3, 11, 2), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(9, 5, 7), new int3(9, 7, 2), new int3(9, 2, 0), new int3(2, 7, 11), new int3(-1, -1, -1)},
            new[] {new int3(2, 3, 11), new int3(0, 1, 8), new int3(1, 7, 8), new int3(1, 5, 7), new int3(-1, -1, -1)},
            new[]
            {
                new int3(11, 2, 1), new int3(11, 1, 7), new int3(7, 1, 5), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(9, 5, 8), new int3(8, 5, 7), new int3(10, 1, 3), new int3(10, 3, 11), new int3(-1, -1, -1)},
            new[] {new int3(5, 7, 0), new int3(5, 0, 9), new int3(7, 11, 0), new int3(1, 0, 10), new int3(11, 10, 0)},
            new[] {new int3(11, 10, 0), new int3(11, 0, 3), new int3(10, 5, 0), new int3(8, 0, 7), new int3(5, 7, 0)},
            new[]
            {
                new int3(11, 10, 5), new int3(7, 11, 5), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(10, 6, 5), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(0, 8, 3), new int3(5, 10, 6), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(9, 0, 1), new int3(5, 10, 6), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(1, 8, 3), new int3(1, 9, 8), new int3(5, 10, 6), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(1, 6, 5), new int3(2, 6, 1), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(1, 6, 5), new int3(1, 2, 6), new int3(3, 0, 8), new int3(-1, -1, -1), new int3(-1, -1, -1)},
            new[] {new int3(9, 6, 5), new int3(9, 0, 6), new int3(0, 2, 6), new int3(-1, -1, -1), new int3(-1, -1, -1)},
            new[] {new int3(5, 9, 8), new int3(5, 8, 2), new int3(5, 2, 6), new int3(3, 2, 8), new int3(-1, -1, -1)},
            new[]
            {
                new int3(2, 3, 11), new int3(10, 6, 5), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(11, 0, 8), new int3(11, 2, 0), new int3(10, 6, 5), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(0, 1, 9), new int3(2, 3, 11), new int3(5, 10, 6), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(5, 10, 6), new int3(1, 9, 2), new int3(9, 11, 2), new int3(9, 8, 11), new int3(-1, -1, -1)},
            new[]
            {
                new int3(6, 3, 11), new int3(6, 5, 3), new int3(5, 1, 3), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(0, 8, 11), new int3(0, 11, 5), new int3(0, 5, 1), new int3(5, 11, 6), new int3(-1, -1, -1)},
            new[] {new int3(3, 11, 6), new int3(0, 3, 6), new int3(0, 6, 5), new int3(0, 5, 9), new int3(-1, -1, -1)},
            new[]
            {
                new int3(6, 5, 9), new int3(6, 9, 11), new int3(11, 9, 8), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(5, 10, 6), new int3(4, 7, 8), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(4, 3, 0), new int3(4, 7, 3), new int3(6, 5, 10), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(1, 9, 0), new int3(5, 10, 6), new int3(8, 4, 7), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(10, 6, 5), new int3(1, 9, 7), new int3(1, 7, 3), new int3(7, 9, 4), new int3(-1, -1, -1)},
            new[] {new int3(6, 1, 2), new int3(6, 5, 1), new int3(4, 7, 8), new int3(-1, -1, -1), new int3(-1, -1, -1)},
            new[] {new int3(1, 2, 5), new int3(5, 2, 6), new int3(3, 0, 4), new int3(3, 4, 7), new int3(-1, -1, -1)},
            new[] {new int3(8, 4, 7), new int3(9, 0, 5), new int3(0, 6, 5), new int3(0, 2, 6), new int3(-1, -1, -1)},
            new[] {new int3(7, 3, 9), new int3(7, 9, 4), new int3(3, 2, 9), new int3(5, 9, 6), new int3(2, 6, 9)},
            new[]
            {
                new int3(3, 11, 2), new int3(7, 8, 4), new int3(10, 6, 5), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(5, 10, 6), new int3(4, 7, 2), new int3(4, 2, 0), new int3(2, 7, 11), new int3(-1, -1, -1)},
            new[] {new int3(0, 1, 9), new int3(4, 7, 8), new int3(2, 3, 11), new int3(5, 10, 6), new int3(-1, -1, -1)},
            new[] {new int3(9, 2, 1), new int3(9, 11, 2), new int3(9, 4, 11), new int3(7, 11, 4), new int3(5, 10, 6)},
            new[] {new int3(8, 4, 7), new int3(3, 11, 5), new int3(3, 5, 1), new int3(5, 11, 6), new int3(-1, -1, -1)},
            new[] {new int3(5, 1, 11), new int3(5, 11, 6), new int3(1, 0, 11), new int3(7, 11, 4), new int3(0, 4, 11)},
            new[] {new int3(0, 5, 9), new int3(0, 6, 5), new int3(0, 3, 6), new int3(11, 6, 3), new int3(8, 4, 7)},
            new[] {new int3(6, 5, 9), new int3(6, 9, 11), new int3(4, 7, 9), new int3(7, 11, 9), new int3(-1, -1, -1)},
            new[]
            {
                new int3(10, 4, 9), new int3(6, 4, 10), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(4, 10, 6), new int3(4, 9, 10), new int3(0, 8, 3), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(10, 0, 1), new int3(10, 6, 0), new int3(6, 4, 0), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(8, 3, 1), new int3(8, 1, 6), new int3(8, 6, 4), new int3(6, 1, 10), new int3(-1, -1, -1)},
            new[] {new int3(1, 4, 9), new int3(1, 2, 4), new int3(2, 6, 4), new int3(-1, -1, -1), new int3(-1, -1, -1)},
            new[] {new int3(3, 0, 8), new int3(1, 2, 9), new int3(2, 4, 9), new int3(2, 6, 4), new int3(-1, -1, -1)},
            new[]
            {
                new int3(0, 2, 4), new int3(4, 2, 6), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(8, 3, 2), new int3(8, 2, 4), new int3(4, 2, 6), new int3(-1, -1, -1), new int3(-1, -1, -1)},
            new[]
            {
                new int3(10, 4, 9), new int3(10, 6, 4), new int3(11, 2, 3), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(0, 8, 2), new int3(2, 8, 11), new int3(4, 9, 10), new int3(4, 10, 6), new int3(-1, -1, -1)},
            new[] {new int3(3, 11, 2), new int3(0, 1, 6), new int3(0, 6, 4), new int3(6, 1, 10), new int3(-1, -1, -1)},
            new[] {new int3(6, 4, 1), new int3(6, 1, 10), new int3(4, 8, 1), new int3(2, 1, 11), new int3(8, 11, 1)},
            new[] {new int3(9, 6, 4), new int3(9, 3, 6), new int3(9, 1, 3), new int3(11, 6, 3), new int3(-1, -1, -1)},
            new[] {new int3(8, 11, 1), new int3(8, 1, 0), new int3(11, 6, 1), new int3(9, 1, 4), new int3(6, 4, 1)},
            new[]
            {
                new int3(3, 11, 6), new int3(3, 6, 0), new int3(0, 6, 4), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(6, 4, 8), new int3(11, 6, 8), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(7, 10, 6), new int3(7, 8, 10), new int3(8, 9, 10), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(0, 7, 3), new int3(0, 10, 7), new int3(0, 9, 10), new int3(6, 7, 10), new int3(-1, -1, -1)},
            new[] {new int3(10, 6, 7), new int3(1, 10, 7), new int3(1, 7, 8), new int3(1, 8, 0), new int3(-1, -1, -1)},
            new[]
            {
                new int3(10, 6, 7), new int3(10, 7, 1), new int3(1, 7, 3), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(1, 2, 6), new int3(1, 6, 8), new int3(1, 8, 9), new int3(8, 6, 7), new int3(-1, -1, -1)},
            new[] {new int3(2, 6, 9), new int3(2, 9, 1), new int3(6, 7, 9), new int3(0, 9, 3), new int3(7, 3, 9)},
            new[] {new int3(7, 8, 0), new int3(7, 0, 6), new int3(6, 0, 2), new int3(-1, -1, -1), new int3(-1, -1, -1)},
            new[]
            {
                new int3(7, 3, 2), new int3(6, 7, 2), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(2, 3, 11), new int3(10, 6, 8), new int3(10, 8, 9), new int3(8, 6, 7), new int3(-1, -1, -1)},
            new[] {new int3(2, 0, 7), new int3(2, 7, 11), new int3(0, 9, 7), new int3(6, 7, 10), new int3(9, 10, 7)},
            new[] {new int3(1, 8, 0), new int3(1, 7, 8), new int3(1, 10, 7), new int3(6, 7, 10), new int3(2, 3, 11)},
            new[] {new int3(11, 2, 1), new int3(11, 1, 7), new int3(10, 6, 1), new int3(6, 7, 1), new int3(-1, -1, -1)},
            new[] {new int3(8, 9, 6), new int3(8, 6, 7), new int3(9, 1, 6), new int3(11, 6, 3), new int3(1, 3, 6)},
            new[]
            {
                new int3(0, 9, 1), new int3(11, 6, 7), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(7, 8, 0), new int3(7, 0, 6), new int3(3, 11, 0), new int3(11, 6, 0), new int3(-1, -1, -1)},
            new[]
            {
                new int3(7, 11, 6), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(7, 6, 11), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(3, 0, 8), new int3(11, 7, 6), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(0, 1, 9), new int3(11, 7, 6), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(8, 1, 9), new int3(8, 3, 1), new int3(11, 7, 6), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(10, 1, 2), new int3(6, 11, 7), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(1, 2, 10), new int3(3, 0, 8), new int3(6, 11, 7), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(2, 9, 0), new int3(2, 10, 9), new int3(6, 11, 7), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(6, 11, 7), new int3(2, 10, 3), new int3(10, 8, 3), new int3(10, 9, 8), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(7, 2, 3), new int3(6, 2, 7), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(7, 0, 8), new int3(7, 6, 0), new int3(6, 2, 0), new int3(-1, -1, -1), new int3(-1, -1, -1)},
            new[] {new int3(2, 7, 6), new int3(2, 3, 7), new int3(0, 1, 9), new int3(-1, -1, -1), new int3(-1, -1, -1)},
            new[] {new int3(1, 6, 2), new int3(1, 8, 6), new int3(1, 9, 8), new int3(8, 7, 6), new int3(-1, -1, -1)},
            new[]
            {
                new int3(10, 7, 6), new int3(10, 1, 7), new int3(1, 3, 7), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(10, 7, 6), new int3(1, 7, 10), new int3(1, 8, 7), new int3(1, 0, 8), new int3(-1, -1, -1)},
            new[] {new int3(0, 3, 7), new int3(0, 7, 10), new int3(0, 10, 9), new int3(6, 10, 7), new int3(-1, -1, -1)},
            new[]
            {
                new int3(7, 6, 10), new int3(7, 10, 8), new int3(8, 10, 9), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(6, 8, 4), new int3(11, 8, 6), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(3, 6, 11), new int3(3, 0, 6), new int3(0, 4, 6), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(8, 6, 11), new int3(8, 4, 6), new int3(9, 0, 1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(9, 4, 6), new int3(9, 6, 3), new int3(9, 3, 1), new int3(11, 3, 6), new int3(-1, -1, -1)},
            new[]
            {
                new int3(6, 8, 4), new int3(6, 11, 8), new int3(2, 10, 1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(1, 2, 10), new int3(3, 0, 11), new int3(0, 6, 11), new int3(0, 4, 6), new int3(-1, -1, -1)},
            new[] {new int3(4, 11, 8), new int3(4, 6, 11), new int3(0, 2, 9), new int3(2, 10, 9), new int3(-1, -1, -1)},
            new[] {new int3(10, 9, 3), new int3(10, 3, 2), new int3(9, 4, 3), new int3(11, 3, 6), new int3(4, 6, 3)},
            new[] {new int3(8, 2, 3), new int3(8, 4, 2), new int3(4, 6, 2), new int3(-1, -1, -1), new int3(-1, -1, -1)},
            new[]
            {
                new int3(0, 4, 2), new int3(4, 6, 2), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(1, 9, 0), new int3(2, 3, 4), new int3(2, 4, 6), new int3(4, 3, 8), new int3(-1, -1, -1)},
            new[] {new int3(1, 9, 4), new int3(1, 4, 2), new int3(2, 4, 6), new int3(-1, -1, -1), new int3(-1, -1, -1)},
            new[] {new int3(8, 1, 3), new int3(8, 6, 1), new int3(8, 4, 6), new int3(6, 10, 1), new int3(-1, -1, -1)},
            new[]
            {
                new int3(10, 1, 0), new int3(10, 0, 6), new int3(6, 0, 4), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(4, 6, 3), new int3(4, 3, 8), new int3(6, 10, 3), new int3(0, 3, 9), new int3(10, 9, 3)},
            new[]
            {
                new int3(10, 9, 4), new int3(6, 10, 4), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(4, 9, 5), new int3(7, 6, 11), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(0, 8, 3), new int3(4, 9, 5), new int3(11, 7, 6), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(5, 0, 1), new int3(5, 4, 0), new int3(7, 6, 11), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(11, 7, 6), new int3(8, 3, 4), new int3(3, 5, 4), new int3(3, 1, 5), new int3(-1, -1, -1)},
            new[]
            {
                new int3(9, 5, 4), new int3(10, 1, 2), new int3(7, 6, 11), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(6, 11, 7), new int3(1, 2, 10), new int3(0, 8, 3), new int3(4, 9, 5), new int3(-1, -1, -1)},
            new[] {new int3(7, 6, 11), new int3(5, 4, 10), new int3(4, 2, 10), new int3(4, 0, 2), new int3(-1, -1, -1)},
            new[] {new int3(3, 4, 8), new int3(3, 5, 4), new int3(3, 2, 5), new int3(10, 5, 2), new int3(11, 7, 6)},
            new[] {new int3(7, 2, 3), new int3(7, 6, 2), new int3(5, 4, 9), new int3(-1, -1, -1), new int3(-1, -1, -1)},
            new[] {new int3(9, 5, 4), new int3(0, 8, 6), new int3(0, 6, 2), new int3(6, 8, 7), new int3(-1, -1, -1)},
            new[] {new int3(3, 6, 2), new int3(3, 7, 6), new int3(1, 5, 0), new int3(5, 4, 0), new int3(-1, -1, -1)},
            new[] {new int3(6, 2, 8), new int3(6, 8, 7), new int3(2, 1, 8), new int3(4, 8, 5), new int3(1, 5, 8)},
            new[] {new int3(9, 5, 4), new int3(10, 1, 6), new int3(1, 7, 6), new int3(1, 3, 7), new int3(-1, -1, -1)},
            new[] {new int3(1, 6, 10), new int3(1, 7, 6), new int3(1, 0, 7), new int3(8, 7, 0), new int3(9, 5, 4)},
            new[] {new int3(4, 0, 10), new int3(4, 10, 5), new int3(0, 3, 10), new int3(6, 10, 7), new int3(3, 7, 10)},
            new[]
            {
                new int3(7, 6, 10), new int3(7, 10, 8), new int3(5, 4, 10), new int3(4, 8, 10), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(6, 9, 5), new int3(6, 11, 9), new int3(11, 8, 9), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(3, 6, 11), new int3(0, 6, 3), new int3(0, 5, 6), new int3(0, 9, 5), new int3(-1, -1, -1)},
            new[] {new int3(0, 11, 8), new int3(0, 5, 11), new int3(0, 1, 5), new int3(5, 6, 11), new int3(-1, -1, -1)},
            new[]
            {
                new int3(6, 11, 3), new int3(6, 3, 5), new int3(5, 3, 1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(1, 2, 10), new int3(9, 5, 11), new int3(9, 11, 8), new int3(11, 5, 6), new int3(-1, -1, -1)
            },
            new[] {new int3(0, 11, 3), new int3(0, 6, 11), new int3(0, 9, 6), new int3(5, 6, 9), new int3(1, 2, 10)},
            new[] {new int3(11, 8, 5), new int3(11, 5, 6), new int3(8, 0, 5), new int3(10, 5, 2), new int3(0, 2, 5)},
            new[] {new int3(6, 11, 3), new int3(6, 3, 5), new int3(2, 10, 3), new int3(10, 5, 3), new int3(-1, -1, -1)},
            new[] {new int3(5, 8, 9), new int3(5, 2, 8), new int3(5, 6, 2), new int3(3, 8, 2), new int3(-1, -1, -1)},
            new[] {new int3(9, 5, 6), new int3(9, 6, 0), new int3(0, 6, 2), new int3(-1, -1, -1), new int3(-1, -1, -1)},
            new[] {new int3(1, 5, 8), new int3(1, 8, 0), new int3(5, 6, 8), new int3(3, 8, 2), new int3(6, 2, 8)},
            new[]
            {
                new int3(1, 5, 6), new int3(2, 1, 6), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(1, 3, 6), new int3(1, 6, 10), new int3(3, 8, 6), new int3(5, 6, 9), new int3(8, 9, 6)},
            new[] {new int3(10, 1, 0), new int3(10, 0, 6), new int3(9, 5, 0), new int3(5, 6, 0), new int3(-1, -1, -1)},
            new[]
            {
                new int3(0, 3, 8), new int3(5, 6, 10), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(10, 5, 6), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(11, 5, 10), new int3(7, 5, 11), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(11, 5, 10), new int3(11, 7, 5), new int3(8, 3, 0), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(5, 11, 7), new int3(5, 10, 11), new int3(1, 9, 0), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(10, 7, 5), new int3(10, 11, 7), new int3(9, 8, 1), new int3(8, 3, 1), new int3(-1, -1, -1)},
            new[]
            {
                new int3(11, 1, 2), new int3(11, 7, 1), new int3(7, 5, 1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(0, 8, 3), new int3(1, 2, 7), new int3(1, 7, 5), new int3(7, 2, 11), new int3(-1, -1, -1)},
            new[] {new int3(9, 7, 5), new int3(9, 2, 7), new int3(9, 0, 2), new int3(2, 11, 7), new int3(-1, -1, -1)},
            new[] {new int3(7, 5, 2), new int3(7, 2, 11), new int3(5, 9, 2), new int3(3, 2, 8), new int3(9, 8, 2)},
            new[]
            {
                new int3(2, 5, 10), new int3(2, 3, 5), new int3(3, 7, 5), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(8, 2, 0), new int3(8, 5, 2), new int3(8, 7, 5), new int3(10, 2, 5), new int3(-1, -1, -1)},
            new[] {new int3(9, 0, 1), new int3(5, 10, 3), new int3(5, 3, 7), new int3(3, 10, 2), new int3(-1, -1, -1)},
            new[] {new int3(9, 8, 2), new int3(9, 2, 1), new int3(8, 7, 2), new int3(10, 2, 5), new int3(7, 5, 2)},
            new[]
            {
                new int3(1, 3, 5), new int3(3, 7, 5), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(0, 8, 7), new int3(0, 7, 1), new int3(1, 7, 5), new int3(-1, -1, -1), new int3(-1, -1, -1)},
            new[] {new int3(9, 0, 3), new int3(9, 3, 5), new int3(5, 3, 7), new int3(-1, -1, -1), new int3(-1, -1, -1)},
            new[]
            {
                new int3(9, 8, 7), new int3(5, 9, 7), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(5, 8, 4), new int3(5, 10, 8), new int3(10, 11, 8), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(5, 0, 4), new int3(5, 11, 0), new int3(5, 10, 11), new int3(11, 3, 0), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(0, 1, 9), new int3(8, 4, 10), new int3(8, 10, 11), new int3(10, 4, 5), new int3(-1, -1, -1)
            },
            new[] {new int3(10, 11, 4), new int3(10, 4, 5), new int3(11, 3, 4), new int3(9, 4, 1), new int3(3, 1, 4)},
            new[] {new int3(2, 5, 1), new int3(2, 8, 5), new int3(2, 11, 8), new int3(4, 5, 8), new int3(-1, -1, -1)},
            new[] {new int3(0, 4, 11), new int3(0, 11, 3), new int3(4, 5, 11), new int3(2, 11, 1), new int3(5, 1, 11)},
            new[] {new int3(0, 2, 5), new int3(0, 5, 9), new int3(2, 11, 5), new int3(4, 5, 8), new int3(11, 8, 5)},
            new[]
            {
                new int3(9, 4, 5), new int3(2, 11, 3), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(2, 5, 10), new int3(3, 5, 2), new int3(3, 4, 5), new int3(3, 8, 4), new int3(-1, -1, -1)},
            new[]
            {
                new int3(5, 10, 2), new int3(5, 2, 4), new int3(4, 2, 0), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(3, 10, 2), new int3(3, 5, 10), new int3(3, 8, 5), new int3(4, 5, 8), new int3(0, 1, 9)},
            new[] {new int3(5, 10, 2), new int3(5, 2, 4), new int3(1, 9, 2), new int3(9, 4, 2), new int3(-1, -1, -1)},
            new[] {new int3(8, 4, 5), new int3(8, 5, 3), new int3(3, 5, 1), new int3(-1, -1, -1), new int3(-1, -1, -1)},
            new[]
            {
                new int3(0, 4, 5), new int3(1, 0, 5), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(8, 4, 5), new int3(8, 5, 3), new int3(9, 0, 5), new int3(0, 3, 5), new int3(-1, -1, -1)},
            new[]
            {
                new int3(9, 4, 5), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(4, 11, 7), new int3(4, 9, 11), new int3(9, 10, 11), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(0, 8, 3), new int3(4, 9, 7), new int3(9, 11, 7), new int3(9, 10, 11), new int3(-1, -1, -1)},
            new[]
            {
                new int3(1, 10, 11), new int3(1, 11, 4), new int3(1, 4, 0), new int3(7, 4, 11), new int3(-1, -1, -1)
            },
            new[] {new int3(3, 1, 4), new int3(3, 4, 8), new int3(1, 10, 4), new int3(7, 4, 11), new int3(10, 11, 4)},
            new[] {new int3(4, 11, 7), new int3(9, 11, 4), new int3(9, 2, 11), new int3(9, 1, 2), new int3(-1, -1, -1)},
            new[] {new int3(9, 7, 4), new int3(9, 11, 7), new int3(9, 1, 11), new int3(2, 11, 1), new int3(0, 8, 3)},
            new[]
            {
                new int3(11, 7, 4), new int3(11, 4, 2), new int3(2, 4, 0), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(11, 7, 4), new int3(11, 4, 2), new int3(8, 3, 4), new int3(3, 2, 4), new int3(-1, -1, -1)},
            new[] {new int3(2, 9, 10), new int3(2, 7, 9), new int3(2, 3, 7), new int3(7, 4, 9), new int3(-1, -1, -1)},
            new[] {new int3(9, 10, 7), new int3(9, 7, 4), new int3(10, 2, 7), new int3(8, 7, 0), new int3(2, 0, 7)},
            new[] {new int3(3, 7, 10), new int3(3, 10, 2), new int3(7, 4, 10), new int3(1, 10, 0), new int3(4, 0, 10)},
            new[]
            {
                new int3(1, 10, 2), new int3(8, 7, 4), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(4, 9, 1), new int3(4, 1, 7), new int3(7, 1, 3), new int3(-1, -1, -1), new int3(-1, -1, -1)},
            new[] {new int3(4, 9, 1), new int3(4, 1, 7), new int3(0, 8, 1), new int3(8, 7, 1), new int3(-1, -1, -1)},
            new[]
            {
                new int3(4, 0, 3), new int3(7, 4, 3), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(4, 8, 7), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(9, 10, 8), new int3(10, 11, 8), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(3, 0, 9), new int3(3, 9, 11), new int3(11, 9, 10), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(0, 1, 10), new int3(0, 10, 8), new int3(8, 10, 11), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(3, 1, 10), new int3(11, 3, 10), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(1, 2, 11), new int3(1, 11, 9), new int3(9, 11, 8), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(3, 0, 9), new int3(3, 9, 11), new int3(1, 2, 9), new int3(2, 11, 9), new int3(-1, -1, -1)},
            new[]
            {
                new int3(0, 2, 11), new int3(8, 0, 11), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(3, 2, 11), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(2, 3, 8), new int3(2, 8, 10), new int3(10, 8, 9), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(9, 10, 2), new int3(0, 9, 2), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[] {new int3(2, 3, 8), new int3(2, 8, 10), new int3(0, 1, 8), new int3(1, 10, 8), new int3(-1, -1, -1)},
            new[]
            {
                new int3(1, 10, 2), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(1, 3, 8), new int3(9, 1, 8), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(0, 9, 1), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(0, 3, 8), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            },
            new[]
            {
                new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1), new int3(-1, -1, -1),
                new int3(-1, -1, -1)
            }
        };
    }
}