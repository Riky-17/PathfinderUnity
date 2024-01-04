using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPathfinder : MonoBehaviour
{
    [SerializeField] float gridSizeX = 1;
    [SerializeField] float gridSizeY = 1;
    [SerializeField] float gridSizeZ = 1;
    Vector3 gridSize => new(gridSizeX, gridSizeY, gridSizeZ);

    GridNode[,] gridNodes;
    float nodeRadius = .5f;
    float nodeDiameter => nodeRadius * 2;

    [SerializeField] LayerMask obstacleLayer;

    void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        int nodesAmountX = (int)gridSizeX / (int)nodeDiameter;
        int nodesAmountZ = (int)gridSizeZ / (int)nodeDiameter;
        gridNodes = new GridNode[nodesAmountX, nodesAmountZ];

        for (int x = 0; x < nodesAmountX; x++)
        {
            for (int z = 0; z < nodesAmountZ; z++)
            {
                float xCoordinate = nodeDiameter * x + nodeRadius - gridSizeX / 2;
                float zCoordinate = nodeDiameter * z + nodeRadius - gridSizeZ / 2;
                Vector3 nodePos = new(xCoordinate, 0, zCoordinate);
                bool isNodeWalkable = !(Physics.OverlapBox(nodePos, new(nodeRadius, gridSizeY / 2, nodeRadius), Quaternion.identity, obstacleLayer).Length > 0);
                
                GridNode node = new GridNode(nodePos, isNodeWalkable);
                gridNodes[x, z] = node;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(Vector3.zero, gridSize);

        if (gridNodes != null)
        {
            Vector3 cornerOffsets = new(.25f, 0, .25f);
            Vector3 cubeHeight = new(0, .5f, 0);

            foreach (GridNode node in gridNodes)
            {
                Gizmos.color = node.IsWalkable ? Color.green : Color.red;
                Gizmos.DrawCube(node.nodePos, Vector3.one - cornerOffsets + cubeHeight);
            }
        }
    }
}
