using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

namespace BE.ECS
{
    [UpdateBefore(typeof(DamageSystem))]
    public class AttackSystem : JobComponentSystem
    {
        [RequireComponentTag(typeof(InAttackRangeTag))]
        struct AttackSystemJob : IJobForEachWithEntity<AttackTargetComponent, AttackStatusComponent>
        {
            [ReadOnly] public float time;
            [ReadOnly] public float damage;
            [ReadOnly] public float attackRate;
            [WriteOnly] public EntityCommandBuffer.Concurrent commandBuffer;

            public void Execute(Entity entity, int jobIndex, ref AttackTargetComponent attackTarget, ref AttackStatusComponent attackStatus)
            {
                if (time - attackStatus.LastAttack >= attackRate)
                {
                    attackStatus.LastAttack = time;
                    Entity e = commandBuffer.CreateEntity(jobIndex);
                    commandBuffer.AddComponent(jobIndex, e,
                        new DamageComponent { Source = entity, Target = attackTarget.Target, Amount = damage });
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

            var job = new AttackSystemJob
            {
                time = Time.time,
                attackRate = GameData.Instance.agentAttackRate,
                damage = GameData.Instance.agentDamage,
                commandBuffer = commandBuffer
            };

            return job.Schedule(this, inputDependencies);
        }
    }
}