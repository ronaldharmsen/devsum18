using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Net.Http;
using System.Text;
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
        [BindProperty]
        public string LicensePlate { get; set; }
        [BindProperty]
        public DateTime Timestamp { get; set; }

        public IEnumerable<string> LicensePlates { get; private set; }
        public async Task OnGetAsync()
        {
            var addr = await ResolveApiAddress();

            var api = new HttpClient();
            var vehicleJson = await api.GetStringAsync($"{addr}/api/vehicleregistration");

            LicensePlates = JsonConvert.DeserializeObject<List<string>>(vehicleJson);
        }

        public async Task OnPostAsync()
        {
            var addr = await ResolveApiAddress();

            var api = new HttpClient();
            await api.PostAsync($"{addr}/api/vehicleregistration",
                new StringContent(
                    JsonConvert.SerializeObject(new
                    {
                        LicensePlate,
                        Timestamp
                    }), Encoding.UTF8, "application/json"
                ));

            await OnGetAsync(); //make sure list of licenseplates is filled
        }

        private async Task<string> ResolveApiAddress()
        {
            //Warning: Very simplistic/optimistic implementation
            //This demo uses only 1 partition. Other stateful services might have more..
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
