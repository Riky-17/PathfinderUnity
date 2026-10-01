using System;
using System.Collections.Generic;
using UnityEngine;

public struct PathfinderRequest
{
    public Vector3 startPos;
    public Vector3 targetPos;
    public Vector2 gridHalfSize;
    public float nodeRadius;
    public List<PathNode> grid;
    public Vector3 gridCenter;
    public Action<List<Vector3>> callback;

    public PathfinderRequest(Vector3 startPos, Vector3 targetPos, Vector3 gridCenter, Vector2 gridHalfSize, float nodeRadius, List<PathNode> grid, Action<List<Vector3>> callback)
    {
        this.startPos = startPos;
        this.targetPos = targetPos;
        this.gridHalfSize = gridHalfSize;
        this.nodeRadius = nodeRadius;
        this.grid = grid;
        this.gridCenter = gridCenter;
        this.callback = callback;
    }
}
