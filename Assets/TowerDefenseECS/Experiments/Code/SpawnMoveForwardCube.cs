using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace BE.ECS
{
    public class SpawnMoveForwardCube : MonoBehaviour
    {
        public GameObject cube;
        public int number;
        public float spawnRate = 1;

        private float m_LastSpawn;

        private EntityManager m_Manager;
        private Entity m_Prefab;

        private float3 m_FirstPosition;

        // Start is called before the first frame update
        void Start()
        {
            m_Manager = World.Active.EntityManager;
            m_Prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(cube, World.Active);

            GameData.Instance.RegisterAgentEntityPrefab(m_Prefab);
        }

        private void Update()
        {
            if (Time.time - m_LastSpawn < spawnRate)
            {
                return;
            }

            m_LastSpawn = Time.time;
            SpawnEntity();
        }

        private void SpawnEntity()
        {
            Entity cubeInstance = m_Manager.Instantiate(m_Prefab);
            m_Manager.SetName(cubeInstance, "Move Forward Cube");
            m_Manager.SetComponentData(cubeInstance, new Translation { Value = new float3(r, 0, r) });

            m_Manager.AddComponentData(cubeInstance, new MoveSpeedComponent { Value = 10 });

            m_Manager.AddComponentData(cubeInstance, new FollowWaypointTag { });
        }

        float r { get { return UnityEngine.Random.Range(-100, 100); } }
    }
}
