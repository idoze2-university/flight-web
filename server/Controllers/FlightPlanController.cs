using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightRadar.Models;

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
            string id_string = id.ToString().Substring(1);
            if(id_string == "")
            {
                return NotFound();
            }
            long key = Convert.ToInt64(Convert.ToDouble(id_string));
            var flightPlan = await _context.FlightPlans.FindAsync(key);

            if (flightPlan == null)
            {
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
    }
}
