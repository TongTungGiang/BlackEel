using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace BE.ECS
{
    [Serializable]
    public struct AttackRadiusComponent : IComponentData
    {
        public float Value;
    }
}

