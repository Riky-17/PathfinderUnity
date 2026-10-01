using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    Stack<PathFinderJobInfo> pool;

    public ObjectPool() => PopulatePool();

    void PopulatePool()
    {
        pool = new();
        for (int i = 0; i < 10; i++)
            pool.Push(new());
    }

    public PathFinderJobInfo RequestJob(PathfinderRequest request)
    {
        PathFinderJobInfo job;
        if(pool.Count > 0)
        {
            job = pool.Pop();
            job.SetUpRequest(request);
            return job;
        }

        job = new(request);
        return job;
    }

    public void ReturnToPool(PathFinderJobInfo job)
    {
        job.CompleteJob();
        pool.Push(job);
    }

}
