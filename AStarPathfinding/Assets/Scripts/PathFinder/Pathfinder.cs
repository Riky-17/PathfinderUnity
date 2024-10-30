using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

public class Pathfinder : MonoBehaviour
{
    public static Pathfinder Instance {get; private set;}

    //grid fields
    [SerializeField] float gridSizeX = 1;

    [SerializeField] float gridSizeZ = 1;

    int NodesAmountX => (int)math.round(gridSizeX / NodeDiameter);
    int NodesAmountZ => (int)math.round(gridSizeZ / NodeDiameter);
    int TotalNodesAmount => NodesAmountX * NodesAmountZ;

    NativeArray<PathNode> gridNodes;
    float nodeRadius = .5f;
    float NodeDiameter => nodeRadius * 2;

    [SerializeField] LayerMask obstacleLayer;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    void CreateGrid()
    {
        gridNodes = new(TotalNodesAmount, Allocator.Persistent);

        for (int x = 0; x < NodesAmountX; x++)
        {
            for (int z = 0; z < NodesAmountZ; z++)
            {
                float xCoordinate = NodeDiameter * x + nodeRadius - gridSizeX / 2;
                float zCoordinate = NodeDiameter * z + nodeRadius - gridSizeZ / 2;
                Vector3 nodePos = new(xCoordinate, 0, zCoordinate);
                bool isNodeWalkable = !(Physics.OverlapBox(nodePos, new(nodeRadius, .5f, nodeRadius), Quaternion.identity, obstacleLayer).Length > 0);
                
                PathNode node = new(nodePos, isNodeWalkable, x, z, NodesAmountX);
                gridNodes[node.index] = node;
            }
        }
    }

    public void FindPath(float3 startingPos, float3 targetPos)
    {
        CreateGrid();

        NativeList<float3> path = new(TotalNodesAmount, Allocator.TempJob);
        NativeList<PathNode> nodesToCheck = new(Allocator.TempJob);
        NativeHashSet<PathNode> checkedNodes = new(TotalNodesAmount, Allocator.TempJob);
        NativeList<PathNode> neighbours = new(Allocator.TempJob);
        PathFinderJob job = new()
        {
            gridSizeX = gridSizeX,
            gridSizeZ = gridSizeZ,

            nodeRadius = nodeRadius,

            startingPos = startingPos,
            targetPos = targetPos,

            gridNodes = gridNodes,
            nodesToCheck = nodesToCheck,
            checkedNodes = checkedNodes,
            neighbours = neighbours,

            path = path,
        };
        
        JobHandle jobHandle = job.Schedule();
        jobHandle.Complete();
        // Debug.Log(job.path[0]);
        // Debug.Log(job.path[1]);
        gridNodes.Dispose();
        nodesToCheck.Dispose();
        checkedNodes.Dispose();
        neighbours.Dispose();
        // Debug.Log(path[0]);
        PathfinderRequestManager.Instance.FinishedProcessingPath(path, true);
        path.Dispose();
    }
    
    // void OnDrawGizmos()
    // {
    //     Gizmos.DrawWireCube(Vector3.zero, new(gridSizeX, 1, gridSizeZ));

    //     if (gridNodes != null)
    //     {
    //         Vector3 cornerOffsets = new(.25f, 0, .25f);
    //         Vector3 cubeHeight = new(0, .5f, 0);

    //         foreach (PathNode node in gridNodes)
    //         {
    //             Gizmos.color = node.IsWalkable ? Color.green : Color.red;
    //             Gizmos.DrawCube(node.nodePos, Vector3.one - cornerOffsets + cubeHeight);
    //         }
    //     }
    // }
}

public struct PathFinderJob : IJob
{
    public float gridSizeX;
    readonly float HalfGridSizeX => gridSizeX / 2;

    public float gridSizeZ;
    readonly float HalfGridSizeZ => gridSizeZ / 2;
    
    public float nodeRadius;
    readonly float NodeDiameter => nodeRadius * 2;

    readonly int NodesAmountX => (int)math.round(gridSizeX / NodeDiameter);
    readonly int NodesAmountZ => (int)math.round(gridSizeZ / NodeDiameter);

    public float3 startingPos;
    public float3 targetPos;

    public NativeArray<PathNode> gridNodes;

    public NativeList<PathNode> nodesToCheck;
    public NativeHashSet<PathNode> checkedNodes;
    public NativeList<PathNode> neighbours;

    public NativeList<float3> path;

