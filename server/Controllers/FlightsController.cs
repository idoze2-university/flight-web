using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightRadar.Models;
using System.Net;
using Newtonsoft.Json.Linq;

namespace FlightRadar.Controllers
{
    [Route("api/Flights")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private readonly DataContext _context;

        public FlightsController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Flights
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Flight>>> GetFlight()
        {
            _context.Flights.RemoveRange(_context.Flights);
            await _context.SaveChangesAsync();

            var query = HttpContext.Request.Query;

            if (!isValidQuery(query))
                return NotFound();

            GetLocalFlights(query["relative_to"].ToString());

            if (query.Count == 2) {
                GetExternalFlights(query["relative_to"].ToString());
            }

            return await _context.Flights.ToListAsync();
        }


        // DELETE: api/Flights/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<FlightPlan>> DeleteFlight(string id)
        {
            string id_string = id.ToString().Substring(1);
            long key = Convert.ToInt64(Convert.ToDouble(id_string));
            var flight = await _context.FlightPlans.FindAsync(key);
            if (flight == null)
            {
                return NotFound();
            }

            _context.FlightPlans.Remove(flight);
            await _context.SaveChangesAsync();

            return flight;
        }

        private bool FlightExists(long id)
        {
            return _context.FlightPlans.Any(e => e.id == id);
        }

        public bool isValidQuery(IQueryCollection queries)
        {
            if(queries.Count == 0)
            {
                return false;
            }
            if(queries.Count == 1)
            {
                foreach(var key in queries.Keys)
                {
                    if(key != "relative_to")
                    {
                        return false;
                    }
                    string time = queries["relative_to"].ToString();
                    try
                    {
                        Tools.FormatDateTime(time);
                    }
                    catch(Exception e)
                    {
                        if (e != null)
                        {
                            return false;
                        }
                    }
                }
            }
            if (queries.Count == 2)
            {
                int i = 1;
                foreach (var key in queries.Keys)
                {
                    if (i == 1 && key != "relative_to")
                    {
                        return false;
                    }
                    if(i == 2 && key != "sync_all")
                    {
                        return false;
                    }
                    i++;
                }
                string time = queries["relative_to"].ToString();
                try
                {
                    Tools.FormatDateTime(time);
                }
                catch (Exception e)
                {
                    if (e != null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public async void GetLocalFlights(string time)
        {
            foreach (FlightPlan plan in _context.FlightPlans)
            {
                if (plan.isOnAirAtTime(time))
                {
                    Tuple<double, double> coordinates;
                    coordinates = plan.getCoordinatesAtTime(time);
                    Flight flight = new Flight();
                    flight.BuildLocal(plan, coordinates, time);
                    _context.Flights.Add(flight);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async void GetExternalFlights(string time) {
            List <JObject> jsons = new List <JObject> ();
            var servers = _context.servers;
            string content = "";
            foreach (Server server in servers)
            {
                string site = server.ServerURL.ToLower();
                site = site.Replace("http://", "").Replace("www", "");
                var client = new WebClient();
                string request = "http://" + site + "/api/Flights?relative_to=";
                request += time;
                content = content + client.DownloadString(request);
            }
            content = content.Replace("\n", "").Replace("\t", "").Replace("\r", "");
            jsons = Tools.Strings_to_Jsons(content);
            long external_flight_id = 1;
            bool build_succesfull;
            foreach(var json in jsons)
            {
                Flight flight = new Flight();
                build_succesfull = flight.BuildExternal(json, external_flight_id);
                if (build_succesfull)
                {
                    _context.Flights.Add(flight);
                    await _context.SaveChangesAsync();
                    external_flight_id++;
                }
            }
        }
    }
}
