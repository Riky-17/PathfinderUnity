using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seeker : MonoBehaviour
{
    [SerializeField] Transform hider;
    List<Vector3> path;
    bool canFollowPath = false;

    float speed = 10f;

    void Update()
    {
        PathfinderRequestManager.RequestPath(transform.position, hider.position, OnPathFound);

        if (Input.GetKeyDown(KeyCode.Space) && path != null)
        {
            canFollowPath = !canFollowPath;
        }

        if (canFollowPath)
        {
            FollowPath();
        }
    }

    private void FollowPath()
    {   
        if (path.Count < 2)
        {
            // has reached the destination
            canFollowPath = false;
            return;
        }

        Vector3 dir = (path[1] - path[0]).normalized;

        transform.position += Time.deltaTime * speed * dir;
    }

    void OnPathFound(List<Vector3> path, bool isPathComplete)
    {
        if (isPathComplete)
        {
            this.path = path;
        }
        else
        {
            this.path = null;
        }
    }

    // void OnDrawGizmos()
    // {
    //     if (path != null)
    //     {
    //         for (int i = 0; i < path.Count; i++)
    //         {
    //             Gizmos.DrawCube(path[i], Vector3.one);
    //             if (i != path.Count - 1)
    //             {
    //                 Gizmos.DrawLine(path[i], path[i + 1]);
    //             }
    //         }
    //     }
    // }
}
