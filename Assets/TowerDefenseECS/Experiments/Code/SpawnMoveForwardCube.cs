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

        // Start is called before the first frame update
        void Start()
        {
            m_Manager = World.Active.EntityManager;
            m_Prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(cube, World.Active);
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

            m_Manager.AddComponent<MoveForwardComponent>(cubeInstance);
            m_Manager.SetComponentData(cubeInstance, new MoveForwardComponent { Target = new float3(r, 0, r) });

            m_Manager.AddComponent<MoveSpeedComponent>(cubeInstance);
            m_Manager.SetComponentData(cubeInstance, new MoveSpeedComponent { Value = 10 });
        }

        float r { get { return UnityEngine.Random.Range(-100, 100); } }
    }
}
