using System;
using UnityEngine;

public struct PathNode : IEquatable<PathNode>
{
    public Vector3 nodePos;
    public int x;
    public int z;
    public bool IsWalkable;
    
    public int index;

    public int gCost;
    public int hCost;
    public int FCost => gCost + hCost;

    public int parentNode;

    public PathNode(Vector3 nodePos, bool IsWalkable, int x, int z, int gridWidth)
    {
        this.nodePos = nodePos;
        this.IsWalkable = IsWalkable;
        this.x = x;
        this.z = z;
        index = x + z * gridWidth;
        gCost = 0;
        hCost = 0;
        parentNode = -1;
    }

    public override readonly bool Equals(object obj)
    {
        if(obj is PathNode other)
            return this == other;
        return false;
    }

    public override readonly int GetHashCode() => HashCode.Combine(index);
    public readonly bool Equals(PathNode other) => this == other;

    public static bool operator ==(PathNode left, PathNode right) => left.index == right.index; 
    public static bool operator !=(PathNode left, PathNode right) => !(left == right); 
}