using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace BE.ECS
{
    [UpdateBefore(typeof(MoveToAttackTargetSystem)), UpdateAfter(typeof(DieSystem))]
    public class ClearInvalidAttackTargetSystem : ComponentSystem
    {
        EntityQuery m_Query;

        protected override void OnCreate()
        {
            m_Query = GetEntityQuery(ComponentType.ReadOnly(typeof(AttackTargetComponent)));
            m_Random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(0, 1000));
            m_NoiseValue = GameData.Instance.spawnPositionNoise;
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<AttackTargetComponent>().ForEach<AttackTargetComponent>(CheckValidity);
        }

        private void CheckValidity(Entity e, ref AttackTargetComponent a)
        {
            if (EntityManager.Exists(a.Target) == false)
            {
                EntityManager.RemoveComponent<AttackTargetComponent>(e);

                if (EntityManager.HasComponent<EnemyTeamComponent>(e))
                {
                    EntityManager.AddComponent<FollowWaypointTag>(e);
                    int currentTarget = EntityManager.GetComponentData<WaypointMovementComponent>(e).CurrentTargetIndex;
                    var noise = new float3(m_Random.NextFloat(-m_NoiseValue, m_NoiseValue), 0, m_Random.NextFloat(-m_NoiseValue, m_NoiseValue));
                    var moveForward = new MoveForwardComponent { Target = AllWaypointTranslation[currentTarget].Value + noise };
                    EntityManager.AddComponentData(e, moveForward);
                }
            }
        }
        protected override void OnDestroy()
        {
            m_AllWaypointTranslation.Dispose();
        }

        private Unity.Mathematics.Random m_Random;
        private float m_NoiseValue;
        private NativeArray<Translation> m_AllWaypointTranslation;
        private bool m_WaypointTranslationInitialized;
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
    }
}
