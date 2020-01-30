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
        struct DamageSystemJob : IJobForEach<DamageComponent, HealthComponent>
        {
            public void Execute(ref DamageComponent damage, ref HealthComponent health)
            {
                health.Value -= damage.Amount;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new DamageSystemJob() { };
            return job.Schedule(this, inputDependencies);
        }
    }
}
