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

        // Start is called before the first frame update
        void Start()
        {
            EntityManager manager = World.Active.EntityManager;
            Entity cubePrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(cube, World.Active);

            Entity cubeInstance = manager.Instantiate(cubePrefab);
            manager.SetName(cubeInstance, "Move Forward Cube");
            manager.SetComponentData(cubeInstance, new Translation { Value = new float3(100, 0, 100) });

            manager.AddComponent<MoveForwardComponent>(cubeInstance);
            manager.SetComponentData(cubeInstance, new MoveForwardComponent { Target = new float3(0, 0, 0) });

            manager.AddComponent<MoveSpeedComponent>(cubeInstance);
            manager.SetComponentData(cubeInstance, new MoveSpeedComponent { Value = 10 });
        }
    }
}
