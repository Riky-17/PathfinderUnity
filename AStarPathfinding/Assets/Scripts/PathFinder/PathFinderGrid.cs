using System.Collections.Generic;
using UnityEngine;

public static class PathFinderGrid
{
    public static List<PathNode> CreateGrid(Vector3 gridCenter, float nodeRadius, float gridRadius, LayerMask obstacleLayer)
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
                
                bool isNodeWalkable = !(Physics.OverlapBox(nodePos, new(nodeRadius, .5f, nodeRadius), Quaternion.identity, obstacleLayer).Length > 0);
                PathNode node = new(nodePos, isNodeWalkable, x, z, nodeIndex);
                gridNodes.Add(node);
                nodeIndex++;

            }
        }

        return gridNodes;
    }
}
