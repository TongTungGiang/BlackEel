using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace BE.ECS.Signal
{
    /// <summary>
    /// Should use [UpdateBefore(typeof(DestroySignalsSystem))]
    /// </summary>
    public abstract class SignalHandlerSystem<T> : ComponentSystem where T : struct, IComponentData
    {
        private EntityQuery signalQuery;
        private SignalHandler<T> signalHandler;

        protected override void OnCreate()
        {
            this.signalQuery = GetEntityQuery(typeof(Signal), typeof(T));
            this.signalHandler = new SignalHandler<T>(this, this.signalQuery);
            this.signalHandler.AddListener(OnDispatch);
        }

        protected abstract void OnDispatch(Entity entity, T signalComponent);

        protected override void OnUpdate()
        {
            this.signalHandler.Update();
        }
    }
}
