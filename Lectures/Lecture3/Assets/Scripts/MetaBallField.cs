using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class MetaBallField
{
    public Transform[] Balls = new Transform[0];
    public float BallRadius = 1;

    private Vector3[] _ballPositions;
    
    /// <summary>
    /// Call Field.Update to react to ball position and parameters in run-time.
    /// </summary>
    public void Update()
    {
        _ballPositions = Balls.Select(x => x.position).ToArray();
    }
    
    /// <summary>
    /// Calculate scalar field value at point
    /// </summary>
    public float F(Vector3 position)
    {
        float f = 0;
        // Naive implementation, just runs for all balls regardless the distance.
        // A better option would be to construct a sparse grid specifically around 
        foreach (var center in _ballPositions)
        {
            f += 1 / Vector3.SqrMagnitude(center - position);
        }

        f *= BallRadius * BallRadius;

        return f - 1;
    }

    public Bounds GetBounds()
    {
        Vector3 minBounds = Vector3.positiveInfinity;
        Vector3 maxBounds = Vector3.negativeInfinity;
        foreach (var center in _ballPositions)
        {
            minBounds = Vector3.Min(minBounds, center - Vector3.one * BallRadius);
            maxBounds = Vector3.Max(maxBounds, center + Vector3.one * BallRadius);
        }
        return new Bounds((maxBounds + minBounds) / 2, maxBounds - minBounds);
    }
}