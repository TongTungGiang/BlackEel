using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace BE.ECS
{
    [Serializable]
    public struct WaypointIndexComponent : ISharedComponentData
    {
        public int Value;
    }

    public struct WaypointCountComponent : ISharedComponentData
    {
        public int Value;
    }
}
