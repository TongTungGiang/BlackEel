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

    public class FindAttackTargetSystem : ComponentSystem
    {
        EntityQuery m_AgentQuery;

        protected override void OnUpdate()
        {
            int agentCount = m_AgentQuery.CalculateEntityCount();
            NativeMultiHashMap<Cell, Entity> spatialPartition = new NativeMultiHashMap<Cell, Entity>(agentCount, Allocator.TempJob);

            // Allocate entities into cell hashmap
            Entities.With(m_AgentQuery).ForEach<Translation>((Entity e, ref Translation t) => 
            {
                Cell c = Cell.FromPos(t.Value);
                spatialPartition.Add(c, e);
            });

            // Find targets in allocated cells

            spatialPartition.Dispose();            
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
