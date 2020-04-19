using BE.ECS;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace BE
{
    public class WaypointEntityConverter : MonoBehaviour
    {
        // Start is called before the first frame update
        void Awake()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            for (int i = 0; i < transform.childCount; i++)
            {
                float3 waypointPos = transform.GetChild(i).transform.position;

                Entity entity = entityManager.CreateEntity();
                entityManager.AddComponentData(entity, new WaypointIndexComponent { Value = i });
                entityManager.AddComponentData(entity, new Translation { Value = waypointPos });
                entityManager.SetName(entity, string.Format("Waypoint {0}", i));
            }

            Entity total = entityManager.CreateEntity();
            entityManager.AddComponentData(total, new WaypointCountComponent { Value = transform.childCount });
        }
    }

}
