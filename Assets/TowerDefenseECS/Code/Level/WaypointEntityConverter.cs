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
        private float3[] waypoints;

        // Start is called before the first frame update
        void Awake()
        {
            waypoints = new NativeArray<float3>(transform.childCount, Allocator.Persistent);
            for (int i = 0; i < transform.childCount; i++)
            {
                waypoints[i] = transform.GetChild(i).transform.position;
            }

            //var manager = World.Active.EntityManager;
            //Entity waypointHolder = manager.CreateEntity();
            //manager.AddComponentData(waypointHolder, new WaypointListComponent { Waypoints = waypoints });
            //manager.SetName(waypointHolder, "Waypoints");
        }
    }
}
