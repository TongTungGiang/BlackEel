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
using Unity.Collections.LowLevel.Unsafe;

namespace BE.ECS
{

    public class FindAttackTargetSystem : SystemBase
    {
        EntityQuery m_AllyQuery;
        EntityQuery m_EnemyQuery;
        EntityCommandBufferSystem m_Barrier;


        [BurstCompile]
        private struct FindTargetInCell : IJobParallelFor
        {
            [ReadOnly] public NativeMultiHashMap<int, FindAttackTargetDataTuple> InstigatorMap;
            [ReadOnly] public NativeMultiHashMap<int, FindAttackTargetDataTuple> TargetMap;
            [ReadOnly] public NativeArray<int> OverlappingCells;
            [WriteOnly] public EntityCommandBuffer.Concurrent CommandBuffer;

            [NativeSetThreadIndex] private int m_ThreadIndex;

            public void Execute(int index)
            {
                int cellHash = OverlappingCells[index];

                if (InstigatorMap.TryGetFirstValue(cellHash, out FindAttackTargetDataTuple currentInstigatorTuple, out NativeMultiHashMapIterator<int> instigatorIterator) == false)
                    return;

                do
                {
                    if (TargetMap.TryGetFirstValue(cellHash, out FindAttackTargetDataTuple currentTargetTuple, out NativeMultiHashMapIterator<int> targetIterator) == false)
                        return;

                    do
                    {
                        if (CheckInRange(currentTargetTuple.Position, currentInstigatorTuple.Position, currentInstigatorTuple.RangeSqr))
                        {
                            var attackTargetComponent = new AttackTargetComponent { Target = currentTargetTuple.Entity };
                            CommandBuffer.AddComponent(m_ThreadIndex, currentInstigatorTuple.Entity, attackTargetComponent);

                            //CommandBuffer.AddComponent(chunkIndex, EntityToTestAgainst[j], new OccupiedAsTargetTag());
                            break;
                        }
                    } while (InstigatorMap.TryGetNextValue(out currentTargetTuple, ref targetIterator) == true);
                } while (InstigatorMap.TryGetNextValue(out currentInstigatorTuple, ref instigatorIterator) == true);
            }

            private bool CheckInRange(float3 target, float3 center, float radiusSqr)
            {
                float3 delta = target - center;
                float distanceSquare = delta.x * delta.x + delta.z * delta.z;

                return distanceSquare <= radiusSqr;
            }
        }

        private struct FindAttackTargetDataTuple
        {
            public Entity Entity;
            public float3 Position;
            public float RangeSqr;
        }

