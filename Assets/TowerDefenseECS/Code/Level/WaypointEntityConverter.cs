using BE.ECS;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BE
{
    public class WaypointEntityConverter : MonoBehaviour
    {
        // Start is called before the first frame update
        void Awake()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                float3 waypointPos = transform.GetChild(i).transform.position;

                EntityManager entityManager = World.Active.EntityManager;
                Entity entity = entityManager.CreateEntity();
                entityManager.AddComponentData(entity, new WaypointComponent { Index = transform.childCount, Position = waypointPos });
                entityManager.SetName(entity, string.Format("Waypoint {0}", i));
            }
        }
    }

}
