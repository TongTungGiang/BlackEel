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

        protected override void OnCreate()
        {
            m_SpawnRate = GameData.Instance.spawnRate;

            m_LastSpawn = Time.time - m_SpawnRate * 2;
        }

        protected override void OnUpdate()
        {
            if (Time.time - m_LastSpawn < m_SpawnRate)
            {
                return;
            }

            m_LastSpawn = Time.time;
            World.GetOrCreateSystem<WaypointManagementSystem>().GetWaypointPosition(0, out float3 firstWaypointPos);

            Entity prefab = GameData.Instance.EnemyEntityPrefab;
            Entity instance = EntityManager.Instantiate(prefab);
            EntityManager.SetName(instance, "Enemy");
            EntityManager.AddComponentData(instance, new AgentTag { });

            EntityManager.SetComponentData(instance, new Translation { Value = firstWaypointPos });
            EntityManager.AddComponentData(instance, new MoveSpeedComponent { Value = 10 });

            EntityManager.AddComponentData(instance, new FollowWaypointTag { });
            EntityManager.AddComponentData(instance, new WaypointMovementComponent { CurrentTargetIndex = 0 });

            EntityManager.AddComponentData(instance, new AttackRadiusComponent { Value = 10 });

            EntityManager.AddSharedComponentData(instance, new TeamComponent { IsEnemy = true });
        }
    }
}
