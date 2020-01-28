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

        [ExcludeComponent(typeof(MoveForwardComponent))]
        [RequireComponentTag(typeof(FollowWaypointTag))]
        struct UpdatePathwayTargetJob : IJobForEachWithEntity<WaypointMovementComponent>
        {
            [WriteOnly]
            public EntityCommandBuffer.Concurrent CommandBuffer;
            public WaypointManagementSystem WaypointManagement;

            public void Execute(Entity e, int index, ref WaypointMovementComponent waypointMovement)
            {
                waypointMovement.CurrentTargetIndex++;

                CommandBuffer.AddComponent(index, e, new MoveForwardComponent { Target = WaypointManagement.GetWaypointPosition(waypointMovement.CurrentTargetIndex) });
            }
        }

        EntityCommandBufferSystem m_Barrier;
        WaypointManagementSystem m_Waypoint;

        protected override void OnCreate()
        {
            m_Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            m_Waypoint = World.GetOrCreateSystem<WaypointManagementSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent();
            var job = new UpdatePathwayTargetJob() { CommandBuffer = commandBuffer, WaypointManagement = m_Waypoint };

            var jobHandle = job.Schedule(this, inputDeps);
            m_Barrier.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        }
    }
}
