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
    [Route("api/FlightPlan")]
    [ApiController]
    public class FlightPlanController : ControllerBase
    {
        private readonly DataContext _context;

        public FlightPlanController(DataContext context)
        {
            _context = context;
        }

        // GET: api/FlightPlan/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FlightPlanClient>> GetFlightPlan(string id)
        {
            string local_id_string = id.ToString().Substring(1);
            long key = 0;
            if(local_id_string == "")
            {
                return NotFound();
            }
            try
            {
                key = Convert.ToInt64(Convert.ToDouble(local_id_string));
            }catch(Exception e)
            {
                if (e != null)
                {
                    //Do nothing
                }
            }
            var flightPlan = await _context.FlightPlans.FindAsync(key);

            //Check if internal flight exists
            if (flightPlan == null)
            {
                flightPlan = GetExternalFlightPlan(id);
                if (flightPlan == null)
                    return NotFound();
            }
            FlightPlanClient clientData = new FlightPlanClient();
            clientData.Build(flightPlan);
            return clientData;
        }

        // POST: api/FlightPlan
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<FlightPlanClient>> PostFlightPlan(FlightPlan flightPlan)
        {
            bool succeeded = flightPlan.set(_context);
            if (succeeded)
            {
                _context.FlightPlans.Add(flightPlan);
                await _context.SaveChangesAsync();
            }
            FlightPlanClient clientData = new FlightPlanClient();
            clientData.Build(flightPlan);
            return clientData;
        }

        private bool FlightPlanExists(long id)
        {
            return _context.FlightPlans.Any(e => e.id == id);
        }

        public FlightPlan GetExternalFlightPlan(string id) {
            FlightPlan plan = null;
            var servers = _context.servers;
            string content = "";
            foreach (Server server in servers)
            {
                string site = server.ServerURL.ToLower();
                site = site.Replace("http://", "").Replace("www", "");
                var client = new WebClient();
                string request = "http://" + site + "/api/FlightPlan/" + id;
                content = client.DownloadString(request);
                content = content.Replace("\n", "").Replace("\t", "").Replace("\r", "");
                var json_plan_object = JObject.Parse(content);
                FlightPlan flightPlan = new FlightPlan();
                try
                {
                    flightPlan.BuildExternal(json_plan_object,id);
                }catch(Exception e)
                {
                    if (e != null)
                    {
                        continue;
                    }
                }
                //reaches here if build is succesfull
                plan = flightPlan;
                break;
            }
            return plan;
        }
    }
}
