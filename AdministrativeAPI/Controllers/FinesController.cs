using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AdministrativeAPI.Controllers
{
    [Route("api/[controller]")]
    public class FinesController : Controller
    {
        static List<Fine> fines = new List<Fine>();

        // GET api/values
        [HttpGet]
        public IEnumerable<Fine> Get()
        {
            return fines;
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody]Fine value)
        {
            value.Amount = 100; //needs some clever algo
            fines.Add(value);
            return Json(new
            {
                Amount = value.Amount
            }); 
        }
    }

    public class Fine
    {
        public string LicensePlate { get; set; }
        public DateTime Registered { get; set; }
        public double SpeedInMetersPerSecond { get; set; }

        public double Amount { get; set; }
    }
}
