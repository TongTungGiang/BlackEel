using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace BE.ECS
{
    public class FindAttackTargetSystem : JobComponentSystem
    {
        private EntityQueryDesc m_QueryDesc;
        private EntityQuery m_AllyQuery;
        private EntityQuery m_EnemyQuery;
        EntityCommandBufferSystem m_Barrier;

        protected override void OnCreate()
        {
            m_QueryDesc = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<AgentTag>(),
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<TeamComponent>()
                },

                None = new ComponentType[]
                {
                    typeof(AttackTargetComponent)
                },
            };

            m_AllyQuery = GetEntityQuery(m_QueryDesc);
            m_AllyQuery.SetFilter(new TeamComponent { IsEnemy = false });

            m_EnemyQuery = GetEntityQuery(m_QueryDesc);
            m_EnemyQuery.SetFilter(new TeamComponent { IsEnemy = true });

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
                            var attackTarget = EntityToTestAgainst[j];
                            var attackTargetComponent = new AttackTargetComponent { Target = attackTarget };
                            CommandBuffer.AddComponent(chunkIndex, chunkEntities[i], attackTargetComponent);
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
                TranslationToTestAgainst = m_AllyQuery.ToComponentDataArray<Translation>(Allocator.TempJob),
                EntityToTestAgainst = m_AllyQuery.ToEntityArray(Allocator.TempJob),
                AttackRadiusType = radiusType,
                TranslationType = translationType,
                EntityType = entityType,
                CommandBuffer = commandBuffer
            };
            JobHandle jobHandleEvA = jobEvA.Schedule(m_EnemyQuery, inputDependencies);

            var jobAvE = new RangeQueryJob()
            {
                TranslationToTestAgainst = m_EnemyQuery.ToComponentDataArray<Translation>(Allocator.TempJob),
                EntityToTestAgainst = m_EnemyQuery.ToEntityArray(Allocator.TempJob),
                AttackRadiusType = radiusType,
                TranslationType = translationType,
                EntityType = entityType,
                CommandBuffer = commandBuffer
            };
            JobHandle jobHandleAvE = jobAvE.Schedule(m_AllyQuery, jobHandleEvA);

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