    public void Execute()
    {
        if(CheckWorldPosInGrid(startingPos, out PathNode startingNode) && startingNode.IsWalkable && CheckWorldPosInGrid(targetPos, out PathNode targetNode) && targetNode.IsWalkable)
        {
            nodesToCheck.Add(startingNode);
            PathNode currentNode;

            while(nodesToCheck.Length > 0)
            {
                currentNode = nodesToCheck[0];
                int currentNodeIndex = 0;

                for (int i = 0; i < nodesToCheck.Length; i++)
                {
                    PathNode node = nodesToCheck[i];
                    if(node.FCost < currentNode.FCost)
                    {
                        currentNode = node;
                        currentNodeIndex = i;
                    }
                    else if (node.FCost == currentNode.FCost && node.hCost < currentNode.hCost)
                    {
                        currentNode = node;
                        currentNodeIndex = i;
                    }
                }

                nodesToCheck.RemoveAt(currentNodeIndex);
                checkedNodes.Add(currentNode);

                if(currentNode == targetNode)
                {
                    targetNode = currentNode;
                    break;
                }

                neighbours = GetNeighbourNodes(currentNode);

                for (int i = 0; i < neighbours.Length; i++)
                {
                    PathNode neighbour = neighbours[i];
                    if (!neighbour.IsWalkable || checkedNodes.Contains(neighbour))
                        continue;

                    //this is to avoid the seeker cutting corner when next to a wall causing the seeker to momentarily going inside a wall
                    if (CalculateDistance(currentNode, neighbour) == 14 && IsNodePastCorner(currentNode, neighbour))
                        continue;

                    int distanceStartToNeighbour = currentNode.gCost + CalculateDistance(currentNode, neighbour);

                    if (neighbour.gCost > distanceStartToNeighbour || !nodesToCheck.Contains(neighbour))
                    {
                        neighbour.gCost = distanceStartToNeighbour;
                        neighbour.hCost = CalculateDistance(neighbour, targetNode);
                        neighbour.parentNode = currentNode.index;
                        gridNodes[neighbour.index] = neighbour;

                        if (!nodesToCheck.Contains(neighbour))
                            nodesToCheck.Add(neighbour);
                    }
                }
            }

            NativeList<float3> tempPath = RetracePath(startingNode, targetNode);
            tempPath = SimplifyPath(tempPath);
            for (int i = 0; i < tempPath.Length; i++)
            {
                path.Add(tempPath[i]);
            }
        }
    }

    bool CheckWorldPosInGrid(Vector3 worldPos, out PathNode node)
    {
        if (worldPos.x < -HalfGridSizeX || worldPos.x > HalfGridSizeX || worldPos.z < -HalfGridSizeZ || worldPos.z > HalfGridSizeZ )
        {
            // world position is outside of the grid
            node = default;
            return false;
        }

        float tValueX = InverseLerp(-HalfGridSizeX, HalfGridSizeX, worldPos.x);
        float tValueZ = InverseLerp(-HalfGridSizeZ, HalfGridSizeZ, worldPos.z);

        int x = (int)math.round(tValueX * (NodesAmountX - 1)); 
        int z = (int)math.round(tValueZ * (NodesAmountZ - 1));

        node = gridNodes[x + z * NodesAmountX];
        return true; 
    }
    
    NativeList<PathNode> GetNeighbourNodes(PathNode node)
    {
        NativeList<PathNode> neighbours = new(Allocator.Temp);
        for (int x = -1; x < 2; x++)
        {
            for (int z = -1; z < 2; z++)
            {
                int neighbourX = math.clamp(node.x + x, 0, NodesAmountX - 1);                
                int neighbourZ = math.clamp(node.z + z, 0, NodesAmountZ - 1);
                int neighbourIndex = neighbourX + neighbourZ * NodesAmountX;
                
                if(gridNodes[neighbourIndex] != node)
                    neighbours.Add(gridNodes[neighbourIndex]);
            }
        }
        return neighbours;
    }

    readonly int CalculateDistance(PathNode nodeA, PathNode nodeB)
    {
        int distanceX = math.abs(nodeA.x - nodeB.x);
        int distanceZ = math.abs(nodeA.z - nodeB.z);

        if (distanceZ < distanceX)
            return 14 * distanceZ + 10 * (distanceX - distanceZ);
        
        return 14 * distanceX + 10 * (distanceZ - distanceX);
    }
    
    readonly bool IsNodePastCorner(PathNode currentNode, PathNode currentNeighbour)
    {
        foreach (PathNode neighbour in neighbours)
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
    
    readonly NativeList<float3> SimplifyPath(NativeList<float3> path)
    {
        if (path.Length < 2)
            return path;
        NativeList<float3> simplifiedPath = new(Allocator.Temp);
        float3 prevRelVector = path[1] - path[0];

        for (int i = 1; i < path.Length - 1; i++)
        {
            float3 relVector = path[i + 1] - path[i];
            bool3 isSameDir = math.normalize(relVector) == math.normalize(prevRelVector);
                
            if (!isSameDir.x || !isSameDir.y || !isSameDir.z)
            {
                simplifiedPath.Add(path[i]);
            }

            prevRelVector = relVector;
        }
        simplifiedPath.Add(path[^1]);
        return simplifiedPath;
    }
    
    NativeList<float3> RetracePath(PathNode startingNode, PathNode targetNode)
    {
        if (checkedNodes.Contains(targetNode))
        {
            NativeList<float3> path = new(Allocator.Temp) {targetNode.nodePos};

            PathNode currentNode = targetNode;
            while (currentNode != startingNode)
            {
                currentNode = gridNodes[currentNode.parentNode];
                path.Add(currentNode.nodePos);
            }

            NativeList<float3> retracedPath = new(Allocator.Temp);
            for (int i = path.Length - 1; i >= 0 ; i--)
                retracedPath.Add(path[i]);
            
            return retracedPath;
        }
        else
            return new(Allocator.Temp);
    }
    
    readonly float InverseLerp(float a, float b, float c) => (c - a) / (b - a);
}