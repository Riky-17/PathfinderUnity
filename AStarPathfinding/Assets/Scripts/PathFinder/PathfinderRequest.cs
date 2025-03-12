using System;
using System.Collections.Generic;
using UnityEngine;

public struct PathfinderRequest
{
    public Vector3 startPos;
    public Vector3 targetPos;
    public float nodeDiameter;
    public PathNode[] gridNodes;
    public Action<List<Vector3>, bool> callback;

    public PathfinderRequest(Vector3 startPos, Vector3 targetPos, float nodeDiameter, PathNode[] gridNodes, Action<List<Vector3>, bool> callback)
    {
        this.startPos = startPos;
        this.targetPos = targetPos;
        this.nodeDiameter = nodeDiameter;
        this.gridNodes = gridNodes;
        this.callback = callback;
    }
}
