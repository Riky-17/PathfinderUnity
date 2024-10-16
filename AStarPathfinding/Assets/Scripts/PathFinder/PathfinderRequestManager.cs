using System;
using System.Collections.Generic;
using UnityEngine;

public class PathfinderRequestManager : MonoBehaviour
{
    public static PathfinderRequestManager Instance {get; private set;}

    Queue<PathfinderRequest> pathfinderRequests = new();
    bool isProcessingRequest = false;
    PathfinderRequest currentRequest;

    void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RequestPath(PathfinderRequest request)
    {
        pathfinderRequests.Enqueue(request);
        TryProcessNextRequest();
    }

    void TryProcessNextRequest()
    {
        if (!isProcessingRequest && pathfinderRequests.Count > 0)
        {
            isProcessingRequest = true;
            currentRequest = pathfinderRequests.Dequeue();
            Pathfinder.Instance.FindPath(currentRequest.startPos, currentRequest.targetPos);
        }
    }

    public void FinishedProcessingPath(List<Vector3> pathPositions, bool success)
    {
        currentRequest.callback(pathPositions, success);
        isProcessingRequest = false;
        TryProcessNextRequest();
    }
}
