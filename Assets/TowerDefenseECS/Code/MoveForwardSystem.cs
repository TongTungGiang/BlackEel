using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace BE.ECS
{
    public class MoveForwardSystem : ComponentSystem
    {
        private EntityQuery m_EntityQuery;

        protected override void OnCreate()
        {
            m_EntityQuery = GetEntityQuery(typeof(Translation), typeof(MoveForwardComponent), typeof(MoveSpeedComponent));
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<Translation, MoveForwardComponent, MoveSpeedComponent>().
                ForEach<Translation, MoveForwardComponent, MoveSpeedComponent>(MoveEntity);
        }

        private void MoveEntity(Entity e, ref Translation t, ref MoveForwardComponent mf, ref MoveSpeedComponent ms)
        {
            float3 position = t.Value;
            float3 direction = mf.Target - position;
            position += math.normalize( direction) * ms.Value * Time.deltaTime;

            t.Value = position;
        }
    }
}