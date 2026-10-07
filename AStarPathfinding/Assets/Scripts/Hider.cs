using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hider : MonoBehaviour
{
    Vector3 moveDir;
    readonly float speed = 6;
    [SerializeField] LayerMask obstacleMask;

    void Update()
    {
        moveDir = Vector3.zero;

        if(Input.GetKey(KeyCode.W))
            moveDir += Vector3.forward;
        if(Input.GetKey(KeyCode.S))
            moveDir += Vector3.back;
        if(Input.GetKey(KeyCode.D))
            moveDir += Vector3.right;
        if(Input.GetKey(KeyCode.A))
            moveDir += Vector3.left;

        if(moveDir == Vector3.zero)
            return;

        moveDir = moveDir.normalized;

        bool canWalk = !DetectObstacles(moveDir);
        if(!canWalk)
        {
            Vector3 newDir = new(moveDir.x, 0, 0);

            canWalk = moveDir.x != 0 && !DetectObstacles(newDir); 

            if(canWalk)
                moveDir = newDir;
            else
            {
                newDir = new(0, 0, moveDir.z);

                canWalk = moveDir.z != 0 && !DetectObstacles(newDir);

                if(canWalk)
                    moveDir = newDir;
                else
                    return;
            }
        }
        
        transform.position += speed * Time.deltaTime * moveDir;
    }

    bool DetectObstacles(Vector3 dir) => Physics.Raycast(transform.position, dir, 1, obstacleMask);
}
