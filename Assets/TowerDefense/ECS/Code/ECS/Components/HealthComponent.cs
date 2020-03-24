using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace BE.ECS
{
    [Serializable]
    public struct HealthComponent : IComponentData
    {
        public float MaxHealth;
        public float Health;
    }
}
