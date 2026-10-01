using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PathFinderJobInfo
{
    PathFinderJob job;
    JobHandle jobHandle;
    PathfinderRequest request;
    NativeList<PathNode> gridNodes;
    NativeList<float3> jobResult;
    NativeHashMap<(int, int), int> nodesIndexes;
    public List<Vector3> path;

    public PathFinderJobInfo() {}

    public PathFinderJobInfo(PathfinderRequest request) => SetUpRequest(request);

    public void SetUpRequest(PathfinderRequest request)
    {
        this.request = request;

        jobResult = new(Allocator.Persistent);
        gridNodes = new(Allocator.Persistent);
        nodesIndexes = new(request.grid.Count, Allocator.Persistent);

        foreach (PathNode node in request.grid)
        {
            gridNodes.Add(node);
            nodesIndexes.Add((node.x, node.z), node.index);
        }

        job = new()
        {
            nodesAmountX = Mathf.RoundToInt(request.gridHalfSize.x * 2 / (request.nodeRadius * 2)),
            nodesAmountZ = Mathf.RoundToInt(request.gridHalfSize.y * 2 / (request.nodeRadius * 2)),
            gridHalfSize = request.gridHalfSize,
            centerPos = request.gridCenter,

            startingPos = request.startPos,
            targetPos = request.targetPos,

            gridNodes = gridNodes,
            nodesIndices = nodesIndexes,
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

        request.callback(path);
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
