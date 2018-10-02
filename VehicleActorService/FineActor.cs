using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
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

            //yikes, just to fix the demo :-)
            var addr = "http://desktop-euqb4qu:5000/api/fines"; 
            ActorEventSource.Current.ActorMessage(this, $"Sending fine registration to {addr}");

            var api = new HttpClient();
            await api.PostAsync(addr,
                new StringContent(
                    JsonConvert.SerializeObject(new
                    {
                        LicensePlate = licenseplate,
                        Registered = timestamp,
                        SpeedInMetersPerSecond = speedInMetersPerSecond
                    }), Encoding.UTF8, "application/json"
                ));            
        }
    }
}
