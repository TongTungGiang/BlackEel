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
#pragma warning disable CS0618 // Type or member is obsolete
            GameData.Instance.RegisterEnemyEntityPrefab(GameObjectConversionUtility.ConvertGameObjectHierarchy(enemy, World.DefaultGameObjectInjectionWorld));
            GameData.Instance.RegisterAllyEntityPrefab(GameObjectConversionUtility.ConvertGameObjectHierarchy(ally, World.DefaultGameObjectInjectionWorld));
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
