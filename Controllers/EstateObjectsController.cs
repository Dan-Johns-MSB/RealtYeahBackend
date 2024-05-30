using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealtYeahBackend.Entities;
using RealtYeahBackend.Models;
using RealtYeahBackend.Models.Constants;

namespace RealtYeahBackend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [RequireHttps]
    public class EstateObjectsController : ControllerBase
    {
        private readonly RealtyeahContext context;

        public EstateObjectsController(RealtyeahContext context)
        {
            this.context = context;
        }

        // GET: api/EstateObjects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EstateObject>>> GetEstateObjects()
        {
            if (context.EstateObjects == null)
            {
                return NotFound();
            }
            return await context.EstateObjects.ToListAsync();
        }

        // GET: api/EstateObjects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EstateObject>> GetEstateObject(int id)
        {
            if (context.EstateObjects == null)
            {
                return NotFound();
            }
            var estateObject = await context.EstateObjects.FindAsync(id);

            if (estateObject == null)
            {
                return NotFound();
            }

            return estateObject;
        }

        // PUT: api/EstateObjects/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEstateObject(int id, EstateObject estateObject)
        {
            if (id != estateObject.EstateObjectId)
            {
                return BadRequest();
            }

            context.Entry(estateObject).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstateObjectExists(id))
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

        // POST: api/EstateObjects
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<EstateObject>> PostEstateObject(EstateObject estateObject)
        {
            if (context.EstateObjects == null)
            {
                return Problem("Entity set 'RealtyeahContext.EstateObjects'  is null.");
            }
            estateObject.Status = "Внесений у базу";
            context.EstateObjects.Add(estateObject);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (EstateObjectExists(estateObject.EstateObjectId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetEstateObject", new { id = estateObject.EstateObjectId }, estateObject);
        }

        // DELETE: api/EstateObjects/5
        [Authorize(Roles = Role.Admin + "," + Role.MainAdmin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEstateObject(int id)
        {
            if (context.EstateObjects == null)
            {
                return NotFound();
            }
            var estateObject = await context.EstateObjects.FindAsync(id);
            if (estateObject == null)
            {
                return NotFound();
            }

            context.EstateObjects.Remove(estateObject);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("statuses/{status}")]
        public async Task<ActionResult<IList<EstateObject>>> GetObjectByStatus(string status)
        {
            if (context.EstateObjects == null)
            {
                return NotFound();
            }

            IList<EstateObject> estateObjects = await context.EstateObjects.Where(estateObject => estateObject.Status.Equals(status)).ToListAsync();

            return Ok(estateObjects);
        }

        [HttpGet("address/{address}")]
        public async Task<ActionResult<EstateObject>> GetEstateObjectByAddress(string address)
        {
            if (context.EstateObjects == null)
            {
                return NotFound();
            }

            EstateObject estateObject = await context.EstateObjects.Where(estateObject => estateObject.Address.Equals(address)).FirstOrDefaultAsync();

            if (estateObject == null)
            {
                return NotFound();
            }

            return estateObject;
        }

        [HttpGet("{id}/owner")]
        public async Task<ActionResult<Client>> GetOwner(int id)
        {
            if (context.Operations == null)
            {
                return NotFound();
            }

            Operation operation = await context.Operations.Include(operation => operation.CounteragentLeadNavigation)
                .Include(operation => operation.CounteragentSecondaryNavigation)
                .Where(operation => id == operation.EstateObjectId)
                .OrderByDescending(operation => operation.Date)
                .Where(operation => ActTypesConst.SellAgentDeal.Equals(operation.ActType)
                                 || ActTypesConst.ForRentAgentDeal.Equals(operation.ActType)
                                 || ActTypesConst.FinalDeal.Equals(operation.ActType)).FirstOrDefaultAsync();

            if (operation == null)
            {
                return NotFound("operation");
            }

            Client owner;
            if (ActTypesConst.SellAgentDeal.Equals(operation.ActType)
                || ActTypesConst.ForRentAgentDeal.Equals(operation.ActType))
            {
                owner = operation.CounteragentLeadNavigation;
            }
            else
            {
                owner = operation.CounteragentSecondaryNavigation;
            }

            return Ok(owner);
        }

        [HttpGet("{id}/ownerName")]
        public async Task<ActionResult<string>> GetOwnerName(int id)
        {
            if (context.Operations == null)
            {
                return NotFound();
            }

            Operation operation = context.Operations.Include(operation => operation.CounteragentLeadNavigation)
                .Include(operation => operation.CounteragentSecondaryNavigation)
                .Where(operation => id == operation.EstateObjectId)
                .OrderByDescending(operation => operation.Date)
                .Where(operation => ActTypesConst.SellAgentDeal.Equals(operation.ActType)
                                 || ActTypesConst.ForRentAgentDeal.Equals(operation.ActType)
                                 || ActTypesConst.FinalDeal.Equals(operation.ActType)).FirstOrDefault();

            if (operation == null)
            {
                return Ok("\"Unknown\"");
            }

            Client owner;
            if (ActTypesConst.SellAgentDeal.Equals(operation.ActType)
                || ActTypesConst.ForRentAgentDeal.Equals(operation.ActType))
            {
                owner = operation.CounteragentLeadNavigation;
            }
            else
            {
                owner = operation.CounteragentSecondaryNavigation;
            }

            return Ok("\"" + owner.Name + "\"");
        }

        [AllowAnonymous]
        [HttpGet("{id}/interestedClients")]
        public async Task<ActionResult<IList<Client>>> GetInterestedClients(int id)
        {
            if (context.Operations == null)
            {
                return NotFound();
            }

            List<Operation> operations = await context.Operations.Where(operation => id == operation.EstateObjectId)
                .OrderByDescending(operation => operation.Date)
                .Where(operation => (!ActTypesConst.BuyAgentDeal.Equals(operation.ActType)
                                    || !ActTypesConst.SellAgentDeal.Equals(operation.ActType)
                                    || !ActTypesConst.RentAgentDeal.Equals(operation.ActType)
                                    || !ActTypesConst.ForRentAgentDeal.Equals(operation.ActType))
                                 && !operation.Status.Equals("Неуспішно"))
                .Include(operation => operation.CounteragentSecondaryNavigation).ToListAsync();

            if (operations == null || ActTypesConst.FinalDeal.Equals(operations.FirstOrDefault().ActType))
            {
                return NotFound();
            }

            List<Client> interestedClients = new List<Client>();
            int ownerID = operations.FirstOrDefault().CounteragentLead;
            foreach (Operation operation in operations)
            {
                if (operation.CounteragentLead == ownerID)
                {
                    interestedClients.Add(operation.CounteragentSecondaryNavigation);
                }
            }

            if (interestedClients.Count == 0)
            {
                return NotFound();
            }

            return Ok(interestedClients);
        }

        [HttpGet("{id}/operations")]
        public async Task<ActionResult<IList<Operation>>> GetOperations(int id)
        {
            if (context.Operations == null)
            {
                return NotFound();
            }

            IList<Operation> operations = await context.Operations.Where(operation => id == operation.EstateObjectId).ToListAsync();

            return Ok(operations);
        }

        [HttpGet("{id}/lastOperation")]
        public async Task<ActionResult<Operation>> GetLastOperation(int id)
        {
            if (context.Operations == null)
            {
                return NotFound();
            }

            Operation operation = await context.Operations.Where(operation => id == operation.EstateObjectId).OrderByDescending(operation => operation.Date).FirstOrDefaultAsync();

            if (operation == null)
            {
                return NotFound();
            }

            return Ok(operation);
        }

        private bool EstateObjectExists(int id)
        {
            return (context.EstateObjects?.Any(e => e.EstateObjectId == id)).GetValueOrDefault();
        }
    }
}
