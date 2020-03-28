using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

namespace BE.ECS
{
    [UpdateAfter(typeof(DamageSystem))]
    public class DieSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<AgentTag, HealthComponent>().ForEach<HealthComponent>(ProcessDie);
        }

        private void ProcessDie(Entity e, ref HealthComponent health)
        {
            if (health.Health < 0)
                EntityManager.DestroyEntity(e);
        }
    }
}