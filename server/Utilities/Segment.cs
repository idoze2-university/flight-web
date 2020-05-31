using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightRadar.Models
{
    public class Segment
    {
        public double id { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public double timespan_seconds { get; set; }
    }
}
