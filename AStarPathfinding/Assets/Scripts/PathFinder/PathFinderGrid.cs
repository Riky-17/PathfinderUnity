using UnityEngine;

public static class PathFinderGrid
{
    public static PathNode[] CreateGrid(float nodeDiameter, float gridSizeX, float gridSizeZ, LayerMask obstacleLayer)
    {
        float nodeRadius = nodeDiameter / 2;
        int NodesAmountX = Mathf.RoundToInt(gridSizeX / nodeDiameter);
        int NodesAmountZ = Mathf.RoundToInt(gridSizeZ / nodeDiameter);
        int TotalNodesAmount = NodesAmountX * NodesAmountZ;

        PathNode[] gridNodes = new PathNode[TotalNodesAmount];

        for (int x = 0; x < NodesAmountX; x++)
        {
            for (int z = 0; z < NodesAmountZ; z++)
            {
                float xCoordinate = nodeDiameter * x + nodeRadius - gridSizeX / 2;
                float zCoordinate = nodeDiameter * z + nodeRadius - gridSizeZ / 2;
                Vector3 nodePos = new(xCoordinate, 0, zCoordinate);
                bool isNodeWalkable = !(Physics.OverlapBox(nodePos, new Vector3(nodeRadius, .5f, nodeRadius), Quaternion.identity, obstacleLayer).Length > 0);
                
                PathNode node = new(nodePos, isNodeWalkable, x, z, NodesAmountX);
                gridNodes[node.index] = node;
            }
        }
        return gridNodes;
    }
}
