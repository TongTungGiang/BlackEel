using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

namespace BE.ECS
{
    public class DieSystem : JobComponentSystem
    {
        struct DieSystemJob : IJobForEachWithEntity<HealthComponent>
        {
            [WriteOnly]
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity, int jobIndex, ref HealthComponent health)
            {
                if (health.Health < 0)
                {
                    CommandBuffer.DestroyEntity(jobIndex, entity);
                }
            }
        }

        EntityCommandBufferSystem m_Barrier;

        protected override void OnCreate()
        {
            m_Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent();
            var job = new DieSystemJob() { CommandBuffer = commandBuffer };

            var jobHandle = job.Schedule(this, inputDependencies);
            m_Barrier.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        }
    }
}