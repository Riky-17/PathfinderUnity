using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public static class PathFinderGrid
{
    public static NativeArray<PathNode> CreateGrid(float gridSizeX, float gridSizeZ, float nodeRadius, LayerMask obstacleLayer)
    {
        float nodeDiameter = nodeRadius * 2;
        int NodesAmountX = Mathf.RoundToInt(gridSizeX / nodeDiameter);
        int NodesAmountZ = Mathf.RoundToInt(gridSizeZ / nodeDiameter);
        int TotalNodesAmount = NodesAmountX * NodesAmountZ;

        NativeArray<PathNode> gridNodes = new(TotalNodesAmount, Allocator.Persistent);

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
