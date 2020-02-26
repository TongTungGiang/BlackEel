using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

namespace BE.ECS
{
    public class AttackSystem : JobComponentSystem
    {
        struct AttackSystemJob : IJobForEach<Translation, Rotation>
        {
            public float damage;
            public float attackRate;

            public void Execute(ref Translation translation, [ReadOnly] ref Rotation rotation)
            {

            }
        }

        protected override void OnCreate()
        {
            base.OnCreate();

        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new AttackSystemJob
            {
                attackRate = GameData.Instance.agentAttackRate,
                damage = GameData.Instance.agentDamage
            };

            return job.Schedule(this, inputDependencies);
        }
    }
}