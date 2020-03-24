using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace BE.ECS
{
    [Serializable]
    public struct AllySpawnPointIndexComponent : ISharedComponentData
    {
        public int Value;
    }

    [Serializable]
    public struct AllySpawnPointCountComponent : IComponentData
    {
        public int Value;
    }
}
