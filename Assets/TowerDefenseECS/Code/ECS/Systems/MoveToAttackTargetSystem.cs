using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace BE.ECS
{
    [UpdateBefore(typeof(MoveForwardSystem))]
    public class MoveToAttackTargetSystem : JobComponentSystem
    {
        private EntityQuery m_AllyGroup;
        private EntityQuery m_EnemyGroup;
        private EntityCommandBufferSystem m_Barrier;

        struct MoveToAttackTargetSystemJob : IJobChunk
        {
            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            [ReadOnly] public ArchetypeChunkComponentType<MoveForwardComponent> MoveForwardType;
            [ReadOnly] public ArchetypeChunkComponentType<AttackTargetComponent> AttackTargetType;
            [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationType;
            [ReadOnly] public ArchetypeChunkSharedComponentType<EnemyTeamComponent> EnemyType;

            [ReadOnly] public ComponentDataFromEntity<Translation> AllTranslation;

            [WriteOnly] public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var chunkEntities = chunk.GetNativeArray(EntityType);
                var chunkTranslation = chunk.GetNativeArray(TranslationType);
                var chunkTarget = chunk.GetNativeArray(AttackTargetType);

                if (chunk.Has(MoveForwardType))
                {
                    for (int i = 0; i < chunk.Count; i++)
                    {
                        Entity target = chunkTarget[i].Target;
                        float3 targetPos = AllTranslation[target].Value;
                        CommandBuffer.SetComponent<MoveForwardComponent>(
                            chunkIndex,
                            chunkEntities[i],
                            new MoveForwardComponent() { Target = GetStopPosition(chunkTranslation[i].Value, targetPos) });
                    }
                }
                else
                {
                    for (int i = 0; i < chunk.Count; i++)
                    {
                        Entity target = chunkTarget[i].Target;
                        float3 targetPos = AllTranslation[target].Value;
                        CommandBuffer.AddComponent<MoveForwardComponent>(
                            chunkIndex,
                            chunkEntities[i],
                            new MoveForwardComponent() { Target = GetStopPosition(chunkTranslation[i].Value, targetPos) });
                    }
                }

                if (chunk.Has(EnemyType))
                {
                    for (int i = 0; i < chunk.Count; i++)
                    {
                        CommandBuffer.RemoveComponent<FollowWaypointTag>(chunkIndex, chunkEntities[i]);
                    }
                }
            }

            private float3 GetStopPosition(float3 myPosition, float3 enemyPosition)
            {
                float3 enemyToPlayer = myPosition - enemyPosition;
                return enemyPosition + math.normalize(enemyToPlayer) * GameData.Instance.agentStoppingDistance;
            }
        }

        protected override void OnCreate()
        {
            EntityQueryDesc allyQueryDesc = new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<AgentTag>(),
                    ComponentType.ReadOnly<AllyTeamComponent>(),
                    ComponentType.ReadOnly<AttackTargetComponent>(),
                }
            };
            m_AllyGroup = GetEntityQuery(allyQueryDesc);

            EntityQueryDesc enemyQueryDesc = new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<AgentTag>(),
                    ComponentType.ReadOnly<EnemyTeamComponent>(),
                    ComponentType.ReadOnly<AttackTargetComponent>(),
                }
            };
            m_EnemyGroup = GetEntityQuery(enemyQueryDesc);

            m_Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent();

            var translationType = GetArchetypeChunkComponentType<Translation>(true);
            var attackTargetType = GetArchetypeChunkComponentType<AttackTargetComponent>(true);
            var moveForwardType = GetArchetypeChunkComponentType<MoveForwardComponent>(false);
            var enemyType = GetArchetypeChunkSharedComponentType<EnemyTeamComponent>();
            var entityType = GetArchetypeChunkEntityType();
            var allTranslation = GetComponentDataFromEntity<Translation>(true);

            var allyJob = new MoveToAttackTargetSystemJob()
            {
                EntityType = entityType,
                AttackTargetType = attackTargetType,
                MoveForwardType = moveForwardType,
                TranslationType = translationType,
                AllTranslation = allTranslation,
                EnemyType = enemyType,
                CommandBuffer = commandBuffer
            };
            var allyJobHandle = allyJob.Schedule(m_AllyGroup, inputDependencies);
            m_Barrier.AddJobHandleForProducer(allyJobHandle);

            var enemyJob = new MoveToAttackTargetSystemJob()
            {
                EntityType = entityType,
                AttackTargetType = attackTargetType,
                MoveForwardType = moveForwardType,
                TranslationType = translationType,
                EnemyType = enemyType,
                AllTranslation = allTranslation,
                CommandBuffer = commandBuffer
            };
            var enemyJobHandle = enemyJob.Schedule(m_EnemyGroup, allyJobHandle);
            m_Barrier.AddJobHandleForProducer(enemyJobHandle);

            return enemyJobHandle;
        }
    }
}
