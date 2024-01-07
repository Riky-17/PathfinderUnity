using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PathfinderRequest
{
    public Vector3 startPos;
    public Vector3 targetPos;
    public Action<List<Vector3>, bool> callback;

    public PathfinderRequest(Vector3 startPos, Vector3 targetPos, Action<List<Vector3>, bool> callback)
    {
        this.startPos = startPos;
        this.targetPos = targetPos;
        this.callback = callback;
    }
}