        protected override void OnUpdate()
        {
            // Step 1.
            // Allocate entities into cell hashmap
            int allyCount = m_AllyQuery.CalculateEntityCount();
            var allyMap = new NativeMultiHashMap<int, FindAttackTargetDataTuple>(allyCount, Allocator.TempJob);
            var allyParallelMap = allyMap.AsParallelWriter();
            var allyJobHandle = Entities.WithAll<AgentTag, AllyTeamComponent>()
                .WithNone<AttackTargetComponent>()
                .WithName("CellAllocatingJob_Ally")
                .ForEach((Entity e, in Translation t, in AttackRadiusComponent a) =>
                {
                    allyParallelMap.Add(GetCellHash(in t), new FindAttackTargetDataTuple { Entity = e, Position = t.Value, RangeSqr = a.Value * a.Value });
                })
                .ScheduleParallel(Dependency);

            int enemyCount = m_EnemyQuery.CalculateEntityCount();
            var enemyMap = new NativeMultiHashMap<int, FindAttackTargetDataTuple>(enemyCount, Allocator.TempJob);
            var enemyParallelMap = enemyMap.AsParallelWriter();
            var enemyJobHandle = Entities.WithAll<AgentTag, EnemyTeamComponent>()
                .WithNone<AttackTargetComponent>()
                .WithName("CellAllocatingJob_Enemy")
                .ForEach((Entity e, in Translation t, in AttackRadiusComponent a) =>
                {
                    enemyParallelMap.Add(GetCellHash(in t), new FindAttackTargetDataTuple { Entity = e, Position = t.Value, RangeSqr = a.Value * a.Value });
                })
                .ScheduleParallel(Dependency);

            var allocateCellJobHandle = JobHandle.CombineDependencies(allyJobHandle, enemyJobHandle);
            allocateCellJobHandle.Complete();
            Dependency = allocateCellJobHandle;

            // Step 2.
            // Get the list of overlapping cells
            // Note: using a quick O(n) algorithm to get overlapping elements of two array 
            // because probably we don't have more than 100 cells 
            // so no need to waste time on scheduling and waiting.
            // A simple single-threaded piece of code might do the job
            var allyCellHashes = allyMap.GetKeyArray(Allocator.TempJob);
            var enemyCellHashes = enemyMap.GetKeyArray(Allocator.TempJob);
            var existingAllyHashes = new NativeHashSet<int>(allyCellHashes.Length + enemyCellHashes.Length, Allocator.TempJob);
            for (int i = 0; i < allyCellHashes.Length; i++)
            {
                existingAllyHashes.TryAdd(allyCellHashes[i]);
            }

            var overlappingHashes = new NativeHashSet<int>(allyCellHashes.Length + enemyCellHashes.Length, Allocator.TempJob);
            for (int i = 0; i < enemyCellHashes.Length; i++)
            {
                if (existingAllyHashes.Contains(enemyCellHashes[i]))
                    overlappingHashes.TryAdd(enemyCellHashes[i]);
            }

            var overlappingHashesArray = overlappingHashes.ToNativeArray();
            existingAllyHashes.Dispose();

            // Step 3.
            // Perform target detection (O(n^2)) in allocated cells
            if (overlappingHashes.Length > 0)
            {
                var commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent();
                var findTargetJob_Ally = new FindTargetInCell
                {
                    InstigatorMap = allyMap,
                    TargetMap = enemyMap,
                    OverlappingCells = overlappingHashesArray,
                    CommandBuffer = commandBuffer,
                };

                var findTargetJob_Enemy = new FindTargetInCell
                {
                    InstigatorMap = enemyMap,
                    TargetMap = allyMap,
                    OverlappingCells = overlappingHashesArray,
                    CommandBuffer = commandBuffer,
                };

                var findTargetJobHandle_Ally = findTargetJob_Ally.Schedule(overlappingHashes.Length, 64, Dependency);
                findTargetJobHandle_Ally.Complete();
                var findTargetJobHandle_Enemy = findTargetJob_Enemy.Schedule(overlappingHashes.Length, 64, Dependency);
                findTargetJobHandle_Enemy.Complete();

                var findTargetJobHandle = JobHandle.CombineDependencies(Dependency, findTargetJobHandle_Ally);
                findTargetJobHandle = JobHandle.CombineDependencies(Dependency, findTargetJobHandle_Enemy);
                Dependency = findTargetJobHandle;
            }

            // Step 4.
            // Clean up
            var uniqueCellHashesArrayDisposeJobHandle = overlappingHashesArray.Dispose(Dependency);
            var uniqueCellHashesDisposeJobHandle = overlappingHashes.Dispose(Dependency);
            var allyCellHashesDisposeJobHandle = allyCellHashes.Dispose(Dependency);
            var enemyCellHashesDisposeJobHandle = enemyCellHashes.Dispose(Dependency);
            var allyMapDisposeJobHandle = allyMap.Dispose(Dependency);
            var enemyMapDisposeJobHandle = enemyMap.Dispose(Dependency);

            var disposeJobHandle = Dependency;
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, uniqueCellHashesArrayDisposeJobHandle);
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, uniqueCellHashesDisposeJobHandle);
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, allyCellHashesDisposeJobHandle);
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, enemyCellHashesDisposeJobHandle);
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, allyMapDisposeJobHandle);
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, enemyMapDisposeJobHandle);
            Dependency = disposeJobHandle;
        }

        protected override void OnCreate()
        {
            m_AllyQuery = GetAllAgent<AllyTeamComponent>();
            m_EnemyQuery = GetAllAgent<EnemyTeamComponent>();

            RequireForUpdate(m_AllyQuery);
            RequireForUpdate(m_EnemyQuery);

            m_Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        EntityQuery GetAllAgent<TeamTag>() where TeamTag : struct
        {
            return GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<AgentTag>(), ComponentType.ReadOnly<TeamTag>(), },
            });
        }

        private static int GetCellHash(in Translation t)
        {
            Cell c = new Cell { x = (int)t.Value.x / CELL_SIZE, y = (int)t.Value.z / CELL_SIZE };
            return (int)math.hash(new int2(c.x, c.y));
        }

        private const int CELL_SIZE = 50;
    }
}
