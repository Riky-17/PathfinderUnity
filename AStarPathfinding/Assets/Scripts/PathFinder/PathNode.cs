using System;
using Unity.Mathematics;

public struct PathNode : IEquatable<PathNode>
{
    public float3 nodePos;
    public int x;
    public int z;
    public bool IsWalkable;
    
    public int index;

    public int gCost;
    public int hCost;
    public readonly int FCost => gCost + hCost;

    public int parentNode;

    public PathNode(float3 nodePos, bool IsWalkable, int x, int z, int nodeIndex)
    {
        this.nodePos = nodePos;
        this.IsWalkable = IsWalkable;
        this.x = x;
        this.z = z;
        index = nodeIndex;
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

    public override readonly int GetHashCode() => HashCode.Combine(x + z);
    public readonly bool Equals(PathNode other) => this == other;

    public static bool operator ==(PathNode left, PathNode right) => left.x == right.x && left.z == right.z; 
    public static bool operator !=(PathNode left, PathNode right) => !(left == right); 
}