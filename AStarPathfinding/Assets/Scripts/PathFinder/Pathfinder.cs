using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    public static Pathfinder Instance {get; private set;}

    List<GridNode> neighbours = new();
    List<GridNode> nodesToCheck = new();
    HashSet<GridNode> checkedNodes = new();

    //grid fields
    [SerializeField] float gridSizeX = 1;
    float HalfGridSizeX => gridSizeX / 2;

    [SerializeField] float gridSizeZ = 1;
    float HalfGridSizeZ => gridSizeZ / 2;

    int NodesAmountX => Mathf.RoundToInt(gridSizeX / NodeDiameter);
    int NodesAmountZ => Mathf.RoundToInt(gridSizeZ / NodeDiameter);

    GridNode[,] gridNodes;
    float nodeRadius = .5f;
    float NodeDiameter => nodeRadius * 2;

    [SerializeField] LayerMask obstacleLayer;

    void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        CreateGrid();
    }

    void CreateGrid()
    {
        gridNodes = new GridNode[NodesAmountX, NodesAmountZ];

        for (int x = 0; x < NodesAmountX; x++)
        {
            for (int z = 0; z < NodesAmountZ; z++)
            {
                float xCoordinate = NodeDiameter * x + nodeRadius - gridSizeX / 2;
                float zCoordinate = NodeDiameter * z + nodeRadius - gridSizeZ / 2;
                Vector3 nodePos = new(xCoordinate, 0, zCoordinate);
                bool isNodeWalkable = !(Physics.OverlapBox(nodePos, new(nodeRadius, .5f, nodeRadius), Quaternion.identity, obstacleLayer).Length > 0);
                
                GridNode node = new(nodePos, isNodeWalkable, x, z);
                gridNodes[x, z] = node;
            }
        }
    }

    List<GridNode> GetNeighbourNodes(GridNode node)
    {
        List<GridNode> neighbours = new();

        for (int x = -1; x < 2; x++)
        {
            for (int z = -1; z < 2; z++)
            {
                int neighbourX = Mathf.Clamp(node.x + x, 0, NodesAmountX - 1);                
                int neighbourZ = Mathf.Clamp(node.z + z, 0, NodesAmountZ - 1);
                
                if(gridNodes[neighbourX, neighbourZ] != node)
                    neighbours.Add(gridNodes[neighbourX, neighbourZ]);
            }
        }
        return neighbours;
    }

    bool CheckWorldPosInGrid(Vector3 worldPos, out GridNode node)
    {
        if (worldPos.x < -HalfGridSizeX || worldPos.x > HalfGridSizeX || worldPos.z < -HalfGridSizeZ || worldPos.z > HalfGridSizeZ )
        {
            // world position is outside of the grid
            node = null;
            return false;
        }

        float tValueX = Mathf.InverseLerp(-HalfGridSizeX, HalfGridSizeX, worldPos.x);
        float tValueZ = Mathf.InverseLerp(-HalfGridSizeZ, HalfGridSizeZ, worldPos.z);

        int x = Mathf.RoundToInt(tValueX * (NodesAmountX - 1)); 
        int z = Mathf.RoundToInt(tValueZ * (NodesAmountZ - 1));

        node = gridNodes[x, z];
        return true; 
    }

    public void FindPath(Vector3 startingPos, Vector3 targetPos)
    {
        bool isPathComplete = false;

        if(CheckWorldPosInGrid(startingPos, out GridNode startingNode) && startingNode.IsWalkable && CheckWorldPosInGrid(targetPos, out GridNode targetNode) && targetNode.IsWalkable)
        {
            List<Vector3> path = new();
            nodesToCheck.Clear();
            checkedNodes.Clear();

            nodesToCheck.Add(startingNode);
            GridNode currentNode;

            while(nodesToCheck.Count > 0)
            {
                currentNode = nodesToCheck[0];

                foreach (GridNode node in nodesToCheck)
                {
                    if(node.FCost < currentNode.FCost)
                        currentNode = node;
                    else if (node.FCost == currentNode.FCost && node.hCost < currentNode.hCost)
                        currentNode = node;
                }

                nodesToCheck.Remove(currentNode);
                checkedNodes.Add(currentNode);

                if(currentNode == targetNode)
                {
                    isPathComplete = true;
                    path = RetracePath(startingNode, targetNode);
                    path = SimplifyPath(path);
                    break;
                }

                neighbours = GetNeighbourNodes(currentNode);

                foreach (GridNode neighbour in neighbours)
                {
                    if (!neighbour.IsWalkable || checkedNodes.Contains(neighbour))
                        continue;

                    //this is to avoid the seeker cutting corner when next to a wall causing the seeker to momentarily going inside a wall
                    if (CalculateDistance(currentNode, neighbour) == 14 && IsNodePastCorner(neighbours, currentNode, neighbour))
                        continue;

                    int distanceStartToNeighbour = currentNode.gCost + CalculateDistance(currentNode, neighbour);

                    if (neighbour.gCost > distanceStartToNeighbour || !nodesToCheck.Contains(neighbour))
                    {
                        neighbour.gCost = distanceStartToNeighbour;
                        neighbour.hCost = CalculateDistance(neighbour, targetNode);
                        neighbour.parentNode = currentNode;

                        if (!nodesToCheck.Contains(neighbour))
                            nodesToCheck.Add(neighbour);
                    }
                }
            }
            PathfinderRequestManager.Instance.FinishedProcessingPath(path, isPathComplete);
        }
        else
            PathfinderRequestManager.Instance.FinishedProcessingPath(null, isPathComplete);
    }

    List<Vector3> SimplifyPath(List<Vector3> path)
    {
        if (path.Count < 2)
            return path;

        List<Vector3> simplifiedPath = new() {};
        Vector3 prevRelVector = path[1] - path[0];

        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector3 relVector = path[i + 1] - path[i];
                
            if (relVector.normalized != prevRelVector.normalized)
                simplifiedPath.Add(path[i]);

            prevRelVector = relVector;
        }
        simplifiedPath.Add(path[^1]);
        return simplifiedPath;
    }

    List<Vector3> RetracePath(GridNode startingNode, GridNode targetNode)
    {
        if (checkedNodes.Contains(targetNode))
        {
            List<Vector3> path = new() {targetNode.nodePos};
            GridNode currentNode = targetNode;
            while (currentNode != startingNode)
            {
                currentNode = currentNode.parentNode;
                path.Add(currentNode.nodePos);
            }
            path.Reverse();
            return path;
        }
        else
            return null;
    }

    int CalculateDistance(GridNode nodeA, GridNode nodeB)
    {
        int distanceX = Mathf.Abs(nodeA.x - nodeB.x);
        int distanceZ = Mathf.Abs(nodeA.z - nodeB.z);

        if (distanceZ < distanceX)
            return 14 * distanceZ + 10 * (distanceX - distanceZ);
        
        return 14 * distanceX + 10 * (distanceZ - distanceX);
    }

    bool IsNodePastCorner(List<GridNode> neighbours, GridNode currentNode, GridNode currentNeighbour)
    {
        foreach (GridNode neighbour in neighbours)
        {
            if(!neighbour.IsWalkable && CalculateDistance(currentNode, neighbour) == 10)
            {
                Vector2 currentToWall = new(neighbour.nodePos.x - currentNode.nodePos.x, neighbour.nodePos.z - currentNode.nodePos.z);
                Vector2 currentToNeighbour = new(currentNeighbour.nodePos.x - currentNode.nodePos.x, currentNeighbour.nodePos.z - currentNode.nodePos.z);
                if(Vector2.Dot(currentToWall, currentToNeighbour) > 0)
                    return true;
            }
        }
        return false;
    }
    
    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(Vector3.zero, new(gridSizeX, 1, gridSizeZ));

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
