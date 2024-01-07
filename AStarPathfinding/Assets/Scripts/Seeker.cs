using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seeker : MonoBehaviour
{
    [SerializeField] Transform hider;
    List<Vector3> path;

    void Update()
    {
        PathfinderRequestManager.RequestPath(transform.position, hider.position, OnPathFound);
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

    void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = 0; i < path.Count; i++)
            {
                if (i != path.Count - 1)
                {
                    Gizmos.DrawLine(path[i], path[i + 1]);
                }
            }
        }
    }
}
