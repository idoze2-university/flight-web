using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
namespace FlightRadar.Models
{
    public class Flight
    {
        public long id { get; set; }
        public string flight_id { get; set; }       
        public double longitude { get; set; }
        public double latitude { get; set; }
        public int passengers { get; set; }
        public string Company_name { get; set; }
        public string date_time { get; set; }
        public bool is_external { get; set; }

        //Input: Flight plan , coordinates and time
        //Output: None
        //Builds Flight object from a given plan
        public void BuildLocal(FlightPlan plan, Tuple<double,double> coordinates,string time)
        {
            id = plan.id;
            flight_id = plan.flight_id;
            longitude = coordinates.Item1;
            latitude = coordinates.Item2;
            passengers = plan.passengers;
            Company_name = plan.Company_name;
            date_time = time;
            is_external = false;
        }

        //Input: Json flight object, id key
        //Output: None
        //Creates flight object from given JSON object
        public void BuildExternal(JObject jObject, long external_id)
        {
            id = external_id;
            flight_id = jObject["flight_id"].ToString();
            longitude = Convert.ToDouble(jObject["longitude"].ToString());
            latitude = Convert.ToDouble(jObject["latitude"].ToString());
            passengers = Convert.ToInt32(jObject["passengers"].ToString());
            Company_name = jObject["Company_name"].ToString();
            date_time = jObject["date_time"].ToString();
            is_external = true;
        }
    }
}
