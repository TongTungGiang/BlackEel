﻿using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace BE.ECS
{
    [Serializable]
    public struct WaypointIndexComponent : IComponentData
    {
        public int Value;
    }

    public struct WaypointCountComponent : IComponentData
    {
        public int Value;
    }
}
