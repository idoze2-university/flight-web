using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FlightRadar.Models
{
    public class FlightPlan
    {
        Random rnd = new Random();

        public long id { get; set; }
        public string flight_id { get; set; }
        public int passengers { get; set; }
        public string company_name { get; set; }

        public Location initial_location { get; set; } //Not saved in DB

        public double initial_location_longitude { get; set; }
        public double initial_location_latitude { get; set; }
        public string initial_location_date_time { get; set; }

        public Location final_location { get; set; } //Not saved in DB

        public double final_location_longitude { get; set; }
        public double final_location_latitude { get; set; }
        public string final_location_date_time { get; set; }

        public IEnumerable<Segment> segments { get; set; } //Not saved in DB
        public string segments_string { get; set; }

        //Input: None
        //Output : Total flight time of plan
        public double TotalFlightTime()
        {
            double total_time_seconds = 0;
            foreach (Segment segment in this.segments)
            {
                total_time_seconds += segment.timespan_seconds;
            }
            return total_time_seconds;
        }

        //Input: Database context
        //Output: True if flightid generated succesfully
        private bool GenerateFlightId(DataContext context)
        {
            long flight_id;
            bool has_flight_id;
            var sw = new Stopwatch();
            sw.Start();
            while (true && sw.ElapsedMilliseconds<1000)
            {
                flight_id = rnd.Next(100000,99999999);
                has_flight_id = context.FlightPlans.Any(e => e.id == flight_id);
                if (has_flight_id == false)
                {
                    this.flight_id = "F" + flight_id.ToString();
                    this.id = Convert.ToInt64(Convert.ToDouble(flight_id.ToString().Substring(0)));
                    return true;
                }
            }
            sw.Stop();
            return false;
        }

        //Input: None
        //Output: None
        //Creates Location instance of final location
        private void GetFinalLocation()
        {
            Location final_location = new Location();
            DateTime myDate = Tools.FormatDateTime(this.initial_location.date_time);
            final_location.longitude = this.segments.Last().longitude;
            final_location.latitude = this.segments.Last().latitude;
            myDate = myDate.AddSeconds(this.TotalFlightTime());
            final_location.date_time = Tools.ReformatDateTime(myDate);
            this.final_location = final_location;
        }

        //Input: DB context
        //Output: if creation of a plan succeded or not
        //Function checks the plan recieved, return true if plan is ok and false if not
        public bool set(DataContext context)
        {
            if (!isValidPlan())
                return false;
            bool succeded = this.GenerateFlightId(context);
            if (!succeded)
                return false;
            this.GetFinalLocation();
            initial_location_longitude = initial_location.longitude;
            initial_location_latitude = initial_location.latitude;
            initial_location_date_time = initial_location.date_time;
            initial_location = null;
            final_location_longitude = final_location.longitude;
            final_location_latitude = final_location.latitude;
            final_location_date_time = final_location.date_time;
            final_location = null;
            segments_string = "[";
            bool first = true;
            foreach(Segment e in segments)
            {
                if (!first)
                    segments_string += ",";
                segments_string += "{" + e.longitude.ToString() + "," + 
                    e.latitude.ToString() + "," +
                    e.timespan_seconds.ToString() + "}";
                first = false;
            }
            segments_string += "]";
            segments = null;
            return true;
        }

        //Input: Time in string format (yyyy-mm-ddThh-mm-ssZ)
        //Output: True if the flight is currently on air, false if not
        public bool isOnAirAtTime(string time)
        {
            DateTime start = Tools.FormatDateTime(initial_location_date_time);
            DateTime current_time = Tools.FormatDateTime(time);
            DateTime end = Tools.FormatDateTime(final_location_date_time);
            return isBetweenTimes(start, current_time, end);
        }

        //Input: 3 times in DateTime format
        //Output: Checks if middle time is between start time and end time
        public bool isBetweenTimes(DateTime start, DateTime time, DateTime end)
        {
            return start <= time && time <= end;
        }

        //Input: Time in string format (yyyy-mm-ddThh-mm-ssZ)
        //Output: Tuple (longitude,latitude), at time given
        public Tuple<double, double> getCoordinatesAtTime(string time)
        {
            IEnumerable<SegmentClient> segments = SegmentClient.ParseSegments(segments_string);
            Tuple<double, double> coordinates;
            DateTime start_time = Tools.FormatDateTime(initial_location_date_time);
            DateTime current_time = Tools.FormatDateTime(time);
            double time_passed = (current_time - start_time).TotalSeconds;
            if (time_passed <= 0)
            {
                coordinates = new Tuple<double, double>(initial_location_longitude, initial_location_latitude);
                return coordinates;
            }
            SegmentClient start_segment = new SegmentClient();
            start_segment.longitude = initial_location_longitude;
            start_segment.latitude = initial_location_latitude;
            SegmentClient next_segment = new SegmentClient();
            bool hasNoSegments = true;

            foreach (SegmentClient segment in segments)
            {
                hasNoSegments = false;
                next_segment = segment;
                if (next_segment.timespan_seconds == time_passed)
                {
                    coordinates = new Tuple<double, double>(next_segment.longitude, next_segment.latitude);
                    return coordinates;
                }
                else if (time_passed > next_segment.timespan_seconds)
                {
                    time_passed -= segment.timespan_seconds;
                    start_segment = next_segment;
                }
                else
                {
                    double d = Tools.getDistance(next_segment, start_segment); //Positive
                    if(d == 0)
                    {
                        coordinates = new Tuple<double, double>
                            (start_segment.longitude, start_segment.latitude);
                        return coordinates;
                    }
                    double speed = d / next_segment.timespan_seconds;
                    double k = speed * time_passed;
                    double l = d - k;

                    double longitude = (l * start_segment.longitude + 
                        k * next_segment.longitude) / d;

                    double latitude = (l * start_segment.latitude + 
                        k * next_segment.latitude) / d;
                    coordinates = new Tuple<double, double>(longitude, latitude);
                    return coordinates;
                }
            }
            if (hasNoSegments)
            {
                coordinates = new Tuple<double, double>
                    (initial_location_longitude, initial_location_latitude);
            }
            else
            {
                coordinates = new Tuple<double, double>
                    (start_segment.longitude, start_segment.latitude);
            }
            return coordinates;
        }

        //Input: Checks if a plan is valid, AFTER deserializng JSON
        //Output: True if valid, false if not
        public bool isValidPlan()
        {
            try
            {
                Tools.FormatDateTime(initial_location.date_time);
            }catch(Exception e)
            {
                if (e != null)
                {
                    return false;
                }
            }
            double current_longitude = initial_location.longitude;
            double current_latitude = initial_location.latitude;
            foreach(Segment segment in segments)
            {
                double next_longitude = segment.longitude;
                double next_latitude = segment.latitude;
                double time_span = segment.timespan_seconds;
                if(time_span == 0 && 
                    (current_latitude != next_latitude || current_longitude != next_longitude))
                {
                    return false;
                }
                current_longitude = next_longitude;
                current_latitude = next_latitude;
            }
            return true;
        }
        public void BuildExternal(JObject json_plan_object,string flight_id)
        {
            int seg_field = 0;
            id = 1;
            bool first = true;
            this.flight_id = flight_id;
            passengers = Convert.ToInt32(json_plan_object["passengers"].ToString());
            company_name = json_plan_object["company_name"].ToString();
            JToken initial_location = json_plan_object["initial_location"];
            foreach(JProperty prop in initial_location)
            {
                if(prop.Name == "longitude") 
                    initial_location_longitude = Convert.ToDouble(prop.Value.ToString());                 
                else if(prop.Name == "latitude") 
                    initial_location_latitude = Convert.ToDouble(prop.Value.ToString()); 
                else if(prop.Name == "date_time") 
                    initial_location_date_time = prop.Value.ToString(); 
            }
            segments_string = "[";
            JToken segments = json_plan_object["segments"];
            foreach(JObject segment in segments)
            {
                if (!first)                
                    segments_string += ",";                
                segments_string += "{";
                foreach(KeyValuePair<string,JToken> entry in segment)
                {
                    segments_string += entry.Value.ToString();
                    if (seg_field != 2)                    
                        segments_string += ",";                    
                    seg_field++;
                }
                segments_string += "}";
                seg_field = 0;
                first = false;
            }
            segments_string += "]";
        }
    }
}
