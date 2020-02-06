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
    public class SpawnAllySystem : ComponentSystem
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
            World.GetOrCreateSystem<SpawnPointManagementSystem>().GetRandomSpawnPointPosition(out float3 firstWaypointPos);

            Entity prefab = GameData.Instance.AllyEntityPrefab;
            Entity instance = EntityManager.Instantiate(prefab);
            EntityManager.SetName(instance, "Ally");
            EntityManager.AddComponentData(instance, new AgentTag { });

            EntityManager.SetComponentData(instance, new Translation { Value = firstWaypointPos });
            EntityManager.AddComponentData(instance, new MoveSpeedComponent { Value = GameData.Instance.agentMoveSpeed });

            EntityManager.AddComponentData(instance, new AttackRadiusComponent { Value = GameData.Instance.agentScanRadius });

            EntityManager.AddSharedComponentData(instance, new AllyTeamComponent());
        }
    } }