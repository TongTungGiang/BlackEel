﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace BE.ECS
{
    public class FindAttackTargetSystem : JobComponentSystem
    {
        private EntityQuery m_Ally_NoAttackTarget;
        private EntityQuery m_Enemy_NoAttackTarget;
        private EntityQuery m_Ally_All;
        private EntityQuery m_Enemy_All;
        EntityCommandBufferSystem m_Barrier;

        protected override void OnCreate()
        {
            EntityQueryDesc allyWithoutAttackTarget = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<AgentTag>(),
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<AllyTeamComponent>()
                },

                None = new ComponentType[]
                {
                    typeof(AttackTargetComponent)
                },
            };
            m_Ally_NoAttackTarget = GetEntityQuery(allyWithoutAttackTarget);

            EntityQueryDesc enemyWithoutAttackTarget = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<AgentTag>(),
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<EnemyTeamComponent>()
                },

                None = new ComponentType[]
                {
                    typeof(AttackTargetComponent)
                },
            };
            m_Enemy_NoAttackTarget = GetEntityQuery(enemyWithoutAttackTarget);

            EntityQueryDesc allAlly = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<AgentTag>(),
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<AllyTeamComponent>()
                }
            };
            m_Ally_All = GetEntityQuery(allAlly);

            EntityQueryDesc allEnemy = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<AgentTag>(),
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<EnemyTeamComponent>()
                }
            };
            m_Enemy_All = GetEntityQuery(allEnemy);

            m_Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        struct RangeQueryJob : IJobChunk
        {
            [DeallocateOnJobCompletion]
            [ReadOnly] public NativeArray<Translation> TranslationToTestAgainst;
            [DeallocateOnJobCompletion]
            [ReadOnly] public NativeArray<Entity> EntityToTestAgainst;

            [ReadOnly] public ArchetypeChunkComponentType<AttackRadiusComponent> AttackRadiusType;
            [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationType;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;

            [WriteOnly] public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var chunkAttackRadius = chunk.GetNativeArray(AttackRadiusType);
                var chunkTranslations = chunk.GetNativeArray(TranslationType);
                var chunkEntities = chunk.GetNativeArray(EntityType);

                for (int i = 0; i < chunk.Count; i++)
                {
                    AttackRadiusComponent radius = chunkAttackRadius[i];
                    Translation pos = chunkTranslations[i];

                    for (int j = 0; j < TranslationToTestAgainst.Length; j++)
                    {
                        Translation targetPos = TranslationToTestAgainst[j];

                        if (CheckInRange(targetPos.Value, pos.Value, radius.Value * radius.Value))
                        {
                            var attackTargetComponent = new AttackTargetComponent { Target = EntityToTestAgainst[j] };
                            CommandBuffer.AddComponent(chunkIndex, chunkEntities[i], attackTargetComponent);
                            Debug.LogFormat("{0} found target: {1}", chunkEntities[i].Index, EntityToTestAgainst[j].Index);
                            break;
                        }
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent();

            var radiusType = GetArchetypeChunkComponentType<AttackRadiusComponent>(true);
            var translationType = GetArchetypeChunkComponentType<Translation>(true);
            var entityType = GetArchetypeChunkEntityType();

            var jobEvA = new RangeQueryJob()
            {
                TranslationToTestAgainst = m_Ally_All.ToComponentDataArray<Translation>(Allocator.TempJob),
                EntityToTestAgainst = m_Ally_All.ToEntityArray(Allocator.TempJob),
                AttackRadiusType = radiusType,
                TranslationType = translationType,
                EntityType = entityType,
                CommandBuffer = commandBuffer
            };
            JobHandle jobHandleEvA = jobEvA.Schedule(m_Enemy_NoAttackTarget, inputDependencies);

            var jobAvE = new RangeQueryJob()
            {
                TranslationToTestAgainst = m_Enemy_All.ToComponentDataArray<Translation>(Allocator.TempJob),
                EntityToTestAgainst = m_Enemy_All.ToEntityArray(Allocator.TempJob),
                AttackRadiusType = radiusType,
                TranslationType = translationType,
                EntityType = entityType,
                CommandBuffer = commandBuffer
            };
            JobHandle jobHandleAvE = jobAvE.Schedule(m_Ally_NoAttackTarget, jobHandleEvA);

            return jobHandleAvE;
        }

        private static bool CheckInRange(float3 target, float3 center, float radiusSqr)
        {
            float3 delta = target - center;
            float distanceSquare = delta.x * delta.x + delta.z * delta.z;

            return distanceSquare <= radiusSqr;
        }
    }
}
