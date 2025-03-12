using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PathFinderJobContainer : IDisposable
{
    PathFinderJob job;
    JobHandle jobHandle;
    PathfinderRequest request;
    NativeArray<PathNode> gridNodes;
    NativeList<float3> jobResult;
    public List<Vector3> path;

    public PathFinderJobContainer(PathfinderRequest request)
    {
        this.request = request;
        jobResult = new(Allocator.Persistent);
        gridNodes = new(request.gridNodes, Allocator.Persistent);
        path = new();
        job = new()
        {
            gridSizeX = 50f,
            gridSizeZ = 50f,
            nodeDiameter = request.nodeDiameter,

            startingPos = request.startPos,
            targetPos = request.targetPos,

            gridNodes = gridNodes,
            path = jobResult,
        };
    }

    public void CompleteJob()
    {
        jobHandle.Complete();

        foreach (float3 pos in jobResult)
            path.Add(pos);
            
        jobResult.Dispose();
        gridNodes.Dispose();

        request.callback(path, true);
    }

    public bool IsComplete() => jobHandle.IsCompleted;

    public void ScheduleJob() => jobHandle = job.Schedule();

    public void Dispose()
    {
        jobHandle.Complete();
        jobResult.Dispose();
        gridNodes.Dispose();
    }
}
