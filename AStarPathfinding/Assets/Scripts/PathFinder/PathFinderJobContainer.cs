using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PathFinderJobContainer
{
    PathFinderJob job;
    JobHandle jobHandle;
    PathfinderRequest request;
    NativeList<PathNode> gridNodes;
    NativeList<float3> jobResult;
    NativeHashMap<(int, int), int> nodesIndexes;
    public List<Vector3> path;

    public PathFinderJobContainer() {}

    public PathFinderJobContainer(PathfinderRequest request)
    {
        SetUpRequest(request);
    }

    public void SetUpRequest(PathfinderRequest request)
    {
        this.request = request;
        jobResult = new(Allocator.Persistent);
        gridNodes = new(Allocator.Persistent);
        nodesIndexes = new(request.gridNodes.Count, Allocator.Persistent);
        foreach (PathNode node in request.gridNodes)
        {
            gridNodes.Add(node);
            nodesIndexes.Add((node.x, node.z), node.index);
        }

        job = new()
        {
            nodesDiameterAmount = Mathf.RoundToInt(request.gridRadius * 2 / (request.nodeRadius * 2)),
            gridRadius = request.gridRadius,
            centerPos = request.gridCenter,

            startingPos = request.startPos,
            targetPos = request.targetPos,

            gridNodes = gridNodes,
            nodesIndexes = nodesIndexes,
            path = jobResult,
        };
    }

    public void CompleteJob()
    {
        jobHandle.Complete();
        path = new();

        foreach (float3 pos in jobResult)
            path.Add(pos);
            
        jobResult.Dispose();
        gridNodes.Dispose();
        nodesIndexes.Dispose();

        request.callback(path, true);
    }

    public bool IsComplete() => jobHandle.IsCompleted;

    public void ScheduleJob() => jobHandle = job.Schedule();

    public void Disable()
    {
        jobHandle.Complete();
        jobResult.Dispose();
        gridNodes.Dispose();
        nodesIndexes.Dispose();
    }
}
