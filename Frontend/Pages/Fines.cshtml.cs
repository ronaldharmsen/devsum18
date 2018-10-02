using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.ServiceFabric.Services.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Frontend.Pages
{
    public class Fine
    {
        public string LicensePlate { get; set; }
        public DateTime Registered { get; set; }
        public double SpeedInMetersPerSecond { get; set; }

        public double Amount { get; set; }
    }

    public class FinesModel : PageModel
    {        
        public List<Fine> Fines { get; private set; }
        public string Address { get; set; }
        public async Task OnGet()
        {
            Address = "http://localhost:5000";
            try
            {
                var api = new HttpClient();
                var vehicleJson = await api.GetStringAsync($"{Address}/api/fines");

                Fines = JsonConvert.DeserializeObject<List<Fine>>(vehicleJson);
            }
            catch
            {
                Fines = new List<Fine>();
            }
        }

        private async Task<string> ResolveApiAddress()
        {
            //Warning: Very simplistic/optimistic implementation
            //This demo uses only 1 partition. Other stateful services might have more..
            ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();
            ResolvedServicePartition partition =
                await resolver.ResolveAsync(
                    new Uri("fabric:/TrafficServiceFabric/AdministrativeApi"),
                    new ServicePartitionKey(0), CancellationToken.None);

            ResolvedServiceEndpoint endpoint = partition.GetEndpoint();

            JObject addresses = JObject.Parse(endpoint.Address);
            string address = (string)addresses["Endpoints"].First();
            return address;
        }

    }
}
