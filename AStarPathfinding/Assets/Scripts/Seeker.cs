using System.Collections.Generic;
using UnityEngine;

public class Seeker : MonoBehaviour
{
    [SerializeField] Transform hider;
    List<Vector3> path;
    bool canFollowPath = false;
    int index = 0;
    float speed = 10f;

    void Start()
    {
        PathfinderRequestManager.Instance.RequestPath(new(transform.position, hider.position, OnPathFound));
    }

    void Update()
    {
        // PathfinderRequestManager.Instance.RequestPath(new(transform.position, hider.position, OnPathFound));
        
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
        // callback error first pattern
        if (isPathComplete)
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
