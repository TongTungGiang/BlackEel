using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace BE.ECS
{
    [Serializable]
    public struct AttackTargetComponent : IComponentData
    {
        public Entity Target;
    }

    [Serializable]
    public struct AttackStatusComponent : IComponentData
    {
        public float LastAttack;
    }
}
