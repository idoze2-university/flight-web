using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightRadar.Models
{
    // Class which contains JSON fields for client of type FlightPlan
    public class FlightPlanClient
    {
        public string flight_id { get; set; }
        public int passengers { get; set; }
        public string company_name { get; set; }
        public LocationClient initial_location { get; set; }
        public IEnumerable<SegmentClient> segments { get; set; } 

        // Input : FlightPlan of type server (with DB id and other fields)
        //Output : None
        // Function initilazies fields of instance
        public void Build(FlightPlan plan)
        {
            LocationClient locationClient = new LocationClient();
            flight_id = plan.flight_id;
            passengers = plan.passengers;
            company_name = plan.company_name;
            locationClient.longitude = plan.initial_location_longitude;
            locationClient.latitude = plan.initial_location_latitude;
            locationClient.date_time = plan.initial_location_date_time;
            initial_location = locationClient;
            segments = SegmentClient.ParseSegments(plan.segments_string);
        }
    }
}

