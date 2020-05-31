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
    [Route("api/servers")]
    [ApiController]
    public class serversController : ControllerBase
    {
        private readonly DataContext _context;

        public serversController(DataContext context)
        {
            _context = context;
        }

        // GET: api/servers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Server>>> Getservers()
        {
            return await _context.servers.ToListAsync();
        }

        // POST: api/servers
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Server>> Postserver(Server server)
        {
            if (server.ServerURL != null)
            {
                _context.servers.Add(server);
            }
            await _context.SaveChangesAsync();

            return server;
        }

        // DELETE: api/servers/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Server>> Deleteserver(long id)
        {
            var server = await _context.servers.FindAsync(id);
            if (server == null)
            {
                return NotFound();
            }

            _context.servers.Remove(server);
            await _context.SaveChangesAsync();

            return server;
        }

        private bool serverExists(long id)
        {
            return _context.servers.Any(e => e.id == id);
        }
    }
}
