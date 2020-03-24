using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

namespace BE.ECS
{
    public class PathwayMovementSystem : ComponentSystem
    {
        private EntityQuery m_Query;
        private Unity.Mathematics.Random m_Random;

        protected override void OnCreate()
        {
            EntityQueryDesc desc = new EntityQueryDesc()
            {
                All = new ComponentType[] { typeof(WaypointMovementComponent), typeof(FollowWaypointTag) },
                None = new ComponentType[] { typeof(MoveForwardComponent), typeof(AttackTargetComponent) }
            };
            m_Query = GetEntityQuery(desc);

            m_Random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(0, 1000));
        }

        protected override void OnUpdate()
        {
            var entities = m_Query.ToEntityArray(Allocator.TempJob);

            foreach (var e in entities)
            {
                WaypointMovementComponent waypointMovement = EntityManager.GetComponentData<WaypointMovementComponent>(e);
                waypointMovement.CurrentTargetIndex++;
                EntityManager.SetComponentData(e, waypointMovement);

                if (World.GetOrCreateSystem<WaypointManagementSystem>().GetWaypointPosition(waypointMovement.CurrentTargetIndex, out float3 waypointPosition))
                {
                    waypointPosition += new float3(m_Random.NextFloat(-GameData.Instance.spawnPositionNoise, GameData.Instance.spawnPositionNoise), 0, m_Random.NextFloat(-GameData.Instance.spawnPositionNoise, GameData.Instance.spawnPositionNoise));
                    EntityManager.AddComponentData(e, new MoveForwardComponent { Target = waypointPosition });
                }
                else
                {
                    EntityManager.RemoveComponent<WaypointMovementComponent>(e);
                }
            }

            entities.Dispose();
        }
    }
}
