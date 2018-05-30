using VehicleActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Actors.Client;
using VehicleActorService;

namespace VehicleActor
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class VehicleActor : Actor, IVehicleActor
    {
        public VehicleActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, $"VehicleActor {this.GetActorId().GetStringId()} activated.");
            return Task.FromResult(0);
        }

        const string FirstRegistration = "firstregistration";
        private const double speedLimit = 100; //km per hr
        private readonly double speedLimitInMetersPerSecond = (speedLimit * 1000) / 3600;

        public async Task AddRegistrationByCameraAsync(DateTime timestamp)
        {
            ConditionalValue<DateTime> first = await this.StateManager.TryGetStateAsync<DateTime>(FirstRegistration);

            if (first.HasValue)
            {
                ActorEventSource.Current.ActorMessage(this, $"Car with licenseplate {this.Id.GetStringId()} is passing second camera");
                var seconds = (timestamp - first.Value).TotalSeconds;
                var metersPerSecond = 100 / seconds;
                if (metersPerSecond > speedLimitInMetersPerSecond)
                {
                    ActorEventSource.Current.ActorMessage(this, $"Car with licenseplate {this.Id.GetStringId()} is speeding!");
                    var fineProcessor = ActorProxy.Create<IFineProcessingActor>(
                        new ActorId("fines"), new Uri("fabric:/TrafficServiceFabric/FineProcessingActorService"));

                    await fineProcessor.RegisterMaximumSpeedViolation(timestamp, Id.GetStringId(), metersPerSecond);
                }
                else
                {
                    ActorEventSource.Current.ActorMessage(this, $"Car with licenseplate {this.Id.GetStringId()} is driving within speedlimit");
                }
                await this.StateManager.TryRemoveStateAsync(FirstRegistration);
            }
            else
            {
                ActorEventSource.Current.ActorMessage(this, $"Car with licenseplate {this.Id.GetStringId()} is passing first camera");
                await this.StateManager.TryAddStateAsync(FirstRegistration, timestamp);
            }
        }
    }
}
