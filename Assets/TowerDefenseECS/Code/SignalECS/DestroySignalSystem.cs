using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace BE.ECS.Signal
{
    public class DestroySignalsSystem : ComponentSystem
    {
        private EntityQuery query;

        protected override void OnCreate()
        {
            this.query = GetEntityQuery(typeof(Signal));
        }

        protected override void OnUpdate()
        {
            this.EntityManager.DestroyEntity(this.query);
        }
    }
}
