using Unity.Mathematics;
using UnityEngine;

namespace UI.Projections
{
    [ExecuteInEditMode]
    public class Line2D: MonoBehaviour
    {
        public Point2D a, b;
        
        private void Update()
        {
            if (b == null)
            {
                return;
            }

            Vector3 aPos = a == null ? Vector3.zero : a.transform.position;
            Vector3 delta = b.transform.position - aPos;
            if (a == null)
            {
                // make an infinite-long ray
                delta = delta.normalized * 1000;
            }
            
            transform.position = aPos + delta / 2;
            transform.rotation = Quaternion.LookRotation(delta, Vector3.back);
            Vector3 scale = transform.localScale;
            scale.z = delta.magnitude;
            transform.localScale = scale;
            
        }
    }
}