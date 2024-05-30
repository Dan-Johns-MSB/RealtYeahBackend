using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealtYeahBackend.Entities;
using RealtYeahBackend.Models;

namespace RealtYeahBackend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [RequireHttps]
    public class ClientsController : ControllerBase
    {
        private readonly RealtyeahContext context;

        public ClientsController(RealtyeahContext context)
        {
            this.context = context;
        }

        // GET: api/Clients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            if (context.Clients == null)
            {
                return NotFound();
            }

            return await context.Clients.ToListAsync();
        }

        // GET: api/Clients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            if (context.Clients == null)
            {
                return NotFound();
            }
            var client = await context.Clients.FindAsync(id);

            if (client == null)
            {
                return NotFound();
            }

            return client;
        }

        // PUT: api/Clients/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClient(int id, Client client)
        {
            if (id != client.ClientId)
            {
                return BadRequest();
            }

            context.Entry(client).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(id))
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

        // POST: api/Clients
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Client>> PostClient(Client client)
        {
            if (context.Clients == null)
            {
                return Problem("Entity set 'Clients' is null.");
            }
            context.Clients.Add(client);
            
            if (context.ClientsStatusesAssignments == null)
            {
                return Problem("Entity set 'ClientsStatusesAssignments' is null.");
            }

            try
            {
                await context.SaveChangesAsync();          
            }
            catch (DbUpdateException)
            {
                if (ClientExists(client.ClientId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetClient", new { id = client.ClientId }, client);
        }

        // DELETE: api/Clients/5
        [Authorize(Roles = Role.Admin + "," + Role.MainAdmin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            if (context.Clients == null)
            {
                return NotFound();
            }
            var client = await context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            context.Clients.Remove(client);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("statuses/{status}")]
        public async Task<ActionResult<IList<Client>>> GetClientsByStatus(string status)
        {
            if (context.Clients == null)
            {
                return NotFound();
            }

            IList<Client> statusAssignments = await context.Clients.Where(client => client.ClientsStatusesAssignments.Any(statusAssignment => statusAssignment.Status.Equals(status))).ToListAsync();
            
            return Ok(statusAssignments);
        }

        [HttpGet("passport/{passportNumber}")]
        public async Task<ActionResult<Client>> GetClientByPassport(string passportNumber)
        {
            if (context.Clients == null)
            {
                return NotFound();
            }

            Client client = await context.Clients.Where(client => client.PassportNumber.Equals(passportNumber)).FirstOrDefaultAsync();

            if (client == null)
            {
                return NotFound();
            }

            return client;
        }

        [HttpGet("{id}/objects")]
        public async Task<ActionResult<IList<EstateObject>>> GetRelatedObjects(int id)
        {
            if (context.Operations == null)
            {
                return NotFound();
            }

            IList<EstateObject> objects = await context.Operations.Where(operation => operation.CounteragentLead == id || id == operation.CounteragentSecondary.Value)
                .DistinctBy(operation => operation.EstateObjectId)
                .Select(operation => operation.EstateObjectNavigation).ToListAsync();

            return Ok(objects);
        }

        [HttpGet("{id}/objects/latestRelated")]
        public async Task<ActionResult<EstateObject>> GetLatestRelatedObject(int id)
        {
            if (context.Operations == null)
            {
                return NotFound();
            }

            EstateObject estateObject = await context.Operations.Where(operation => ((operation.CounteragentLead == id || id == operation.CounteragentSecondary)) 
                                                                           && operation.EstateObjectId != null && operation.EstateObjectId > 0)
                .OrderByDescending(operation => operation.Date)
                .Select(operation => operation.EstateObjectNavigation)
                .FirstOrDefaultAsync();

            if (estateObject == null)
            {
                return NotFound();
            }

            return Ok(estateObject);
        }

        [HttpGet("{id}/objects/latestRelatedAddress")]
        public async Task<ActionResult<string>> GetLatestRelatedObjectAddress(int id)
        {
            if (context.Operations == null)
            {
                return NotFound();
            }

            Operation operation = context.Operations.Where(operation => ((operation.CounteragentLead == id || id == operation.CounteragentSecondary))
                                                                           && operation.EstateObjectId != null && operation.EstateObjectId > 0)
                .OrderByDescending(operation => operation.Date)
                .Include(operation => operation.EstateObjectNavigation)
                .FirstOrDefault();

            if (operation == null)
            {
                return NotFound();
            }

            string address = operation.EstateObjectNavigation.Address;

            if (string.IsNullOrWhiteSpace(address))
            {
                return NotFound();
            }

            return Ok("\"" + address + "\"");
        }      

        [HttpGet("{id}/buyRequirements")]
        public async Task<ActionResult<IList<ClientsStatusesAssignment>>> GetBuyRequirements(int id)
        {
            if (context.ClientsStatusesAssignments == null)
            {
                return NotFound();
            }

            IList<ClientsStatusesAssignment> requirements = await context.ClientsStatusesAssignments.Where(status => status.ClientId == id
                                                                                                                  && status.Status.Equals(context.ClientsStatuses.Find("Покупець").Status)).ToListAsync();

            return Ok(requirements);
        }

        [HttpGet("{id}/sellRequirements")]
        public async Task<ActionResult<IList<ClientsStatusesAssignment>>> GetSellRequirements(int id)
        {
            if (context.ClientsStatusesAssignments == null)
            {
                return NotFound();
            }

            IList<ClientsStatusesAssignment> requirements = await context.ClientsStatusesAssignments.Where(status => status.ClientId == id
                                                                                                                  && status.Status.Equals(context.ClientsStatuses.Find("Продавець").Status)).ToListAsync();

            return Ok(requirements);
        }

        [HttpGet("{id}/rentRequirements")]
        public async Task<ActionResult<IList<ClientsStatusesAssignment>>> GetRentRequirements(int id)
        {
            if (context.ClientsStatusesAssignments == null)
            {
                return NotFound();
            }

            IList<ClientsStatusesAssignment> requirements = await context.ClientsStatusesAssignments.Where(status => status.ClientId == id
                                                                                                                  && status.Status.Equals(context.ClientsStatuses.Find("Орендар").Status)).ToListAsync();

            return Ok(requirements);
        }

        [HttpGet("{id}/forRentRequirements")]
        public async Task<ActionResult<IList<ClientsStatusesAssignment>>> GetForRentRequirements(int id)
        {
            if (context.ClientsStatusesAssignments == null)
            {
                return NotFound();
            }

            IList<ClientsStatusesAssignment> requirements = await context.ClientsStatusesAssignments.Where(status => status.ClientId == id
                                                                                                                  && status.Status.Equals(context.ClientsStatuses.Find("Орендодавець").Status)).ToListAsync();       

            return Ok(requirements);
        }

        [HttpGet("{id}/operations")]
        public async Task<ActionResult<IList<Operation>>> GetOperations(int id)
        {
            if (context.Operations == null)
            {
                return NotFound();
            }

            IList<Operation> operations = await context.Operations.Where(operation => operation.CounteragentLead == id || id == operation.CounteragentSecondary).ToListAsync();

            return Ok(operations);
        }

        private bool ClientExists(int id)
        {
            return (context.Clients?.Any(e => e.ClientId == id)).GetValueOrDefault();
        }
    }
}
