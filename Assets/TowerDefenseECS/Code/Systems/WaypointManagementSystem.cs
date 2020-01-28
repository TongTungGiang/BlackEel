using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

namespace BE.ECS
{
    public class WaypointManagementSystem : ComponentSystem
    {
        private EntityQuery m_Query;

        protected override void OnCreate()
        {
            m_Query = GetEntityQuery(typeof(WaypointIndexComponent));
        }

        protected override void OnUpdate()
        {
            // Do nothing
        }

        public float3 GetWaypointPosition(int index)
        {
            m_Query.SetFilter(new WaypointIndexComponent() { Value = index });
            var queryResultEntities = m_Query.ToEntityArray(Allocator.TempJob);
            float3 position = EntityManager.GetComponentData<Translation>(queryResultEntities[0]).Value;
            queryResultEntities.Dispose();

            return position;
        }
    }
}