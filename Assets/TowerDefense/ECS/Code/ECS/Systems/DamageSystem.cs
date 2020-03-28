using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;

namespace BE.ECS
{
    public class DamageSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<DamageComponent>().ForEach<DamageComponent>(ProcessDamage);
        }

        private void ProcessDamage(Entity e, ref DamageComponent damage)
        {
            if (EntityManager.Exists(damage.Target))
            {
                var health = EntityManager.GetComponentData<HealthComponent>(damage.Target);
                health.Health -= damage.Amount;
                EntityManager.SetComponentData(damage.Target, health);
            }

            EntityManager.DestroyEntity(e);
        }
    }
}
