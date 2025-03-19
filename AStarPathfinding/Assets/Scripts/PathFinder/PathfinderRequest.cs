using System;
using System.Collections.Generic;
using UnityEngine;

public struct PathfinderRequest
{
    public Vector3 startPos;
    public Vector3 targetPos;
    public float gridRadius;
    public float nodeRadius;
    public List<PathNode> gridNodes;
    public Vector3 gridCenter;
    public Action<List<Vector3>, bool> callback;

    public PathfinderRequest(Vector3 startPos, Vector3 targetPos, float gridRadius, float nodeRadius, List<PathNode> gridNodes, Vector3 gridCenter, Action<List<Vector3>, bool> callback)
    {
        this.startPos = startPos;
        this.targetPos = targetPos;
        this.gridRadius = gridRadius;
        this.nodeRadius = nodeRadius;
        this.gridNodes = gridNodes;
        this.gridCenter = gridCenter;
        this.callback = callback;
    }
}
