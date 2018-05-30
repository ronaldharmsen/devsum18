using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VehicleActor;

namespace VehicleActorService
{
    public interface IFineProcessingActor : IActor
    {
        Task RegisterMaximumSpeedViolation(DateTime timestamp, string licenseplate, double speedInMetersPerSecond); 
    }

    public class FineActor : Actor, IFineProcessingActor
    {
        public FineActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public async Task RegisterMaximumSpeedViolation(DateTime timestamp, string licenseplate, double speedInMetersPerSecond)
        {
            ActorEventSource.Current.ActorMessage(this, $"Registering fine for {licenseplate}, registered at {timestamp} for driving {speedInMetersPerSecond * 3.6}km/hr");
            await Task.FromResult(0);
        }
    }
}
