using System.Collections.Generic;
using UnityEngine;

public static class PathFinderGrid
{
    public static List<PathNode> CreateWorldGrid(float nodeRadius, LayerMask obstacleLayer)
    {
        float nodeDiameter = nodeRadius * 2;
        Vector2 gridSize = new(100, 100);
        Vector2 halfGridSIze = gridSize / 2f;

        List<PathNode> gridNodes = new();

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int z = 0; z < gridSize.y; z++)
            {
                float xCoord = nodeDiameter * x + nodeRadius - halfGridSIze.x;
                float zCoord = nodeDiameter * z + nodeRadius - halfGridSIze.y;

                Vector3 nodePos = new(xCoord, 0, zCoord);

                bool isNodeWalkable = Physics.OverlapBox(nodePos, new(nodeRadius, .5f, nodeRadius), Quaternion.identity, obstacleLayer).Length == 0;
                PathNode node = new(nodePos, isNodeWalkable, x, z, x * (int)gridSize.y + z);
                
                gridNodes.Add(node);
            }
        }

        return gridNodes;
    }

    public static List<PathNode> CreateCircularGrid(Vector3 gridCenter, float nodeRadius, float gridRadius, LayerMask obstacleLayer)
    {
        float nodeDiameter = nodeRadius * 2;
        float gridDiameter = gridRadius * 2;
        int gridSize = Mathf.CeilToInt(gridDiameter);
        float halfGridSize = gridSize / 2f;

        List<PathNode> gridNodes = new();
        int nodeIndex = 0;

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                float xCoord = nodeDiameter * x + nodeRadius - halfGridSize + gridCenter.x;
                float ZCoord = nodeDiameter * z + nodeRadius - halfGridSize + gridCenter.z;
                Vector3 nodePos = new(xCoord, gridCenter.y, ZCoord);

                float nodeDist = (nodePos - gridCenter).magnitude;
                if(nodeDist > gridRadius)
                    continue;
                
                bool isNodeWalkable = Physics.OverlapBox(nodePos, new(nodeRadius, .5f, nodeRadius), Quaternion.identity, obstacleLayer).Length == 0;
                PathNode node = new(nodePos, isNodeWalkable, x, z, nodeIndex);
                gridNodes.Add(node);
                nodeIndex++;

            }
        }

        return gridNodes;
    }
}
