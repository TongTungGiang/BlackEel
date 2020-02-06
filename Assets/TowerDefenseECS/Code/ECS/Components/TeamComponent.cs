using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace BE.ECS
{
    [Serializable]
    public struct EnemyTeamComponent : ISharedComponentData { }

    [Serializable]
    public struct AllyTeamComponent : ISharedComponentData { }
}