using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

namespace BE.ECS
{
    public class DamageSystem : JobComponentSystem
    {
        [BurstCompile]
        struct DamageSystemJob : IJobForEachWithEntity<DamageComponent, HealthComponent>
        {
            [WriteOnly]
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity, int jobIndex, ref DamageComponent damage, ref HealthComponent health)
            {
                health.Value -= damage.Amount;

                CommandBuffer.RemoveComponent<DamageComponent>(jobIndex, entity);
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
            var job = new DamageSystemJob() { CommandBuffer = commandBuffer };
            return job.Schedule(this, inputDependencies);
        }
    }
}
