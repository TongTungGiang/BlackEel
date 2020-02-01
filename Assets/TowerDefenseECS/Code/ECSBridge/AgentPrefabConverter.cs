using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace BE.ECS
{
    public class AgentPrefabConverter : MonoBehaviour
    {
        public GameObject cube;

        // Start is called before the first frame update
        void Start()
        {
            Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(cube, World.Active);

            GameData.Instance.RegisterAgentEntityPrefab(prefab);
        }
    }
}
