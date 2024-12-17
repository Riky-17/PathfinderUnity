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

    float sizeX = 50f;
    float sizeZ = 50f;
    float nodeRadius = .5f;

    public PathFinderJobContainer(PathfinderRequest request)
    {
        this.request = request;
        gridNodes = PathFinderGrid.CreateGrid(sizeX, sizeZ, nodeRadius, request.obstacleLayer);
        jobResult = new(Allocator.Persistent);
        path = new();
        job = new()
        {
            gridSizeX = sizeX,
            gridSizeZ = sizeZ,
            nodeDiameter = nodeRadius * 2,

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
