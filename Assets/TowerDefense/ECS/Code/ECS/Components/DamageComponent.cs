using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace BE.ECS
{
    [Serializable]
    public struct DamageComponent : IComponentData
    {
        public Entity Source;
        public Entity Target;
        public float Amount;
    }
}