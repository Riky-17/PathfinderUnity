using UnityEngine;

public class GridNode
{
    public Vector3 nodePos;
    public int x;
    public int z;
    public bool IsWalkable;

    public int gCost;
    public int hCost;
    public int FCost => gCost + hCost;

    public GridNode parentNode;

    public GridNode(Vector3 nodePos, bool IsWalkable, int x, int z)
    {
        this.nodePos = nodePos;
        this.IsWalkable = IsWalkable;
        this.x = x;
        this.z = z;
    }
}
