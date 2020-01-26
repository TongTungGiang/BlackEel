using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BE.ECS.Signal
{
    // Signal is just a tag component
    public struct Signal : IComponentData
    {
        public static void Dispatch<T>(EntityManager entityManager, T signalComponent) where T : struct, IComponentData
        {
            Entity entity = entityManager.CreateEntity();
            entityManager.AddComponentData(entity, new Signal());
            entityManager.AddComponentData(entity, signalComponent);
        }

        public static void Dispatch<T>(EntityCommandBuffer buffer, T signalComponent) where T : struct, IComponentData
        {
            Entity entity = buffer.CreateEntity();
            buffer.AddComponent(entity, new Signal());
            buffer.AddComponent(entity, signalComponent);
        }
    }
}
