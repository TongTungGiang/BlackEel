using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using BE.ECS;

namespace BE
{
    public class AllySpawnPointEntityConverter : MonoBehaviour
    {
        // Start is called before the first frame update
        private void Awake()
        {
            EntityManager entityManager = World.Active.EntityManager;

            for (int i = 0; i < transform.childCount; i++)
            {
                float3 waypointPos = transform.GetChild(i).transform.position;

                Entity entity = entityManager.CreateEntity();
                entityManager.AddSharedComponentData(entity, new AllySpawnPointIndexComponent { Value = i });
                entityManager.AddComponentData(entity, new Translation { Value = waypointPos });
                entityManager.SetName(entity, string.Format("SpawnPoint {0}", i));
            }

            Entity total = entityManager.CreateEntity();
            entityManager.AddComponentData(total, new AllySpawnPointCountComponent { Value = transform.childCount });
        }
    }
}
