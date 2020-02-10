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
        private EntityQuery m_AgentGroup;
        private EntityCommandBufferSystem m_Barrier;

        struct MoveToAttackTargetSystemJob : IJobChunk
        {
            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            [ReadOnly] public ArchetypeChunkComponentType<MoveForwardComponent> MoveForwardType;
            [ReadOnly] public ArchetypeChunkComponentType<AttackTargetComponent> AttackTargetType;
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
            }
        }

        protected override void OnCreate()
        {
            EntityQueryDesc queryDesc = new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<AgentTag>(),
                    ComponentType.ReadOnly<AllyTeamComponent>(),
                    ComponentType.ReadOnly<AttackTargetComponent>(),
                }
            };
            m_AgentGroup = GetEntityQuery(queryDesc);

            m_Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent();

            var attackTargetType = GetArchetypeChunkComponentType<AttackTargetComponent>(true);
            var moveForwardType = GetArchetypeChunkComponentType<MoveForwardComponent>(false);
            var entityType = GetArchetypeChunkEntityType();
            var allTranslation = GetComponentDataFromEntity<Translation>(true);

            var job = new MoveToAttackTargetSystemJob()
            {
                EntityType = entityType,
                AttackTargetType = attackTargetType,
                MoveForwardType = moveForwardType,
                AllTranslation = allTranslation,
                CommandBuffer = commandBuffer
            };

            // Now that the job is set up, schedule it to be run. 
            var jobHandle = job.Schedule(m_AgentGroup, inputDependencies);
            m_Barrier.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        }
    }
}
