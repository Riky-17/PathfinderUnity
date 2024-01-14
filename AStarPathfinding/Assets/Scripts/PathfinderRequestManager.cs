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
        Instance = this;
    }

    public static void RequestPath(PathfinderRequest request)
    {
        Instance.pathfinderRequests.Enqueue(request);
        Instance.TryProcessNextRequest();
    }

    void TryProcessNextRequest()
    {
        if (!isProcessingRequest && pathfinderRequests.Count > 0)
        {
            isProcessingRequest = true;
            currentRequest = pathfinderRequests.Dequeue();
            Pathfinder.Instance.StartFindPath(currentRequest.startPos, currentRequest.targetPos);
        }
    }

    public void FinishedProcessingPath(List<Vector3> pathPositions, bool success)
    {
        currentRequest.callback(pathPositions, success);
        isProcessingRequest = false;
        TryProcessNextRequest();
    }
}
