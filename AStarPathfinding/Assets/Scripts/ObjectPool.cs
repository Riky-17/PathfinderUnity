using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    int initialSize;
    Stack<PathFinderJobContainer> pool;
    
    public ObjectPool(int initialSize)
    {
        this.initialSize = initialSize;
        PopulatePool();
    }

    public ObjectPool()
    {
        initialSize = 5;
        PopulatePool();
    }

    void PopulatePool()
    {
        pool = new();
        for (int i = 0; i < initialSize; i++)
            pool.Push(new());
    }

    public PathFinderJobContainer RequestJob(PathfinderRequest request)
    {
        PathFinderJobContainer job;
        if(pool.Count > 0)
        {
            job = pool.Pop();
            job.SetUpRequest(request);
            return job;
        }

        job = new(request);
        return job;
    }

    public void ReturnToPool(PathFinderJobContainer job)
    {
        job.CompleteJob();
        pool.Push(job);
    }

}
