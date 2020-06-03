using System;
using System.Collections.Generic;

namespace FlightRadar.Models
{
    // Class which contains JSON fields for client of type segment
    public class SegmentClient
    {
        public double longitude { get; set; }
        public double latitude { get; set; }
        public double timespan_seconds { get; set; }

        //Input : Segments string
        //Output : SegmentClient list
        public static IEnumerable<SegmentClient> ParseSegments(string segments_string)
        {
            int i = 0;
            List<SegmentClient> segmentsClient = new List<SegmentClient>();
            SegmentClient segmentClient = null;
            if(segments_string == null || segments_string == "")
                return segmentsClient;
            segments_string = segments_string.Replace("[", "").Replace("]", "");
            segments_string = segments_string.Replace("{", "").Replace("}", "");
            string[] words = segments_string.Split(',');
            if (words.Length == 1)
                return segmentsClient;
            foreach (string word in words)
            {
                if (i % 3 == 0) //Longitude (start of new segment)
                {
                    segmentClient = new SegmentClient();
                    segmentClient.longitude = Convert.ToDouble(word);
                }
                else if (i % 3 == 1) //Latitude
                    segmentClient.latitude = Convert.ToDouble(word);
                else if (i % 3 == 2) //time_span (end of segment)
                {
                    segmentClient.timespan_seconds = Convert.ToDouble(word);
                    segmentsClient.Add(segmentClient);
                }
                i++;
            }
            return segmentsClient;
        }
    }
}
