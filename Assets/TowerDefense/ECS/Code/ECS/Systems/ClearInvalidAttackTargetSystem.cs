using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace BE.ECS
{
    [UpdateBefore(typeof(MoveToAttackTargetSystem)), UpdateAfter(typeof(DieSystem))]
    public class ClearInvalidAttackTargetSystem : ComponentSystem
    {
        EntityQuery m_Query;

        protected override void OnCreate()
        {
            m_Query = GetEntityQuery(ComponentType.ReadOnly(typeof(AttackTargetComponent)));
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<AttackTargetComponent>().ForEach<AttackTargetComponent>(CheckValidity);
        }

        private void CheckValidity(Entity e, ref AttackTargetComponent a)
        {
            if (EntityManager.Exists(a.Target) == false)
            {
                EntityManager.RemoveComponent<AttackTargetComponent>(e);

                if (EntityManager.HasComponent<EnemyTeamComponent>(e))
                {
                    EntityManager.AddComponent<FollowWaypointTag>(e);
                }
            }
        }
    }
}
