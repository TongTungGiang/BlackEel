using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;

namespace BE.ECS
{
    public class SpawnEnemySystem : ComponentSystem
    {
        private float m_LastSpawn;
        private float m_SpawnRate;
        private Unity.Mathematics.Random m_Random;
        private float3 m_FirstWaypointPos;
        private bool m_HasFirstWaypointInitialized = false;

        private float3 FirstWaypointPos
        {
            get
            {
                if (m_HasFirstWaypointInitialized == false)
                {
                    var allWaypoint = GetEntityQuery(typeof(WaypointIndexComponent));
                    int count = allWaypoint.CalculateEntityCount();
                    if (count > 0)
                    {
                        NativeArray<Entity> waypointEntities = allWaypoint.ToEntityArray(Allocator.TempJob);
                        for (int i = 0; i < waypointEntities.Length; i++)
                        {
                            WaypointIndexComponent index = EntityManager.GetComponentData<WaypointIndexComponent>(waypointEntities[i]);
                            Translation trans = EntityManager.GetComponentData<Translation>(waypointEntities[i]);
                            if (index.Value == 0)
                            {
                                m_FirstWaypointPos = trans.Value;
                                m_HasFirstWaypointInitialized = true;
                                break;
                            }
                        }
                        waypointEntities.Dispose();
                    }
                }

                return m_FirstWaypointPos;
            }
        }

        protected override void OnCreate()
        {
            m_SpawnRate = GameData.Instance.spawnRate;

            m_LastSpawn = UnityEngine.Time.time;

            m_Random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(0, 1000));
        }

        protected override void OnUpdate()
        {
            if (UnityEngine.Time.time - m_LastSpawn < m_SpawnRate)
            {
                return;
            }

            m_LastSpawn = UnityEngine.Time.time - m_Random.NextFloat(-GameData.Instance.spawnRateNoise, GameData.Instance.spawnRateNoise);

            int batchCount = m_Random.NextInt(GameData.Instance.enemySpawnBatchCountMin, GameData.Instance.enemySpawnBatchCountMax);
            for (int i = 0; i < batchCount; i++)
            {
                var instanceSpawnPos = FirstWaypointPos +
                    new float3(m_Random.NextFloat(-GameData.Instance.spawnPositionNoise, GameData.Instance.spawnPositionNoise),
                    0, m_Random.NextFloat(-GameData.Instance.spawnPositionNoise, GameData.Instance.spawnPositionNoise));

                Entity prefab = GameData.Instance.EnemyEntityPrefab;
                Entity instance = EntityManager.Instantiate(prefab);
                EntityManager.SetName(instance, "Enemy");
                EntityManager.AddComponentData(instance, new AgentTag { });

                EntityManager.SetComponentData(instance, new Translation { Value = instanceSpawnPos });
                EntityManager.AddComponentData(instance, new MoveSpeedComponent { Value = GameData.Instance.agentMoveSpeed });

                EntityManager.AddComponentData(instance, new FollowWaypointTag { });
                EntityManager.AddComponentData(instance, new WaypointMovementComponent { CurrentTargetIndex = 0 });

                EntityManager.AddComponentData(instance, new AttackRadiusComponent { Value = GameData.Instance.agentScanRadius });

                EntityManager.AddSharedComponentData(instance, new EnemyTeamComponent());

                int maxHealth = m_Random.NextInt(GameData.Instance.agentInitialHealthMin, GameData.Instance.agentInitialHealthMax);
                EntityManager.AddComponentData(instance, new HealthComponent { Health = maxHealth, MaxHealth = maxHealth });
                EntityManager.AddComponentData(instance, new AttackStatusComponent { });
            }

            StatDisplay.Instance.AgentCount += batchCount;
        }
    }
}
