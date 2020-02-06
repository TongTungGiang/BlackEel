using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace BE.ECS
{
    public class FindAttackTargetSystem : JobComponentSystem
    {
        private EntityQueryDesc m_QueryDesc;
        private EntityQuery allyQuery;
        private EntityQuery enemyQuery;

        protected override void OnCreate()
        {
            m_QueryDesc = new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<AgentTag>(), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<TeamComponent>() },
            };

            allyQuery = GetEntityQuery(m_QueryDesc);
            allyQuery.SetFilter(new TeamComponent { IsEnemy = false });

            enemyQuery = GetEntityQuery(m_QueryDesc);
            enemyQuery.SetFilter(new TeamComponent { IsEnemy = true });
        }

        struct RangeQueryJob : IJobChunk
        {
            [DeallocateOnJobCompletion, ReadOnly]
            public NativeArray<Translation> TranslationToTestAgainst;

            [ReadOnly] public ArchetypeChunkComponentType<AttackRadiusComponent> AttackRadiusType;
            [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var chunkAttackRadius = chunk.GetNativeArray(AttackRadiusType);
                var chunkTranslations = chunk.GetNativeArray(TranslationType);

                for (int i = 0; i < chunk.Count; i++)
                {
                    AttackRadiusComponent radius = chunkAttackRadius[i];
                    Translation pos = chunkTranslations[i];

                    for (int j = 0; j < TranslationToTestAgainst.Length; j++)
                    {
                        Translation targetPos = TranslationToTestAgainst[j];

                        if (CheckInRange(targetPos.Value, pos.Value, radius.Value * radius.Value))
                        {
                            UnityEngine.Debug.LogFormat("{0} finds {1} in range", i, j);
                        }
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {

            var radiusType = GetArchetypeChunkComponentType<AttackRadiusComponent>(true);
            var translationType = GetArchetypeChunkComponentType<Translation>(true);

            var jobEvA = new RangeQueryJob()
            {
                TranslationToTestAgainst = allyQuery.ToComponentDataArray<Translation>(Allocator.TempJob),
                AttackRadiusType = radiusType,
                TranslationType = translationType
            };
            JobHandle jobHandleEvA = jobEvA.Schedule(enemyQuery, inputDependencies);

            var jobAvE = new RangeQueryJob()
            {
                TranslationToTestAgainst = enemyQuery.ToComponentDataArray<Translation>(Allocator.TempJob),
                AttackRadiusType = radiusType,
                TranslationType = translationType
            };
            JobHandle jobHandleAvE = jobAvE.Schedule(allyQuery, jobHandleEvA);

            return jobHandleAvE;
        }

        private static bool CheckInRange(float3 target, float3 center, float radiusSqr)
        {
            float3 delta = target - center;
            float distanceSquare = delta.x * delta.x + delta.z * delta.z;

            return distanceSquare <= radiusSqr;
        }
    }
}
