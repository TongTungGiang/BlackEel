using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace BE.ECS.Signal
{
    [UpdateAfter(typeof(DestroySignalsSystem))]
    public class SignalFramePassedSystem : ComponentSystem
    {
        private EntityQuery query;

        protected override void OnCreate()
        {
            this.query = GetEntityQuery(typeof(Signal), ComponentType.Exclude<SignalFramePassedTag>());
        }

        protected override void OnUpdate()
        {
            this.PostUpdateCommands.AddComponent(this.query, typeof(SignalFramePassedTag));
        }
    }
}
