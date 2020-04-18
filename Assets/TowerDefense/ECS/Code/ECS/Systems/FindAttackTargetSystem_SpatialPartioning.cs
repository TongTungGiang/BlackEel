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

    public class FindAttackTargetSystem : SystemBase
    {
        EntityQuery m_AgentQuery;

        protected override void OnUpdate()
        {
            int agentCount = m_AgentQuery.CalculateEntityCount();
            NativeMultiHashMap<int, Entity> cellMap = new NativeMultiHashMap<int, Entity>(agentCount, Allocator.TempJob);

            // Allocate entities into cell hashmap
            var parallelCellMap = cellMap.AsParallelWriter();
            var cellAllocateJobHandle = Entities.WithAll<AgentTag>()
                .ForEach((Entity e, in Translation t) =>
                {
                    Cell c = Cell.FromPos(t.Value);
                    var hash = (int)math.hash(new int2(c.x, c.y));
                    parallelCellMap.Add(hash, e);
                })
                .ScheduleParallel(Dependency);

            // Find targets in allocated cells


            var disposeJobHandle = cellMap.Dispose(cellAllocateJobHandle);

            Dependency = disposeJobHandle;
        }

        protected override void OnCreate()
        {
            m_AgentQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<AgentTag>(), },
            });

            RequireForUpdate(m_AgentQuery);
        }
    }
}
