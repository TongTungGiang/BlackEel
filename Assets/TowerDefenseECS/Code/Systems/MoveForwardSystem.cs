using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;

namespace BE.ECS
{
    [BurstCompile]
    public class MoveForwardSystem : JobComponentSystem
    {
        struct MoveForwardJob : IJobForEach<Translation, MoveForwardComponent, MoveSpeedComponent>
        {
            public float dt;

            public void Execute(ref Translation t, ref MoveForwardComponent mf, ref MoveSpeedComponent ms)
            {
                float3 position = t.Value;
                float3 direction = mf.Target - position;
                position += math.normalize(direction) * ms.Value * Time.deltaTime;

                t.Value = position;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new MoveForwardJob() { dt = Time.deltaTime };
            return job.Schedule(this, inputDeps);
        }
    }
}