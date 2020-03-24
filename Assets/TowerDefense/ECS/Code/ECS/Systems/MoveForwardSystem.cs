using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

namespace BE.ECS
{
    public class MoveForwardSystem : JobComponentSystem
    {
        struct MoveForwardJob : IJobForEachWithEntity<Translation, Rotation, MoveForwardComponent, MoveSpeedComponent>
        {
            public float DeltaTime;

            [WriteOnly]
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity, int index, ref Translation t, ref Rotation r, ref MoveForwardComponent mf, ref MoveSpeedComponent ms)
            {
                float3 position = t.Value;
                float3 direction = mf.Target - position;
                float moveStep = ms.Value * DeltaTime;

                if (math.length(direction) > moveStep)
                {
                    position += math.normalize(direction) * moveStep;
                }
                else
                {
                    position = mf.Target;
                    CommandBuffer.RemoveComponent<MoveForwardComponent>(index, entity);
                }

                t.Value = position;

                r.Value = quaternion.LookRotation(direction, math.up());
            }
        }        

        EntityCommandBufferSystem m_Barrier;

        protected override void OnCreate()
        {
            m_Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent();
            var job = new MoveForwardJob() { DeltaTime = Time.deltaTime, CommandBuffer = commandBuffer };

            var jobHandle = job.Schedule(this, inputDeps);
            m_Barrier.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        }
    }
}