using VehicleActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using System.Threading;
using System.Threading.Tasks;

namespace VehicleActor
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class VerhicleActor : Actor, IVehicleActor
    {
        public VerhicleActor(ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        {
        }

        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, $"VehicleActor {this.GetActorId().GetStringId()} activated.");
            return this.StateManager.TryAddStateAsync("count", 0);
        }

        Task<int> IVehicleActor.GetCountAsync(CancellationToken cancellationToken)
        {
            return this.StateManager.GetStateAsync<int>("count", cancellationToken);
        }

        Task IVehicleActor.SetCountAsync(int count, CancellationToken cancellationToken)
        {
            // Requests are not guaranteed to be processed in order nor at most once.
            // The update function here verifies that the incoming count is greater than the current count to preserve order.
            return this.StateManager.AddOrUpdateStateAsync("count", count, (key, value) => count > value ? count : value, cancellationToken);
        }
    }
}
