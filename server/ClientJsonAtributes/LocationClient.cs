using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightRadar.Models
{
    // Class which contains JSON fields for client of type location
    public class LocationClient
    {
        public double longitude { get; set; }
        public double latitude { get; set; }
        public string date_time { get; set; }
    }
}
