using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Frontend.Pages
{
    public class IndexModel : PageModel
    {
        public IEnumerable<string> LicensePlates { get; private set; }
        public async Task OnGetAsync()
        {
            var addr = await ResolveApiAddress();

            var api = new HttpClient();
            var vehicleJson = await api.GetStringAsync($"{addr}/api/vehicleregistration");

            LicensePlates = JsonConvert.DeserializeObject<List<string>>(vehicleJson);
        }

        private async Task<string> ResolveApiAddress()
        {
            
            ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();            
            ResolvedServicePartition partition =
                await resolver.ResolveAsync(
                    new Uri("fabric:/TrafficServiceFabric/TrafficApi"), 
                    new ServicePartitionKey(0), CancellationToken.None);

            ResolvedServiceEndpoint endpoint = partition.GetEndpoint();

            JObject addresses = JObject.Parse(endpoint.Address);
            string address = (string)addresses["Endpoints"].First();
            return address;
        }

    }
}
