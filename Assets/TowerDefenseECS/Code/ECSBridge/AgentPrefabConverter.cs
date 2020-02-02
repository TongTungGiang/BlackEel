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
        public GameObject enemy;
        public GameObject ally;

        // Start is called before the first frame update
        void Start()
        {
            GameData.Instance.RegisterEnemyEntityPrefab(GameObjectConversionUtility.ConvertGameObjectHierarchy(enemy, World.Active));
            GameData.Instance.RegisterEnemyEntityPrefab(GameObjectConversionUtility.ConvertGameObjectHierarchy(ally, World.Active));
        }
    }
}
