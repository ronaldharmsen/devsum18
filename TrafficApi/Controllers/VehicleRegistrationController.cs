using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using VehicleActor.Interfaces;

namespace TrafficApi.Controllers
{
    [Route("api/[controller]")]
    public class VehicleRegistrationController : Controller
    {
        public const string LicensePlates = "licenseplates";
        private readonly IReliableStateManager stateManager;
        private const VehicleRegistration NoRegistration = null;
        public VehicleRegistrationController(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            var result = new List<string>();

            try
            {
                var dict = await stateManager.GetOrAddAsync<IReliableDictionary<string, VehicleRegistration>>(LicensePlates);

                using (ITransaction tx = stateManager.CreateTransaction())
                {
                    var enu = await dict.CreateEnumerableAsync(tx);
                    using (Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<string, VehicleRegistration>> enumerator = enu.GetAsyncEnumerator())
                    {
                        while (await enumerator.MoveNextAsync(CancellationToken.None))
                        {
                            var value = enumerator.Current;
                            result.Add(value.Key);
                        }
                    }
                }
            }
            catch (TimeoutException)
            {
                return null;
            }

            return result;
        }

        [HttpPost]
        public async Task Post([FromBody]VehicleRegistration registration)
        {
            try
            {
                var dict = await stateManager.GetOrAddAsync<IReliableDictionary<string, VehicleRegistration>>(LicensePlates);

                using (ITransaction tx = stateManager.CreateTransaction())
                {
                    await dict.AddOrUpdateAsync(tx, registration.LicensePlate, NoRegistration, (k, v) => v ?? NoRegistration);
                    await tx.CommitAsync();
                }

                ActorId actorId = new ActorId(registration.LicensePlate);
                IVehicleActor proxy = ActorProxy.Create<IVehicleActor>(
                    actorId, new Uri("fabric:/TrafficServiceFabric/VehicleActorService"));

                await proxy.SetCountAsync(0, CancellationToken.None);
            }
            catch (TimeoutException)
            {
                await Task.Delay(100);
                await Post(registration); //possible infiniteloop
            }
        }
    }
}
