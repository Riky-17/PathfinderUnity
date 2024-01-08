using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    public static Pathfinder Instance {get; private set;}
    GridPathfinder grid;
    PathfinderRequestManager requestManager;

    List<GridNode> neighbours = new();
    List<GridNode> nodesToCheck = new();
    HashSet<GridNode> checkedNodes = new();
    

    void Awake()
    {
        Instance = this;
        grid = GetComponent<GridPathfinder>();
        requestManager = GetComponent<PathfinderRequestManager>();
    }

    public void StartFindPath(Vector3 startingPos, Vector3 targetPos)
    {
        FindPath(startingPos, targetPos);
        //StartCoroutine(FindPath(startingPos, targetPos));
    }

    void FindPath(Vector3 startingPos, Vector3 targetPos)
    {
        
        bool isPathComplete = false;

        if(grid.CheckWorldPosInGrid(startingPos, out GridNode startingNode) && grid.CheckWorldPosInGrid(targetPos, out GridNode targetNode) && targetNode.IsWalkable)
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
                    if(node.fCost < currentNode.fCost)
                    {
                        currentNode = node;
                    }
                    else if (node.fCost == currentNode.fCost && node.hCost < currentNode.hCost)
                    {
                        currentNode = node;
                    }
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

                neighbours = grid.GetNeighnourNodes(currentNode);

                foreach (GridNode neighbour in neighbours)
                {
                    if (!neighbour.IsWalkable || checkedNodes.Contains(neighbour))
                    {
                        continue;
                    }

                    int distanceStartToNeighbour = currentNode.gCost + CalculateDistance(currentNode, neighbour);

                    if (neighbour.gCost > distanceStartToNeighbour || !nodesToCheck.Contains(neighbour))
                    {
                        neighbour.gCost = distanceStartToNeighbour;
                        neighbour.hCost = CalculateDistance(neighbour, targetNode);
                        neighbour.parentNode = currentNode;

                        if (!nodesToCheck.Contains(neighbour))
                        {
                            nodesToCheck.Add(neighbour);
                        }
                    }
                }
            }
            //yield return null;
            requestManager.FinishedProcessingPath(path, isPathComplete);
        }
        else
        {
            //yield return null;
            requestManager.FinishedProcessingPath(null, isPathComplete);
        }
    }

    List<Vector3> SimplifyPath(List<Vector3> path)
    {
        if (path.Count < 2)
        {
            return path;
        }

        List<Vector3> simplifiedPath = new() {path[0]};
        Vector3 prevRelVector = path[1] - path[0];

        for (int i = 1; i < path.Count; i++)
        {
            if (i != path.Count - 1)
            {
                Vector3 relVector = path[i + 1] - path[i];
                
                if (relVector.normalized != prevRelVector.normalized)
                {
                    prevRelVector = relVector;
                    simplifiedPath.Add(path[i]);
                } 
            }
            else
            {
                simplifiedPath.Add(path[i]);
            }
        }
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
        {
            return null;
        }
    }

    int CalculateDistance(GridNode nodeA, GridNode nodeB)
    {
        int distanceX = Mathf.Abs(nodeA.x - nodeB.x);
        int distanceZ = Mathf.Abs(nodeA.z - nodeB.z);
        int distance;

        if (distanceZ < distanceX)
        {
            distance = 14 * distanceZ + 10 * (distanceX - distanceZ);
            return distance;
        }
        
        distance = 14 * distanceX + 10 * (distanceZ - distanceX);
        return distance;
    }
}
