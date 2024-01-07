using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPathfinder : MonoBehaviour
{
    Pathfinder pathfinder;
    List<GridNode> path = new List<GridNode>();
    [SerializeField] float gridSizeX = 1;
    float halfGridSizeX => gridSizeX / 2;

    [SerializeField] float gridSizeY = 1;

    [SerializeField] float gridSizeZ = 1;
    float halfGridSizeZ => gridSizeZ / 2;

    Vector3 gridSize => new(gridSizeX, gridSizeY, gridSizeZ);

    int nodesAmountX => Mathf.RoundToInt(gridSizeX / nodeDiameter);
    int nodesAmountZ => Mathf.RoundToInt(gridSizeZ / nodeDiameter);

    GridNode[,] gridNodes;
    float nodeRadius = .5f;
    float nodeDiameter => nodeRadius * 2;

    [SerializeField] LayerMask obstacleLayer;

    void Awake()
    {
        pathfinder = GetComponent<Pathfinder>();
    }

    void Start()
    {
        CreateGrid();
    }

    void Update()
    {
        path = pathfinder.FindPath();
    }

    void CreateGrid()
    {
        gridNodes = new GridNode[nodesAmountX, nodesAmountZ];

        for (int x = 0; x < nodesAmountX; x++)
        {
            for (int z = 0; z < nodesAmountZ; z++)
            {
                float xCoordinate = nodeDiameter * x + nodeRadius - gridSizeX / 2;
                float zCoordinate = nodeDiameter * z + nodeRadius - gridSizeZ / 2;
                Vector3 nodePos = new(xCoordinate, 0, zCoordinate);
                bool isNodeWalkable = !(Physics.OverlapBox(nodePos, new(nodeRadius, gridSizeY / 2, nodeRadius), Quaternion.identity, obstacleLayer).Length > 0);
                
                GridNode node = new GridNode(nodePos, isNodeWalkable, x, z);
                gridNodes[x, z] = node;
            }
        }
    }

    public List<GridNode> GetNeighnourNodes(GridNode node)
    {
        List<GridNode> neigbours = new List<GridNode>();

        for (int x = -1; x < 2; x++)
        {
            for (int z = -1; z < 2; z++)
            {
                int neighbourX = Mathf.Clamp(node.x + x, 0, nodesAmountX - 1);                
                int neighbourZ = Mathf.Clamp(node.z + z, 0, nodesAmountZ - 1);
                
                if(gridNodes[neighbourX, neighbourZ] != node)
                    neigbours.Add(gridNodes[neighbourX, neighbourZ]);
            }
        }
        return neigbours;
    }

    public bool CheckWorldPosInGrid(Vector3 worldPos, out GridNode node)
    {
        if (worldPos.x < -halfGridSizeX || worldPos.x > halfGridSizeX || worldPos.z < -halfGridSizeZ || worldPos.z > halfGridSizeZ )
        {
            // world position is outside of the grid
            node = null;
            return false;
        }

        float tValueX = Mathf.InverseLerp(-halfGridSizeX, halfGridSizeX, worldPos.x);
        float tValueZ = Mathf.InverseLerp(-halfGridSizeZ, halfGridSizeZ, worldPos.z);

        int x = Mathf.RoundToInt(tValueX * (nodesAmountX - 1)); 
        int z = Mathf.RoundToInt(tValueZ * (nodesAmountZ - 1));

        node = gridNodes[x, z];
        return true; 
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
                if (path != null && path.Contains(node))
                {
                    Gizmos .color = Color.black;
                }
                Gizmos.DrawCube(node.nodePos, Vector3.one - cornerOffsets + cubeHeight);
            }
        }
    }
}
