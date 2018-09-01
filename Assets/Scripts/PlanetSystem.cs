using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

public class PlanetSystem : JobComponentSystem {
    ComponentGroup allPlanets;
    ComponentGroup suns;
    NativeArray<Position> sunPositions;
    protected override void OnCreateManager(int capacity)
    {
        allPlanets = GetComponentGroup(typeof(Position), typeof(Heading), typeof(MoveSpeed));
        suns = GetComponentGroup(typeof(Sun), typeof(Position));
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var positions = allPlanets.GetComponentDataArray<Position>();
        var headings = allPlanets.GetComponentDataArray<Heading>();
        var speeds = allPlanets.GetComponentDataArray<MoveSpeed>();

        var sunPos = suns.GetComponentDataArray<Position>();
        sunPositions = new NativeArray<Position>(sunPos.Length, Allocator.Persistent);

        for(int i = 0; i < sunPos.Length; i++)
        {
            sunPositions[i] = sunPos[i];
        }

        var steerJob = new Steer
        {
            headings = headings,
            planetPositions = positions,
            speeds = speeds,
            sunPositions = sunPositions
        };
        var steerJobHandle = steerJob.Schedule(allPlanets.CalculateLength(), 64);
        steerJobHandle.Complete();
        sunPositions.Dispose();
        inputDeps = steerJobHandle;

        return inputDeps;
    }
}

struct Steer : IJobParallelFor
{
    public ComponentDataArray<Heading> headings;
    public ComponentDataArray<Position> planetPositions;
    public ComponentDataArray<MoveSpeed> speeds;
    [ReadOnly]
    public NativeArray<Position> sunPositions;

    public void Execute(int index)
    {
        float3 newHeading = headings[index].Value;
        float orbitalSpeed = 10;
        float closestPlaneDist = 100000;

        for(int j = 0; j < sunPositions.Length; j++)
        {
            float3 sunPos = sunPositions[j].Value;
            float3 difference = math.normalize(sunPos - planetPositions[index].Value);
            float distance = math.lengthSquared(difference) + 0.1f;
            if(distance < closestPlaneDist)
            {
                closestPlaneDist = distance;
            }
            float gravity = math.clamp(distance / 100f, 0, 1);
            newHeading += math.lerp(headings[index].Value, difference, gravity);
        }
        orbitalSpeed = math.sqrt(500 / closestPlaneDist);
        headings[index] = new Heading { Value = math.normalize(newHeading) };
        speeds[index] = new MoveSpeed { speed = orbitalSpeed };
    }
}
