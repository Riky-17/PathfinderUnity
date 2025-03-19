using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

public class Pathfinder : MonoBehaviour
{
    public static Pathfinder Instance {get; private set;}

    ObjectPool pathfinderJobPool;
    List<PathFinderJobContainer> pathFinderJobs = new();

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        pathfinderJobPool = new();
    }

    void OnDisable()
    {
        foreach (PathFinderJobContainer job in pathFinderJobs)
            job.Disable();
    }

    void Update()
    {
        for (int i = pathFinderJobs.Count - 1; i >= 0 ; i--)
        {
            PathFinderJobContainer job = pathFinderJobs[i];
            if(job.IsComplete())
            {
                pathfinderJobPool.ReturnToPool(job);
                pathFinderJobs.RemoveAt(i);
            }
        }
    }

    public void FindPath(PathfinderRequest request)
    {
        PathFinderJobContainer jobContainer = pathfinderJobPool.RequestJob(request);
        jobContainer.ScheduleJob();
        pathFinderJobs.Add(jobContainer);
    }
}

public struct PathFinderJob : IJob
{
    public int nodesDiameterAmount; // the amount of nodes across the diameter

    public float gridRadius;
    public float3 centerPos;

    public float3 startingPos;
    public float3 targetPos;

    public NativeList<PathNode> gridNodes;
    public NativeHashMap<(int, int), int> nodesIndexes;

    public NativeList<float3> path;

    public void Execute()
    {
        if(CheckWorldPosInGrid(startingPos, out PathNode startingNode) && startingNode.IsWalkable && CheckWorldPosInGrid(targetPos, out PathNode targetNode) && targetNode.IsWalkable)
        {
            NativeList<PathNode> nodesToCheck = new(Allocator.Temp) { startingNode };
            NativeList<PathNode> checkedNodes = new(Allocator.Temp);
            NativeList<PathNode> neighbours;
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

                    int dist = CalculateDistance(currentNode, neighbour);

                    //this is to avoid the seeker cutting corner when next to a wall causing the seeker to momentarily going inside a wall
                    if (dist == 14 && IsNodePastCorner(neighbours, currentNode, neighbour))
                        continue;

                    int distanceStartToNeighbour = currentNode.gCost + dist;

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
            NativeList<float3> tempPath = RetracePath(checkedNodes, startingNode, targetNode);
            tempPath = SimplifyPath(tempPath);
            for (int i = 0; i < tempPath.Length; i++)
                path.Add(tempPath[i]);
        }
        
    }

    bool CheckWorldPosInGrid(float3 worldPos, out PathNode node)
    {
        float3 relV = worldPos - centerPos;
        float dist = relV.x * relV.x + relV.y * relV.y;;

        if(dist > gridRadius * gridRadius)
        {
            node = default;
            return false;
        }

        float tValueX = InverseLerp(-gridRadius, gridRadius, relV.x);
        float tValueZ = InverseLerp(-gridRadius, gridRadius, relV.z);

        int x = (int)math.round(tValueX * (nodesDiameterAmount - 1)); 
        int z = (int)math.round(tValueZ * (nodesDiameterAmount - 1));

        if(!nodesIndexes.ContainsKey((x, z)))
        {
            node = default;
            return false;
        }

        node = gridNodes[nodesIndexes[(x, z)]];
        return true; 
    }
    
    NativeList<PathNode> GetNeighbourNodes(PathNode node)
    {
        NativeList<PathNode> neighbours = new(Allocator.Temp);
        for (int x = -1; x < 2; x++)
        {
            int neighbourX = node.x + x;

            for (int z = -1; z < 2; z++)
            {
                if(x == 0 && z == 0)
                    continue;
                
                int neighbourZ = node.z + z;
                if(nodesIndexes.ContainsKey((neighbourX, neighbourZ)))
                    neighbours.Add(gridNodes[nodesIndexes[(neighbourX, neighbourZ)]]);
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
    
    readonly bool IsNodePastCorner(NativeList<PathNode> neighbours, PathNode currentNode, PathNode currentNeighbour)
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

    
    NativeList<float3> RetracePath(NativeList<PathNode> checkedNodes, PathNode startingNode, PathNode targetNode)
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
