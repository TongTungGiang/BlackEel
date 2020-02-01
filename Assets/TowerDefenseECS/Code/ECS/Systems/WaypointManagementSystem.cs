using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

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

        public bool GetWaypointPosition(int index, out float3 position)
        {
            m_Query.SetFilter(new WaypointIndexComponent() { Value = index });
            var queryResultEntities = m_Query.ToEntityArray(Allocator.TempJob);

            if (queryResultEntities.Length == 0)
            {
                position = float3.zero;
                queryResultEntities.Dispose();
                return false;
            }

            position = EntityManager.GetComponentData<Translation>(queryResultEntities[0]).Value;
            queryResultEntities.Dispose();
            return true;
        }
    }
}