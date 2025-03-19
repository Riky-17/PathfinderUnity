using System.Collections.Generic;
using UnityEngine;

public class Seeker : MonoBehaviour
{
    [SerializeField] Transform hider;
    [SerializeField] LayerMask obstacleLayer;
    List<Vector3> path;
    bool canFollowPath = false;
    int index = 0;
    float speed = 10f;
    bool pathRequest = false;
    
    float gridRadius = 25;
    float nodeRadius = .5f;
    List<PathNode> gridNodes;
    Vector3 gridCenter;

    void Awake()
    {
        gridCenter = transform.position;
        gridNodes = PathFinderGrid.CreateGrid(gridCenter, nodeRadius, gridRadius, obstacleLayer);
    }

    void Update()
    {
        if((hider.transform.position - transform.position).magnitude > gridRadius)
        {
            pathRequest = false;
            path = null;
            index = 0;
        }
        else if(!pathRequest)
        {
            Pathfinder.Instance.FindPath(new(transform.position, hider.transform.position, gridRadius, nodeRadius, gridNodes, gridCenter, OnPathFound));
            pathRequest = true;
        }

        if (Input.GetKeyDown(KeyCode.Space) && path != null)
            canFollowPath = !canFollowPath;

        if (canFollowPath && path != null)
            FollowPath();

    }

    private void FollowPath()
    {  
        Vector3 relVector = path[index] - transform.position;

        if (relVector.magnitude < .1f)
            index++;

        if (index >= path.Count || path == null || path.Count < 1)
        {
            // has reached the destination
            canFollowPath = false;
            index = 0;
            return;
        }

        Vector3 dir = (path[index] - transform.position).normalized;

        transform.position += Time.deltaTime * speed * dir;
    }

    void OnPathFound(List<Vector3> path, bool isPathComplete)
    {
        pathRequest = false;
        
        if (path.Count > 0)
        {
            this.path = path;
            index = 0;
        }
        else
        {
            this.path = null;
            index = 0;
        }
    }

    void OnDrawGizmos()
    {
        if(gridNodes == null)
            return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(gridCenter, gridRadius);
        Gizmos.color = Color.white;
        // foreach(PathNode node in gridNodes)
        // {
        //     Gizmos.color = node.IsWalkable ? Color.green : Color.red;
        //     float size = nodeRadius * 2;
        //     Gizmos.DrawCube(node.nodePos, new(size, size, size));
        // }
        // if(gridNodes != null)
        // {
        //     foreach (PathNode node in gridNodes)
        //     {
        //         Gizmos.DrawCube(node.nodePos, new(nodeDiameter, nodeDiameter, nodeDiameter));
        //     }
        // }

        if (path != null)
        {
            for (int i = 0; i < path.Count; i++)
            {
                if (i == index)
                    Gizmos.DrawLine(transform.position, path[0]);

                Gizmos.DrawCube(path[i], Vector3.one);
                if (i != path.Count - 1)
                {
                    Gizmos.DrawLine(path[i], path[i + 1]);
                }
            }
        }
    }
}
