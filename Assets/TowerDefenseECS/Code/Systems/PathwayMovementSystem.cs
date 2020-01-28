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
        EntityQuery m_Query;

        protected override void OnCreate()
        {
            EntityQueryDesc desc = new EntityQueryDesc()
            {
                All = new ComponentType[] { typeof(WaypointMovementComponent), typeof(FollowWaypointTag) },
                None = new ComponentType[] { typeof(MoveForwardComponent) }
            };
            m_Query = GetEntityQuery(desc);
        }

        protected override void OnUpdate()
        {
            var entities = m_Query.ToEntityArray(Allocator.TempJob);

            foreach (var e in entities)
            {
                WaypointMovementComponent waypointMovement = EntityManager.GetComponentData<WaypointMovementComponent>(e);
                waypointMovement.CurrentTargetIndex++;
                EntityManager.SetComponentData(e, waypointMovement);

                EntityManager.AddComponentData(e, new MoveForwardComponent { Target = World.GetOrCreateSystem<WaypointManagementSystem>().GetWaypointPosition(waypointMovement.CurrentTargetIndex) });
            }

            entities.Dispose();
        }
    }
}
