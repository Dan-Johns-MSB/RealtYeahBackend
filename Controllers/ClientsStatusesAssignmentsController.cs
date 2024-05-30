using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealtYeahBackend.Entities;
using System.IO;

namespace RealtYeahBackend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [RequireHttps]
    public class ClientsStatusesAssignmentsController : ControllerBase
    {
        private readonly RealtyeahContext context;

        public ClientsStatusesAssignmentsController(RealtyeahContext context)
        {
            this.context = context;
        }

        // GET: api/ClientsStatusesAssignments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientsStatusesAssignment>>> GetClientsStatusesAssignments()
        {
            if (context.ClientsStatusesAssignments == null)
            {
                return NotFound();
            }
            return await context.ClientsStatusesAssignments.ToListAsync();
        }

        [HttpGet("{client}/{status}/{operation}")]
        public async Task<ActionResult<ClientsStatusesAssignment>> GetClientsStatusesAssignment(int client, string status, int operation)
        {
            if (context.ClientsStatusesAssignments == null)
            {
                return NotFound();
            }
            var clientsStatusesAssignment = await context.ClientsStatusesAssignments.Where(assignment => assignment.ClientId == client
                                                                                                      && assignment.Status.Equals(status)
                                                                                                      && assignment.OperationId == operation).FirstOrDefaultAsync();

            if (clientsStatusesAssignment == null)
            {
                return NotFound();
            }

            return clientsStatusesAssignment;
        }
        
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{client}/{status}/{operation}")]
        public async Task<IActionResult> PutClientsStatusesAssignment(int client, string status, int operation, ClientsStatusesAssignment clientsStatusesAssignment)
        {    
            if (client != clientsStatusesAssignment.ClientId
                || !status.Equals(clientsStatusesAssignment.Status)
                || operation != clientsStatusesAssignment.OperationId)
            {
                return BadRequest();
            }           

            context.Entry(clientsStatusesAssignment).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientsStatusesAssignmentExists(client, status, operation))
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

        // POST: api/ClientsStatusesAssignments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ClientsStatusesAssignment>> PostClientsStatusesAssignment(ClientsStatusesAssignment clientsStatusesAssignment)
        {
            if (context.ClientsStatusesAssignments == null)
            {
                return Problem("Entity set 'ClientsStatusesAssignments' is null.");
            }
            context.ClientsStatusesAssignments.Add(clientsStatusesAssignment);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ClientsStatusesAssignmentExists(clientsStatusesAssignment.ClientId, 
                                                    clientsStatusesAssignment.Status, 
                                                    clientsStatusesAssignment.OperationId))
                {
                    return Conflict("Conflicting record presence in database");
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetClientsStatusesAssignment", new { id = clientsStatusesAssignment.ClientId }, clientsStatusesAssignment);
        }

        [HttpDelete("{client}/{status}/{operation}")]
        public async Task<IActionResult> DeleteClientsStatusesAssignment(int client, string status, int operation)
        {
            if (context.ClientsStatusesAssignments == null)
            {
                return NotFound();
            }
            var clientsStatusesAssignment = await context.ClientsStatusesAssignments.Where(assignment => assignment.ClientId == client
                                                                                                      && assignment.Status.Equals(status)
                                                                                                      && assignment.OperationId == operation).FirstOrDefaultAsync();
            if (clientsStatusesAssignment == null)
            {
                return NotFound();
            }

            context.ClientsStatusesAssignments.Remove(clientsStatusesAssignment);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool ClientsStatusesAssignmentExists(int client, string status, int operation)
        {
            return (context.ClientsStatusesAssignments?.Any(e => e.ClientId == client
                                                              && e.Status.Equals(status)
                                                              && e.OperationId == operation)).GetValueOrDefault();
        }
    }
}
