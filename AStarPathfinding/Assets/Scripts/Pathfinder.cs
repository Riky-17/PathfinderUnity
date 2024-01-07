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

    public List<GridNode> FindPath(/*Vector3 startingPos, Vector3 targetPos*/)
    {
        if(grid.CheckWorldPosInGrid(seeker.position, out GridNode startingNode) && grid.CheckWorldPosInGrid(target.position, out GridNode targetNode))
        {
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
                        neighbour.hCost = CalculateDistance(neighbour, targetNode);
                        neighbour.parentNode = currentNode;

                        if (!nodesToCheck.Contains(neighbour))
                        {
                            nodesToCheck.Add(neighbour);
                        }
                    }
                }
            }

            if (checkedNodes.Contains(targetNode))
            {
                List<GridNode> path = new List<GridNode>() {targetNode};
                GridNode thisNode = targetNode;
                while (thisNode != startingNode)
                {
                    thisNode = thisNode.parentNode;
                    path.Add(thisNode);
                }
                return path;
            }
            else
            {
                return null;
            }
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
