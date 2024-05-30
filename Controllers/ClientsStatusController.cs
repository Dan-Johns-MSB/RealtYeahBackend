using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealtYeahBackend.Entities;

namespace RealtYeahBackend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [RequireHttps]
    public class ClientsStatusController : ControllerBase
    {
        private readonly RealtyeahContext context;

        public ClientsStatusController(RealtyeahContext context)
        {
            this.context = context;
        }

        // GET: api/ClientsStatus
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientsStatus>>> GetClientsStatuses()
        {
            if (context.ClientsStatuses == null)
            {
                return NotFound();
            }
            return await context.ClientsStatuses.ToListAsync();
        }

        // GET: api/ClientsStatus/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ClientsStatus>> GetClientsStatus(string id)
        {
            if (context.ClientsStatuses == null)
            {
                return NotFound();
            }
            var clientsStatus = await context.ClientsStatuses.FindAsync(id);

            if (clientsStatus == null)
            {
                return NotFound();
            }

            return clientsStatus;
        }

        // PUT: api/ClientsStatus/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClientsStatus(string id, ClientsStatus clientsStatus)
        {
            if (id != clientsStatus.Status)
            {
                return BadRequest();
            }

            context.Entry(clientsStatus).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientsStatusExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ClientsStatus
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ClientsStatus>> PostClientsStatus(ClientsStatus clientsStatus)
        {
            if (context.ClientsStatuses == null)
            {
                return Problem("Entity set 'RealtyeahContext.ClientsStatuses'  is null.");
            }
            context.ClientsStatuses.Add(clientsStatus);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ClientsStatusExists(clientsStatus.Status))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetClientsStatus", new { id = clientsStatus.Status }, clientsStatus);
        }

        // DELETE: api/ClientsStatus/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClientsStatus(string id)
        {
            if (context.ClientsStatuses == null)
            {
                return NotFound();
            }
            var clientsStatus = await context.ClientsStatuses.FindAsync(id);
            if (clientsStatus == null)
            {
                return NotFound();
            }

            context.ClientsStatuses.Remove(clientsStatus);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool ClientsStatusExists(string id)
        {
            return (context.ClientsStatuses?.Any(e => e.Status == id)).GetValueOrDefault();
        }     
    }
}
