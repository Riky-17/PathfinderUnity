using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridNode
{
    public Vector3 nodePos;
    public bool IsWalkable;

    public GridNode(Vector3 nodePos, bool IsWalkable)
    {
        this.nodePos = nodePos;
        this.IsWalkable = IsWalkable;
    }
}
