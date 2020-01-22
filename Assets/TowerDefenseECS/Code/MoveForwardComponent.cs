using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace BE.ECS
{
    public struct MoveForwardComponent : IComponentData
    {
        public float3 Target;
    }

    public struct MoveSpeedComponent : IComponentData
    {
        public float Value;
    }
}