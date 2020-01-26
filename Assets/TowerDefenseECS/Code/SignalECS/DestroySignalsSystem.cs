using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE.ECS.Signal
{
    public class DestroySignalsSystem : ComponentSystem
    {
        private EntityQuery query;

        protected override void OnCreate()
        {
            this.query = GetEntityQuery(typeof(Signal), typeof(SignalFramePassed));
        }

        protected override void OnUpdate()
        {
            this.EntityManager.DestroyEntity(this.query);
        }
    }
}
