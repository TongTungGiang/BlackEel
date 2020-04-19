using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.CodeGeneratedJobForEach;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using JacksonDunstan.NativeCollections;

namespace BE.ECS
{

    public class FindAttackTargetSystem : SystemBase
    {
        EntityQuery m_AllyQuery;
        EntityQuery m_EnemyQuery;

        struct AddUniqueCellJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<int> Input;
            [WriteOnly] public NativeHashSet<int>.ParallelWriter OutputWriter;

            public void Execute(int index)
            {
                OutputWriter.TryAdd(Input[index]);
            }
        }

        struct FindTargetInCell : IJobParallelFor
        {
            [ReadOnly] public NativeMultiHashMap<int, Entity> InstigatorMap;
            [ReadOnly] public NativeMultiHashMap<int, Entity> TargetMap;
            [ReadOnly] public NativeArray<int> UniqueCells;

            public void Execute(int index)
            {
                int cellHash = UniqueCells[index];

                NativeMultiHashMapIterator<int> InstigatorIterator;
                NativeMultiHashMapIterator<int> TargetIterator;
            }
        }

        protected override void OnUpdate()
        {
            // Step 1.
            // Allocate entities into cell hashmap
            int allyCount = m_AllyQuery.CalculateEntityCount();
            var allyMap = new NativeMultiHashMap<int, Entity>(allyCount, Allocator.TempJob);
            var allyParallelMap = allyMap.AsParallelWriter();
            var allyJobHandle = Entities.WithAll<AgentTag, AllyTeamComponent>()
                .WithName("CellAllocatingJob_Ally")
                .ForEach((Entity e, in Translation t) =>
                {
                    Cell c = Cell.FromPos(t.Value);
                    var hash = (int)math.hash(new int2(c.x, c.y));
                    allyParallelMap.Add(hash, e);
                })
                .ScheduleParallel(Dependency);

            int enemyCount = m_EnemyQuery.CalculateEntityCount();
            var enemyMap = new NativeMultiHashMap<int, Entity>(enemyCount, Allocator.TempJob);
            var enemyParallelMap = enemyMap.AsParallelWriter();
            var enemyJobHandle = Entities.WithAll<AgentTag, EnemyTeamComponent>()
                .WithName("CellAllocatingJob_Enemy")
                .ForEach((Entity e, in Translation t) =>
                {
                    Cell c = Cell.FromPos(t.Value);
                    var hash = (int)math.hash(new int2(c.x, c.y));
                    enemyParallelMap.Add(hash, e);
                })
                .ScheduleParallel(Dependency);

            var allocateCellJobHandle = JobHandle.CombineDependencies(allyJobHandle, enemyJobHandle);
            allocateCellJobHandle.Complete();
            Dependency = allocateCellJobHandle;

            // Step 2.
            // Get list of unique cells
            var allyCellHashes = allyMap.GetKeyArray(Allocator.TempJob);
            var enemyCellHashes = enemyMap.GetKeyArray(Allocator.TempJob);
            var uniqueCellHashes = new NativeHashSet<int>(allyCellHashes.Length + enemyCellHashes.Length, Allocator.TempJob);
            var uniqueCellHashesWriter = uniqueCellHashes.AsParallelWriter();
            var writeToUniqueCellJob_Ally = new AddUniqueCellJob { Input = allyCellHashes, OutputWriter = uniqueCellHashesWriter };
            var writeToUniqueCellJob_Enemy = new AddUniqueCellJob { Input = enemyCellHashes, OutputWriter = uniqueCellHashesWriter };

            var writeToUniqueCellJobHandle_Ally = writeToUniqueCellJob_Ally.Schedule(allyCellHashes.Length, 64);
            writeToUniqueCellJobHandle_Ally.Complete();
            var writeToUniqueCellJobHandle_Enemy = writeToUniqueCellJob_Enemy.Schedule(enemyCellHashes.Length, 64);
            writeToUniqueCellJobHandle_Enemy.Complete();

            var uniqueCellHashesArray = uniqueCellHashes.ToNativeArray();

            var writeToUniqueCellJobHandle = JobHandle.CombineDependencies(Dependency, writeToUniqueCellJobHandle_Ally);
            writeToUniqueCellJobHandle = JobHandle.CombineDependencies(Dependency, writeToUniqueCellJobHandle_Enemy);
            Dependency = writeToUniqueCellJobHandle;

            // Step 3.
            // Perform target detection (O(n^2)) in allocated cells


            // Step 4.
            // Clean up
            var uniqueCellHashesDisposeArrayJobHandle = uniqueCellHashesArray.Dispose(Dependency);
            var uniqueCellHashesDisposeJobHandle = uniqueCellHashes.Dispose(Dependency);
            var allyCellHashesDisposeJobHandle = allyCellHashes.Dispose(Dependency);
            var enemyCellHashesDisposeJobHandle = enemyCellHashes.Dispose(Dependency);
            var allyMapDisposeJobHandle = allyMap.Dispose(Dependency);
            var enemyMapDisposeJobHandle = enemyMap.Dispose(Dependency);

            var disposeJobHandle = Dependency;
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, uniqueCellHashesDisposeArrayJobHandle);
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, uniqueCellHashesDisposeJobHandle);
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, allyCellHashesDisposeJobHandle);
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, enemyCellHashesDisposeJobHandle);
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, allyMapDisposeJobHandle);
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, enemyMapDisposeJobHandle);
            Dependency = disposeJobHandle;
        }

        protected override void OnCreate()
        {
            m_AllyQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<AgentTag>(), ComponentType.ReadOnly<AllyTeamComponent>(), },
            });

            m_EnemyQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<AgentTag>(), ComponentType.ReadOnly<EnemyTeamComponent>(), },
            });

            RequireForUpdate(m_AllyQuery);
            RequireForUpdate(m_EnemyQuery);
        }

        private static bool CheckInRange(float3 target, float3 center, float radiusSqr)
        {
            float3 delta = target - center;
            float distanceSquare = delta.x * delta.x + delta.z * delta.z;

            return distanceSquare <= radiusSqr;
        }
    }
}
