using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace BE.ECS
{
    public class SpawnPointManagementSystem : ComponentSystem
    {
        private int m_TotalSpawnPoint = -1;
        private EntityQuery m_Query;
        private Random m_Random;

        private int TotalSpawnPoint
        {
            get
            {
                if (m_TotalSpawnPoint > 0)
                {
                    return m_TotalSpawnPoint;
                }

                var countEntityQuery = GetEntityQuery(typeof(AllySpawnPointCountComponent));
                var countEntities = countEntityQuery.ToComponentDataArray<AllySpawnPointCountComponent>(Allocator.TempJob);
                if (countEntities.Length == 0)
                {
                    countEntities.Dispose();
                    return -1;
                }

                m_TotalSpawnPoint = countEntities[0].Value;
                countEntities.Dispose();

                return m_TotalSpawnPoint;
            }
        }

        protected override void OnCreate()
        {
            m_Query = GetEntityQuery(typeof(AllySpawnPointIndexComponent));

            m_Random = new Random((uint)UnityEngine.Random.Range(1, 10000));
        }

        protected override void OnUpdate()
        {
            // Do nothing
        }

        public bool GetRandomSpawnPointPosition(out float3 position)
        {
            if (TotalSpawnPoint < 0)
            {
                position = float3.zero;
                return false;
            }

            int randomIndex = m_Random.NextInt() % TotalSpawnPoint;
            return GetSpawnPointPosition(randomIndex, out position);
        }

        public bool GetSpawnPointPosition(int index, out float3 position)
        {
            if (TotalSpawnPoint < 0)
            {
                position = float3.zero;
                return false;
            }

            if (index < 0 || index >= TotalSpawnPoint)
            {
                position = float3.zero;
                return false;
            }

            m_Query.SetFilter(new AllySpawnPointIndexComponent() { Value = index });
            var queryResultEntities = m_Query.ToEntityArray(Allocator.TempJob);

            position = EntityManager.GetComponentData<Translation>(queryResultEntities[0]).Value;
            queryResultEntities.Dispose();
            return true;
        }
    }
}