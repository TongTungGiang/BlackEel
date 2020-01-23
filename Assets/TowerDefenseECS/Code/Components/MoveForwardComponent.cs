using System;
using Unity.Entities;
using Unity.Mathematics;

namespace BE.ECS
{
    [Serializable]
    public struct MoveForwardComponent : IComponentData
    {
        public float3 Target;
    }
}