using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

namespace BE.ECS
{
    public class PathwayMovementSystem : JobComponentSystem
    {
        struct PathwayMovementJob : IJobChunk
        {
            [ReadOnly] public NativeArray<Translation> AllWaypoint;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            [ReadOnly] public ArchetypeChunkComponentType<WaypointMovementComponent> WaypointMovementType;
            [ReadOnly] public Random Random;
            [WriteOnly] public EntityCommandBuffer.Concurrent CommandBuffer;
            [ReadOnly] public float Noise;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var chunkEntities = chunk.GetNativeArray(EntityType);
                var chunkWaypointMovement = chunk.GetNativeArray(WaypointMovementType);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity target = chunkEntities[i];
                    WaypointMovementComponent waypointMovement = chunkWaypointMovement[i];
                    waypointMovement.CurrentTargetIndex++;

                    if (waypointMovement.CurrentTargetIndex > 0 && waypointMovement.CurrentTargetIndex < AllWaypoint.Length)
                    {
                        var noise = new float3(Random.NextFloat(-Noise, Noise), 0, Random.NextFloat(-Noise, Noise));
                        var waypointPos = AllWaypoint[waypointMovement.CurrentTargetIndex].Value + noise;
                        CommandBuffer.AddComponent(chunkIndex, target, new MoveForwardComponent { Target = waypointPos });
                        CommandBuffer.SetComponent(chunkIndex, target, waypointMovement);
                    }
                    else
                    {
                        CommandBuffer.RemoveComponent<WaypointMovementComponent>(chunkIndex, target);
                    }
                }
            }
        }

        private EntityQuery m_AgentQuery;
        private NativeArray<Translation> m_AllWaypointTranslation;
        private bool m_WaypointTranslationInitialized;
        private Unity.Mathematics.Random m_Random;
        private EntityCommandBufferSystem m_Barrier;
        private float m_NoiseValue;

        private NativeArray<Translation> AllWaypointTranslation
        {
            get
            {
                if (m_WaypointTranslationInitialized == false)
                {

                    var allWaypoint = GetEntityQuery(typeof(WaypointIndexComponent));
                    int count = allWaypoint.CalculateEntityCount();
                    if (count > 0)
                    {
                        m_AllWaypointTranslation = new NativeArray<Translation>(count, Allocator.Persistent);
                        NativeArray<Entity> waypointEntities = allWaypoint.ToEntityArray(Allocator.TempJob);
                        for (int i = 0; i < waypointEntities.Length; i++)
                        {
                            WaypointIndexComponent index = EntityManager.GetComponentData<WaypointIndexComponent>(waypointEntities[i]);
                            Translation trans = EntityManager.GetComponentData<Translation>(waypointEntities[i]);
                            m_AllWaypointTranslation[index.Value] = trans;
                        }
                        waypointEntities.Dispose();
                        m_WaypointTranslationInitialized = true;
                    }
                }

                return m_AllWaypointTranslation;
            }
        }

        protected override void OnCreate()
        {
            EntityQueryDesc agentQueryDesc = new EntityQueryDesc()
            {
                All = new ComponentType[] { typeof(WaypointMovementComponent), typeof(FollowWaypointTag) },
                None = new ComponentType[] { typeof(MoveForwardComponent), typeof(AttackTargetComponent) }
            };
            m_AgentQuery = GetEntityQuery(agentQueryDesc);

            m_Random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(0, 1000));
            m_Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            m_NoiseValue = GameData.Instance.spawnPositionNoise;
        }

        protected override void OnDestroy()
        {
            m_AllWaypointTranslation.Dispose();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent();
            PathwayMovementJob pathwayMovementJob = new PathwayMovementJob
            {
                EntityType = GetArchetypeChunkEntityType(),

                WaypointMovementType = GetArchetypeChunkComponentType<WaypointMovementComponent>(false),

                AllWaypoint = AllWaypointTranslation,

                CommandBuffer = commandBuffer,

                Random = m_Random,

                Noise = m_NoiseValue
            };

            var handle = pathwayMovementJob.Schedule(m_AgentQuery, inputDeps);
            m_Barrier.AddJobHandleForProducer(handle);

            return handle;
        }
    }
}
