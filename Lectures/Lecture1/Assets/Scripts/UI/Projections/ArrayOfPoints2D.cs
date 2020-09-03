using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace UI.Projections
{
    public class ArrayOfPoints2D: MonoBehaviour
    {
        public int2 Count = new int2(2, 1);

        public float2 Min = new float2(-10, 0);
        public float2 Max = new float2(10, 0);
        
        public GameObject PointPrefab;
        private List<GameObject> _points = new List<GameObject>();
        
        private void Awake()
        {
            PopulatePoints();
        }
        
        private void OnValidate()
        {
            if (Count.x < 1)
            {
                Count.x = 1;
            }
            if (Count.y < 1)
            {
                Count.y = 1;
            }

            if (Application.isPlaying && Time.frameCount > 0)
            {
                PopulatePoints();
            }
        }

        private void PopulatePoints()
        {
            float2 delta = new float2(
                Count.x < 2 ? 0 : (Max.x - Min.x) / (Count.x - 1),
                Count.y < 2 ? 0 : (Max.y - Min.y) / (Count.y - 1)
            );

            foreach (var point in _points)
            {
                Destroy(point);
            }
            _points.Clear();

            for (int i = 0; i < Count.x; i++)
            {
                for (int j = 0; j < Count.y; j++)
                {
                    float2 pos = new float2(i, j) * delta + Min;
                    _points.Add(
                        Instantiate(PointPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, transform)
                    );
                }
            }
        }
    }
}