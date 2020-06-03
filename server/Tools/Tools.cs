using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlightRadar.Models;
using Newtonsoft.Json.Linq;

namespace FlightRadar
{
    public class Tools
    {
        //Input: 2 segments
        //Output: The distance between them
        public static double getDistance(SegmentClient s1, SegmentClient s2)
        {
            double a = (s2.latitude - s1.latitude) * (s2.latitude - s1.latitude);
            double b = (s2.longitude - s1.longitude) * (s2.longitude - s1.longitude);
            double c = a + b;
            return Math.Sqrt(c);
        }

        //Input: Time in Datetime format
        //Output: Time in string format (yyyy-mm-ddThh-mm-ssZ)
        public static string ReformatDateTime(DateTime myDate)
        {
            string date = PadZero(myDate.Year.ToString()) + "-";
            date += PadZero(myDate.Month.ToString()) + "-";
            date += PadZero(myDate.Day.ToString()) + "T";
            date += PadZero(myDate.Hour.ToString()) + ":" + 
                PadZero(myDate.Minute.ToString()) + ":" + 
                PadZero(myDate.Second.ToString()) + "Z";
            return date;
        }

        public static string PadZero(string n)
        {
            string res = n;
            if (res.Length < 2)
            {
                res = "0" + res;
            }
            return res;
        }

        //Input: Time in string format (yyyy-mm-ddThh-mm-ssZ)
        //Output: Time in Datetime format
        public static DateTime FormatDateTime(string time)
        {
            var x = DateTime.ParseExact
                (time, "yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture)
                .ToUniversalTime();
            return x;
        }

        //Input: Strings
        //Output: The input strings in json format
        public static List<JObject> Strings_to_Jsons(string data)
        {
            data = data.Replace("[", "").Replace("]", "");
            data = data.Replace("\n", "").Replace("\r", "").Replace("\t", "");
            string json_string_obj = "";
            List<string> json_string_objects = new List<string>();
            bool start = false;
            bool end = false;
            foreach (char c in data)
            {
                if (c == '{')
                {
                    start = true;
                    end = false;
                }
                else if (c == '}')
                {
                    start = false;
                    end = true;
                }

                if (start)
                {
                    json_string_obj += c;
                }
                else if (end)
                {
                    json_string_obj += c;
                    if (json_string_obj != "{}")
                        json_string_objects.Add(json_string_obj);
                    json_string_obj = "";
                    end = false;
                }
            }
            List<JObject> jObjects = new List<JObject>();
            JObject jObject;
            foreach (string string_obj in json_string_objects)
            {
                jObject = JObject.Parse(string_obj);
                jObjects.Add(jObject);
            }
            return jObjects;
        }
    }
}
