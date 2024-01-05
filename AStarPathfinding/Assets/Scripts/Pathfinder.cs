using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    [SerializeField] Transform seeker;
    [SerializeField] Transform target;

    GridPathfinder grid;

    List<GridNode> neighbours = new List<GridNode>();
    List<GridNode> nodesToCheck = new List<GridNode>();
    HashSet<GridNode> checkedNodes = new HashSet<GridNode>();

    void Awake()
    {
        grid = GetComponent<GridPathfinder>();
    }

    void FindPath(Vector3 startingPos, Vector3 targetPos)
    {
        if(grid.CheckWorldPosInGrid(startingPos, out GridNode startingNode) && grid.CheckWorldPosInGrid(targetPos, out GridNode targetNode))
        {
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
                }

                nodesToCheck.Remove(currentNode);
                checkedNodes.Add(currentNode);

                if(currentNode == targetNode)
                    break;

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
                        neighbour.parentNode = currentNode;

                        if (!nodesToCheck.Contains(neighbour))
                        {
                            neighbour.hCost = CalculateDistance(neighbour, targetNode);
                            nodesToCheck.Add(neighbour);
                        }
                    }
                }
            }  
        }
    }

    int CalculateDistance(GridNode nodeA, GridNode nodeB)
    {
        int distanceX = Mathf.Abs(nodeA.x - nodeB.x);
        int distanceZ = Mathf.Abs(nodeA.z - nodeB.z);

        if (distanceX < distanceZ)
        {
            return 14 * distanceX + 10 * distanceZ - distanceX;
        }
        else
        {
            return 14 * distanceZ + 10 * distanceX - distanceZ;
        }
    }
}
