using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace BE.ECS
{
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
            [ReadOnly] public ArchetypeChunkSharedComponentType<EnemyTeamComponent> EnemyType;

            [ReadOnly] public ComponentDataFromEntity<Translation> AllTranslation;

            [WriteOnly] public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var chunkEntities = chunk.GetNativeArray(EntityType);
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
                            new MoveForwardComponent() { Target = targetPos });
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
                            new MoveForwardComponent() { Target = targetPos });
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
                AllTranslation = allTranslation,
                EnemyType = enemyType,
                CommandBuffer = commandBuffer
            };
            var allyJobHandle = allyJob.Schedule(m_AllyGroup, inputDependencies);

            var enemyJob = new MoveToAttackTargetSystemJob()
            {
                EntityType = entityType,
                AttackTargetType = attackTargetType,
                MoveForwardType = moveForwardType,
                EnemyType = enemyType,
                AllTranslation = allTranslation,
                CommandBuffer = commandBuffer
            };
            var enemyJobHandle = enemyJob.Schedule(m_EnemyGroup, allyJobHandle);

            m_Barrier.AddJobHandleForProducer(allyJobHandle);
            m_Barrier.AddJobHandleForProducer(enemyJobHandle);

            return enemyJobHandle;
        }
    }
}
